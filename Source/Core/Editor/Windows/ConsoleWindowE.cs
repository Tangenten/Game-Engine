using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using CSScriptLib;
using ImGuiNET;
using RavContainers;
using RavUtilities;

namespace RavEngine {
	public class ConsoleWindowE : EditorWindow {
		private bool scrollToBottom;
		private string[] currentConsoleCommand;
		private List<ConsoleEntry> consoleEntries;

		public ConsoleWindowE() { this.consoleEntries = new List<ConsoleEntry>(256); }

		internal override unsafe void Update() {
			if (!this.open) {
				return;
			}

			ImGui.Begin("Console", ref this.open, ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoNavInputs);

			float footer = ImGui.GetStyle().ItemSpacing.Y + ImGui.GetFrameHeightWithSpacing();
			ImGui.BeginChild("Scrolling Region", new Vector2(0, -footer), true, ImGuiWindowFlags.AlwaysVerticalScrollbar | ImGuiWindowFlags.NoNavInputs);

			foreach (ConsoleEntry entry in this.consoleEntries) {
				foreach ((string text, Color4 color) segment in entry.segments) {
					ImGui.TextColored(segment.color, segment.text);
					ImGui.SameLine();
				}
				ImGui.NewLine();
				ImGui.Separator();
			}

			if (this.scrollToBottom) {
				ImGui.SetScrollHereY(0f);
				this.scrollToBottom = false;
			}

			ImGui.EndChild();

			ImGui.BeginChild("Input", new Vector2(0, 0), false, ImGuiWindowFlags.NoScrollbar);

			ImGui.PushItemWidth(ImGui.GetWindowWidth());
			string input = "";
			if (ImGui.InputText("", ref input, 256, ImGuiInputTextFlags.EnterReturnsTrue, this.Callback, IntPtr.Zero)) {
				this.scrollToBottom = true;
				this.currentConsoleCommand = input.Split(" ");
				this.consoleEntries.Add(ConsoleEntry.White(input));
				this.RunAttributeConsoleCommand();

				ImGui.SetKeyboardFocusHere(-1);
			}

			ImGui.PopItemWidth();

			ImGui.EndChild();
			ImGui.End();
		}

		private unsafe int Callback(ImGuiInputTextCallbackData* data) {
			switch (data->EventKey) {
				case ImGuiKey.Tab:         break;
				case ImGuiKey.UpArrow:     break;
				case ImGuiKey.DownArrow:   break;
				case ImGuiKey.Delete:      break;
				case ImGuiKey.Escape:      break;
				case ImGuiKey.Enter:       break;
				case ImGuiKey.KeyPadEnter: break;
				default:                   throw new ArgumentOutOfRangeException();
			}

			switch (data->EventFlag) {
				case ImGuiInputTextFlags.CallbackCompletion: break;
				case ImGuiInputTextFlags.CallbackHistory:    break;
				default:                                     throw new ArgumentOutOfRangeException();
			}

			return 0;
		}

		internal override void Reset() { }

		[Conditional("DEBUG")] [MethodImpl(MethodImplOptions.Synchronized)]
		public void WriteLine(ConsoleEntry consoleEntry) { this.consoleEntries.Add(consoleEntry); }

		[ConsoleCommand("LIST_COMMANDS")]
		internal void ListCommands() {
			List<string> uniqueCommands = new List<string>();

			foreach (FieldInfo fieldInfo in typeof(Engine).GetFields()) {
				object engineMember = fieldInfo.GetValue(null);
				List<(object, MemberInfo)> parentsAndInfos = ReflectionU.GetAllObjectsThatImplementCustomAttribute<ConsoleCommandAttribute>(ref engineMember);

				for (int i = 0; i < parentsAndInfos.Count; i++) {
					object parentObject = parentsAndInfos[i].Item1;
					MemberInfo parentMember = parentsAndInfos[i].Item2;
					ConsoleCommandAttribute memberAttribute = parentMember.GetCustomAttribute<ConsoleCommandAttribute>();

					if (!uniqueCommands.Contains(memberAttribute.command)) {
						uniqueCommands.Add(memberAttribute.command + " : " + parentMember);
					}
				}
			}

			for (int i = 0; i < uniqueCommands.Count; i++) {
				Console.WriteLine(uniqueCommands[i]);
				Engine.Editor.Console.WriteLine(ConsoleEntry.Debug(uniqueCommands[i]));
			}
		}

