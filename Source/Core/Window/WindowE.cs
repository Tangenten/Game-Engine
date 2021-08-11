using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using RavUtilities;
using Silk.NET.Core;
using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Silk.NET.Windowing.Glfw;
using PixelFormat = RavUtilities.PixelFormat;

namespace RavEngine {
	public class WindowE : EngineCore {
		private IWindow window;

		[ConsoleCommand("WINDOW_TITLE")]
		public string Title {
			get => this.window.Title;
			set {
				this.window.Title = value;
				Engine.Settings.Set("WINDOW_TITLE", this.window.Title);
			}
		}

		public Vector2 Position {
			get => new Vector2(this.window.Position.X, this.window.Position.Y);
			set => this.window.Position = new Vector2D<int>((int) value.X, (int) value.Y);
		}

		[ConsoleCommand("WINDOW_SIZE")]
		public Vector2 Size {
			get => new Vector2(this.window.Size.X, this.window.Size.Y);
			set => this.window.Size = new Vector2D<int>((int) value.X, (int) value.Y);
		}

		[ConsoleCommand("VSYNC")]
		public bool VSync {
			get => this.window.VSync;
			set {
				this.window.VSync = value;
				Engine.Settings.Set("WINDOW_VSYNC", value);
			}
		}

		[ConsoleCommand("WINDOW_STATE")]
		public WindowState State {
			get => this.window.WindowState;
			set => this.window.WindowState = value;
		}

		[ConsoleCommand("FPS")]
		public double Fps {
			get => this.window.FramesPerSecond;
			set {
				this.window.FramesPerSecond = value;
				this.window.UpdatesPerSecond = value;
				Engine.Settings.Set("WINDOW_FPS", value);
			}
		}

		[ConsoleCommand("WINDOW_BORDER")]
		public WindowBorder Border {
			get => this.window.WindowBorder;
			set {
				this.window.WindowBorder = value;
				Engine.Settings.Set("WINDOW_BORDER", value);
			}
		}

		public event Action<string[]>? OnFileDrop;
		public event Action<Vector2D<int>> OnMove;
		public event Action<bool> OnFocusChanged;
		public event Action<WindowState> OnStateChanged;
		public event Action<Vector2D<int>> OnFramebufferResized;
		public event Action<Vector2D<int>> OnWindowResize;

		internal IInputContext InputContext { get; private set; }
		internal GL GLContext { get; private set; }
		internal IView View => this.window;

		public bool IsOpen => !this.window.IsClosing;
		public bool IsVisible => this.window.IsVisible;
		[ConsoleCommand("MONITOR_RESOLUTION")]
		public Vector2 MonitorResolution => (Vector2) this.window.VideoMode.Resolution;
		[ConsoleCommand("MONITOR_REFRESH_RATE")]
		public int MonitorRefreshRate => (int) this.window.VideoMode.RefreshRate;
		[ConsoleCommand("WINDOW_SIZE_BORDERS")]
		public Vector2 SizeWithBorders => (Vector2) this.window.GetFullSize();

		public WindowE() {
			WindowOptions windowOptions = WindowOptions.Default;

			#if DEBUG
			windowOptions.API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.Debug, new APIVersion(Version.Parse("4.6")));
			#else
			windowOptions.API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.Default, new APIVersion(Version.Parse("4.6")));
			#endif

			if (Engine.Settings.TryGet("WINDOW_SIZE", out Vector2 size)) {
				windowOptions.Size = new Vector2D<int>((int) size.X, (int) size.Y);
			}
			if (Engine.Settings.TryGet("WINDOW_POSITION", out Vector2 position)) {
				windowOptions.Position = new Vector2D<int>((int) position.X, (int) position.Y);
			}
			if (Engine.Settings.TryGet("WINDOW_STATE", out WindowState state)) {
				windowOptions.WindowState = state;
			}
			if (Engine.Settings.TryGet("WINDOW_TITLE", out string title)) {
				windowOptions.Title = title;
			}
			if (Engine.Settings.TryGet("WINDOW_VSYNC", out bool vsync)) {
				windowOptions.VSync = vsync;
			}
			if (Engine.Settings.TryGet("WINDOW_BORDER", out WindowBorder border)) {
				windowOptions.WindowBorder = border;
			}
			if (Engine.Settings.TryGet("WINDOW_FPS", out double fps)) {
				windowOptions.UpdatesPerSecond = fps;
				windowOptions.FramesPerSecond = fps;
			}

			GlfwWindowing.Use();
			this.window = Window.Create(windowOptions);
			this.window.Initialize();
			this.window.SetDefaultIcon();

