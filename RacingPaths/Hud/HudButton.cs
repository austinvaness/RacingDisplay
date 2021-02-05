using Sandbox.ModAPI;
using System;
using VRageMath;

namespace avaness.RacingPaths.Hud
{
    public class HudButton : HudRect
    {
        private BoundingBox2D area;
        private Color baseColor;
        private double alpha = 1;
        private readonly Cursor cursor;

        public HudButton(Cursor cursor, Vector2D origin, Vector2D size, Color color, string texture) : base (origin, size, color, texture)
        {
            baseColor = color;
            this.cursor = cursor;
            cursor.OnMouseMoved += MouseMoved;
            cursor.OnMouseDown += MouseDown;
            UpdateArea();
        }

        public void Unload()
        {
            cursor.OnMouseMoved -= MouseMoved;
            cursor.OnMouseDown -= MouseDown;
        }

        public override Vector2D Size
        {
            get
            {
                return base.Size;
            }

            set
            {
                base.Size = value;
                UpdateArea();
            }
        }

        public override double Height
        {
            get
            {
                return base.Height;
            }

            set
            {
                base.Height = value;
                UpdateArea();
            }
        }

        public override double Width
        {
            get
            {
                return base.Width;
            }

            set
            {
                base.Width = value;
                UpdateArea();
            }
        }

        private void UpdateArea()
        {
            Vector2D origin = board.Origin;
            Vector2D size = base.Size * 0.5;
            area = new BoundingBox2D(origin - size, origin + size);
        }

        private void SetTransparency(double alpha)
        {
            if(this.alpha != alpha)
            {
                board.BillBoardColor = baseColor * (float)alpha;
                this.alpha = alpha;
            }
        }

        private void MouseDown(Vector2D pos)
        {

        }

        private void MouseMoved(Vector2D pos)
        {
            bool contains = area.Contains(pos) != ContainmentType.Disjoint;
            if (contains)
            {
                SetTransparency(1);
            }
            else
            {
                SetTransparency(0.5);
            }
            MyAPIGateway.Utilities.ShowNotification($"Contains? {contains}\n{area} {pos}", 16);
        }
    }
}
