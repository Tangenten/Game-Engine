using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using ImGuiNET;
using RavUtilities;
using Silk.NET.OpenGL.Extensions.ImGui;

namespace RavEngine {
	public class EditorE : EngineCore {
		private ImGuiController imguiController;
		private string imguiSettingsPath;
		[ConsoleCommand("IMGUI_SCALE")]
		public float ImguiScale {
			get => this.imguiScale;
			set {
				this.imguiScale = value;
				Engine.Settings.Set("IMGUI_SCALE", value);
			}
		}
		private float imguiScale;
		[ConsoleCommand("SHOW_IMGUI_DEMO")]
		private bool imguiShowDemo;

		private Dictionary<Type, EditorWindow> userWindows;

		public MainMenuBarE MainMenu;
		public ConsoleWindowE Console;
		public GameViewWindowE Game;
		public SceneViewWindowE Scene;
		public HierarchyWindowE Hierarchy;
		public InspectorWindowE Inspector;
		public ResourcesWindowE Resources;

		public EditorE() {
			this.MainMenu = new MainMenuBarE();
			this.Console = new ConsoleWindowE();
			this.Game = new GameViewWindowE();
			this.Scene = new SceneViewWindowE();
			this.Hierarchy = new HierarchyWindowE();
			this.Inspector = new InspectorWindowE();
			this.Resources = new ResourcesWindowE();

			this.userWindows = new Dictionary<Type, EditorWindow>();
		}

		internal override void Start() {
			this.imguiController = new ImGuiController(Engine.Window.GLContext, Engine.Window.View, Engine.Window.InputContext);

			ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable | ImGuiConfigFlags.ViewportsEnable | ImGuiConfigFlags.DpiEnableScaleViewports | ImGuiConfigFlags.DpiEnableScaleFonts;
			ImGui.GetIO().ConfigDockingWithShift = true;
			ImGui.GetIO().ConfigWindowsResizeFromEdges = true;
			ImGui.GetIO().ConfigWindowsMoveFromTitleBarOnly = true;
			ImGui.GetIO().ConfigDockingAlwaysTabBar = false;
			ImGui.GetIO().ConfigViewportsNoTaskBarIcon = true;
			ImGui.GetIO().IniSavingRate = 5f;
			ImGui.GetIO().FontAllowUserScaling = true;
			ImGui.GetIO().WantSaveIniSettings = true;

			this.imguiSettingsPath = Engine.Resources.ResourcePathToFilePath("imgui.ini");
			ImGui.LoadIniSettingsFromDisk(this.imguiSettingsPath);

			if (Engine.Settings.TryGet("IMGUI_SCALE", out float scale)) {
				this.imguiScale = scale;
			} else {
				this.ImguiScale = 1.75f;
			}
			ImGui.GetMainViewport().DpiScale = this.ImguiScale;

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
			ImGui.GetStyle().WindowMenuButtonPosition = ImGuiDir.Right;
			ImGui.GetStyle().TabMinWidthForUnselectedCloseButton = 0f;
			ImGui.GetStyle().Colors[(int) ImGuiCol.ResizeGrip].W = 0f;
			ImGui.GetStyle().Colors[(int) ImGuiCol.TitleBg] = ImGui.GetStyle().Colors[(int) ImGuiCol.TitleBgActive];
			ImGui.GetStyle().Colors[(int) ImGuiCol.TabActive] = ImGui.GetStyle().Colors[(int) ImGuiCol.ButtonActive];

			Engine.Project.ScriptReload += this.ScriptsReload;
		}

		internal override void Stop() { }

		internal override void Update() {
			ImGui.GetMainViewport().DpiScale = this.ImguiScale;

			this.imguiController.Update((float) Engine.Time.DeltaRealTime);
			ImGui.DockSpaceOverViewport(ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);

			this.Game.Update();
			this.MainMenu.Update();
			this.Console.Update();
			this.Scene.Update();
			this.Hierarchy.Update();
			this.Inspector.Update();
			this.Resources.Update();

			#if !PUBLISH
			ImGui.SaveIniSettingsToDisk(this.imguiSettingsPath);
			#endif

			foreach (var window in this.userWindows.Values) {
				window.Update();
			}

			if (this.imguiShowDemo) {
				ImGui.ShowDemoWindow();
			}
		}

		internal override void Reset() {
			this.MainMenu.Reset();
			this.Console.Reset();
			this.Game.Reset();
			this.Scene.Reset();
			this.Hierarchy.Reset();
			this.Inspector.Reset();
			this.Resources.Reset();

			foreach (var window in this.userWindows.Values) {
				window.Reset();
			}
		}

		internal void ScriptsReload() {
			this.userWindows.Clear();
			foreach (Type type in Engine.Project.GetAllEditorWindowTypes()) {
				this.userWindows[type] = (EditorWindow) Activator.CreateInstance(type)! ?? throw new InvalidOperationException();
			}
		}

		[Conditional("DEBUG"), Conditional("RELEASE")]
		internal void Render() { this.imguiController.Render(); }
	}

	public abstract class EditorWindow {
		protected bool open;

		public bool Open {
			get => this.open;
			set {
				this.open = value;
				Engine.Settings.Set(this.GetType().NameWithoutNamespace() + "_OPEN", value);
			}
		}

		public EditorWindow() {
			if (Engine.Settings.TryGet(this.GetType().NameWithoutNamespace() + "_OPEN", out bool value)) {
				this.open = value;
			}
			this.Open = true;
		}

		internal abstract void Update();
		internal abstract void Reset();
	}
}