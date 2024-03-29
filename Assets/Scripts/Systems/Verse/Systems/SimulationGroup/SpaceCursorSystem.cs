using Apes.Input;
using Apes.UI;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;

namespace Verse
{
	public partial class SpaceCursorSystem : SystemBase
	{
		public static Coord Coord
		{
			get;
			private set;
		}

		private InputActions actions;

		protected override void OnCreate()
		{
			base.OnCreate();

			actions = PlayerInput.Actions;
		}

		protected override void OnUpdate()
		{
			Vector2 cursorPos = actions.Global.Position.ReadValue<Vector2>();

			Camera camera = Camera.main;

			Entities.WithAll<Space.Tag>().ForEach(
				(in LocalToWorldTransform spaceTransform) =>
				{
					Coord = Space.WorldToSpaceCoord(spaceTransform, camera.ScreenToWorldPoint(cursorPos));
				}
			).WithoutBurst().Run();
		}
	}
}