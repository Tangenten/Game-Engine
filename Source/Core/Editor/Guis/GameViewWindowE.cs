using System.Numerics;

namespace RavEngine {
	public class GameViewWindowE {
		internal void Update() { }
		internal void Reset() { }

		public bool WindowFocused { get; set; }
		public bool Active { get; set; }

		public Vector2 GetMousePosition() {
			throw new System.NotImplementedException();
		}
	}
}
