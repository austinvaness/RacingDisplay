using Sandbox.ModAPI;
using System;
using VRage.Input;

namespace RacingMod
{
    public class RacingPreferences
    {
        private const string FileLocation = "RacingDisplayPreferences.xml";

        public RacingPreferences()
        {

        }

        Keybind nextPlayer = MyKeys.OemCloseBrackets;
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
                    SaveXML();
                }
            }
        }

        Keybind prevPlayer = MyKeys.OemOpenBrackets;
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
                    SaveXML();
                }
            }
        }

        Keybind hideHud = MyKeys.OemPlus;
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
                    SaveXML();
                }
            }
        }

        Keybind lookAt = MyKeys.OemQuotes;
        public Keybind LookAt
        {
            get
            {
                return lookAt;
            }
            set
            {
                if (value != lookAt)
                {
                    lookAt = value;
                    SaveXML();
                }
            }
        }

        public void SaveXML ()
        {
            var writer = MyAPIGateway.Utilities.WriteFileInLocalStorage(FileLocation, typeof(RacingPreferences));
            writer.Write(MyAPIGateway.Utilities.SerializeToXML(this));
            writer.Flush();
            writer.Close();
        }

        public static RacingPreferences LoadXML (bool loadDefault = false)
        {
            if (!loadDefault)
            {
                try
                {
                    if (MyAPIGateway.Utilities.FileExistsInLocalStorage(FileLocation, typeof(RacingPreferences)))
                    {
                        var reader = MyAPIGateway.Utilities.ReadFileInLocalStorage(FileLocation, typeof(RacingPreferences));
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
            }

            RacingPreferences result = new RacingPreferences();
            result.SaveXML();
            return result;
        }


    }
}
