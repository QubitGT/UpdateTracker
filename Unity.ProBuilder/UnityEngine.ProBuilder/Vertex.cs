using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Rendering;

namespace UnityEngine.ProBuilder;

[Serializable]
public sealed class Vertex : IEquatable<Vertex>
{
	[SerializeField]
	private Vector3 m_Position;

	[SerializeField]
	private Color m_Color;

	[SerializeField]
	private Vector3 m_Normal;

	[SerializeField]
	private Vector4 m_Tangent;

	[SerializeField]
	private Vector2 m_UV0;

	[SerializeField]
	private Vector2 m_UV2;

	[SerializeField]
	private Vector4 m_UV3;

	[SerializeField]
	private Vector4 m_UV4;

	[SerializeField]
	private MeshArrays m_Attributes;

	public Vector3 position
	{
		get
		{
			return m_Position;
		}
		set
		{
			hasPosition = true;
			m_Position = value;
		}
	}

	public Color color
	{
		get
		{
			return m_Color;
		}
		set
		{
			hasColor = true;
			m_Color = value;
		}
	}

	public Vector3 normal
	{
		get
		{
			return m_Normal;
		}
		set
		{
			hasNormal = true;
			m_Normal = value;
		}
	}

	public Vector4 tangent
	{
		get
		{
			return m_Tangent;
		}
		set
		{
			hasTangent = true;
			m_Tangent = value;
		}
	}

	public Vector2 uv0
	{
		get
		{
			return m_UV0;
		}
		set
		{
			hasUV0 = true;
			m_UV0 = value;
		}
	}

	public Vector2 uv2
	{
		get
		{
			return m_UV2;
		}
		set
		{
			hasUV2 = true;
			m_UV2 = value;
		}
	}

	public Vector4 uv3
	{
		get
		{
			return m_UV3;
		}
		set
		{
			hasUV3 = true;
			m_UV3 = value;
		}
	}

	public Vector4 uv4
	{
		get
		{
			return m_UV4;
		}
		set
		{
			hasUV4 = true;
			m_UV4 = value;
		}
	}

	internal MeshArrays attributes => m_Attributes;

	private bool hasPosition
	{
		get
		{
			return (m_Attributes & MeshArrays.Position) == MeshArrays.Position;
		}
		set
		{
			m_Attributes = (value ? (m_Attributes | MeshArrays.Position) : (m_Attributes & ~MeshArrays.Position));
		}
	}

	private bool hasColor
	{
		get
		{
			return (m_Attributes & MeshArrays.Color) == MeshArrays.Color;
		}
		set
		{
			m_Attributes = (value ? (m_Attributes | MeshArrays.Color) : (m_Attributes & ~MeshArrays.Color));
		}
	}

	private bool hasNormal
	{
		get
		{
			return (m_Attributes & MeshArrays.Normal) == MeshArrays.Normal;
		}
		set
		{
			m_Attributes = (value ? (m_Attributes | MeshArrays.Normal) : (m_Attributes & ~MeshArrays.Normal));
		}
	}

	private bool hasTangent
	{
		get
		{
			return (m_Attributes & MeshArrays.Tangent) == MeshArrays.Tangent;
		}
		set
		{
			m_Attributes = (value ? (m_Attributes | MeshArrays.Tangent) : (m_Attributes & ~MeshArrays.Tangent));
		}
	}

	private bool hasUV0
	{
		get
		{
			return (m_Attributes & MeshArrays.Texture0) == MeshArrays.Texture0;
		}
		set
		{
			m_Attributes = (value ? (m_Attributes | MeshArrays.Texture0) : (m_Attributes & ~MeshArrays.Texture0));
		}
	}

	private bool hasUV2
	{
		get
		{
			return (m_Attributes & MeshArrays.Texture1) == MeshArrays.Texture1;
		}
		set
		{
			m_Attributes = (value ? (m_Attributes | MeshArrays.Texture1) : (m_Attributes & ~MeshArrays.Texture1));
		}
	}

	private bool hasUV3
	{
		get
		{
			return (m_Attributes & MeshArrays.Texture2) == MeshArrays.Texture2;
		}
		set
		{
			m_Attributes = (value ? (m_Attributes | MeshArrays.Texture2) : (m_Attributes & ~MeshArrays.Texture2));
		}
	}

