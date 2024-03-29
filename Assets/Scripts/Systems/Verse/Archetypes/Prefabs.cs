using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public static class Prefabs
{
	public static Entity Chunk { get; private set; }
	public static Entity Region { get; private set; }

	public struct ChunkPrefab : IComponentData { public Entity prefab; }
	public struct RegionPrefab : IComponentData { public Entity prefab; }

	public static void RegisterCorePrefabs(Entity region, Entity chunk)
	{
		Chunk = chunk;
		Region = region;
	}
}
