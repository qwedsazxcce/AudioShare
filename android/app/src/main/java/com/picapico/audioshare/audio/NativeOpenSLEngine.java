package com.picapico.audioshare.audio;

public class NativeOpenSLEngine implements AudioOutputEngine {
    static {
        System.loadLibrary("audioshare-opensl");
    }

    private long nativeHandle = 0L;

    @Override
    public boolean prepare(int sampleRate, int channelCount, int channelMask, int audioFormat, int bufferSize) {
        if (nativeHandle == 0L) {
            nativeHandle = nativeCreate();
        }
        if (nativeHandle == 0L) return false;
        return nativeInit(nativeHandle, sampleRate, channelCount, channelMask);
    }

    @Override
    public void start() {
        if (nativeHandle != 0L) {
            nativeStart(nativeHandle);
        }
    }

    @Override
    public int write(byte[] buffer, int offset, int size) {
        if (nativeHandle == 0L) return -1;
        return nativeWrite(nativeHandle, buffer, offset, size);
    }

    @Override
    public void stop() {
        if (nativeHandle != 0L) {
            nativeStop(nativeHandle);
        }
    }

    @Override
    public void release() {
        if (nativeHandle != 0L) {
            nativeRelease(nativeHandle);
            nativeHandle = 0L;
        }
    }

    @Override
    public int getAudioSessionId() {
        return -1;
    }

    private static native long nativeCreate();

    private static native boolean nativeInit(long handle, int sampleRate, int channelCount, int channelMask);

    private static native void nativeStart(long handle);

    private static native int nativeWrite(long handle, byte[] buffer, int offset, int size);

    private static native void nativeStop(long handle);

    private static native void nativeRelease(long handle);
}
