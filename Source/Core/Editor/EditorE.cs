using System.Numerics;
using ImGuiNET;
using Silk.NET.OpenGL.Extensions.ImGui;

namespace RavEngine {
	public class EditorE : EngineCore {
		private ImGuiController controller;

		internal MainMenuBarE MainMenu;
		internal ConsoleWindowE Console;
		internal GameViewWindowE Game;

		public EditorE() {
		}

		internal override void Start() {
			this.controller = new ImGuiController(Engine.Window.GLContext, Engine.Window.View, Engine.Window.InputContext);

			ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable | ImGuiConfigFlags.DpiEnableScaleFonts | ImGuiConfigFlags.NavEnableKeyboard | ImGuiConfigFlags.ViewportsEnable;
			ImGui.GetIO().ConfigDockingWithShift = true;
			ImGui.GetIO().ConfigWindowsResizeFromEdges = true;
			ImGui.GetIO().ConfigWindowsMoveFromTitleBarOnly = true;
			ImGui.GetIO().WantSaveIniSettings = true;

			ImGui.GetFont().Scale = 1.5f;

			ImGui.GetStyle().FrameRounding = 0f;
			ImGui.GetStyle().WindowRounding = 0f;
			ImGui.GetStyle().TabRounding = 0f;
			ImGui.GetStyle().ScrollbarRounding = 0f;
			ImGui.GetStyle().FrameBorderSize = 1f;
			ImGui.GetStyle().FramePadding = new Vector2(4f, 4f);
			ImGui.GetStyle().WindowPadding = new Vector2(4f, 4f);
			ImGui.GetStyle().ItemSpacing = new Vector2(4f, 4f);
			ImGui.GetStyle().IndentSpacing = 12;
			ImGui.GetStyle().ScrollbarSize = 12;
			ImGui.GetStyle().GrabMinSize = 12;
			ImGui.GetStyle().TabBorderSize = 1;
			ImGui.GetStyle().DisplaySafeAreaPadding = new Vector2(0f, 0f);
			ImGui.GetStyle().WindowMenuButtonPosition = ImGuiDir.None;
			ImGui.GetStyle().TabMinWidthForUnselectedCloseButton = 0f;
			ImGui.GetStyle().Colors[(int) ImGuiCol.ResizeGrip].W = 0f;
			ImGui.GetStyle().Colors[(int) ImGuiCol.TitleBg] = ImGui.GetStyle().Colors[(int) ImGuiCol.TitleBgActive];

			this.MainMenu = new MainMenuBarE();
			this.Console = new ConsoleWindowE();
			this.Game = new GameViewWindowE();
		}

		internal override void Stop() {
		}

		internal override void Update() {
			this.controller.Update((float) Engine.Time.DeltaRealTime);

			ImGui.DockSpaceOverViewport();

			this.MainMenu.Update();
			this.Game.Update();
			this.Console.Update();

			ImGui.ShowDemoWindow();

			this.controller.Render();
		}

		internal override void Reset() {
			this.MainMenu.Reset();
			this.Console.Reset();
			this.Game.Reset();
		}
	}
}
