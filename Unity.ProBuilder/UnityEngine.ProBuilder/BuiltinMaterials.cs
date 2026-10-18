using System.Reflection;

namespace UnityEngine.ProBuilder;

public static class BuiltinMaterials
{
	private static bool s_IsInitialized;

	public const string faceShader = "Hidden/ProBuilder/FaceHighlight";

	public const string lineShader = "Hidden/ProBuilder/LineBillboard";

	public const string lineShaderMetal = "Hidden/ProBuilder/LineBillboardMetal";

	public const string pointShader = "Hidden/ProBuilder/PointBillboard";

	public const string wireShader = "Hidden/ProBuilder/FaceHighlight";

	public const string dotShader = "Hidden/ProBuilder/VertexShader";

	internal static readonly Color previewColor = new Color(0.5f, 0.9f, 1f, 0.56f);

	private static Shader s_SelectionPickerShader;

	private static bool s_GeometryShadersSupported;

	private static Material s_DefaultMaterial;

	private static Material s_FacePickerMaterial;

	private static Material s_VertexPickerMaterial;

	private static Material s_EdgePickerMaterial;

	private static Material s_UnityDefaultDiffuse;

	private static Material s_ShapePreviewMaterial;

	private static string k_EdgePickerMaterial = "Materials/EdgePicker";

	private static string k_FacePickerMaterial = "Materials/FacePicker";

	private static string k_VertexPickerMaterial = "Materials/VertexPicker";

	private static string k_EdgePickerShader = "Hidden/ProBuilder/EdgePicker";

	private static string k_FacePickerShader = "Hidden/ProBuilder/FacePicker";

	private static string k_VertexPickerShader = "Hidden/ProBuilder/VertexPicker";

	public static bool geometryShadersSupported
	{
		get
		{
			Init();
			return s_GeometryShadersSupported;
		}
	}

	public static Material defaultMaterial
	{
		get
		{
			Init();
			if (s_DefaultMaterial == null)
			{
				s_DefaultMaterial = GetDefaultMaterial();
			}
			return s_DefaultMaterial;
		}
	}

	internal static Shader selectionPickerShader
	{
		get
		{
			Init();
			return s_SelectionPickerShader;
		}
	}

	internal static Material facePickerMaterial
	{
		get
		{
			Init();
			return s_FacePickerMaterial;
		}
	}

	internal static Material vertexPickerMaterial
	{
		get
		{
			Init();
			return s_VertexPickerMaterial;
		}
	}

	internal static Material edgePickerMaterial
	{
		get
		{
			Init();
			return s_EdgePickerMaterial;
		}
	}

	internal static Material triggerMaterial
	{
		get
		{
			Init();
			return (Material)Resources.Load("Materials/Trigger", typeof(Material));
		}
	}

	internal static Material colliderMaterial
	{
		get
		{
			Init();
			return (Material)Resources.Load("Materials/Collider", typeof(Material));
		}
	}

	internal static Material ShapePreviewMaterial
	{
		get
		{
			if (s_ShapePreviewMaterial == null)
			{
				s_ShapePreviewMaterial = GetPreviewMaterial();
			}
			return s_ShapePreviewMaterial;
		}
	}

	private static void Init()
	{
		if (!s_IsInitialized)
		{
			s_IsInitialized = true;
			Shader shader = Shader.Find("Hidden/ProBuilder/LineBillboard");
			s_GeometryShadersSupported = shader != null && shader.isSupported;
			s_SelectionPickerShader = Shader.Find("Hidden/ProBuilder/SelectionPicker");
			if ((s_FacePickerMaterial = Resources.Load<Material>(k_FacePickerMaterial)) == null)
			{
				Log.Error("FacePicker material not loaded... please re-install ProBuilder to fix this error.");
				s_FacePickerMaterial = new Material(Shader.Find(k_FacePickerShader));
			}
			if ((s_VertexPickerMaterial = Resources.Load<Material>(k_VertexPickerMaterial)) == null)
			{
				Log.Error("VertexPicker material not loaded... please re-install ProBuilder to fix this error.");
				s_VertexPickerMaterial = new Material(Shader.Find(k_VertexPickerShader));
			}
			if ((s_EdgePickerMaterial = Resources.Load<Material>(k_EdgePickerMaterial)) == null)
			{
				Log.Error("EdgePicker material not loaded... please re-install ProBuilder to fix this error.");
				s_EdgePickerMaterial = new Material(Shader.Find(k_EdgePickerShader));
			}
		}
	}

	internal static Material GetLegacyDiffuse()
	{
		Init();
		if (s_UnityDefaultDiffuse == null)
		{
			MethodInfo method = typeof(Material).GetMethod("GetDefaultMaterial", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (method != null)
			{
				s_UnityDefaultDiffuse = method.Invoke(null, null) as Material;
			}
			if (s_UnityDefaultDiffuse == null)
			{
				GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
				s_UnityDefaultDiffuse = gameObject.GetComponent<MeshRenderer>().sharedMaterial;
				Object.DestroyImmediate(gameObject);
			}
		}
		return s_UnityDefaultDiffuse;
	}

	internal static Material GetDefaultMaterial()
	{
		Material material = (Material)Resources.Load("Materials/ProBuilderDefault", typeof(Material));
		material.shader = Shader.Find("ProBuilder6/Standard Vertex Color");
		if (material == null || !material.shader.isSupported)
		{
			material = GetLegacyDiffuse();
		}
		return material;
	}

	private static Material GetPreviewMaterial()
	{
		if (defaultMaterial == null)
		{
			return null;
		}
		Material material = new Material(defaultMaterial.shader);
		material.hideFlags = HideFlags.HideAndDontSave;
		if (material.HasProperty("_MainTex"))
		{
			material.mainTexture = (Texture2D)Resources.Load("Textures/GridBox_Default");
		}
		if (material.HasProperty("_Color"))
		{
			material.SetColor("_Color", previewColor);
		}
		return material;
	}
}
