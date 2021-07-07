using System;
using System.IO;

namespace RavEngine {
	public class Resource {
		private Action? fileModifiedCallback;
		private DateTime fileModifiedDate;
		private string resourceName;
		private string filePath;

		public Stream fileStream;

		public Resource(in string filePath, in string resourceName, Action? onFileModify = null) {
			this.filePath = filePath;
			this.resourceName = resourceName;
			this.fileModifiedCallback = onFileModify;
			this.LoadStream();
		}

		public Resource(ref Resource assetReference) {
			this.filePath = assetReference.filePath;
			this.fileStream = assetReference.fileStream;
			this.resourceName = assetReference.resourceName;
			this.fileModifiedCallback = assetReference.fileModifiedCallback;
			this.fileModifiedDate = assetReference.fileModifiedDate;
		}

		~Resource() {
			Engine.Resources.UnloadResource(this.resourceName);
		}

		public void Dispose() {
			Engine.Resources.UnloadResource(this.resourceName);
			GC.SuppressFinalize(this);
		}

		internal void Update() {
			DateTime currFileModified = File.GetLastWriteTime(this.filePath);

			if (currFileModified != this.fileModifiedDate) {
				this.LoadStream();
				this.fileModifiedCallback?.Invoke();
			}
		}

		private void LoadStream() {
			// Will try to load file over and over again, usually a file will get locked for a short while
			// as the file is being saved in another program

			this.fileStream = new MemoryStream();
			while (true) {
				try {
					File.Open(this.filePath,
						FileMode.Open,
						FileAccess.ReadWrite,
						FileShare.ReadWrite).CopyTo(this.fileStream);
					break;
				} catch (Exception e) {
					Console.WriteLine(e.Message);
					Engine.Editor.Console.WriteToOutput(ConsoleEntry.Error("Failed Loading Asset: " + e.Message));
				}
			}

			Engine.Editor.Console.WriteToOutput(ConsoleEntry.Info("Loaded Asset: " + this.resourceName));
			this.fileModifiedDate = File.GetLastWriteTime(this.filePath);
		}
	}
}
