using Unity.Entities;
using UnityEngine;

using static Verse.Matter;

namespace Verse
{
	public class MatterDataAuthoring : MonoBehaviour
	{
		// Must be unique
		[SerializeField]
		private string id = "unknown";
		
		[SerializeField]
		private string group = "unknown group";

		// Visuals

		[SerializeField]
		private string displayName = "unknown matter";

		[SerializeField]
		private Color32[] colors = new Color32[0];

		// Physics

		[SerializeField]
		private State state = State.Solid;

		// KG per cubic meter (it's funny to say "cubic" in a 2D game, but whatever)
		[SerializeField]
		private float density = 1000f;

		// Measured in Celsium
		public float defaultTemperature = 20f;

		public static readonly float AbsoluteZero = -273.15f;

		public class Baker : Baker<MatterDataAuthoring>
		{
			public override void Bake(MatterDataAuthoring authoring)
			{
				AddComponent(new StringId { value = authoring.id });
				AddComponent(new RuntimeId());
				AddComponent(new Group { groupName = authoring.group });
				AddComponent(new DisplayName { value = authoring.displayName });

				AddComponent(new AtomState { value = authoring.state });
				AddComponent(new Creation { temperature = authoring.defaultTemperature });

				AddComponent(new PhysicProperties { density = authoring.density });

				var buffer = AddBuffer<ColorBufferElement>();
				foreach (Color color in authoring.colors)
					buffer.Add(color);

				// MatterLibrary.Add(authoring.id, entity);
			}
		}
	}
}