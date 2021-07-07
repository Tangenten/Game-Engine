using System;
using System.Threading;

namespace RavEngine {
	public class ResourceReference {
		private Resource resource;
		private int referenceCount;

		public ResourceReference(in string filePath, in string resourceName, Action onFileModify = null) {
			this.resource = new Resource(filePath, resourceName, onFileModify);
		}

		public void RemoveReference() {
			Interlocked.Decrement(ref this.referenceCount);
		}

		public Resource GetReference() {
			Interlocked.Increment(ref this.referenceCount);
			return new Resource(ref this.resource);
		}

		public bool IsDead() {
			return this.referenceCount == 0;
		}

		public void Update() {
			this.resource.Update();
		}
	}
}
