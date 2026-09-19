using Engine;
using Engine.Graphics;
using System;

namespace Game
{
    public class DecorFlexBlockDebrisParticleSystem : BlockDebrisParticleSystem
    {
        public int BaseTextureSlotsCount = 16;
        public int TextureSlotScale = 2;

        public DecorFlexBlockDebrisParticleSystem(SubsystemTerrain terrain, Vector3 position, float strength, float scale, Color color, int textureSlot, Texture2D texture, int textureSlotsCount = 16, int textureSlotScale = 2)
          : base(terrain, position, strength, scale, color, textureSlot, texture)
        {
            BaseTextureSlotsCount = MathUtils.Clamp(textureSlotsCount, 1, 1024);
            TextureSlotScale = MathUtils.Clamp(textureSlotScale, 1, 16);
            TextureSlotsCount = BaseTextureSlotsCount * TextureSlotScale;
            SetBlockDebrisParticle(terrain, position, strength, scale, color, textureSlot);
        }

        public new void SetBlockDebrisParticle(SubsystemTerrain terrain, Vector3 position, float strength, float scale, Color color, int textureSlot)
        {
            m_subsystemTerrain = terrain;
            int num = Terrain.ToCell(position.X);
            int num2 = Terrain.ToCell(position.Y);
            int num3 = Terrain.ToCell(position.Z);
            int x = 0;
            x = MathUtils.Max(x, terrain.Terrain.GetCellLight(num + 1, num2, num3));
            x = MathUtils.Max(x, terrain.Terrain.GetCellLight(num - 1, num2, num3));
            x = MathUtils.Max(x, terrain.Terrain.GetCellLight(num, num2 + 1, num3));
            x = MathUtils.Max(x, terrain.Terrain.GetCellLight(num, num2 - 1, num3));
            x = MathUtils.Max(x, terrain.Terrain.GetCellLight(num, num2, num3 + 1));
            x = MathUtils.Max(x, terrain.Terrain.GetCellLight(num, num2, num3 - 1));
            float num4 = LightingManager.LightIntensityByLightValue[x];
            color *= num4;
            color.A = 255;
            float num5 = MathUtils.Sqrt(strength);
            for (int i = 0; i < Particles.Length; i++)
            {
                Particle obj = Particles[i];
                obj.IsActive = true;
                Vector3 vector = new(m_random.Float(-1f, 1f), m_random.Float(-1f, 1f), m_random.Float(-1f, 1f));
                obj.Position = position + strength * 0.45f * vector;
                obj.Color = Color.MultiplyColorOnly(color, m_random.Float(0.7f, 1f));
                obj.Size = num5 * scale * new Vector2(m_random.Float(0.05f, 0.06f));
                obj.TimeToLive = num5 * m_random.Float(1f, 3f);
                obj.Velocity = num5 * 2f * (vector + new Vector3(m_random.Float(-0.2f, 0.2f), 0.6f, m_random.Float(-0.2f, 0.2f)));
                obj.TextureSlot = CalculateDebrisTextureSlot(textureSlot);
            }
        }

        public virtual int CalculateDebrisTextureSlot(int textureSlot)
        {
            int x = textureSlot % BaseTextureSlotsCount;
            int y = textureSlot / BaseTextureSlotsCount;

            int sx =
                x * TextureSlotScale +
                m_random.Int(0, TextureSlotScale - 1);

            int sy =
                y * TextureSlotScale +
                m_random.Int(0, TextureSlotScale - 1);

            return sx + TextureSlotsCount * sy;
        }
    }
}
