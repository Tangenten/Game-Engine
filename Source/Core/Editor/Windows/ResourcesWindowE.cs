using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using ImGuiNET;

namespace RavEngine {
	public class ResourcesWindowE : EditorWindow {
		private string currentFolder;
		private string selectedFile;

		internal override void Update() {
			if (!this.open) {
				return;
			}

			ImGui.Begin("Resources", ref this.open, ImGuiWindowFlags.NoCollapse);

			if (Engine.Project.IsProjectLoaded) {
				ImGui.BeginChildFrame(1, new Vector2(-1, -1));

				if (ImGui.IsMouseClicked(ImGuiMouseButton.Right)) {
					try {
						this.currentFolder = Directory.GetParent(this.currentFolder).FullName;
					} catch (Exception e) { }
				}

				List<string> fileSystemEntries = this.GetFileSystemEntries(this.currentFolder);
				foreach (var fileSystemEntry in fileSystemEntries) {
					if (Directory.Exists(fileSystemEntry)) {
						string? name = Path.GetFileName(fileSystemEntry);
						if (ImGui.Selectable(name + "/", false, ImGuiSelectableFlags.DontClosePopups)) {
							this.currentFolder = fileSystemEntry;
						}
					} else {
						string? name = Path.GetFileName(fileSystemEntry);
						bool isSelected = this.selectedFile == fileSystemEntry;
						if (ImGui.Selectable(name, isSelected, ImGuiSelectableFlags.DontClosePopups)) {
							this.selectedFile = fileSystemEntry;
						}

						if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left)) { }
					}
				}

				ImGui.EndChildFrame();
			}

			ImGui.End();
		}

		internal override void Reset() { }

		private List<string> GetFileSystemEntries(string rootPath = "C:\\", string searchPattern = "", bool onlyFolders = false) {
			FileSystemInfo[] filesAndDirectories = new DirectoryInfo(rootPath).GetFileSystemInfos();
			return filesAndDirectories
				   .Where(x => {
					   if (searchPattern != "") {
						   return x.Name.Contains(searchPattern, StringComparison.CurrentCultureIgnoreCase);
					   }
					   if (x.Attributes.HasFlag(FileAttributes.Directory)) {
						   return !x.Attributes.HasFlag(FileAttributes.Hidden) && !onlyFolders;
					   }
					   return !x.Attributes.HasFlag(FileAttributes.Hidden) || !x.Attributes.HasFlag(FileAttributes.System);
				   })
				   .Select(x => x.FullName).ToList();
		}
	}
}