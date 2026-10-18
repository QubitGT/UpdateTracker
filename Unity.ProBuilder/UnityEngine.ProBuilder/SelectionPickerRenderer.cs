using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering;

namespace UnityEngine.ProBuilder;

internal static class SelectionPickerRenderer
{
	internal interface ISelectionPickerRenderer
	{
		Texture2D RenderLookupTexture(Camera camera, Shader shader, string tag, int width, int height);
	}

	internal class SelectionPickerRendererHDRP : ISelectionPickerRenderer
	{
		public Texture2D RenderLookupTexture(Camera camera, Shader shader, string tag, int width = -1, int height = -1)
		{
			return null;
		}
	}

	internal class SelectionPickerRendererStandard : ISelectionPickerRenderer
	{
		public Texture2D RenderLookupTexture(Camera camera, Shader shader, string tag, int width = -1, int height = -1)
		{
			int num;
			int num2;
			if (width >= 0)
			{
				num = ((height < 0) ? 1 : 0);
				if (num == 0)
				{
					num2 = width;
					goto IL_0024;
				}
			}
			else
			{
				num = 1;
			}
			num2 = (int)camera.pixelRect.width;
			goto IL_0024;
			IL_0024:
			int num3 = num2;
			int num4 = ((num != 0) ? ((int)camera.pixelRect.height) : height);
			GameObject gameObject = new GameObject();
			Camera camera2 = gameObject.AddComponent<Camera>();
			camera2.CopyFrom(camera);
			camera2.renderingPath = RenderingPath.Forward;
			camera2.enabled = false;
			camera2.clearFlags = CameraClearFlags.Color;
			camera2.backgroundColor = Color.white;
			camera2.allowHDR = false;
			camera2.allowMSAA = false;
			camera2.forceIntoRenderTexture = true;
			RenderTextureDescriptor desc = default(RenderTextureDescriptor);
			desc.width = num3;
			desc.height = num4;
			desc.colorFormat = renderTextureFormat;
			desc.autoGenerateMips = false;
			desc.depthBufferBits = 16;
			desc.dimension = TextureDimension.Tex2D;
			desc.enableRandomWrite = false;
			desc.memoryless = RenderTextureMemoryless.None;
			desc.sRGB = false;
			desc.useMipMap = false;
			desc.volumeDepth = 1;
			desc.msaaSamples = 1;
			RenderTexture temporary = RenderTexture.GetTemporary(desc);
			RenderTexture active = RenderTexture.active;
			camera2.targetTexture = temporary;
			RenderTexture.active = temporary;
			RenderPipelineAsset defaultRenderPipeline = GraphicsSettings.defaultRenderPipeline;
			RenderPipelineAsset renderPipeline = QualitySettings.renderPipeline;
			GraphicsSettings.defaultRenderPipeline = null;
			QualitySettings.renderPipeline = null;
			camera2.RenderWithShader(shader, tag);
			GraphicsSettings.defaultRenderPipeline = defaultRenderPipeline;
			QualitySettings.renderPipeline = renderPipeline;
			Texture2D texture2D = new Texture2D(num3, num4, textureFormat, mipChain: false, linear: false);
			texture2D.ReadPixels(new Rect(0f, 0f, num3, num4), 0, 0);
			texture2D.Apply();
			RenderTexture.active = active;
			RenderTexture.ReleaseTemporary(temporary);
			Object.DestroyImmediate(gameObject);
			return texture2D;
		}
	}

	private const string k_FacePickerOcclusionTintUniform = "_Tint";

	private static readonly Color k_Blackf = new Color(0f, 0f, 0f, 1f);

	private static readonly Color k_Whitef = new Color(1f, 1f, 1f, 1f);

	private const uint k_PickerHashNone = 0u;

	private const uint k_PickerHashMin = 1u;

	private const uint k_PickerHashMax = 16777215u;

	private const uint k_MinEdgePixelsForValidSelection = 1u;

	private static bool s_Initialized = false;

	private static ISelectionPickerRenderer s_PickerRenderer = null;

	private static RenderTextureFormat s_RenderTextureFormat = RenderTextureFormat.Default;

