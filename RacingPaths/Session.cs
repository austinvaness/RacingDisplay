using Sandbox.ModAPI;
using VRage.Game.Components;

namespace avaness.RacingPaths
{
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class RacingPathsSession : MySessionComponentBase
    {
        public static RacingPathsSession Instance;

        private bool init = false;

        public override void LoadData()
        {
            Instance = this;
        }

        protected override void UnloadData()
        {
            Instance = null;
        }

        private void Start()
        {
            init = true;
        }

        public override void UpdateAfterSimulation()
        {
            if (MyAPIGateway.Session == null)
                return;
            if (!init)
                Start();


        }

        public override void SaveData()
        {

        }
    }
}