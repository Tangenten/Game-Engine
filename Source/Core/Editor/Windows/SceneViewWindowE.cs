using ImGuiNET;

namespace RavEngine {
	public class SceneViewWindowE : EditorWindow {
		internal override void Update() {
			if (!this.open) {
				return;
			}

			ImGui.Begin("Scene", ref this.open, ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoCollapse);

			ImGui.End();
		}

		internal override void Reset() { }
	}
}