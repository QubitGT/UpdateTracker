using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Meta.XR.Acoustics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

internal class MetaXRAcousticGeometry : MonoBehaviour
{
	internal enum LoadState
	{
		NotLoaded,
		Loading,
		Interrupted,
		Loaded
	}

	private struct MeshMaterial
	{
		internal Mesh mesh;

		internal Transform meshTransform;

		internal IMaterialDataProvider[] meshMaterials;
	}

	private struct TerrainMaterial
	{
		internal Terrain terrain;

		internal IMaterialDataProvider[] terrainMaterials;

		internal Mesh[] treePrototypeMeshes;
	}

	internal interface ITransformVisitor
	{
		object visit(Transform transform, object userData);
	}

	private interface IGatherer : ITransformVisitor
	{
		List<MeshMaterial> Meshes { get; }

		List<TerrainMaterial> Terrains { get; }
	}

	private class MeshGatherer : IGatherer, ITransformVisitor
	{
		private List<MeshMaterial> meshes = new List<MeshMaterial>();

		private List<TerrainMaterial> terrains = new List<TerrainMaterial>();

		internal int ignoredMeshCount;

		internal bool ignoreStatic;

		public List<MeshMaterial> Meshes => meshes;

		public List<TerrainMaterial> Terrains => terrains;

		internal MeshGatherer(bool ignoreStatic)
		{
			this.ignoreStatic = ignoreStatic;
		}

		public object visit(Transform transform, object parentData)
		{
			IMaterialDataProvider[] array = parentData as IMaterialDataProvider[];
			MeshFilter[] components = transform.GetComponents<MeshFilter>();
			Terrain[] components2 = transform.GetComponents<Terrain>();
			IMaterialDataProvider[] array2 = Array.ConvertAll((from x in transform.GetComponents<MetaXRAcousticMaterial>()
				where x.enabled
				select x).ToArray(), (MetaXRAcousticMaterial x) => x);
			IMaterialDataProvider[] array3 = array2;
			if (array3 != null && array3.Length != 0)
			{
				int num = array3.Length;
				if (array != null && array.Length > num)
				{
					num = array.Length;
				}
				IMaterialDataProvider[] array4 = new IMaterialDataProvider[num];
				if (array != null)
				{
					for (int i = array3.Length; i < num; i++)
					{
						array4[i] = array[i];
					}
				}
				array = array4;
				for (int j = 0; j < array3.Length; j++)
				{
					array[j] = array3[j];
				}
			}
			MeshFilter[] array5 = components;
			foreach (MeshFilter meshFilter in array5)
			{
				Mesh sharedMesh = meshFilter.sharedMesh;
				if (!(sharedMesh == null))
				{
					if (ignoreStatic && (!sharedMesh.isReadable || transform.gameObject.isStatic))
					{
						Debug.LogError("Mesh: " + meshFilter.gameObject.name + " not readable. Use \"File Enabled\" for static geometry", transform);
						ignoredMeshCount++;
						continue;
					}
					meshes.Add(new MeshMaterial
					{
						mesh = sharedMesh,
						meshTransform = transform,
						meshMaterials = array
					});
				}
			}
			Terrain[] array6 = components2;
			foreach (Terrain terrain in array6)
			{
				terrains.Add(new TerrainMaterial
				{
					terrain = terrain,
					terrainMaterials = array
				});
			}
			return array;
		}
	}

	private class ColliderGatherer : IGatherer, ITransformVisitor
	{
		private List<MeshMaterial> meshes = new List<MeshMaterial>();

		private List<TerrainMaterial> terrains = new List<TerrainMaterial>();

		public List<MeshMaterial> Meshes => meshes;

		public List<TerrainMaterial> Terrains => terrains;

