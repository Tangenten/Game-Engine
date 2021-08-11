using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using RavContainers;

namespace RavUtilities {
	public static class GraphicsU {
		public static Graphic OpenPngFile(Stream fileStream) {
			Bitmap bitmap = new Bitmap(fileStream);
			BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			int bytesPerScanline = bitmapData.Stride;
			int numScanlines = bitmapData.Height;
			int numBytes = numScanlines * bytesPerScanline;
			IntPtr bytesPtr = bitmapData.Scan0;

			byte[] colorBytes = new byte[numBytes];
			Marshal.Copy(bytesPtr, colorBytes, 0, numBytes);
			bitmap.UnlockBits(bitmapData);

			// Pixelformat is supposed to be ARGB, as we set in PixelFormat
			// But because of little endianess we get BGRA, a Reverse, as the colors are parsed as UINT32
			// We want RGBA
			// So To convert BGRA to RGBA, we swap B and R,
			if (BitConverter.IsLittleEndian) {
				for (int i = 0; i < colorBytes.Length / 4; i++) {
					byte s = colorBytes[i * 4];                // 0. 4. 8
					colorBytes[i * 4] = colorBytes[i * 4 + 2]; // 2, 6, 10
					colorBytes[i * 4 + 2] = s;
				}
			} else {
				throw new Exception("Big Endian not Handled");
			}

			return new Graphic(bitmap.Width, bitmap.Height, PixelFormat.RGBA, BitDepth.EIGHT, colorBytes);
		}
	}

	public class Graphic {
		private byte[] pixels;

		public BitDepth BitDepth { get; }
		public PixelFormat PixelFormat { get; }
		public int Width { get; }
		public int Height { get; }

		public int BitDepthInBits => (int) this.BitDepth;
		public int BitDepthInBytes => (int) this.BitDepth / 8;

		public Graphic(int width, int height, PixelFormat pixelFormat, BitDepth bitDepth) {
			this.Width = width;
			this.Height = height;
			this.BitDepth = bitDepth;
			this.PixelFormat = pixelFormat;
			this.pixels = new byte[this.Height * this.Width * this.BitDepthInBytes];
		}

		public Graphic(int width, int height, PixelFormat pixelFormat, BitDepth bitDepth, byte[] bytes) {
			this.Width = width;
			this.Height = height;
			this.BitDepth = bitDepth;
			this.PixelFormat = pixelFormat;
			this.pixels = bytes;
		}

		public byte[] GetPixels(BitDepth bitDepth, PixelFormat pixelFormat) {
			if (this.BitDepth == bitDepth && this.PixelFormat == pixelFormat) {
				return this.pixels;
			}

			throw new Exception("Unsupported Color format");
		}

		public void SetPixel(int x, int y, Color4 color) {
			if (this.BitDepth == BitDepth.EIGHT) {
				int offset = y * this.Width + x;
				this.pixels[offset + 0] = (byte) (color.R * 255f);
				this.pixels[offset + 1] = (byte) (color.G * 255f);
				this.pixels[offset + 2] = (byte) (color.B * 255f);
				this.pixels[offset + 3] = (byte) (color.A * 255f);
			} else {
				throw new Exception("Unsupported Color format");
			}
		}

		public Color4 GetPixel(int x, int y) {
			if (this.BitDepth == BitDepth.EIGHT && this.PixelFormat == PixelFormat.RGBA) {
				int offset = y * this.Width + x;
				return new Color4(this.pixels[offset + 0], this.pixels[offset + 1], this.pixels[offset + 2], this.pixels[offset + 3]);
			}
			throw new Exception("Unsupported Color format");
		}
	}

	public enum BitDepth {
		EIGHT = 8,
		SIXTEEN = 16
	}

	public enum PixelFormat {
		RGBA
	}

	public class Transform2D {
		private Matrix4x4 translationMatrix;
		private Matrix4x4 shearMatrix;
		private Matrix4x4 scaleMatrix;
		private Matrix4x4 rotationMatrix;

		private Matrix4x4 matrix;
		private bool shouldRebuild;
		private float radian;
		private Vector3 rotationOrigin;

		public Transform2D() {
			this.rotationMatrix = Matrix4x4.Identity;
			this.scaleMatrix = Matrix4x4.Identity;
			this.shearMatrix = Matrix4x4.Identity;
			this.translationMatrix = Matrix4x4.Identity;

			this.matrix = Matrix4x4.Identity;
			this.shouldRebuild = true;
			this.radian = 0f;
			this.rotationOrigin = new Vector3(0.5f, 0.5f, 0.5f);
		}

		public void MoveTo(Vector2 moveto) {
			this.translationMatrix.M41 = moveto.X;
			this.translationMatrix.M42 = moveto.Y;
			this.shouldRebuild = true;
		}

