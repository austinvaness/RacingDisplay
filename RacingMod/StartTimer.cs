using Sandbox.Common.ObjectBuilders;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using System;
using VRage.Game.Components;
using VRage.ModAPI;
using VRage.ObjectBuilders;

namespace avaness.RacingMod
{

    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_TimerBlock), false)]
    class RacingStartTrigger : MyGameLogicComponent
    {
        IMyTimerBlock Timer;
        bool valid = false;

        public override void Init (MyObjectBuilder_EntityBase objectBuilder)
        {
            Timer = Entity as IMyTimerBlock;

            if (RacingConstants.IsServer)
            {
                // wait for the grid to fully load in
                NeedsUpdate = MyEntityUpdateEnum.EACH_100TH_FRAME;

                // register events
                Timer.CustomNameChanged += OnCustomNameChanged;
            }
        }

        public override void Close ()
        {
            if (Timer != null)
            {
                if (valid)
                    RacingSession.Instance.StartTimers.Remove(Timer);
                // Unregister events
                Timer.CustomNameChanged -= OnCustomNameChanged;
            }
            NeedsUpdate = MyEntityUpdateEnum.NONE;
        }

        public override void UpdateBeforeSimulation100 ()
        {
            try
            {
                if (RacingSession.Instance == null)
                    return;

                if (Timer?.CubeGrid?.Physics == null)
                    return;

                OnCustomNameChanged(Timer);

                // done waiting, bail out of here for good
                NeedsUpdate = MyEntityUpdateEnum.NONE;
            }
            catch (Exception e)
            {
                RacingTools.ShowError(e, GetType());
            }
        }

        private void OnCustomNameChanged (IMyTerminalBlock timer)
        {
            valid = Timer.CustomName.ToLower().StartsWith("[racetimer]");
            if (valid)
                RacingSession.Instance.StartTimers.Add(Timer);
        }
    }
}
