using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

namespace Verse
{
	[UpdateInGroup(typeof(VerseTickSystemGroup), OrderLast = true)]
	public partial class GizmoSystem : SystemBase
	{
		private float tickDuration;

		EntityQuery regionQuery;
		EntityQuery chunkQuery;

		protected override void OnCreate()
		{
			base.OnCreate();

			RequireForUpdate<GizmoSettings>();

			regionQuery = GetEntityQuery(
				ComponentType.ReadOnly<Region.SpatialIndex>(),
				ComponentType.ReadOnly<LocalToWorldTransform>()
			);

			chunkQuery = GetEntityQuery(
				ComponentType.ReadOnly<Chunk.DirtyArea>()
			);
		}

		protected override void OnStartRunning()
		{
			base.OnStartRunning();

			RequireForUpdate<Space.Tag>();

			tickDuration = 1f / GetSingleton<TickerSettings>().ticksPerSecond;
		}

		protected override void OnUpdate()
		{
			GizmoSettings settings = GetSingleton<GizmoSettings>();

			if (settings.showDirtyAreas)
			{
				new DrawDirtyAreaGizmosJob
				{
					dirtyAreaColor = new Color(1f, 0f, 0f, .25f),
					duration = tickDuration
				}.Run(chunkQuery);
			}

			if (settings.showRegionBorders || settings.showChunkBorders)
			{
				new DrawRegionGizmosJob
				{
					showChunkBorders = settings.showChunkBorders,

					chunkBorderColor = new Color(.8f, .5f, 0f, .25f),
					regionBorderColor = new Color(1f, 1f, 0f, .25f),

					duration = tickDuration,
				}.Run(regionQuery);
			}

			if (settings.showNullNeighbours)
			{
				new DrawNeighbourhoodGizmosJob
				{
					neighbourColor = new Color(.8f, 0f, 0f, 1f),
					duration = tickDuration
				}.Run(chunkQuery);
			}
		}

		public partial struct DrawRegionGizmosJob : IJobEntity
		{
			[ReadOnly]
			public bool showChunkBorders;

			[ReadOnly]
			public float duration;

			[ReadOnly]
			public Color chunkBorderColor;

			[ReadOnly]
			public Color regionBorderColor;
			
			public void Execute(in LocalToWorldTransform transform)
			{
				int regionSize = Space.regionSize, chunkSize = Space.chunkSize;
				float metersPerCell = Space.metersPerCell;

				Vector3 origin = transform.Value.Position;
				Vector3 size = new (regionSize * metersPerCell, regionSize * metersPerCell);

				Vector3 rightBorderShift = size.x * Vector3.right;
				Vector3 upperBorderShift = size.y * Vector3.up;

				if (showChunkBorders)
				{
					for (int y = chunkSize; y < regionSize; y += chunkSize)
					{
						Vector3 line = origin + y * metersPerCell * Vector3.up;
						Debug.DrawLine(line, line + rightBorderShift, chunkBorderColor, duration: duration);
					}

					for (int x = chunkSize; x < regionSize; x += chunkSize)
					{
						Vector3 line = origin + x * metersPerCell * Vector3.right;
						Debug.DrawLine(line, line + upperBorderShift, chunkBorderColor, duration: duration);
					}
				}

				Debug.DrawLine(origin, origin + rightBorderShift, regionBorderColor, duration: duration);
				Debug.DrawLine(origin, origin + upperBorderShift, regionBorderColor, duration: duration);
				Debug.DrawLine(origin + size, origin + rightBorderShift, regionBorderColor, duration: duration);
				Debug.DrawLine(origin + size, origin + upperBorderShift, regionBorderColor, duration: duration);
			}
		}

		public partial struct DrawDirtyAreaGizmosJob : IJobEntity
		{
			[ReadOnly]
			public Color dirtyAreaColor;

			[ReadOnly]
			public float duration;

