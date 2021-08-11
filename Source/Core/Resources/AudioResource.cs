using System.IO;
using RavUtilities;

namespace RavEngine {
	public class AudioResource : Resource {
		private Audio monoAudio;
		private Audio stereoAudio;

		public Audio MonoAudio => this.GetMono();
		public Audio StereoAudio => this.GetStereo();

		protected override void LoadImplementation() {
			#if DEBUG
			Stream memoryStream = FileU.LoadStreamWaitLock(this.FilePath);
			#else
			Stream memoryStream = File.Open(this.FilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
			#endif

			Audio audio = AudioU.OpenWavFile(memoryStream, -1, Engine.Audio.SampleRate);

			if (audio.ChannelMode == ChannelMode.MONO) {
				this.monoAudio = audio;
				this.stereoAudio = this.monoAudio.CreateStereo();
			} else if (audio.ChannelMode == ChannelMode.STEREO) {
				this.stereoAudio = audio;
				this.monoAudio = this.stereoAudio.CreateMono();
			}
		}

		private Audio GetMono() {
			this.LoadIfNotLoaded();
			return this.monoAudio;
		}

		private Audio GetStereo() {
			this.LoadIfNotLoaded();
			return this.stereoAudio;
		}
	}
}