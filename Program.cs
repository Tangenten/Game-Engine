using RavUtilities;

namespace RavEngine {
	internal class Program {
		internal static void Main() {
			if (!SystemU.IsProcessUnique()) { return; }

			Engine.Start();
			Engine.Run();
			Engine.Stop();
		}
	}
}