		public object visit(Transform transform, object parentData)
		{
			IMaterialDataProvider[] array = Array.ConvertAll((from x in transform.GetComponents<MetaXRAcousticMaterial>()
				where x.enabled
				select x).ToArray(), (MetaXRAcousticMaterial x) => x);
			IMaterialDataProvider[] array2 = array;
			MeshCollider[] components = transform.GetComponents<MeshCollider>();
			foreach (MeshCollider meshCollider in components)
			{
				if (meshCollider.sharedMesh == null)
				{
					continue;
				}
				if (array2.Length == 0)
				{
					MetaXRAcousticMaterialProperties metaXRAcousticMaterialProperties = MetaXRAcousticMaterialMapping.Instance.findAcousticMaterial(meshCollider.sharedMaterial);
					if (metaXRAcousticMaterialProperties != null)
					{
						array2 = new IMaterialDataProvider[1] { metaXRAcousticMaterialProperties };
					}
				}
				meshes.Add(new MeshMaterial
				{
					mesh = meshCollider.sharedMesh,
					meshTransform = transform,
					meshMaterials = array2
				});
			}
			BoxCollider[] components2 = transform.GetComponents<BoxCollider>();
			foreach (BoxCollider boxCollider in components2)
			{
				Mesh mesh = new Mesh();
				Vector3[] vertices = new Vector3[8]
				{
					boxCollider.center + Vector3.Scale(boxCollider.size * 0.5f, new Vector3(1f, 1f, 1f)),
					boxCollider.center + Vector3.Scale(boxCollider.size * 0.5f, new Vector3(1f, 1f, -1f)),
					boxCollider.center + Vector3.Scale(boxCollider.size * 0.5f, new Vector3(1f, -1f, 1f)),
					boxCollider.center + Vector3.Scale(boxCollider.size * 0.5f, new Vector3(1f, -1f, -1f)),
					boxCollider.center + Vector3.Scale(boxCollider.size * 0.5f, new Vector3(-1f, 1f, 1f)),
					boxCollider.center + Vector3.Scale(boxCollider.size * 0.5f, new Vector3(-1f, 1f, -1f)),
					boxCollider.center + Vector3.Scale(boxCollider.size * 0.5f, new Vector3(-1f, -1f, 1f)),
					boxCollider.center + Vector3.Scale(boxCollider.size * 0.5f, new Vector3(-1f, -1f, -1f))
				};
				int[] indices = new int[24]
				{
					1, 0, 2, 3, 0, 4, 6, 2, 4, 5,
					7, 6, 5, 1, 3, 7, 1, 5, 4, 0,
					2, 6, 7, 3
				};
				mesh.vertices = vertices;
				mesh.SetIndices(indices, MeshTopology.Quads, 0);
				if (array2.Length == 0)
				{
					MetaXRAcousticMaterialProperties metaXRAcousticMaterialProperties2 = MetaXRAcousticMaterialMapping.Instance.findAcousticMaterial(boxCollider.sharedMaterial);
					if (metaXRAcousticMaterialProperties2 != null)
					{
						array2 = new IMaterialDataProvider[1] { metaXRAcousticMaterialProperties2 };
					}
				}
				meshes.Add(new MeshMaterial
				{
					mesh = mesh,
					meshTransform = transform,
					meshMaterials = array2
				});
			}
			return null;
		}
	}

	internal static bool AUTO_VALIDATE = true;

	internal const string FILE_EXTENSION = "xrageo";

	internal static int EnabledGeometryCount = 0;

	[SerializeField]
	[FormerlySerializedAs("relativeFilePath_")]
	private string relativeFilePath = "";

	[SerializeField]
	internal bool FileEnabled = true;

	[SerializeField]
	internal bool IncludeChildMeshes = true;

	[SerializeField]
	internal MeshFlags Flags = MeshFlags.ENABLE_SIMPLIFICATION;

	[SerializeField]
	private float maxSimplifyError = 0.1f;

	[SerializeField]
	private float minDiffractionEdgeAngle = 1f;

	[SerializeField]
	private float minDiffractionEdgeLength = 0.01f;

	[SerializeField]
	private float flagLength = 1f;

	[SerializeField]
	private int lodSelection;

	[SerializeField]
	private bool useColliders;

	[SerializeField]
	private bool overrideExcludeTagsEnabled;

	[SerializeField]
	private string[] overrideExcludeTags;

	[NonSerialized]
	internal IntPtr geometryHandle = IntPtr.Zero;

	[NonSerialized]
	internal LoadState loadState_;

	[NonSerialized]
	private int vertexCount = -1;

	[SerializeField]
	private Color[] materialColors;

	[SerializeField]
	private Hash128 HierarchyHash;

	internal const int Success = 0;

	private static int terrainDecimation;

	internal string RelativeFilePath => relativeFilePath;

	internal string AbsoluteFilePath
	{
		get
		{
			return Path.GetFullPath(Path.Combine(Application.dataPath, RelativeFilePath));
		}
		set
		{
			string text = value.Replace('\\', '/');
			if (text.StartsWith(Application.dataPath))
			{
				relativeFilePath = text.Substring(Application.dataPath.Length + 1);
			}
			else
			{
				Debug.LogError("invalid path " + value + ", outside application path " + Application.dataPath, base.gameObject);
			}
		}
	}

	internal bool EnableSimplification
	{
		get
		{
			return (Flags & MeshFlags.ENABLE_SIMPLIFICATION) != 0;
		}
		set
		{
			if (value)
			{
				Flags |= MeshFlags.ENABLE_SIMPLIFICATION;
			}
			else
			{
				Flags &= ~MeshFlags.ENABLE_SIMPLIFICATION;
			}
		}
	}

