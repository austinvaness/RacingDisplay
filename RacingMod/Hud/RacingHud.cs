using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace avaness.RacingMod.Hud
{
    public abstract class RacingHud
    {
        protected StringBuilder text = new StringBuilder();

        public virtual int Length
        {
            get
            {
                return text.Length;
            }
            set
            {
                text.Length = value;
            }
        }

        public virtual void Broadcast() { }
        public virtual void Unload() { }
        public virtual void ToggleUI() { }

        public virtual RacingHud Clear()
        {
            text.Clear();
            return this;
        }

        public abstract RacingHud CreateTemporary();

        public virtual RacingHud Append(string value)
        {
            text.Append(value);
            return this;
        }
        public virtual RacingHud Append(char value)
        {
            text.Append(value);
            return this;
        }
        public virtual RacingHud Append(int value)
        {
            text.Append(value);
            return this;
        }
        public virtual RacingHud AppendLine()
        {
            text.AppendLine();
            return this;
        }

        public virtual RacingHud Append(VRageMath.Color color)
        {
            return this;
        }

        public static RacingHud Create(RacingPreferences config, RacingMapSettings mapSettings)
        {
            bool useTextHudApi = MyAPIGateway.Session.Mods?.Any(x => x.PublishedFileId == 758597413 && x.PublishedServiceName == "Steam") == true;
            //if (useTextHudApi)
            //    return new TextApiRacingHud(config, mapSettings);
            //else
                return new VanillaRacingHud();
        }

        public override string ToString()
        {
            return text.ToString();
        }
    }
}
