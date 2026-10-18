using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OVRSimpleJSON;
using UnityEngine;

public class OVRGLTFLoader
{
	private const float LoadingMaxTimePerFrame = 1f / 70f;

	private readonly Func<Stream> m_deferredStream;

	private JSONNode m_jsonData;

	private Stream m_glbStream;

	private GameObject[] m_Nodes;

	private Dictionary<OVRGLTFInputNode, OVRGLTFAnimatinonNode> m_InputAnimationNodes;

	private Dictionary<int, OVRGLTFAnimatinonNode[]> m_AnimationLookup;

	private Dictionary<int, OVRGLTFAnimationNodeMorphTargetHandler> m_morphTargetHandlers;

	private Shader m_Shader = Shader.Find("Legacy Shaders/Diffuse");

	private Shader m_AlphaBlendShader = Shader.Find("Unlit/Transparent");

	private OVRTextureQualityFiltering m_TextureQuality;

	private float m_TextureMipmapBias;

	public OVRGLTFScene scene;

	public static readonly Vector3 GLTFToUnitySpace = new Vector3(-1f, 1f, 1f);

	public static readonly Vector3 GLTFToUnityTangent = new Vector4(-1f, 1f, 1f, -1f);

	public static readonly Vector4 GLTFToUnitySpace_Rotation = new Vector4(1f, -1f, -1f, 1f);

	private static Dictionary<string, OVRGLTFInputNode> InputNodeNameMap = new Dictionary<string, OVRGLTFInputNode>
	{
		{
			"button_a",
			OVRGLTFInputNode.Button_A_X
		},
		{
			"button_x",
			OVRGLTFInputNode.Button_A_X
		},
		{
			"button_b",
			OVRGLTFInputNode.Button_B_Y
		},
		{
			"button_y",
			OVRGLTFInputNode.Button_B_Y
		},
		{
			"button_oculus",
			OVRGLTFInputNode.Button_Oculus_Menu
		},
		{
			"trigger_front",
			OVRGLTFInputNode.Trigger_Front
		},
		{
			"trigger_grip",
			OVRGLTFInputNode.Trigger_Grip
		},
		{
			"thumbstick",
			OVRGLTFInputNode.ThumbStick
		}
	};

	public Func<string, Material, Texture2D> textureUriHandler;

	private Dictionary<int, Texture2D> m_textures;

	private Dictionary<int, Material> m_materials;

	private float m_processingNodesStart;

	private OVRGLTFAccessor _dataAccessor;

	public OVRGLTFLoader(string fileName)
	{
		m_glbStream = File.Open(fileName, FileMode.Open);
	}

	public OVRGLTFLoader(byte[] data)
	{
		m_glbStream = new MemoryStream(data, 0, data.Length, writable: false, publiclyVisible: true);
	}

	public OVRGLTFLoader(Func<Stream> deferredStream)
	{
		m_deferredStream = deferredStream;
	}

	public OVRGLTFScene LoadGLB(bool supportAnimation, bool loadMips = true)
	{
		IEnumerator enumerator = LoadGLBCoroutine(supportAnimation, loadMips);
		while (enumerator.MoveNext())
		{
		}
		return scene;
	}

	public IEnumerator LoadGLBCoroutine(bool supportAnimation, bool loadMips = true)
	{
		scene = default(OVRGLTFScene);
		m_InputAnimationNodes = new Dictionary<OVRGLTFInputNode, OVRGLTFAnimatinonNode>();
		m_AnimationLookup = new Dictionary<int, OVRGLTFAnimatinonNode[]>();
		m_morphTargetHandlers = new Dictionary<int, OVRGLTFAnimationNodeMorphTargetHandler>();
		m_textures = new Dictionary<int, Texture2D>();
		m_materials = new Dictionary<int, Material>();
		if (Application.isBatchMode)
		{
			Debug.Log("Batch Mode Single Threaded Loading");
			m_jsonData = InitializeGLBLoad();
		}
		else
		{
			Task<JSONNode> task = Task.Run(() => InitializeGLBLoad());
			yield return new WaitUntil(() => task.IsCompleted);
			m_jsonData = task.Result;
			if (task.IsFaulted)
			{
				Debug.LogException(task.Exception);
			}
		}
		if (m_jsonData == null || !OVRGLTFAccessor.TryCreate(m_jsonData["accessors"], m_jsonData["bufferViews"], m_jsonData["buffers"], m_glbStream, out _dataAccessor))
		{
			m_glbStream?.Close();
			yield break;
		}
		IEnumerator loadGltf = LoadGLTF(supportAnimation, loadMips);
		while (loadGltf.MoveNext())
		{
			yield return loadGltf.Current;
		}
		m_glbStream.Close();
		if (m_Nodes.Any())
		{
			scene.root.transform.Rotate(Vector3.up, 180f);
			scene.root.SetActive(value: true);
			scene.animationNodes = m_InputAnimationNodes;
			scene.animationNodeLookup = m_AnimationLookup;
			scene.morphTargetHandlers = m_morphTargetHandlers.Values.ToList();
		}
	}

