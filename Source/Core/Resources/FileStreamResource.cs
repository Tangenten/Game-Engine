using System.IO;
using RavUtilities;

namespace RavEngine {
	public class FileStreamResource : Resource {
		private FileStream fileStream;
		public FileStream FileStream => this.GetStream();

		protected override void LoadImplementation() {
			#if DEBUG
			this.fileStream = FileU.LoadStreamWaitLock(this.FilePath);
			#else
			this.fileStream = new FileStream(this.FilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
			#endif
		}

		private FileStream GetStream() {
			this.LoadIfNotLoaded();
			return this.fileStream;
		}
	}
}