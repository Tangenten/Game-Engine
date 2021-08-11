using System;
using System.Collections.Generic;
using ImGuiNET;

namespace RavEngine {
	public class HierarchyWindowE : EditorWindow {
		internal Node? selectedNode;

		public HierarchyWindowE() { this.selectedNode = null; }

		internal override void Update() {
			if (!this.open) {
				return;
			}

			ImGui.Begin("Hierarchy", ref this.open, ImGuiWindowFlags.NoCollapse);

			if (ImGui.BeginPopupContextWindow("Hierarchy_Popup")) {
				if (ImGui.BeginMenu("Add Node")) {
					if (ImGui.BeginMenu("Prefabs")) {
						ImGui.EndMenu();
					}

					if (ImGui.BeginMenu("Types")) {
						foreach (Type nodeType in Engine.Project.GetAllNodeTypes()) {
							if (ImGui.MenuItem(nodeType.Name)) {
								Engine.Game.Scene.TopNode.AddChild((Node) Activator.CreateInstance(nodeType));
							}
						}
						ImGui.EndMenu();
					}

					ImGui.EndMenu();
				}
				ImGui.EndPopup();
			}

			this.RecursiveDraw(Engine.Game.Scene.TopNode);

			ImGui.End();
		}

		internal override void Reset() { this.selectedNode = null; }

		private void RecursiveDraw(Node node) {
			if (node.HasChildren()) {
				List<Node>? list = node.GetChildren();
				for (int index = list.Count - 1; index >= 0; index--) {
					Node child = list[index];
					ImGui.TreePush();
					this.DrawLeaf(child);
					this.RecursiveDraw(child);
				}
			}

			ImGui.TreePop();
		}

		private void DrawLeaf(Node node) {
			ImGuiTreeNodeFlags treeNodeFlags = ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.CollapsingHeader;
			if (this.selectedNode == node) {
				treeNodeFlags |= ImGuiTreeNodeFlags.Bullet;
			}

			if (ImGui.TreeNodeEx(node.GetType().Name, treeNodeFlags)) {
				if (ImGui.IsItemClicked(ImGuiMouseButton.Left) || ImGui.IsItemClicked(ImGuiMouseButton.Right)) {
					this.selectedNode = node;
				}

				if (ImGui.BeginPopupContextItem("Hierarchy_Leaf_Popup " + node.GetHashCode())) {
					if (ImGui.MenuItem("Copy Node")) { }
					if (ImGui.MenuItem("Paste Node")) { }
					if (ImGui.MenuItem("Create Node Prefab")) { }
					if (ImGui.MenuItem("Remove Node")) {
						node.Remove();
						if (this.selectedNode == node) {
							this.selectedNode = null;
						}
					}

					if (ImGui.BeginMenu("Add Child Node")) {
						if (ImGui.BeginMenu("Prefabs")) {
							ImGui.EndMenu();
						}

						if (ImGui.BeginMenu("Types")) {
							foreach (Type nodeType in Engine.Project.GetAllNodeTypes()) {
								if (ImGui.MenuItem(nodeType.Name)) {
									node.AddChild((Node) Activator.CreateInstance(nodeType));
								}
							}
							ImGui.EndMenu();
						}

						ImGui.EndMenu();
					}

					ImGui.EndPopup();
				}
			}
		}
	}
}