using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using RavUtilities;

namespace RavEngine {
	public class ResourcesE : EngineCore {
		private Dictionary<string, Resource> resourceCache;
		private List<string> filePaths;

		private FileSystemWatcher engineDirectoryWatcher;
		private FileSystemWatcher userDirectoryWatcher;
		private bool directoryModified;

		internal ResourcesE() {
			this.resourceCache = new Dictionary<string, Resource>();
			this.filePaths = new List<string>();

			#if DEBUG
			string engineResourcesPath = "";
			if (FileU.TryGetSolutionPath(out string path)) {
				engineResourcesPath = $@"{path}\Resources";
			} else {
				throw new Exception("Engine Resources Path Not Found");
			}
			#elif RELEASE
			string engineResourcesPath = $@"{Environment.CurrentDirectory}\Resources";
			#elif PUBLISH
			#endif

			this.engineDirectoryWatcher = new FileSystemWatcher(engineResourcesPath);
			this.engineDirectoryWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
			this.engineDirectoryWatcher.IncludeSubdirectories = true;
			this.engineDirectoryWatcher.EnableRaisingEvents = true;

			this.engineDirectoryWatcher.Changed += this.DirectoryModified;
			this.engineDirectoryWatcher.Created += this.DirectoryModified;
			this.engineDirectoryWatcher.Deleted += this.DirectoryModified;
			this.engineDirectoryWatcher.Renamed += this.DirectoryModified;

			this.BuildFilePaths();
		}

		internal override void Start() { }

		internal override void Stop() { }

		internal override void Update() {
			if (this.directoryModified) {
				this.BuildFilePaths();
				this.directoryModified = false;
			}
			foreach (KeyValuePair<string, Resource> keyValuePair in this.resourceCache) {
				keyValuePair.Value.Update();
			}
		}

		internal override void Reset() {
			this.userDirectoryWatcher = null;
			this.BuildFilePaths();
		}

		internal void SetUserResourceDirectory(string directoryPath) {
			this.userDirectoryWatcher = new FileSystemWatcher(directoryPath);
			this.userDirectoryWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
			this.userDirectoryWatcher.IncludeSubdirectories = true;
			this.userDirectoryWatcher.EnableRaisingEvents = true;

			this.userDirectoryWatcher.Changed += this.DirectoryModified;
			this.userDirectoryWatcher.Created += this.DirectoryModified;
			this.userDirectoryWatcher.Deleted += this.DirectoryModified;
			this.userDirectoryWatcher.Renamed += this.DirectoryModified;

			this.BuildFilePaths();
		}

		[ConsoleCommand("CREATE_RESOURCE")]
		internal void CreateEngineResource(string resourcePath) {
			string filePath = Path.Combine(this.engineDirectoryWatcher.Path, resourcePath);
			this.engineDirectoryWatcher.EnableRaisingEvents = false;
			File.Create(filePath);
			this.engineDirectoryWatcher.EnableRaisingEvents = true;
			this.BuildFilePaths();
		}

		public void CreateResource(string resourcePath) {
			string filePath = Path.Combine(this.userDirectoryWatcher.Path, resourcePath);
			this.userDirectoryWatcher.EnableRaisingEvents = false;
			File.Create(filePath);
			this.userDirectoryWatcher.EnableRaisingEvents = true;
			this.BuildFilePaths();
		}

		[ConsoleCommand("HAS_RESOURCE")]
		public bool HasResource(string resourcePath) {
			for (int i = this.filePaths.Count - 1; i >= 0; i--) {
				if (this.filePaths[i].Contains(resourcePath)) {
					return true;
				}
			}

			return false;
		}