	internal bool EnableDiffraction
	{
		get
		{
			return (Flags & MeshFlags.ENABLE_DIFFRACTION) != 0;
		}
		set
		{
			if (value)
			{
				Flags |= MeshFlags.ENABLE_DIFFRACTION;
			}
			else
			{
				Flags &= ~MeshFlags.ENABLE_DIFFRACTION;
			}
		}
	}

	internal float MaxSimplifyError
	{
		get
		{
			return maxSimplifyError;
		}
		set
		{
			maxSimplifyError = Math.Max(value, 0f);
		}
	}

	internal float MinDiffractionEdgeAngle
	{
		get
		{
			return minDiffractionEdgeAngle;
		}
		set
		{
			minDiffractionEdgeAngle = Math.Clamp(value, 0f, 180f);
		}
	}

	internal float MinDiffractionEdgeLength
	{
		get
		{
			return minDiffractionEdgeLength;
		}
		set
		{
			minDiffractionEdgeLength = Math.Max(value, 0f);
		}
	}

	internal float FlagLength
	{
		get
		{
			return flagLength;
		}
		set
		{
			flagLength = value;
		}
	}

	internal int LodSelection
	{
		get
		{
			return lodSelection;
		}
		set
		{
			lodSelection = value;
		}
	}

	internal bool UseColliders
	{
		get
		{
			return useColliders;
		}
		set
		{
			useColliders = value;
		}
	}

	internal bool OverrideExcludeTagsEnabled
	{
		get
		{
			return overrideExcludeTagsEnabled;
		}
		set
		{
			overrideExcludeTagsEnabled = value;
		}
	}

	internal string[] OverrideExcludeTags
	{
		get
		{
			return overrideExcludeTags;
		}
		set
		{
			overrideExcludeTags = value;
		}
	}

	internal string[] ExcludeTags
	{
		get
		{
			if (!OverrideExcludeTagsEnabled)
			{
				return MetaXRAcousticSettings.Instance.ExcludeTags;
			}
			return OverrideExcludeTags;
		}
	}

	internal bool IsLoaded => loadState_ == LoadState.Loaded;

	internal int VertexCount => vertexCount;

	internal static event Action OnAnyGeometryEnabled;

	private void Awake()
	{
		StartInternal();
	}

	internal bool StartInternal()
	{
		if (!CreatePropagationGeometry())
		{
			return false;
		}
		ApplyTransform();
		return true;
	}

	internal bool CreatePropagationGeometry()
	{
		if (geometryHandle != IntPtr.Zero)
		{
			Debug.LogWarning("Tried to initialize geometry twice, destroying stale copy", base.gameObject);
			DestroyPropagationGeometry();
		}
		if (geometryHandle != IntPtr.Zero)
		{
			Debug.LogError("Unable to clean up stale geometry", base.gameObject);
			return false;
		}
		if (MetaXRAcousticNativeInterface.Interface.CreateAudioGeometry(out geometryHandle) != 0)
		{
			Debug.LogError("Unable to create geometry handle", base.gameObject);
			return false;
		}
		if (FileEnabled)
		{
			if (string.IsNullOrEmpty(relativeFilePath))
			{
				if (Application.isPlaying)
				{
					Debug.LogError("No file set, make sure to Bake Mesh to File", base.gameObject);
				}
				return false;
			}
			if (!ReadFile())
			{
				return false;
			}
		}
		else if (Application.isPlaying)
		{
			if (base.gameObject.isStatic)
			{
				Debug.LogError("Static geometry requires \"File Enabled\"", base.gameObject);
				return false;
			}
			if (!GatherGeometryRuntime())
			{
				return false;
			}
		}
		return true;
	}

	private void IncrementEnabledGeometryCount()
	{
		EnabledGeometryCount++;
		if (EnabledGeometryCount == 1)
		{
			MetaXRAcousticGeometry.OnAnyGeometryEnabled();
		}
	}

	private void DecrementEnabledGeometryCount()
	{
		EnabledGeometryCount--;
	}

	private void OnEnable()
	{
		if (loadState_ == LoadState.Interrupted)
		{
			Debug.Log("Resuming interrupted load!!");
			ReadFile();
		}
		else if (!(geometryHandle == IntPtr.Zero) && (loadState_ != 0 || !FileEnabled))
		{
			Debug.Log("Enabling Geometry: " + relativeFilePath, base.gameObject);
			MetaXRAcousticNativeInterface.Interface.AudioGeometrySetObjectFlag(geometryHandle, ObjectFlags.ENABLED, enabled: true);
			ApplyTransform();
			MetaXRAcousticNativeInterface.Interface.AudioGeometrySetObjectFlag(geometryHandle, ObjectFlags.STATIC, base.gameObject.isStatic);
			if (IsLoaded)
			{
				IncrementEnabledGeometryCount();
			}
		}
	}

