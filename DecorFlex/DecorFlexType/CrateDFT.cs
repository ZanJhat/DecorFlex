using Engine;
using Engine.Graphics;
using System;
using System.Collections.Generic;

namespace Game
{
    public class CrateDFT : DecorFlexType
    {
        public override int Id => 1;

        public Texture2D m_texture;
        public BlockMesh m_blockMesh = new();
        public BlockMesh m_standaloneBlockMesh = new();
        public Random m_random = new();

        public override void Initialize()
        {
            base.Initialize();

            RotationType = DFRotationType.None;

            DisplayName = "Crate";

            FuelHeatLevel = 1f;
            FuelFireDuration = 10f;
            DefaultSoundMaterialName = "Wood";

            DigMethod = BlockDigMethod.Hack;
            DigResilience = 5f;
            ExplosionResilience = 10f;

            m_texture = ContentManager.Get<Texture2D>("Textures/DFBlocks");
            Model model = ContentManager.Get<Model>("Models/Blocks/Crate");

            Matrix absoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Main").ParentBone);
            m_blockMesh.AppendModelMeshPart(model.FindMesh("Main").MeshParts[0], absoluteTransform * Matrix.CreateTranslation(0.5f, 0f, 0.5f), false, false, false, false, Color.White);
            m_standaloneBlockMesh.AppendModelMeshPart(model.FindMesh("Main").MeshParts[0], absoluteTransform * Matrix.CreateTranslation(0.0f, -0.5f, 0.0f), false, false, false, false, Color.White);

            m_defaultCollisionBoxes = new[]
            {
                m_blockMesh.CalculateBoundingBox()
            };
        }

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
        {
            generator.GenerateShadedMeshVertices(BlocksManager.Blocks[Terrain.ExtractContents(value)], x, y, z, m_blockMesh, Color.White, null, null, geometry.GetGeometry(m_texture).SubsetOpaque);
        }

        public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
        {
            BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, m_texture, color, size, ref matrix, environmentData);
        }

        public override BlockDebrisParticleSystem CreateDebrisParticleSystem(SubsystemTerrain subsystemTerrain, Vector3 position, int value, float strength)
        {
            return new BlockDebrisParticleSystem(subsystemTerrain, position, strength, DestructionDebrisScale, Color.White, 4);
        }
    }
}