	private static RenderTextureFormat[] s_PreferredFormats = new RenderTextureFormat[2]
	{
		RenderTextureFormat.ARGB32,
		RenderTextureFormat.ARGBFloat
	};

	private static RenderTextureFormat renderTextureFormat
	{
		get
		{
			if (s_Initialized)
			{
				return s_RenderTextureFormat;
			}
			s_Initialized = true;
			for (int i = 0; i < s_PreferredFormats.Length; i++)
			{
				if (SystemInfo.SupportsRenderTextureFormat(s_PreferredFormats[i]))
				{
					s_RenderTextureFormat = s_PreferredFormats[i];
					break;
				}
			}
			return s_RenderTextureFormat;
		}
	}

	private static TextureFormat textureFormat => TextureFormat.ARGB32;

	private static ISelectionPickerRenderer pickerRenderer
	{
		get
		{
			if (s_PickerRenderer == null)
			{
				ISelectionPickerRenderer selectionPickerRenderer2;
				if (!ShouldUseHDRP())
				{
					ISelectionPickerRenderer selectionPickerRenderer = new SelectionPickerRendererStandard();
					selectionPickerRenderer2 = selectionPickerRenderer;
				}
				else
				{
					ISelectionPickerRenderer selectionPickerRenderer = new SelectionPickerRendererHDRP();
					selectionPickerRenderer2 = selectionPickerRenderer;
				}
				s_PickerRenderer = selectionPickerRenderer2;
			}
			return s_PickerRenderer;
		}
	}

	public static Dictionary<ProBuilderMesh, HashSet<Face>> PickFacesInRect(Camera camera, Rect pickerRect, IList<ProBuilderMesh> selection, int renderTextureWidth = -1, int renderTextureHeight = -1)
	{
		Dictionary<uint, SimpleTuple<ProBuilderMesh, Face>> map;
		Texture2D texture2D = RenderSelectionPickerTexture(camera, selection, out map, renderTextureWidth, renderTextureHeight);
		Color32[] pixels = texture2D.GetPixels32();
		int num = System.Math.Max(0, Mathf.FloorToInt(pickerRect.x));
		int num2 = System.Math.Max(0, Mathf.FloorToInt((float)texture2D.height - pickerRect.y - pickerRect.height));
		int width = texture2D.width;
		int height = texture2D.height;
		int num3 = Mathf.FloorToInt(pickerRect.width);
		int num4 = Mathf.FloorToInt(pickerRect.height);
		Object.DestroyImmediate(texture2D);
		Dictionary<ProBuilderMesh, HashSet<Face>> dictionary = new Dictionary<ProBuilderMesh, HashSet<Face>>();
		HashSet<Face> value = null;
		HashSet<uint> hashSet = new HashSet<uint>();
		for (int i = num2; i < System.Math.Min(num2 + num4, height); i++)
		{
			for (int j = num; j < System.Math.Min(num + num3, width); j++)
			{
				uint num5 = DecodeRGBA(pixels[i * width + j]);
				if (hashSet.Add(num5) && map.TryGetValue(num5, out var value2))
				{
					if (dictionary.TryGetValue(value2.item1, out value))
					{
						value.Add(value2.item2);
						continue;
					}
					dictionary.Add(value2.item1, new HashSet<Face> { value2.item2 });
				}
			}
		}
		return dictionary;
	}