	private bool hasUV4
	{
		get
		{
			return (m_Attributes & MeshArrays.Texture3) == MeshArrays.Texture3;
		}
		set
		{
			m_Attributes = (value ? (m_Attributes | MeshArrays.Texture3) : (m_Attributes & ~MeshArrays.Texture3));
		}
	}

	public bool HasArrays(MeshArrays attribute)
	{
		return (m_Attributes & attribute) == attribute;
	}

	public Vertex()
	{
	}

	public override bool Equals(object obj)
	{
		if (obj is Vertex)
		{
			return Equals((Vertex)obj);
		}
		return false;
	}

	public bool Equals(Vertex other)
	{
		if ((object)other == null)
		{
			return false;
		}
		if (m_Position.Approx3(other.m_Position) && m_Color.ApproxC(other.m_Color) && m_Normal.Approx3(other.m_Normal) && m_Tangent.Approx4(other.m_Tangent) && m_UV0.Approx2(other.m_UV0) && m_UV2.Approx2(other.m_UV2) && m_UV3.Approx4(other.m_UV3))
		{
			return m_UV4.Approx4(other.m_UV4);
		}
		return false;
	}

	public bool Equals(Vertex other, MeshArrays mask)
	{
		if ((object)other == null)
		{
			return false;
		}
		if (((mask & MeshArrays.Position) != MeshArrays.Position || m_Position.Approx3(other.m_Position)) && ((mask & MeshArrays.Color) != MeshArrays.Color || m_Color.ApproxC(other.m_Color)) && ((mask & MeshArrays.Normal) != MeshArrays.Normal || m_Normal.Approx3(other.m_Normal)) && ((mask & MeshArrays.Tangent) != MeshArrays.Tangent || m_Tangent.Approx4(other.m_Tangent)) && ((mask & MeshArrays.Texture0) != MeshArrays.Texture0 || m_UV0.Approx2(other.m_UV0)) && ((mask & MeshArrays.Texture1) != MeshArrays.Texture1 || m_UV2.Approx2(other.m_UV2)) && ((mask & MeshArrays.Texture2) != MeshArrays.Texture2 || m_UV3.Approx4(other.m_UV3)))
		{
			if ((mask & MeshArrays.Texture3) == MeshArrays.Texture3)
			{
				return m_UV4.Approx4(other.m_UV4);
			}
			return true;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ((783 + VectorHash.GetHashCode(position)) * 29 + VectorHash.GetHashCode(uv0)) * 31 + VectorHash.GetHashCode(normal);
	}

	public Vertex(Vertex vertex)
	{
		if (vertex == null)
		{
			throw new ArgumentNullException("vertex");
		}
		m_Position = vertex.m_Position;
		hasPosition = vertex.hasPosition;
		m_Color = vertex.m_Color;
		hasColor = vertex.hasColor;
		m_UV0 = vertex.m_UV0;
		hasUV0 = vertex.hasUV0;
		m_Normal = vertex.m_Normal;
		hasNormal = vertex.hasNormal;
		m_Tangent = vertex.m_Tangent;
		hasTangent = vertex.hasTangent;
		m_UV2 = vertex.m_UV2;
		hasUV2 = vertex.hasUV2;
		m_UV3 = vertex.m_UV3;
		hasUV3 = vertex.hasUV3;
		m_UV4 = vertex.m_UV4;
		hasUV4 = vertex.hasUV4;
	}

	public static bool operator ==(Vertex a, Vertex b)
	{
		if ((object)a == b)
		{
			return true;
		}
		if ((object)a == null || (object)b == null)
		{
			return false;
		}
		return a.Equals(b);
	}

	public static bool operator !=(Vertex a, Vertex b)
	{
		return !(a == b);
	}

	public static Vertex operator +(Vertex a, Vertex b)
	{
		return Add(a, b);
	}

	public static Vertex Add(Vertex a, Vertex b)
	{
		Vertex vertex = new Vertex(a);
		vertex.Add(b);
		return vertex;
	}

	public void Add(Vertex b)
	{
		if (b == null)
		{
			throw new ArgumentNullException("b");
		}
		m_Position += b.m_Position;
		m_Color += b.m_Color;
		m_Normal += b.m_Normal;
		m_Tangent += b.m_Tangent;
		m_UV0 += b.m_UV0;
		m_UV2 += b.m_UV2;
		m_UV3 += b.m_UV3;
		m_UV4 += b.m_UV4;
	}

	public static Vertex operator -(Vertex a, Vertex b)
	{
		return Subtract(a, b);
	}

	public static Vertex Subtract(Vertex a, Vertex b)
	{
		Vertex vertex = new Vertex(a);
		vertex.Subtract(b);
		return vertex;
	}

	public void Subtract(Vertex b)
	{
		if (b == null)
		{
			throw new ArgumentNullException("b");
		}
		m_Position -= b.m_Position;
		m_Color -= b.m_Color;
		m_Normal -= b.m_Normal;
		m_Tangent -= b.m_Tangent;
		m_UV0 -= b.m_UV0;
		m_UV2 -= b.m_UV2;
		m_UV3 -= b.m_UV3;
		m_UV4 -= b.m_UV4;
	}

	public static Vertex operator *(Vertex a, float value)
	{
		return Multiply(a, value);
	}

	public static Vertex Multiply(Vertex a, float value)
	{
		Vertex vertex = new Vertex(a);
		vertex.Multiply(value);
		return vertex;
	}

	public void Multiply(float value)
	{
		m_Position *= value;
		m_Color *= value;
		m_Normal *= value;
		m_Tangent *= value;
		m_UV0 *= value;
		m_UV2 *= value;
		m_UV3 *= value;
		m_UV4 *= value;
	}

	public static Vertex operator /(Vertex a, float value)
	{
		return Divide(a, value);
	}

	public static Vertex Divide(Vertex a, float value)
	{
		Vertex vertex = new Vertex(a);
		vertex.Divide(value);
		return vertex;
	}

	public void Divide(float value)
	{
		m_Position /= value;
		m_Color /= value;
		m_Normal /= value;
		m_Tangent /= value;
		m_UV0 /= value;
		m_UV2 /= value;
		m_UV3 /= value;
		m_UV4 /= value;
	}

	public void Normalize()
	{
		m_Position.Normalize();
		Vector4 vector = m_Color;
		vector.Normalize();
		m_Color = vector;
		m_Normal.Normalize();
		m_Tangent.Normalize();
		m_UV0.Normalize();
		m_UV2.Normalize();
		m_UV3.Normalize();
		m_UV4.Normalize();
	}

	public string ToString(string args = null)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (hasPosition)
		{
			stringBuilder.AppendLine("position: " + m_Position.ToString(args));
		}
		if (hasColor)
		{
			stringBuilder.AppendLine("color: " + m_Color.ToString(args));
		}
		if (hasNormal)
		{
			stringBuilder.AppendLine("normal: " + m_Normal.ToString(args));
		}
		if (hasTangent)
		{
			stringBuilder.AppendLine("tangent: " + m_Tangent.ToString(args));
		}
		if (hasUV0)
		{
			stringBuilder.AppendLine("uv0: " + m_UV0.ToString(args));
		}
		if (hasUV2)
		{
			stringBuilder.AppendLine("uv2: " + m_UV2.ToString(args));
		}
		if (hasUV3)
		{
			stringBuilder.AppendLine("uv3: " + m_UV3.ToString(args));
		}
		if (hasUV4)
		{
			stringBuilder.AppendLine("uv4: " + m_UV4.ToString(args));
		}
		return stringBuilder.ToString();
	}

