#include <jni.h>
#include <SLES/OpenSLES.h>
#include <SLES/OpenSLES_Android.h>
#include <android/log.h>
#include <cstdint>
#include <vector>
#include <deque>
#include <mutex>
#include <condition_variable>

#define LOG_TAG "AudioShareOpenSL"
#define LOGE(...) __android_log_print(ANDROID_LOG_ERROR, LOG_TAG, __VA_ARGS__)
#define LOGI(...) __android_log_print(ANDROID_LOG_INFO, LOG_TAG, __VA_ARGS__)

namespace {
SLuint32 ConvertChannelMask(int androidMask, int channelCount) {
    SLuint32 mask = 0;
    if (androidMask & 0x4) mask |= SL_SPEAKER_FRONT_LEFT;
    if (androidMask & 0x8) mask |= SL_SPEAKER_FRONT_RIGHT;
    if (androidMask & 0x10) mask |= SL_SPEAKER_FRONT_CENTER;
    if (androidMask & 0x20) mask |= SL_SPEAKER_LOW_FREQUENCY;
    if (androidMask & 0x40) mask |= SL_SPEAKER_BACK_LEFT;
    if (androidMask & 0x80) mask |= SL_SPEAKER_BACK_RIGHT;
    if (androidMask & 0x800) mask |= SL_SPEAKER_SIDE_LEFT;
    if (androidMask & 0x1000) mask |= SL_SPEAKER_SIDE_RIGHT;
    if (mask == 0) {
        if (channelCount == 1) {
            mask = SL_SPEAKER_FRONT_CENTER;
        } else {
            mask = SL_SPEAKER_FRONT_LEFT | SL_SPEAKER_FRONT_RIGHT;
        }
    }
    return mask;
}
}

class OpenSLPlayer {
public:
    bool init(int sampleRate, int channelCount, int channelMask) {
        release();
        SLresult result = slCreateEngine(&engineObject, 0, nullptr, 0, nullptr, nullptr);
        if (result != SL_RESULT_SUCCESS) {
            LOGE("Create engine failed: %d", result);
            return false;
        }
        result = (*engineObject)->Realize(engineObject, SL_BOOLEAN_FALSE);
        if (result != SL_RESULT_SUCCESS) {
            LOGE("Realize engine failed: %d", result);
            release();
            return false;
        }
        result = (*engineObject)->GetInterface(engineObject, SL_IID_ENGINE, &engineInterface);
        if (result != SL_RESULT_SUCCESS) {
            LOGE("Get engine interface failed: %d", result);
            release();
            return false;
        }
        result = (*engineInterface)->CreateOutputMix(engineInterface, &outputMixObject, 0, nullptr, nullptr);
        if (result != SL_RESULT_SUCCESS) {
            LOGE("Create output mix failed: %d", result);
            release();
            return false;
        }
        result = (*outputMixObject)->Realize(outputMixObject, SL_BOOLEAN_FALSE);
        if (result != SL_RESULT_SUCCESS) {
            LOGE("Realize output mix failed: %d", result);
            release();
            return false;
        }

        SLDataLocator_AndroidSimpleBufferQueue locatorBufferQueue = {
                SL_DATALOCATOR_ANDROIDSIMPLEBUFFERQUEUE, 4
        };
        SLuint32 slChannelMask = ConvertChannelMask(channelMask, channelCount);
        SLDataFormat_PCM formatPcm = {
                SL_DATAFORMAT_PCM,
                static_cast<SLuint32>(channelCount),
                static_cast<SLuint32>(sampleRate * 1000),
                SL_PCMSAMPLEFORMAT_FIXED_16,
                SL_PCMSAMPLEFORMAT_FIXED_16,
                slChannelMask,
                SL_BYTEORDER_LITTLEENDIAN
        };
        SLDataSource audioSource = { &locatorBufferQueue, &formatPcm };
        SLDataLocator_OutputMix locatorOutputMix = {SL_DATALOCATOR_OUTPUTMIX, outputMixObject};
        SLDataSink audioSink = { &locatorOutputMix, nullptr };

        const SLInterfaceID interfaceIds[2] = {SL_IID_BUFFERQUEUE, SL_IID_ANDROIDCONFIGURATION};
        const SLboolean interfaceRequired[2] = {SL_BOOLEAN_TRUE, SL_BOOLEAN_TRUE};
        result = (*engineInterface)->CreateAudioPlayer(engineInterface,
                                                       &playerObject,
                                                       &audioSource,
                                                       &audioSink,
                                                       2,
                                                       interfaceIds,
                                                       interfaceRequired);
        if (result != SL_RESULT_SUCCESS) {
            LOGE("Create audio player failed: %d", result);
            release();
            return false;
        }
        SLAndroidConfigurationItf configItf = nullptr;
        if ((*playerObject)->GetInterface(playerObject, SL_IID_ANDROIDCONFIGURATION, &configItf) == SL_RESULT_SUCCESS) {
            const SLint32 streamType = SL_ANDROID_STREAM_MEDIA;
            (*configItf)->SetConfiguration(configItf, SL_ANDROID_KEY_STREAM_TYPE, &streamType, sizeof(streamType));
        }
        result = (*playerObject)->Realize(playerObject, SL_BOOLEAN_FALSE);
        if (result != SL_RESULT_SUCCESS) {
            LOGE("Realize player failed: %d", result);
            release();
            return false;
        }
        result = (*playerObject)->GetInterface(playerObject, SL_IID_PLAY, &playInterface);
        if (result != SL_RESULT_SUCCESS) {
            LOGE("Get play interface failed: %d", result);
            release();
            return false;
        }
        result = (*playerObject)->GetInterface(playerObject, SL_IID_BUFFERQUEUE, &bufferQueueInterface);
        if (result != SL_RESULT_SUCCESS) {
            LOGE("Get buffer queue interface failed: %d", result);
            release();
            return false;
        }
        (*bufferQueueInterface)->RegisterCallback(bufferQueueInterface, BufferCallback, this);
        initialized = true;
        return true;
    }

