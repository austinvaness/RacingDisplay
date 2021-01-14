using System;

namespace avaness.RacingMod.Race.Finish
{
    public interface IFinisher
    {
        ulong Id { get; }
        string Name { get; }
        TimeSpan BestTime { get; }
        void RemoveFinish();
    }
}