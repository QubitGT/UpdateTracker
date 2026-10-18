using System;

namespace UnityEngine.ProBuilder.Shapes;

[Shape("Stairs")]
public class Stairs : Shape
{
	[SerializeField]
	private StepGenerationType m_StepGenerationType = StepGenerationType.Count;

	[Min(0.01f)]
	[SerializeField]
	private float m_StepsHeight = 0.2f;

	[Range(1f, 256f)]
	[SerializeField]
	private int m_StepsCount = 10;

	[SerializeField]
	private bool m_HomogeneousSteps = true;

	[Range(-360f, 360f)]
	[SerializeField]
	private float m_Circumference;

	[SerializeField]
	private bool m_Sides = true;

	[SerializeField]
	[Min(0f)]
	private float m_InnerRadius;

	internal int stepsCount
	{
		get
		{
			return m_StepsCount;
		}
		set
		{
			m_StepsCount = value;
		}
	}

	internal float circumference
	{
		get
		{
			return m_Circumference;
		}
		set
		{
			m_Circumference = value;
		}
	}

	public bool sides
	{
		get
		{
			return m_Sides;
		}
		set
		{
			m_Sides = value;
		}
	}

	internal float innerRadius
	{
		get
		{
			return m_InnerRadius;
		}
		set
		{
			m_InnerRadius = value;
		}
	}

	internal override void SetParametersToBuiltInShape()
	{
		m_StepsHeight = 0.4f;
		m_StepsCount = 6;
		m_HomogeneousSteps = true;
		m_Circumference = 0f;
		m_Sides = true;
	}

	public override void CopyShape(Shape shape)
	{
		if (shape is Stairs)
		{
			Stairs stairs = (Stairs)shape;
			m_StepGenerationType = stairs.m_StepGenerationType;
			m_StepsHeight = stairs.m_StepsHeight;
			m_StepsCount = stairs.m_StepsCount;
			m_HomogeneousSteps = stairs.m_HomogeneousSteps;
			m_Circumference = stairs.m_Circumference;
			m_Sides = stairs.m_Sides;
			m_InnerRadius = stairs.m_InnerRadius;
		}
	}

	public override Bounds RebuildMesh(ProBuilderMesh mesh, Vector3 size, Quaternion rotation)
	{
		if (Mathf.Abs(m_Circumference) > 0f)
		{
			return BuildCurvedStairs(mesh, size, rotation);
		}
		return BuildStairs(mesh, size, rotation);
	}

	public override Bounds UpdateBounds(ProBuilderMesh mesh, Vector3 size, Quaternion rotation, Bounds bounds)
	{
		if (Mathf.Abs(m_Circumference) > 0f)
		{
			bounds.center = mesh.mesh.bounds.center;
			bounds.size = Vector3.Scale(size.Sign(), mesh.mesh.bounds.size);
		}
		else
		{
			bounds = mesh.mesh.bounds;
			bounds.size = size;
		}
		return bounds;
	}

