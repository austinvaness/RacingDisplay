using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRageMath;
using Draygo.API;
using VRage.Utils;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;
using avaness.RacingPaths.Storage;

namespace avaness.RacingPaths.Hud
{
    public class LeaderboardHud<T>
    {
        private const string fontId = "FreeMono_Racing";

        private readonly List<HudRect> billboards = new List<HudRect>();
        private readonly List<HudText> labels = new List<HudText>();
        private readonly HudButton btn;

        public LeaderboardHud(PathStorage paths, Cursor cursor, Vector2D origin, double height, params string[] cols)
        {
            MyStringId mat = MyStringId.GetOrCompute("Square");

            Color fontColor = new Color(220, 235, 242);
            Color bgColor = new Color(41, 54, 62, 150);
            Color bodyColor = new Color(58, 68, 77);

            if (cols.Length == 0)
                return;


            HudRect bg = new HudRect(origin, new Vector2D(0, height), bgColor);
            billboards.Add(bg); // Background

            HudRect header = new HudRect(origin, new Vector2D(), bodyColor);
            billboards.Add(header);

            Vector2D pixel = HudAPIv2.APIinfo.ScreenPositionOnePX;
            Vector2D tempPos = origin;
            for (int i = 0; i < cols.Length; i++)
            {
                if (i > 0)
                    billboards.Add(new HudRect(tempPos, new Vector2D(pixel.X, height), bodyColor)); // Vertical Lines

                double colWidth = pixel.X * 10;
                if(!string.IsNullOrEmpty(cols[i]))
                {
                    HudText label = new HudText(cols[i], tempPos, fontColor, fontId);
                    labels.Add(label);
                    Vector2D labelArea = label.GetTextLength();
                    if (header.Height == 0)
                        header.Height = Math.Abs(labelArea.Y);
                    colWidth += Math.Abs(labelArea.X);
                }
                tempPos.X += colWidth;
            }

            bg.Width = tempPos.X;
            header.Width = tempPos.X;

            tempPos = new Vector2D(origin.X, origin.Y - header.Height);
            int rowCount = (int)((height - header.Height) / header.Height);
            if(rowCount > 0)
            {
                double rowStep = (height - header.Height) / rowCount;
                for (int i = 0; i < rowCount; i++)
                {
                    if (i > 0)
                        billboards.Add(new HudRect(tempPos, new Vector2D(bg.Width, pixel.Y), bodyColor));

                    tempPos.Y -= rowStep;
                }
            }

            btn = new HudButton(cursor, origin, new Vector2D(0.1), Color.White, "Square");
        }

        public void Load(IEnumerable<SerializablePathInfo> paths)
        {
            
        }

        public void Unload()
        {
            btn.Unload();
        }

        //public event Action<string, T> OnRowSelected;
    }
}
