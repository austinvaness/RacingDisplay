using Sandbox.ModAPI;
using System;
using VRageMath;

namespace avaness.RacingLeaderboard.Hud
{
    public class HudButton : HudRect
    {
        private readonly Cursor cursor;
        
        public bool MouseOver { get; private set; }

        public event Action OnClicked;

        public HudButton(Cursor cursor, Vector2D origin, Vector2D size, Color color, string texture) : base (origin, size, color, texture)
        {
            this.cursor = cursor;
            cursor.OnMouseMoved += MouseMoved;
            cursor.OnMouseDown += MouseDown;
            UpdateArea();
        }

        public void Unload()
        {
            cursor.OnMouseMoved -= MouseMoved;
            cursor.OnMouseDown -= MouseDown;
            OnClicked = null;
        }

        protected override void UpdateArea()
        {
            MouseOver = false;
            base.UpdateArea();
        }

        private void MouseDown(Vector2D pos)
        {
            if (MouseOver && OnClicked != null)
                OnClicked.Invoke();
        }

        private void MouseMoved(Vector2D pos)
        {
            MouseOver = Area.Contains(pos) != ContainmentType.Disjoint;
        }
    }
}
