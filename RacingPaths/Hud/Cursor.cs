using Draygo.API;
using Sandbox.ModAPI;
using System;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

namespace avaness.RacingPaths.Hud
{
    public class Cursor
    {
        private HudAPIv2.BillBoardHUDMessage sprite;
        private Vector2 prevMouse;
        private bool visible;
        private bool init;

        public bool Visible
        {
            get
            {
                return visible;
            }
            set
            {
                if(init && visible != value)
                {
                    visible = value;
                    sprite.Visible = value;
                    if (value)
                        sprite.Origin = GetHudPos(MyAPIGateway.Input.GetMousePosition());
                    OnVisibleChanged?.Invoke(value);
                }
            }
        }

        public Cursor()
        {

        }

        public void Create()
        {
            init = true;
            Vector2D size = HudAPIv2.APIinfo.ScreenPositionOnePX * 64;
            sprite = new HudAPIv2.BillBoardHUDMessage(MyStringId.GetOrCompute("MouseCursor"), Vector2D.Zero, Color.White, Width: (float)size.X, Height: (float)size.Y, Blend: BlendTypeEnum.PostPP)
            {
                Visible = visible
            };
        }

        public void Unload()
        {
            OnMouseDown = null;
            OnMouseMoved = null;
            OnVisibleChanged = null;
        }

        private Vector2D GetHudPos(Vector2 screenPos)
        {
            Vector2 halfSize = MyAPIGateway.Session.Camera.ViewportSize / 2;
            if (halfSize != Vector2.Zero)
            {
                Vector2 pos = screenPos - halfSize;
                return new Vector2D(Math.Min(pos.X / halfSize.X, 1), -Math.Min(pos.Y / halfSize.Y, 1));
            }
            return Vector2D.Zero;
        }

        public void Draw()
        {
            if (!visible || !init)
                return;

            if (!MyAPIGateway.Gui.ChatEntryVisible)
            {
                Visible = false;
                return;
            }

            IMyInput input = MyAPIGateway.Input;

            Vector2 mouse = input.GetMousePosition();
            if(mouse != prevMouse)
            {
                Vector2D pos = GetHudPos(mouse);
                sprite.Origin = pos;
                OnMouseMoved?.Invoke(pos);
                prevMouse = mouse;
            }

            if (input.IsNewLeftMousePressed())
                OnMouseDown?.Invoke(sprite.Origin);
        }

        public event Action<Vector2D> OnMouseMoved;
        public event Action<Vector2D> OnMouseDown;
        public event Action<bool> OnVisibleChanged;
    }
}