	private JSONNode InitializeGLBLoad()
	{
		if (m_deferredStream != null)
		{
			m_glbStream = m_deferredStream();
		}
		if (ValidateGLB(m_glbStream))
		{
			byte[] array = ReadChunk(m_glbStream, OVRChunkType.JSON);
			if (array != null)
			{
				return JSON.Parse(Encoding.ASCII.GetString(array));
			}
		}
		return null;
	}

	public void SetModelShader(Shader shader)
	{
		m_Shader = shader;
	}

	public void SetModelAlphaBlendShader(Shader shader)
	{
		m_AlphaBlendShader = shader;
	}

	public void SetTextureQualityFiltering(OVRTextureQualityFiltering loadedTexturesQuality)
	{
		m_TextureQuality = loadedTexturesQuality;
	}

	public void SetMipMapBias(float loadedTexturesMipmapBiasing)
	{
		m_TextureMipmapBias = Mathf.Clamp(loadedTexturesMipmapBiasing, -1f, 1f);
	}

	public static OVRTextureQualityFiltering DetectTextureQuality(in Texture2D srcTexture)
	{
		OVRTextureQualityFiltering oVRTextureQualityFiltering = OVRTextureQualityFiltering.None;
		switch (srcTexture.filterMode)
		{
		case FilterMode.Point:
			return OVRTextureQualityFiltering.None;
		case FilterMode.Trilinear:
			if (srcTexture.anisoLevel <= 1)
			{
				return OVRTextureQualityFiltering.Trilinear;
			}
			if (srcTexture.anisoLevel < 4)
			{
				return OVRTextureQualityFiltering.Aniso2x;
			}
			if (srcTexture.anisoLevel < 8)
			{
				return OVRTextureQualityFiltering.Aniso4x;
			}
			if (srcTexture.anisoLevel < 16)
			{
				return OVRTextureQualityFiltering.Aniso8x;
			}
			return OVRTextureQualityFiltering.Aniso16x;
		default:
			return OVRTextureQualityFiltering.Bilinear;
		}
	}

	public static void ApplyTextureQuality(OVRTextureQualityFiltering qualityLevel, ref Texture2D destTexture)
	{
		if (!(destTexture == null))
		{
			switch (qualityLevel)
			{
			case OVRTextureQualityFiltering.None:
				destTexture.filterMode = FilterMode.Point;
				destTexture.anisoLevel = 0;
				break;
			case OVRTextureQualityFiltering.Bilinear:
				destTexture.filterMode = FilterMode.Bilinear;
				destTexture.anisoLevel = 0;
				break;
			case OVRTextureQualityFiltering.Trilinear:
				destTexture.filterMode = FilterMode.Trilinear;
				destTexture.anisoLevel = 0;
				break;
			default:
				destTexture.filterMode = FilterMode.Trilinear;
				destTexture.anisoLevel = Mathf.FloorToInt(Mathf.Pow(2f, (float)(qualityLevel - 1)));
				break;
			}
		}
	}

