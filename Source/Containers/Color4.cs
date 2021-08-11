using System;
using System.Numerics;
using RavUtilities;

namespace RavContainers {
	public struct Color4 {
		private static readonly Vector4 clampMin = new Vector4(-1f, -1f, -1f, -1f);
		private static readonly Vector4 clampMax = new Vector4(1f, 1f, 1f, 1f);
		private static readonly Vector4 brightness = new Vector4(0.299f, 0.587f, 0.114f, 1f);

		public static readonly Color4 Black = new Color4(0f, 0f, 0f);
		public static readonly Color4 White = new Color4(1f);
		public static readonly Color4 DarkGrey = new Color4(0.75f, 0.75f, 0.75f);
		public static readonly Color4 Grey = new Color4(0.5f, 0.5f, 0.5f);
		public static readonly Color4 LightGrey = new Color4(0.25f, 0.25f, 0.25f);
		public static readonly Color4 Red = new Color4(1f, 0f, 0f);
		public static readonly Color4 Green = new Color4(0f, 1f, 0f);
		public static readonly Color4 Blue = new Color4(0f, 0f);
		public static readonly Color4 Yellow = new Color4(1f, 1f, 0f);
		public static readonly Color4 Magenta = new Color4(1f, 0f);
		public static readonly Color4 Cyan = new Color4(0f);
		public static readonly Color4 Transparent = new Color4(0f, 0f, 0f, 0f);

		private Vector4 vector;

		public float R {
			get => this.vector.X;
			set => this.vector.X = value;
		}

		public float G {
			get => this.vector.Y;
			set => this.vector.Y = value;
		}

		public float B {
			get => this.vector.Z;
			set => this.vector.Z = value;
		}

		public float A {
			get => this.vector.W;
			set => this.vector.W = value;
		}

		public Color4(float r = 1, float g = 1, float b = 1, float a = 1) { this.vector = new Vector4(r, g, b, a); }

		public Color4(byte r = 255, byte g = 255, byte b = 255, byte a = 255) { this.vector = new Vector4(r / 255f, g / 255f, b / 255f, a / 255f); }

		public Color4(ushort r = 65535, ushort g = 65535, ushort b = 65535, ushort a = 65535) { this.vector = new Vector4(r / 65535f, g / 65535f, b / 65535f, a / 65535f); }

		public Color4(Color4 color) { this.vector = new Vector4(color.R, color.G, color.B, color.A); }

		private Color4(Vector4 vector) { this.vector = vector; }

		public Color4(Span<byte> bytes) {
			if (bytes.Length == 4) {
				this.vector = new Vector4(bytes[0] / 255f, bytes[1] / 255f, bytes[2] / 255f, bytes[3] / 255f);
			} else if (bytes.Length == 8) {
				this.vector = new Vector4(BitConverter.ToUInt16(bytes.Slice(0, 2)) / 65535f, BitConverter.ToUInt16(bytes.Slice(2, 2)) / 65535f, BitConverter.ToUInt16(bytes.Slice(4, 2)) / 65535f, BitConverter.ToUInt16(bytes.Slice(6, 2)) / 65535f);
			} else if (bytes.Length == 3) {
				this.vector = new Vector4(bytes[0] / 255f, bytes[1] / 255f, bytes[2] / 255f, 1f);
			} else if (bytes.Length == 6) {
				this.vector = new Vector4(BitConverter.ToUInt16(bytes.Slice(0, 2)) / 65535f, BitConverter.ToUInt16(bytes.Slice(2, 2)) / 65535f, BitConverter.ToUInt16(bytes.Slice(4, 2)) / 65535f, 1f);
			} else {
				throw new Exception("Cant convert bytes to color");
			}
		}

		public void AdjustHue(float amount) {
			Color4 HSL = RGBToHSV(this);
			HSL.R += amount;
			HSL.R = Math.Clamp(HSL.R, 0f, 1f);
			this.vector = HSVToRGB(HSL).vector;
		}

		public float GetHue() {
			Color4 HSL = RGBToHSV(this);
			return HSL.R;
		}

