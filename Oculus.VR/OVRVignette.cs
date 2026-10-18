using System;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
[ExecuteAlways]
[HelpURL("https://developer.oculus.com/reference/unity/latest/class_o_v_r_vignette")]
public class OVRVignette : MonoBehaviour
{
	public enum MeshComplexityLevel
	{
		VerySimple,
		Simple,
		Normal,
		Detailed,
		VeryDetailed
	}

	public enum FalloffType
	{
		Linear,
		Quadratic
	}

	private static readonly string QUADRATIC_FALLOFF = "QUADRATIC_FALLOFF";

	[SerializeField]
	[HideInInspector]
	private Shader VignetteShader;

	[SerializeField]
	[Tooltip("Controls the number of triangles used for the vignette mesh. Normal is best for most purposes.")]
	private MeshComplexityLevel MeshComplexity = MeshComplexityLevel.Normal;

	[SerializeField]
	[Tooltip("Controls how the falloff looks.")]
	private FalloffType Falloff;

	[Tooltip("The Vertical FOV of the vignette")]
	public float VignetteFieldOfView = 60f;

	[Tooltip("The Aspect ratio of the vignette controls the Horizontal FOV. (Larger numbers are wider)")]
	public float VignetteAspectRatio = 1f;

	[Tooltip("The width of the falloff for the vignette in degrees")]
	public float VignetteFalloffDegrees = 10f;

	[ColorUsage(false)]
	[Tooltip("The color of the vignette. Alpha value is ignored")]
	public Color VignetteColor;

	[Tooltip("Whether the Vignette Should write to the Stencil Buffer.")]
	public bool WriteStencil;

	[Tooltip("If WriteStencil is enabled, the stencil value for the opaque portion of the vignette")]
	public int OpaqueStencilValue;

	[Tooltip("If WriteStencil is enabled, the stencil value for the transparent portion of the vignette")]
	public int TransparentStencilValue;

	[Tooltip("If the Vignette should write color, or only depth/stencil.")]
	public bool WriteColor = true;

	private Camera _Camera;

	private MeshFilter _OpaqueMeshFilter;

	private MeshFilter _TransparentMeshFilter;

	private MeshRenderer _OpaqueMeshRenderer;

	private MeshRenderer _TransparentMeshRenderer;

	private Mesh _OpaqueMesh;

	private Mesh _TransparentMesh;

	private Material _OpaqueMaterial;

	private Material _TransparentMaterial;

	private int _ShaderScaleAndOffset0Property;

	private int _ShaderScaleAndOffset1Property;

	private int _ShaderStencilRefProperty;

	private int _ShaderStencilOpProperty;

	private int _ShaderColorMaskProperty;

	private Vector4[] _TransparentScaleAndOffset0 = new Vector4[2];

	private Vector4[] _TransparentScaleAndOffset1 = new Vector4[2];

	private Vector4[] _OpaqueScaleAndOffset0 = new Vector4[2];

	private Vector4[] _OpaqueScaleAndOffset1 = new Vector4[2];

	private bool _OpaqueVignetteVisible;

	private bool _TransparentVignetteVisible;

	private int GetTriangleCount()
	{
		return MeshComplexity switch
		{
			MeshComplexityLevel.VerySimple => 32, 
			MeshComplexityLevel.Simple => 64, 
			MeshComplexityLevel.Normal => 128, 
			MeshComplexityLevel.Detailed => 256, 
			MeshComplexityLevel.VeryDetailed => 512, 
			_ => 128, 
		};
	}