	public static bool ValidateGLB(Stream glbStream)
	{
		if (glbStream == null)
		{
			return false;
		}
		int num = 4;
		byte[] array = new byte[num];
		glbStream.Read(array, 0, num);
		if (BitConverter.ToUInt32(array, 0) != 1179937895)
		{
			Debug.LogError("Data stream was not a valid glTF format");
			return false;
		}
		glbStream.Read(array, 0, num);
		if (BitConverter.ToUInt32(array, 0) != 2)
		{
			Debug.LogError("Only glTF 2.0 is supported");
			return false;
		}
		glbStream.Read(array, 0, num);
		if (BitConverter.ToUInt32(array, 0) != glbStream.Length)
		{
			Debug.LogError("glTF header length does not match file length");
			return false;
		}
		return true;
	}

	public static byte[] ReadChunk(Stream glbStream, OVRChunkType type)
	{
		if (ValidateChunk(glbStream, type, out var chunkLength))
		{
			byte[] array = new byte[chunkLength];
			glbStream.Read(array, 0, (int)chunkLength);
			return array;
		}
		return null;
	}

	private static bool ValidateChunk(Stream glbStream, OVRChunkType type, out uint chunkLength)
	{
		int num = 4;
		byte[] array = new byte[num];
		glbStream.Read(array, 0, num);
		chunkLength = BitConverter.ToUInt32(array, 0);
		glbStream.Read(array, 0, num);
		if (BitConverter.ToUInt32(array, 0) != (uint)type)
		{
			Debug.LogError("Read chunk does not match type.");
			return false;
		}
		return true;
	}

	private IEnumerator LoadGLTF(bool supportAnimation, bool loadMips)
	{
		if (m_jsonData == null)
		{
			Debug.LogError("m_jsonData was null");
			yield break;
		}
		JSONNode jSONNode = m_jsonData["scenes"];
		if (jSONNode.Count == 0)
		{
			Debug.LogError("No valid scenes in this glTF.");
			yield break;
		}
		scene.root = new GameObject("GLB Scene Root");
		Transform sceneRootTransform = scene.root.transform;
		scene.root.SetActive(value: false);
		JSONArray nodes = m_jsonData["nodes"].AsArray;
		m_Nodes = new GameObject[nodes.Count];
		sceneRootTransform.hierarchyCapacity = nodes.Count;
		int num = 0;
		JSONNode.ValueEnumerator enumerator = nodes.Values.GetEnumerator();
		while (enumerator.MoveNext())
		{
			_ = enumerator.Current;
			GameObject gameObject = new GameObject();
			gameObject.transform.SetParent(sceneRootTransform, worldPositionStays: false);
			m_Nodes[num++] = gameObject;
		}
		JSONArray asArray = jSONNode[0]["nodes"].AsArray;
		m_processingNodesStart = Time.realtimeSinceStartup;
		JSONNode.Enumerator enumerator2 = asArray.GetEnumerator();
		while (enumerator2.MoveNext())
		{
			int asInt = ((JSONNode)enumerator2.Current).AsInt;
			IEnumerator processNode2 = ProcessNode(nodes, asInt, loadMips, sceneRootTransform);
			while (processNode2.MoveNext())
			{
				yield return processNode2.Current;
			}
		}
		if (supportAnimation)
		{
			IEnumerator processNode2 = ProcessAnimations();
			while (processNode2.MoveNext())
			{
				yield return processNode2.Current;
			}
		}
	}

