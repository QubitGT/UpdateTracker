using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace DigitalOpus.MB.Core;

[Serializable]
public abstract class MB3_MeshCombiner : MB_IMeshBakerSettings, IDisposable
{
	public enum MeshCombiningStatus
	{
		preAddDeleteOrUpdate,
		readyForApply
	}

	public delegate void GenerateUV2Delegate(Mesh m, float hardAngle, float packMargin);

	public class MBBlendShapeKey
	{
		public GameObject gameObject;

		public int blendShapeIndexInSrc;

		public MBBlendShapeKey(GameObject srcSkinnedMeshRenderGameObject, int blendShapeIndexInSource)
		{
			gameObject = srcSkinnedMeshRenderGameObject;
			blendShapeIndexInSrc = blendShapeIndexInSource;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is MBBlendShapeKey) || obj == null)
			{
				return false;
			}
			MBBlendShapeKey mBBlendShapeKey = (MBBlendShapeKey)obj;
			if (gameObject == mBBlendShapeKey.gameObject)
			{
				return blendShapeIndexInSrc == mBBlendShapeKey.blendShapeIndexInSrc;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (23 * 31 + gameObject.GetInstanceID()) * 31 + blendShapeIndexInSrc;
		}
	}

	public class MBBlendShapeValue
	{
		public GameObject combinedMeshGameObject;

		public int blendShapeIndex;
	}

	[SerializeField]
	protected MeshCombiningStatus _bakeStatus;

	[SerializeField]
	protected MB2_ValidationLevel _validationLevel = MB2_ValidationLevel.robust;

	[SerializeField]
	protected string _name;

	[SerializeField]
	protected MB2_TextureBakeResults _textureBakeResults;

	[SerializeField]
	protected GameObject _resultSceneObject;

	[SerializeField]
	protected Renderer _targetRenderer;

	[SerializeField]
	protected MB2_LogLevel _LOG_LEVEL = MB2_LogLevel.info;

	[SerializeField]
	protected UnityEngine.Object _settingsHolder;

	[SerializeField]
	protected MB2_OutputOptions _outputOption;

	[SerializeField]
	protected MB_RenderType _renderType;

	[SerializeField]
	protected MB2_LightmapOptions _lightmapOption = MB2_LightmapOptions.ignore_UV2;

	[SerializeField]
	protected bool _doNorm = true;

	[SerializeField]
	protected bool _doTan = true;

	[SerializeField]
	protected bool _doCol;

	[SerializeField]
	protected bool _doUV = true;

	[SerializeField]
	protected bool _doUV3;

	[SerializeField]
	protected bool _doUV4;

	[SerializeField]
	protected bool _doUV5;

	[SerializeField]
	protected bool _doUV6;

	[SerializeField]
	protected bool _doUV7;

	[SerializeField]
	protected bool _doUV8;

	[SerializeField]
	protected bool _doBlendShapes;

	[FormerlySerializedAs("_recenterVertsToBoundsCenter")]
	[SerializeField]
	protected MB_MeshPivotLocation _pivotLocationType;

	[SerializeField]
	protected Vector3 _pivotLocation;

	[SerializeField]
	protected bool _clearBuffersAfterBake;

	[SerializeField]
	public bool _optimizeAfterBake = true;

	[SerializeField]
	[FormerlySerializedAs("uv2UnwrappingParamsHardAngle")]
	protected float _uv2UnwrappingParamsHardAngle = 60f;

	[SerializeField]
	[FormerlySerializedAs("uv2UnwrappingParamsPackMargin")]
	protected float _uv2UnwrappingParamsPackMargin = 0.005f;

	[SerializeField]
	protected bool _smrNoExtraBonesWhenCombiningMeshRenderers;

	[SerializeField]
	protected bool _smrMergeBlendShapesWithSameNames;

	[SerializeField]
	protected UnityEngine.Object _assignToMeshCustomizer;

	[SerializeField]
	protected MB_MeshCombineAPIType _meshAPItoUse = MB_MeshCombineAPIType.betaNativeArrayAPI;

	protected bool _usingTemporaryTextureBakeResult;

	private bool _disposed;

	public static bool EVAL_VERSION => false;

	public virtual MeshCombiningStatus bakeStatus => _bakeStatus;

	public virtual MB2_ValidationLevel validationLevel
	{
		get
		{
			return _validationLevel;
		}
		set
		{
			_validationLevel = value;
		}
	}

	public string name
	{
		get
		{
			return _name;
		}
		set
		{
			_name = value;
		}
	}

	public virtual MB2_TextureBakeResults textureBakeResults
	{
		get
		{
			return _textureBakeResults;
		}
		set
		{
			_textureBakeResults = value;
		}
	}

	public virtual GameObject resultSceneObject
	{
		get
		{
			return _resultSceneObject;
		}
		set
		{
			_resultSceneObject = value;
		}
	}

	public virtual Renderer targetRenderer
	{
		get
		{
			return _targetRenderer;
		}
		set
		{
			if (_targetRenderer != null && _targetRenderer != value)
			{
				Debug.LogWarning("Previous targetRenderer was not null. Combined mesh may be shared by more than one Renderer");
			}
			_targetRenderer = value;
			if (value != null && MB_Utility.IsSceneInstance(value.gameObject) && value.transform.parent != null)
			{
				_resultSceneObject = value.transform.parent.gameObject;
			}
		}
	}

	public virtual MB2_LogLevel LOG_LEVEL
	{
		get
		{
			return _LOG_LEVEL;
		}
		set
		{
			_LOG_LEVEL = value;
		}
	}

	public MB_IMeshBakerSettings settings
	{
		get
		{
			if (_settingsHolder != null)
			{
				return settingsHolder.GetMeshBakerSettings();
			}
			return this;
		}
	}

	public virtual MB_IMeshBakerSettingsHolder settingsHolder
	{
		get
		{
			if (_settingsHolder != null)
			{
				if (_settingsHolder is MB_IMeshBakerSettingsHolder)
				{
					return (MB_IMeshBakerSettingsHolder)_settingsHolder;
				}
				_settingsHolder = null;
			}
			return null;
		}
		set
		{
			if (value is UnityEngine.Object)
			{
				_settingsHolder = (UnityEngine.Object)value;
			}
			else
			{
				Debug.LogError("The settings holder must be a UnityEngine.Object");
			}
		}
	}

	public virtual MB2_OutputOptions outputOption
	{
		get
		{
			return _outputOption;
		}
		set
		{
			_outputOption = value;
		}
	}

	public virtual MB_RenderType renderType
	{
		get
		{
			return _renderType;
		}
		set
		{
			_renderType = value;
		}
	}

	public virtual MB2_LightmapOptions lightmapOption
	{
		get
		{
			return _lightmapOption;
		}
		set
		{
			_lightmapOption = value;
		}
	}

	public virtual bool doNorm
	{
		get
		{
			return _doNorm;
		}
		set
		{
			_doNorm = value;
		}
	}

	public virtual bool doTan
	{
		get
		{
			return _doTan;
		}
		set
		{
			_doTan = value;
		}
	}

	public virtual bool doCol
	{
		get
		{
			return _doCol;
		}
		set
		{
			_doCol = value;
		}
	}

	public virtual bool doUV
	{
		get
		{
			return _doUV;
		}
		set
		{
			_doUV = value;
		}
	}

	public virtual bool doUV1
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	public virtual bool doUV3
	{
		get
		{
			return _doUV3;
		}
		set
		{
			_doUV3 = value;
		}
	}

	public virtual bool doUV4
	{
		get
		{
			return _doUV4;
		}
		set
		{
			_doUV4 = value;
		}
	}

	public virtual bool doUV5
	{
		get
		{
			return _doUV5;
		}
		set
		{
			_doUV5 = value;
		}
	}

	public virtual bool doUV6
	{
		get
		{
			return _doUV6;
		}
		set
		{
			_doUV6 = value;
		}
	}

	public virtual bool doUV7
	{
		get
		{
			return _doUV7;
		}
		set
		{
			_doUV7 = value;
		}
	}

	public virtual bool doUV8
	{
		get
		{
			return _doUV8;
		}
		set
		{
			_doUV8 = value;
		}
	}

	public virtual bool doBlendShapes
	{
		get
		{
			return _doBlendShapes;
		}
		set
		{
			_doBlendShapes = value;
		}
	}

	public virtual MB_MeshPivotLocation pivotLocationType
	{
		get
		{
			return _pivotLocationType;
		}
		set
		{
			_pivotLocationType = value;
		}
	}

	public virtual Vector3 pivotLocation
	{
		get
		{
			return _pivotLocation;
		}
		set
		{
			_pivotLocation = value;
		}
	}

	public virtual bool clearBuffersAfterBake
	{
		get
		{
			return _clearBuffersAfterBake;
		}
		set
		{
			_clearBuffersAfterBake = value;
		}
	}

	public bool optimizeAfterBake
	{
		get
		{
			return _optimizeAfterBake;
		}
		set
		{
			_optimizeAfterBake = value;
		}
	}

	public float uv2UnwrappingParamsHardAngle
	{
		get
		{
			return _uv2UnwrappingParamsHardAngle;
		}
		set
		{
			_uv2UnwrappingParamsHardAngle = value;
		}
	}

	public float uv2UnwrappingParamsPackMargin
	{
		get
		{
			return _uv2UnwrappingParamsPackMargin;
		}
		set
		{
			_uv2UnwrappingParamsPackMargin = value;
		}
	}

	public bool smrNoExtraBonesWhenCombiningMeshRenderers
	{
		get
		{
			return _smrNoExtraBonesWhenCombiningMeshRenderers;
		}
		set
		{
			_smrNoExtraBonesWhenCombiningMeshRenderers = value;
		}
	}

	public bool smrMergeBlendShapesWithSameNames
	{
		get
		{
			return _smrMergeBlendShapesWithSameNames;
		}
		set
		{
			_smrMergeBlendShapesWithSameNames = value;
		}
	}

	public IAssignToMeshCustomizer assignToMeshCustomizer
	{
		get
		{
			if (_assignToMeshCustomizer is IAssignToMeshCustomizer)
			{
				return (IAssignToMeshCustomizer)_assignToMeshCustomizer;
			}
			_assignToMeshCustomizer = null;
			return null;
		}
		set
		{
			_assignToMeshCustomizer = (UnityEngine.Object)value;
		}
	}

	public MB_MeshCombineAPIType meshAPI
	{
		get
		{
			return _meshAPItoUse;
		}
		set
		{
			_meshAPItoUse = value;
		}
	}

	public virtual bool doUV2()
	{
		if (settings.lightmapOption != MB2_LightmapOptions.copy_UV2_unchanged && settings.lightmapOption != 0)
		{
			return settings.lightmapOption == MB2_LightmapOptions.copy_UV2_unchanged_to_separate_rects;
		}
		return true;
	}

	public virtual void DisposeRuntimeCreated()
	{
		Dispose();
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	public bool IsDisposed()
	{
		return _disposed;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			_DisposeRuntimeCreated();
			_disposed = true;
		}
	}

	public abstract int GetLightmapIndex();

	public abstract void ClearBuffers();

	public abstract void ClearMesh();

	public abstract void ClearMesh(MB2_EditorMethodsInterface editorMethods);

	internal abstract void _DisposeRuntimeCreated();

	public abstract void DestroyMesh();

	public abstract void DestroyMeshEditor(MB2_EditorMethodsInterface editorMethods);

	public abstract List<GameObject> GetObjectsInCombined();

	public abstract int GetNumObjectsInCombined();

	public virtual bool Apply()
	{
		return Apply(null);
	}

	public abstract bool Apply(GenerateUV2Delegate uv2GenerationMethod);

	public abstract bool Apply(bool triangles, bool vertices, bool normals, bool tangents, bool uvs, bool uv2, bool uv3, bool uv4, bool uv5, bool uv6, bool uv7, bool uv8, bool colors, bool bones = false, bool blendShapeFlag = false, GenerateUV2Delegate uv2GenerationMethod = null);

	public abstract bool Apply(bool triangles, bool vertices, bool normals, bool tangents, bool uvs, bool uv2, bool uv3, bool uv4, bool colors, bool bones = false, bool blendShapeFlag = false, GenerateUV2Delegate uv2GenerationMethod = null);

	public virtual bool UpdateGameObjects(GameObject[] gos)
	{
		return UpdateGameObjects(gos, recalcBounds: true, updateVertices: true, updateNormals: true, updateTangents: true, updateUV: true, updateUV2: false, updateUV3: false, updateUV4: false, updateUV5: false, updateUV6: false, updateUV7: false, updateUV8: false, updateColors: false, updateSkinningInfo: false);
	}

	public virtual bool UpdateGameObjects(GameObject[] gos, bool updateBounds)
	{
		return UpdateGameObjects(gos, updateBounds, updateVertices: true, updateNormals: true, updateTangents: true, updateUV: true, updateUV2: false, updateUV3: false, updateUV4: false, updateUV5: false, updateUV6: false, updateUV7: false, updateUV8: false, updateColors: false, updateSkinningInfo: false);
	}

	public abstract bool UpdateGameObjects(GameObject[] gos, bool recalcBounds, bool updateVertices, bool updateNormals, bool updateTangents, bool updateUV, bool updateUV2, bool updateUV3, bool updateUV4, bool updateColors, bool updateSkinningInfo);

	public abstract bool UpdateGameObjects(GameObject[] gos, bool recalcBounds, bool updateVertices, bool updateNormals, bool updateTangents, bool updateUV, bool updateUV2, bool updateUV3, bool updateUV4, bool updateUV5, bool updateUV6, bool updateUV7, bool updateUV8, bool updateColors, bool updateSkinningInfo);

	public abstract bool AddDeleteGameObjects(GameObject[] gos, GameObject[] deleteGOs, bool disableRendererInSource = true);

	public abstract bool AddDeleteGameObjectsByID(GameObject[] gos, int[] deleteGOinstanceIDs, bool disableRendererInSource);

	public abstract bool CombinedMeshContains(GameObject go);

	public abstract void UpdateSkinnedMeshApproximateBounds();

	public abstract void UpdateSkinnedMeshApproximateBoundsFromBones();

	public abstract void CheckIntegrity();

	public abstract void UpdateSkinnedMeshApproximateBoundsFromBounds();

	public static void UpdateSkinnedMeshApproximateBoundsFromBonesStatic(Transform[] bs, SkinnedMeshRenderer smr)
	{
		Vector3 position = bs[0].position;
		Vector3 position2 = bs[0].position;
		for (int i = 1; i < bs.Length; i++)
		{
			Vector3 position3 = bs[i].position;
			if (position3.x < position2.x)
			{
				position2.x = position3.x;
			}
			if (position3.y < position2.y)
			{
				position2.y = position3.y;
			}
			if (position3.z < position2.z)
			{
				position2.z = position3.z;
			}
			if (position3.x > position.x)
			{
				position.x = position3.x;
			}
			if (position3.y > position.y)
			{
				position.y = position3.y;
			}
			if (position3.z > position.z)
			{
				position.z = position3.z;
			}
		}
		Vector3 vector = (position + position2) / 2f;
		Vector3 vector2 = position - position2;
		Matrix4x4 worldToLocalMatrix = smr.worldToLocalMatrix;
		Bounds localBounds = new Bounds(worldToLocalMatrix * vector, worldToLocalMatrix * vector2);
		smr.localBounds = localBounds;
	}

	public static void UpdateSkinnedMeshApproximateBoundsFromBoundsStatic(List<GameObject> objectsInCombined, SkinnedMeshRenderer smr)
	{
		Bounds b = default(Bounds);
		Bounds bounds = default(Bounds);
		if (MB_Utility.GetBounds(objectsInCombined[0], out b))
		{
			bounds = b;
			for (int i = 1; i < objectsInCombined.Count; i++)
			{
				if (MB_Utility.GetBounds(objectsInCombined[i], out b))
				{
					bounds.Encapsulate(b);
					continue;
				}
				Debug.LogError("Could not get bounds. Not updating skinned mesh bounds");
				return;
			}
			smr.localBounds = bounds;
		}
		else
		{
			Debug.LogError("Could not get bounds. Not updating skinned mesh bounds");
		}
	}

	protected virtual bool _CreateTemporaryTextrueBakeResult(GameObject[] gos, List<Material> matsOnTargetRenderer)
	{
		if (GetNumObjectsInCombined() > 0)
		{
			Debug.LogError("Can't add objects if there are already objects in combined mesh when 'Texture Bake Result' is not set. Perhaps enable 'Clear Buffers After Bake'");
			return false;
		}
		_usingTemporaryTextureBakeResult = true;
		_textureBakeResults = MB2_TextureBakeResults.CreateForMaterialsOnRenderer(gos, matsOnTargetRenderer);
		return true;
	}

	public abstract List<Material> GetMaterialsOnTargetRenderer();
}
