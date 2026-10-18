using System.Collections.Generic;
using UnityEngine;

namespace Technie.PhysicsCreator;

public class SphereUtils
{
	public class Support
	{
		public int m_iQuantity;

		public int[] m_aiIndex = new int[4];

		public bool Contains(int iIndex, List<Vector3> points)
		{
			for (int i = 0; i < m_iQuantity; i++)
			{
				if ((points[iIndex] - points[m_aiIndex[i]]).sqrMagnitude < 0.001f)
				{
					return true;
				}
			}
			return false;
		}
	}

	private const float kEpsilon = 0.001f;

	private const float kOnePlusEpsilon = 1.001f;

	private static bool PointInsideSphere(Vector3 rkP, Sphere rkS)
	{
		return (rkP - rkS.center).sqrMagnitude <= 1.001f * rkS.radius;
	}

	private static Sphere ExactSphere1(Vector3 rkP)
	{
		return new Sphere
		{
			center = rkP,
			radius = 0f
		};
	}

	private static Sphere ExactSphere2(Vector3 rkP0, Vector3 rkP1)
	{
		return new Sphere
		{
			center = 0.5f * (rkP0 + rkP1),
			radius = 0.25f * (rkP1 - rkP0).sqrMagnitude
		};
	}

	private static Sphere ExactSphere3(Vector3 rkP0, Vector3 rkP1, Vector3 rkP2)
	{
		Vector3 vector = rkP0 - rkP2;
		Vector3 vector2 = rkP1 - rkP2;
		float num = Vector3.Dot(vector, vector);
		float num2 = Vector3.Dot(vector, vector2);
		float num3 = Vector3.Dot(vector2, vector2);
		float num4 = num * num3 - num2 * num2;
		Sphere sphere = new Sphere();
		float num5 = 0.5f / num4;
		float num6 = num5 * num3 * (num - num2);
		float num7 = num5 * num * (num3 - num2);
		float num8 = 1f - num6 - num7;
		sphere.center = num6 * rkP0 + num7 * rkP1 + num8 * rkP2;
		sphere.radius = (num6 * vector + num7 * vector2).sqrMagnitude;
		return sphere;
	}