	private IEnumerator ProcessNode(JSONArray nodes, int nodeId, bool loadMips, Transform parent)
	{
		bool hasSkipped = false;
		if (Time.realtimeSinceStartup - m_processingNodesStart > 1f / 70f)
		{
			m_processingNodesStart = Time.realtimeSinceStartup;
			hasSkipped = true;
			yield return null;
		}
		JSONNode node = nodes[nodeId];
		GameObject nodeGameObject = m_Nodes[nodeId];
		Transform nodeTransform = nodeGameObject.transform;
		string nodeName = (nodeTransform.name = node["name"].Value);
		nodeTransform.SetParent(parent, worldPositionStays: false);
		JSONArray asArray = node["children"].AsArray;
		if (asArray.Count > 0)
		{
			JSONNode.ValueEnumerator enumerator = asArray.Values.GetEnumerator();
			while (enumerator.MoveNext())
			{
				int asInt = enumerator.Current.AsInt;
				IEnumerator processNode = ProcessNode(nodes, asInt, loadMips, nodeTransform);
				while (processNode.MoveNext())
				{
					yield return processNode.Current;
				}
			}
		}
		if (nodeName.StartsWith("batteryIndicator"))
		{
			nodeGameObject.SetActive(value: false);
			yield break;
		}
		if (node["mesh"] != null)
		{
			int asInt2 = node["mesh"].AsInt;
			OVRMeshData meshData = ProcessMesh(m_jsonData["meshes"][asInt2], loadMips);
			if (node["skin"] != null)
			{
				SkinnedMeshRenderer skinnedMeshRenderer = nodeGameObject.AddComponent<SkinnedMeshRenderer>();
				skinnedMeshRenderer.sharedMesh = meshData.mesh;
				skinnedMeshRenderer.sharedMaterial = meshData.material;
				int asInt3 = node["skin"].AsInt;
				ProcessSkin(m_jsonData["skins"][asInt3], skinnedMeshRenderer);
			}
			else
			{
				nodeGameObject.AddComponent<MeshFilter>().sharedMesh = meshData.mesh;
				nodeGameObject.AddComponent<MeshRenderer>().sharedMaterial = meshData.material;
			}
			if (meshData.morphTargets != null)
			{
				m_morphTargetHandlers[nodeId] = new OVRGLTFAnimationNodeMorphTargetHandler(meshData);
			}
		}
		JSONArray asArray2 = node["translation"].AsArray;
		JSONArray asArray3 = node["rotation"].AsArray;
		JSONArray asArray4 = node["scale"].AsArray;
		if (asArray2.Count > 0 || asArray3.Count > 0)
		{
			Vector3 position = Vector3.zero;
			Quaternion rotation = Quaternion.identity;
			if (asArray2.Count > 0)
			{
				position = new Vector3((float)asArray2[0] * GLTFToUnitySpace.x, (float)asArray2[1] * GLTFToUnitySpace.y, (float)asArray2[2] * GLTFToUnitySpace.z);
			}
			if (asArray3.Count > 0)
			{
				rotation = new Quaternion((float)asArray3[0] * GLTFToUnitySpace.x * -1f, (float)asArray3[1] * GLTFToUnitySpace.y * -1f, (float)asArray3[2] * GLTFToUnitySpace.z * -1f, asArray3[3]);
			}
			nodeTransform.SetPositionAndRotation(position, rotation);
		}
		if (asArray4.Count > 0)
		{
			nodeTransform.localScale = new Vector3(asArray4[0], asArray4[1], asArray4[2]);
			nodeTransform.gameObject.SetActive(nodeTransform.gameObject.transform.localScale != Vector3.zero);
		}
		_ = Time.realtimeSinceStartup;
		_ = m_processingNodesStart;
		if (!hasSkipped && Time.realtimeSinceStartup - m_processingNodesStart > 1f / 70f)
		{
			m_processingNodesStart = Time.realtimeSinceStartup;
			yield return null;
		}
	}

