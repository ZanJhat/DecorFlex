using Engine;
using Engine.Graphics;
using System;
using System.Collections.Generic;
using Game;

namespace Game
{
    public abstract class PaintableDecorFlexType : DecorFlexType, IPaintableBlock
    {
        public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value) => SubsystemPalette.GetName(subsystemTerrain, GetColor(value), DisplayName);

        public override string GetCategory(int value) => IsColored(value) ? "Painted (DF)" : DefaultCategory;

        public int? GetPaintColor(int value) => GetColor(value);

        public static int? GetColor(int value)
        {
            if (IsColored(value))
                return DecorFlexBlock.GetExtraData(value);
            return null;
        }

        public static bool IsColored(int value) => DecorFlexBlock.GetReservedFlag(value);

        public int Paint(SubsystemTerrain terrain, int value, int? color)
        {
            int data = Terrain.ExtractData(value);
            return Terrain.ReplaceData(value, SetColor(data, color));
        }

        public static int SetColor(int value, int? color)
        {
            if (color.HasValue)
            {
                return DecorFlexBlock.SetReservedFlag(DecorFlexBlock.SetExtraData(value, color.Value), true);
            }
            return DecorFlexBlock.SetReservedFlag(DecorFlexBlock.SetExtraData(value, 0), false);
        }
    }
}