	private void OnDisable()
	{
		if (!(geometryHandle == IntPtr.Zero))
		{
			Debug.Log("Disabling Geometry: " + relativeFilePath, base.gameObject);
			if (loadState_ == LoadState.Loading && !base.gameObject.activeInHierarchy)
			{
				Debug.Log("Interrupted load!!");
				loadState_ = LoadState.Interrupted;
			}
			MetaXRAcousticNativeInterface.Interface.AudioGeometrySetObjectFlag(geometryHandle, ObjectFlags.ENABLED, enabled: false);
			ApplyTransform();
			MetaXRAcousticNativeInterface.Interface.AudioGeometrySetObjectFlag(geometryHandle, ObjectFlags.STATIC, base.gameObject.isStatic);
			if (IsLoaded)
			{
				DecrementEnabledGeometryCount();
			}
		}
	}

	private void LateUpdate()
	{
		if (!(geometryHandle == IntPtr.Zero) && base.transform.hasChanged)
		{
			ApplyTransform();
			base.transform.hasChanged = false;
		}
	}

	private void ApplyTransform()
	{
		if (!(geometryHandle == IntPtr.Zero))
		{
			MetaXRAcousticNativeInterface.INativeInterface @interface = MetaXRAcousticNativeInterface.Interface;
			IntPtr geometry = geometryHandle;
			Matrix4x4 matrix = base.transform.localToWorldMatrix;
			@interface.AudioGeometrySetTransform(geometry, in matrix);
		}
	}

	private void OnDestroy()
	{
		DestroyInternal();
	}

	internal bool DestroyInternal()
	{
		if (!DestroyPropagationGeometry())
		{
			return false;
		}
		return true;
	}

	private bool DestroyPropagationGeometry()
	{
		lock (this)
		{
			if (geometryHandle != IntPtr.Zero && MetaXRAcousticNativeInterface.Interface.DestroyAudioGeometry(geometryHandle) != 0)
			{
				Debug.LogError("Unable to destroy geometry", base.gameObject);
				return false;
			}
			geometryHandle = IntPtr.Zero;
			return true;
		}
	}

	private static bool isObjectUsedByLODGroup(GameObject obj, LODGroup lod)
	{
		LOD[] lODs = lod.GetLODs();
		for (int i = 0; i < lODs.Length; i++)
		{
			Renderer[] renderers = lODs[i].renderers;
			for (int j = 0; j < renderers.Length; j++)
			{
				if (renderers[j].gameObject == obj)
				{
					return true;
				}
			}
		}
		return false;
	}

	private static void traverseMeshHierarchy(GameObject obj, bool includeChildren, string[] excludeTags, bool parentWasExcluded, int lodSelection, LODGroup parentLOD, ITransformVisitor visitor, object parentData = null)
	{
		if (!obj.activeInHierarchy)
		{
			return;
		}
		LODGroup lODGroup = obj.GetComponent(typeof(LODGroup)) as LODGroup;
		if (lODGroup != null)
		{
			LOD[] lODs = lODGroup.GetLODs();
			if (lODs.Length != 0)
			{
				if (lODs.Length == 1 && lODs[0].renderers.Length == 1)
				{
					obj = lODs[0].renderers[0].gameObject;
				}
				else
				{
					int num = Mathf.Clamp(lodSelection, 0, lODs.Length - 1);
					Renderer[] renderers = lODs[num].renderers;
					for (int i = 0; i < renderers.Length; i++)
					{
						if (renderers[i] != null && !(renderers[i].gameObject == obj))
						{
							traverseMeshHierarchy(renderers[i].gameObject, includeChildren, excludeTags, parentWasExcluded, lodSelection, null, visitor, parentData);
						}
					}
				}
				parentLOD = lODGroup;
			}
		}
		bool flag = true;
		bool flag2 = parentLOD != lODGroup && parentLOD != null && isObjectUsedByLODGroup(obj, parentLOD);
		if (excludeTags.Contains(obj.tag) || parentWasExcluded || flag2)
		{
			MetaXRAcousticMaterial component = obj.GetComponent<MetaXRAcousticMaterial>();
			flag = ((!(component == null) && component.enabled) ? true : false);
		}
		if (flag)
		{
			parentData = visitor.visit(obj.transform, parentData);
		}
		if (!includeChildren)
		{
			return;
		}
		foreach (Transform item in obj.transform)
		{
			if (item.GetComponent<MetaXRAcousticGeometry>() == null)
			{
				traverseMeshHierarchy(item.gameObject, includeChildren, excludeTags, !flag, lodSelection, parentLOD, visitor, parentData);
			}
		}
	}

