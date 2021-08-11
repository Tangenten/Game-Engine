using System;
using System.IO;
using System.Numerics;
using ImGuiNET;

namespace RavEngine {
	public class MainMenuBarE : EditorWindow {
		private bool loadClicked;
		private string loadCurrentFolder;
		private string loadSelectedFile;

		private bool newClicked;
		private string newCurrentFolder;
		private string newSelectedFolder;

		public MainMenuBarE() {
			this.loadClicked = false;
			this.loadCurrentFolder = "C:\\";
			this.loadSelectedFile = "";

			this.newClicked = false;
			this.newCurrentFolder = "C:\\";
			this.newSelectedFolder = "";
		}

		internal override void Update() {
			ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(4, 8));

			if (ImGui.BeginMainMenuBar()) {
				this.DrawProjectMenu();
				this.DrawSceneMenu();
				this.DrawWindowMenu();
				this.DrawPlayEdit();

				if (Engine.Project.IsProjectLoaded) {
					ImGui.SameLine(ImGui.GetWindowWidth() - ImGui.CalcTextSize("Current Project: " + Engine.Project.ProjectName).X - 32);
					ImGui.Text("Current Project: " + Engine.Project.ProjectName);
				} else {
					ImGui.SameLine(ImGui.GetWindowWidth() - ImGui.CalcTextSize("No Project Loaded").X - 32);
					ImGui.Text("No Project Loaded");
				}

				ImGui.EndMainMenuBar();
			}

			ImGui.PopStyleVar();

			this.DrawLoadProject();
			this.DrawNewProject();
		}

		internal override void Reset() { }