	public static Dictionary<ProBuilderMesh, HashSet<int>> PickVerticesInRect(Camera camera, Rect pickerRect, IList<ProBuilderMesh> selection, bool doDepthTest, int renderTextureWidth = -1, int renderTextureHeight = -1)
	{
		Dictionary<ProBuilderMesh, HashSet<int>> dictionary = new Dictionary<ProBuilderMesh, HashSet<int>>();
		Dictionary<uint, SimpleTuple<ProBuilderMesh, int>> map;
		Texture2D texture2D = RenderSelectionPickerTexture(camera, selection, doDepthTest, out map, renderTextureWidth, renderTextureHeight);
		Color32[] pixels = texture2D.GetPixels32();
		int num = System.Math.Max(0, Mathf.FloorToInt(pickerRect.x));
		int num2 = System.Math.Max(0, Mathf.FloorToInt((float)texture2D.height - pickerRect.y - pickerRect.height));
		int width = texture2D.width;
		int height = texture2D.height;
		int num3 = Mathf.FloorToInt(pickerRect.width);
		int num4 = Mathf.FloorToInt(pickerRect.height);
		Object.DestroyImmediate(texture2D);
		HashSet<int> value = null;
		HashSet<uint> hashSet = new HashSet<uint>();
		for (int i = num2; i < System.Math.Min(num2 + num4, height); i++)
		{
			for (int j = num; j < System.Math.Min(num + num3, width); j++)
			{
				uint num5 = DecodeRGBA(pixels[i * width + j]);
				if (hashSet.Add(num5) && map.TryGetValue(num5, out var value2))
				{
					if (dictionary.TryGetValue(value2.item1, out value))
					{
						value.Add(value2.item2);
						continue;
					}
					dictionary.Add(value2.item1, new HashSet<int> { value2.item2 });
				}
			}
		}
		Dictionary<ProBuilderMesh, HashSet<int>> dictionary2 = new Dictionary<ProBuilderMesh, HashSet<int>>();
		foreach (KeyValuePair<ProBuilderMesh, HashSet<int>> item in dictionary)
		{
			Vector3[] positions = item.Key.positionsInternal;
			SharedVertex[] sharedVertices = item.Key.sharedVerticesInternal;
			HashSet<int> hashSet2 = new HashSet<int>(item.Value.Select((int x) => VectorHash.GetHashCode(positions[sharedVertices[x][0]])));
			HashSet<int> hashSet3 = new HashSet<int>();
			int k = 0;
			for (int num6 = sharedVertices.Length; k < num6; k++)
			{
				int hashCode = VectorHash.GetHashCode(positions[sharedVertices[k][0]]);
				if (hashSet2.Contains(hashCode))
				{
					hashSet3.Add(k);
				}
			}
			dictionary2.Add(item.Key, hashSet3);
		}
		return dictionary2;
	}

	public static Dictionary<ProBuilderMesh, HashSet<Edge>> PickEdgesInRect(Camera camera, Rect pickerRect, IList<ProBuilderMesh> selection, bool doDepthTest, int renderTextureWidth = -1, int renderTextureHeight = -1)
	{
		Dictionary<ProBuilderMesh, HashSet<Edge>> dictionary = new Dictionary<ProBuilderMesh, HashSet<Edge>>();
		Dictionary<uint, SimpleTuple<ProBuilderMesh, Edge>> map;
		Texture2D texture2D = RenderSelectionPickerTexture(camera, selection, doDepthTest, out map, renderTextureWidth, renderTextureHeight);
		Color32[] pixels = texture2D.GetPixels32();
		int num = System.Math.Max(0, Mathf.FloorToInt(pickerRect.x));
		int num2 = System.Math.Max(0, Mathf.FloorToInt((float)texture2D.height - pickerRect.y - pickerRect.height));
		int width = texture2D.width;
		int height = texture2D.height;
		int num3 = Mathf.FloorToInt(pickerRect.width);
		int num4 = Mathf.FloorToInt(pickerRect.height);
		Object.DestroyImmediate(texture2D);
		Dictionary<uint, uint> dictionary2 = new Dictionary<uint, uint>();
		for (int i = num2; i < System.Math.Min(num2 + num4, height); i++)
		{
			for (int j = num; j < System.Math.Min(num + num3, width); j++)
			{
				uint num5 = DecodeRGBA(pixels[i * width + j]);
				if (num5 != 0 && num5 != 16777215)
				{
					if (!dictionary2.ContainsKey(num5))
					{
						dictionary2.Add(num5, 1u);
					}
					else
					{
						dictionary2[num5]++;
					}
				}
			}
		}
		foreach (KeyValuePair<uint, uint> item in dictionary2)
		{
			if (item.Value > 1 && map.TryGetValue(item.Key, out var value))
			{
				HashSet<Edge> value2 = null;
				if (dictionary.TryGetValue(value.item1, out value2))
				{
					value2.Add(value.item2);
					continue;
				}
				dictionary.Add(value.item1, new HashSet<Edge> { value.item2 });
			}
		}
		return dictionary;
	}

