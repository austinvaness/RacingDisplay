using Sandbox.ModAPI;
using System;
using VRage.Input;

namespace avaness.RacingMod
{
    public class RacingPreferences
    {

        public RacingPreferences()
        {

        }

        private Keybind nextPlayer = MyKeys.OemCloseBrackets;
        public Keybind NextPlayer
        {
            get
            {
                return nextPlayer;
            }
            set
            {
                if(value != nextPlayer)
                {
                    nextPlayer = value;
                    SaveFile();
                }
            }
        }

        private Keybind prevPlayer = MyKeys.OemOpenBrackets;
        public Keybind PrevPlayer
        {
            get
            {
                return prevPlayer;
            }
            set
            {
                if (value != prevPlayer)
                {
                    prevPlayer = value;
                    SaveFile();
                }
            }
        }

        private Keybind hideHud = MyKeys.OemPlus;
        public Keybind HideHud
        {
            get
            {
                return hideHud;
            }
            set
            {
                if (value != hideHud)
                {
                    hideHud = value;
                    SaveFile();
                }
            }
        }

        private Keybind stopSpec = MyKeys.None;
        public Keybind StopSpec
        {
            get
            {
                return stopSpec;
            }
            set
            {
                if (value != stopSpec)
                {
                    stopSpec = value;
                    SaveFile();
                }
            }
        }

        public void SaveFile ()
        {
            var writer = MyAPIGateway.Utilities.WriteFileInLocalStorage(RacingConstants.playerFile, typeof(RacingPreferences));
            writer.Write(MyAPIGateway.Utilities.SerializeToXML(this));
            writer.Flush();
            writer.Close();
        }

        public static RacingPreferences LoadFile ()
        {
            try
            {
                if (MyAPIGateway.Utilities.FileExistsInLocalStorage(RacingConstants.playerFile, typeof(RacingPreferences)))
                {
                    var reader = MyAPIGateway.Utilities.ReadFileInLocalStorage(RacingConstants.playerFile, typeof(RacingPreferences));
                    string xmlText = reader.ReadToEnd();
                    reader.Close();
                    RacingPreferences config = MyAPIGateway.Utilities.SerializeFromXML<RacingPreferences>(xmlText);
                    if (config == null)
                        throw new NullReferenceException("Failed to serialize from xml.");
                    else
                        return config;
                }
            }
            catch { }

            RacingPreferences result = new RacingPreferences();
            result.SaveFile();
            return result;
        }


    }
}
