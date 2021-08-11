using System;
using System.Numerics;

namespace RavUtilities {
	public static class CollisionU {
		public static bool PointCircleCollision(Vector2 pPos, Vector2 cPos, float cRad) {
			float distanceBetweenPoints = VectorU.LengthBetweenPoints(cPos, pPos);
			return distanceBetweenPoints <= cRad;
		}

		public static bool PointCircleCollision(Vector2 pPos, Vector2 cPos, float cRad, out CollisionData data) {
			float distanceBetweenPoints = VectorU.LengthBetweenPoints(cPos, pPos);

			Vector2 angleBetweenCircles = cPos - pPos;
			angleBetweenCircles.X = MathF.Abs(angleBetweenCircles.X);
			angleBetweenCircles.Y = MathF.Abs(angleBetweenCircles.Y);
			angleBetweenCircles = VectorU.NormalizeVector(angleBetweenCircles);

			data.collisionNormal = angleBetweenCircles;
			data.collisionPosition = cPos + angleBetweenCircles * cRad;
			data.collisionDepth = MathF.Abs(distanceBetweenPoints - cRad);

			return distanceBetweenPoints <= cRad;
		}

		public static bool PointAABBCollision(Vector2 pPos, AABB aabb) { return pPos.X > aabb.Left && pPos.X < aabb.Left + aabb.Width && pPos.Y > aabb.Top && pPos.Y < aabb.Top + aabb.Height; }

		public static bool PointAABBCollision(Vector2 pPos, AABB aabb, out CollisionData data) {
			Vector2 rectCenter = aabb.Center();
			float dx = MathF.Max(MathF.Abs(pPos.X - rectCenter.X) - aabb.Width / 2, 0);
			float dy = MathF.Max(MathF.Abs(pPos.Y - rectCenter.Y) - aabb.Height / 2, 0);
			Vector2 collidePoint = new Vector2(dx, dy);
			data.collisionPosition = collidePoint;

			float distanceBetweenRectAndCollide = VectorU.LengthBetweenPoints(rectCenter, collidePoint);
			float distanceBetweenRectAndPoint = VectorU.LengthBetweenPoints(rectCenter, pPos);
			data.collisionDepth = MathF.Abs(distanceBetweenRectAndPoint - distanceBetweenRectAndCollide);

			if (collidePoint.X == aabb.Left) {
				data.collisionNormal = new Vector2(-1f, 0);
			} else if (collidePoint.X == aabb.Left + aabb.Width) {
				data.collisionNormal = new Vector2(1f, 0);
			} else if (collidePoint.Y == aabb.Top) {
				data.collisionNormal = new Vector2(0f, -1f);
			} else {
				data.collisionNormal = new Vector2(0f, 1f);
			}

			return pPos.X > aabb.Left && pPos.X < aabb.Left + aabb.Width && pPos.Y > aabb.Top && pPos.Y < aabb.Top + aabb.Height;
		}

		public static bool CircleCircleCollision(Vector2 c1Pos, float c1Rad, Vector2 c2Pos, float c2Rad) {
			float distanceBetweenCircles = VectorU.LengthBetweenPoints(c1Pos, c2Pos);
			float radiansPutTogether = c1Rad + c2Rad;

			return distanceBetweenCircles <= radiansPutTogether;
		}

		public static bool CircleCircleCollision(Vector2 c1Pos, float c1Rad, Vector2 c2Pos, float c2Rad, out CollisionData data) {
			float distanceBetweenCircles = VectorU.LengthBetweenPoints(c1Pos, c2Pos);
			float radiansPutTogether = c1Rad + c2Rad;

			Vector2 angleBetweenCircles = c1Pos - c2Pos;
			angleBetweenCircles.X = MathF.Abs(angleBetweenCircles.X);
			angleBetweenCircles.Y = MathF.Abs(angleBetweenCircles.Y);
			angleBetweenCircles = VectorU.NormalizeVector(angleBetweenCircles);

			data.collisionNormal = angleBetweenCircles;
			data.collisionPosition = c1Pos + angleBetweenCircles * c1Rad;
			data.collisionDepth = MathF.Abs(distanceBetweenCircles - radiansPutTogether);

			return distanceBetweenCircles <= radiansPutTogether;
		}