	internal static Texture2D RenderSelectionPickerTexture(Camera camera, IList<ProBuilderMesh> selection, out Dictionary<uint, SimpleTuple<ProBuilderMesh, Face>> map, int width = -1, int height = -1)
	{
		GameObject[] array = GenerateFacePickingObjects(selection, out map);
		BuiltinMaterials.facePickerMaterial.SetColor("_Tint", k_Whitef);
		Texture2D result = pickerRenderer.RenderLookupTexture(camera, BuiltinMaterials.selectionPickerShader, "ProBuilderPicker", width, height);
		GameObject[] array2 = array;
		foreach (GameObject obj in array2)
		{
			Object.DestroyImmediate(obj.GetComponent<MeshFilter>().sharedMesh);
			Object.DestroyImmediate(obj);
		}
		return result;
	}

	internal static Texture2D RenderSelectionPickerTexture(Camera camera, IList<ProBuilderMesh> selection, bool doDepthTest, out Dictionary<uint, SimpleTuple<ProBuilderMesh, int>> map, int width = -1, int height = -1)
	{
		GenerateVertexPickingObjects(selection, doDepthTest, out map, out var depthObjects, out var pickerObjects);
		BuiltinMaterials.facePickerMaterial.SetColor("_Tint", k_Blackf);
		Texture2D result = pickerRenderer.RenderLookupTexture(camera, BuiltinMaterials.selectionPickerShader, "ProBuilderPicker", width, height);
		int i = 0;
		for (int num = pickerObjects.Length; i < num; i++)
		{
			Object.DestroyImmediate(pickerObjects[i].GetComponent<MeshFilter>().sharedMesh);
			Object.DestroyImmediate(pickerObjects[i]);
		}
		if (doDepthTest)
		{
			int j = 0;
			for (int num2 = depthObjects.Length; j < num2; j++)
			{
				Object.DestroyImmediate(depthObjects[j]);
			}
		}
		return result;
	}

	internal static Texture2D RenderSelectionPickerTexture(Camera camera, IList<ProBuilderMesh> selection, bool doDepthTest, out Dictionary<uint, SimpleTuple<ProBuilderMesh, Edge>> map, int width = -1, int height = -1)
	{
		GenerateEdgePickingObjects(selection, doDepthTest, out map, out var depthObjects, out var pickerObjects);
		BuiltinMaterials.facePickerMaterial.SetColor("_Tint", k_Blackf);
		Texture2D result = pickerRenderer.RenderLookupTexture(camera, BuiltinMaterials.selectionPickerShader, "ProBuilderPicker", width, height);
		int i = 0;
		for (int num = pickerObjects.Length; i < num; i++)
		{
			Object.DestroyImmediate(pickerObjects[i].GetComponent<MeshFilter>().sharedMesh);
			Object.DestroyImmediate(pickerObjects[i]);
		}
		if (doDepthTest)
		{
			int j = 0;
			for (int num2 = depthObjects.Length; j < num2; j++)
			{
				Object.DestroyImmediate(depthObjects[j]);
			}
		}
		return result;
	}

	private static GameObject[] GenerateFacePickingObjects(IList<ProBuilderMesh> selection, out Dictionary<uint, SimpleTuple<ProBuilderMesh, Face>> map)
	{
		int count = selection.Count;
		GameObject[] array = new GameObject[count];
		map = new Dictionary<uint, SimpleTuple<ProBuilderMesh, Face>>();
		uint num = 0u;
		for (int i = 0; i < count; i++)
		{
			ProBuilderMesh proBuilderMesh = selection[i];
			Mesh mesh = new Mesh();
			mesh.vertices = proBuilderMesh.positionsInternal;
			mesh.triangles = proBuilderMesh.facesInternal.SelectMany((Face x) => x.indexesInternal).ToArray();
			Color32[] array2 = new Color32[mesh.vertexCount];
			Face[] facesInternal = proBuilderMesh.facesInternal;
			foreach (Face face in facesInternal)
			{
				Color32 color = EncodeRGBA(num++);
				map.Add(DecodeRGBA(color), new SimpleTuple<ProBuilderMesh, Face>(proBuilderMesh, face));
				for (int k = 0; k < face.distinctIndexesInternal.Length; k++)
				{
					array2[face.distinctIndexesInternal[k]] = color;
				}
			}
			mesh.colors32 = array2;
			GameObject gameObject = InternalUtility.MeshGameObjectWithTransform(proBuilderMesh.name + " (Face Depth Test)", proBuilderMesh.transform, mesh, BuiltinMaterials.facePickerMaterial, inheritParent: true);
			array[i] = gameObject;
		}
		return array;
	}

