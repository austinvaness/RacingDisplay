using Sandbox.ModAPI;
using VRage.Input;

// In part from Relative Spectator v2, Keybind.cs
namespace RacingMod
{
	public struct Keybind
	{
		public MyKeys key;
		public bool shift;
		public bool ctrl;
		public bool alt;

		public Keybind(MyKeys key, bool shift, bool ctrl, bool alt) : this()
		{
			this.key = key;
			this.shift = shift;
			this.ctrl = ctrl;
			this.alt = alt;
		}

		public static bool operator ==(Keybind a, Keybind b)
		{
			return a.Equals(b);
		}

		public static bool operator != (Keybind a, Keybind b)
		{
			return !a.Equals(b);
		}

		public override bool Equals (object obj)
		{
			if(obj is Keybind)
			{
				Keybind keybind = (Keybind)obj;
				return key == keybind.key &&
					   shift == keybind.shift &&
					   ctrl == keybind.ctrl &&
					   alt == keybind.alt;
			}
			return false;
		}

		public override int GetHashCode ()
		{
			var hashCode = -1726813277;
			hashCode = hashCode * -1521134295 + key.GetHashCode();
			hashCode = hashCode * -1521134295 + shift.GetHashCode();
			hashCode = hashCode * -1521134295 + ctrl.GetHashCode();
			hashCode = hashCode * -1521134295 + alt.GetHashCode();
			return hashCode;
		}

		public bool IsKeybindPressed()
		{
			if (key == MyKeys.None)
				return false;
			return MyAPIGateway.Input.IsNewKeyPressed(key) && (shift == MyAPIGateway.Input.IsAnyShiftKeyPressed()) && (ctrl == MyAPIGateway.Input.IsAnyCtrlKeyPressed()) && (alt == MyAPIGateway.Input.IsAnyAltKeyPressed());
        }

		public override string ToString()
		{
			return $"{(shift ? "Shift " : "")}{(ctrl ? "Ctrl " : "")}{(alt ? "Alt " : "")}{key}";
		}

		public static implicit operator Keybind (MyKeys key)
		{
			return new Keybind(key, false, false, false);
		}
	}
}