	public static void GetArrays(IList<Vertex> vertices, out Vector3[] position, out Color[] color, out Vector2[] uv0, out Vector3[] normal, out Vector4[] tangent, out Vector2[] uv2, out List<Vector4> uv3, out List<Vector4> uv4)
	{
		GetArrays(vertices, out position, out color, out uv0, out normal, out tangent, out uv2, out uv3, out uv4, MeshArrays.All);
	}

	public static void GetArrays(IList<Vertex> vertices, out Vector3[] position, out Color[] color, out Vector2[] uv0, out Vector3[] normal, out Vector4[] tangent, out Vector2[] uv2, out List<Vector4> uv3, out List<Vector4> uv4, MeshArrays attributes)
	{
		if (vertices == null)
		{
			throw new ArgumentNullException("vertices");
		}
		int count = vertices.Count;
		Vertex vertex = ((count < 1) ? new Vertex() : vertices[0]);
		bool flag = (attributes & MeshArrays.Position) == MeshArrays.Position && vertex.hasPosition;
		bool flag2 = (attributes & MeshArrays.Color) == MeshArrays.Color && vertex.hasColor;
		bool flag3 = (attributes & MeshArrays.Texture0) == MeshArrays.Texture0 && vertex.hasUV0;
		bool flag4 = (attributes & MeshArrays.Normal) == MeshArrays.Normal && vertex.hasNormal;
		bool flag5 = (attributes & MeshArrays.Tangent) == MeshArrays.Tangent && vertex.hasTangent;
		bool flag6 = (attributes & MeshArrays.Texture1) == MeshArrays.Texture1 && vertex.hasUV2;
		bool flag7 = (attributes & MeshArrays.Texture2) == MeshArrays.Texture2 && vertex.hasUV3;
		bool flag8 = (attributes & MeshArrays.Texture3) == MeshArrays.Texture3 && vertex.hasUV4;
		position = (flag ? new Vector3[count] : null);
		color = (flag2 ? new Color[count] : null);
		uv0 = (flag3 ? new Vector2[count] : null);
		normal = (flag4 ? new Vector3[count] : null);
		tangent = (flag5 ? new Vector4[count] : null);
		uv2 = (flag6 ? new Vector2[count] : null);
		uv3 = (flag7 ? new List<Vector4>(count) : null);
		uv4 = (flag8 ? new List<Vector4>(count) : null);
		for (int i = 0; i < count; i++)
		{
			if (flag)
			{
				position[i] = vertices[i].m_Position;
			}
			if (flag2)
			{
				color[i] = vertices[i].m_Color;
			}
			if (flag3)
			{
				uv0[i] = vertices[i].m_UV0;
			}
			if (flag4)
			{
				normal[i] = vertices[i].m_Normal;
			}
			if (flag5)
			{
				tangent[i] = vertices[i].m_Tangent;
			}
			if (flag6)
			{
				uv2[i] = vertices[i].m_UV2;
			}
			if (flag7)
			{
				uv3.Add(vertices[i].m_UV3);
			}
			if (flag8)
			{
				uv4.Add(vertices[i].m_UV4);
			}
		}
	}

