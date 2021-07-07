using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Silk.NET.Core;
using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using VideoMode = Silk.NET.Windowing.VideoMode;

namespace RavEngine {
	public class WindowE : EngineCore {
		private IWindow window;

		public string Title {
			get { return this.window.Title; }
			set { this.window.Title = value; }
		}

		public Vector2 Position {
			get { return new Vector2(this.window.Position.X, this.window.Position.Y); }
			set { this.window.Position = new Vector2D<int>((int) value.X, (int) value.Y); }
		}

		public Vector2 Size {
			get { return new Vector2(this.window.Size.X, this.window.Size.Y); }
			set { this.window.Size = new Vector2D<int>((int) value.X, (int) value.Y); }
		}

		public Vector2 SizeWithBorders {
			get { return (Vector2) this.window.GetFullSize(); }
		}

		public bool VSync {
			get { return this.window.VSync; }
			set { this.window.VSync = value; }
		}

		public WindowState WindowState {
			get { return this.window.WindowState; }
			set { this.window.WindowState = value; }
		}

		public double TargetFps {
			get { return this.window.FramesPerSecond; }
			set { this.window.FramesPerSecond = value; }
		}

		public WindowBorder WindowBorder {
			get { return this.window.WindowBorder; }
			set { this.window.WindowBorder = value; }
		}

		public event Action<string[]>? OnFileDrop;
		public event Action<Vector2D<int>> OnMove;

		public IInputContext InputContext { get; private set; }
		public GL GLContext { get; private set; }
		public IView View => this.window;

		public WindowE() {
		}

		internal override void Start() {
			WindowOptions windowOptions = WindowOptions.Default;
			windowOptions.Title = "Rav Engine";
			windowOptions.API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.Debug, new APIVersion(Version.Parse("4.6")));

			this.window = Window.Create(windowOptions);

			this.window.Initialize();
			this.window.SetDefaultIcon();

			this.window.FileDrop += this.OnFileDrop;
			this.window.Move += this.OnMove;

			this.InputContext = this.window.CreateInput();
			this.GLContext = this.window.CreateOpenGL();
		}

		internal override void Stop() {
			this.window.Reset();
		}

		internal override void Update() {
			this.window.DoRender();
			this.window.DoEvents();
		}

		internal override void Reset() {
		}

		public void SetAspectRatio(int numer, int denom) {
			unsafe {
				Silk.NET.Windowing.Glfw.GlfwWindowing.GetExistingApi(this.window).SetWindowAspectRatio((WindowHandle*) this.window.Handle, numer, denom);
			}
		}

		public void SetInputMode(CursorStateAttribute cursorStateAttribute, CursorModeValue cursorModeValue) {
			unsafe {
				Silk.NET.Windowing.Glfw.GlfwWindowing.GetExistingApi(this.window).SetInputMode((WindowHandle*) this.window.Handle, cursorStateAttribute, cursorModeValue);
			}
		}

		public void SetWindowIcon(RawImage image) {
			this.window.SetWindowIcon(ref image);
		}

		public string GetClipboard() {
			unsafe {
				return Silk.NET.Windowing.Glfw.GlfwWindowing.GetExistingApi(this.window).GetClipboardString((WindowHandle*) this.window.Handle);
			}
		}

		public void SetClipboard(string clip) {
			unsafe {
				Silk.NET.Windowing.Glfw.GlfwWindowing.GetExistingApi(this.window).SetClipboardString((WindowHandle*) this.window.Handle, clip);
			}
		}

		public void SetCursor(Image img, Vector2 hotSpot) {
			unsafe {
				IntPtr imgPtr = new IntPtr();
				Marshal.StructureToPtr(img, imgPtr, false);
				Cursor* cursor = Silk.NET.Windowing.Glfw.GlfwWindowing.GetExistingApi(this.window).CreateCursor((Image*) imgPtr, (int) hotSpot.X, (int) hotSpot.Y);
				Silk.NET.Windowing.Glfw.GlfwWindowing.GetExistingApi(this.window).SetCursor((WindowHandle*) this.window.Handle, cursor);
			}
		}

		public void SetCursor(CursorShape cursorShape) {
			unsafe {
				Cursor* cursor = Silk.NET.Windowing.Glfw.GlfwWindowing.GetExistingApi(this.window).CreateStandardCursor(cursorShape);
				Silk.NET.Windowing.Glfw.GlfwWindowing.GetExistingApi(this.window).SetCursor((WindowHandle*) this.window.Handle, cursor);
			}
		}

		public bool RawMouseSupported() {
			return Silk.NET.Windowing.Glfw.GlfwWindowing.GetExistingApi(this.window).RawMouseMotionSupported();
		}

		public void SetCursorPosition(Vector2 position) {
			unsafe {
				Silk.NET.Windowing.Glfw.GlfwWindowing.GetExistingApi(this.window).SetCursorPos((WindowHandle*) this.window.Handle,position.X, position.Y);
			}
		}

		public void RequestWindowAttention() {
			unsafe {
				Silk.NET.Windowing.Glfw.GlfwWindowing.GetExistingApi(this.window).RequestWindowAttention((WindowHandle*) this.window.Handle);
			}
		}

		public bool Open => !this.window.IsClosing;
		public bool Visible => this.window.IsVisible;
		public VideoMode Monitor => this.window.VideoMode;
	}
}