	private static void GenerateVertexPickingObjects(IList<ProBuilderMesh> selection, bool doDepthTest, out Dictionary<uint, SimpleTuple<ProBuilderMesh, int>> map, out GameObject[] depthObjects, out GameObject[] pickerObjects)
	{
		map = new Dictionary<uint, SimpleTuple<ProBuilderMesh, int>>();
		uint index = 2u;
		int count = selection.Count;
		pickerObjects = new GameObject[count];
		for (int i = 0; i < count; i++)
		{
			ProBuilderMesh proBuilderMesh = selection[i];
			Mesh mesh = BuildVertexMesh(proBuilderMesh, map, ref index);
			GameObject gameObject = InternalUtility.MeshGameObjectWithTransform(proBuilderMesh.name + " (Vertex Billboards)", proBuilderMesh.transform, mesh, BuiltinMaterials.vertexPickerMaterial, inheritParent: true);
			pickerObjects[i] = gameObject;
		}
		if (doDepthTest)
		{
			depthObjects = new GameObject[count];
			for (int j = 0; j < count; j++)
			{
				ProBuilderMesh proBuilderMesh2 = selection[j];
				GameObject gameObject2 = InternalUtility.MeshGameObjectWithTransform(proBuilderMesh2.name + " (Depth Mask)", proBuilderMesh2.transform, proBuilderMesh2.mesh, BuiltinMaterials.facePickerMaterial, inheritParent: true);
				depthObjects[j] = gameObject2;
			}
		}
		else
		{
			depthObjects = null;
		}
	}

	private static void GenerateEdgePickingObjects(IList<ProBuilderMesh> selection, bool doDepthTest, out Dictionary<uint, SimpleTuple<ProBuilderMesh, Edge>> map, out GameObject[] depthObjects, out GameObject[] pickerObjects)
	{
		map = new Dictionary<uint, SimpleTuple<ProBuilderMesh, Edge>>();
		uint index = 2u;
		int count = selection.Count;
		pickerObjects = new GameObject[count];
		for (int i = 0; i < count; i++)
		{
			ProBuilderMesh proBuilderMesh = selection[i];
			Mesh mesh = BuildEdgeMesh(proBuilderMesh, map, ref index);
			GameObject gameObject = InternalUtility.MeshGameObjectWithTransform(proBuilderMesh.name + " (Edge Billboards)", proBuilderMesh.transform, mesh, BuiltinMaterials.edgePickerMaterial, inheritParent: true);
			pickerObjects[i] = gameObject;
		}
		if (doDepthTest)
		{
			depthObjects = new GameObject[count];
			for (int j = 0; j < count; j++)
			{
				ProBuilderMesh proBuilderMesh2 = selection[j];
				GameObject gameObject2 = InternalUtility.MeshGameObjectWithTransform(proBuilderMesh2.name + " (Depth Mask)", proBuilderMesh2.transform, proBuilderMesh2.mesh, BuiltinMaterials.facePickerMaterial, inheritParent: true);
				depthObjects[j] = gameObject2;
			}
		}
		else
		{
			depthObjects = null;
		}
	}