			public void Execute(in Chunk.SpatialIndex index, in Chunk.DirtyArea area)
			{
				if (!area.active)
					return;

				float metersPerCell = Space.metersPerCell;
				
				float2 margin = new(.25f);
				Vector2 size = area.Size + Coord.one - margin * 2;
				size *= metersPerCell;

				Vector2 cornerA = (index.origin + area.From + margin) * metersPerCell;
				Vector2 cornerB = cornerA + new Vector2(size.x, 0);
				Vector2 cornerC = cornerA + new Vector2(0, size.y);
				Vector2 cornerD = cornerA + size;

				Debug.DrawLine(cornerA, cornerB, dirtyAreaColor, duration: duration);
				Debug.DrawLine(cornerA, cornerC, dirtyAreaColor, duration: duration);
				Debug.DrawLine(cornerD, cornerB, dirtyAreaColor, duration: duration);
				Debug.DrawLine(cornerD, cornerC, dirtyAreaColor, duration: duration);

				Debug.DrawLine(cornerA, cornerC, dirtyAreaColor, duration: duration);
				Debug.DrawLine(cornerC, cornerA, dirtyAreaColor, duration: duration);
			}
		}

		public partial struct DrawBatchIndexJob : IJobEntity
		{
			[ReadOnly]
			public Color dirtyAreaColor;

			public void Execute(in Chunk.SpatialIndex index, ref Chunk.ProcessingBatchIndex batchIndex)
			{
				UnityEditor.Handles.Label((Vector2)(index.origin + new Coord(Space.chunkSize / 2, Space.chunkSize / 2)) * Space.metersPerCell, $"{batchIndex}");
			}
		}

		public partial struct DrawNeighbourhoodGizmosJob : IJobEntity
		{
			[ReadOnly]
			public float duration;

			[ReadOnly]
			public Color neighbourColor;

			public void Execute(in Chunk.SpatialIndex index, in Chunk.Neighbourhood neighbourhood)
			{
				int chunkSize = Space.chunkSize;
				float metersPerCell = Space.metersPerCell;

				Vector3 center = (Vector2)(index.origin + new Coord(Space.chunkSize / 2)) * metersPerCell;

				float size = 4 * metersPerCell;
				const float margin = 1;
				float offset = (chunkSize / 2 - margin) * metersPerCell;

				Vector3 cornerA = new(offset, offset);
				Vector3 cornerALeft = new(offset - size, offset);
				Vector3 cornerADown = new(offset, offset - size);

				Vector3 cornerB = new(-offset, offset);
				Vector3 cornerBRight = new(-offset + size, offset);
				Vector3 cornerBDown = new(-offset, offset - size);

				Vector3 hSideDown = new(offset, -size);
				Vector3 hSideUp = new(offset, size);
				Vector3 vSideLeft = new(-size, offset);
				Vector3 vSideRight = new(size, offset);

				if (neighbourhood.East == Entity.Null)
					Debug.DrawLine(center + hSideDown, center + hSideUp, neighbourColor, duration);

				if (neighbourhood.NorthEast == Entity.Null)
				{
					Debug.DrawLine(center + cornerA, center + cornerALeft, neighbourColor, duration);
					Debug.DrawLine(center + cornerA, center + cornerADown, neighbourColor, duration);
				}

				if (neighbourhood.North == Entity.Null)
					Debug.DrawLine(center + vSideLeft, center + vSideRight, neighbourColor, duration);

				if (neighbourhood.NorthWest == Entity.Null)
				{
					Debug.DrawLine(center + cornerB, center + cornerBRight, neighbourColor, duration);
					Debug.DrawLine(center + cornerB, center + cornerBDown, neighbourColor, duration);
				}

				if (neighbourhood.West == Entity.Null)
					Debug.DrawLine(center - hSideDown, center - hSideUp, neighbourColor, duration);

				if (neighbourhood.SouthWest == Entity.Null)
				{
					Debug.DrawLine(center - cornerA, center - cornerALeft, neighbourColor, duration);
					Debug.DrawLine(center - cornerA, center - cornerADown, neighbourColor, duration);
				}

				if (neighbourhood.South == Entity.Null)
					Debug.DrawLine(center - vSideLeft, center - vSideRight, neighbourColor, duration);

				if (neighbourhood.SouthEast == Entity.Null)
				{
					Debug.DrawLine(center - cornerB, center - cornerBRight, neighbourColor, duration);
					Debug.DrawLine(center - cornerB, center - cornerBDown, neighbourColor, duration);
				}
			}
		}
	}
}
