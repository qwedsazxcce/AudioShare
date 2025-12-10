package com.picapico.audioshare.audio;

import android.media.AudioAttributes;
import android.media.AudioFormat;
import android.media.AudioManager;
import android.media.AudioTrack;
import android.os.Build;
import android.util.Log;

import androidx.annotation.NonNull;

public class AudioTrackEngine implements AudioOutputEngine {
    private static final String TAG = "AudioTrackEngine";
    private final boolean lowLatencyMode;
    private AudioTrack audioTrack;

    public AudioTrackEngine(boolean lowLatencyMode) {
        this.lowLatencyMode = lowLatencyMode;
    }

    @Override
    public boolean prepare(int sampleRate, int channelCount, int channelMask, int audioFormat, int bufferSize) {
        release();
        try {
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {
                AudioAttributes attributes = new AudioAttributes.Builder()
                        .setUsage(AudioAttributes.USAGE_MEDIA)
                        .setContentType(AudioAttributes.CONTENT_TYPE_MUSIC)
                        .build();
                AudioFormat format = new AudioFormat.Builder()
                        .setEncoding(audioFormat)
                        .setSampleRate(sampleRate)
                        .setChannelMask(channelMask)
                        .build();
                AudioTrack.Builder builder = new AudioTrack.Builder()
                        .setTransferMode(AudioTrack.MODE_STREAM)
                        .setBufferSizeInBytes(bufferSize)
                        .setAudioAttributes(attributes)
                        .setAudioFormat(format);
                if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O && lowLatencyMode) {
                    builder.setPerformanceMode(AudioTrack.PERFORMANCE_MODE_LOW_LATENCY);
                }
                audioTrack = builder.build();
            } else {
                audioTrack = new AudioTrack(
                        AudioManager.STREAM_MUSIC,
                        sampleRate,
                        channelMask,
                        audioFormat,
                        bufferSize,
                        AudioTrack.MODE_STREAM);
            }
        } catch (IllegalArgumentException exception) {
            Log.e(TAG, "prepare audio track error", exception);
            audioTrack = null;
        }
        return audioTrack != null;
    }

    @Override
    public void start() {
        if (audioTrack != null) {
            audioTrack.play();
        }
    }

    @Override
    public int write(@NonNull byte[] buffer, int offset, int size) {
        if (audioTrack == null) return AudioTrack.ERROR_INVALID_OPERATION;
        return audioTrack.write(buffer, offset, size);
    }

    @Override
    public void stop() {
        if (audioTrack != null) {
            try {
                audioTrack.pause();
                audioTrack.flush();
                audioTrack.stop();
            } catch (IllegalStateException ignore) {
            }
        }
    }

    @Override
    public void release() {
        if (audioTrack != null) {
            try {
                audioTrack.release();
            } catch (Exception ignore) {
            }
            audioTrack = null;
        }
    }

    @Override
    public int getAudioSessionId() {
        if (audioTrack == null) return AudioTrack.ERROR;
        return audioTrack.getAudioSessionId();
    }
}
