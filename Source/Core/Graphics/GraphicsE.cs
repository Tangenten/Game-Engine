using System;
using System.Runtime.InteropServices;
using Silk.NET.OpenGL;

namespace RavEngine {
	public class GraphicsE : EngineCore {
		public GL GL { get; private set; }

		private DebugProc debugProcCallback;
		private GCHandle debugProcCallbackHandle;

		internal override void Start() {
			this.GL = Engine.Window.GLContext;

			this.debugProcCallback = this.DebugCallback;
			this.debugProcCallbackHandle = GCHandle.Alloc(this.debugProcCallback);

			this.GL.DebugMessageCallback(this.debugProcCallback, IntPtr.Zero);
			this.GL.Enable(EnableCap.DebugOutput);
			this.GL.Enable(EnableCap.DebugOutputSynchronous);
		}

		internal override void Stop() { }

		internal override void Update() {
			#if !PUBLISH
			this.DrawToEditorWindow();
			#else
			this.DrawToWindow();
			#endif
		}

		internal override void Reset() { }

		private void DrawToWindow() { }

		private void DrawToEditorWindow() { }

		private void DebugCallback(GLEnum glEnum, GLEnum @enum, int id, GLEnum severity1, int length, nint message1, nint userParam1) {
			string messageString = Marshal.PtrToStringAnsi(message1, length);

			Console.WriteLine($"{severity1} {glEnum} | {messageString} \n\n");
			Engine.Editor.Console.WriteLine(ConsoleEntry.Warning($"OpenGL Crash: {severity1} {glEnum} | {messageString} \n\n"));

			if (glEnum == GLEnum.DebugTypeError) {
				throw new Exception(messageString);
			}
		}
	}
}