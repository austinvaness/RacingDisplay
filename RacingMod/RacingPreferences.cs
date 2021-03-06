﻿using Sandbox.ModAPI;
using System;
using VRage.Input;

namespace avaness.RacingMod
{
    public class RacingPreferences
    {

        public RacingPreferences()
        {

        }

        public void Unload()
        {
            OnHideHudChanged = null;
            OnAutoRecordChanged = null;
        }

        private Keybind hideHud = MyKeys.None;
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
                    if (OnHideHudChanged != null)
                        OnHideHudChanged.Invoke(value);
                }
            }
        }
        public event Action<Keybind> OnHideHudChanged;

        private bool autoRecord = false;
        public bool AutoRecord
        {
            get
            {
                return autoRecord;
            }
            set
            {
                if(value != autoRecord)
                {
                    autoRecord = value;
                    SaveFile();
                    if (OnAutoRecordChanged != null)
                        OnAutoRecordChanged.Invoke(value);
                }
            }
        }
        public event Action<bool> OnAutoRecordChanged;

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

        public void Copy(RacingPreferences racingPreferences)
        {
            hideHud = racingPreferences.hideHud;
            if (OnHideHudChanged != null)
                OnHideHudChanged.Invoke(hideHud);
            autoRecord = racingPreferences.autoRecord;
            if (OnAutoRecordChanged != null)
                OnAutoRecordChanged.Invoke(autoRecord);
        }
    }
}