	public static void SetMesh(Mesh mesh, IList<Vertex> vertices)
	{
		if (mesh == null)
		{
			throw new ArgumentNullException("mesh");
		}
		if (vertices == null)
		{
			throw new ArgumentNullException("vertices");
		}
		Vector3[] vertices2 = null;
		Color[] colors = null;
		Vector2[] uv = null;
		Vector3[] normals = null;
		Vector4[] tangents = null;
		Vector2[] array = null;
		List<Vector4> list = null;
		List<Vector4> list2 = null;
		GetArrays(vertices, out vertices2, out colors, out uv, out normals, out tangents, out array, out list, out list2);
		mesh.Clear();
		Vertex vertex = vertices[0];
		if (vertex.hasPosition)
		{
			mesh.vertices = vertices2;
		}
		if (vertex.hasColor)
		{
			mesh.colors = colors;
		}
		if (vertex.hasUV0)
		{
			mesh.uv = uv;
		}
		if (vertex.hasNormal)
		{
			mesh.normals = normals;
		}
		if (vertex.hasTangent)
		{
			mesh.tangents = tangents;
		}
		if (vertex.hasUV2)
		{
			mesh.uv2 = array;
		}
		if (vertex.hasUV3 && list != null)
		{
			mesh.SetUVs(2, list);
		}
		if (vertex.hasUV4 && list2 != null)
		{
			mesh.SetUVs(3, list2);
		}
		mesh.indexFormat = ((mesh.vertexCount > 65535) ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16);
	}