	private bool GatherGeometryInternal(IntPtr geometryHandle, GameObject meshObject, Matrix4x4 worldToLocal, bool ignoreStatic, out int ignoredMeshCount)
	{
		ignoredMeshCount = 0;
		IGatherer gatherer = ((!useColliders) ? ((IGatherer)new MeshGatherer(ignoreStatic)) : ((IGatherer)new ColliderGatherer()));
		traverseMeshHierarchy(meshObject, IncludeChildMeshes, ExcludeTags, parentWasExcluded: false, lodSelection, null, gatherer);
		int totalVertexCount = 0;
		uint totalIndexCount = 0u;
		int totalFaceCount = 0;
		int totalMaterialCount = 0;
		foreach (MeshMaterial mesh in gatherer.Meshes)
		{
			updateCountsForMesh(ref totalVertexCount, ref totalIndexCount, ref totalFaceCount, ref totalMaterialCount, mesh.mesh);
		}
		IMaterialDataProvider[] array = new IMaterialDataProvider[1];
		for (int i = 0; i < gatherer.Terrains.Count; i++)
		{
			TerrainMaterial value = gatherer.Terrains[i];
			TerrainData terrainData = value.terrain.terrainData;
			int heightmapResolution = terrainData.heightmapResolution;
			int heightmapResolution2 = terrainData.heightmapResolution;
			int num = (heightmapResolution - 1) / terrainDecimation + 1;
			int num2 = (heightmapResolution2 - 1) / terrainDecimation + 1;
			int num3 = num * num2;
			int num4 = (num - 1) * (num2 - 1) * 6;
			totalMaterialCount++;
			totalVertexCount += num3;
			totalIndexCount += (uint)num4;
			totalFaceCount += num4 / 3;
			TreePrototype[] treePrototypes = terrainData.treePrototypes;
			if (treePrototypes.Length == 0)
			{
				continue;
			}
			if (array[0] == null)
			{
				array[0] = value.terrainMaterials.Last();
			}
			value.treePrototypeMeshes = new Mesh[treePrototypes.Length];
			for (int j = 0; j < treePrototypes.Length; j++)
			{
				MeshFilter[] componentsInChildren = treePrototypes[j].prefab.GetComponentsInChildren<MeshFilter>();
				int num5 = int.MaxValue;
				int num6 = -1;
				for (int k = 0; k < componentsInChildren.Length; k++)
				{
					int num7 = componentsInChildren[k].sharedMesh.vertexCount;
					if (num7 < num5)
					{
						num5 = num7;
						num6 = k;
					}
				}
				value.treePrototypeMeshes[j] = componentsInChildren[num6].sharedMesh;
			}
			TreeInstance[] treeInstances = terrainData.treeInstances;
			for (int l = 0; l < treeInstances.Length; l++)
			{
				TreeInstance treeInstance = treeInstances[l];
				updateCountsForMesh(ref totalVertexCount, ref totalIndexCount, ref totalFaceCount, ref totalMaterialCount, value.treePrototypeMeshes[treeInstance.prototypeIndex]);
			}
			gatherer.Terrains[i] = value;
		}
		List<Vector3> tempVertices = new List<Vector3>();
		List<int> tempIndices = new List<int>();
		MeshGroup[] array2 = new MeshGroup[totalMaterialCount];
		float[] array3 = new float[totalVertexCount * 3];
		int[] array4 = new int[totalIndexCount];
		int vertexOffset = 0;
		int indexOffset = 0;
		int groupOffset = 0;
		foreach (MeshMaterial mesh2 in gatherer.Meshes)
		{
			Matrix4x4 matrix = worldToLocal * mesh2.meshTransform.localToWorldMatrix;
			if (!uploadMeshFilter(tempVertices, tempIndices, array2, array3, array4, ref vertexOffset, ref indexOffset, ref groupOffset, mesh2.mesh, mesh2.meshMaterials, matrix))
			{
				return false;
			}
		}
		foreach (TerrainMaterial terrain in gatherer.Terrains)
		{
			TerrainData terrainData2 = terrain.terrain.terrainData;
			Matrix4x4 matrix4x = worldToLocal * terrain.terrain.gameObject.transform.localToWorldMatrix;
			int heightmapResolution3 = terrainData2.heightmapResolution;
			int heightmapResolution4 = terrainData2.heightmapResolution;
			float[,] heights = terrainData2.GetHeights(0, 0, heightmapResolution3, heightmapResolution4);
			Vector3 size = terrainData2.size;
			size = new Vector3(size.x / (float)(heightmapResolution3 - 1) * (float)terrainDecimation, size.y, size.z / (float)(heightmapResolution4 - 1) * (float)terrainDecimation);
			int num8 = (heightmapResolution3 - 1) / terrainDecimation + 1;
			int num9 = (heightmapResolution4 - 1) / terrainDecimation + 1;
			int num10 = num8 * num9;
			int num11 = (num8 - 1) * (num9 - 1) * 2;
			array2[groupOffset].faceType = FaceType.TRIANGLES;
			array2[groupOffset].faceCount = (UIntPtr)(ulong)num11;
			array2[groupOffset].indexOffset = (UIntPtr)(ulong)indexOffset;
			if (terrain.terrainMaterials != null && terrain.terrainMaterials.Length != 0)
			{
				array2[groupOffset].material = MetaXRAcousticMaterial.CreateMaterialNativeHandle(terrain.terrainMaterials[0].Data);
			}
			else
			{
				array2[groupOffset].material = IntPtr.Zero;
			}
			for (int m = 0; m < num9; m++)
			{
				for (int n = 0; n < num8; n++)
				{
					int num12 = (vertexOffset + m * num8 + n) * 3;
					Vector3 vector = matrix4x.MultiplyPoint3x4(Vector3.Scale(size, new Vector3(m, heights[n * terrainDecimation, m * terrainDecimation], n)));
					array3[num12] = vector.x;
					array3[num12 + 1] = vector.y;
					array3[num12 + 2] = vector.z;
				}
			}
			for (int num13 = 0; num13 < num9 - 1; num13++)
			{
				for (int num14 = 0; num14 < num8 - 1; num14++)
				{
					array4[indexOffset] = vertexOffset + num13 * num8 + num14;
					array4[indexOffset + 1] = vertexOffset + (num13 + 1) * num8 + num14;
					array4[indexOffset + 2] = vertexOffset + num13 * num8 + num14 + 1;
					array4[indexOffset + 3] = vertexOffset + (num13 + 1) * num8 + num14;
					array4[indexOffset + 4] = vertexOffset + (num13 + 1) * num8 + num14 + 1;
					array4[indexOffset + 5] = vertexOffset + num13 * num8 + num14 + 1;
					indexOffset += 6;
				}
			}
			vertexOffset += num10;
			groupOffset++;
			TreeInstance[] treeInstances = terrainData2.treeInstances;
			for (int l = 0; l < treeInstances.Length; l++)
			{
				TreeInstance treeInstance2 = treeInstances[l];
				Vector3 vector2 = Vector3.Scale(treeInstance2.position, terrainData2.size);
				Matrix4x4 localToWorldMatrix = terrain.terrain.gameObject.transform.localToWorldMatrix;
				localToWorldMatrix.SetColumn(3, localToWorldMatrix.GetColumn(3) + new Vector4(vector2.x, vector2.y, vector2.z, 0f));
				Matrix4x4 matrix2 = worldToLocal * localToWorldMatrix;
				if (!uploadMeshFilter(tempVertices, tempIndices, array2, array3, array4, ref vertexOffset, ref indexOffset, ref groupOffset, terrain.treePrototypeMeshes[treeInstance2.prototypeIndex], array, matrix2))
				{
					return false;
				}
			}
		}
		if (totalVertexCount == 0)
		{
			_ = base.gameObject.scene;
			string text = base.gameObject.scene.name + ":" + string.Join("/", (from t in base.gameObject.GetComponentsInParent<Transform>()
				select t.name).Reverse().ToArray());
			Debug.LogError("Geometry unable to upload mesh, vertex count is zero " + text, base.gameObject);
			return false;
		}
		Debug.Log($"Uploading mesh {base.name} with {totalVertexCount} vertices");
		MeshSimplification simplification = default(MeshSimplification);
		simplification.thisSize = (UIntPtr)(ulong)Marshal.SizeOf(typeof(MeshSimplification));
		simplification.flags = Flags;
		simplification.unitScale = 1f;
		simplification.maxError = MaxSimplifyError;
		simplification.minDiffractionEdgeAngle = MinDiffractionEdgeAngle;
		simplification.minDiffractionEdgeLength = MinDiffractionEdgeLength;
		simplification.flagLength = FlagLength;
		simplification.threadCount = (UIntPtr)1uL;
		int num15 = MetaXRAcousticNativeInterface.Interface.AudioGeometryUploadSimplifiedMeshArrays(geometryHandle, array3, totalVertexCount, array4, array4.Length, array2, array2.Length, ref simplification);
		MetaXRAcousticNativeInterface.Interface.AudioGeometrySetObjectFlag(geometryHandle, ObjectFlags.ENABLED, base.isActiveAndEnabled);
		MetaXRAcousticNativeInterface.Interface.AudioGeometrySetObjectFlag(geometryHandle, ObjectFlags.STATIC, base.gameObject.isStatic);
		MeshGroup[] array5 = array2;
		for (int l = 0; l < array5.Length; l++)
		{
			MeshGroup meshGroup = array5[l];
			MetaXRAcousticMaterial.DestroyMaterialNativeHandle(meshGroup.material);
		}
		if (num15 == 0)
		{
			List<IMaterialDataProvider> list = new List<IMaterialDataProvider>();
			foreach (MeshMaterial mesh3 in gatherer.Meshes)
			{
				int num16 = 0;
				int subMeshCount = mesh3.mesh.subMeshCount;
				int num17 = ((mesh3.meshMaterials != null) ? mesh3.meshMaterials.Length : 0);
				if (num17 != 0)
				{
					int num18 = Mathf.Min(num17, subMeshCount);
					for (num16 = 0; num16 < num18; num16++)
					{
						list.Add(mesh3.meshMaterials[num16]);
					}
					for (num16 = num18; num16 < subMeshCount; num16++)
					{
						list.Add(mesh3.meshMaterials[num17 - 1]);
					}
				}
				else
				{
					for (int num19 = 0; num19 < subMeshCount; num19++)
					{
						list.Add(null);
					}
				}
			}
			foreach (TerrainMaterial terrain2 in gatherer.Terrains)
			{
				if (terrain2.terrainMaterials != null && terrain2.terrainMaterials.Length != 0)
				{
					list.AddRange(terrain2.terrainMaterials);
				}
			}
			return true;
		}
		return false;
	}

