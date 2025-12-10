namespace AudioShare
{
    public enum ChannelRole
    {
        FrontLeft,
        FrontRight,
        FrontCenter,
        LowFrequency,
        BackLeft,
        BackRight,
        SideLeft,
        SideRight,
        Unknown
    }

    public static class ChannelRoleHelper
    {
        private static readonly ChannelRole[] SingleChannel = new[] { ChannelRole.FrontCenter };
        private static readonly ChannelRole[] TwoChannels = new[] { ChannelRole.FrontLeft, ChannelRole.FrontRight };
        private static readonly ChannelRole[] ThreeChannels = new[] { ChannelRole.FrontLeft, ChannelRole.FrontRight, ChannelRole.LowFrequency };
        private static readonly ChannelRole[] FourChannels = new[] { ChannelRole.FrontLeft, ChannelRole.FrontRight, ChannelRole.BackLeft, ChannelRole.BackRight };
        private static readonly ChannelRole[] FiveChannels = new[] { ChannelRole.FrontLeft, ChannelRole.FrontRight, ChannelRole.FrontCenter, ChannelRole.BackLeft, ChannelRole.BackRight };
        private static readonly ChannelRole[] SixChannels = new[] { ChannelRole.FrontLeft, ChannelRole.FrontRight, ChannelRole.FrontCenter, ChannelRole.LowFrequency, ChannelRole.BackLeft, ChannelRole.BackRight };
        private static readonly ChannelRole[] SevenChannels = new[] { ChannelRole.FrontLeft, ChannelRole.FrontRight, ChannelRole.FrontCenter, ChannelRole.LowFrequency, ChannelRole.BackLeft, ChannelRole.BackRight, ChannelRole.SideLeft };
        private static readonly ChannelRole[] EightChannels = new[] { ChannelRole.FrontLeft, ChannelRole.FrontRight, ChannelRole.FrontCenter, ChannelRole.LowFrequency, ChannelRole.BackLeft, ChannelRole.BackRight, ChannelRole.SideLeft, ChannelRole.SideRight };

        public static ChannelRole[] GetDefaultRoles(int channelCount)
        {
            if (channelCount <= 1) return SingleChannel;
            if (channelCount == 2) return TwoChannels;
            if (channelCount == 3) return ThreeChannels;
            if (channelCount == 4) return FourChannels;
            if (channelCount == 5) return FiveChannels;
            if (channelCount == 6) return SixChannels;
            if (channelCount == 7) return SevenChannels;
            return EightChannels;
        }

        public static ChannelRole[] EnsureLength(ChannelRole[] roles, int channelCount)
        {
            if (roles == null || roles.Length != channelCount)
            {
                return GetDefaultRoles(channelCount);
            }
            return roles;
        }
    }
}