	private OVRMeshData ProcessMesh(JSONNode meshNode, bool loadMips)
	{
		OVRMeshData result = default(OVRMeshData);
		int num = 0;
		JSONNode jSONNode = meshNode["primitives"];
		int[] array = new int[jSONNode.Count];
		for (int i = 0; i < jSONNode.Count; i++)
		{
			JSONNode jSONNode2 = jSONNode[i]["attributes"]["POSITION"];
			JSONNode jSONNode3 = m_jsonData["accessors"][jSONNode2.AsInt];
			array[i] = jSONNode3["count"];
			num += array[i];
		}
		int[][] array2 = new int[jSONNode.Count][];
		JSONNode jSONNode4 = jSONNode[0]["material"];
		if (jSONNode4 != null)
		{
			OVRMaterialData matData = ProcessMaterial(jSONNode4.AsInt);
			matData.texture = ProcessTexture(matData.textureId);
			TranscodeTexture(ref matData.texture);
			int asInt = jSONNode4.AsInt;
			if (m_materials.TryGetValue(asInt, out var value))
			{
				result.material = value;
			}
			else
			{
				Material material = CreateUnityMaterial(matData, loadMips);
				m_materials.Add(asInt, material);
				result.material = material;
			}
		}
		OVRMeshAttributes baseAttributes = default(OVRMeshAttributes);
		OVRMeshAttributes[] array3 = null;
		int num2 = 0;
		for (int j = 0; j < jSONNode.Count; j++)
		{
			JSONNode jSONNode5 = jSONNode[j];
			int asInt2 = jSONNode5["indices"].AsInt;
			_dataAccessor.Seek(asInt2);
			array2[j] = _dataAccessor.ReadInt();
			FlipTriangleIndices(ref array2[j]);
			baseAttributes = ReadMeshAttributes(jSONNode5["attributes"], num, num2);
			JSONNode jSONNode6 = jSONNode5["targets"];
			if (jSONNode6 != null)
			{
				array3 = new OVRMeshAttributes[jSONNode6.Count];
				for (int k = 0; k < jSONNode6.Count; k++)
				{
					array3[k] = ReadMeshAttributes(jSONNode6[k], num, num2);
				}
			}
			num2 += array[j];
		}
		Mesh mesh = new Mesh();
		mesh.vertices = baseAttributes.vertices;
		mesh.normals = baseAttributes.normals;
		mesh.tangents = baseAttributes.tangents;
		mesh.colors = baseAttributes.colors;
		mesh.uv = baseAttributes.texcoords;
		mesh.boneWeights = baseAttributes.boneWeights;
		mesh.subMeshCount = jSONNode.Count;
		int num3 = 0;
		for (int l = 0; l < jSONNode.Count; l++)
		{
			mesh.SetIndices(array2[l], MeshTopology.Triangles, l, calculateBounds: false, num3);
			num3 += array[l];
		}
		mesh.RecalculateBounds();
		result.mesh = mesh;
		result.morphTargets = array3;
		if (array3 != null)
		{
			result.baseAttributes = baseAttributes;
		}
		return result;
	}

	private static void FlipTriangleIndices(ref int[] indices)
	{
		for (int i = 0; i < indices.Length; i += 3)
		{
			ref int reference = ref indices[i];
			ref int reference2 = ref indices[i + 2];
			int num = indices[i + 2];
			int num2 = indices[i];
			reference = num;
			reference2 = num2;
		}
	}

	private OVRMeshAttributes ReadMeshAttributes(JSONNode jsonAttributes, int totalVertexCount, int vertexOffset)
	{
		OVRMeshAttributes result = default(OVRMeshAttributes);
		JSONNode jSONNode = jsonAttributes["POSITION"];
		if (jSONNode != null)
		{
			_dataAccessor.Seek(jSONNode.AsInt);
			result.vertices = _dataAccessor.ReadVector3(GLTFToUnitySpace);
		}
		jSONNode = jsonAttributes["NORMAL"];
		if (jSONNode != null)
		{
			_dataAccessor.Seek(jSONNode.AsInt);
			result.normals = _dataAccessor.ReadVector3(GLTFToUnitySpace);
		}
		jSONNode = jsonAttributes["TANGENT"];
		if (jSONNode != null)
		{
			_dataAccessor.Seek(jSONNode.AsInt);
			result.tangents = _dataAccessor.ReadVector4(GLTFToUnityTangent);
		}
		jSONNode = jsonAttributes["TEXCOORD_0"];
		if (jSONNode != null)
		{
			_dataAccessor.Seek(jSONNode.AsInt);
			result.texcoords = _dataAccessor.ReadVector2();
		}
		jSONNode = jsonAttributes["COLOR_0"];
		if (jSONNode != null)
		{
			_dataAccessor.Seek(jSONNode.AsInt);
			result.colors = _dataAccessor.ReadColor();
		}
		jSONNode = jsonAttributes["WEIGHTS_0"];
		if (jSONNode != null)
		{
			result.boneWeights = new BoneWeight[totalVertexCount];
			_dataAccessor.Seek(jSONNode.AsInt);
			_dataAccessor.ReadWeights(ref result.boneWeights);
			JSONNode jSONNode2 = jsonAttributes["JOINTS_0"];
			_dataAccessor.Seek(jSONNode2.AsInt);
			_dataAccessor.ReadJoints(ref result.boneWeights);
		}
		return result;
	}

