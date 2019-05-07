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
        string customData = "";
        float nodeNumber = float.NaN;

        public override void Init (MyObjectBuilder_EntityBase objectBuilder)
        {
            b = Entity as IMyBeacon;
            if(MyAPIGateway.Multiplayer.IsServer)
                NeedsUpdate = MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        public override void Close ()
        {
            if (!float.IsNaN(nodeNumber))
                RacingSession.Instance.Nodes.Remove(nodeNumber);
            b = null;
            NeedsUpdate = MyEntityUpdateEnum.NONE;
        }

        public override void UpdateBeforeSimulation100 ()
        {
            if (b?.CubeGrid?.Physics == null || !b.CustomName.StartsWith("[Node]"))
                return;

            if (b.CustomData != customData)
            {
                // Value changed
                if (!float.IsNaN(nodeNumber))
                    RacingSession.Instance.Nodes.Remove(nodeNumber);

                customData = b.CustomData;
                float temp;
                if (float.TryParse(b.CustomData, out temp))
                    nodeNumber = temp;
                else
                    nodeNumber = float.NaN;
            }
            if (!float.IsNaN(nodeNumber))
                RacingSession.Instance.Nodes [nodeNumber] = b;

        }
    }
}
