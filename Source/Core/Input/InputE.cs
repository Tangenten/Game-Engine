using System;
using System.Collections.Generic;
using System.Numerics;
using RavContainers;
using Silk.NET.Input;

namespace RavEngine {
	public class InputE : EngineCore {
		private Dictionary<Key, RingArray<TimeStamp>> keyDownDict;
		private Dictionary<Key, RingArray<TimeStamp>> keyUpDict;

		private Dictionary<Mouse, bool> mouseDownDict;
		private Dictionary<Mouse, bool> mouseHeldDict;
		private Dictionary<Mouse, bool> mouseUpDict;

		private Dictionary<Gamepad, bool> gamepadDownDict;
		private Dictionary<Gamepad, bool> gamepadHeldDict;
		private Dictionary<Gamepad, bool> gamepadUpDict;
		private Vector2 gamepadLeftStick;
		private Vector2 gamepadRightStick;
		private float gamepadLeftTrigger;
		private float gamepadRightTrigger;

		private Vector2 mousePosition;
		private Vector2 mouseDelta;
		private bool mouseWheelHorizontalMoved;
		private bool mouseWheelVerticalMoved;
		private float mouseWheelHorizontalTicks;
		private float mouseWheelVerticalTicks;

		public InputE() {
			this.keyDownDict = new Dictionary<Key, RingArray<TimeStamp>>();
			this.keyUpDict = new Dictionary<Key, RingArray<TimeStamp>>();

			this.mouseDownDict = new Dictionary<Mouse, bool>();
			this.mouseHeldDict = new Dictionary<Mouse, bool>();
			this.mouseUpDict = new Dictionary<Mouse, bool>();

			this.gamepadDownDict = new Dictionary<Gamepad, bool>();
			this.gamepadHeldDict = new Dictionary<Gamepad, bool>();
			this.gamepadUpDict = new Dictionary<Gamepad, bool>();
		}

		internal override void Start() {
			foreach (IKeyboard inputContextKeyboard in Engine.Window.InputContext.Keyboards) {
				inputContextKeyboard.KeyDown += this.KeyDown;
				inputContextKeyboard.KeyUp += this.KeyUp;
			}

			foreach (IMouse inputContextMouse in Engine.Window.InputContext.Mice) {
				inputContextMouse.MouseDown += this.MouseButtonDown;
				inputContextMouse.MouseUp += this.MouseButtonUp;
				inputContextMouse.Scroll += this.MouseWheelScrolled;
				inputContextMouse.MouseMove += this.MouseMoved;
			}

			foreach (IGamepad inputContextGamepad in Engine.Window.InputContext.Gamepads) {
				inputContextGamepad.ButtonDown += this.GamepadButtonDown;
				inputContextGamepad.ButtonUp += this.GamepadButtonUp;
				inputContextGamepad.ThumbstickMoved += this.GamepadThumbstickMoved;
				inputContextGamepad.TriggerMoved += this.GamepadTriggerMoved;
			}

			foreach (Key key in (Key[]) Enum.GetValues(typeof(Key))) {
				this.keyDownDict[key] = new RingArray<TimeStamp>(16, new TimeStamp());
				this.keyUpDict[key] = new RingArray<TimeStamp>(16, new TimeStamp());
			}
		}

		internal override void Stop() { }

		internal override void Update() {
			this.mouseDownDict.Clear();
			this.mouseUpDict.Clear();

			this.gamepadDownDict.Clear();
			this.gamepadUpDict.Clear();

			this.mouseDelta = new Vector2(0f, 0f);
			this.mouseWheelHorizontalMoved = false;
			this.mouseWheelHorizontalTicks = 0;
			this.mouseWheelVerticalMoved = false;
			this.mouseWheelVerticalTicks = 0;
		}

		internal override void Reset() { }

		public bool Key(params (Key, State)[] keys) {
			if (!Engine.Editor.Game.WindowFocused || keys.Length == 0) return false;

			foreach ((Key button, State state) in keys) {
				switch (state) {
					case State.DOWN:
						if (this.keyDownDict[button].PeekData()?.FrameStamp() != Engine.Time.ElapsedFrames) {
							return false;
						}
						break;
					case State.UP:
						if (this.keyUpDict[button].PeekData()?.FrameStamp() != Engine.Time.ElapsedFrames) {
							return false;
						}
						break;
					case State.HELD:
						if (this.keyDownDict[button].PeekData()?.FrameStamp() <= this.keyUpDict[button].PeekData()?.FrameStamp()) {
							return false;
						}
						break;
				}
			}

			return true;
		}