		[ConsoleCommand("HAS_RESOURCE_LOADED")] [MethodImpl(MethodImplOptions.Synchronized)]
		public bool HasResourceLoaded(string resourcePath) {
			foreach (KeyValuePair<string, Resource> keyValuePair in this.resourceCache) {
				if (keyValuePair.Key.Contains(resourcePath)) {
					return true;
				}
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public ResourceReference<T> CreateReference<T>(in string resourcePath) where T : Resource, new() {
			string filePath = this.ResourcePathToFilePath(resourcePath);

			if (!this.resourceCache.ContainsKey(filePath)) {
				T resource = new T();
				resource.FilePath = filePath;
				resource.ResourceName = resourcePath;
				resource.fileModifiedDate = File.GetLastWriteTime(filePath);
				this.resourceCache[filePath] = resource;
				Engine.Editor?.Console.WriteLine(ConsoleEntry.Info("Loaded Resource: " + resource.ResourceName));
			}

			// A Finalizer will only be called if ALL of the data in the class has no living references to other objects
			// So we need to make sure ResourceReference data is only referenced by itself, Therefore deep copy
			ResourceReference<T> reference = new ResourceReference<T>();
			reference.filePath = string.Copy(filePath);
			reference.resourceName = string.Copy(resourcePath);
			reference.fileModifiedData = this.resourceCache[filePath].fileModifiedDate;
			this.resourceCache[filePath].AddReference(reference);

			return reference;
		}

		public void CreateReferenceAsync<T>(string resourcePath, Action<ResourceReference<T>> onDoneLoading) where T : Resource, new() {
			Engine.Threading.LaunchJob(() => {
				onDoneLoading.Invoke(this.CreateReference<T>(resourcePath));
			});
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		internal void RemoveReference<T>(ResourceReference<T> reference) where T : Resource {
			if (this.resourceCache.ContainsKey(reference.filePath)) {
				this.resourceCache[reference.filePath].RemoveReference(reference);
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		internal T GetResource<T>(ResourceReference<T> reference) where T : Resource { return this.resourceCache[reference.filePath] as T ?? throw new InvalidOperationException(); }

		[MethodImpl(MethodImplOptions.Synchronized)]
		internal void UnloadResource(Resource resource) {
			if (this.resourceCache.ContainsKey(resource.FilePath)) {
				this.resourceCache.Remove(resource.FilePath);
				Engine.Editor.Console.WriteLine(ConsoleEntry.Info("Unloaded Resource: " + resource.FilePath));
			}
		}

		[ConsoleCommand("RESOURCE_TO_PATH")]
		public string ResourcePathToFilePath(string resourcePath) {
			#if DEBUG
			string filePath = null;
			for (int i = this.filePaths.Count - 1; i >= 0; i--) {
				if (this.filePaths[i].EndsWith(resourcePath)) {
					if (filePath == null) {
						filePath = this.filePaths[i];
					} else {
						throw new Exception("Duplicate Resource Names, Add Folder To ResourceName or Rename Resource to Fix: \n" + filePath + "\n" + this.filePaths[i]);
					}
				}
			}
			return filePath;
			#elif RELEASE
			for (int i = this.filePaths.Count - 1; i >= 0; i--) {
				if (this.filePaths[i].EndsWith(resourcePath)) {
					return this.filePaths[i];
				}
			}
			throw new Exception("Resource not Found: " + resourcePath);
			#endif
		}

		internal void DirectoryModified(object sender, FileSystemEventArgs fileSystemEventArgs) { this.directoryModified = true; }

		[MethodImpl(MethodImplOptions.Synchronized)]
		internal void BuildFilePaths() {
			this.filePaths.Clear();

			string[] paths = Directory.GetFiles(this.engineDirectoryWatcher.Path, "", SearchOption.AllDirectories);
			this.filePaths.AddRange(paths);

			if (this.userDirectoryWatcher != null) {
				paths = Directory.GetFiles(this.userDirectoryWatcher.Path, "", SearchOption.AllDirectories);
				this.filePaths.AddRange(paths);
			}
		}

		[ConsoleCommand("LIST_RESOURCES")]
		internal void ListResources() {
			for (int i = this.filePaths.Count - 1; i >= 0; i--) {
				Engine.Editor.Console.WriteLine(ConsoleEntry.Debug(this.filePaths[i]));
			}
		}

		[ConsoleCommand("LIST_LOADED_RESOURCES")] [MethodImpl(MethodImplOptions.Synchronized)]
		internal void ListLoadedResources() {
			foreach (KeyValuePair<string, Resource> keyValuePair in this.resourceCache) {
				Engine.Editor.Console.WriteLine(ConsoleEntry.Debug(keyValuePair.Key));
			}
		}
	}
}
