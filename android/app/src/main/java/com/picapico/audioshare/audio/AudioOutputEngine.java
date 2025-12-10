package com.picapico.audioshare.audio;

public interface AudioOutputEngine {
    boolean prepare(int sampleRate, int channelCount, int channelMask, int audioFormat, int bufferSize);

    void start();

    int write(byte[] buffer, int offset, int size);

    void stop();

    void release();

    int getAudioSessionId();
}
