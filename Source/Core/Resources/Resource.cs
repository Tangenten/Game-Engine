using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace RavEngine {
	public abstract class Resource : IDisposable {
		public string FilePath { get; internal set; }
		public string ResourceName { get; internal set; }

		internal DateTime fileModifiedDate;

		private bool hasLoaded;
		private bool skipNextReload;

		private int referenceCount;
		private Stopwatch stopwatch;

		public Resource() {
			this.stopwatch = new Stopwatch();
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		protected abstract void LoadImplementation();

		protected void Load() {
			this.LoadImplementation();
			this.hasLoaded = true;
		}

		protected void LoadIfNotLoaded() {
			if (!this.hasLoaded) {
				this.Load();
			}
		}

		protected void SkipNextReload() => this.skipNextReload = true;

		private void Reload() {
			if (this.hasLoaded && !this.skipNextReload) {
				this.Load();
				Engine.Editor.Console.WriteLine(ConsoleEntry.Info("Reloaded Resource: " + this.ResourceName));
			}

			if (this.skipNextReload) {
				this.skipNextReload = false;
			}
			this.fileModifiedDate = File.GetLastWriteTime(this.FilePath);
		}

		internal void AddReference<T>(ResourceReference<T> reference) where T : Resource {
			Interlocked.Increment(ref this.referenceCount);
		}

		internal void RemoveReference<T>(ResourceReference<T> reference) where T : Resource {
			Interlocked.Decrement(ref this.referenceCount);
		}

		[Conditional("DEBUG")]
		internal void Update() {
			if (this.referenceCount == 0) {
				if (!this.stopwatch.IsRunning) {
					this.stopwatch.Restart();
				}
				// Wait 30 seconds before actually unloading resource
				if (this.stopwatch.Elapsed.TotalSeconds > 30) {
					Engine.Resources.UnloadResource(this);
				}
			} else {
				if (!this.stopwatch.IsRunning) {
					this.stopwatch.Stop();
				}
			}

			if (!(File.GetLastWriteTime(this.FilePath) == this.fileModifiedDate)) {
				this.Reload();
			}
		}

		public void Dispose() { }
	}
}
