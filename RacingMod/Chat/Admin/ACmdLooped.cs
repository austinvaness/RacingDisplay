using avaness.RacingMod.Race;
using VRage.Game.ModAPI;

namespace avaness.RacingMod.Chat.Admin
{
    public class ACmdLooped : AdminCommand
    {
        public override string Id => "looped";

        public override string Usage => ": Toggles if the track is looped.";

        protected override void ExecuteAdmin(IMyPlayer p, string[] cmd, Track race)
        {
            RacingMapSettings mapSettings = race.MapSettings;
            if (mapSettings.NumLaps > 1)
            {
                ShowChatMsg(p, "Race tracks with more than one lap are always looped.");
            }
            else
            {
                mapSettings.Looped = !mapSettings.Looped;
                if (mapSettings.Looped)
                    ShowChatMsg(p, "The race track is now looped.");
                else
                    ShowChatMsg(p, "The race track is no longer looped.");
            }
        }
    }
}
