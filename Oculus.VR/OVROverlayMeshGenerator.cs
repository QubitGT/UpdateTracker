using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteAlways]
[HelpURL("https://developer.oculus.com/documentation/unity/unity-sf-stereo180video/")]
public class OVROverlayMeshGenerator : MonoBehaviour
{
	private enum CubeFace
	{
		Bottom,
		Front,
		Back,
		Right,
		Left,
		Top,
		COUNT
	}

	private readonly List<int> _Tris = new List<int>();

	private readonly List<Vector2> _UV = new List<Vector2>();

	private readonly List<Vector4> _CubeUV = new List<Vector4>();

	private readonly List<Vector3> _Verts = new List<Vector3>();

	private Transform _CameraRoot;

	private Rect _LastDestRectLeft;

	private Vector3 _LastPosition;

	private Quaternion _LastRotation;

	private Vector3 _LastScale;

	private TextureDimension _LastTextureDimension = TextureDimension.Tex2D;

	private OVROverlay.OverlayShape _LastShape;

	private Rect _LastSrcRectLeft;

	private Mesh _Mesh;

	private MeshCollider _MeshCollider;

	private MeshFilter _MeshFilter;

	private MeshRenderer _MeshRenderer;

	private OVROverlay _Overlay;

	private Transform _Transform;

	private Material _PreviewMaterial;

	protected void OnEnable()
	{
		Initialize();
	}

