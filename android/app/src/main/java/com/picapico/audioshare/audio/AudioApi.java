package com.picapico.audioshare.audio;

import androidx.annotation.NonNull;

public enum AudioApi {
    AUDIO_TRACK("AudioTrack"),
    OPENSL("OpenSL ES"),
    AAUDIO("AAudio (Low Latency)");

    private final String displayName;

    AudioApi(String displayName) {
        this.displayName = displayName;
    }

    public String getDisplayName() {
        return displayName;
    }

    public static AudioApi fromPreference(@NonNull String value) {
        for (AudioApi api : values()) {
            if (api.name().equalsIgnoreCase(value)) {
                return api;
            }
        }
        return AUDIO_TRACK;
    }
}