	private void ProcessSkin(JSONNode skinNode, SkinnedMeshRenderer renderer)
	{
		Matrix4x4[] bindposes = null;
		if (skinNode["inverseBindMatrices"] != null)
		{
			int asInt = skinNode["inverseBindMatrices"].AsInt;
			_dataAccessor.Seek(asInt);
			bindposes = _dataAccessor.ReadMatrix4x4(GLTFToUnitySpace);
		}
		if (skinNode["skeleton"] != null)
		{
			int asInt2 = skinNode["skeleton"].AsInt;
			renderer.rootBone = m_Nodes[asInt2].transform;
		}
		Transform[] array = null;
		if (skinNode["joints"] != null)
		{
			JSONArray asArray = skinNode["joints"].AsArray;
			array = new Transform[asArray.Count];
			for (int i = 0; i < asArray.Count; i++)
			{
				array[i] = m_Nodes[(int)asArray[i]].transform;
			}
		}
		renderer.sharedMesh.bindposes = bindposes;
		renderer.bones = array;
	}

	private OVRMaterialData ProcessMaterial(int matId)
	{
		OVRMaterialData result = default(OVRMaterialData);
		JSONNode jSONNode = m_jsonData["materials"][matId];
		JSONNode jSONNode2 = jSONNode["alphaMode"];
		bool flag = jSONNode2 != null && jSONNode2.Value == "BLEND";
		JSONNode jSONNode3 = jSONNode["pbrMetallicRoughness"];
		result.baseColorFactor = Color.white;
		JSONNode jSONNode4 = jSONNode3["baseColorFactor"];
		if (jSONNode4 != null)
		{
			result.baseColorFactor = new Color(jSONNode4[0].AsFloat, jSONNode4[1].AsFloat, jSONNode4[2].AsFloat, jSONNode4[3].AsFloat);
		}
		JSONNode jSONNode5 = jSONNode3["baseColorTexture"];
		if (jSONNode5 != null)
		{
			int asInt = jSONNode5["index"].AsInt;
			result.textureId = asInt;
		}
		else
		{
			JSONNode jSONNode6 = jSONNode["emissiveTexture"];
			if (jSONNode6 != null)
			{
				int asInt2 = jSONNode6["index"].AsInt;
				result.textureId = asInt2;
			}
		}
		result.shader = (flag ? m_AlphaBlendShader : m_Shader);
		return result;
	}

	private OVRTextureData ProcessTexture(int textureId)
	{
		JSONNode jSONNode = m_jsonData["textures"][textureId];
		int aIndex = -1;
		JSONNode jSONNode2 = jSONNode["extensions"];
		if (jSONNode2 != null)
		{
			JSONNode jSONNode3 = jSONNode2["KHR_texture_basisu"];
			if (jSONNode3 != null)
			{
				aIndex = jSONNode3["source"].AsInt;
			}
		}
		else
		{
			aIndex = jSONNode["source"].AsInt;
		}
		JSONNode jSONNode4 = m_jsonData["images"][aIndex];
		OVRTextureData result = default(OVRTextureData);
		string value = jSONNode4["uri"].Value;
		if (!string.IsNullOrEmpty(value))
		{
			result.uri = value;
			return result;
		}
		int asInt = jSONNode4["bufferView"].AsInt;
		string value2 = jSONNode4["mimeType"].Value;
		if (!(value2 == "image/ktx2"))
		{
			if (value2 == "image/png")
			{
				result.data = _dataAccessor.ReadBuffer(asInt);
				result.format = OVRTextureFormat.PNG;
			}
			else
			{
				Debug.LogWarning("Unsupported image mimeType '" + jSONNode4["mimeType"].Value + "'");
			}
		}
		else
		{
			result.data = _dataAccessor.ReadBuffer(asInt);
			result.format = OVRTextureFormat.KTX2;
		}
		return result;
	}

