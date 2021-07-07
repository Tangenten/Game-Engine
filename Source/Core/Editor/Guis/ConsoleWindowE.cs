using System.Diagnostics;
using System.Numerics;
using RavContainers;

namespace RavEngine {
	public class ConsoleWindowE {
		internal void Update() { }
		internal void Reset() { }

		public void WriteToInput(string command) {
		}

		[Conditional("DEBUG")]
		public void WriteToOutput(ConsoleEntry consoleEntry) {
		}
	}


	public struct ConsoleEntry {
		public Color4 color;
		public string text;

		public ConsoleEntry(string text, Color4 color) {
			this.text = text;
			this.color = color;
		}

		public static ConsoleEntry Info(string message) {
			return new ConsoleEntry(message, Color4.Green);
		}

		public static ConsoleEntry Debug(string message) {
			return new ConsoleEntry(message, Color4.Blue);
		}

		public static ConsoleEntry Warning(string message) {
			return new ConsoleEntry(message, Color4.Yellow);
		}

		public static ConsoleEntry Error(string message) {
			return new ConsoleEntry(message, Color4.Red);
		}
	}
}
