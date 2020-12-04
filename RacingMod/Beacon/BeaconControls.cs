﻿using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using System.Collections.Generic;
using System.Text;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;

namespace avaness.RacingMod.Beacon
{
    public static class BeaconControls
    {
        private static bool controls;

        public static void CreateControls()
        {
            if (controls)
                return;

            IMyTerminalControlSeparator sep = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSeparator, IMyBeacon>("RCD_Sep");
            sep.Visible = IsStatic;
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(sep);

            IMyTerminalControlListbox existingTracks = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlListbox, IMyBeacon>("RCD_AllTracks");
            existingTracks.Visible = IsStatic;
            existingTracks.Enabled = IsStatic;
            existingTracks.Multiselect = false;
            existingTracks.VisibleRowsCount = 5;
            existingTracks.Title = MyStringId.GetOrCompute("All Tracks");
            existingTracks.ListContent = GetAllTracks;
            existingTracks.ItemSelected = SelectTrack;
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(existingTracks);

            IMyTerminalControlTextbox trackName = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlTextbox, IMyBeacon>("RCD_TrackName");
            trackName.Visible = IsStatic;
            trackName.Enabled = IsStatic;
            trackName.Getter = GetTrackName;
            trackName.Setter = SetTrackName;
            trackName.Title = MyStringId.GetOrCompute("Selected Track");
            trackName.Tooltip = MyStringId.GetOrCompute("Multi track support coming soon.");//"Select a track above or enter a new track name.\nNames are case sensitive.");
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(trackName);

            IMyTerminalControlCheckbox enabled = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyBeacon>("RCD_Enabled");
            enabled.Enabled = IsStatic;
            enabled.Visible = IsStatic;
            enabled.Getter = GetEnabled;
            enabled.Setter = SetEnabled;
            enabled.Title = MyStringId.GetOrCompute("Enabled");
            enabled.Tooltip = MyStringId.GetOrCompute("When checked, the beacon is in use on the selected track.");
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(enabled);

            IMyTerminalControlCheckbox isCheckpoint = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyBeacon>("RCD_Checkpoint");
            isCheckpoint.Visible = IsStatic;
            isCheckpoint.Enabled = IsStatic;
            isCheckpoint.Getter = GetCheckpoint;
            isCheckpoint.Setter = SetCheckpoint;
            isCheckpoint.Title = MyStringId.GetOrCompute("Checkpoint");
            isCheckpoint.Tooltip = MyStringId.GetOrCompute("When checked, racers must pass through the grid of the beacon to continue the race.");
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(isCheckpoint);

            IMyTerminalControlCheckbox gridPosition = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCheckbox, IMyBeacon>("RCD_GridPosition");
            gridPosition.Visible = IsStatic;
            gridPosition.Enabled = IsStatic;
            gridPosition.Getter = GetGridPosition;
            gridPosition.Setter = SetGridPosition;
            gridPosition.Title = MyStringId.GetOrCompute("Grid Positioning");
            gridPosition.Tooltip = MyStringId.GetOrCompute("When checked, the beacon will use the center of the grid as its position for the race.");
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(gridPosition);

            IMyTerminalControlTextbox nodeNum = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlTextbox, IMyBeacon>("RCD_NodeNum");
            nodeNum.Visible = IsStatic;
            nodeNum.Enabled = IsStatic;
            nodeNum.Getter = GetNodeNum;
            nodeNum.Setter = SetNodeNum;
            nodeNum.Title = MyStringId.GetOrCompute("Number");
            nodeNum.Tooltip = MyStringId.GetOrCompute("The number representing the position of this beacon along the track.");
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(nodeNum);


            IMyTerminalControlButton set = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyBeacon>("RCD_Update");
            set.Visible = IsStatic;
            set.Enabled = HasChanges;
            set.Action = UpdateStorage;
            set.Title = MyStringId.GetOrCompute("Save");
            set.Tooltip = MyStringId.GetOrCompute("Save the settings for this beacon.");
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(set);

            IMyTerminalControlButton cancel = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyBeacon>("RCD_Cancel");
            cancel.Visible = IsStatic;
            cancel.Enabled = HasChanges;
            cancel.Action = ResetStorage;
            cancel.Title = MyStringId.GetOrCompute("Cancel");
            cancel.Tooltip = MyStringId.GetOrCompute("Revert the settings for this beacon.");
            MyAPIGateway.TerminalControls.AddControl<IMyBeacon>(cancel);

            controls = true;
        }

        private static void SelectTrack(IMyTerminalBlock block, List<MyTerminalControlListBoxItem> sel)
        {
            /*if (sel.Count == 0)
                return;

            string selected = (string)sel[0].UserData;

            BeaconStorage s = block.GameLogic.GetAs<RacingBeacon>().Storage;
            if (s.HasTemp())
            {
                s.Temporary.TrackName = selected;
                RefreshUI(block);
            }
            else
            {
                if (s.TrackName != selected)
                {
                    s.CreateTemp();
                    s.Temporary.TrackName = selected;
                    RefreshUI(block);
                }

            }*/

        }