	private static bool uploadMeshFilter(List<Vector3> tempVertices, List<int> tempIndices, MeshGroup[] groups, float[] vertices, int[] indices, ref int vertexOffset, ref int indexOffset, ref int groupOffset, Mesh mesh, IMaterialDataProvider[] materials, Matrix4x4 matrix)
	{
		tempVertices.Clear();
		mesh.GetVertices(tempVertices);
		int count = tempVertices.Count;
		for (int i = 0; i < count; i++)
		{
			Vector3 vector = matrix.MultiplyPoint3x4(tempVertices[i]);
			int num = (vertexOffset + i) * 3;
			vertices[num] = vector.x;
			vertices[num + 1] = vector.y;
			vertices[num + 2] = vector.z;
		}
		for (int j = 0; j < mesh.subMeshCount; j++)
		{
			MeshTopology topology = mesh.GetTopology(j);
			if (topology != 0 && topology != MeshTopology.Quads)
			{
				continue;
			}
			tempIndices.Clear();
			mesh.GetIndices(tempIndices, j);
			int count2 = tempIndices.Count;
			for (int k = 0; k < count2; k++)
			{
				indices[indexOffset + k] = tempIndices[k] + vertexOffset;
			}
			switch (topology)
			{
			case MeshTopology.Triangles:
				groups[groupOffset + j].faceType = FaceType.TRIANGLES;
				groups[groupOffset + j].faceCount = (UIntPtr)(ulong)(count2 / 3);
				break;
			case MeshTopology.Quads:
				groups[groupOffset + j].faceType = FaceType.QUADS;
				groups[groupOffset + j].faceCount = (UIntPtr)(ulong)(count2 / 4);
				break;
			}
			groups[groupOffset + j].indexOffset = (UIntPtr)(ulong)indexOffset;
			if (materials != null && materials.Length != 0)
			{
				int num2 = j;
				if (num2 >= materials.Length)
				{
					num2 = materials.Length - 1;
				}
				groups[groupOffset + j].material = MetaXRAcousticMaterial.CreateMaterialNativeHandle(materials[num2].Data);
			}
			else
			{
				groups[groupOffset + j].material = IntPtr.Zero;
			}
			indexOffset += count2;
		}
		vertexOffset += count;
		groupOffset += mesh.subMeshCount;
		return true;
	}

