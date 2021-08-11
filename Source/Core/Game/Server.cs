namespace RavEngine {
	public abstract class Server {
		public abstract void Start();
		public abstract void Stop();

		public abstract void Before();
		public abstract void After();
	}
}