		public void SetHue(float hue) {
			Color4 HSL = RGBToHSV(this);
			HSL.R = hue;
			HSL.R = Math.Clamp(HSL.R, 0f, 1f);
			this.vector = HSVToRGB(HSL).vector;
		}

		public void AdjustSaturation(float amount) {
			Color4 HSL = RGBToHSV(this);
			HSL.G += amount;
			HSL.G = Math.Clamp(HSL.G, 0f, 1f);
			this.vector = HSVToRGB(HSL).vector;
		}

		public float GetSaturation() {
			Color4 HSL = RGBToHSV(this);
			return HSL.G;
		}

		public void SetSaturation(float saturation) {
			Color4 HSL = RGBToHSV(this);
			HSL.G = saturation;
			HSL.G = Math.Clamp(HSL.G, 0f, 1f);
			this.vector = HSVToRGB(HSL).vector;
		}

		public void AdjustBrightness(float amount) {
			Color4 HSL = RGBToHSV(this);
			HSL.B += amount;
			HSL.B = Math.Clamp(HSL.B, 0f, 1f);
			this.vector = HSVToRGB(HSL).vector;
		}

		public float GetBrightness() {
			Color4 HSL = RGBToHSV(this);
			return HSL.B;
		}

		public void SetBrightness(float brightness) {
			Color4 HSL = RGBToHSV(this);
			HSL.B = brightness;
			HSL.B = Math.Clamp(HSL.B, 0f, 1f);
			this.vector = HSVToRGB(HSL).vector;
		}

		public static Color4[] GenerateColorTable(Color4 startColor4, int points = 8, int layers = 4, float randLayerScale = 0f, float randHueScale = 0f, float randSatScale = 0f, float randBrightScale = 0f) {
			Color4[] colors = new Color4[points * layers];
			for (int i = 0; i < layers; i++) {
				Color4 layerColor4 = startColor4 * (i + 1 + RandomU.Get(0f, randLayerScale));
				for (int j = 0; j < colors.Length / layers; j++) {
					layerColor4.AdjustHue(1f / points + RandomU.Get(0f, randHueScale));
					layerColor4.AdjustSaturation(RandomU.Get(0f, randSatScale));
					layerColor4.AdjustBrightness(RandomU.Get(0f, randBrightScale));
					colors[j * i] = new Color4(HSVToRGB(layerColor4));
				}
			}

			return colors;
		}

		public static float Brightness(Color4 color) { return Vector4.Dot(color.vector, brightness) / 2f; }

		public static Color4 Grayscale(Color4 color) {
			float brightness = Brightness(color);
			return new Color4(brightness, brightness, brightness);
		}

		public static Color4 GetRandomColor() { return new Color4(RandomU.Get(0f, 1f), RandomU.Get(0f, 1f), RandomU.Get(0f, 1f)); }

		public static Color4 GetRandomColorScaled(Color4 color, float scalar) {
			color.vector *= scalar;
			return color;
		}

		public static Color4 GetRandomFixedHue(float hue) { return HSVToRGB(new Color4(hue, RandomU.Get(0f, 1f), RandomU.Get(0f, 1f))); }

		public static Color4 GetRandomFixedSaturation(float saturation) { return HSVToRGB(new Color4(RandomU.Get(0f, 1f), saturation, RandomU.Get(0f, 1f))); }

		public static Color4 GetRandomFixedBrightness(float brightness) { return HSVToRGB(new Color4(RandomU.Get(0f, 1f), RandomU.Get(0f, 1f), brightness)); }

		public static Color4 RGBToHSV(Color4 color) {
			Vector4 K = new Vector4(0f, -1f / 3f, 2f / 3f, -1f);

			float px = color.G > color.B ? 1f : 0f;
			Vector4 p = Vector4.Lerp(new Vector4(color.B, color.G, K.W, K.Z), new Vector4(color.G, color.B, K.X, K.Y), px);
			float qx = color.R > p.X ? 1f : 0f;
			Vector4 q = Vector4.Lerp(new Vector4(p.X, p.Y, p.W, color.R), new Vector4(color.R, p.Y, p.Z, p.X), qx);

			float d = q.X - MathF.Min(q.W, q.Y);
			const float e = (float) 1.0e-10;
			return new Color4(new Vector4(MathF.Abs(q.Z + (q.W - q.Y) / (6f * d + e)), d / (q.X + e), q.X, color.A));
		}