	private Bounds BuildStairs(ProBuilderMesh mesh, Vector3 size, Quaternion rotation)
	{
		Vector3 vector = Vector3.Scale(rotation * Vector3.up, size);
		Vector3 vector2 = Vector3.Scale(rotation * Vector3.right, size);
		Vector3 vector3 = Vector3.Scale(rotation * Vector3.forward, size);
		Vector3 vector4 = new Vector3(vector2.magnitude, vector.magnitude, vector3.magnitude);
		bool flag = m_StepGenerationType == StepGenerationType.Height;
		float y = vector4.y;
		float num = Mathf.Min(m_StepsHeight, y);
		int num2 = m_StepsCount;
		if (flag)
		{
			if (y > 0f)
			{
				num2 = (int)(y / num);
				if (m_HomogeneousSteps)
				{
					num = y / (float)num2;
				}
				else
				{
					num2 += ((y / num - (float)num2 > 0.001f) ? 1 : 0);
				}
			}
			else
			{
				num2 = 1;
			}
		}
		if (num2 > 256)
		{
			num2 = 256;
			num = y / (float)num2;
		}
		Vector3[] array = new Vector3[4 * num2 * 2];
		Face[] array2 = new Face[num2 * 2];
		Vector3 vector5 = vector4 * 0.5f;
		int num3 = 0;
		int num4 = 0;
		for (int i = 0; i < num2; i++)
		{
			float num5 = (float)i * num;
			float num6 = ((i != num2 - 1) ? ((float)(i + 1) * num) : vector4.y);
			float num7 = (float)i / (float)num2;
			float num8 = (float)(i + 1) / (float)num2;
			float x = vector4.x - vector5.x;
			float x2 = 0f - vector5.x;
			float y2 = (flag ? num5 : (vector4.y * num7)) - vector5.y;
			float y3 = (flag ? num6 : (vector4.y * num8)) - vector5.y;
			float z = vector4.z * num7 - vector5.z;
			float z2 = vector4.z * num8 - vector5.z;
			array[num3] = new Vector3(x, y2, z);
			array[num3 + 1] = new Vector3(x2, y2, z);
			array[num3 + 2] = new Vector3(x, y3, z);
			array[num3 + 3] = new Vector3(x2, y3, z);
			array[num3 + 4] = new Vector3(x, y3, z);
			array[num3 + 5] = new Vector3(x2, y3, z);
			array[num3 + 6] = new Vector3(x, y3, z2);
			array[num3 + 7] = new Vector3(x2, y3, z2);
			array2[num4] = new Face(new int[6]
			{
				num3,
				num3 + 1,
				num3 + 2,
				num3 + 1,
				num3 + 3,
				num3 + 2
			});
			array2[num4 + 1] = new Face(new int[6]
			{
				num3 + 4,
				num3 + 5,
				num3 + 6,
				num3 + 5,
				num3 + 7,
				num3 + 6
			});
			num3 += 8;
			num4 += 2;
		}
		if (sides)
		{
			float num9 = 0f;
			for (int j = 0; j < 2; j++)
			{
				Vector3[] array3 = new Vector3[num2 * 4 + (num2 - 1) * 3];
				Face[] array4 = new Face[num2 + num2 - 1];
				int num10 = 0;
				int num11 = 0;
				for (int k = 0; k < num2; k++)
				{
					float num5 = (float)Mathf.Max(k, 1) * num;
					float num6 = ((k != num2 - 1) ? ((float)(k + 1) * num) : vector4.y);
					float num7 = (float)Mathf.Max(k, 1) / (float)num2;
					float num8 = (float)(k + 1) / (float)num2;
					float y2 = (flag ? num5 : (num7 * vector4.y));
					float y3 = (flag ? num6 : (num8 * vector4.y));
					num7 = (float)k / (float)num2;
					float z = num7 * vector4.z;
					float z2 = num8 * vector4.z;
					array3[num10] = new Vector3(num9, 0f, z) - vector5;
					array3[num10 + 1] = new Vector3(num9, 0f, z2) - vector5;
					array3[num10 + 2] = new Vector3(num9, y2, z) - vector5;
					array3[num10 + 3] = new Vector3(num9, y3, z2) - vector5;
					array4[num11++] = new Face((j % 2 != 0) ? new int[6]
					{
						num3 + 2,
						num3 + 1,
						num3,
						num3 + 2,
						num3 + 3,
						num3 + 1
					} : new int[6]
					{
						num3,
						num3 + 1,
						num3 + 2,
						num3 + 1,
						num3 + 3,
						num3 + 2
					});
					array4[num11 - 1].textureGroup = j + 1;
					num3 += 4;
					num10 += 4;
					if (k > 0)
					{
						array3[num10] = new Vector3(num9, y2, z) - vector5;
						array3[num10 + 1] = new Vector3(num9, y3, z) - vector5;
						array3[num10 + 2] = new Vector3(num9, y3, z2) - vector5;
						array4[num11++] = new Face((j % 2 != 0) ? new int[3]
						{
							num3,
							num3 + 1,
							num3 + 2
						} : new int[3]
						{
							num3 + 2,
							num3 + 1,
							num3
						});
						array4[num11 - 1].textureGroup = j + 1;
						num3 += 3;
						num10 += 3;
					}
				}
				array = array.Concat(array3);
				array2 = array2.Concat(array4);
				num9 += vector4.x;
			}
			array = array.Concat(new Vector3[4]
			{
				new Vector3(0f, 0f, vector4.z) - vector5,
				new Vector3(vector4.x, 0f, vector4.z) - vector5,
				new Vector3(0f, vector4.y, vector4.z) - vector5,
				new Vector3(vector4.x, vector4.y, vector4.z) - vector5
			});
			array2 = array2.Add(new Face(new int[6]
			{
				num3,
				num3 + 1,
				num3 + 2,
				num3 + 1,
				num3 + 3,
				num3 + 2
			}));
		}
		Vector3 scale = size.Sign();
		for (int l = 0; l < array.Length; l++)
		{
			array[l] = rotation * array[l];
			array[l].Scale(scale);
		}
		if (scale.x * scale.y * scale.z < 0f)
		{
			Face[] array5 = array2;
			for (int m = 0; m < array5.Length; m++)
			{
				array5[m].Reverse();
			}
		}
		mesh.RebuildWithPositionsAndFaces(array, array2);
		return UpdateBounds(mesh, size, rotation, default(Bounds));
	}

