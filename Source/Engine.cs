using RavUtilities;

namespace RavEngine {
	public static class Engine {
		public static AudioE Audio;
		public static CoroutinesE Coroutines;
		public static EditorE Editor;
		public static GameE Game;
		public static GraphicsE Graphics;
		public static InputE Input;
		public static NetworkingE Networking;
		public static PostBoxE Postbox;
		public static ProjectE Project;
		public static ResourcesE Resources;
		public static SettingsE Settings;
		public static ThreadingE Threading;
		public static TimeE Time;
		public static WindowE Window;

		internal static void Start() {
			TimerU.StartTimer();

			Settings = new();  // Enable Loading From Settings Early
			Editor = new();    // Enable Logging Early
			Resources = new(); // Enable Opening Resources Early
			Threading = new(); // Enable Starting Threads Early

			Audio = new();
			Coroutines = new();
			Game = new();
			Graphics = new();
			Input = new();
			Networking = new();
			Postbox = new();
			Project = new();
			Time = new();
			Window = new();

			Audio.Start();
			Coroutines.Start();
			Editor.Start();
			Game.Start();
			Graphics.Start();
			Input.Start();
			Networking.Start();
			Postbox.Start();
			Project.Start();
			Resources.Start();
			Settings.Start();
			Threading.Start();
			Time.Start();
			Window.Start();

			Reset();

			double timeTaken = TimerU.StopTimer();
			Editor.Console.WriteLine(ConsoleEntry.Info($"Engine Initialized in {timeTaken} Seconds"));
		}

		internal static void Stop() {
			Audio.Stop();
			Coroutines.Stop();
			Editor.Stop();
			Game.Stop();
			Graphics.Stop();
			Input.Stop();
			Networking.Stop();
			Postbox.Stop();
			Project.Stop();
			Resources.Stop();
			Settings.Stop();
			Threading.Stop();
			Time.Stop();
			Window.Stop();
		}

		internal static void Update() {
			Project.Update(); // Reload Scripts if Needed
			Time.Update();    // Increments Time
			Input.Update();   // Clears Inputs
			Window.Update();  // Updates Inputs and Other Events

			Settings.Update();  // Checks if Any Settings Should be Reloaded
			Resources.Update(); // Checks if Any Resources Should be Reloaded

			Networking.Update(); // Collects Network Data and Invokes Listeners if new data
			Coroutines.Update(); // Invokes Coroutines
			Threading.Update();  // Removes Jobs
			Postbox.Update();    // Removes Postboxes if no Listeners
			Editor.Update();     // Invokes Editors

			Game.Update(); // Runs Game for 1 Frame

			Audio.Update();    // Render Audio
			Graphics.Update(); // Renders Graphics
			Editor.Render();   // Renders Editors
			Window.Render();   // Displays
		}

		internal static void Reset() {
			Audio.Reset();
			Coroutines.Reset();
			Editor.Reset();
			Game.Reset();
			Graphics.Reset();
			Input.Reset();
			Networking.Reset();
			Postbox.Reset();
			Project.Reset();
			Resources.Reset();
			Settings.Reset();
			Threading.Reset();
			Time.Reset();
			Window.Reset();
		}

		internal static void Run() {
			while (Window.IsOpen) {
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