	public static Vertex Average(IList<Vertex> vertices, IList<int> indexes = null)
	{
		if (vertices == null)
		{
			throw new ArgumentNullException("vertices");
		}
		Vertex vertex = new Vertex();
		int num = indexes?.Count ?? vertices.Count;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		int num8 = 0;
		int num9 = 0;
		for (int i = 0; i < num; i++)
		{
			int index = indexes?[i] ?? i;
			if (vertices[index].hasPosition)
			{
				num2++;
				vertex.m_Position += vertices[index].m_Position;
			}
			if (vertices[index].hasColor)
			{
				num3++;
				vertex.m_Color += vertices[index].m_Color;
			}
			if (vertices[index].hasUV0)
			{
				num4++;
				vertex.m_UV0 += vertices[index].m_UV0;
			}
			if (vertices[index].hasNormal)
			{
				num5++;
				vertex.m_Normal += vertices[index].m_Normal;
			}
			if (vertices[index].hasTangent)
			{
				num6++;
				vertex.m_Tangent += vertices[index].m_Tangent;
			}
			if (vertices[index].hasUV2)
			{
				num7++;
				vertex.m_UV2 += vertices[index].m_UV2;
			}
			if (vertices[index].hasUV3)
			{
				num8++;
				vertex.m_UV3 += vertices[index].m_UV3;
			}
			if (vertices[index].hasUV4)
			{
				num9++;
				vertex.m_UV4 += vertices[index].m_UV4;
			}
		}
		if (num2 > 0)
		{
			vertex.hasPosition = true;
			vertex.m_Position *= 1f / (float)num2;
		}
		if (num3 > 0)
		{
			vertex.hasColor = true;
			vertex.m_Color *= 1f / (float)num3;
		}
		if (num4 > 0)
		{
			vertex.hasUV0 = true;
			vertex.m_UV0 *= 1f / (float)num4;
		}
		if (num5 > 0)
		{
			vertex.hasNormal = true;
			vertex.m_Normal *= 1f / (float)num5;
		}
		if (num6 > 0)
		{
			vertex.hasTangent = true;
			vertex.m_Tangent *= 1f / (float)num6;
		}
		if (num7 > 0)
		{
			vertex.hasUV2 = true;
			vertex.m_UV2 *= 1f / (float)num7;
		}
		if (num8 > 0)
		{
			vertex.hasUV3 = true;
			vertex.m_UV3 *= 1f / (float)num8;
		}
		if (num9 > 0)
		{
			vertex.hasUV4 = true;
			vertex.m_UV4 *= 1f / (float)num9;
		}
		return vertex;
	}

	public static Vertex Mix(Vertex x, Vertex y, float weight)
	{
		if (x == null || y == null)
		{
			throw new ArgumentNullException("x", "Mix does accept null vertices.");
		}
		float num = 1f - weight;
		Vertex vertex = new Vertex();
		vertex.m_Position = x.m_Position * num + y.m_Position * weight;
		if (x.hasColor && y.hasColor)
		{
			vertex.m_Color = x.m_Color * num + y.m_Color * weight;
		}
		else if (x.hasColor)
		{
			vertex.m_Color = x.m_Color;
		}
		else if (y.hasColor)
		{
			vertex.m_Color = y.m_Color;
		}
		if (x.hasNormal && y.hasNormal)
		{
			vertex.m_Normal = x.m_Normal * num + y.m_Normal * weight;
		}
		else if (x.hasNormal)
		{
			vertex.m_Normal = x.m_Normal;
		}
		else if (y.hasNormal)
		{
			vertex.m_Normal = y.m_Normal;
		}
		if (x.hasTangent && y.hasTangent)
		{
			vertex.m_Tangent = x.m_Tangent * num + y.m_Tangent * weight;
		}
		else if (x.hasTangent)
		{
			vertex.m_Tangent = x.m_Tangent;
		}
		else if (y.hasTangent)
		{
			vertex.m_Tangent = y.m_Tangent;
		}
		if (x.hasUV0 && y.hasUV0)
		{
			vertex.m_UV0 = x.m_UV0 * num + y.m_UV0 * weight;
		}
		else if (x.hasUV0)
		{
			vertex.m_UV0 = x.m_UV0;
		}
		else if (y.hasUV0)
		{
			vertex.m_UV0 = y.m_UV0;
		}
		if (x.hasUV2 && y.hasUV2)
		{
			vertex.m_UV2 = x.m_UV2 * num + y.m_UV2 * weight;
		}
		else if (x.hasUV2)
		{
			vertex.m_UV2 = x.m_UV2;
		}
		else if (y.hasUV2)
		{
			vertex.m_UV2 = y.m_UV2;
		}
		if (x.hasUV3 && y.hasUV3)
		{
			vertex.m_UV3 = x.m_UV3 * num + y.m_UV3 * weight;
		}
		else if (x.hasUV3)
		{
			vertex.m_UV3 = x.m_UV3;
		}
		else if (y.hasUV3)
		{
			vertex.m_UV3 = y.m_UV3;
		}
		if (x.hasUV4 && y.hasUV4)
		{
			vertex.m_UV4 = x.m_UV4 * num + y.m_UV4 * weight;
		}
		else if (x.hasUV4)
		{
			vertex.m_UV4 = x.m_UV4;
		}
		else if (y.hasUV4)
		{
			vertex.m_UV4 = y.m_UV4;
		}
		return vertex;
	}
}
