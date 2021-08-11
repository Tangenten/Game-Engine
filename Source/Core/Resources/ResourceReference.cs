using System;

namespace RavEngine {
	public class ResourceReference<T> where T : Resource, IDisposable {
		internal DateTime fileModifiedData;
		internal string filePath;
		internal string resourceName;
		private bool skipNextReload;

		public T Resource => Engine.Resources.GetResource(this);
		public bool HasReloaded => this.CheckReloaded();

		public void SkipNextReload() { this.skipNextReload = true; }

		private bool CheckReloaded() {
			if (this.fileModifiedData != this.Resource.fileModifiedDate) {
				this.fileModifiedData = this.Resource.fileModifiedDate;
				if (this.skipNextReload) {
					this.skipNextReload = false;
					return false;
				}
				return true;
			}
			return false;
		}

		~ResourceReference() { Engine.Resources.RemoveReference(this); }

		public void Dispose() {
			GC.SuppressFinalize(this);
			Engine.Resources.RemoveReference(this);
		}
	}
}