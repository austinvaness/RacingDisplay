using System;
using System.Collections.Generic;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;

namespace avaness.RacingMod.Beacon
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Beacon), false)]
    public partial class RacingBeacon : MyGameLogicComponent, IComparable<RacingBeacon>, IEquatable<RacingBeacon>
    {
        public IMyBeacon Beacon { get; private set; } = null;
        public BeaconStorage Storage { get; private set; }
        public bool IsCheckpoint => Storage.Checkpoint;

        private Vector3D gridCenter = new Vector3D();
        private float NodeNumber => Storage.NodeNum;
        private bool registered;
        private bool autoDisabled;
        private bool isStatic;

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            Beacon = Entity as IMyBeacon;

            NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME;

            if (Beacon.Storage == null)
                Beacon.Storage = new MyModStorageComponent();
            Storage = new BeaconStorage(Beacon);
        }

        public override void Close()
        {
            if (isStatic && RacingSession.Instance?.Nodes != null)
                RacingSession.Instance.Nodes.RemoveNode(this);

            if (Storage != null)
            {
                Storage.OnDataReceived -= UpdateRegistration;
                Storage.Unload();
            }

            if (Beacon.CubeGrid != null)
                Unsubscribe(Beacon.CubeGrid);

            NeedsUpdate = MyEntityUpdateEnum.NONE;
        }

        public override void UpdateOnceBeforeFrame()
        {
            if (Beacon.CubeGrid?.Physics == null)
                return; // This beacon is not important.

            if (RacingSession.Instance?.Net == null)
            {
                NeedsUpdate = MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
                return; // Wait until RacingSession has started.
            }

            BeaconControls.CreateControls();

            Storage.Load();

            if (RacingConstants.IsServer)
            {
                Storage.BackwardsCompatibility(Beacon);

                isStatic = Beacon.CubeGrid.IsStatic;
                Subscribe(Beacon.CubeGrid);

                Storage.OnDataReceived += UpdateRegistration;
                UpdateRegistration(true);
            }
            else
            {
                Storage.OnDataReceived += RefreshUI;
            }
        }

        private void RefreshUI()
        {
            BeaconControls.RefreshUI(Beacon);
        }

        private void Subscribe(IMyCubeGrid grid)
        {
            grid.OnMarkForClose += CubeGrid_OnMarkForClose;
            grid.OnIsStaticChanged += CubeGrid_OnIsStaticChanged;
            grid.PositionComp.OnLocalAABBChanged += UpdateGridCenter;
            grid.OnGridSplit += Grid_OnGridSplit;
        }

        private void Unsubscribe(IMyCubeGrid grid)
        {
            grid.OnMarkForClose -= CubeGrid_OnMarkForClose;
            grid.OnIsStaticChanged -= CubeGrid_OnIsStaticChanged;
            grid.PositionComp.OnLocalAABBChanged -= UpdateGridCenter;
            grid.OnGridSplit -= Grid_OnGridSplit;
        }

        public Vector3D GetCoords()
        {
            return Storage.GridPosition ? gridCenter : Beacon.GetPosition();
        }

        public bool Contains (IMyPlayer p)
        {
            IMyEntity e = RacingTools.GetCockpit(p)?.CubeGrid;
            if (e == null)
                e = p.Character;

            if (e == null)
                return false;

            if (Storage.CheckpointSize <= 0)
            {
                MatrixD gridMatrix;
                BoundingBoxD gridAABB;
                GetGridInfo(out gridAABB, out gridMatrix);

                return Contains(e, ref gridAABB, ref gridMatrix);
            }
            else
            {
                BoundingSphereD sphere = new BoundingSphereD(gridCenter, Storage.CheckpointSize);
                
                return Contains(e, ref sphere);
            }

        }

        public void DrawDebug()
        {
            DrawDebug(Color.Red);
        }

        public void DrawDebug(Color outsideColor)
        {
            Color color;
            if (Contains(MyAPIGateway.Session.Player))
                color = Color.Green;
            else
                color = outsideColor;
            color.A = 1;

            MyStringId material = MyStringId.GetOrCompute("Square");

            float radius = Storage.CheckpointSize;
            if(radius <= 0)
            {
                MatrixD gridMatrix;
                BoundingBoxD gridAABB;
                GetGridInfo(out gridAABB, out gridMatrix);
                MySimpleObjectDraw.DrawTransparentBox(ref gridMatrix, ref gridAABB, ref color, MySimpleObjectRasterizer.SolidAndWireframe, 1, 0.01f, material, material, blendType: BlendTypeEnum.PostPP);
            }
            else
            {
                MatrixD matrix = MatrixD.CreateWorld(gridCenter);
                MySimpleObjectDraw.DrawTransparentSphere(ref matrix, radius, ref color, MySimpleObjectRasterizer.Solid, 30, material, material, 0.01f, -1, null, BlendTypeEnum.PostPP);
            }
        }

        private void GetGridInfo(out BoundingBoxD gridAABB, out MatrixD gridMatrix)
        {
            if(InvalidGrid())
            {
                gridAABB = Beacon.LocalAABB;
                gridAABB = gridAABB.Inflate(15);
                gridMatrix = Beacon.WorldMatrix;
            }
            else
            {
                gridAABB = Beacon.CubeGrid.LocalAABB;
                gridMatrix = Beacon.CubeGrid.WorldMatrix;
            }
        }

        private bool Contains(IMyEntity e, ref BoundingSphereD sphere)
        {
            if (e == null)
                return false;

            if (sphere.Intersects(e.WorldVolume))
                return true;

            if (e?.Physics == null)
                return false;

            Vector3D vel = e.Physics.LinearVelocity;
            if (vel == Vector3D.Zero)
                return false;

            Vector3D direction = e.Physics.LinearVelocity / -300;
            double speed = direction.Length();
            direction /= speed;
            double? result = sphere.Intersects(new RayD(e.GetPosition(), direction));
            if (!result.HasValue)
                return false;

            return result.Value > 0 && result.Value < speed;
        }

        private bool Contains(IMyEntity e, ref BoundingBoxD gridAABB, ref MatrixD gridMatrix)
        {
            MatrixD transMatrix = MatrixD.Transpose(gridMatrix);

            if (e == null)
                return false;

            Vector3D gridLocalPos = Vector3D.TransformNormal(e.WorldVolume.Center - gridMatrix.Translation, transMatrix);
            if (gridAABB.Intersects(new BoundingSphereD(gridLocalPos, e.WorldVolume.Radius)))
                return true;

            if (e?.Physics == null)
                return false;

            Vector3D vel = e.Physics.LinearVelocity;
            if (vel == Vector3D.Zero)
                return false;

            Vector3D direction = Vector3D.TransformNormal(e.Physics.LinearVelocity / -300, transMatrix);
            double speed = direction.Length();
            direction /= speed;
            double? result = gridAABB.Intersects(new Ray(e.GetPosition(), direction));
            if (!result.HasValue)
                return false;

            return result.Value > 0 && result.Value < speed;
        }



        public override void UpdateAfterSimulation100()
        {
            if (InvalidGrid())
                return;

            isStatic = Beacon.CubeGrid.IsStatic;
            Subscribe(Beacon.CubeGrid);
            UpdateRegistration(true);

            NeedsUpdate &= ~MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        private bool InvalidGrid()
        {
            return Beacon.CubeGrid == null || Beacon.CubeGrid.MarkedForClose;
        }

        private void Grid_OnGridSplit(IMyCubeGrid grid1, IMyCubeGrid grid2)
        {
            isStatic = false;
            UpdateRegistration(true);
            Unsubscribe(Beacon.CubeGrid);
            if (Entity != null)
                NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        private void CubeGrid_OnIsStaticChanged(IMyCubeGrid grid, bool isStatic)
        {
            this.isStatic = isStatic;
            UpdateRegistration(true);
        }

        private void CubeGrid_OnMarkForClose(IMyEntity e)
        {
            isStatic = false;
            UpdateRegistration(true);
            Unsubscribe(Beacon.CubeGrid);
            if(Entity != null)
                NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
        }

        private void UpdateRegistration()
        {
            UpdateRegistration(false);
        }
        private void UpdateRegistration(bool save)
        {
            if (float.IsNaN(Storage.NodeNum))
                Storage.NodeNum = ComputeNodeNum();

            if (Storage.Enabled)
            {
                if (isStatic)
                {
                    Unregister();
                    Register(save);
                    autoDisabled = false;
                }
                else
                {
                    Disable(save);
                    autoDisabled = true;
                }
            }
            else
            {
                if(autoDisabled)
                {
                    Enable(save);
                    autoDisabled = false;
                }
                else
                {
                    Unregister();
                }
            }
        }

        private float ComputeNodeNum()
        {
            RacingBeacon start;
            RacingBeacon end;
            if(RacingSession.Instance.Nodes.GetNeighbors(Beacon.GetPosition(), out start, out end))
            {
                if(start == null)
                    return end.NodeNumber - 1;
                else if(end == null)
                    return start.NodeNumber + 1;
                else
                    return (start.NodeNumber + end.NodeNumber) / 2;
            }
            return 0;
        }

        private void Register(bool save)
        {
            if(!registered)
            {
                UpdateGridCenter();
                if (!float.IsNaN(NodeNumber) && !float.IsInfinity(NodeNumber) && RacingSession.Instance.Nodes.RegisterNode(this))
                {
                    registered = true;
                }
                else
                {
                    Disable(save);
                    autoDisabled = false;
                }
            }
        }

        private void UpdateGridCenter(MyPositionComponentBase comp)
        {
            gridCenter = comp.WorldAABB.Center;
        }

        private void UpdateGridCenter()
        {
            gridCenter = Beacon.CubeGrid.PositionComp.WorldAABB.Center;
        }

        private void Unregister()
        {
            if(registered)
            {
                RacingSession.Instance.Nodes.RemoveNode(this);
                registered = false;
            }
        }

        private void Disable(bool save)
        {
            if(Storage.Enabled)
            {
                Storage.Enabled = false;
                if(save)
                    Storage.Save();
                RefreshUI();
            }
            Unregister();
        }

        private void Enable(bool save)
        {
            if(!Storage.Enabled)
            {
                Storage.Enabled = true;
                if(save)
                    Storage.Save();
                RefreshUI();
            }
            Register(save);
        }

        public int CompareTo (RacingBeacon other)
        {
            return NodeNumber.CompareTo(other.NodeNumber);
        }

        public bool Equals (RacingBeacon other)
        {
            if (other == null)
                return !isStatic;
            return NodeNumber.Equals(other.NodeNumber);
        }

        public void Insert()
        {
            BeaconStorage s = Storage;
            s.CreateTemp();
            s.Temporary.Enabled = true;
            s.Temporary.NodeNum = float.NaN;
            s.ApplyTemp();
            RefreshUI();
        }
    }
}
