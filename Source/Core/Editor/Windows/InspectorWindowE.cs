using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using ImGuiNET;
using RavContainers;
using RavUtilities;

namespace RavEngine {
	public class InspectorWindowE : EditorWindow {
		internal override void Update() {
			if (!this.open) {
				return;
			}

			ImGui.Begin("Inspector", ref this.open, ImGuiWindowFlags.NoCollapse);

			Node node = Engine.Editor.Hierarchy.selectedNode;
			if (node is not null) {
				string nodeName = nameof(node);
				Type nodeType = node.GetType();
				ImGui.Text(nodeName);

				if (Engine.Project.IsProjectPlaying) {
					if (ImGui.Button("Save")) { }
					if (ImGui.Button("Load")) { }
				}

				IEnumerable<string> nodeTags = node.GetTags();
				foreach (string nodeTag in nodeTags) {
					ImGui.Text(nodeTag);
					ImGui.SameLine();
				}
				ImGui.NewLine();

				FieldInfo[] fieldInfos = nodeType.GetFields(BindingFlags.Public);
				foreach (FieldInfo fieldInfo in fieldInfos) {
					ImGui.Indent();
					ImGui.Separator();

					Type fieldType = fieldInfo.FieldType;
					string fieldName = fieldInfo.Name;

					if (fieldType.IsNumber()) {
						float fieldValue = Convert.ToSingle(fieldInfo.GetValue(node));

						float increment = fieldType.IsInteger() ? 1f : 0.1f;
						(float min, float max) = ReflectionU.NumberClamp(fieldType);

						if (ImGui.DragFloat(fieldName, ref fieldValue, increment, min, max, "", ImGuiSliderFlags.ClampOnInput)) {
							fieldInfo.SetValue(node, Convert.ChangeType(fieldValue, fieldType));
						}
					} else if (fieldType == typeof(bool)) {
						bool fieldValue = (bool) fieldInfo.GetValue(node);

						if (ImGui.Checkbox(fieldName, ref fieldValue)) {
							fieldInfo.SetValue(node, fieldValue);
						}
					} else if (fieldType == typeof(char)) {
						string fieldValue = (string) fieldInfo.GetValue(node);

						if (ImGui.InputText(fieldName, ref fieldValue, 256, ImGuiInputTextFlags.EnterReturnsTrue)) {
							fieldInfo.SetValue(node, fieldValue[0]);
						}
					} else if (fieldType == typeof(string)) {
						string fieldValue = (string) fieldInfo.GetValue(node);

						if (ImGui.InputText(fieldName, ref fieldValue, 256, ImGuiInputTextFlags.EnterReturnsTrue)) {
							fieldInfo.SetValue(node, fieldValue);
						}
					} else if (fieldType.IsEnum) {
						Enum fieldEnum = (Enum) fieldInfo.GetValue(node);
						string fieldEnumSelectedName = fieldEnum.ToString();
						object? fieldEnumSelectedValue = Enum.Parse(fieldEnum.GetType(), fieldEnumSelectedName, true);
						Array fieldEnumValues = Enum.GetValues(fieldEnum.GetType());
						string[] fieldEnumNames = Enum.GetNames(fieldEnum.GetType());

						int selectedBox = Array.IndexOf(fieldEnumNames, fieldEnumSelectedName);
						if (ImGui.Combo(fieldName, ref selectedBox, fieldEnumNames, fieldEnumNames.Length)) {
							fieldInfo.SetValue(node, fieldEnumValues.GetValue(selectedBox));
						}
					} else if (fieldType == typeof(Vector2)) {
						Vector2 fieldValue = (Vector2) fieldInfo.GetValue(node);

						if (ImGui.DragFloat2(fieldName, ref fieldValue)) {
							fieldInfo.SetValue(node, fieldValue);
						}
					} else if (fieldType == typeof(Vector3)) {
						Vector3 fieldValue = (Vector3) fieldInfo.GetValue(node);

						if (ImGui.DragFloat3(fieldName, ref fieldValue)) {
							fieldInfo.SetValue(node, fieldValue);
						}
					} else if (fieldType == typeof(Vector4)) {
						Vector4 fieldValue = (Vector4) fieldInfo.GetValue(node);

						if (ImGui.DragFloat4(fieldName, ref fieldValue)) {
							fieldInfo.SetValue(node, fieldValue);
						}
					} else if (fieldType == typeof(Color4)) {
						Vector4 fieldValue = (Color4) fieldInfo.GetValue(node);

						if (ImGui.ColorEdit4(fieldName, ref fieldValue)) {
							fieldInfo.SetValue(node, (Color4) fieldValue);
						}
					}

					ImGui.Unindent();
				}
			}

			ImGui.End();
		}

		internal override void Reset() { }
	}
}