	private static Mesh BuildVertexMesh(ProBuilderMesh pb, Dictionary<uint, SimpleTuple<ProBuilderMesh, int>> map, ref uint index)
	{
		int num = System.Math.Min(pb.sharedVerticesInternal.Length, 16382);
		Vector3[] array = new Vector3[num * 4];
		Vector2[] array2 = new Vector2[num * 4];
		Vector2[] array3 = new Vector2[num * 4];
		Color[] array4 = new Color[num * 4];
		int[] array5 = new int[num * 6];
		int num2 = 0;
		int num3 = 0;
		Vector3 up = Vector3.up;
		Vector3 right = Vector3.right;
		for (int i = 0; i < num; i++)
		{
			array[num3 + 3] = (array[num3 + 2] = (array[num3 + 1] = (array[num3] = pb.positionsInternal[pb.sharedVerticesInternal[i][0]])));
			array2[num3] = Vector3.zero;
			array2[num3 + 1] = Vector3.right;
			array2[num3 + 2] = Vector3.up;
			array2[num3 + 3] = Vector3.one;
			array3[num3] = -up - right;
			array3[num3 + 1] = -up + right;
			array3[num3 + 2] = up - right;
			array3[num3 + 3] = up + right;
			array5[num2] = num3;
			array5[num2 + 1] = num3 + 1;
			array5[num2 + 2] = num3 + 2;
			array5[num2 + 3] = num3 + 1;
			array5[num2 + 4] = num3 + 3;
			array5[num2 + 5] = num3 + 2;
			Color32 color = EncodeRGBA(index);
			map.Add(index++, new SimpleTuple<ProBuilderMesh, int>(pb, i));
			array4[num3] = color;
			array4[num3 + 1] = color;
			array4[num3 + 2] = color;
			array4[num3 + 3] = color;
			num3 += 4;
			num2 += 6;
		}
		return new Mesh
		{
			name = "Vertex Billboard",
			vertices = array,
			uv = array2,
			uv2 = array3,
			colors = array4,
			triangles = array5
		};
	}

	private static Mesh BuildEdgeMesh(ProBuilderMesh pb, Dictionary<uint, SimpleTuple<ProBuilderMesh, Edge>> map, ref uint index)
	{
		int num = 0;
		int faceCount = pb.faceCount;
		for (int i = 0; i < faceCount; i++)
		{
			num += pb.facesInternal[i].edgesInternal.Length;
		}
		int num2 = System.Math.Min(num, 32766);
		Vector3[] array = new Vector3[num2 * 2];
		Color32[] array2 = new Color32[num2 * 2];
		int[] array3 = new int[num2 * 2];
		int num3 = 0;
		for (int j = 0; j < faceCount; j++)
		{
			if (num3 >= num2)
			{
				break;
			}
			for (int k = 0; k < pb.facesInternal[j].edgesInternal.Length; k++)
			{
				if (num3 >= num2)
				{
					break;
				}
				Edge item = pb.facesInternal[j].edgesInternal[k];
				Vector3 vector = pb.positionsInternal[item.a];
				Vector3 vector2 = pb.positionsInternal[item.b];
				int num4 = num3 * 2;
				array[num4] = vector;
				array[num4 + 1] = vector2;
				Color32 color = EncodeRGBA(index);
				map.Add(index++, new SimpleTuple<ProBuilderMesh, Edge>(pb, item));
				array2[num4] = color;
				array2[num4 + 1] = color;
				array3[num4] = num4;
				array3[num4 + 1] = num4 + 1;
				num3++;
			}
		}
		Mesh mesh = new Mesh();
		mesh.name = "Edge Billboard";
		mesh.vertices = array;
		mesh.colors32 = array2;
		mesh.subMeshCount = 1;
		mesh.SetIndices(array3, MeshTopology.Lines, 0);
		return mesh;
	}

	public static uint DecodeRGBA(Color32 color)
	{
		uint r = color.r;
		uint g = color.g;
		uint b = color.b;
		if (BitConverter.IsLittleEndian)
		{
			return (r << 16) | (g << 8) | b;
		}
		return (r << 24) | (g << 16) | (b << 8);
	}

	public static Color32 EncodeRGBA(uint hash)
	{
		if (BitConverter.IsLittleEndian)
		{
			return new Color32((byte)((hash >> 16) & 0xFFu), (byte)((hash >> 8) & 0xFFu), (byte)(hash & 0xFFu), byte.MaxValue);
		}
		return new Color32((byte)((hash >> 24) & 0xFFu), (byte)((hash >> 16) & 0xFFu), (byte)((hash >> 8) & 0xFFu), byte.MaxValue);
	}

	private static bool ShouldUseHDRP()
	{
		return false;
	}
}
