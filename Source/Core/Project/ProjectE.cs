using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using ExtendedXmlSerializer;
using ExtendedXmlSerializer.Configuration;
using RavUtilities;

namespace RavEngine {
	public class ProjectE : EngineCore {
		internal string ProjectName { get; private set; }

		private ProjectMode projectMode;
		internal bool IsProjectPlaying => this.projectMode == ProjectMode.PLAY;
		internal bool IsProjectEditing => this.projectMode == ProjectMode.EDIT;

		internal bool IsProjectLoaded { get; private set; }
		internal bool HasProjectCrashed { get; set; }
		internal event Action ProjectLoaded;

		internal string ProjectFileExtension => ".rav";
		internal string ProjectFilePath => Path.Combine(this.ProjectDirectory, this.ProjectName + this.ProjectFileExtension);

		internal string ProjectDirectory { get; private set; }

		internal string CurrentSceneName { get; private set; }
		internal string ProjectSceneDirectory => Path.Combine(this.ProjectDirectory, "Scenes");

		private string SerilizationExtension => ".xml";
		private XmlWriterSettings xmlWriterSettings;
		private XmlReaderSettings xmlReaderSettings;
		private IExtendedXmlSerializer XML;

		private Assembly projectAssembly;
		private bool projectAssemblyBuilding;
		private DateTime projectAssemblyDateModified;
		private Type[] serverTypes;
		private Type[] nodeTypes;
		private Type[] editorWindowTypes;
		internal event Action ScriptReload;

		#if DEBUG
		internal string ProjectAssemblyDirectory => Path.Combine(this.ProjectDirectory, "Build", "Debug");
		#elif RELEASE
		internal string ProjectAssemblyDirectory => Path.Combine(this.ProjectDirectory, "Build", "Release");
		#endif
		internal string ProjectAssemblyDLLPath => Path.Combine(this.ProjectAssemblyDirectory, this.ProjectName + ".dll");
		internal string ProjectAssemblyPDBPath => Path.Combine(this.ProjectAssemblyDirectory, this.ProjectName + ".pdb");

		internal string ProjectResourcesDirectory => Path.Combine(this.ProjectDirectory, "Resources");
		internal string ProjectPrefabsDirectory => Path.Combine(this.ProjectDirectory, "Prefabs");
		internal string ProjectSettingsPath => Path.Combine(this.ProjectDirectory, "Settings" + this.SerilizationExtension);

		public ProjectE() {
			this.XML = new ConfigurationContainer().Create();

			this.xmlWriterSettings = new XmlWriterSettings();
			this.xmlWriterSettings.CloseOutput = false;
			this.xmlWriterSettings.Encoding = Encoding.Default;
			this.xmlWriterSettings.Indent = true;
			this.xmlWriterSettings.NamespaceHandling = NamespaceHandling.Default;
			this.xmlWriterSettings.NewLineHandling = NewLineHandling.None;

			this.xmlReaderSettings = new XmlReaderSettings();
			this.xmlReaderSettings.CloseInput = false;

			AppDomain.CurrentDomain.AssemblyResolve += this.CurrentDomainOnAssemblyResolve;
			AppDomain.CurrentDomain.TypeResolve += this.CurrentDomainOnTypeResolve;
		}

		internal override void Start() { this.Reset(); }

		internal override void Stop() { }

		internal override void Update() { this.WatchProject(); }

