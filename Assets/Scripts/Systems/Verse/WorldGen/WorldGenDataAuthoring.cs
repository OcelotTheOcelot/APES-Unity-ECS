using Unity.Entities;
using Unity.Collections;
using UnityEngine;

namespace Verse.WorldGen
{
	public class WorldGenDataAuthoring : MonoBehaviour
	{
		public int terrainHeight;
		public int hillsHeight;

		public int waterTestHeight;

		public GameObject soilMatter;
		public GameObject graniteMatter;
		public GameObject waterMatter;
		public GameObject airMatter;
		
		public bool fillAir;

		public class Baker : Baker<WorldGenDataAuthoring>
		{
			public override void Bake(WorldGenDataAuthoring authoring)
			{
				AddComponent(new TerrainGenerationData
					{
						terrainHeight = authoring.terrainHeight,
						hillsHeight = authoring.hillsHeight,

						waterTestHeight = authoring.waterTestHeight,

						soilMatter = GetEntity(authoring.soilMatter),
						graniteMatter = GetEntity(authoring.graniteMatter),
						
						airMatter = GetEntity(authoring.airMatter),
						fillAir = authoring.fillAir,

						waterMatter = GetEntity(authoring.waterMatter)
					}
				);
			}
		}
	}

	public struct TerrainGenerationData : IComponentData
	{
		public int waterTestHeight;

		public int terrainHeight;
		public int hillsHeight;

		public Entity soilMatter;
		public Entity graniteMatter;
		public Entity waterMatter;

		public bool fillAir;
		public Entity airMatter;
	}
}