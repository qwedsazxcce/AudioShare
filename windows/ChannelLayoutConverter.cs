using System;
using System.Collections.Generic;

namespace AudioShare
{
    internal static class ChannelLayoutConverter
    {
        private struct MixContribution
        {
            public MixContribution(ChannelRole role, float weight)
            {
                Role = role;
                Weight = weight;
            }

            public ChannelRole Role { get; }
            public float Weight { get; }
        }

        private static readonly Dictionary<ChannelRole, MixContribution[]> MixMatrix =
            new Dictionary<ChannelRole, MixContribution[]>
            {
                {
                    ChannelRole.FrontLeft,
                    new[]
                    {
                        new MixContribution(ChannelRole.FrontLeft, 1f),
                        new MixContribution(ChannelRole.SideLeft, 0.8f),
                        new MixContribution(ChannelRole.BackLeft, 0.7f),
                        new MixContribution(ChannelRole.FrontCenter, 0.5f)
                    }
                },
                {
                    ChannelRole.FrontRight,
                    new[]
                    {
                        new MixContribution(ChannelRole.FrontRight, 1f),
                        new MixContribution(ChannelRole.SideRight, 0.8f),
                        new MixContribution(ChannelRole.BackRight, 0.7f),
                        new MixContribution(ChannelRole.FrontCenter, 0.5f)
                    }
                },
                {
                    ChannelRole.FrontCenter,
                    new[]
                    {
                        new MixContribution(ChannelRole.FrontCenter, 1f),
                        new MixContribution(ChannelRole.FrontLeft, 0.5f),
                        new MixContribution(ChannelRole.FrontRight, 0.5f)
                    }
                },
                {
                    ChannelRole.LowFrequency,
                    new[]
                    {
                        new MixContribution(ChannelRole.LowFrequency, 1f),
                        new MixContribution(ChannelRole.FrontLeft, 0.25f),
                        new MixContribution(ChannelRole.FrontRight, 0.25f),
                        new MixContribution(ChannelRole.BackLeft, 0.15f),
                        new MixContribution(ChannelRole.BackRight, 0.15f)
                    }
                },
                {
                    ChannelRole.BackLeft,
                    new[]
                    {
                        new MixContribution(ChannelRole.BackLeft, 1f),
                        new MixContribution(ChannelRole.SideLeft, 0.8f),
                        new MixContribution(ChannelRole.FrontLeft, 0.4f)
                    }
                },
                {
                    ChannelRole.BackRight,
                    new[]
                    {
                        new MixContribution(ChannelRole.BackRight, 1f),
                        new MixContribution(ChannelRole.SideRight, 0.8f),
                        new MixContribution(ChannelRole.FrontRight, 0.4f)
                    }
                },
                {
                    ChannelRole.SideLeft,
                    new[]
                    {
                        new MixContribution(ChannelRole.SideLeft, 1f),
                        new MixContribution(ChannelRole.BackLeft, 0.8f),
                        new MixContribution(ChannelRole.FrontLeft, 0.4f)
                    }
                },
                {
                    ChannelRole.SideRight,
                    new[]
                    {
                        new MixContribution(ChannelRole.SideRight, 1f),
                        new MixContribution(ChannelRole.BackRight, 0.8f),
                        new MixContribution(ChannelRole.FrontRight, 0.4f)
                    }
                }
            };

        public static byte[] Convert(AudioFrameEventArgs frame, ChannelPreset preset)
        {
            if (frame == null || frame.Buffer == null || frame.BytesRecorded <= 0 || preset == null)
            {
                return Array.Empty<byte>();
            }

            var format = frame.Format;
            if (format == null) return Array.Empty<byte>();
            int bytesPerSample = format.BitsPerSample / 8;
            if (bytesPerSample != 2)
            {
                return Array.Empty<byte>();
            }

            int sourceChannels = Math.Max(1, format.Channels);
            var sourceRoles = ChannelRoleHelper.EnsureLength(frame.ChannelRoles, sourceChannels);
            int targetChannels = preset.Roles.Length;
            if (targetChannels == 0) return Array.Empty<byte>();
            int frameStride = sourceChannels * bytesPerSample;
            int totalFrames = frame.BytesRecorded / frameStride;
            if (totalFrames <= 0) return Array.Empty<byte>();

            byte[] output = new byte[totalFrames * targetChannels * bytesPerSample];
            short[] sampleValues = new short[sourceChannels];

            for (int frameIndex = 0; frameIndex < totalFrames; frameIndex++)
            {
                int readOffset = frameIndex * frameStride;
                for (int ch = 0; ch < sourceChannels; ch++)
                {
                    int sampleOffset = readOffset + ch * bytesPerSample;
                    short value = (short)(frame.Buffer[sampleOffset] | (frame.Buffer[sampleOffset + 1] << 8));
                    sampleValues[ch] = value;
                }

                for (int targetIndex = 0; targetIndex < targetChannels; targetIndex++)
                {
                    short mixed = MixSample(preset.Roles[targetIndex], sampleValues, sourceRoles);
                    int writeOffset = frameIndex * targetChannels * bytesPerSample + targetIndex * bytesPerSample;
                    output[writeOffset] = (byte)(mixed & 0xFF);
                    output[writeOffset + 1] = (byte)((mixed >> 8) & 0xFF);
                }
            }
            return output;
        }

        private static short MixSample(ChannelRole target, short[] sourceSamples, ChannelRole[] sourceRoles)
        {
            MixContribution[] contributions;
            double weightedSum = 0;
            double totalWeight = 0;
            if (MixMatrix.TryGetValue(target, out contributions))
            {
                foreach (var contribution in contributions)
                {
                    for (int channelIndex = 0; channelIndex < sourceRoles.Length; channelIndex++)
                    {
                        if (sourceRoles[channelIndex] == contribution.Role)
                        {
                            weightedSum += sourceSamples[channelIndex] * contribution.Weight;
                            totalWeight += contribution.Weight;
                        }
                    }
                }
            }
            if (totalWeight <= 0)
            {
                for (int channelIndex = 0; channelIndex < sourceSamples.Length; channelIndex++)
                {
                    weightedSum += sourceSamples[channelIndex];
                }
                totalWeight = sourceSamples.Length;
            }

            if (totalWeight <= 0) return 0;
            int sample = (int)(weightedSum / totalWeight);
            if (sample > short.MaxValue) sample = short.MaxValue;
            if (sample < short.MinValue) sample = short.MinValue;
            return (short)sample;
        }
    }
}
