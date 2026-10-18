namespace UnityEngine.ProBuilder.Csg;

internal struct Vertex
{
	private Vector3 m_Position;

	private Color m_Color;

	private Vector3 m_Normal;

	private Vector4 m_Tangent;

	private Vector2 m_UV0;

	private Vector2 m_UV2;

	private Vector4 m_UV3;

	private Vector4 m_UV4;

	private VertexAttributes m_Attributes;

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

	public bool hasPosition
	{
		get
		{
			return (m_Attributes & VertexAttributes.Position) == VertexAttributes.Position;
		}
		private set
		{
			m_Attributes = (value ? (m_Attributes | VertexAttributes.Position) : (m_Attributes & ~VertexAttributes.Position));
		}
	}

	public bool hasColor
	{
		get
		{
			return (m_Attributes & VertexAttributes.Color) == VertexAttributes.Color;
		}
		private set
		{
			m_Attributes = (value ? (m_Attributes | VertexAttributes.Color) : (m_Attributes & ~VertexAttributes.Color));
		}
	}

	public bool hasNormal
	{
		get
		{
			return (m_Attributes & VertexAttributes.Normal) == VertexAttributes.Normal;
		}
		private set
		{
			m_Attributes = (value ? (m_Attributes | VertexAttributes.Normal) : (m_Attributes & ~VertexAttributes.Normal));
		}
	}

	public bool hasTangent
	{
		get
		{
			return (m_Attributes & VertexAttributes.Tangent) == VertexAttributes.Tangent;
		}
		private set
		{
			m_Attributes = (value ? (m_Attributes | VertexAttributes.Tangent) : (m_Attributes & ~VertexAttributes.Tangent));
		}
	}

	public bool hasUV0
	{
		get
		{
			return (m_Attributes & VertexAttributes.Texture0) == VertexAttributes.Texture0;
		}
		private set
		{
			m_Attributes = (value ? (m_Attributes | VertexAttributes.Texture0) : (m_Attributes & ~VertexAttributes.Texture0));
		}
	}

	public bool hasUV2
	{
		get
		{
			return (m_Attributes & VertexAttributes.Texture1) == VertexAttributes.Texture1;
		}
		private set
		{
			m_Attributes = (value ? (m_Attributes | VertexAttributes.Texture1) : (m_Attributes & ~VertexAttributes.Texture1));
		}
	}

	public bool hasUV3
	{
		get
		{
			return (m_Attributes & VertexAttributes.Texture2) == VertexAttributes.Texture2;
		}
		private set
		{
			m_Attributes = (value ? (m_Attributes | VertexAttributes.Texture2) : (m_Attributes & ~VertexAttributes.Texture2));
		}
	}

	public bool hasUV4
	{
		get
		{
			return (m_Attributes & VertexAttributes.Texture3) == VertexAttributes.Texture3;
		}
		private set
		{
			m_Attributes = (value ? (m_Attributes | VertexAttributes.Texture3) : (m_Attributes & ~VertexAttributes.Texture3));
		}
	}

	public bool HasArrays(VertexAttributes attribute)
	{
		return (m_Attributes & attribute) == attribute;
	}

	public void Flip()
	{
		if (hasNormal)
		{
			m_Normal *= -1f;
		}
		if (hasTangent)
		{
			m_Tangent *= -1f;
		}
	}
}