	private void TranscodeTexture(ref OVRTextureData textureData)
	{
		if (string.IsNullOrEmpty(textureData.uri))
		{
			if (textureData.format == OVRTextureFormat.KTX2)
			{
				OVRKtxTexture.Load(textureData.data, ref textureData);
			}
			else if (textureData.format != OVRTextureFormat.PNG)
			{
				Debug.LogWarning("Only KTX2 textures can be trascoded.");
			}
		}
	}

	private Material CreateUnityMaterial(OVRMaterialData matData, bool loadMips)
	{
		Material material = new Material(matData.shader);
		material.color = matData.baseColorFactor;
		if (loadMips && material.HasProperty("_MainTexMMBias"))
		{
			material.SetFloat("_MainTexMMBias", m_TextureMipmapBias);
		}
		Texture2D value = null;
		bool flag = false;
		if (m_textures.TryGetValue(matData.textureId, out value))
		{
			material.mainTexture = value;
			return material;
		}
		if (matData.texture.format == OVRTextureFormat.KTX2)
		{
			value = new Texture2D(matData.texture.width, matData.texture.height, matData.texture.transcodedFormat, loadMips);
			value.LoadRawTextureData(matData.texture.data);
			flag = true;
		}
		else if (matData.texture.format == OVRTextureFormat.PNG)
		{
			value = new Texture2D(2, 2, TextureFormat.RGBA32, loadMips);
			value.LoadImage(matData.texture.data);
			flag = true;
		}
		else if (!string.IsNullOrEmpty(matData.texture.uri))
		{
			value = textureUriHandler?.Invoke(matData.texture.uri, material);
		}
		if (!value)
		{
			return material;
		}
		if (flag)
		{
			ApplyTextureQuality(m_TextureQuality, ref value);
			value.Apply(updateMipmaps: false, makeNoLongerReadable: true);
		}
		m_textures[matData.textureId] = value;
		material.mainTexture = value;
		return material;
	}

	private OVRGLTFInputNode GetInputNodeType(string name)
	{
		foreach (KeyValuePair<string, OVRGLTFInputNode> item in InputNodeNameMap)
		{
			if (name.Contains(item.Key))
			{
				return item.Value;
			}
		}
		return OVRGLTFInputNode.None;
	}

	private IEnumerator ProcessAnimations()
	{
		JSONNode jSONNode = m_jsonData["animations"];
		int animationIndex = 0;
		float processingStart = Time.realtimeSinceStartup;
		JSONNode.Enumerator enumerator = jSONNode.AsArray.GetEnumerator();
		while (enumerator.MoveNext())
		{
			JSONNode jSONNode2 = enumerator.Current;
			Dictionary<int, OVRGLTFAnimatinonNode> dictionary = new Dictionary<int, OVRGLTFAnimatinonNode>();
			JSONNode.Enumerator enumerator2 = jSONNode2["channels"].AsArray.GetEnumerator();
			while (enumerator2.MoveNext())
			{
				JSONNode jSONNode3 = enumerator2.Current;
				int asInt = jSONNode3["target"]["node"].AsInt;
				OVRGLTFInputNode inputNodeType = GetInputNodeType(m_Nodes[asInt].name);
				if (!dictionary.TryGetValue(asInt, out var value))
				{
					m_morphTargetHandlers.TryGetValue(asInt, out var value2);
					OVRGLTFAnimatinonNode oVRGLTFAnimatinonNode2 = (dictionary[asInt] = new OVRGLTFAnimatinonNode(inputNodeType, m_Nodes[asInt], value2));
					value = oVRGLTFAnimatinonNode2;
				}
				if (inputNodeType != 0 && !m_InputAnimationNodes.ContainsKey(inputNodeType))
				{
					m_InputAnimationNodes[inputNodeType] = value;
				}
				value.AddChannel(jSONNode3, jSONNode2["samplers"], _dataAccessor);
			}
			m_AnimationLookup[animationIndex] = dictionary.Values.ToArray();
			animationIndex++;
			if (Time.realtimeSinceStartup - processingStart > 1f / 70f)
			{
				processingStart = Time.realtimeSinceStartup;
				yield return null;
			}
		}
	}
}
