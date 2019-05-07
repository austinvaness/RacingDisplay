using VRage.Game.Components;
using VRage.ObjectBuilders;
using VRage.ModAPI;
using Sandbox.ModAPI;
using Sandbox.Common.ObjectBuilders;

namespace RacingMod
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Beacon), false)]
    public class RacingBeacon : MyGameLogicComponent
    {
        IMyBeacon b = null;
        string data = "";
        float value = float.NaN;

        public override void Init (MyObjectBuilder_EntityBase objectBuilder)
        {
            b = Entity as IMyBeacon;
            if(MyAPIGateway.Multiplayer.IsServer)
                NeedsUpdate = MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        public override void Close ()
        {
            if (!float.IsNaN(value))
                RacingSession.Instance.Nodes.Remove(value);
            b = null;
            NeedsUpdate = MyEntityUpdateEnum.NONE;
        }

        public override void UpdateBeforeSimulation100 ()
        {
            if (b?.CubeGrid?.Physics == null || !b.CustomName.StartsWith("[Node]"))
                return;

            if (b.CustomData != data)
            {
                // Value changed
                if (!float.IsNaN(value))
                    RacingSession.Instance.Nodes.Remove(value);

                data = b.CustomData;
                float temp;
                if (float.TryParse(b.CustomData, out temp))
                    value = temp;
                else
                    value = float.NaN;
            }
            if (!float.IsNaN(value))
                RacingSession.Instance.Nodes [value] = b;

        }
    }
}