	private static void updateCountsForMesh(ref int totalVertexCount, ref uint totalIndexCount, ref int totalFaceCount, ref int totalMaterialCount, Mesh mesh)
	{
		totalMaterialCount += mesh.subMeshCount;
		totalVertexCount += mesh.vertexCount;
		for (int i = 0; i < mesh.subMeshCount; i++)
		{
			MeshTopology topology = mesh.GetTopology(i);
			if (topology == MeshTopology.Triangles || topology == MeshTopology.Quads)
			{
				uint indexCount = mesh.GetIndexCount(i);
				totalIndexCount += indexCount;
				switch (topology)
				{
				case MeshTopology.Triangles:
					totalFaceCount += (int)indexCount / 3;
					break;
				case MeshTopology.Quads:
					totalFaceCount += (int)indexCount / 4;
					break;
				}
			}
		}
	}

	internal bool GatherGeometryRuntime()
	{
		Debug.Log("Gathering geometry");
		if (!GatherGeometryInternal(geometryHandle, base.gameObject, base.gameObject.transform.worldToLocalMatrix, Application.isPlaying, out var ignoredMeshCount))
		{
			return false;
		}
		if (ignoredMeshCount != 0)
		{
			Debug.LogWarning($"Failed to upload meshes, {ignoredMeshCount} static meshes ignored. Turn on \"File Enabled\" to process static meshes offline", base.gameObject);
		}
		return true;
	}

