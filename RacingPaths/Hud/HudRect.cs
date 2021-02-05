﻿using Draygo.API;
using VRage.Utils;
using VRageMath;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

namespace avaness.RacingPaths.Hud
{
    public class HudRect
    {
        protected readonly Vector2D origin;
        protected readonly HudAPIv2.BillBoardHUDMessage board;

        public virtual Vector2D Size
        {
            get
            {
                return new Vector2D(board.Width, board.Height);
            }
            set
            {
                board.Width = (float)value.X;
                board.Height = (float)value.Y;
                board.Origin = origin + new Vector2D(value.X * 0.5, value.Y * -0.5);
            }
        }

        public virtual double Height
        {
            get
            {
                return board.Height;
            }
            set
            {
                board.Height = (float)value;
                board.Origin = new Vector2D(board.Origin.X, origin.Y + (value * -0.5));
            }
        }

        public virtual double Width
        {
            get
            {
                return board.Width;
            }
            set
            {
                board.Width = (float)value;
                board.Origin = new Vector2D(origin.X + (value * 0.5), board.Origin.Y);
            }
        }

        public HudRect(Vector2D origin, Vector2D size, string texture) : this(origin, size, Color.White, texture)
        {
        }

        public HudRect(Vector2D origin, Vector2D size, Color color, string texture = "Square")
        {
            board = new HudAPIv2.BillBoardHUDMessage(MyStringId.GetOrCompute(texture), origin + new Vector2D(size.X * 0.5, size.Y * -0.5), color, Width: (float)size.X, Height: (float)size.Y, HideHud: false, Blend: BlendTypeEnum.PostPP);
            this.origin = origin;
        }
    }
}
