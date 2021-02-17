using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRageMath;
using Draygo.API;
using VRage.Utils;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;
using avaness.RacingLeaderboard.Storage;
using avaness.RacingLeaderboard.Data;
using avaness.RacingLeaderboard.Recording;

namespace avaness.RacingLeaderboard.Hud
{
    public class LeaderboardHud
    {
        private const string fontId = "FreeMono_Racing";
        private const string timeFormat = "mm\\:\\.ff";

        private readonly List<HudRect> billboards = new List<HudRect>();
        private readonly List<HudText> labels = new List<HudText>();
        private readonly PathRow[] rows;
        private bool visible = true;

        public bool Visible
        {
            get
            {
                return visible;
            }
            set
            {
                if(visible != value)
                {
                    visible = value;
                    foreach (HudRect rect in billboards)
                        rect.Visible = value;
                    foreach (HudText text in labels)
                        text.Visible = value;
                }
            }
        }

        public LeaderboardHud(PlaybackManager play, Cursor cursor, Vector2D origin, double height, params string[] cols)
        {
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
            double[] colWidths = new double[cols.Length];
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
                    double width = Math.Abs(labelArea.X);
                    colWidths[i] = width;
                    colWidth += width;
                }
                tempPos.X += colWidth;
            }

            bg.Width = tempPos.X;
            header.Width = tempPos.X;

            List<PathRow> rows = new List<PathRow>();
            tempPos = new Vector2D(origin.X, origin.Y - header.Height);
            int rowCount = (int)((height - header.Height) / header.Height);
            if(rowCount > 0)
            {
                double rowStep = (height - header.Height) / rowCount;
                Vector2D rowSize = new Vector2D(bg.Width, rowStep);
                for (int i = 0; i < rowCount; i++)
                {
                    if (i > 0)
                        billboards.Add(new HudRect(tempPos, new Vector2D(bg.Width, pixel.Y), bodyColor));
                    rows.Add(new PathRow(cursor, play, tempPos, rowStep, colWidths));
                    tempPos.Y -= rowStep;
                }
            }

            this.rows = rows.ToArray();
        }

        public void Unload()
        {
            foreach (PathRow r in rows)
                r.Unload();
        }

        //public event Action<string, T> OnRowSelected;

        private class PathRow
        {
            private HudRect btnPlay, btnDelete;
            private HudText[] cols;
            private BoundingBox2D area;
            private readonly Cursor cursor;
            private readonly PlaybackManager play;
            
            private ulong playerId;
            private bool playSelected;
            private BtnHover selection = BtnHover.None;

            public PathRow(Cursor cursor, PlaybackManager play, Vector2D corner, double height, double[] colWidths)
            {
                this.cursor = cursor;
                this.play = play;
                
                btnPlay = new HudRect(corner, new Vector2D(1, height), "RCG_PlayBtn");
                btnPlay.SquarifyWidth();
                btnPlay.Origin = new Vector2D(corner.X - btnPlay.Width, corner.Y);
                btnPlay.Visible = false;

                cols = new HudText[colWidths.Length];
                for (int i = 0; i < colWidths.Length; i++)
                {
                    cols[i] = new HudText("", corner, Color.White, fontId);
                    corner.X += colWidths[i];
                }

                btnDelete = new HudRect(corner, new Vector2D(1, height), "RCG_Cross");
                btnDelete.SquarifyWidth();
                btnDelete.Visible = false;

                cursor.OnMouseMoved += MouseMoved;
                cursor.OnMouseDown += MouseDown;
                
                area = new BoundingBox2D(Vector2D.Min(btnPlay.Area.Min, btnDelete.Area.Min), Vector2D.Max(btnDelete.Area.Max, btnDelete.Area.Max));
            }

            public void SetData(int index, Path p)
            {
                playSelected = play.IsPlaying(p.PlayerId);
                playerId = p.PlayerId;
                cols[0].Text = (index + 1).ToString();
                cols[1].Text = p.PlayerName;
                cols[2].Text = p.Length.ToString(timeFormat);
            }

            public void Unload()
            {
                cursor.OnMouseMoved -= MouseMoved;
                cursor.OnMouseDown -= MouseDown;
            }

            private void MouseDown(Vector2D pos)
            {
                if (playerId == 0)
                    return;

                switch (selection)
                {
                    case BtnHover.Play:
                        playSelected = play.TogglePlay(playerId);
                        break;
                    case BtnHover.Delete:

                        break;
                }
            }

            private void MouseMoved(Vector2D pos)
            {
                if(playerId != 0 && area.Contains(pos) == ContainmentType.Contains)
                {
                    btnPlay.Visible = true;
                    btnDelete.Visible = true;
                    if (btnPlay.Area.Contains(pos) == ContainmentType.Contains)
                    {
                        btnPlay.Alpha = 1;
                        btnDelete.Alpha = 0.5;
                        selection = BtnHover.Play;
                    }
                    else if(btnDelete.Area.Contains(pos) == ContainmentType.Contains)
                    {
                        btnPlay.Alpha = 0.5;
                        btnDelete.Alpha = 1;
                        selection = BtnHover.Delete;
                    }
                    else
                    {
                        btnPlay.Alpha = 0.5;
                        btnDelete.Alpha = 0.5;
                        selection = BtnHover.None;
                    }
                }
                else
                {
                    btnPlay.Visible = playSelected;
                    btnDelete.Visible = false;
                    selection = BtnHover.None;
                }
            }

            private enum BtnHover
            {
                None, Play, Delete
            }
        }
    }
}
