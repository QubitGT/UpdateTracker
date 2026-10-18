using System;
using System.Collections.Generic;

namespace g3;

public class ContMinCircle2
{
	private struct Circle
	{
		public Vector2d Center;

		public double Radius;

		public Circle(Vector2d c, double radius)
		{
			Center = c;
			Radius = radius;
		}
	}

	protected class Support
	{
		public int Quantity;

		public Index3i Index;

		public bool Contains(int index, IList<Vector2d> Points, int[] permutation, double epsilon)
		{
			for (int i = 0; i < Quantity; i++)
			{
				if ((Points[permutation[index]] - Points[permutation[Index[i]]]).LengthSquared < epsilon)
				{
					return true;
				}
			}
			return false;
		}
	}

	private double mEpsilon;

	private Func<int, int[], Support, Circle>[] mUpdate = new Func<int, int[], Support, Circle>[4];

	private IList<Vector2d> Points;

	private Circle[] circle_buf = new Circle[6];

	public Circle2d Result;

	private static readonly int[,] type2_2 = new int[2, 2]
	{
		{ 0, 1 },
		{ 1, 0 }
	};

	private static readonly int[,] type2_3 = new int[3, 3]
	{
		{ 0, 1, 2 },
		{ 1, 0, 2 },
		{ 2, 0, 1 }
	};

	private static readonly int[,] type3_3 = new int[3, 3]
	{
		{ 0, 1, 2 },
		{ 0, 2, 1 },
		{ 1, 2, 0 }
	};

	public ContMinCircle2(IList<Vector2d> pointsIn, double epsilon = 1E-05)
	{
		mEpsilon = epsilon;
		mUpdate[0] = null;
		mUpdate[1] = UpdateSupport1;
		mUpdate[2] = UpdateSupport2;
		mUpdate[3] = UpdateSupport3;
		Support support = new Support();
		double distDiff = 0.0;
		Points = pointsIn;
		int count = pointsIn.Count;
		int[] array = null;
		Random random = new Random();
		if (count >= 1)
		{
			array = new int[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = i;
			}
			for (int num = count - 1; num > 0; num--)
			{
				int num2 = random.Next() % (num + 1);
				if (num2 != num)
				{
					int num3 = array[num];
					array[num] = array[num2];
					array[num2] = num3;
				}
			}
			Circle circle = new Circle(Points[array[0]], 0.0);
			support.Quantity = 1;
			support.Index[0] = 0;
			int num4 = 1 % count;
			int num5 = 0;
			while (num4 != num5)
			{
				if (!support.Contains(num4, Points, array, mEpsilon) && !Contains(Points[array[num4]], ref circle, ref distDiff))
				{
					Circle circle2 = mUpdate[support.Quantity](num4, array, support);
					if (circle2.Radius > circle.Radius)
					{
						circle = circle2;
						num5 = num4;
					}
				}
				num4 = (num4 + 1) % count;
			}
			Result = new Circle2d(circle.Center, Math.Sqrt(circle.Radius));
			return;
		}
		throw new Exception("ContMinCircle2: Input must contain points\n");
	}

	private bool Contains(Vector2d point, ref Circle circle, ref double distDiff)
	{
		double lengthSquared = (point - circle.Center).LengthSquared;
		distDiff = lengthSquared - circle.Radius;
		return distDiff <= 0.0;
	}

	private Circle ExactCircle2(ref Vector2d P0, ref Vector2d P1)
	{
		return new Circle(0.5 * (P0 + P1), 0.25 * P1.DistanceSquared(P0));
	}

	private Circle ExactCircle2(Vector2d P0, ref Vector2d P1)
	{
		return new Circle(0.5 * (P0 + P1), 0.25 * P1.DistanceSquared(P0));
	}

	private Circle ExactCircle3(ref Vector2d P0, ref Vector2d P1, ref Vector2d P2)
	{
		Vector2d vector2d = P1 - P0;
		Vector2d vector2d2 = P2 - P0;
		Matrix2d matrix2d = new Matrix2d(vector2d.x, vector2d.y, vector2d2.x, vector2d2.y);
		Vector2d vector2d3 = new Vector2d(0.5 * vector2d.LengthSquared, 0.5 * vector2d2.LengthSquared);
		double num = matrix2d.m00 * matrix2d.m11 - matrix2d.m01 * matrix2d.m10;
		if (Math.Abs(num) > mEpsilon)
		{
			double num2 = 1.0 / num;
			Vector2d vector2d4 = default(Vector2d);
			vector2d4.x = (matrix2d.m11 * vector2d3.x - matrix2d.m01 * vector2d3.y) * num2;
			vector2d4.y = (matrix2d.m00 * vector2d3.y - matrix2d.m10 * vector2d3.x) * num2;
			return new Circle(P0 + vector2d4, vector2d4.LengthSquared);
		}
		return new Circle(Vector2d.Zero, double.MaxValue);
	}

