using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace RavEngine {
	public class ResourcesE : EngineCore {
		private Dictionary<string, ResourceReference> resourceCache;
		private List<string> filePaths;

		private FileSystemWatcher userDirectoryWatcher;
		private FileSystemWatcher engineDirectoryWatcher;

		public ResourcesE() {
		}

		internal override void Start() {
			this.resourceCache = new Dictionary<string, ResourceReference>();

			this.filePaths = new List<string>();

			this.engineDirectoryWatcher = new FileSystemWatcher($@"{Environment.CurrentDirectory}\Assets");
			this.engineDirectoryWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;

			this.engineDirectoryWatcher.Changed += this.DirectoryModified;
			this.engineDirectoryWatcher.Created += this.DirectoryModified;
			this.engineDirectoryWatcher.Deleted += this.DirectoryModified;
			this.engineDirectoryWatcher.Renamed += this.DirectoryModified;

			this.engineDirectoryWatcher.IncludeSubdirectories = true;
			this.engineDirectoryWatcher.EnableRaisingEvents = true;

			this.BuildFilePaths();
		}

		internal override void Stop() {
		}

		internal override void Update() {
			foreach (KeyValuePair<string, ResourceReference> keyValuePair in this.resourceCache) {
				keyValuePair.Value.Update();
			}
		}

		internal override void Reset() {
			this.resourceCache.Clear();
			this.userDirectoryWatcher = null;
		}

		internal void SetUserResourceDirectory(string path) {
			this.userDirectoryWatcher = new FileSystemWatcher(path);
			this.userDirectoryWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;

			this.userDirectoryWatcher.Changed += this.DirectoryModified;
			this.userDirectoryWatcher.Created += this.DirectoryModified;
			this.userDirectoryWatcher.Deleted += this.DirectoryModified;
			this.userDirectoryWatcher.Renamed += this.DirectoryModified;

			this.userDirectoryWatcher.IncludeSubdirectories = true;
			this.userDirectoryWatcher.EnableRaisingEvents = true;

			this.BuildFilePaths();
		}

		internal void CreateEngineResource(string path) {
			string fullPath = Path.Combine(this.engineDirectoryWatcher.Path, path);
			File.Create(fullPath);
			this.BuildFilePaths();
		}

		public void CreateResource(string path) {
			string fullPath = Path.Combine(this.userDirectoryWatcher.Path, path);
			File.Create(fullPath);
			this.BuildFilePaths();
		}

		public bool HasResource(string fileName) {
			for (int i = 0; i < this.filePaths.Count; i++) {
				if (this.filePaths[i].Contains(fileName)) {
					return true;
				}
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public Resource LoadResource(in string resourceName, Action onFileModify = null) {
			if (!this.resourceCache.ContainsKey(resourceName)) {
				string path = this.ResourceNameToPath(resourceName);
				this.resourceCache[resourceName] = new ResourceReference(path, resourceName, onFileModify);
			}

			return this.resourceCache[resourceName].GetReference();
		}

		public void LoadResourceAsync(string resourceName, Action<Resource> onDoneLoading, Action onFileModify = null) {
			Task.Run(() => {
				this.LoadResource(resourceName, onFileModify);

				onDoneLoading.Invoke(this.resourceCache[resourceName].GetReference());
			});
		}

		internal void UnloadResource(in string resourceName) {
			lock (this.resourceCache) {
				if (this.resourceCache.ContainsKey(resourceName)) {
					this.resourceCache[resourceName].RemoveReference();
					if (this.resourceCache[resourceName].IsDead()) {
						this.resourceCache.Remove(resourceName);
						Engine.Editor.Console.WriteToOutput(ConsoleEntry.Info("Unloaded Asset: " + resourceName));
					}
				}
			}
		}

		internal void DirectoryModified(object sender, FileSystemEventArgs fileSystemEventArgs) {
			this.BuildFilePaths();
		}

		internal void BuildFilePaths() {
			this.filePaths.Clear();

			string[] files = Directory.GetFiles(this.engineDirectoryWatcher.Path, "", SearchOption.AllDirectories);
			this.filePaths.AddRange(files);

			if (this.userDirectoryWatcher != null) {
				Directory.GetFiles(this.userDirectoryWatcher.Path, "", SearchOption.AllDirectories);
				this.filePaths.AddRange(files);
			}
		}

		internal string ResourceNameToPath(string fileName) {
			for (int i = 0; i < this.filePaths.Count; i++) {
				if (this.filePaths[i].Contains(fileName)) {
					return this.filePaths[i];
				}
			}

			throw new Exception("Resource not Found: " + fileName);
		}
	}
}