		internal void RunAttributeConsoleCommand() {
			foreach (FieldInfo fieldInfo in typeof(Engine).GetFields()) {
				object engineMember = fieldInfo.GetValue(null);
				List<(object, MemberInfo)> parentsAndInfos = ReflectionU.GetAllObjectsThatImplementCustomAttribute<ConsoleCommandAttribute>(ref engineMember);

				try {
					for (int i = 0; i < parentsAndInfos.Count; i++) {
						object parentObject = parentsAndInfos[i].Item1;
						MemberInfo parentMember = parentsAndInfos[i].Item2;
						ConsoleCommandAttribute memberAttribute = parentMember.GetCustomAttribute<ConsoleCommandAttribute>();

						if (this.currentConsoleCommand[0].Equals(memberAttribute.command, StringComparison.CurrentCultureIgnoreCase)) {
							object? returnVal = null;

							if (this.currentConsoleCommand.Length == 1) {
								returnVal = parentMember.GetUnderlyingValue(parentObject);
							} else {
								if (parentMember.MemberType is MemberTypes.Field or MemberTypes.Property) {
									object parameter = ReflectionU.Convert(this.currentConsoleCommand[1], parentMember.UnderlyingType());
									parentMember.SetUnderlyingValue(parentObject, parameter);
									returnVal = parentMember.GetUnderlyingValue(parentObject);
								} else if (parentMember.MemberType == MemberTypes.Method) {
									object[] parameters = new object[this.currentConsoleCommand.Length - 1];
									for (int j = 0; j < parameters.Length; j++) {
										Type parameterType = parentMember.ParameterType(j);
										if (parameterType.IsGenericParameter) {
											parameters[j] = ReflectionU.Convert(this.currentConsoleCommand[j + 1], typeof(string));
										} else {
											parameters[j] = ReflectionU.Convert(this.currentConsoleCommand[j + 1], parameterType);
										}
									}

									if (((MethodInfo) parentMember).IsGenericMethod) {
										returnVal = ((MethodInfo) parentMember).MakeGenericMethod(typeof(string)).Invoke(parentObject, parameters);
									} else {
										returnVal = parentMember.GetUnderlyingValue(parentObject, parameters);
									}
								}
							}

							if (returnVal != null) {
								Engine.Editor.Console.WriteLine(ConsoleEntry.White(memberAttribute.outputFormat + Convert.ToString(returnVal)));
							}
						}
					}
				} catch (Exception e) {
					Console.WriteLine(e.ToString());
					Engine.Editor.Console.WriteLine(ConsoleEntry.Debug(e.Message));
				}
			}
		}
	}

	public struct ConsoleEntry {
		public List<(string text, Color4 color)> segments;

		private ConsoleEntry(string text, Color4 color) {
			this.segments = new List<(string, Color4)>();
			this.segments.Add((this.ParseString(text), color));
		}

		private string ParseString(string text) {
			if (text.Contains("$")) {
				try {
					List<string> commands = text.Split(" ").ToList();
					int signIndex = commands.IndexOf("$");
					if (commands.Count - 1 > signIndex) {
						string toEval = commands[commands.IndexOf("$") + 1];

						MethodDelegate dele = CSScript.Evaluator.CreateDelegate($@"
						using System;
						using RavEngine;
						using RavUtilities;
							public string CSScriptDelegate() {{
								return Convert.ToString({toEval});
							}}");

						string toReplace = (string) dele.Invoke(null);
						text = text.Replace(toEval, "");
						text = text.Replace("$", toReplace);
					}
				} catch (Exception e) {
					text = e.Message;
				}
			}

			return text;
		}

		public ConsoleEntry Add(string text, Color4 color) {
			this.segments.Add((this.ParseString(text), color));
			return this;
		}

		public static ConsoleEntry White(string message) { return new ConsoleEntry(message, Color4.White); }

		public static ConsoleEntry Grey(string message) { return new ConsoleEntry(message, Color4.Grey); }

		public static ConsoleEntry Info(string message) { return new ConsoleEntry("Info: ", Color4.Green).Add(message, Color4.White); }

		public static ConsoleEntry Debug(string message) { return new ConsoleEntry("Debug: ", Color4.Cyan).Add(message, Color4.White); }

		public static ConsoleEntry Warning(string message) { return new ConsoleEntry("Warning: ", Color4.Yellow).Add(message, Color4.White); }

		public static ConsoleEntry Error(string message) { return new ConsoleEntry("Error: ", Color4.Red).Add(message, Color4.White); }
	}

	public class ConsoleCommandAttribute : Attribute {
		public string command;
		public string outputFormat;

		public ConsoleCommandAttribute(string command, string outputFormat = "") {
			this.command = command;
			this.outputFormat = outputFormat;
		}
	}
}