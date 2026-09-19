using Engine;
using Engine.Graphics;
using System;
using System.Collections.Generic;

namespace Game
{
    public class ModernTableSinglePDFT : PaintableDecorFlexType
    {
        public override int Id => 2;

        public Texture2D m_texture;
        public BlockMesh m_topMesh = new();
        public BlockMesh m_legsMesh = new();
        public BlockMesh m_standaloneTopMesh = new();
        public BlockMesh m_standaloneLegsMesh = new();
        public Random m_random = new();

        public override void Initialize()
        {
            base.Initialize();

            RotationType = DFRotationType.None;
            DisplayName = "Modern Table Single";

            DefaultSoundMaterialName = "Stone";

            DigMethod = BlockDigMethod.Quarry;
            DigResilience = 5f;
            ExplosionResilience = 5f;

            IsTransparent = true;

            m_texture = ContentManager.Get<Texture2D>("Textures/DFBlocks");
            Model model = ContentManager.Get<Model>("Models/Blocks/ModernTableSingle");

            Matrix topTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Top").ParentBone);

            m_topMesh.AppendModelMeshPart(model.FindMesh("Top").MeshParts[0], topTransform * Matrix.CreateTranslation(0.5f, 0f, 0.5f), false, false, false, false, Color.White);

            m_standaloneTopMesh.AppendModelMeshPart(model.FindMesh("Top").MeshParts[0], topTransform * Matrix.CreateTranslation(0f, -0.5f, 0f), false, false, false, false, Color.White);

            Matrix legsTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Legs").ParentBone);

            m_legsMesh.AppendModelMeshPart(model.FindMesh("Legs").MeshParts[0], legsTransform * Matrix.CreateTranslation(0.5f, 0f, 0.5f), false, false, false, false, Color.White);

            m_standaloneLegsMesh.AppendModelMeshPart(model.FindMesh("Legs").MeshParts[0], legsTransform * Matrix.CreateTranslation(0f, -0.5f, 0f), false, false, false, false, Color.White);

            BoundingBox topBox = m_topMesh.CalculateBoundingBox();
            BoundingBox legsBox = m_legsMesh.CalculateBoundingBox();
            m_defaultCollisionBoxes = new[]
            {
                BoundingBox.Union(topBox, legsBox)
            };
        }

        public override IEnumerable<int> GetCreativeValues()
        {
            yield return Terrain.MakeBlockValue(BlockIndex, 0, Id);

            for (int i = 0; i < 16; i++)
            {
                int value = Terrain.MakeBlockValue(BlockIndex, 0, Id);
                yield return SetColor(value, i);
            }
        }

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
        {
            Block block = BlocksManager.Blocks[Terrain.ExtractContents(value)];
            TerrainGeometrySubset subset = geometry.GetGeometry(m_texture).SubsetOpaque;

            Color topColor = SubsystemPalette.GetColor(generator, GetColor(value));

            generator.GenerateShadedMeshVertices(block, x, y, z, m_topMesh, topColor, null, null, subset);
            generator.GenerateShadedMeshVertices(block, x, y, z, m_legsMesh, Color.White, null, null, subset);
        }

        public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
        {
            Color topColor = color * SubsystemPalette.GetColor(environmentData, GetColor(value));

            BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneTopMesh, m_texture, topColor, size, ref matrix, environmentData);
            BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneLegsMesh, m_texture, color, size, ref matrix, environmentData);
        }

        public override BlockDebrisParticleSystem CreateDebrisParticleSystem(SubsystemTerrain subsystemTerrain, Vector3 position, int value, float strength)
        {
            Color color = SubsystemPalette.GetColor(subsystemTerrain, GetColor(value));

            return new BlockDebrisParticleSystem(subsystemTerrain, position, strength, DestructionDebrisScale, color, 48, m_texture);
        }
    }
}