	private Bounds BuildCurvedStairs(ProBuilderMesh mesh, Vector3 size, Quaternion rotation)
	{
		Vector3 vector = size.Abs();
		bool flag = m_Sides;
		float num = Mathf.Min(vector.x, vector.z);
		float num2 = Mathf.Clamp(m_InnerRadius, 0f, num - float.Epsilon);
		float num3 = num - num2;
		float num4 = Mathf.Abs(vector.y);
		float num5 = m_Circumference;
		bool flag2 = num2 < Mathf.Epsilon;
		bool flag3 = m_StepGenerationType == StepGenerationType.Height;
		float num6 = Mathf.Min(m_StepsHeight, num4);
		int num7 = m_StepsCount;
		if (flag3 && num6 > 0.01f * m_StepsHeight)
		{
			if (num4 > 0f)
			{
				num7 = (int)(num4 / m_StepsHeight);
				if (m_HomogeneousSteps && num7 > 0)
				{
					num6 = num4 / (float)num7;
				}
				else
				{
					num7 += ((num4 / m_StepsHeight - (float)num7 > 0.001f) ? 1 : 0);
				}
			}
			else
			{
				num7 = 1;
			}
		}
		if (num7 > 256)
		{
			num7 = 256;
			num6 = num4 / (float)num7;
		}
		Vector3[] array = new Vector3[4 * num7 + (flag2 ? 3 : 4) * num7];
		Face[] array2 = new Face[num7 * 2];
		int num8 = 0;
		int num9 = 0;
		float num10 = Mathf.Abs(num5) * (MathF.PI / 180f);
		float num11 = num2 + num3;
		for (int i = 0; i < num7; i++)
		{
			float num12 = (float)i / (float)num7 * num10;
			float num13 = (float)(i + 1) / (float)num7 * num10;
			float y = (flag3 ? ((float)i * num6) : ((float)i / (float)num7 * num4));
			float y2 = ((!flag3) ? ((float)(i + 1) / (float)num7 * num4) : ((i != num7 - 1) ? ((float)(i + 1) * num6) : num4));
			Vector3 vector2 = new Vector3(0f - Mathf.Cos(num12), 0f, Mathf.Sin(num12));
			Vector3 vector3 = new Vector3(0f - Mathf.Cos(num13), 0f, Mathf.Sin(num13));
			array[num8] = vector2 * num2;
			array[num8 + 1] = vector2 * num11;
			array[num8 + 2] = vector2 * num2;
			array[num8 + 3] = vector2 * num11;
			array[num8].y = y;
			array[num8 + 1].y = y;
			array[num8 + 2].y = y2;
			array[num8 + 3].y = y2;
			array[num8 + 4] = array[num8 + 2];
			array[num8 + 5] = array[num8 + 3];
			array[num8 + 6] = vector3 * num11;
			array[num8 + 6].y = y2;
			if (!flag2)
			{
				array[num8 + 7] = vector3 * num2;
				array[num8 + 7].y = y2;
			}
			array2[num9] = new Face(new int[6]
			{
				num8,
				num8 + 1,
				num8 + 2,
				num8 + 1,
				num8 + 3,
				num8 + 2
			});
			if (flag2)
			{
				array2[num9 + 1] = new Face(new int[3]
				{
					num8 + 4,
					num8 + 5,
					num8 + 6
				});
			}
			else
			{
				array2[num9 + 1] = new Face(new int[6]
				{
					num8 + 4,
					num8 + 5,
					num8 + 6,
					num8 + 4,
					num8 + 6,
					num8 + 7
				});
			}
			float num14 = (num13 + num12) * -0.5f * 57.29578f;
			num14 %= 360f;
			if (num14 < 0f)
			{
				num14 = 360f + num14;
			}
			AutoUnwrapSettings uv = array2[num9 + 1].uv;
			uv.rotation = num14;
			array2[num9 + 1].uv = uv;
			num8 += (flag2 ? 7 : 8);
			num9 += 2;
		}
		if (flag)
		{
			float num15 = (flag2 ? (num2 + num3) : num2);
			for (int j = (flag2 ? 1 : 0); j < 2; j++)
			{
				Vector3[] array3 = new Vector3[num7 * 4 + (num7 - 1) * 3];
				Face[] array4 = new Face[num7 + num7 - 1];
				int num16 = 0;
				int num17 = 0;
				for (int k = 0; k < num7; k++)
				{
					float f = (float)k / (float)num7 * num10;
					float f2 = (float)(k + 1) / (float)num7 * num10;
					float y3 = (flag3 ? ((float)Mathf.Max(k, 1) * num6) : ((float)Mathf.Max(k, 1) / (float)num7 * num4));
					float y4 = ((!flag3) ? ((float)(k + 1) / (float)num7 * num4) : ((k != num7 - 1) ? ((float)(k + 1) * num6) : vector.y));
					Vector3 vector4 = new Vector3(0f - Mathf.Cos(f), 0f, Mathf.Sin(f)) * num15;
					Vector3 vector5 = new Vector3(0f - Mathf.Cos(f2), 0f, Mathf.Sin(f2)) * num15;
					array3[num16] = vector4;
					array3[num16 + 1] = vector5;
					array3[num16 + 2] = vector4;
					array3[num16 + 3] = vector5;
					array3[num16].y = 0f;
					array3[num16 + 1].y = 0f;
					array3[num16 + 2].y = y3;
					array3[num16 + 3].y = y4;
					array4[num17++] = new Face((j % 2 != 0) ? new int[6]
					{
						num8,
						num8 + 1,
						num8 + 2,
						num8 + 1,
						num8 + 3,
						num8 + 2
					} : new int[6]
					{
						num8 + 2,
						num8 + 1,
						num8,
						num8 + 2,
						num8 + 3,
						num8 + 1
					});
					array4[num17 - 1].smoothingGroup = j + 1;
					num8 += 4;
					num16 += 4;
					if (k > 0)
					{
						array4[num17 - 1].textureGroup = j * num7 + k;
						array3[num16] = vector4;
						array3[num16 + 1] = vector5;
						array3[num16 + 2] = vector4;
						array3[num16].y = y3;
						array3[num16 + 1].y = y4;
						array3[num16 + 2].y = y4;
						array4[num17++] = new Face((j % 2 != 0) ? new int[3]
						{
							num8,
							num8 + 1,
							num8 + 2
						} : new int[3]
						{
							num8 + 2,
							num8 + 1,
							num8
						});
						array4[num17 - 1].textureGroup = j * num7 + k;
						array4[num17 - 1].smoothingGroup = j + 1;
						num8 += 3;
						num16 += 3;
					}
				}
				array = array.Concat(array3);
				array2 = array2.Concat(array4);
				num15 += num3;
			}
			float num18 = 0f - Mathf.Cos(num10);
			float num19 = Mathf.Sin(num10);
			array = array.Concat(new Vector3[4]
			{
				new Vector3(num18, 0f, num19) * num2,
				new Vector3(num18, 0f, num19) * num11,
				new Vector3(num18 * num2, num4, num19 * num2),
				new Vector3(num18 * num11, num4, num19 * num11)
			});
			array2 = array2.Add(new Face(new int[6]
			{
				num8 + 2,
				num8 + 1,
				num8,
				num8 + 2,
				num8 + 3,
				num8 + 1
			}));
		}
		if (num5 < 0f)
		{
			Vector3 scale = new Vector3(-1f, 1f, 1f);
			for (int l = 0; l < array.Length; l++)
			{
				array[l].Scale(scale);
			}
			Face[] array5 = array2;
			for (int m = 0; m < array5.Length; m++)
			{
				array5[m].Reverse();
			}
		}
		Vector3 scale2 = size.Sign();
		for (int n = 0; n < array.Length; n++)
		{
			array[n] = rotation * array[n];
			array[n].Scale(scale2);
		}
		if (scale2.x * scale2.y * scale2.z < 0f)
		{
			Face[] array5 = array2;
			for (int m = 0; m < array5.Length; m++)
			{
				array5[m].Reverse();
			}
		}
		mesh.RebuildWithPositionsAndFaces(array, array2);
		mesh.TranslateVerticesInWorldSpace(mesh.mesh.triangles, mesh.transform.TransformDirection(-mesh.mesh.bounds.center));
		mesh.Refresh();
		return UpdateBounds(mesh, size, rotation, default(Bounds));
	}
}