		public static Color4 HSVToRGB(Color4 color) {
			Vector4 K = new Vector4(1f, 2f / 3f, 1f / 3f, 3f);
			Vector3 round = new Vector3(color.R, color.R, color.R) + new Vector3(K.X, K.Y, K.Z);
			round.X %= 1f;
			round.Y %= 1f;
			round.Z %= 1f;
			Vector3 p = Vector3.Abs(round * 6f - new Vector3(K.W, K.W, K.W));
			return new Color4(new Vector4(color.B * Vector3.Lerp(new Vector3(K.X, K.X, K.X), Vector3.Clamp(p - new Vector3(K.X, K.X, K.X), Vector3.Zero, Vector3.One), color.G), 1));
		}

		public static Color4 LerpRGB(Color4 color1, Color4 color2, float frac) { return new Color4(Vector4.Lerp(color1.vector, color2.vector, frac)); }

		public static Color4 LerpHSV(Color4 color1, Color4 color2, float frac) {
			color1 = RGBToHSV(color1);
			color2 = RGBToHSV(color2);
			return HSVToRGB(new Color4(Vector4.Lerp(color1.vector, color2.vector, frac)));
		}

		public static Color4 operator +(Color4 left, Color4 right) {
			Color4 color = new Color4(Vector4.Clamp(left.vector + right.vector, clampMin, clampMax));
			return color;
		}

		public static Color4 operator +(Color4 left, float right) {
			Color4 color = new Color4(Vector4.Clamp(left.vector + new Vector4(right), clampMin, clampMax));
			return color;
		}

		public static Color4 operator ++(Color4 left) {
			Color4 color = new Color4(Vector4.Clamp(left.vector + Vector4.One, clampMin, clampMax));
			return color;
		}

		public static Color4 operator -(Color4 left, Color4 right) {
			Color4 color = new Color4(Vector4.Clamp(left.vector - right.vector, clampMin, clampMax));
			return color;
		}

		public static Color4 operator -(Color4 left, float right) {
			Color4 color = new Color4(Vector4.Clamp(left.vector - new Vector4(right), clampMin, clampMax));
			return color;
		}

		public static Color4 operator --(Color4 left) {
			Color4 color = new Color4(Vector4.Clamp(left.vector - Vector4.One, clampMin, clampMax));
			return color;
		}

		public static Color4 operator *(Color4 left, Color4 right) {
			Color4 color = new Color4(Vector4.Clamp(left.vector * right.vector, clampMin, clampMax));
			return color;
		}

		public static Color4 operator *(Color4 left, float right) {
			Color4 color = new Color4(Vector4.Clamp(left.vector * new Vector4(right), clampMin, clampMax));
			return color;
		}

		public static Color4 operator /(Color4 left, Color4 right) {
			Color4 color = new Color4(Vector4.Clamp(left.vector / right.vector, clampMin, clampMax));
			return color;
		}

		public static Color4 operator /(Color4 left, float right) {
			Color4 color = new Color4(Vector4.Clamp(left.vector / new Vector4(right), clampMin, clampMax));
			return color;
		}

		public static bool operator ==(Color4 left, Color4 right) { return left.Equals(right); }

		public static bool operator !=(Color4 left, Color4 right) { return !left.Equals(right); }

		public override string ToString() { return $"[FastColor] R({this.R}) G({this.G}) B({this.B}) A({this.A})"; }

		public override bool Equals(object obj) { return obj is Color4 other && this.Equals(other); }

		private bool Equals(Color4 other) {
			const float tolerance = 0.001f;
			return MathF.Abs(this.R - other.R) < tolerance && MathF.Abs(this.G - other.G) < tolerance && MathF.Abs(this.B - other.B) < tolerance && MathF.Abs(this.A - other.A) < tolerance;
		}

		public static implicit operator Vector4(Color4 color) { return new Vector4(color.R, color.G, color.B, color.A); }

		public static implicit operator Color4(Vector4 vector) { return new Color4(vector.X, vector.Y, vector.Z, vector.W); }
	}
}