using System;

namespace RavEngine {
	public class GameE : EngineCore {
		public Scene Scene { get; internal set; }

		public GameE() { this.Scene = new Scene(); }

		internal override void Start() { }

		internal override void Stop() { }

		internal override void Update() {
			this.Scene.Update();

			if (Engine.Project.IsProjectLoaded && !Engine.Project.HasProjectCrashed && Engine.Project.IsProjectPlaying) {
				#if !PUBLISH

				try {
					this.Scene.Update();
				} catch (Exception e) {
					Console.WriteLine(e);
					Engine.Editor.Console.WriteLine(ConsoleEntry.Error("Soft Crash: " + e.Message));
					Engine.Project.HasProjectCrashed = true;
				}

				#else
					this.Scene.Update();
				#endif
			}
		}

		internal override void Reset() {
			if (Engine.Project.IsProjectLoaded) {
				#if !PUBLISH

				try {
					this.Scene.Reset();
				} catch (Exception e) {
					Console.WriteLine(e);
					Engine.Editor.Console.WriteLine(ConsoleEntry.Error("Soft Crash: " + e.Message));
					Engine.Project.HasProjectCrashed = true;
				}

				#else
					this.Scene.Reset();
				#endif
			}
		}

		public void SetScene(string sceneName) { Engine.Project.LoadScene(sceneName); }
	}
}