namespace RavEngine {
	public static class Engine {
		public static AudioE Audio = new();
		public static CoroutinesE Coroutines = new();
		public static DebugE Debug = new();
		public static EditorE Editor = new();
		public static GameE Game = new();
		public static GraphicsE Graphics = new();
		public static InputE Input = new();
		public static NetworkingE Networking = new();
		public static PostBoxE Postbox = new();
		public static ProfilerE Profiler = new();
		public static ReflectionE Reflection = new();
		public static ResourcesE Resources = new();
		public static SerilizationE Serilization = new();
		public static SettingsE Settings = new();
		public static SignalsE Signals = new();
		public static TestsE Tests = new();
		public static ThreadingE Threading = new();
		public static TimeE Time = new();
		public static WindowE Window = new();

		internal static void Start() {
			Window.Start();

			Audio.Start();
			Coroutines.Start();
			Debug.Start();
			Editor.Start();
			Game.Start();
			Graphics.Start();
			Input.Start();
			Networking.Start();
			Postbox.Start();
			Profiler.Start();
			Reflection.Start();
			Resources.Start();
			Serilization.Start();
			Settings.Start();
			Signals.Start();
			Tests.Start();
			Threading.Start();
			Time.Start();
		}

		internal static void Stop() {
			Audio.Stop();
			Coroutines.Stop();
			Debug.Stop();
			Editor.Stop();
			Game.Stop();
			Graphics.Stop();
			Input.Stop();
			Networking.Stop();
			Postbox.Stop();
			Profiler.Stop();
			Reflection.Stop();
			Resources.Stop();
			Serilization.Stop();
			Settings.Stop();
			Signals.Stop();
			Tests.Stop();
			Threading.Stop();
			Time.Stop();
			Window.Stop();
		}

		internal static void Reset() {
			Audio.Reset();
			Coroutines.Reset();
			Debug.Reset();
			Editor.Reset();
			Game.Reset();
			Graphics.Reset();
			Input.Reset();
			Networking.Reset();
			Postbox.Reset();
			Profiler.Reset();
			Reflection.Reset();
			Resources.Reset();
			Serilization.Reset();
			Settings.Reset();
			Signals.Reset();
			Tests.Reset();
			Threading.Reset();
			Time.Reset();
			Window.Reset();
		}

		internal static void Update() {
			Audio.Update();
			Coroutines.Update();
			Debug.Update();
			Editor.Update();
			Game.Update();
			Graphics.Update();
			Input.Update();
			Networking.Update();
			Postbox.Update();
			Profiler.Update();
			Reflection.Update();
			Resources.Update();
			Serilization.Update();
			Settings.Update();
			Signals.Update();
			Tests.Update();
			Threading.Update();
			Time.Update();
			Window.Update();
		}

		internal static void Run() {
			while (Window.Open) {
				Update();
			}
		}
	}

	public abstract class EngineCore {
		internal abstract void Start();
		internal abstract void Stop();
		internal abstract void Update();
		internal abstract void Reset();
	}
}
