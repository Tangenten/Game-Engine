using ImGuiNET;

namespace RavEngine {
	public class ServersWindowE : EditorWindow {
		internal override void Update() {
			if (!this.open) {
				return;
			}

			ImGui.Begin("Servers", ref this.open, ImGuiWindowFlags.NoCollapse);

			ImGui.End();
		}

		internal override void Reset() { }
	}
}