        private static void GetAllTracks(IMyTerminalBlock block, List<MyTerminalControlListBoxItem> all, List<MyTerminalControlListBoxItem> sel)
        {
            all.Add(new MyTerminalControlListBoxItem(MyStringId.GetOrCompute("Default"), MyStringId.GetOrCompute("Multi track support coming soon."), "Default"));
            /*string current = GetLatestStorage(block).TrackName;

            foreach (string name in RacingSession.Instance.TestList)
            {
                var item = new MyTerminalControlListBoxItem(MyStringId.GetOrCompute(name), MyStringId.NullOrEmpty, name);
                all.Add(item);
                if (name == current)
                    sel.Add(item);
            }*/
        }

        private static void UpdateStorage(IMyTerminalBlock block)
        {
            BeaconStorage s = block.GameLogic.GetAs<RacingBeacon>().Storage;
            s.ApplyTemp();
            RefreshUI(block);
        }

        private static void ResetStorage(IMyTerminalBlock block)
        {
            BeaconStorage s = block.GameLogic.GetAs<RacingBeacon>().Storage;
            s.DeleteTemp();
            RefreshUI(block);
        }

        private static bool GetGridPosition(IMyTerminalBlock block)
        {
            BeaconStorage s = GetLatestStorage(block);
            return s.GridPosition;
        }

        private static void SetGridPosition(IMyTerminalBlock block, bool gridPosition)
        {
            BeaconStorage s = GetTemporaryStorage(block);
            s.GridPosition = gridPosition;
        }

        private static StringBuilder GetTrackName(IMyTerminalBlock block)
        {
            return new StringBuilder("Default");
            /*BeaconStorage s = GetLatestStorage(block);
            return new StringBuilder(s.TrackName);*/
        }

        private static void SetTrackName(IMyTerminalBlock block, StringBuilder name)
        {
            /*string temp = name.ToString().Trim();
            if (string.IsNullOrWhiteSpace(temp))
                temp = "Default";
            BeaconStorage s = GetTemporaryStorage(block);
            s.TrackName = temp;*/
        }

        private static void SetNodeNum(IMyTerminalBlock block, StringBuilder sb)
        {
            string str = sb.ToString();
            float num;
            if (float.TryParse(str, out num))
            {
                BeaconStorage s = GetTemporaryStorage(block);
                s.NodeNum = num;
            }
        }

        private static StringBuilder GetNodeNum(IMyTerminalBlock block)
        {
            BeaconStorage s = GetLatestStorage(block);
            return new StringBuilder().Append(s.NodeNum);
        }

        private static void SetCheckpoint(IMyTerminalBlock block, bool checkpoint)
        {
            BeaconStorage s = GetTemporaryStorage(block);
            s.Checkpoint = checkpoint;
        }

        private static bool GetCheckpoint(IMyTerminalBlock block)
        {
            BeaconStorage s = GetLatestStorage(block);
            return s.Checkpoint;
        }

        private static void SetEnabled(IMyTerminalBlock block, bool enabled)
        {
            BeaconStorage s = GetTemporaryStorage(block);
            s.Enabled = enabled;
        }

        private static bool GetEnabled(IMyTerminalBlock block)
        {
            BeaconStorage s = GetLatestStorage(block);
            return s.Enabled;
        }

        private static bool IsStatic(IMyTerminalBlock block)
        {
            return block.CubeGrid.IsStatic;
        }

        private static bool HasChanges(IMyTerminalBlock block)
        {
            BeaconStorage s = block.GameLogic.GetAs<RacingBeacon>().Storage;
            return s.HasTemp();
        }

        private static BeaconStorage GetTemporaryStorage(IMyTerminalBlock block)
        {
            BeaconStorage s = block.GameLogic.GetAs<RacingBeacon>().Storage;
            if (s.CreateTemp())
                RefreshUI(block);
            return s.Temporary;
        }

        private static BeaconStorage GetLatestStorage(IMyTerminalBlock block)
        {
            BeaconStorage s = block.GameLogic.GetAs<RacingBeacon>().Storage;
            if (s.HasTemp())
                return s.Temporary;
            return s;
        }

        public static void RefreshUI(IMyTerminalBlock block)
        {
            MyCubeBlock cube = (MyCubeBlock)block;
            if (cube.IDModule == null || RacingConstants.IsDedicated || MyAPIGateway.Gui.GetCurrentScreen != MyTerminalPageEnum.ControlPanel)
                return;

            MyOwnershipShareModeEnum shareMode = cube.IDModule.ShareMode;
            long ownerId = cube.IDModule.Owner;

            cube.ChangeOwner(ownerId, shareMode != MyOwnershipShareModeEnum.All ? MyOwnershipShareModeEnum.All : MyOwnershipShareModeEnum.Faction);
            cube.ChangeOwner(ownerId, shareMode);
        }
    }
}