		public bool KeyHeldGameTime(Key heldKey, out float time) {
			time = 0f;
			if (!Engine.Editor.Game.WindowFocused) return false;

			if (this.keyDownDict[heldKey].PeekData()?.FrameStamp() > this.keyUpDict[heldKey].PeekData()?.FrameStamp()) {
				time = (float) this.keyDownDict[heldKey].PeekData()?.GameTimeSinceStamped()!;
				return true;
			}

			return false;
		}

		public bool KeyHeldGameTime(Key heldKey, float time) {
			if (!Engine.Editor.Game.WindowFocused) return false;

			if (this.keyDownDict[heldKey].PeekData()?.FrameStamp() > this.keyUpDict[heldKey].PeekData()?.FrameStamp()) {
				float timestampTime = (float) this.keyDownDict[heldKey].PeekData()?.GameTimeSinceStamped()!;
				return time <= timestampTime;
			}

			return false;
		}

		public bool KeyHeldRealTime(Key heldKey, out float time) {
			time = 0f;
			if (!Engine.Editor.Game.WindowFocused) return false;

			if (this.keyDownDict[heldKey].PeekData()?.FrameStamp() > this.keyUpDict[heldKey].PeekData()?.FrameStamp()) {
				time = (float) this.keyDownDict[heldKey].PeekData()?.RealTimeSinceStamp()!;
				return true;
			}

			return false;
		}

		public bool KeyHeldRealTime(Key heldKey, float time) {
			if (!Engine.Editor.Game.WindowFocused) return false;

			if (this.keyDownDict[heldKey].PeekData()?.FrameStamp() > this.keyUpDict[heldKey].PeekData()?.FrameStamp()) {
				float timestampTime = (float) this.keyDownDict[heldKey].PeekData()?.RealTimeSinceStamp()!;
				return time <= timestampTime;
			}

			return false;
		}

		public bool MultiKeyClick((Key, State) key, int clicks, float time) {
			RingArray<TimeStamp> ringArray = this.keyDownDict[key.Item1];
			TimeStamp[] timeStamps = ringArray.PeekData(clicks);

			double delta = 0f;
			for (int i = 1; i < timeStamps.Length; i++) {
				if (timeStamps[i].RealTimeSinceStamp() >= time || timeStamps[i - 1].RealTimeSinceStamp() >= time) {
					return false;
				}

				double timeBetween = timeStamps[i].RealTimeBetweenStamps(timeStamps[i - 1]);
				delta += timeBetween;
			}
			bool firstPass = delta <= time && this.Key(key);

			// Second pass, Looks at previous click and denies if to close, Janky?
			TimeStamp[] timeStamps2 = ringArray.PeekData(clicks + 1);
			for (int i = 1; i < timeStamps2.Length - 1; i++) {
				if (timeStamps2[i].RealTimeSinceStamp() >= time || timeStamps2[i - 1].RealTimeSinceStamp() >= time) {
					return false;
				}
			}

			return firstPass;
		}

		public bool Gamepad(params (Gamepad, State)[] buttons) {
			if (!Engine.Editor.Game.WindowFocused || buttons.Length == 0) return false;

			foreach ((Gamepad button, State state) in buttons) {
				switch (state) {
					case State.DOWN:
						if (this.gamepadDownDict.ContainsKey(button)) {
							if (!this.gamepadDownDict[button]) {
								return false;
							}
						} else {
							return false;
						}

						break;
					case State.HELD:
						if (this.gamepadHeldDict.ContainsKey(button)) {
							if (!this.gamepadHeldDict[button]) {
								return false;
							}
						} else {
							return false;
						}

						break;
					case State.UP:
						if (this.gamepadUpDict.ContainsKey(button)) {
							if (!this.gamepadUpDict[button]) {
								return false;
							}
						} else {
							return false;
						}

						break;
				}
			}

			return true;
		}

		public bool Triggers(Trigger trigger, float min, float max) {
			if (!Engine.Editor.Game.WindowFocused) return false;

			switch (trigger) {
				case Trigger.LEFT:      return this.gamepadLeftTrigger > min && this.gamepadLeftTrigger < max;
				case Trigger.RIGHT:     return this.gamepadRightTrigger > min && this.gamepadRightTrigger < max;
				case Trigger.LEFTRIGHT: return this.gamepadLeftTrigger > min && this.gamepadLeftTrigger < max && this.gamepadRightTrigger > min && this.gamepadRightTrigger < max;
			}

			return false;
		}

