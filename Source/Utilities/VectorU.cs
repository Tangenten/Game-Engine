using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace RavUtilities {
	public static class VectorU {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float DotProduct(Vector2 v1, Vector2 v2) {
			return v1.X * v2.X + v1.Y * v2.Y;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float DotProduct(Vector3 v1, Vector3 v2) {
			return v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Length(Vector2 v) {
			return MathF.Sqrt(v.X * v.X + v.Y * v.Y);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float Length(Vector3 v) {
			return MathF.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float LengthBetweenPoints(Vector2 v1, Vector2 v2) {
			return MathF.Sqrt((v2.X - v1.X) * (v2.X - v1.X) + (v2.Y - v1.Y) * (v2.Y - v1.Y));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float LengthBetweenPoints(Vector3 v1, Vector3 v2) {
			return MathF.Sqrt((v2.X - v1.X) * (v2.X - v1.X) + (v2.Y - v1.Y) * (v2.Y - v1.Y) + (v2.Z - v1.Z) * (v2.Z - v1.Z));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float RadianBetweenPoints(Vector2 v1, Vector2 v2) {
			return MathF.Atan2(v2.Y - v1.Y, v2.X - v1.X);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float DegreeBetweenPoints(Vector2 v1, Vector2 v2) {
			return MathF.Atan2(v2.Y - v1.Y, v2.X - v1.X) * (180f / MathF.PI);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float VectorToDegree(Vector2 v) {
			return MathF.Atan2(v.Y, v.X) * (180f / MathF.PI);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float VectorToRadian(Vector2 v) {
			return MathF.Atan2(v.Y, v.X);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 DegreeToVector(float degree) {
			float radian = degree * (MathF.PI / 180f);
			return new Vector2(MathF.Cos(radian), MathF.Sin(radian));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 RadianToVector(float radians) {
			return new Vector2(MathF.Cos(radians), MathF.Sin(radians));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 Divide(Vector2 v1, Vector2 v2) {
			return new Vector2(v1.X / v2.X, v1.Y / v2.Y);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 Divide(Vector3 v1, Vector3 v2) {
			return new Vector3(v1.X / v2.X, v1.Y / v2.Y, v1.Z / v2.Z);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 Multiply(Vector2 v1, Vector2 v2) {
			return new Vector2(v1.X * v2.X, v1.Y * v2.Y);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 Multiply(Vector3 v1, Vector3 v2) {
			return new Vector3(v1.X * v2.X, v1.Y * v2.Y, v1.Z * v2.Z);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 LineNormal(Vector2 lineStart, Vector2 lineEnd) {
			float dx = lineEnd.X - lineStart.X;
			float dy = lineEnd.Y - lineStart.Y;

			return new Vector2(-dy, dx);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 NormalizeVector(Vector2 v) {
			float length = Length(v);
			return new Vector2(v.X / length, v.Y / length);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 NormalVectorBetweenPoints(Vector2 v1, Vector2 v2) {
			return NormalizeVector(v2 - v1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float FastLength(Vector2 v) {
			return 1.0f / MathU.InverseSqrtFast(v.X * v.X + v.Y * v.Y);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 FastNormalizeVector(Vector2 v) {
			float inversedMagnitude = MathU.InverseSqrtFast(Length(v));
			return v * inversedMagnitude;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 RotatePointAroundPoint(Vector2 point, Vector2 origin, float radians) {
			float rotX = MathF.Cos(radians) * (point.X - origin.X) - MathF.Sin(radians) * (point.Y - origin.Y) + origin.X;
			float rotY = MathF.Sin(radians) * (point.X - origin.X) + MathF.Cos(radians) * (point.Y - origin.Y) + origin.Y;
			return new Vector2(rotX, rotY);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 VectorBetweenPoints(Vector2 v1, Vector2 v2) {
			return v2 - v1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 FlipY(Vector2 vec2, float yResolution) {
			vec2.Y = MathF.Abs(vec2.Y - yResolution);
			return vec2;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 FlipY(Vector3 vec3, float yResolution) {
			vec3.Y = MathF.Abs(vec3.Y - yResolution);
			return vec3;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector2 ToVector2(Vector3 v) {
			return new Vector2(v.X, v.Y);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Vector3 ToVector3(Vector2 v) {
			return new Vector3(v.X, v.Y, 0f);
		}
	}
}