		public void MoveTo(Vector3 moveto) {
			this.translationMatrix.M41 = moveto.X;
			this.translationMatrix.M42 = moveto.Y;
			this.translationMatrix.M43 = moveto.Z;
			this.shouldRebuild = true;
		}

		public void MoveBy(Vector2 moveBy) {
			this.translationMatrix.M41 += moveBy.X;
			this.translationMatrix.M42 += moveBy.Y;
			this.shouldRebuild = true;
		}

		public void MoveBy(Vector3 moveBy) {
			this.translationMatrix.M41 += moveBy.X;
			this.translationMatrix.M42 += moveBy.Y;
			this.translationMatrix.M43 += moveBy.Z;
			this.shouldRebuild = true;
		}

		public void ScaleTo(Vector2 scaleTo) {
			this.scaleMatrix.M11 = scaleTo.X;
			this.scaleMatrix.M22 = scaleTo.Y;
			this.shouldRebuild = true;
		}

		public void ScaleTo(Vector3 scaleTo) {
			this.scaleMatrix.M11 = scaleTo.X;
			this.scaleMatrix.M22 = scaleTo.Y;
			this.scaleMatrix.M33 = scaleTo.Z;
			this.shouldRebuild = true;
		}

		public void ScaleBy(Vector2 scaleBy) {
			this.scaleMatrix.M11 += scaleBy.X;
			this.scaleMatrix.M22 += scaleBy.Y;
			this.shouldRebuild = true;
		}

		public void ScaleBy(Vector3 scaleBy) {
			this.scaleMatrix.M11 += scaleBy.X;
			this.scaleMatrix.M22 += scaleBy.Y;
			this.scaleMatrix.M33 += scaleBy.Z;
			this.shouldRebuild = true;
		}

		public void RotateTo(float radian) {
			this.radian = radian;
			this.shouldRebuild = true;
		}

		public void RotateBy(float radian) {
			this.radian += radian;
			this.shouldRebuild = true;
		}

		public void ShearTo(Vector2 shearTo) {
			this.shearMatrix.M12 = shearTo.X;
			this.shearMatrix.M21 = shearTo.Y;
			this.shouldRebuild = true;
		}

		public void ShearBy(Vector2 shearBy) {
			this.shearMatrix.M12 += shearBy.X;
			this.shearMatrix.M21 += shearBy.Y;
			this.shouldRebuild = true;
		}

		public Vector2 GetPosition2D() { return new Vector2(this.translationMatrix.M13, this.translationMatrix.M23); }

		public Vector3 GetPosition3D() { return new Vector3(this.translationMatrix.M13, this.translationMatrix.M23, this.translationMatrix.M33); }

		public float GetRotation() { return this.radian; }

		public Matrix4x4 GetMatrix() {
			this.Build();
			return this.matrix;
		}

		public Matrix4x4 GetInverseMatrix() {
			this.Build();
			Matrix4x4 inverseMatrix;
			Matrix4x4.Invert(this.matrix, out inverseMatrix);
			return inverseMatrix;
		}

		public void Invert() {
			this.Build();
			Matrix4x4.Invert(this.matrix, out this.matrix);
		}

		private bool Build() {
			if (this.shouldRebuild) {
				float c = MathF.Cos(this.radian);
				float s = MathF.Sin(this.radian);

				this.rotationMatrix.M22 = c;
				this.rotationMatrix.M23 = s;
				this.rotationMatrix.M32 = -s;
				this.rotationMatrix.M33 = c;

				this.matrix = this.translationMatrix * this.rotationMatrix * this.scaleMatrix * this.shearMatrix;

				this.shouldRebuild = false;
				return true;
			}

			return false;
		}

		public Vector2 Apply(Vector2 vector) {
			this.Build();
			vector -= this.rotationOrigin.XY();
			vector = Vector2.Transform(vector, this.matrix);
			vector += this.rotationOrigin.XY();

			return vector;
		}

		public void Apply(ref Vector2 vector) {
			this.Build();
			vector -= this.rotationOrigin.XY();
			vector = Vector2.Transform(vector, this.matrix);
			vector += this.rotationOrigin.XY();
		}

		public Vector3 Apply(Vector3 vector) {
			this.Build();
			vector -= this.rotationOrigin;
			vector = Vector3.Transform(vector, this.matrix);
			vector += this.rotationOrigin;

			return vector;
		}

		public void Apply(ref Vector3 vector) {
			this.Build();
			vector -= this.rotationOrigin;
			vector = Vector3.Transform(vector, this.matrix);
			vector += this.rotationOrigin;
		}
	}
}