		public bool Mouse(params (Mouse, State)[] buttons) {
			if (!Engine.Editor.Game.WindowFocused || buttons.Length == 0) return false;

			foreach ((Mouse button, State state) in buttons) {
				switch (state) {
					case State.DOWN:
						if (this.mouseDownDict.ContainsKey(button)) {
							if (!this.mouseDownDict[button]) {
								return false;
							}
						} else {
							return false;
						}

						break;
					case State.HELD:
						if (this.mouseHeldDict.ContainsKey(button)) {
							if (!this.mouseHeldDict[button]) {
								return false;
							}
						} else {
							return false;
						}

						break;
					case State.UP:
						if (this.mouseUpDict.ContainsKey(button)) {
							if (!this.mouseUpDict[button]) {
								return false;
							}
						} else {
							return false;
						}

						break;
				}
			}

			return true;
		}

		public bool AnyKeyPressed() {
			if (!Engine.Editor.Game.WindowFocused) return false;

			foreach (Key key in (Key[]) Enum.GetValues(typeof(Key))) {
				if (this.keyDownDict[key].PeekData()?.FrameStamp() == Engine.Time.ElapsedFrames ||
					this.keyUpDict[key].PeekData()?.FrameStamp() == Engine.Time.ElapsedFrames) {
					return true;
				}
			}

			return false;
		}

		public bool AnyGamepadButtonPressed() {
			if (!Engine.Editor.Game.WindowFocused) return false;

			return this.gamepadDownDict.Keys.Count == 0 && this.gamepadUpDict.Keys.Count == 0 && this.gamepadHeldDict.Keys.Count == 0;
		}

		public bool AnyGamepadTriggerPressed() {
			if (!Engine.Editor.Game.WindowFocused) return false;

			return this.gamepadLeftTrigger == 0f && this.gamepadRightTrigger == 0f;
		}

		public bool AnyMouseButtonPressed() {
			if (!Engine.Editor.Game.WindowFocused) return false;

			return this.mouseDownDict.Keys.Count == 0 && this.mouseUpDict.Keys.Count == 0 && this.mouseHeldDict.Keys.Count == 0;
		}

		public Vector2 GetGamepadLeftThumbstick() { return this.gamepadLeftStick; }

		public Vector2 GetGamepadRightThumbstick() { return this.gamepadRightStick; }

		public Vector2 GetMouseWindowPosition() { return this.mousePosition; }

		public Vector2 GetMouseWindowDelta() { return this.mouseDelta; }

		public Vector2 GetMouseGamePosition() { return new Vector2(1, 1); }

		public Vector2 GetMouseGameDelta() { return new Vector2(1, 1); }

		public bool GetMouseHorizontalScroll(out float ticks) {
			ticks = this.mouseWheelHorizontalTicks;
			return this.mouseWheelHorizontalMoved;
		}

		public bool GetMouseVerticalScroll(out float ticks) {
			ticks = this.mouseWheelVerticalTicks;
			return this.mouseWheelVerticalMoved;
		}

		public string GetClipboard() { return Engine.Window.GetClipboard(); }

		public void SetClipboard(in string content) { Engine.Window.SetClipboard(content); }

		private void KeyDown(IKeyboard keyboard, Silk.NET.Input.Key key, int arg3) { this.keyDownDict[(Key) key].PushData(Engine.Time.GetTimeStamp()); }

		private void KeyUp(IKeyboard keyboard, Silk.NET.Input.Key key, int arg3) { this.keyUpDict[(Key) key].PushData(Engine.Time.GetTimeStamp()); }

		private void GamepadButtonDown(IGamepad gamepad, Button button) {
			this.gamepadDownDict[(Gamepad) button.Name] = true;
			this.gamepadHeldDict[(Gamepad) button.Name] = true;
		}

		private void GamepadButtonUp(IGamepad gamepad, Button button) {
			this.gamepadUpDict[(Gamepad) button.Name] = true;
			this.gamepadHeldDict[(Gamepad) button.Name] = false;
		}

		private void GamepadTriggerMoved(IGamepad gamepad, Silk.NET.Input.Trigger trigger) {
			if (trigger.Index == 0) {
				this.gamepadLeftTrigger = trigger.Position;
			} else if (trigger.Index == 1) {
				this.gamepadRightTrigger = trigger.Position;
			}
		}

		private void GamepadThumbstickMoved(IGamepad gamepad, Thumbstick thumbstick) {
			if (thumbstick.Index == 0) {
				this.gamepadLeftStick = new Vector2(thumbstick.X, thumbstick.Y);
			} else if (thumbstick.Index == 1) {
				this.gamepadRightStick = new Vector2(thumbstick.X, thumbstick.Y);
			}
		}

		private void MouseButtonDown(IMouse mouse, MouseButton mouseButton) {
			this.mouseDownDict[(Mouse) mouseButton] = true;
			this.mouseHeldDict[(Mouse) mouseButton] = true;
		}