		public static bool AABBAABBCollision(AABB rect1, AABB rect2) { return rect1.Left < rect2.Left + rect2.Width && rect1.Left + rect1.Width > rect2.Left && rect1.Top < rect2.Top + rect2.Height && rect1.Top + rect1.Height > rect2.Top; }

		public static bool PointInsideVertices(Vector2 point, in Vector2[] vertices) {
			float x = point.X;
			float y = point.Y;
			int n = vertices.Length;
			bool inside = false;
			const bool includeEdges = true;

			float p1X = vertices[0].X;
			float p1Y = vertices[0].Y;
			for (int i = 1; i < n + 1; i++) {
				float p2X = vertices[i % n].X;
				float p2Y = vertices[i % n].Y;

				if (p1Y == p2Y) {
					if (y == p1Y) {
						if (MathF.Min(p1X, p2X) <= x && x <= MathF.Max(p1X, p2X)) {
							inside = includeEdges;
							break;
						}

						if (x < MathF.Min(p1X, p2X)) {
							inside = !inside;
						}
					}
				} else {
					if (MathF.Min(p1Y, p2Y) <= y && y <= MathF.Max(p1Y, p2Y)) {
						float xinters = (y - p1Y) * (p2X - p1X) / (p2Y - p1Y) + p1X;
						if (x == xinters) {
							inside = includeEdges;
						}

						if (x < xinters) {
							inside = !inside;
						}
					}
				}

				p1X = p2X;
				p1Y = p2Y;
			}

			return inside;
		}

		public static Vector2 LineIntersection(in Vector2 line1Start, in Vector2 line1End, in Vector2 line2Start, in Vector2 line2End) {
			float Slope(Vector2 p1, Vector2 p2) {
				if (p2.X == p1.X) {
					return p2.Y - p1.Y;
				}

				return (p2.Y - p1.Y) / (p2.X - p1.X);
			}

			float YIntercept(float slope, Vector2 p1) { return p1.Y - 1f * slope * p1.X; }

			float m1 = Slope(line1Start, line1End);
			float b1 = YIntercept(m1, line1Start);
			float m2 = Slope(line2Start, line2End);
			float b2 = YIntercept(m2, line2Start);

			float x = 0;
			if (m1 == m2) {
				x = b2 - b1;
			} else {
				x = (b2 - b1) / (m1 - m2);
			}

			float y = m1 * x + b1;

			return new Vector2(x, y);
		}

		public static bool LineSegmentIntersection(in Vector2 line1Start, in Vector2 line1End, in Vector2 line2Start, in Vector2 line2End, out Vector2 intersection) {
			Vector2 intersectionPoint = LineIntersection(line1Start, line1End, line2Start, line2End);

			if (MathF.Min(line1Start.X, line1End.X) - 0.001 <= intersectionPoint.X && intersectionPoint.X <= MathF.Max(line1Start.X, line1End.X) + 0.001) {
				if (MathF.Min(line1Start.Y, line1End.Y) - 0.001 <= intersectionPoint.Y && intersectionPoint.Y <= MathF.Max(line1Start.Y, line1End.Y) + 0.001) {
					if (MathF.Min(line2Start.X, line2End.X) - 0.001 <= intersectionPoint.X && intersectionPoint.X <= MathF.Max(line2Start.X, line2End.X) + 0.001) {
						if (MathF.Min(line2Start.Y, line2End.Y) - 0.001 <= intersectionPoint.Y && intersectionPoint.Y <= MathF.Max(line2Start.Y, line2End.Y) + 0.001) {
							intersection = intersectionPoint;
							return true;
						}
					}
				}
			}

			intersection = default;
			return false;
		}
	}

	public struct CollisionData {
		public float collisionDepth;
		public Vector2 collisionPosition;
		public Vector2 collisionNormal;
	}

	public struct AABB {
		public float Left;
		public float Top;
		public float Width;
		public float Height;

		public Vector2 Center() { return new Vector2(this.Left + this.Width / 2f, this.Top + this.Height / 2f); }
	}
}