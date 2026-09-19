using Engine;
using Engine.Graphics;
using System;
using System.Collections.Generic;

namespace Game
{
    public enum DFRotationType
    {
        None,
        FourDirections,
        EightDirections
    }

    public class DecorFlexBlock : Block
    {
        public static int Index = 1023;

        public override void Initialize()
        {
            base.Initialize();
            DecorFlexTypesManager.AutoRegister();
            foreach (DecorFlexType dfType in DecorFlexTypesManager.DecorFlexTypes.Values)
                dfType.Initialize();
        }

        // 0-9
        public static int GetId(int value) => Terrain.ExtractData(value) & 0x3FF;

        public static int SetId(int value, int type)
        {
            int data = Terrain.ExtractData(value);

            data &= ~0x3FF;
            data |= Math.Clamp(type, 0, 1023);

            return Terrain.ReplaceData(value, data);
        }

        // 10-13
        public static int GetExtraData(int value) => (Terrain.ExtractData(value) >> 10) & 0xF;

        public static int SetExtraData(int value, int extraData)
        {
            int data = Terrain.ExtractData(value);

            data &= ~(0xF << 10);
            data |= (Math.Clamp(extraData, 0, 15) << 10);

            return Terrain.ReplaceData(value, data);
        }

        // 14-16
        public static int GetRotation(int value) => (Terrain.ExtractData(value) >> 14) & 0x7;

        public static int SetRotation(int value, int rotation)
        {
            int data = Terrain.ExtractData(value);

            data &= ~(0x7 << 14);
            data |= (Math.Clamp(rotation, 0, 7) << 14);

            return Terrain.ReplaceData(value, data);
        }

        // 17
        public static bool GetReservedFlag(int value) => ((Terrain.ExtractData(value) >> 17) & 1) != 0;

        public static int SetReservedFlag(int value, bool flag)
        {
            int data = Terrain.ExtractData(value);

            if (flag)
                data |= (1 << 17);
            else
                data &= ~(1 << 17);

            return Terrain.ReplaceData(value, data);
        }

        // Helper nhanh

        public static DecorFlexType GetDecorFlexType(int value) => DecorFlexTypesManager.GetDecorFlexType(GetId(value));

        // Thuộc tính

        public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value) => GetDecorFlexType(value)?.GetDisplayName(subsystemTerrain, value) ?? DefaultDisplayName;
        public override string GetDescription(int value) => GetDecorFlexType(value)?.Description ?? DefaultDescription;
        public override string GetCategory(int value) => GetDecorFlexType(value)?.GetCategory(value) ?? DefaultCategory;

        public override Vector3 GetIconBlockOffset(int value, DrawBlockEnvironmentData environmentData) => GetDecorFlexType(value)?.IconBlockOffset ?? DefaultIconBlockOffset;
        public override Vector3 GetIconViewOffset(int value, DrawBlockEnvironmentData environmentData) => GetDecorFlexType(value)?.IconViewOffset ?? DefaultIconViewOffset;
        public override float GetIconViewScale(int value, DrawBlockEnvironmentData environmentData) => GetDecorFlexType(value)?.IconViewScale ?? DefaultIconViewScale;

        public override float GetFirstPersonScale(int value) => GetDecorFlexType(value)?.FirstPersonScale ?? FirstPersonScale;
        public override Vector3 GetFirstPersonOffset(int value) => GetDecorFlexType(value)?.FirstPersonOffset ?? FirstPersonOffset;
        public override Vector3 GetFirstPersonRotation(int value) => GetDecorFlexType(value)?.FirstPersonRotation ?? FirstPersonRotation;

        public override float GetInHandScale(int value) => GetDecorFlexType(value)?.InHandScale ?? InHandScale;
        public override Vector3 GetInHandOffset(int value) => GetDecorFlexType(value)?.InHandOffset ?? InHandOffset;
        public override Vector3 GetInHandRotation(int value) => GetDecorFlexType(value)?.InHandRotation ?? InHandRotation;