	private Circle ExactCircle3(Vector2d P0, Vector2d P1, ref Vector2d P2)
	{
		return ExactCircle3(ref P0, ref P1, ref P2);
	}

	private Circle ExactCircle3(Vector2d P0, ref Vector2d P1, ref Vector2d P2)
	{
		return ExactCircle3(ref P0, ref P1, ref P2);
	}

	private Circle UpdateSupport1(int i, int[] permutation, Support support)
	{
		Vector2d P = Points[permutation[support.Index[0]]];
		Vector2d P2 = Points[permutation[i]];
		Circle result = ExactCircle2(ref P, ref P2);
		support.Quantity = 2;
		support.Index[1] = i;
		return result;
	}

	private Circle UpdateSupport2(int i, int[] permutation, Support support)
	{
		Vector2dTuple2 vector2dTuple = new Vector2dTuple2(Points[permutation[support.Index[0]]], Points[permutation[support.Index[1]]]);
		Vector2d P = Points[permutation[i]];
		int num = 2;
		Circle[] array = circle_buf;
		int num2 = 0;
		double num3 = double.MaxValue;
		int num4 = -1;
		double distDiff = 0.0;
		double num5 = double.MaxValue;
		int num6 = -1;
		int num7 = 0;
		while (num7 < num)
		{
			array[num2] = ExactCircle2(vector2dTuple[type2_2[num7, 0]], ref P);
			if (array[num2].Radius < num3)
			{
				if (Contains(vector2dTuple[type2_2[num7, 1]], ref array[num2], ref distDiff))
				{
					num3 = array[num2].Radius;
					num4 = num2;
				}
				else if (distDiff < num5)
				{
					num5 = distDiff;
					num6 = num2;
				}
			}
			num7++;
			num2++;
		}
		array[num2] = ExactCircle3(vector2dTuple[0], vector2dTuple[1], ref P);
		if (array[num2].Radius < num3)
		{
			num3 = array[num2].Radius;
			num4 = num2;
		}
		if (num4 == -1)
		{
			num4 = num6;
		}
		Circle result = array[num4];
		switch (num4)
		{
		case 0:
			support.Index[1] = i;
			break;
		case 1:
			support.Index[0] = i;
			break;
		case 2:
			support.Quantity = 3;
			support.Index[2] = i;
			break;
		}
		return result;
	}

	private Circle UpdateSupport3(int i, int[] permutation, Support support)
	{
		Vector2dTuple3 vector2dTuple = new Vector2dTuple3(Points[permutation[support.Index[0]]], Points[permutation[support.Index[1]]], Points[permutation[support.Index[2]]]);
		Vector2d P = Points[permutation[i]];
		int num = 3;
		int num2 = 3;
		Circle[] array = circle_buf;
		int num3 = 0;
		double num4 = double.MaxValue;
		int num5 = -1;
		double distDiff = 0.0;
		double num6 = double.MaxValue;
		int num7 = -1;
		int num8 = 0;
		while (num8 < num)
		{
			array[num3] = ExactCircle2(vector2dTuple[type2_3[num8, 0]], ref P);
			if (array[num3].Radius < num4)
			{
				if (Contains(vector2dTuple[type2_3[num8, 1]], ref array[num3], ref distDiff) && Contains(vector2dTuple[type2_3[num8, 2]], ref array[num3], ref distDiff))
				{
					num4 = array[num3].Radius;
					num5 = num3;
				}
				else if (distDiff < num6)
				{
					num6 = distDiff;
					num7 = num3;
				}
			}
			num8++;
			num3++;
		}
		num8 = 0;
		while (num8 < num2)
		{
			array[num3] = ExactCircle3(vector2dTuple[type3_3[num8, 0]], vector2dTuple[type3_3[num8, 1]], ref P);
			if (array[num3].Radius < num4)
			{
				if (Contains(vector2dTuple[type3_3[num8, 2]], ref array[num3], ref distDiff))
				{
					num4 = array[num3].Radius;
					num5 = num3;
				}
				else if (distDiff < num6)
				{
					num6 = distDiff;
					num7 = num3;
				}
			}
			num8++;
			num3++;
		}
		if (num5 == -1)
		{
			num5 = num7;
		}
		Circle result = array[num5];
		switch (num5)
		{
		case 0:
			support.Quantity = 2;
			support.Index[1] = i;
			break;
		case 1:
			support.Quantity = 2;
			support.Index[0] = i;
			break;
		case 2:
			support.Quantity = 2;
			support.Index[0] = support.Index[2];
			support.Index[1] = i;
			break;
		case 3:
			support.Index[2] = i;
			break;
		case 4:
			support.Index[1] = i;
			break;
		case 5:
			support.Index[0] = i;
			break;
		}
		return result;
	}
}
