using System;

namespace avaness.RacingLeaderboard.Data
{
    public interface IPath
    {
        string PlayerName { get; }
        ulong PlayerId { get; }
        TimeSpan Length { get; }
    }
}
