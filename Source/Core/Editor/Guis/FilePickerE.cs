using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using ImGuiNET;

namespace RavEngine {
	public class FilePickerE {
		public static bool Draw(ref string currentFolder, ref string selectedFile, float footerHeight = 0) {
			bool fileOpened = false;

			// ==== Paths ====
			string[]? individualDirectories = currentFolder.Split("\\");
			if (individualDirectories[^1] == "") {
				Array.Resize(ref individualDirectories, individualDirectories.Length - 1);
			}
			for (int index = 0; index < individualDirectories.Length; index++) {
				string directory = individualDirectories[index];
				if (ImGui.Button(directory)) {
					currentFolder = Path.Combine(individualDirectories[..(index + 1)]) + "\\";
				}
				ImGui.SameLine();
			}
			ImGui.NewLine();

			// ==== Search ====
			string search = "";
			ImGui.PushItemWidth(-1);
			if (ImGui.InputText("Search: ", ref search, 256)) { }
			ImGui.PopItemWidth();

			// ==== Picker ====
			if (ImGui.BeginChildFrame(1, new Vector2(0, -footerHeight))) {
				if (ImGui.IsMouseClicked(ImGuiMouseButton.Right)) {
					try {
						currentFolder = Directory.GetParent(currentFolder).FullName;
					} catch (Exception e) { }
				}

				List<string> fileSystemEntries = GetFileSystemEntries(currentFolder, search);
				foreach (var fileSystemEntry in fileSystemEntries) {
					if (Directory.Exists(fileSystemEntry)) {
						string? name = Path.GetFileName(fileSystemEntry);
						if (ImGui.Selectable(name + "/", false, ImGuiSelectableFlags.DontClosePopups)) {
							currentFolder = fileSystemEntry;
						}
					} else {
						string? name = Path.GetFileName(fileSystemEntry);
						bool isSelected = selectedFile == fileSystemEntry;
						if (ImGui.Selectable(name, isSelected, ImGuiSelectableFlags.DontClosePopups)) {
							selectedFile = fileSystemEntry;
						}

						if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left)) {
							fileOpened = true;
							ImGui.CloseCurrentPopup();
						}
					}
				}
			}
			ImGui.EndChildFrame();

			return fileOpened;
		}

		private static List<string> GetFileSystemEntries(string rootPath = "C:\\", string searchPattern = "", bool onlyFolders = false) {
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