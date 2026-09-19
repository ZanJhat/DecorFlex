using Engine;
using Engine.Graphics;
using System;
using System.Collections.Generic;

namespace Game
{
    public abstract class DecorFlexType
    {
        public abstract int Id { get; }
        public int BlockIndex { get; private set; }

        public DFRotationType RotationType = DFRotationType.None;
        public BoundingBox[] m_defaultCollisionBoxes = [new(Vector3.Zero, Vector3.One)];

        public string DisplayName = string.Empty;
        public string Description = string.Empty;
        public string DefaultCategory = "DecorFlex";

        public Vector3 IconBlockOffset = Vector3.Zero;
        public Vector3 IconViewOffset = new(1f);
        public float IconViewScale = 1f;

        public float FirstPersonScale = 0.4f;
        public Vector3 FirstPersonOffset = new Vector3(0.5f, -0.5f, -0.6f);
        public Vector3 FirstPersonRotation = new Vector3(0f, 40f, 0f);

        public float InHandScale = 0.3f;
        public Vector3 InHandOffset = new Vector3(0f, 0.12f, 0f);
        public Vector3 InHandRotation = new Vector3(0f, 0f, 45f);

        public bool IsCollidable = true;
        public bool IsPlaceable = true;
        public bool DefaultIsInteractive;
        public bool IsEditable;
        public bool IsNonDuplicable;
        public bool HasCollisionBehavior;
        public bool IsFluidBlocker = true;
        public bool IsTransparent;

        public int RequiredToolLevel;
        public int MaxStacking = 40;
        public int DefaultTextureSlot;
        public float DestructionDebrisScale = 1f;

        public float FuelHeatLevel;
        public float FuelFireDuration;
        public string DefaultSoundMaterialName;

        public BlockDigMethod DigMethod;
        public float DigResilience = 1f;
        public float ProjectileResilience = 1f;
        public float ExplosionResilience;

        public virtual void Initialize()
        {
            BlockIndex = BlocksManager.GetBlockIndex<DecorFlexBlock>();
        }

        public virtual string GetDisplayName(SubsystemTerrain subsystemTerrain, int value) => DisplayName;

        public virtual string GetCategory(int value) => DefaultCategory;

        public virtual IEnumerable<int> GetCreativeValues()
        {
            yield return Terrain.MakeBlockValue(BlockIndex, 0, Id);
        }

        public virtual bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value) => IsTransparent;

        public virtual void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
        {
        }

        public virtual void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
        {
        }

        public virtual BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
          => GetPlacementValue(subsystemTerrain, componentMiner, value, raycastResult, RotationType);

        public virtual BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult, DFRotationType rotationType)
        {
            BlockPlacementData result = default;
            result.CellFace = raycastResult.CellFace;

            if (rotationType == DFRotationType.None)
            {
                result.Value = value;
                return result;
            }

            Vector3 forward = Matrix.CreateFromQuaternion(componentMiner.ComponentCreature.ComponentCreatureModel.EyeRotation).Forward;
            float angle = MathUtils.Atan2(forward.X, forward.Z);
            int rotation = 0;

            if (rotationType == DFRotationType.FourDirections)
            {
                rotation = ((int)MathUtils.Round(angle / (MathUtils.PI / 2f)) + 4) % 4;
            }
            else if (rotationType == DFRotationType.EightDirections)
            {
                rotation = ((int)MathUtils.Round(angle / (MathUtils.PI / 4f)) + 8) % 8;
            }

            result.Value = DecorFlexBlock.SetRotation(value, rotation);
            return result;
        }

        public virtual BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value) => m_defaultCollisionBoxes;

        public virtual void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris)
        {
            showDebris = DestructionDebrisScale > 0f;
            if (toolLevel < RequiredToolLevel)
                return;

            BlockDropValue item = new BlockDropValue
            {
                Value = DecorFlexBlock.SetRotation(oldValue, 0),
                Count = 1
            };
            dropValues.Add(item);
        }

        public virtual BlockDebrisParticleSystem CreateDebrisParticleSystem(SubsystemTerrain subsystemTerrain, Vector3 position, int value, float strength)
        {
            return null;
        }
    }
}