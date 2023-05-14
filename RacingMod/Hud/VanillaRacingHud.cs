using Sandbox.Game;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI;
using VRageMath;

namespace avaness.RacingMod.Hud
{
    public class VanillaRacingHud : RacingHud
    {
        private static readonly Vector2 hudPosition = new Vector2(-0.125f, 0.05f);
        private const float fontSize = 0.7f;
        private const long stringId = 1708268562;
        private readonly Dictionary<string, UiString> uiStrings = new Dictionary<string, UiString>();

        private int lineIndex = 0;
        private int lineLength = 0;
        private UiString currentString;
        private string font = RacingConstants.fontId;

        public VanillaRacingHud()
        {
            string whiteFont = RacingConstants.colorWhite.AltFont;
            uiStrings.Add(whiteFont, new UiString(stringId, whiteFont));
            currentString = uiStrings.Values.First();
        }


        public override void Broadcast()
        {
            if (RacingSession.Instance.Runticks % 10 != 0)
                return;

            foreach (UiString s in uiStrings.Values)
                s.Update();
        }

        public override RacingHud Clear()
        {
            foreach (UiString s in uiStrings.Values)
                s.Clear();
            lineIndex = 0;
            lineLength = 0;
            currentString = uiStrings[RacingConstants.colorWhite.AltFont];
            font = currentString.Font;
            return this;
        }

        public override RacingHud AppendLine()
        {
            lineIndex++;
            lineLength = 0;
            currentString.AppendLine();
            return this;
        }

        public override RacingHud Append(HudColor color)
        {
            font = color.AltFont;
            return this;
        }

        private void CheckForFontChange()
        {
            if(font != currentString.Font)
            {
                if(!uiStrings.TryGetValue(font, out currentString))
                {
                    currentString = new UiString(stringId + uiStrings.Count, font);
                    uiStrings.Add(font, currentString);
                }
                currentString.SetPosition(lineIndex, lineLength);
            }
        }

        public override RacingHud Append(string value)
        {
            CheckForFontChange();
            lineLength += value.Length;
            currentString.Append(value);
            return this;
        }

        public override RacingHud Append(char value)
        {
            CheckForFontChange();
            lineLength++;
            currentString.Append(value);
            return this;
        }

        public override RacingHud Append(int value)
        {
            CheckForFontChange();
            lineLength += currentString.Append(value);
            return this;
        }

        private class UiString
        {
            public string Font { get; }
            public bool IsEmpty => Text.Length == baseLength;

            private readonly long id;

            private StringBuilder Text = new StringBuilder();
            private int lines;
            private int chars;
            private bool active;
            private int baseLength;

            public UiString(long id, string font)
            {
                this.id = id;
                Font = font;
            }

            public void Clear()
            {
                Text.Clear();
                baseLength = 0;
                lines = 0;
                chars = 0;
            }

            public void SetPosition(int lines, int chars)
            {
                bool empty = IsEmpty;
                for (int i = this.lines; i < lines; i++)
                    Text.AppendLine();
                if(chars > this.chars)
                    Text.Append(' ', chars - this.chars);
                if (empty)
                    baseLength = Text.Length;
            }

            public void AppendLine()
            {
                bool empty = IsEmpty;
                Text.AppendLine();
                lines++;
                chars = 0;
                if (empty)
                    baseLength = Text.Length;
            }

            public void Append(string value)
            {
                Text.Append(value);
                chars += value.Length;
            }

            public void Append(char value)
            {
                Text.Append(value);
                chars++;
            }

            public int Append(int value)
            {
                int start = Text.Length;
                Text.Append(value);
                int length = Text.Length - start;
                chars += length;
                return length;
            }

            public void Update()
            {
                if(IsEmpty)
                {
                    if(active)
                    {
                        MyVisualScriptLogicProvider.RemoveUIString(id, -1);
                        active = false;
                    }
                }
                else
                {
                    MyVisualScriptLogicProvider.CreateUIString(id, Text.ToString(), hudPosition.X, hudPosition.Y, fontSize, Font, playerId: -1);
                    active = true;
                }
            }
        }
    }
}