	private void BuildMeshes()
	{
		int triangleCount = GetTriangleCount();
		Vector3[] array = new Vector3[triangleCount];
		Vector2[] array2 = new Vector2[triangleCount];
		Vector3[] array3 = new Vector3[triangleCount];
		Vector2[] array4 = new Vector2[triangleCount];
		int[] array5 = new int[triangleCount * 3];
		for (int i = 0; i < triangleCount; i += 2)
		{
			float f = (float)(2 * i) * MathF.PI / (float)triangleCount;
			float x = Mathf.Cos(f);
			float y = Mathf.Sin(f);
			array3[i] = new Vector3(x, y, 0f);
			array3[i + 1] = new Vector3(x, y, 0f);
			array4[i] = new Vector2(0f, 1f);
			array4[i + 1] = new Vector2(1f, 1f);
			array[i] = new Vector3(x, y, 0f);
			array[i + 1] = new Vector3(x, y, 0f);
			array2[i] = new Vector2(0f, 1f);
			array2[i + 1] = new Vector2(1f, 0f);
			int num = i * 3;
			array5[num] = i;
			array5[num + 1] = i + 1;
			array5[num + 2] = (i + 2) % triangleCount;
			array5[num + 3] = i + 1;
			array5[num + 4] = (i + 3) % triangleCount;
			array5[num + 5] = (i + 2) % triangleCount;
		}
		if (_OpaqueMesh != null)
		{
			UnityEngine.Object.DestroyImmediate(_OpaqueMesh);
		}
		if (_TransparentMesh != null)
		{
			UnityEngine.Object.DestroyImmediate(_TransparentMesh);
		}
		_OpaqueMesh = new Mesh
		{
			name = "Opaque Vignette Mesh",
			hideFlags = HideFlags.HideAndDontSave
		};
		_TransparentMesh = new Mesh
		{
			name = "Transparent Vignette Mesh",
			hideFlags = HideFlags.HideAndDontSave
		};
		_OpaqueMesh.vertices = array3;
		_OpaqueMesh.uv = array4;
		_OpaqueMesh.triangles = array5;
		_OpaqueMesh.UploadMeshData(markNoLongerReadable: true);
		_OpaqueMesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10000f);
		_OpaqueMeshFilter.sharedMesh = _OpaqueMesh;
		_TransparentMesh.vertices = array;
		_TransparentMesh.uv = array2;
		_TransparentMesh.triangles = array5;
		_TransparentMesh.UploadMeshData(markNoLongerReadable: true);
		_TransparentMesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10000f);
		_TransparentMeshFilter.sharedMesh = _TransparentMesh;
	}

	private void BuildMaterials()
	{
		if (VignetteShader == null)
		{
			VignetteShader = Shader.Find("Oculus/OVRVignette");
		}
		if (VignetteShader == null)
		{
			Debug.LogError("Could not find Vignette Shader! Vignette will not be drawn!");
			return;
		}
		if (_OpaqueMaterial == null)
		{
			_OpaqueMaterial = new Material(VignetteShader)
			{
				name = "Opaque Vignette Material",
				hideFlags = HideFlags.HideAndDontSave,
				renderQueue = 1000
			};
			_OpaqueMaterial.SetInt("_BlendSrc", 1);
			_OpaqueMaterial.SetInt("_BlendDst", 0);
			_OpaqueMaterial.SetInt("_ZWrite", 1);
		}
		_OpaqueMeshRenderer.sharedMaterial = _OpaqueMaterial;
		if (_TransparentMaterial == null)
		{
			_TransparentMaterial = new Material(VignetteShader)
			{
				name = "Transparent Vignette Material",
				hideFlags = HideFlags.HideAndDontSave,
				renderQueue = 4000
			};
			_TransparentMaterial.SetInt("_BlendSrc", 5);
			_TransparentMaterial.SetInt("_BlendDst", 10);
			_TransparentMaterial.SetInt("_ZWrite", 0);
		}
		if (Falloff == FalloffType.Quadratic)
		{
			_TransparentMaterial.EnableKeyword(QUADRATIC_FALLOFF);
		}
		else
		{
			_TransparentMaterial.DisableKeyword(QUADRATIC_FALLOFF);
		}
		_TransparentMeshRenderer.sharedMaterial = _TransparentMaterial;
	}

	private void OnEnable()
	{
		RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
	}

	private void OnDisable()
	{
		RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
		DisableRenderers();
	}

	private void Awake()
	{
		Initialize();
	}

	private void Initialize()
	{
		if (!(_OpaqueMeshRenderer != null) || !(_TransparentMeshRenderer != null))
		{
			_Camera = GetComponent<Camera>();
			_ShaderScaleAndOffset0Property = Shader.PropertyToID("_ScaleAndOffset0");
			_ShaderScaleAndOffset1Property = Shader.PropertyToID("_ScaleAndOffset1");
			_ShaderStencilRefProperty = Shader.PropertyToID("_StencilRef");
			_ShaderStencilOpProperty = Shader.PropertyToID("_StencilOp");
			_ShaderColorMaskProperty = Shader.PropertyToID("_ColorMask");
			GameObject gameObject = new GameObject("Opaque Vignette")
			{
				hideFlags = HideFlags.HideAndDontSave
			};
			gameObject.transform.SetParent(_Camera.transform, worldPositionStays: false);
			_OpaqueMeshFilter = gameObject.AddComponent<MeshFilter>();
			_OpaqueMeshRenderer = gameObject.AddComponent<MeshRenderer>();
			_OpaqueMeshRenderer.receiveShadows = false;
			_OpaqueMeshRenderer.shadowCastingMode = ShadowCastingMode.Off;
			_OpaqueMeshRenderer.lightProbeUsage = LightProbeUsage.Off;
			_OpaqueMeshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
			_OpaqueMeshRenderer.allowOcclusionWhenDynamic = false;
			_OpaqueMeshRenderer.enabled = false;
			GameObject gameObject2 = new GameObject("Transparent Vignette")
			{
				hideFlags = HideFlags.HideAndDontSave
			};
			gameObject2.transform.SetParent(_Camera.transform, worldPositionStays: false);
			_TransparentMeshFilter = gameObject2.AddComponent<MeshFilter>();
			_TransparentMeshRenderer = gameObject2.AddComponent<MeshRenderer>();
			_TransparentMeshRenderer.receiveShadows = false;
			_TransparentMeshRenderer.shadowCastingMode = ShadowCastingMode.Off;
			_TransparentMeshRenderer.lightProbeUsage = LightProbeUsage.Off;
			_TransparentMeshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
			_TransparentMeshRenderer.allowOcclusionWhenDynamic = false;
			_TransparentMeshRenderer.enabled = false;
			BuildMeshes();
			BuildMaterials();
		}
	}

	private void GetTanFovAndOffsetForStereoEye(Camera.StereoscopicEye eye, out float tanFovX, out float tanFovY, out float offsetX, out float offsetY)
	{
		Matrix4x4 transpose = _Camera.GetStereoProjectionMatrix(eye).transpose;
		Vector4 vector = transpose * new Vector4(-1f, 0f, 0f, 1f);
		Vector4 vector2 = transpose * new Vector4(1f, 0f, 0f, 1f);
		Vector4 vector3 = transpose * new Vector4(0f, -1f, 0f, 1f);
		Vector4 vector4 = transpose * new Vector4(0f, 1f, 0f, 1f);
		float num = vector.z / vector.x;
		float num2 = vector2.z / vector2.x;
		float num3 = vector3.z / vector3.y;
		float num4 = vector4.z / vector4.y;
		offsetX = (0f - (num + num2)) / 2f;
		offsetY = (0f - (num3 + num4)) / 2f;
		tanFovX = (num - num2) / 2f;
		tanFovY = (num3 - num4) / 2f;
	}

	private void GetTanFovAndOffsetForMonoEye(out float tanFovX, out float tanFovY, out float offsetX, out float offsetY)
	{
		tanFovY = Mathf.Tan(MathF.PI / 180f * _Camera.fieldOfView * 0.5f);
		tanFovX = tanFovY * _Camera.aspect;
		offsetX = 0f;
		offsetY = 0f;
	}

	private bool VisibilityTest(float scaleX, float scaleY, float offsetX, float offsetY)
	{
		return new Vector2((1f + Mathf.Abs(offsetX)) / scaleX, (1f + Mathf.Abs(offsetY)) / scaleY).sqrMagnitude > 1f;
	}

	private void Update()
	{
		if (_OpaqueMaterial == null)
		{
			return;
		}
		float num = Mathf.Tan(VignetteFieldOfView * (MathF.PI / 180f) * 0.5f);
		float num2 = num * VignetteAspectRatio;
		float num3 = Mathf.Tan((VignetteFieldOfView + VignetteFalloffDegrees) * (MathF.PI / 180f) * 0.5f);
		float num4 = num3 * VignetteAspectRatio;
		_TransparentVignetteVisible = false;
		_OpaqueVignetteVisible = false;
		for (int i = 0; i < 2; i++)
		{
			float tanFovX;
			float tanFovY;
			float offsetX;
			float offsetY;
			if (_Camera.stereoEnabled)
			{
				GetTanFovAndOffsetForStereoEye((Camera.StereoscopicEye)i, out tanFovX, out tanFovY, out offsetX, out offsetY);
			}
			else
			{
				GetTanFovAndOffsetForMonoEye(out tanFovX, out tanFovY, out offsetX, out offsetY);
			}
			float num5 = new Vector2((1f + Mathf.Abs(offsetX)) / VignetteAspectRatio, 1f + Mathf.Abs(offsetY)).magnitude * 1.01f;
			float num6 = num2 / tanFovX;
			float num7 = num / tanFovY;
			float num8 = num3 / tanFovX;
			float num9 = num4 / tanFovY;
			float x = num5 * VignetteAspectRatio;
			float y = num5;
			_TransparentVignetteVisible |= VisibilityTest(num6, num7, offsetX, offsetY);
			_OpaqueVignetteVisible |= VisibilityTest(num8, num9, offsetX, offsetY);
			_OpaqueScaleAndOffset0[i] = new Vector4(x, y, offsetX, offsetY);
			_OpaqueScaleAndOffset1[i] = new Vector4(num8, num9, offsetX, offsetY);
			_TransparentScaleAndOffset0[i] = new Vector4(num8, num9, offsetX, offsetY);
			_TransparentScaleAndOffset1[i] = new Vector4(num6, num7, offsetX, offsetY);
		}
		_TransparentVignetteVisible &= VignetteFalloffDegrees > 0f;
		_OpaqueMaterial.SetVectorArray(_ShaderScaleAndOffset0Property, _OpaqueScaleAndOffset0);
		_OpaqueMaterial.SetVectorArray(_ShaderScaleAndOffset1Property, _OpaqueScaleAndOffset1);
		_OpaqueMaterial.SetInt(_ShaderStencilOpProperty, WriteStencil ? 2 : 0);
		_OpaqueMaterial.SetInt(_ShaderStencilRefProperty, OpaqueStencilValue);
		_OpaqueMaterial.SetInt(_ShaderColorMaskProperty, WriteColor ? 15 : 0);
		_OpaqueMaterial.color = VignetteColor;
		_TransparentMaterial.SetVectorArray(_ShaderScaleAndOffset0Property, _TransparentScaleAndOffset0);
		_TransparentMaterial.SetVectorArray(_ShaderScaleAndOffset1Property, _TransparentScaleAndOffset1);
		_TransparentMaterial.SetInt(_ShaderStencilOpProperty, WriteStencil ? 2 : 0);
		_TransparentMaterial.SetInt(_ShaderStencilRefProperty, TransparentStencilValue);
		_TransparentMaterial.SetInt(_ShaderColorMaskProperty, WriteColor ? 15 : 0);
		_TransparentMaterial.renderQueue = (WriteColor ? 4000 : 1000);
		_TransparentMaterial.color = VignetteColor;
	}

	private void EnableRenderers()
	{
		Initialize();
		_OpaqueMeshRenderer.enabled = _OpaqueVignetteVisible;
		_TransparentMeshRenderer.enabled = _TransparentVignetteVisible;
	}

	private void DisableRenderers()
	{
		if (_OpaqueMeshRenderer != null)
		{
			_OpaqueMeshRenderer.enabled = false;
		}
		if (_TransparentMeshRenderer != null)
		{
			_TransparentMeshRenderer.enabled = false;
		}
	}

	private void OnPreCull()
	{
		EnableRenderers();
	}

	private void OnPostRender()
	{
		DisableRenderers();
	}

	private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
	{
		if (camera == _Camera)
		{
			EnableRenderers();
		}
		else
		{
			DisableRenderers();
		}
	}
}