	internal bool ReadFile()
	{
		if (string.IsNullOrEmpty(AbsoluteFilePath))
		{
			Debug.LogError("Invalid mesh file path", base.gameObject);
			return false;
		}
		int num = AbsoluteFilePath.IndexOf("StreamingAssets");
		if (Application.isPlaying && num > 0)
		{
			string relativePath = AbsoluteFilePath.Substring(num + 16);
			StartCoroutine(LoadGeometryAsync(relativePath));
		}
		else
		{
			if (MetaXRAcousticNativeInterface.Interface.AudioGeometryReadMeshFile(geometryHandle, AbsoluteFilePath) != 0)
			{
				Debug.LogError("Error reading mesh file " + AbsoluteFilePath, base.gameObject);
				return false;
			}
			MetaXRAcousticNativeInterface.Interface.AudioGeometrySetObjectFlag(geometryHandle, ObjectFlags.ENABLED, base.isActiveAndEnabled);
			ApplyTransform();
			MetaXRAcousticNativeInterface.Interface.AudioGeometrySetObjectFlag(geometryHandle, ObjectFlags.STATIC, base.gameObject.isStatic);
		}
		return true;
	}

	private IEnumerator LoadGeometryAsync(string relativePath)
	{
		string text = Application.streamingAssetsPath + "/" + relativePath;
		Debug.Log("Loading Geometry " + base.name + " from StreamingAssets " + text, base.gameObject);
		float startTime = Time.realtimeSinceStartup;
		UnityWebRequest unityWebRequest = UnityWebRequest.Get(text);
		loadState_ = LoadState.Loading;
		yield return unityWebRequest.SendWebRequest();
		if (!string.IsNullOrEmpty(unityWebRequest.error))
		{
			Debug.LogError($"web request: done={unityWebRequest.isDone}: {unityWebRequest.error}", base.gameObject);
		}
		float num = Time.realtimeSinceStartup - startTime;
		Debug.Log($"Geometry {base.name}, read time = {num}", base.gameObject);
		LoadGeometryFromMemory(unityWebRequest.downloadHandler.nativeData);
	}

	private unsafe async void LoadGeometryFromMemory(NativeArray<byte>.ReadOnly data)
	{
		if (data.Length == 0)
		{
			return;
		}
		float startTime = Time.realtimeSinceStartup;
		int result = -1;
		await Task.Run(delegate
		{
			IntPtr data2 = (IntPtr)data.GetUnsafeReadOnlyPtr();
			lock (this)
			{
				if (geometryHandle != IntPtr.Zero)
				{
					result = MetaXRAcousticNativeInterface.Interface.AudioGeometryReadMeshMemory(geometryHandle, data2, (ulong)data.Length);
					GC.KeepAlive(data);
				}
			}
		});
		if (result == 0)
		{
			float num = Time.realtimeSinceStartup - startTime;
			Debug.Log($"Sucessfully loaded Geometry {base.name}, load time = {num}", base.gameObject);
			MetaXRAcousticNativeInterface.Interface.AudioGeometrySetObjectFlag(geometryHandle, ObjectFlags.ENABLED, base.isActiveAndEnabled);
			ApplyTransform();
			MetaXRAcousticNativeInterface.Interface.AudioGeometrySetObjectFlag(geometryHandle, ObjectFlags.STATIC, base.gameObject.isStatic);
			loadState_ = LoadState.Loaded;
			if (base.isActiveAndEnabled)
			{
				IncrementEnabledGeometryCount();
			}
		}
		else
		{
			Debug.Log("Unable to read the geometry " + base.name, base.gameObject);
		}
	}

	static MetaXRAcousticGeometry()
	{
		MetaXRAcousticGeometry.OnAnyGeometryEnabled = delegate
		{
		};
		terrainDecimation = 4;
	}
}
