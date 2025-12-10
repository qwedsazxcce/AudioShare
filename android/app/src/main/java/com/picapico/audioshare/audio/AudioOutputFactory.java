package com.picapico.audioshare.audio;

import android.os.Build;

public final class AudioOutputFactory {
    private AudioOutputFactory() {
    }

    public static AudioOutputEngine create(AudioApi api) {
        switch (api) {
            case OPENSL:
                return new NativeOpenSLEngine();
            case AAUDIO:
                // Placeholder low-latency pipeline while native AAudio integration is staged.
                return new AudioTrackEngine(true);
            case AUDIO_TRACK:
            default:
                return new AudioTrackEngine(false);
        }
    }
}
