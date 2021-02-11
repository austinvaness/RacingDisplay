using System;

namespace avaness.RacingPaths.Data
{
    public interface IPath
    {
        string PlayerName { get; }
        ulong PlayerId { get; }
        TimeSpan Length { get; }
    }
}
