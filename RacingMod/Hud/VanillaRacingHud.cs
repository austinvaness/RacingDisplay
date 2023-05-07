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
        private readonly List<UiString> uiStrings = new List<UiString>();

        private string font = RacingConstants.fontId;
        private int lineIndex = 0;
        private int stringIndex = 0;

        public VanillaRacingHud()
        {
            uiStrings.Add(new UiString(stringId, RacingConstants.fontId));
            text = uiStrings[0].Text;
        }


        public override void Broadcast()
        {
            if (RacingSession.Instance.Runticks % 10 != 0)
                return;

            foreach (UiString s in uiStrings)
                s.Update();
        }

        public override RacingHud Clear()
        {
            for (int i = 0; i <= stringIndex; i++)
                uiStrings[i].Clear();
            lineIndex = 0;
            stringIndex = -1;
            MoveToNextString();
            return this;
        }

        public override RacingHud AppendLine()
        {
            lineIndex++;

            // Check if a new font is needed
            UiString s = uiStrings[stringIndex];
            if (font != s.Font)
                MoveToNextString();
            else
                s.AppendLine();
            return this;
        }

        private void MoveToNextString()
        {
            stringIndex++;

            // Create or get the existing string
            UiString s;
            if (stringIndex < uiStrings.Count)
            {
                s = uiStrings[stringIndex];
                s.Font = font;
            }
            else
            {
                s = new UiString(stringId + uiStrings.Count, font);
                uiStrings.Add(s);
            }

            // Move the string to the correct line (measuring line heights is difficult with the mod api)
            text = s.Text;
            s.Clear();
            s.SetLine(lineIndex);
        }

        public override RacingHud Append(HudColor color)
        {
            AppendFont(color.AltFont);
            return this;
        }

        private void AppendFont(string font)
        {
            this.font = font;

            UiString s = uiStrings[stringIndex];
            if (s.IsEmpty) // If current string has no text, change font for the current string
                s.Font = font;
            else if (EndsWithNewline()) // If the current position is at a blank newline, create a new string object
                MoveToNextString();
        }

        private bool EndsWithNewline()
        {
            string newline = Environment.NewLine;
            int i = newline.Length - 1;
            int j = text.Length - 1;
            while(i >= 0 && j >= 0)
            {
                if (text[j] != newline[i])
                    return false;
                i--;
                j--;
            }
            return true;
        }

        private class UiString
        {
            public StringBuilder Text = new StringBuilder();
            public string Font;

            private readonly long id;

            private bool active;
            private int baseLength;
            public bool IsEmpty => Text.Length == baseLength;

            public UiString(long id, string font)
            {
                this.id = id;
                Font = font;
            }

            public void Clear()
            {
                Text.Clear();
                baseLength = 0;
            }

            public void SetLine(int lineIndex)
            {
                for (int i = 0; i < lineIndex; i++)
                    Text.AppendLine();
                baseLength = Text.Length;
            }

            public void AppendLine()
            {
                if (IsEmpty)
                {
                    Text.AppendLine();
                    baseLength = Text.Length;
                }
                else
                {
                    Text.AppendLine();
                }
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