		private void MouseButtonUp(IMouse mouse, MouseButton mouseButton) {
			this.mouseUpDict[(Mouse) mouseButton] = true;
			this.mouseHeldDict[(Mouse) mouseButton] = false;
		}

		private void MouseWheelScrolled(IMouse mouse, ScrollWheel scrollWheel) {
			if (scrollWheel.X != 0) {
				this.mouseWheelHorizontalMoved = true;
				this.mouseWheelHorizontalTicks = scrollWheel.X;
			}

			if (scrollWheel.Y != 0) {
				this.mouseWheelVerticalMoved = true;
				this.mouseWheelVerticalTicks = scrollWheel.Y;
			}
		}

		private void MouseMoved(IMouse mouse, Vector2 mousePos) {
			Vector2 v = mousePos;
			v.Y = Math.Abs(v.Y - Engine.Window.Size.Y);

			this.mouseDelta = this.mousePosition - v;
			this.mousePosition = v;
		}
	}

	public enum Key {
		Unknown = -1,
		Space = 32,
		Apostrophe = 39,
		Comma = 44,
		Minus = 45,
		Period = 46,
		Slash = 47,
		D0 = 48,
		Number0 = 48,
		Number1 = 49,
		Number2 = 50,
		Number3 = 51,
		Number4 = 52,
		Number5 = 53,
		Number6 = 54,
		Number7 = 55,
		Number8 = 56,
		Number9 = 57,
		Semicolon = 59,
		Equal = 61,
		A = 65,
		B = 66,
		C = 67,
		D = 68,
		E = 69,
		F = 70,
		G = 71,
		H = 72,
		I = 73,
		J = 74,
		K = 75,
		L = 76,
		M = 77,
		N = 78,
		O = 79,
		P = 80,
		Q = 81,
		R = 82,
		S = 83,
		T = 84,
		U = 85,
		V = 86,
		W = 87,
		X = 88,
		Y = 89,
		Z = 90,
		LeftBracket = 91,
		BackSlash = 92,
		RightBracket = 93,
		GraveAccent = 96,
		World1 = 161,
		World2 = 162,
		Escape = 256,
		Enter = 257,
		Tab = 258,
		Backspace = 259,
		Insert = 260,
		Delete = 261,
		Right = 262,
		Left = 263,
		Down = 264,
		Up = 265,
		PageUp = 266,
		PageDown = 267,
		Home = 268,
		End = 269,
		CapsLock = 280,
		ScrollLock = 281,
		NumLock = 282,
		PrintScreen = 283,
		Pause = 284,
		F1 = 290,
		F2 = 291,
		F3 = 292,
		F4 = 293,
		F5 = 294,
		F6 = 295,
		F7 = 296,
		F8 = 297,
		F9 = 298,
		F10 = 299,
		F11 = 300,
		F12 = 301,
		F13 = 302,
		F14 = 303,
		F15 = 304,
		F16 = 305,
		F17 = 306,
		F18 = 307,
		F19 = 308,
		F20 = 309,
		F21 = 310,
		F22 = 311,
		F23 = 312,
		F24 = 313,
		F25 = 314,
		Keypad0 = 320,
		Keypad1 = 321,
		Keypad2 = 322,
		Keypad3 = 323,
		Keypad4 = 324,
		Keypad5 = 325,
		Keypad6 = 326,
		Keypad7 = 327,
		Keypad8 = 328,
		Keypad9 = 329,
		KeypadDecimal = 330,
		KeypadDivide = 331,
		KeypadMultiply = 332,
		KeypadSubtract = 333,
		KeypadAdd = 334,
		KeypadEnter = 335,
		KeypadEqual = 336,
		ShiftLeft = 340,
		ControlLeft = 341,
		AltLeft = 342,
		SuperLeft = 343,
		ShiftRight = 344,
		ControlRight = 345,
		AltRight = 346,
		SuperRight = 347,
		Menu = 348
	}

	public enum Mouse {
		Left = 0,
		Right = 1,
		Middle = 2,
		Button4 = 3,
		Button5 = 4,
		Button6 = 5,
		Button7 = 6,
		Button8 = 7,
		Button9 = 8,
		Button10 = 9,
		Button11 = 10,
		Button12 = 11
	}

	public enum Gamepad {
		A = 0,
		B = 1,
		X = 2,
		Y = 3,
		LeftBumper = 4,
		RightBumper = 5,
		Back = 6,
		Start = 7,
		Home = 8,
		LeftStick = 9,
		RightStick = 10,
		DPadUp = 11,
		DPadRight = 12,
		DPadDown = 13,
		DPadLeft = 14
	}

	public enum Trigger {
		LEFT,
		RIGHT,
		LEFTRIGHT
	}

	public enum State {
		DOWN,
		UP,
		HELD
	}
}