	protected void OnDestroy()
	{
		if (_Mesh != null)
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(_Mesh);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(_Mesh);
			}
		}
		if (_PreviewMaterial != null)
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(_PreviewMaterial);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(_PreviewMaterial);
			}
		}
	}

	private void Initialize()
	{
		_MeshFilter = GetComponent<MeshFilter>();
		_MeshRenderer = GetComponent<MeshRenderer>();
		_MeshCollider = GetComponent<MeshCollider>();
		_Transform = base.transform;
		if ((bool)Camera.main && (bool)Camera.main.transform.parent)
		{
			_CameraRoot = Camera.main.transform.parent;
		}
		TryUpdateMesh();
	}

	public void SetOverlay(OVROverlay overlay)
	{
		_Overlay = overlay;
		Initialize();
	}

	private void TryUpdateMesh()
	{
		if (_Overlay == null)
		{
			return;
		}
		if (_Mesh == null)
		{
			_Mesh = new Mesh
			{
				name = "Overlay"
			};
			_Mesh.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
		}
		if (_Transform == null)
		{
			_Transform = base.transform;
		}
		OVROverlay.OverlayShape currentOverlayShape = _Overlay.currentOverlayShape;
		Vector3 vector = (_CameraRoot ? (_Transform.position - _CameraRoot.position) : _Transform.position);
		Quaternion rotation = _Transform.rotation;
		Vector3 lossyScale = _Transform.lossyScale;
		Rect rect = (_Overlay.overrideTextureRectMatrix ? _Overlay.destRectLeft : new Rect(0f, 0f, 1f, 1f));
		Rect rect2 = (_Overlay.overrideTextureRectMatrix ? _Overlay.srcRectLeft : new Rect(0f, 0f, 1f, 1f));
		Texture texture = _Overlay.textures[0];
		TextureDimension textureDimension = ((texture != null) ? texture.dimension : TextureDimension.Tex2D);
		if (_LastShape != currentOverlayShape || _LastPosition != vector || _LastRotation != rotation || _LastScale != lossyScale || _LastDestRectLeft != rect || _LastTextureDimension != textureDimension)
		{
			UpdateMesh(currentOverlayShape, vector, rotation, lossyScale, rect, textureDimension == TextureDimension.Cube);
		}
		if ((bool)_MeshRenderer)
		{
			if (_MeshRenderer.sharedMaterial == null || textureDimension != _LastTextureDimension)
			{
				if (_PreviewMaterial != null)
				{
					if (Application.isPlaying)
					{
						UnityEngine.Object.Destroy(_PreviewMaterial);
					}
					else
					{
						UnityEngine.Object.DestroyImmediate(_PreviewMaterial);
					}
				}
				_PreviewMaterial = null;
				switch (textureDimension)
				{
				case TextureDimension.Tex2D:
					_PreviewMaterial = new Material(Shader.Find("Unlit/Transparent"));
					break;
				case TextureDimension.Cube:
					_PreviewMaterial = new Material(Shader.Find("Hidden/CubeCopy"));
					break;
				}
				if (_PreviewMaterial != null)
				{
					_PreviewMaterial.mainTexture = texture;
				}
				_MeshRenderer.sharedMaterial = _PreviewMaterial;
			}
			if (_LastSrcRectLeft != rect2)
			{
				_MeshRenderer.sharedMaterial.mainTextureOffset = rect2.position;
				_MeshRenderer.sharedMaterial.mainTextureScale = rect2.size;
			}
			if (_MeshRenderer.sharedMaterial.mainTexture != texture)
			{
				_MeshRenderer.sharedMaterial.mainTexture = texture;
			}
		}
		if ((bool)_MeshFilter)
		{
			_MeshFilter.sharedMesh = _Mesh;
		}
		if ((bool)_MeshCollider)
		{
			_MeshCollider.sharedMesh = _Mesh;
		}
		_LastShape = currentOverlayShape;
		_LastPosition = vector;
		_LastRotation = rotation;
		_LastScale = lossyScale;
		_LastDestRectLeft = rect;
		_LastSrcRectLeft = rect2;
		_LastTextureDimension = textureDimension;
	}

	private void UpdateMesh(OVROverlay.OverlayShape shape, Vector3 position, Quaternion rotation, Vector3 scale, Rect rect, bool cubemap = false)
	{
		_Verts.Clear();
		_UV.Clear();
		_CubeUV.Clear();
		_Tris.Clear();
		GenerateMesh(_Verts, _UV, _CubeUV, _Tris, shape, position, rotation, scale, rect);
		_Mesh.Clear(keepVertexLayout: false);
		_Mesh.SetVertices(_Verts);
		if (cubemap)
		{
			_Mesh.SetUVs(0, _CubeUV);
		}
		else
		{
			_Mesh.SetUVs(0, _UV);
		}
		_Mesh.SetTriangles(_Tris, 0);
		_Mesh.UploadMeshData(markNoLongerReadable: false);
	}

	public static void GenerateMesh(List<Vector3> verts, List<Vector2> uvs, List<Vector4> cubeUVs, List<int> tris, OVROverlay.OverlayShape shape, Vector3 position, Quaternion rotation, Vector3 scale, Rect rect)
	{
		switch (shape)
		{
		case OVROverlay.OverlayShape.Equirect:
			BuildSphere(verts, uvs, tris, position, rotation, scale, rect);
			break;
		case OVROverlay.OverlayShape.Cubemap:
		case OVROverlay.OverlayShape.OffcenterCubemap:
			BuildCube(verts, uvs, cubeUVs, tris, position, rotation, scale);
			break;
		case OVROverlay.OverlayShape.Quad:
			BuildQuad(verts, uvs, tris, rect);
			break;
		case OVROverlay.OverlayShape.Cylinder:
			BuildHemicylinder(verts, uvs, tris, scale, rect);
			break;
		case (OVROverlay.OverlayShape)3:
			break;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Vector3 InverseTransformVert(in Vector3 vert, in Vector3 position, in Vector3 scale, float worldScale)
	{
		return new Vector3((worldScale * vert.x - position.x) / scale.x, (worldScale * vert.y - position.y) / scale.y, (worldScale * vert.z - position.z) / scale.z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Vector2 GetSphereUV(float theta, float phi, float expandScale)
	{
		float x = expandScale * (theta / (MathF.PI * 2f) - 0.5f) + 0.5f;
		float y = expandScale * phi / MathF.PI + 0.5f;
		return new Vector2(x, y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Vector3 GetSphereVert(float theta, float phi)
	{
		return new Vector3((0f - Mathf.Sin(theta)) * Mathf.Cos(phi), Mathf.Sin(phi), (0f - Mathf.Cos(theta)) * Mathf.Cos(phi));
	}

	public static void BuildSphere(List<Vector3> verts, List<Vector2> uv, List<int> triangles, Vector3 position, Quaternion rotation, Vector3 scale, Rect rect, float worldScale = 800f, int latitudes = 128, int longitudes = 128, float expandCoefficient = 1f)
	{
		position = Quaternion.Inverse(rotation) * position;
		latitudes = Mathf.CeilToInt((float)latitudes * rect.height);
		longitudes = Mathf.CeilToInt((float)longitudes * rect.width);
		float num = MathF.PI * 2f * rect.x;
		float num2 = MathF.PI * (0.5f - rect.y - rect.height);
		float num3 = MathF.PI * 2f * rect.width / (float)longitudes;
		float num4 = MathF.PI * rect.height / (float)latitudes;
		float expandScale = 1f / expandCoefficient;
		for (int i = 0; i < latitudes + 1; i++)
		{
			for (int j = 0; j < longitudes + 1; j++)
			{
				float theta = num + (float)j * num3;
				float phi = num2 + (float)i * num4;
				Vector2 sphereUV = GetSphereUV(theta, phi, expandScale);
				uv.Add(new Vector2((sphereUV.x - rect.x) / rect.width, (sphereUV.y - rect.y) / rect.height));
				Vector3 vert = GetSphereVert(theta, phi);
				verts.Add(InverseTransformVert(in vert, in position, in scale, worldScale));
			}
		}
		for (int k = 0; k < latitudes; k++)
		{
			for (int l = 0; l < longitudes; l++)
			{
				triangles.Add(k * (longitudes + 1) + l);
				triangles.Add((k + 1) * (longitudes + 1) + l);
				triangles.Add((k + 1) * (longitudes + 1) + l + 1);
				triangles.Add((k + 1) * (longitudes + 1) + l + 1);
				triangles.Add(k * (longitudes + 1) + l + 1);
				triangles.Add(k * (longitudes + 1) + l);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Vector2 GetCubeUV(CubeFace face, float sideU, float sideV, float expandScale, float expandOffset)
	{
		sideU = sideU * expandScale + expandOffset;
		sideV = sideV * expandScale + expandOffset;
		return face switch
		{
			CubeFace.Bottom => new Vector2(sideU / 3f, sideV / 2f), 
			CubeFace.Front => new Vector2((1f + sideU) / 3f, sideV / 2f), 
			CubeFace.Back => new Vector2((2f + sideU) / 3f, sideV / 2f), 
			CubeFace.Right => new Vector2(sideU / 3f, (1f + sideV) / 2f), 
			CubeFace.Left => new Vector2((1f + sideU) / 3f, (1f + sideV) / 2f), 
			CubeFace.Top => new Vector2((2f + sideU) / 3f, (1f + sideV) / 2f), 
			_ => Vector2.zero, 
		};
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static Vector3 GetCubeVert(CubeFace face, float sideU, float sideV)
	{
		return face switch
		{
			CubeFace.Bottom => new Vector3(0.5f - sideU, -0.5f, 0.5f - sideV), 
			CubeFace.Front => new Vector3(0.5f - sideU, -0.5f + sideV, -0.5f), 
			CubeFace.Back => new Vector3(-0.5f + sideU, -0.5f + sideV, 0.5f), 
			CubeFace.Right => new Vector3(-0.5f, -0.5f + sideV, -0.5f + sideU), 
			CubeFace.Left => new Vector3(0.5f, -0.5f + sideV, 0.5f - sideU), 
			CubeFace.Top => new Vector3(0.5f - sideU, 0.5f, -0.5f + sideV), 
			_ => Vector3.zero, 
		};
	}

	public static void BuildCube(List<Vector3> verts, List<Vector2> uv, List<Vector4> cubeUV, List<int> triangles, Vector3 position, Quaternion rotation, Vector3 scale, float worldScale = 800f, int subQuads = 1, float expandCoefficient = 1.01f)
	{
		position = Quaternion.Inverse(rotation) * position;
		int num = (subQuads + 1) * (subQuads + 1);
		float expandScale = 1f / expandCoefficient;
		float expandOffset = 0.5f - 0.5f / expandCoefficient;
		for (int i = 0; i < 6; i++)
		{
			for (int j = 0; j < subQuads + 1; j++)
			{
				for (int k = 0; k < subQuads + 1; k++)
				{
					float sideU = (float)j / (float)subQuads;
					float sideV = (float)k / (float)subQuads;
					uv.Add(GetCubeUV((CubeFace)i, sideU, sideV, expandScale, expandOffset));
					Vector3 vert = GetCubeVert((CubeFace)i, sideU, sideV);
					verts.Add(InverseTransformVert(in vert, in position, in scale, worldScale));
					cubeUV.Add(vert.normalized);
				}
			}
			for (int l = 0; l < subQuads; l++)
			{
				for (int m = 0; m < subQuads; m++)
				{
					triangles.Add(num * i + (l + 1) * (subQuads + 1) + m);
					triangles.Add(num * i + l * (subQuads + 1) + m);
					triangles.Add(num * i + (l + 1) * (subQuads + 1) + m + 1);
					triangles.Add(num * i + (l + 1) * (subQuads + 1) + m + 1);
					triangles.Add(num * i + l * (subQuads + 1) + m);
					triangles.Add(num * i + l * (subQuads + 1) + m + 1);
				}
			}
		}
	}

	public static void BuildQuad(List<Vector3> verts, List<Vector2> uv, List<int> triangles, Rect rect)
	{
		verts.Add(new Vector3(rect.x - 0.5f, 1f - rect.y - rect.height - 0.5f, 0f));
		verts.Add(new Vector3(rect.x - 0.5f, 1f - rect.y - 0.5f, 0f));
		verts.Add(new Vector3(rect.x + rect.width - 0.5f, 1f - rect.y - 0.5f, 0f));
		verts.Add(new Vector3(rect.x + rect.width - 0.5f, 1f - rect.y - rect.height - 0.5f, 0f));
		uv.Add(new Vector2(0f, 0f));
		uv.Add(new Vector2(0f, 1f));
		uv.Add(new Vector2(1f, 1f));
		uv.Add(new Vector2(1f, 0f));
		triangles.Add(0);
		triangles.Add(1);
		triangles.Add(2);
		triangles.Add(2);
		triangles.Add(3);
		triangles.Add(0);
	}

	public static void BuildHemicylinder(List<Vector3> verts, List<Vector2> uv, List<int> triangles, Vector3 scale, Rect rect, int longitudes = 128)
	{
		float num = Mathf.Abs(scale.y) * rect.height;
		float z = scale.z;
		float num2 = scale.x * rect.width;
		float num3 = num2 / z;
		float num4 = scale.x * (-0.5f + rect.x) / z;
		int num5 = Mathf.CeilToInt((float)longitudes * num3 / (MathF.PI * 2f));
		float num6 = num2 / (float)num5;
		int num7 = Mathf.CeilToInt(num / num6 / 2f);
		for (int i = 0; i < num7 + 1; i++)
		{
			for (int j = 0; j < num5 + 1; j++)
			{
				uv.Add(new Vector2((float)j / (float)num5, 1f - (float)i / (float)num7));
				verts.Add(new Vector3(Mathf.Sin(num4 + (float)j * num3 / (float)num5) * z / scale.x, 0.5f - rect.y - rect.height + rect.height * (1f - (float)i / (float)num7), Mathf.Cos(num4 + (float)j * num3 / (float)num5) * z / scale.z));
			}
		}
		for (int k = 0; k < num7; k++)
		{
			for (int l = 0; l < num5; l++)
			{
				triangles.Add(k * (num5 + 1) + l);
				triangles.Add((k + 1) * (num5 + 1) + l + 1);
				triangles.Add((k + 1) * (num5 + 1) + l);
				triangles.Add((k + 1) * (num5 + 1) + l + 1);
				triangles.Add(k * (num5 + 1) + l);
				triangles.Add(k * (num5 + 1) + l + 1);
			}
		}
	}
}