    void start() {
        if (!initialized) return;
        (*playInterface)->SetPlayState(playInterface, SL_PLAYSTATE_PLAYING);
    }

    void stop() {
        if (!initialized) return;
        (*playInterface)->SetPlayState(playInterface, SL_PLAYSTATE_STOPPED);
        if (bufferQueueInterface != nullptr) {
            (*bufferQueueInterface)->Clear(bufferQueueInterface);
        }
        std::lock_guard<std::mutex> lock(mutex);
        pendingBuffers.clear();
        condition.notify_all();
    }

    int write(const uint8_t *data, size_t length) {
        if (!initialized || bufferQueueInterface == nullptr || length == 0) return -1;
        std::vector<uint8_t> buffer(data, data + length);
        std::unique_lock<std::mutex> lock(mutex);
        while (pendingBuffers.size() >= 8) {
            condition.wait(lock);
        }
        pendingBuffers.push_back(std::move(buffer));
        std::vector<uint8_t> &current = pendingBuffers.back();
        SLresult result = (*bufferQueueInterface)->Enqueue(bufferQueueInterface, current.data(), static_cast<SLuint32>(current.size()));
        if (result != SL_RESULT_SUCCESS) {
            LOGE("Enqueue buffer failed: %d", result);
            pendingBuffers.pop_back();
            return -1;
        }
        return static_cast<int>(length);
    }

    void release() {
        stop();
        if (playerObject != nullptr) {
            (*playerObject)->Destroy(playerObject);
            playerObject = nullptr;
            playInterface = nullptr;
            bufferQueueInterface = nullptr;
        }
        if (outputMixObject != nullptr) {
            (*outputMixObject)->Destroy(outputMixObject);
            outputMixObject = nullptr;
        }
        if (engineObject != nullptr) {
            (*engineObject)->Destroy(engineObject);
            engineObject = nullptr;
            engineInterface = nullptr;
        }
        initialized = false;
    }

    ~OpenSLPlayer() {
        release();
    }

private:
    static void BufferCallback(SLAndroidSimpleBufferQueueItf queue, void *context) {
        auto *player = static_cast<OpenSLPlayer *>(context);
        if (player == nullptr) return;
        std::lock_guard<std::mutex> lock(player->mutex);
        if (!player->pendingBuffers.empty()) {
            player->pendingBuffers.pop_front();
        }
        player->condition.notify_one();
    }

    SLObjectItf engineObject = nullptr;
    SLEngineItf engineInterface = nullptr;
    SLObjectItf outputMixObject = nullptr;
    SLObjectItf playerObject = nullptr;
    SLPlayItf playInterface = nullptr;
    SLAndroidSimpleBufferQueueItf bufferQueueInterface = nullptr;
    bool initialized = false;
    std::mutex mutex;
    std::condition_variable condition;
    std::deque<std::vector<uint8_t>> pendingBuffers;
};

extern "C"
JNIEXPORT jlong JNICALL
Java_com_picapico_audioshare_audio_NativeOpenSLEngine_nativeCreate(JNIEnv *env, jclass clazz) {
    auto *player = new OpenSLPlayer();
    return reinterpret_cast<jlong>(player);
}

extern "C"
JNIEXPORT jboolean JNICALL
Java_com_picapico_audioshare_audio_NativeOpenSLEngine_nativeInit(JNIEnv *env, jclass clazz,
                                                                 jlong handle, jint sampleRate,
                                                                 jint channelCount,
                                                                 jint channelMask) {
    auto *player = reinterpret_cast<OpenSLPlayer *>(handle);
    if (player == nullptr) return JNI_FALSE;
    bool result = player->init(sampleRate, channelCount, channelMask);
    return result ? JNI_TRUE : JNI_FALSE;
}

extern "C"
JNIEXPORT void JNICALL
Java_com_picapico_audioshare_audio_NativeOpenSLEngine_nativeStart(JNIEnv *env, jclass clazz,
                                                                  jlong handle) {
    auto *player = reinterpret_cast<OpenSLPlayer *>(handle);
    if (player == nullptr) return;
    player->start();
}

extern "C"
JNIEXPORT jint JNICALL
Java_com_picapico_audioshare_audio_NativeOpenSLEngine_nativeWrite(JNIEnv *env, jclass clazz,
                                                                  jlong handle, jbyteArray array,
                                                                  jint offset, jint length) {
    auto *player = reinterpret_cast<OpenSLPlayer *>(handle);
    if (player == nullptr || array == nullptr) return -1;
    jbyte *data = env->GetByteArrayElements(array, nullptr);
    if (data == nullptr) return -1;
    int written = player->write(reinterpret_cast<uint8_t *>(data + offset), static_cast<size_t>(length));
    env->ReleaseByteArrayElements(array, data, JNI_ABORT);
    return written;
}

extern "C"
JNIEXPORT void JNICALL
Java_com_picapico_audioshare_audio_NativeOpenSLEngine_nativeStop(JNIEnv *env, jclass clazz,
                                                                 jlong handle) {
    auto *player = reinterpret_cast<OpenSLPlayer *>(handle);
    if (player == nullptr) return;
    player->stop();
}

extern "C"
JNIEXPORT void JNICALL
Java_com_picapico_audioshare_audio_NativeOpenSLEngine_nativeRelease(JNIEnv *env, jclass clazz,
                                                                    jlong handle) {
    auto *player = reinterpret_cast<OpenSLPlayer *>(handle);
    if (player == nullptr) return;
    delete player;
}
