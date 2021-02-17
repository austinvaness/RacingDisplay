using Draygo.API;
using System;
using VRage.Utils;
using VRageMath;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

namespace avaness.RacingLeaderboard.Hud
{
    public class HudRect
    {
        private readonly Color color;
        protected Vector2D origin; // Top left corner
        protected readonly HudAPIv2.BillBoardHUDMessage board;
        
        public virtual BoundingBox2D Area { get; protected set; }

        public virtual double Alpha
        {
            get
            {
                return alpha;
            }
            set
            {
                if(alpha != value)
                {
                    alpha = value;
                    board.BillBoardColor = color * (float)alpha;
                }
            }
        }
        private double alpha;

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
                UpdateArea();
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
                UpdateArea();
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
                UpdateArea();
            }
        }

        public virtual bool Visible
        {
            get
            {
                return visible;
            }
            set
            {
                if(visible != value)
                {
                    board.Visible = value;
                    visible = value;
                }
            }
        }
        private bool visible = true;

        public virtual Vector2D Origin
        {
            get
            {
                return origin;
            }
            set
            {
                board.Origin = value + new Vector2D(board.Width * 0.5, board.Height * -0.5);
                origin = value;
                UpdateArea();
            }
        }

        public virtual MyStringId Material
        {
            get
            {
                return board.Material;
            }
            set
            {
                board.Material = value;
            }
        }

        public HudRect(Vector2D origin, Vector2D size, string texture) : this(origin, size, Color.White, texture)
        {
        }

        public HudRect(Vector2D origin, Vector2D size, Color color, string texture = "Square")
        {
            this.color = color;
            alpha = color.A / 255f;
            board = new HudAPIv2.BillBoardHUDMessage(MyStringId.GetOrCompute(texture), origin + new Vector2D(size.X * 0.5, size.Y * -0.5), color, Width: (float)size.X, Height: (float)size.Y, HideHud: false, Blend: BlendTypeEnum.PostPP);
            this.origin = origin;
            UpdateArea();
        }

        public void SquarifyHeight()
        {
            Vector2D pixel = HudAPIv2.APIinfo.ScreenPositionOnePX;
            double pixelWidth = board.Width / pixel.X;
            Height = pixelWidth * pixel.Y;
        }

        public void SquarifyWidth()
        {
            Vector2D pixel = HudAPIv2.APIinfo.ScreenPositionOnePX;
            double pixelHeight = board.Height / pixel.Y;
            Width = pixelHeight * pixel.X;
        }

        protected virtual void UpdateArea()
        {
            Area = new BoundingBox2D(new Vector2D(origin.X, origin.Y - board.Height), new Vector2D(origin.X + board.Width, origin.Y));
        }
    }
}