        public override bool IsCollidable_(int value) => GetDecorFlexType(value)?.IsCollidable ?? IsCollidable;
        public override bool IsPlaceable_(int value) => GetDecorFlexType(value)?.IsPlaceable ?? IsPlaceable;
        public override bool IsInteractive(SubsystemTerrain subsystemTerrain, int value) => GetDecorFlexType(value)?.DefaultIsInteractive ?? DefaultIsInteractive;
        public override bool IsEditable_(int value) => GetDecorFlexType(value)?.IsEditable ?? IsEditable;
        public override bool IsNonDuplicable_(int value) => GetDecorFlexType(value)?.IsNonDuplicable ?? IsNonDuplicable;
        public override bool HasCollisionBehavior_(int value) => GetDecorFlexType(value)?.HasCollisionBehavior ?? HasCollisionBehavior;
        public override bool IsFluidBlocker_(int value) => GetDecorFlexType(value)?.IsFluidBlocker ?? IsFluidBlocker;
        public override bool IsTransparent_(int value) => GetDecorFlexType(value)?.IsTransparent ?? IsTransparent;

        public override float GetRequiredToolLevel(int value) => GetDecorFlexType(value)?.RequiredToolLevel ?? RequiredToolLevel;
        public override int GetMaxStacking(int value) => GetDecorFlexType(value)?.MaxStacking ?? MaxStacking;
        public override int GetFaceTextureSlot(int face, int value) => GetDecorFlexType(value)?.DefaultTextureSlot ?? DefaultTextureSlot;

        public override float GetFuelHeatLevel(int value) => GetDecorFlexType(value)?.FuelHeatLevel ?? FuelHeatLevel;
        public override float GetFuelFireDuration(int value) => GetDecorFlexType(value)?.FuelFireDuration ?? FuelFireDuration;
        public override string GetSoundMaterialName(SubsystemTerrain subsystemTerrain, int value) => GetDecorFlexType(value)?.DefaultSoundMaterialName ?? DefaultSoundMaterialName;

        public override BlockDigMethod GetBlockDigMethod(int value) => GetDecorFlexType(value)?.DigMethod ?? DigMethod;

        public override float GetDigResilience(int value) => GetDecorFlexType(value)?.DigResilience ?? DigResilience;
        public override float GetProjectileResilience(int value) => GetDecorFlexType(value)?.ProjectileResilience ?? ProjectileResilience;
        public override float GetExplosionResilience(int value) => GetDecorFlexType(value)?.ExplosionResilience ?? ExplosionResilience;

        // Các phương thức base

        public override IEnumerable<int> GetCreativeValues()
        {
            foreach (DecorFlexType type in DecorFlexTypesManager.DecorFlexTypes.Values)
            {
                foreach (int value in type.GetCreativeValues())
                    yield return value;
            }
        }

        public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value)
        {
            return GetDecorFlexType(value)?.IsFaceTransparent(subsystemTerrain, face, value) ?? base.IsFaceTransparent(subsystemTerrain, face, value);
        }

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
        {
            GetDecorFlexType(value)?.GenerateTerrainVertices(generator, geometry, value, x, y, z);
        }

        public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
        {
            GetDecorFlexType(value)?.DrawBlock(primitivesRenderer, value, color, size, ref matrix, environmentData);
        }

        public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
        {
            return GetDecorFlexType(value)?.GetPlacementValue(subsystemTerrain, componentMiner, value, raycastResult) ?? base.GetPlacementValue(subsystemTerrain, componentMiner, value, raycastResult);
        }

        public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value)
          => GetDecorFlexType(value)?.GetCustomCollisionBoxes(terrain, value) ?? base.GetCustomCollisionBoxes(terrain, value);

        public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris)
        {
            DecorFlexType type = GetDecorFlexType(oldValue);
            if (type != null)
                type.GetDropValues(subsystemTerrain, oldValue, newValue, toolLevel, dropValues, out showDebris);
            else
                base.GetDropValues(subsystemTerrain, oldValue, newValue, toolLevel, dropValues, out showDebris);
        }

        public override BlockDebrisParticleSystem CreateDebrisParticleSystem(SubsystemTerrain subsystemTerrain, Vector3 position, int value, float strength)
        {
            return GetDecorFlexType(value)?.CreateDebrisParticleSystem(subsystemTerrain, position, value, strength) ?? new BlockDebrisParticleSystem(subsystemTerrain, position, strength, DestructionDebrisScale, Color.White, GetFaceTextureSlot(4, value));
        }
    }
}
