using System;
using NAudio.Wave;

namespace AudioShare
{
    public class AudioFrameEventArgs : EventArgs
    {
        public AudioFrameEventArgs(byte[] buffer, int bytesRecorded, WaveFormat format, ChannelRole[] channelRoles)
        {
            Buffer = buffer;
            BytesRecorded = bytesRecorded;
            Format = format;
            ChannelRoles = channelRoles;
        }

        public byte[] Buffer { get; }

        public int BytesRecorded { get; }

        public WaveFormat Format { get; }

        public ChannelRole[] ChannelRoles { get; }
    }
}
