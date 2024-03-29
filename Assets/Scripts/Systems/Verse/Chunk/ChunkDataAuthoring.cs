using UnityEngine;
using Unity.Entities;

using static Verse.Chunk;

namespace Verse
{
	public class ChunkDataAuthoring : MonoBehaviour
	{
		public class Baker : Baker<ChunkDataAuthoring>
		{
			public override void Bake(ChunkDataAuthoring authoring)
			{
				AddComponent(new Chunk.Region());
				AddComponent(new RegionalIndex());
				AddComponent(new SpatialIndex());

				AddComponent(new Neighbourhood());
				AddBuffer<AtomBufferElement>();

				AddSharedComponentManaged(new ProcessingBatchIndex());
				AddComponent(new DirtyArea());
				AddComponent(new ScheduledDirtyRect());

				AddComponent(new ColliderStatus());
			}
		}
	}
}