		private void DrawWindowMenu() {
			ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(4, 8));
			ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(8, ImGui.GetWindowHeight()));
			ImGui.PushStyleVar(ImGuiStyleVar.ItemInnerSpacing, new Vector2(0f, 0f));

			if (ImGui.BeginMenu("Windows")) {
				ImGui.PopStyleVar(3);

				if (ImGui.MenuItem("Game", "", Engine.Editor.Game.Open)) {
					Engine.Editor.Game.Open = !Engine.Editor.Game.Open;
				}
				if (ImGui.MenuItem("Scene", "", Engine.Editor.Scene.Open)) {
					Engine.Editor.Scene.Open = !Engine.Editor.Scene.Open;
				}
				if (ImGui.MenuItem("Hierarchy", "", Engine.Editor.Hierarchy.Open)) {
					Engine.Editor.Hierarchy.Open = !Engine.Editor.Hierarchy.Open;
				}
				if (ImGui.MenuItem("Inspector", "", Engine.Editor.Inspector.Open)) {
					Engine.Editor.Inspector.Open = !Engine.Editor.Inspector.Open;
				}
				if (ImGui.MenuItem("Resources", "", Engine.Editor.Resources.Open)) {
					Engine.Editor.Resources.Open = !Engine.Editor.Resources.Open;
				}
				if (ImGui.MenuItem("Console", "", Engine.Editor.Console.Open)) {
					Engine.Editor.Console.Open = !Engine.Editor.Console.Open;
				}

				ImGui.EndMenu();
			} else {
				ImGui.PopStyleVar(3);
			}
		}

		private void DrawProjectMenu() {
			ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(4, 8));
			ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(8, ImGui.GetWindowHeight()));
			ImGui.PushStyleVar(ImGuiStyleVar.ItemInnerSpacing, new Vector2(0f, 0f));

			if (ImGui.BeginMenu("Project")) {
				ImGui.PopStyleVar(3);

				if (ImGui.MenuItem("New Project")) {
					this.newClicked = true;
					this.loadClicked = false;
				}

				if (ImGui.MenuItem("Save Project")) {
					Engine.Project.SaveProject();
				}

				if (ImGui.MenuItem("Load Project")) {
					this.loadClicked = true;
					this.newClicked = false;
				}

				if (ImGui.MenuItem("Reload Project")) {
					Engine.Project.ReloadProject();
				}

				if (ImGui.MenuItem("Compile Scripts")) {
					Engine.Project.CompileProjectAsync();
				}

				ImGui.EndMenu();
			} else {
				ImGui.PopStyleVar(3);
			}
		}

		private void DrawSceneMenu() {
			ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(4, 8));
			ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(8, ImGui.GetWindowHeight()));
			ImGui.PushStyleVar(ImGuiStyleVar.ItemInnerSpacing, new Vector2(0f, 0f));

			if (ImGui.BeginMenu("Scene")) {
				ImGui.PopStyleVar(3);

				if (ImGui.MenuItem("New Scene")) {
					Engine.Project.AddScene("New Scene");
				}

				if (ImGui.BeginMenu("Delete Scene")) {
					string[] scenes = Engine.Project.GetAllSceneNames();

					for (int i = 0; i < scenes.Length; i++) {
						if (ImGui.MenuItem(scenes[i])) {
							Engine.Project.RemoveScene(scenes[i]);
						}
					}

					ImGui.EndMenu();
				}

				if (ImGui.BeginMenu("Load Scene")) {
					string[] scenes = Engine.Project.GetAllSceneNames();

					for (int i = 0; i < scenes.Length; i++) {
						if (ImGui.MenuItem(scenes[i])) {
							Engine.Project.LoadScene(scenes[i]);
						}
					}

					ImGui.EndMenu();
				}

				if (ImGui.MenuItem("Reload Scene")) {
					Engine.Project.ReloadScene();
				}

				ImGui.EndMenu();
			} else {
				ImGui.PopStyleVar(3);
			}
		}

		private void DrawPlayEdit() {
			float center = ImGui.GetWindowWidth() / 2f;
			float buttonWidth = 64;
			float buttonHeight = ImGui.GetWindowHeight();
			ImGui.SameLine(center - buttonWidth);

			if (Engine.Project.IsProjectPlaying) {
				ImGui.PushStyleColor(ImGuiCol.Button, ImGui.GetStyle().Colors[(int) ImGuiCol.Button]);
			} else {
				ImGui.PushStyleColor(ImGuiCol.Button, ImGui.GetStyle().Colors[(int) ImGuiCol.MenuBarBg]);
			}

			if (ImGui.Button("Play", new Vector2(buttonWidth, buttonHeight))) {
				Engine.Project.PlayProject();
			}

			ImGui.PopStyleColor();

			if (Engine.Project.IsProjectEditing) {
				ImGui.PushStyleColor(ImGuiCol.Button, ImGui.GetStyle().Colors[(int) ImGuiCol.Button]);
			} else {
				ImGui.PushStyleColor(ImGuiCol.Button, ImGui.GetStyle().Colors[(int) ImGuiCol.MenuBarBg]);
			}

			if (ImGui.Button("Edit", new Vector2(buttonWidth, buttonHeight))) {
				Engine.Project.EditProject();
			}

			ImGui.PopStyleColor();
		}

		private void DrawLoadProject() {
			if (this.loadClicked) {
				ImGui.OpenPopup("Load Project");
			}

			Vector2 center = ImGui.GetMainViewport().GetCenter();
			ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));

			bool loadIsOpen = true;
			if (ImGui.BeginPopupModal("Load Project", ref loadIsOpen)) {
				if (ImGui.Button("Computer")) {
					this.loadCurrentFolder = "C:\\";
				}
				ImGui.SameLine();

				if (ImGui.Button("User")) {
					this.loadCurrentFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
				}
				ImGui.SameLine();

				if (ImGui.Button("Documents")) {
					this.loadCurrentFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				}
				ImGui.SameLine();

				if (ImGui.Button("Downloads")) {
					this.loadCurrentFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
				}
				ImGui.SameLine();

				if (ImGui.Button("Desktop")) {
					this.loadCurrentFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
				}

				if (FilePickerE.Draw(ref this.loadCurrentFolder, ref this.loadSelectedFile)) {
					Engine.Project.LoadProject(this.loadCurrentFolder);
					this.loadClicked = false;
				}

				ImGui.EndPopup();
			} else {
				this.loadClicked = false;
			}
		}

		private void DrawNewProject() {
			if (this.newClicked) {
				ImGui.OpenPopup("New Project");
			}

			Vector2 center = ImGui.GetMainViewport().GetCenter();
			ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));

			bool newIsOpen = true;
			if (ImGui.BeginPopupModal("New Project", ref newIsOpen)) {
				if (ImGui.Button("Computer")) {
					this.newCurrentFolder = "C:\\";
				}
				ImGui.SameLine();

				if (ImGui.Button("User")) {
					this.newCurrentFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
				}
				ImGui.SameLine();

				if (ImGui.Button("Documents")) {
					this.newCurrentFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				}
				ImGui.SameLine();

				if (ImGui.Button("Downloads")) {
					this.newCurrentFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads";
				}
				ImGui.SameLine();

				if (ImGui.Button("Desktop")) {
					this.newCurrentFolder = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
				}

				float footer = ImGui.GetStyle().ItemSpacing.Y + ImGui.GetFrameHeightWithSpacing();
				FilePickerE.Draw(ref this.newCurrentFolder, ref this.newSelectedFolder, footer);

				ImGui.PushItemWidth(-1);
				string newProjectName = "";
				if (ImGui.InputText("Project Name: ", ref newProjectName, 128, ImGuiInputTextFlags.EnterReturnsTrue)) {
					if (newProjectName != "") {
						if (newProjectName.StartsWith("\\") || newProjectName.StartsWith("//")) {
							Directory.CreateDirectory(Path.Combine(this.newCurrentFolder, newProjectName));
							Engine.Project.NewProject(Path.Combine(this.newCurrentFolder, newProjectName), newProjectName);
						} else {
							Engine.Project.NewProject(this.newCurrentFolder, newProjectName);
						}

						this.newClicked = false;
						ImGui.CloseCurrentPopup();
					}
				}
				ImGui.PopItemWidth();

				ImGui.EndPopup();
			} else {
				this.newClicked = false;
			}
		}
	}
}