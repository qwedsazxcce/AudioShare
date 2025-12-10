using System;
using System.Collections.Generic;
using System.Linq;

namespace AudioShare
{
    public class ChannelPreset
    {
        public ChannelPreset(AudioChannel channel, string resourceKey, string fallbackDisplay, int androidChannelMask, params ChannelRole[] roles)
        {
            Channel = channel;
            ResourceKey = resourceKey;
            FallbackDisplay = fallbackDisplay;
            AndroidChannelMask = androidChannelMask;
            Roles = roles?.ToArray() ?? Array.Empty<ChannelRole>();
        }

        public AudioChannel Channel { get; }

        public string ResourceKey { get; }

        public string FallbackDisplay { get; }

        public int AndroidChannelMask { get; }

        public ChannelRole[] Roles { get; }

        public string DisplayName
        {
            get
            {
                var text = Languages.Language.GetLanguageText(ResourceKey);
                if (string.IsNullOrWhiteSpace(text)) return FallbackDisplay;
                return text;
            }
        }
    }

    public static class ChannelPresetCatalog
    {
        private static readonly ChannelPreset[] OrderedPresets = new[]
        {
            new ChannelPreset(AudioChannel.Stereo, "channelStereo", "Stereo",
                AndroidChannelMask.FrontLeft | AndroidChannelMask.FrontRight,
                ChannelRole.FrontLeft, ChannelRole.FrontRight),
            new ChannelPreset(AudioChannel.Mono, "channelMono", "Mono",
                AndroidChannelMask.FrontCenter,
                ChannelRole.FrontCenter),
            new ChannelPreset(AudioChannel.Left, "channelLeft", "Left",
                AndroidChannelMask.FrontLeft,
                ChannelRole.FrontLeft),
            new ChannelPreset(AudioChannel.Right, "channelRight", "Right",
                AndroidChannelMask.FrontRight,
                ChannelRole.FrontRight),
            new ChannelPreset(AudioChannel.Surround21, "channel21", "2.1 Surround",
                AndroidChannelMask.FrontLeft | AndroidChannelMask.FrontRight | AndroidChannelMask.LowFrequency,
                ChannelRole.FrontLeft, ChannelRole.FrontRight, ChannelRole.LowFrequency),
            new ChannelPreset(AudioChannel.Surround40, "channel40", "4.0 Surround",
                AndroidChannelMask.FrontLeft | AndroidChannelMask.FrontRight | AndroidChannelMask.BackLeft | AndroidChannelMask.BackRight,
                ChannelRole.FrontLeft, ChannelRole.FrontRight, ChannelRole.BackLeft, ChannelRole.BackRight),
            new ChannelPreset(AudioChannel.Surround51, "channel51", "5.1 Surround",
                AndroidChannelMask.FrontLeft | AndroidChannelMask.FrontRight | AndroidChannelMask.FrontCenter |
                AndroidChannelMask.LowFrequency | AndroidChannelMask.BackLeft | AndroidChannelMask.BackRight,
                ChannelRole.FrontLeft, ChannelRole.FrontRight, ChannelRole.FrontCenter, ChannelRole.LowFrequency,
                ChannelRole.BackLeft, ChannelRole.BackRight),
            new ChannelPreset(AudioChannel.Surround71, "channel71", "7.1 Surround",
                AndroidChannelMask.FrontLeft | AndroidChannelMask.FrontRight | AndroidChannelMask.FrontCenter |
                AndroidChannelMask.LowFrequency | AndroidChannelMask.BackLeft | AndroidChannelMask.BackRight |
                AndroidChannelMask.SideLeft | AndroidChannelMask.SideRight,
                ChannelRole.FrontLeft, ChannelRole.FrontRight, ChannelRole.FrontCenter, ChannelRole.LowFrequency,
                ChannelRole.BackLeft, ChannelRole.BackRight, ChannelRole.SideLeft, ChannelRole.SideRight),
        };

        private static readonly Dictionary<AudioChannel, ChannelPreset> Presets =
            OrderedPresets.ToDictionary(preset => preset.Channel, preset => preset);

        public static ChannelPreset Get(AudioChannel channel)
        {
            ChannelPreset preset;
            return Presets.TryGetValue(channel, out preset) ? preset : null;
        }

        public static IEnumerable<KeyValuePair<AudioChannel, string>> GetDisplayPairs()
        {
            foreach (var preset in OrderedPresets)
            {
                yield return new KeyValuePair<AudioChannel, string>(preset.Channel, preset.DisplayName);
            }
        }
    }

    internal static class AndroidChannelMask
    {
        public const int FrontLeft = 0x4;
        public const int FrontRight = 0x8;
        public const int FrontCenter = 0x10;
        public const int LowFrequency = 0x20;
        public const int BackLeft = 0x40;
        public const int BackRight = 0x80;
        public const int SideLeft = 0x800;
        public const int SideRight = 0x1000;
    }
}
