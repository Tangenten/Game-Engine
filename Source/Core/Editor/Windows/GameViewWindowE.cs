using System;
using System.Numerics;
using ImGuiNET;
using RavUtilities;

namespace RavEngine {
	public class GameViewWindowE : EditorWindow {
		public bool WindowFocused { get; set; }
		public Vector2 WindowSize { get; set; }
		public Vector2 WindowPosition { get; set; }

		public GameViewWindowE() { this.WindowFocused = true; }

		internal override void Update() {
			if (!this.open) {
				return;
			}

			ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0f, 0f));
			ImGui.Begin("Game", ref this.open, ImGuiWindowFlags.NoCollapse);

			this.WindowSize = ImGui.GetWindowContentRegionMax() - ImGui.GetWindowContentRegionMin();
			float titleBarHeight = ImGui.GetFontSize() + ImGui.GetStyle().FramePadding.Y * 2f;

			// Ignore TitleBar, Just GameWindow
			this.WindowPosition = new Vector2(ImGui.GetWindowPos().X, ImGui.GetWindowPos().Y + titleBarHeight);
			this.WindowFocused = ImGui.IsWindowHovered() || ImGui.IsWindowFocused();

			//Engine.Graphics.MainTexture.Bind();
			//ImGui.Image((IntPtr) Engine.Graphics.MainTexture.textureId, this.WindowSize, new Vector2(0, 1), new Vector2(1, 0));
			//Engine.Graphics.MainTexture.UnBind();

			ImGui.End();
			ImGui.PopStyleVar();
		}

		internal override void Reset() { }

		public Vector2 GetMousePosition() {
			Vector2 mouseWindowPosition = Engine.Input.GetMouseWindowPosition();

			// Invert Y Axis, And Translate From TopLeft Corner to BottomLeft
			Vector2 windowPosInv = this.WindowPosition;
			windowPosInv.Y = MathF.Abs(Engine.Window.Size.Y - windowPosInv.Y) - this.WindowSize.Y;

			mouseWindowPosition.X = Math.Clamp(mouseWindowPosition.X, windowPosInv.X, windowPosInv.X + this.WindowSize.X);
			mouseWindowPosition.Y = Math.Clamp(mouseWindowPosition.Y, windowPosInv.Y, windowPosInv.Y + this.WindowSize.Y);

			mouseWindowPosition.X = TweenU.Linear(mouseWindowPosition.X, windowPosInv.X, windowPosInv.X + this.WindowSize.X, 0f, 1f);
			mouseWindowPosition.Y = TweenU.Linear(mouseWindowPosition.Y, windowPosInv.Y, windowPosInv.Y + this.WindowSize.Y, 0f, 1f);

			return mouseWindowPosition;
		}
	}
}