		internal override void Reset() {
			this.ProjectName = "";

			this.projectMode = ProjectMode.EDIT;

			this.IsProjectLoaded = false;

			this.ProjectDirectory = "";

			this.CurrentSceneName = "";

			this.projectAssembly = null;
			this.projectAssemblyBuilding = false;
			this.projectAssemblyDateModified = new DateTime();
			this.serverTypes = Array.Empty<Type>();
			this.nodeTypes = Array.Empty<Type>();
			this.editorWindowTypes = Array.Empty<Type>();
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		internal void PlayProject() {
			if (!this.IsProjectLoaded || this.IsProjectPlaying) {
				return;
			}

			this.SaveScene();
			this.LoadScene(this.CurrentSceneName);

			this.projectMode = ProjectMode.PLAY;
			Engine.Editor.Console.WriteLine(ConsoleEntry.Info("Play Mode"));
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		internal void EditProject() {
			if (!this.IsProjectLoaded || this.IsProjectEditing) {
				return;
			}

			this.LoadScene(this.CurrentSceneName);

			this.projectMode = ProjectMode.EDIT;
			Engine.Editor.Console.WriteLine(ConsoleEntry.Info("Edit Mode"));
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		internal void ReloadProject() {
			if (!this.IsProjectLoaded) {
				return;
			}
			this.LoadProject(this.ProjectDirectory);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		internal void NewProject(string projectDirectoryPath, string projectName) {
			this.SaveProject();

			string templatePath = Path.GetDirectoryName(Engine.Resources.ResourcePathToFilePath("Template" + this.ProjectFileExtension));
			DirectoryInfo? diSource = new DirectoryInfo(templatePath);
			DirectoryInfo? diTarget = new DirectoryInfo(projectDirectoryPath);

			// Copy and Rename All Files
			FileU.CopyAllFilesInDirectory(diSource, diTarget);
			FileU.RenameAllFilesInDirectory(diTarget, "Template", projectName);

			// Rewrite .sln files
			string solutionPath = Path.Combine(projectDirectoryPath, projectName + ".sln");
			File.WriteAllText(solutionPath, File.ReadAllText(solutionPath).Replace("Template", projectName));

			// Rewrite .csproj files
			string csprojPath = Path.Combine(projectDirectoryPath, projectName + ".csproj");
			File.WriteAllText(csprojPath, File.ReadAllText(csprojPath).Replace("Template", projectName));

			this.LoadProject(projectDirectoryPath);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		internal void RenameProject(string newName) {
			this.SaveProject();

			DirectoryInfo? directoryInfo = new DirectoryInfo(this.ProjectDirectory);

			// Copy and Rename All Files
			FileU.RenameAllFilesInDirectory(directoryInfo, this.ProjectName, newName);

			// Rewrite .sln files
			string solutionPath = Path.Combine(this.ProjectDirectory, this.ProjectName + ".sln");
			File.WriteAllText(solutionPath, File.ReadAllText(solutionPath).Replace(this.ProjectName, newName));

			// Rewrite .csproj files
			string csprojPath = Path.Combine(this.ProjectDirectory, this.ProjectName + ".csproj");
			File.WriteAllText(csprojPath, File.ReadAllText(csprojPath).Replace(this.ProjectName, newName));

			this.LoadProject(this.ProjectDirectory);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		internal void LoadProject(string projectDirectoryPath) {
			if (Directory.GetFiles(projectDirectoryPath, "*" + this.ProjectFileExtension).Length != 1) {
				Engine.Editor.Console.WriteLine(ConsoleEntry.Info("No Project File Found in Directory"));
				return;
			}

			this.SaveProject();
			Engine.Settings.LoadXml(Path.Combine(projectDirectoryPath, "Settings" + this.SerilizationExtension));
			Engine.Reset();

			this.IsProjectLoaded = true;
			this.ProjectDirectory = projectDirectoryPath;

			this.ProjectName = Path.GetFileNameWithoutExtension(Directory.GetFiles(projectDirectoryPath, "*" + this.ProjectFileExtension)[0]);
			this.projectMode = ProjectMode.EDIT;

			if (Engine.Settings.TryGet("STARTUP_SCENE", out string val)) {
				this.CurrentSceneName = val;
			} else {
				this.IsProjectLoaded = false;
				throw new Exception("Startup scene not found");
			}

			this.projectAssembly = null;
			this.projectAssemblyBuilding = false;
			this.projectAssemblyDateModified = new DateTime();
			this.serverTypes = Array.Empty<Type>();
			this.nodeTypes = Array.Empty<Type>();

			DateTime currentEngineRefAssembly = File.GetLastWriteTime(Engine.Resources.ResourcePathToFilePath("Engine.dll"));
			DateTime userEngineRefAssembly = File.GetLastWriteTime(Path.Combine(this.ProjectDirectory, "Engine.dll"));
			if (currentEngineRefAssembly != userEngineRefAssembly) {
				File.Copy(Engine.Resources.ResourcePathToFilePath("Engine.dll"), Path.Combine(this.ProjectDirectory, "Engine.dll"), true);
			}

			this.CompileProject();
			this.ReloadScene();

			this.ProjectLoaded?.Invoke();
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		internal void SaveProject() {
			if (!this.IsProjectLoaded || this.IsProjectPlaying) {
				return;
			}

			this.SaveScene();

			Engine.Editor.Console.WriteLine(ConsoleEntry.Info("Project Saved"));
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		internal void PublishProject(string publishDirectory) {
			if (!this.IsProjectLoaded || this.IsProjectPlaying) { }

			// Compile Project with RELEASE PUBLISH define
			// Move DLL's to Project Directory/Libraries
			// Move and Rename PreCompiled Engine Executable with PUBLISH define to Directory
			// Compress Resources to .zip file, Place next to executable Directory/Resources.zip
			// Set Exe Icon
			// PUBLISH Build looks for game in
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		internal void WatchProject() {
			if (!this.IsProjectLoaded || this.projectAssemblyBuilding || !File.Exists(this.ProjectAssemblyDLLPath)) {
				return;
			}

			DateTime currProjectDLLDateModified = File.GetLastWriteTime(this.ProjectAssemblyDLLPath);
			if (currProjectDLLDateModified != this.projectAssemblyDateModified) {
				this.ReloadScripts();
				this.projectAssemblyDateModified = currProjectDLLDateModified;
			}
		}

		private void ReloadScripts() {
			this.UnloadScriptsDLL();
			this.LoadScriptsDLL();
			this.ScriptReload?.Invoke();
		}

		private void UnloadScriptsDLL() {
			this.projectAssembly = null;
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}

		private void LoadScriptsDLL() {
			if (!File.Exists(this.ProjectAssemblyDLLPath)) {
				return;
			}

			if (File.Exists(this.ProjectAssemblyPDBPath)) {
				this.projectAssembly = Assembly.Load(File.ReadAllBytes(this.ProjectAssemblyDLLPath), File.ReadAllBytes(this.ProjectAssemblyPDBPath));
			} else {
				this.projectAssembly = Assembly.Load(File.ReadAllBytes(this.ProjectAssemblyDLLPath));
			}

			this.nodeTypes = this.projectAssembly.TypesImplementing<Node>();
			this.serverTypes = this.projectAssembly.TypesImplementing<Server>();
			this.editorWindowTypes = this.projectAssembly.TypesImplementing<EditorWindow>();

			Engine.Editor.Console.WriteLine(ConsoleEntry.Info("Scripts Loaded"));
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		internal void CompileProject() {
			if (!this.IsProjectLoaded) {
				return;
			}

			Stopwatch stopwatch = Stopwatch.StartNew();

			Process process = new Process {
				StartInfo = new ProcessStartInfo {
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
					CreateNoWindow = true,
					ErrorDialog = true,
					FileName = "cmd.exe",
					WorkingDirectory = this.ProjectDirectory,
					Arguments = "/c dotnet build"
				}
			};

			process.OutputDataReceived += (s, e) => {
				if (e.Data != null) {
					if (e.Data.Contains("Build succeeded.")) {
						Engine.Editor.Console.WriteLine(ConsoleEntry.Info("Build Succeeded"));
						this.projectAssemblyBuilding = false;
					}
				}
			};
			process.ErrorDataReceived += (s, e) => {
				string err = e.Data;
				if (err != null) {
					Engine.Editor.Console.WriteLine(ConsoleEntry.Error("Build Failed: " + e.Data));
					Console.WriteLine(e.Data);
				}

				this.projectAssemblyBuilding = false;
			};
			process.Exited += (s, e) => { this.projectAssemblyBuilding = false; };

			if (File.Exists(this.ProjectAssemblyDirectory)) {
				Directory.Delete(this.ProjectAssemblyDirectory, true);
			}
			this.projectAssemblyBuilding = true;
			process.EnableRaisingEvents = true;
			process.Start();
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();
			process.WaitForExit();
			Engine.Editor.Console.WriteLine(ConsoleEntry.Info("Compiled Project In: " + stopwatch.Elapsed.TotalSeconds + " Seconds"));
			this.ReloadScripts();
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		internal void CompileProjectAsync() {
			if (!this.IsProjectLoaded) {
				return;
			}

			Process process = new Process {
				StartInfo = new ProcessStartInfo {
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
					CreateNoWindow = true,
					ErrorDialog = true,
					FileName = "cmd.exe",
					WorkingDirectory = this.ProjectDirectory,
					Arguments = "/c dotnet build"
				}
			};

			process.OutputDataReceived += (s, e) => {
				if (e.Data != null) {
					if (e.Data.Contains("Build succeeded.")) {
						Engine.Editor.Console.WriteLine(ConsoleEntry.Info("Build Succeeded"));
						this.projectAssemblyBuilding = false;
					}
				}
			};
			process.ErrorDataReceived += (s, e) => {
				string err = e.Data;
				if (err != null) {
					Engine.Editor.Console.WriteLine(ConsoleEntry.Error("Build Failed: " + e.Data));
					Console.WriteLine(e.Data);
				}

				this.projectAssemblyBuilding = false;
			};
			process.Exited += (s, e) => { this.projectAssemblyBuilding = false; };

			if (File.Exists(this.ProjectAssemblyDirectory)) {
				Directory.Delete(this.ProjectAssemblyDirectory, true);
			}
			this.projectAssemblyBuilding = true;
			process.EnableRaisingEvents = true;
			process.Start();
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		internal void AddScene(string sceneName) {
			if (!this.IsProjectLoaded) {
				return;
			}
			if (this.GetAllSceneNames().Contains(sceneName)) {
				Engine.Editor.Console.WriteLine(ConsoleEntry.Warning("Scene '" + sceneName + "' Already Exists"));
				return;
			}

			File.Create(Path.Combine(this.ProjectSceneDirectory, sceneName + this.SerilizationExtension));
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		internal void RemoveScene(string sceneName) {
			if (!this.IsProjectLoaded) {
				return;
			}
			if (this.CurrentSceneName == sceneName) {
				Engine.Editor.Console.WriteLine(ConsoleEntry.Warning("Cant Remove Current Scene, Switch Scenes to Delete"));
				return;
			}
			if (sceneName == Engine.Settings.Get<string>("STARTUP_SCENE")) {
				Engine.Editor.Console.WriteLine(ConsoleEntry.Warning("Cant Remove Startup Scene, Change Startup scene to Delete"));
				return;
			}

			FileU.DeleteFileToRecycleBin(Path.Combine(this.ProjectSceneDirectory, sceneName + this.SerilizationExtension));
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		internal void RenameScene(string oldName, string newName) {
			if (!this.IsProjectLoaded) {
				return;
			}
			File.Move(Path.Combine(this.ProjectSceneDirectory, oldName + this.SerilizationExtension), Path.Combine(this.ProjectSceneDirectory, newName + this.SerilizationExtension));
			if (oldName == this.CurrentSceneName) {
				this.CurrentSceneName = newName;
			}
			if (oldName == Engine.Settings.Get<string>("STARTUP_SCENE")) {
				Engine.Settings.Set("STARTUP_SCENE", newName);
			}
			Engine.Editor.Console.WriteLine(ConsoleEntry.Warning("Renamed Scene"));
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		internal void LoadScene(string sceneName) {
			if (!this.IsProjectLoaded) {
				return;
			}
			if (!File.Exists(Path.Combine(this.ProjectSceneDirectory, sceneName + this.SerilizationExtension))) {
				Engine.Editor.Console.WriteLine(ConsoleEntry.Warning("Cant load Scene as it cant be found: " + sceneName));
				return;
			}

			string serilization = File.ReadAllText(Path.Combine(this.ProjectSceneDirectory, sceneName + this.SerilizationExtension));
			if (serilization == "") {
				Engine.Game.Scene = new Scene();
			} else {
				Engine.Game.Scene = this.XML.Deserialize<Scene>(this.xmlReaderSettings, serilization);
			}
			this.CurrentSceneName = sceneName;

			this.projectMode = ProjectMode.EDIT;
			Engine.Editor.Console.WriteLine(ConsoleEntry.Info("Scene Loaded"));
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		internal void SaveScene() {
			if (!this.IsProjectLoaded) {
				return;
			}
			this.EditProject();

			string serilization = this.XML.Serialize(this.xmlWriterSettings, Engine.Game.Scene);
			File.WriteAllText(Path.Combine(this.ProjectSceneDirectory, this.CurrentSceneName + this.SerilizationExtension), serilization);

			Engine.Editor.Console.WriteLine(ConsoleEntry.Info("Scene Saved"));
		}

		internal void ReloadScene() {
			if (!this.IsProjectLoaded) {
				return;
			}
			this.LoadScene(this.CurrentSceneName);
		}

		internal Type[] GetAllEditorWindowTypes() {
			if (!this.IsProjectLoaded) {
				return Array.Empty<Type>();
			}
			return this.editorWindowTypes;
		}

		internal string[] GetAllEditorWindowNames() {
			if (!this.IsProjectLoaded) {
				return Array.Empty<string>();
			}
			return this.editorWindowTypes.Select(window => window.Name).ToArray();
		}

		internal string[] GetAllServerNames() {
			if (!this.IsProjectLoaded) {
				return Array.Empty<string>();
			}
			return this.serverTypes.Select(server => server.Name).ToArray();
		}

		internal Type[] GetAllServerTypes() {
			if (!this.IsProjectLoaded) {
				return Array.Empty<Type>();
			}
			return this.serverTypes;
		}

		internal string[] GetAllNodeNames() {
			if (!this.IsProjectLoaded) {
				return Array.Empty<string>();
			}
			return this.nodeTypes.Select(node => node.Name).ToArray();
		}

		internal Type[] GetAllNodeTypes() {
			if (!this.IsProjectLoaded) {
				return Array.Empty<Type>();
			}
			return this.nodeTypes;
		}

		internal string[] GetAllSceneNames() {
			if (!this.IsProjectLoaded) {
				return Array.Empty<string>();
			}
			return Directory.EnumerateFiles(this.ProjectSceneDirectory).Select(file => Path.GetFileNameWithoutExtension(file)).ToArray();
		}

		internal string[] GetAllResourceNames() {
			if (!this.IsProjectLoaded) {
				return Array.Empty<string>();
			}
			return Directory.EnumerateFiles(this.ProjectResourcesDirectory).Select(file => Path.GetFileNameWithoutExtension(file)).ToArray();
		}

		private Assembly? CurrentDomainOnTypeResolve(object? sender, ResolveEventArgs args) { return this.projectAssembly; }

		private Assembly? CurrentDomainOnAssemblyResolve(object? sender, ResolveEventArgs args) { return this.projectAssembly; }
	}

	public enum ProjectMode {
		PLAY,
		EDIT
	}
}