	private static Sphere ExactSphere4(Vector3 rkP0, Vector3 rkP1, Vector3 rkP2, Vector3 rkP3)
	{
		Vector3 vector = rkP0 - rkP3;
		Vector3 vector2 = rkP1 - rkP3;
		Vector3 vector3 = rkP2 - rkP3;
		float[,] array = new float[3, 3];
		array[0, 0] = Vector3.Dot(vector, vector);
		array[0, 1] = Vector3.Dot(vector, vector2);
		array[0, 2] = Vector3.Dot(vector, vector3);
		array[1, 0] = array[0, 1];
		array[1, 1] = Vector3.Dot(vector2, vector2);
		array[1, 2] = Vector3.Dot(vector2, vector3);
		array[2, 0] = array[0, 2];
		array[2, 1] = array[1, 2];
		array[2, 2] = Vector3.Dot(vector3, vector3);
		float[] array2 = new float[3]
		{
			0.5f * array[0, 0],
			0.5f * array[1, 1],
			0.5f * array[2, 2]
		};
		float[,] array3 = new float[3, 3]
		{
			{
				array[1, 1] * array[2, 2] - array[1, 2] * array[2, 1],
				array[0, 2] * array[2, 1] - array[0, 1] * array[2, 2],
				array[0, 1] * array[1, 2] - array[0, 2] * array[1, 1]
			},
			{
				array[1, 2] * array[2, 0] - array[1, 0] * array[2, 2],
				array[0, 0] * array[2, 2] - array[0, 2] * array[2, 0],
				array[0, 2] * array[1, 0] - array[0, 0] * array[1, 2]
			},
			{
				array[1, 0] * array[2, 1] - array[1, 1] * array[2, 0],
				array[0, 1] * array[2, 0] - array[0, 0] * array[2, 1],
				array[0, 0] * array[1, 1] - array[0, 1] * array[1, 0]
			}
		};
		float num = array[0, 0] * array3[0, 0] + array[0, 1] * array3[1, 0] + array[0, 2] * array3[2, 0];
		Sphere sphere = new Sphere();
		float num2 = 1f / num;
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				array3[i, j] *= num2;
			}
		}
		float[] array4 = new float[4];
		for (int i = 0; i < 3; i++)
		{
			array4[i] = 0f;
			for (int j = 0; j < 3; j++)
			{
				array4[i] += array3[i, j] * array2[j];
			}
		}
		array4[3] = 1f - array4[0] - array4[1] - array4[2];
		sphere.center = array4[0] * rkP0 + array4[1] * rkP1 + array4[2] * rkP2 + array4[3] * rkP3;
		sphere.radius = (array4[0] * vector + array4[1] * vector2 + array4[2] * vector3).sqrMagnitude;
		return sphere;
	}

	private static Sphere UpdateSupport1(int i, List<Vector3> apkPerm, Support rkSupp)
	{
		Vector3 rkP = apkPerm[rkSupp.m_aiIndex[0]];
		Vector3 rkP2 = apkPerm[i];
		Sphere result = ExactSphere2(rkP, rkP2);
		rkSupp.m_iQuantity = 2;
		rkSupp.m_aiIndex[1] = i;
		return result;
	}

	private static Sphere UpdateSupport2(int i, List<Vector3> apkPerm, Support rkSupp)
	{
		Vector3 vector = apkPerm[rkSupp.m_aiIndex[0]];
		Vector3 vector2 = apkPerm[rkSupp.m_aiIndex[1]];
		Vector3 vector3 = apkPerm[i];
		Sphere[] array = new Sphere[3];
		float num = float.PositiveInfinity;
		int num2 = -1;
		array[0] = ExactSphere2(vector, vector3);
		if (PointInsideSphere(vector2, array[0]))
		{
			num = array[0].radius;
			num2 = 0;
		}
		array[1] = ExactSphere2(vector2, vector3);
		if (PointInsideSphere(vector, array[1]) && array[1].radius < num)
		{
			num = array[1].radius;
			num2 = 1;
		}
		Sphere result;
		if (num2 != -1)
		{
			result = array[num2];
			rkSupp.m_aiIndex[1 - num2] = i;
		}
		else
		{
			result = ExactSphere3(vector, vector2, vector3);
			rkSupp.m_iQuantity = 3;
			rkSupp.m_aiIndex[2] = i;
		}
		return result;
	}

	private static Sphere UpdateSupport3(int i, List<Vector3> apkPerm, Support rkSupp)
	{
		Vector3 vector = apkPerm[rkSupp.m_aiIndex[0]];
		Vector3 vector2 = apkPerm[rkSupp.m_aiIndex[1]];
		Vector3 vector3 = apkPerm[rkSupp.m_aiIndex[2]];
		Vector3 vector4 = apkPerm[i];
		Sphere[] array = new Sphere[6];
		float num = float.PositiveInfinity;
		int num2 = -1;
		array[0] = ExactSphere2(vector, vector4);
		if (PointInsideSphere(vector2, array[0]) && PointInsideSphere(vector3, array[0]))
		{
			num = array[0].radius;
			num2 = 0;
		}
		array[1] = ExactSphere2(vector2, vector4);
		if (PointInsideSphere(vector, array[1]) && PointInsideSphere(vector3, array[1]) && array[1].radius < num)
		{
			num = array[1].radius;
			num2 = 1;
		}
		array[2] = ExactSphere2(vector3, vector4);
		if (PointInsideSphere(vector, array[2]) && PointInsideSphere(vector2, array[2]) && array[2].radius < num)
		{
			num = array[2].radius;
			num2 = 2;
		}
		array[3] = ExactSphere3(vector, vector2, vector4);
		if (PointInsideSphere(vector3, array[3]) && array[3].radius < num)
		{
			num = array[3].radius;
			num2 = 3;
		}
		array[4] = ExactSphere3(vector, vector3, vector4);
		if (PointInsideSphere(vector2, array[4]) && array[4].radius < num)
		{
			num = array[4].radius;
			num2 = 4;
		}
		array[5] = ExactSphere3(vector2, vector3, vector4);
		if (PointInsideSphere(vector, array[5]) && array[5].radius < num)
		{
			num = array[5].radius;
			num2 = 5;
		}
		Sphere result;
		switch (num2)
		{
		case 0:
			result = array[0];
			rkSupp.m_iQuantity = 2;
			rkSupp.m_aiIndex[1] = i;
			break;
		case 1:
			result = array[1];
			rkSupp.m_iQuantity = 2;
			rkSupp.m_aiIndex[0] = i;
			break;
		case 2:
			result = array[2];
			rkSupp.m_iQuantity = 2;
			rkSupp.m_aiIndex[0] = rkSupp.m_aiIndex[2];
			rkSupp.m_aiIndex[1] = i;
			break;
		case 3:
			result = array[3];
			rkSupp.m_aiIndex[2] = i;
			break;
		case 4:
			result = array[4];
			rkSupp.m_aiIndex[1] = i;
			break;
		case 5:
			result = array[5];
			rkSupp.m_aiIndex[0] = i;
			break;
		default:
			result = ExactSphere4(vector, vector2, vector3, vector4);
			rkSupp.m_iQuantity = 4;
			rkSupp.m_aiIndex[3] = i;
			break;
		}
		return result;
	}

	public static Sphere UpdateSupport4(int i, List<Vector3> apkPerm, Support rkSupp)
	{
		Vector3 vector = apkPerm[rkSupp.m_aiIndex[0]];
		Vector3 vector2 = apkPerm[rkSupp.m_aiIndex[1]];
		Vector3 vector3 = apkPerm[rkSupp.m_aiIndex[2]];
		Vector3 vector4 = apkPerm[rkSupp.m_aiIndex[3]];
		Vector3 vector5 = apkPerm[i];
		Sphere[] array = new Sphere[14];
		float num = float.PositiveInfinity;
		int num2 = -1;
		array[0] = ExactSphere2(vector, vector5);
		if (PointInsideSphere(vector2, array[0]) && PointInsideSphere(vector3, array[0]) && PointInsideSphere(vector4, array[0]))
		{
			num = array[0].radius;
			num2 = 0;
		}
		array[1] = ExactSphere2(vector2, vector5);
		if (PointInsideSphere(vector, array[1]) && PointInsideSphere(vector3, array[1]) && PointInsideSphere(vector4, array[1]) && array[1].radius < num)
		{
			num = array[1].radius;
			num2 = 1;
		}
		array[2] = ExactSphere2(vector3, vector5);
		if (PointInsideSphere(vector, array[2]) && PointInsideSphere(vector2, array[2]) && PointInsideSphere(vector4, array[2]) && array[2].radius < num)
		{
			num = array[2].radius;
			num2 = 2;
		}
		array[3] = ExactSphere2(vector4, vector5);
		if (PointInsideSphere(vector, array[3]) && PointInsideSphere(vector2, array[3]) && PointInsideSphere(vector3, array[3]) && array[3].radius < num)
		{
			num = array[3].radius;
			num2 = 3;
		}
		array[4] = ExactSphere3(vector, vector2, vector5);
		if (PointInsideSphere(vector3, array[4]) && PointInsideSphere(vector4, array[4]) && array[4].radius < num)
		{
			num = array[4].radius;
			num2 = 4;
		}
		array[5] = ExactSphere3(vector, vector3, vector5);
		if (PointInsideSphere(vector2, array[5]) && PointInsideSphere(vector4, array[5]) && array[5].radius < num)
		{
			num = array[5].radius;
			num2 = 5;
		}
		array[6] = ExactSphere3(vector, vector4, vector5);
		if (PointInsideSphere(vector2, array[6]) && PointInsideSphere(vector3, array[6]) && array[6].radius < num)
		{
			num = array[6].radius;
			num2 = 6;
		}
		array[7] = ExactSphere3(vector2, vector3, vector5);
		if (PointInsideSphere(vector, array[7]) && PointInsideSphere(vector4, array[7]) && array[7].radius < num)
		{
			num = array[7].radius;
			num2 = 7;
		}
		array[8] = ExactSphere3(vector2, vector4, vector5);
		if (PointInsideSphere(vector, array[8]) && PointInsideSphere(vector3, array[8]) && array[8].radius < num)
		{
			num = array[8].radius;
			num2 = 8;
		}
		array[9] = ExactSphere3(vector3, vector4, vector5);
		if (PointInsideSphere(vector, array[9]) && PointInsideSphere(vector2, array[9]) && array[9].radius < num)
		{
			num = array[9].radius;
			num2 = 9;
		}
		array[10] = ExactSphere4(vector, vector2, vector3, vector5);
		if (PointInsideSphere(vector4, array[10]) && array[10].radius < num)
		{
			num = array[10].radius;
			num2 = 10;
		}
		array[11] = ExactSphere4(vector, vector2, vector4, vector5);
		if (PointInsideSphere(vector3, array[11]) && array[11].radius < num)
		{
			num = array[11].radius;
			num2 = 11;
		}
		array[12] = ExactSphere4(vector, vector3, vector4, vector5);
		if (PointInsideSphere(vector2, array[12]) && array[12].radius < num)
		{
			num = array[12].radius;
			num2 = 12;
		}
		array[13] = ExactSphere4(vector2, vector3, vector4, vector5);
		if (PointInsideSphere(vector, array[13]) && array[13].radius < num)
		{
			num = array[13].radius;
			num2 = 13;
		}
		Sphere result = array[num2];
		switch (num2)
		{
		case 0:
			rkSupp.m_iQuantity = 2;
			rkSupp.m_aiIndex[1] = i;
			break;
		case 1:
			rkSupp.m_iQuantity = 2;
			rkSupp.m_aiIndex[0] = i;
			break;
		case 2:
			rkSupp.m_iQuantity = 2;
			rkSupp.m_aiIndex[0] = rkSupp.m_aiIndex[2];
			rkSupp.m_aiIndex[1] = i;
			break;
		case 3:
			rkSupp.m_iQuantity = 2;
			rkSupp.m_aiIndex[0] = rkSupp.m_aiIndex[3];
			rkSupp.m_aiIndex[1] = i;
			break;
		case 4:
			rkSupp.m_iQuantity = 3;
			rkSupp.m_aiIndex[2] = i;
			break;
		case 5:
			rkSupp.m_iQuantity = 3;
			rkSupp.m_aiIndex[1] = i;
			break;
		case 6:
			rkSupp.m_iQuantity = 3;
			rkSupp.m_aiIndex[1] = rkSupp.m_aiIndex[3];
			rkSupp.m_aiIndex[2] = i;
			break;
		case 7:
			rkSupp.m_iQuantity = 3;
			rkSupp.m_aiIndex[0] = i;
			break;
		case 8:
			rkSupp.m_iQuantity = 3;
			rkSupp.m_aiIndex[0] = rkSupp.m_aiIndex[3];
			rkSupp.m_aiIndex[2] = i;
			break;
		case 9:
			rkSupp.m_iQuantity = 3;
			rkSupp.m_aiIndex[0] = rkSupp.m_aiIndex[3];
			rkSupp.m_aiIndex[1] = i;
			break;
		case 10:
			rkSupp.m_aiIndex[3] = i;
			break;
		case 11:
			rkSupp.m_aiIndex[2] = i;
			break;
		case 12:
			rkSupp.m_aiIndex[1] = i;
			break;
		case 13:
			rkSupp.m_aiIndex[0] = i;
			break;
		}
		return result;
	}

	private static Sphere Update(int funcIndex, int numPoints, List<Vector3> points, Support support)
	{
		return funcIndex switch
		{
			0 => null, 
			1 => UpdateSupport1(numPoints, points, support), 
			2 => UpdateSupport2(numPoints, points, support), 
			3 => UpdateSupport3(numPoints, points, support), 
			4 => UpdateSupport4(numPoints, points, support), 
			_ => null, 
		};
	}

	public static Sphere MinSphere(List<Vector3> inputPoints)
	{
		Sphere sphere = new Sphere();
		Support support = new Support();
		if (inputPoints.Count >= 1)
		{
			List<Vector3> list = new List<Vector3>(inputPoints);
			Shuffle(list);
			sphere = ExactSphere1(list[0]);
			support.m_iQuantity = 1;
			support.m_aiIndex[0] = 0;
			int num = 1;
			while (num < inputPoints.Count)
			{
				if (!support.Contains(num, list) && !PointInsideSphere(list[num], sphere))
				{
					sphere = Update(support.m_iQuantity, num, list, support);
					num = 0;
				}
				else
				{
					num++;
				}
			}
		}
		sphere.radius = Mathf.Sqrt(sphere.radius);
		return sphere;
	}

	public static void Shuffle(List<Vector3> list)
	{
		for (int i = 0; i < list.Count; i++)
		{
			int index = i + Random.Range(0, list.Count - i);
			Vector3 value = list[index];
			list[index] = list[i];
			list[i] = value;
		}
	}
}