			this.window.FileDrop += strings => this.OnFileDrop?.Invoke(strings);
			this.window.Move += vector2D => this.OnMove?.Invoke(vector2D);
			this.window.FocusChanged += b => this.OnFocusChanged?.Invoke(b);
			this.window.StateChanged += state => this.OnStateChanged?.Invoke(state);
			this.window.FramebufferResize += vector2D => this.OnFramebufferResized?.Invoke(vector2D);
			this.window.Resize += vector2D => this.OnWindowResize?.Invoke(vector2D);

			this.InputContext = this.window.CreateInput();
			this.GLContext = this.window.CreateOpenGL();

			#if !PUBLISH
			Engine.Resources.CreateReferenceAsync<FileStreamResource>("FaceIcon.png", reference => {
				this.SetIcon(GraphicsU.OpenPngFile(reference.Resource.FileStream));
				this.SetCursor(CursorShape.Crosshair);
			});
			#endif

			this.OnFramebufferResized += vector2D => this.GLContext.Viewport(vector2D);

			this.OnWindowResize += vector2D => {
				Engine.Threading.LaunchJob(() => Engine.Settings.Set("WINDOW_SIZE", (Vector2) vector2D));
			};

			this.OnMove += vector2D => {
				Engine.Threading.LaunchJob(() => Engine.Settings.Set("WINDOW_POSITION", (Vector2) vector2D));
			};

			this.OnStateChanged += state => {
				Engine.Threading.LaunchJob(() => Engine.Settings.Set("WINDOW_STATE", state));
			};
		}

		internal override void Start() { }

		internal override void Stop() => this.window.Reset();

		internal override void Update() {
			this.window.DoEvents();
			this.window.DoUpdate();
		}

		internal override void Reset() { }

		internal void Render() => this.window.DoRender();

		[ConsoleCommand("SET_ASPECT_RATIO")] [MethodImpl(MethodImplOptions.Synchronized)]
		public unsafe void SetAspectRatio(int numer, int denom) {
			GlfwWindowing.GetExistingApi(this.window).SetWindowAspectRatio((WindowHandle*) this.window.Handle, numer, denom);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		internal unsafe void SetInputMode(CursorStateAttribute cursorStateAttribute, CursorModeValue cursorModeValue) {
			GlfwWindowing.GetExistingApi(this.window).SetInputMode((WindowHandle*) this.window.Handle, cursorStateAttribute, cursorModeValue);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void SetIcon(Graphic graphic) {
			RawImage image = new RawImage(graphic.Width, graphic.Height, graphic.GetPixels(BitDepth.EIGHT, PixelFormat.RGBA));
			this.window.SetWindowIcon(ref image);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		internal unsafe void SetCursor(Image img, Vector2 hotSpot) {
			IntPtr imgPtr = new IntPtr();
			Marshal.StructureToPtr(img, imgPtr, false);
			Cursor* cursor = GlfwWindowing.GetExistingApi(this.window).CreateCursor((Image*) imgPtr, (int) hotSpot.X, (int) hotSpot.Y);
			GlfwWindowing.GetExistingApi(this.window).SetCursor((WindowHandle*) this.window.Handle, cursor);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		internal unsafe void SetCursor(CursorShape cursorShape) {
			Cursor* cursor = GlfwWindowing.GetExistingApi(this.window).CreateStandardCursor(cursorShape);
			GlfwWindowing.GetExistingApi(this.window).SetCursor((WindowHandle*) this.window.Handle, cursor);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		internal unsafe void SetCursorPosition(Vector2 position) {
			GlfwWindowing.GetExistingApi(this.window).SetCursorPos((WindowHandle*) this.window.Handle, position.X, position.Y);
		}

		[ConsoleCommand("RAW_MOUSE_SUPPORT")] [MethodImpl(MethodImplOptions.Synchronized)]
		internal bool IsRawMouseSupported() {
			return GlfwWindowing.GetExistingApi(this.window).RawMouseMotionSupported();
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public unsafe void RequestAttention() {
			GlfwWindowing.GetExistingApi(this.window).RequestWindowAttention((WindowHandle*) this.window.Handle);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		internal unsafe string GetClipboard() {
			return GlfwWindowing.GetExistingApi(this.window).GetClipboardString((WindowHandle*) this.window.Handle);
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		internal unsafe void SetClipboard(string clip) {
			GlfwWindowing.GetExistingApi(this.window).SetClipboardString((WindowHandle*) this.window.Handle, clip);
		}
	}
}
