using System;
using System.Collections.Generic;
using UnityEngine;

namespace DigitalOpus.MB.Core;

[Serializable]
public class MB3_MultiMeshCombiner : MB3_MeshCombiner
{
	[Serializable]
	public class CombinedMesh
	{
		public MB3_MeshCombinerSingle combinedMesh;

		public int extraSpace = -1;

		public int numVertsInListToDelete;

		public int numVertsInListToAdd;

		public List<GameObject> gosToAdd;

		public List<int> gosToDelete;

		public List<GameObject> gosToUpdate;

		public bool isDirty;

		public CombinedMesh(int maxNumVertsInMesh, GameObject resultSceneObject, MB2_LogLevel ll)
		{
			combinedMesh = new MB3_MeshCombinerSingle();
			combinedMesh.resultSceneObject = resultSceneObject;
			combinedMesh.LOG_LEVEL = ll;
			extraSpace = maxNumVertsInMesh;
			numVertsInListToDelete = 0;
			numVertsInListToAdd = 0;
			gosToAdd = new List<GameObject>();
			gosToDelete = new List<int>();
			gosToUpdate = new List<GameObject>();
		}

		public bool isEmpty()
		{
			List<GameObject> list = new List<GameObject>();
			list.AddRange(combinedMesh.GetObjectsInCombined());
			for (int i = 0; i < gosToDelete.Count; i++)
			{
				for (int j = 0; j < list.Count; j++)
				{
					if (list[j].GetInstanceID() == gosToDelete[i])
					{
						list.RemoveAt(j);
						break;
					}
				}
			}
			if (list.Count == 0)
			{
				return true;
			}
			return false;
		}
	}

	private static GameObject[] empty = new GameObject[0];

	private static int[] emptyIDs = new int[0];

	public Dictionary<int, CombinedMesh> obj2MeshCombinerMap = new Dictionary<int, CombinedMesh>();

	[SerializeField]
	public List<CombinedMesh> meshCombiners = new List<CombinedMesh>();

	[SerializeField]
	private int _maxVertsInMesh = 65535;

	public override MB2_LogLevel LOG_LEVEL
	{
		get
		{
			return _LOG_LEVEL;
		}
		set
		{
			_LOG_LEVEL = value;
			for (int i = 0; i < meshCombiners.Count; i++)
			{
				meshCombiners[i].combinedMesh.LOG_LEVEL = value;
			}
		}
	}

	public override MB2_ValidationLevel validationLevel
	{
		get
		{
			return _validationLevel;
		}
		set
		{
			_validationLevel = value;
			for (int i = 0; i < meshCombiners.Count; i++)
			{
				meshCombiners[i].combinedMesh.validationLevel = _validationLevel;
			}
		}
	}

	public int maxVertsInMesh
	{
		get
		{
			return _maxVertsInMesh;
		}
		set
		{
			if (obj2MeshCombinerMap.Count <= 0)
			{
				if (value < 3)
				{
					Debug.LogError("Max verts in mesh must be greater than three.");
				}
				else if (value > MBVersion.MaxMeshVertexCount())
				{
					Debug.LogError("MultiMeshCombiner error in maxVertsInMesh. Meshes in unity cannot have more than " + MBVersion.MaxMeshVertexCount() + " vertices. " + value);
				}
				else
				{
					_maxVertsInMesh = value;
				}
			}
		}
	}

	public override int GetNumObjectsInCombined()
	{
		return obj2MeshCombinerMap.Count;
	}

	public override List<GameObject> GetObjectsInCombined()
	{
		List<GameObject> list = new List<GameObject>();
		for (int i = 0; i < meshCombiners.Count; i++)
		{
			list.AddRange(meshCombiners[i].combinedMesh.GetObjectsInCombined());
		}
		return list;
	}

	public override int GetLightmapIndex()
	{
		if (meshCombiners.Count > 0)
		{
			return meshCombiners[0].combinedMesh.GetLightmapIndex();
		}
		return -1;
	}

	public override bool CombinedMeshContains(GameObject go)
	{
		return obj2MeshCombinerMap.ContainsKey(go.GetInstanceID());
	}

	private bool _validateTextureBakeResults()
	{
		if (_textureBakeResults == null)
		{
			Debug.LogError("Texture Bake Results is null. Can't combine meshes.");
			return false;
		}
		if (_textureBakeResults.materialsAndUVRects == null || _textureBakeResults.materialsAndUVRects.Length == 0)
		{
			Debug.LogError("Texture Bake Results has no materials in material to sourceUVRect map. Try baking materials. Can't combine meshes. If you are trying to combine meshes without combining materials, try removing the Texture Bake Result.");
			return false;
		}
		if (_textureBakeResults.NumResultMaterials() == 0)
		{
			Debug.LogError("Texture Bake Results has no result materials. Try baking materials. Can't combine meshes.");
			return false;
		}
		return true;
	}

	public override bool Apply(GenerateUV2Delegate uv2GenerationMethod)
	{
		bool flag = true;
		if (_bakeStatus != MeshCombiningStatus.readyForApply)
		{
			Debug.LogError("Apply was called when combiner was not in 'readyForApply' state. Did you call AddDelete(), Update() or ShowHide()");
			return false;
		}
		for (int i = 0; i < meshCombiners.Count; i++)
		{
			if (meshCombiners[i].isDirty)
			{
				flag &= meshCombiners[i].combinedMesh.Apply(uv2GenerationMethod);
				meshCombiners[i].isDirty = false;
			}
		}
		if (base.settings.clearBuffersAfterBake)
		{
			obj2MeshCombinerMap.Clear();
		}
		_bakeStatus = MeshCombiningStatus.preAddDeleteOrUpdate;
		return flag;
	}

	public override bool Apply(bool triangles, bool vertices, bool normals, bool tangents, bool uvs, bool uv2, bool uv3, bool uv4, bool colors, bool bones = false, bool blendShapeFlag = false, GenerateUV2Delegate uv2GenerationMethod = null)
	{
		return Apply(triangles, vertices, normals, tangents, uvs, uv2, uv3, uv4, uv5: false, uv6: false, uv7: false, uv8: false, colors, bones, blendShapeFlag, uv2GenerationMethod);
	}

	public override bool Apply(bool triangles, bool vertices, bool normals, bool tangents, bool uvs, bool uv2, bool uv3, bool uv4, bool uv5, bool uv6, bool uv7, bool uv8, bool colors, bool bones = false, bool blendShapesFlag = false, GenerateUV2Delegate uv2GenerationMethod = null)
	{
		if (_bakeStatus != MeshCombiningStatus.readyForApply)
		{
			Debug.LogError("Apply was called when combiner was not in 'readyForApply' state. Did you call AddDelete(), Update() or ShowHide()");
			return false;
		}
		bool flag = true;
		for (int i = 0; i < meshCombiners.Count; i++)
		{
			if (meshCombiners[i].isDirty)
			{
				flag &= meshCombiners[i].combinedMesh.Apply(triangles, vertices, normals, tangents, uvs, uv2, uv3, uv4, colors, bones, blendShapesFlag, uv2GenerationMethod);
				meshCombiners[i].isDirty = false;
			}
		}
		if (base.settings.clearBuffersAfterBake)
		{
			obj2MeshCombinerMap.Clear();
		}
		_bakeStatus = MeshCombiningStatus.preAddDeleteOrUpdate;
		return flag;
	}

	public override void UpdateSkinnedMeshApproximateBounds()
	{
		for (int i = 0; i < meshCombiners.Count; i++)
		{
			meshCombiners[i].combinedMesh.UpdateSkinnedMeshApproximateBounds();
		}
	}

	public override void UpdateSkinnedMeshApproximateBoundsFromBones()
	{
		for (int i = 0; i < meshCombiners.Count; i++)
		{
			meshCombiners[i].combinedMesh.UpdateSkinnedMeshApproximateBoundsFromBones();
		}
	}

	public override void UpdateSkinnedMeshApproximateBoundsFromBounds()
	{
		for (int i = 0; i < meshCombiners.Count; i++)
		{
			meshCombiners[i].combinedMesh.UpdateSkinnedMeshApproximateBoundsFromBounds();
		}
	}

	public override bool UpdateGameObjects(GameObject[] gos, bool recalcBounds, bool updateVertices, bool updateNormals, bool updateTangents, bool updateUV, bool updateUV2, bool updateUV3, bool updateUV4, bool updateColors, bool updateSkinningInfo)
	{
		return UpdateGameObjects(gos, recalcBounds, updateVertices, updateNormals, updateTangents, updateUV, updateUV2, updateUV3, updateUV4, updateUV5: false, updateUV6: false, updateUV7: false, updateUV8: false, updateColors, updateSkinningInfo);
	}

	public override bool UpdateGameObjects(GameObject[] gos, bool recalcBounds, bool updateVertices, bool updateNormals, bool updateTangents, bool updateUV, bool updateUV2, bool updateUV3, bool updateUV4, bool updateUV5, bool updateUV6, bool updateUV7, bool updateUV8, bool updateColors, bool updateSkinningInfo)
	{
		if (gos == null)
		{
			Debug.LogError("list of game objects cannot be null");
			return false;
		}
		if (_bakeStatus != 0)
		{
			Debug.LogError("Bake Status of combiner was not 'preAddDeleteOrUpdate'. This can happen if AddDeleteGameObjects or UpdateGameObjects is called twice without calling Apply. You can call 'ClearBuffers' to reset the combiner.");
			return false;
		}
		for (int i = 0; i < meshCombiners.Count; i++)
		{
			meshCombiners[i].gosToUpdate.Clear();
		}
		for (int j = 0; j < gos.Length; j++)
		{
			CombinedMesh value = null;
			obj2MeshCombinerMap.TryGetValue(gos[j].GetInstanceID(), out value);
			if (value != null)
			{
				value.gosToUpdate.Add(gos[j]);
			}
			else
			{
				Debug.LogWarning("Object " + gos[j]?.ToString() + " is not in the combined mesh.");
			}
		}
		bool flag = true;
		for (int k = 0; k < meshCombiners.Count; k++)
		{
			if (meshCombiners[k].gosToUpdate.Count > 0)
			{
				meshCombiners[k].isDirty = true;
				GameObject[] gos2 = meshCombiners[k].gosToUpdate.ToArray();
				flag = flag && meshCombiners[k].combinedMesh.UpdateGameObjects(gos2, recalcBounds, updateVertices, updateNormals, updateTangents, updateUV, updateUV2, updateUV3, updateUV4, updateUV5, updateUV6, updateUV7, updateUV8, updateColors, updateSkinningInfo);
			}
		}
		_bakeStatus = MeshCombiningStatus.readyForApply;
		return flag;
	}

	public override bool AddDeleteGameObjects(GameObject[] gos, GameObject[] deleteGOs, bool disableRendererInSource = true)
	{
		int[] array = null;
		if (deleteGOs != null)
		{
			array = new int[deleteGOs.Length];
			for (int i = 0; i < deleteGOs.Length; i++)
			{
				if (deleteGOs[i] == null)
				{
					Debug.LogError("The " + i + "th object on the list of objects to delete is 'Null'");
				}
				else
				{
					array[i] = deleteGOs[i].GetInstanceID();
				}
			}
		}
		return AddDeleteGameObjectsByID(gos, array, disableRendererInSource);
	}

	public override bool AddDeleteGameObjectsByID(GameObject[] gos, int[] deleteGOinstanceIDs, bool disableRendererInSource = true)
	{
		if (_bakeStatus != 0)
		{
			Debug.LogError("Bake Status of combiner was not 'preAddDeleteOrUpdate'. This can happen if AddDeleteGameObjects or UpdateGameObjects is called twice without calling Apply. You can call 'ClearBuffers' to reset the combiner.");
			return false;
		}
		if (deleteGOinstanceIDs == null)
		{
			deleteGOinstanceIDs = emptyIDs;
		}
		if (_usingTemporaryTextureBakeResult && gos != null && gos.Length != 0)
		{
			MB_Utility.Destroy(_textureBakeResults);
			_textureBakeResults = null;
			_usingTemporaryTextureBakeResult = false;
		}
		if (_textureBakeResults == null && gos != null && gos.Length != 0 && gos[0] != null && !_CreateTemporaryTextrueBakeResult(gos, GetMaterialsOnTargetRenderer()))
		{
			return false;
		}
		if (!_validate(gos, deleteGOinstanceIDs))
		{
			return false;
		}
		_distributeAmongBakers(gos, deleteGOinstanceIDs);
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			MB2_Log.LogDebug("MB2_MultiMeshCombiner.AddDeleteGameObjects numCombinedMeshes: " + meshCombiners.Count + " added:" + ((gos != null) ? gos.Length : 0) + " deleted:" + ((deleteGOinstanceIDs != null) ? deleteGOinstanceIDs.Length : 0) + " disableRendererInSource:" + disableRendererInSource + " maxVertsPerCombined:" + _maxVertsInMesh);
		}
		bool result = _bakeStep1(gos, deleteGOinstanceIDs, disableRendererInSource);
		_bakeStatus = MeshCombiningStatus.readyForApply;
		return result;
	}

	private bool _validate(GameObject[] gos, int[] deleteGOinstanceIDs)
	{
		if (_validationLevel == MB2_ValidationLevel.none)
		{
			return true;
		}
		if (_maxVertsInMesh < 3)
		{
			Debug.LogError("Invalid value for maxVertsInMesh=" + _maxVertsInMesh);
		}
		_validateTextureBakeResults();
		int num = 0;
		for (int i = 0; i < meshCombiners.Count; i++)
		{
			num += meshCombiners[i].combinedMesh.GetNumObjectsInCombined();
		}
		if (obj2MeshCombinerMap.Count != num)
		{
			obj2MeshCombinerMap.Clear();
			for (int j = 0; j < meshCombiners.Count; j++)
			{
				List<MB3_MeshCombinerSingle.MB_DynamicGameObject> mbDynamicObjectsInCombinedMesh = meshCombiners[j].combinedMesh.mbDynamicObjectsInCombinedMesh;
				for (int k = 0; k < mbDynamicObjectsInCombinedMesh.Count; k++)
				{
					if (mbDynamicObjectsInCombinedMesh[k].gameObject != null)
					{
						int instanceID = mbDynamicObjectsInCombinedMesh[k].gameObject.GetInstanceID();
						mbDynamicObjectsInCombinedMesh[k].instanceID = instanceID;
					}
					obj2MeshCombinerMap.Add(mbDynamicObjectsInCombinedMesh[k].instanceID, meshCombiners[j]);
				}
			}
		}
		if (gos != null)
		{
			for (int l = 0; l < gos.Length; l++)
			{
				if (gos[l] == null)
				{
					Debug.LogError("The " + l + "th object on the list of objects to combine is 'None'. Use Command-Delete on Mac OS X; Delete or Shift-Delete on Windows to remove this one element.");
					return false;
				}
				if (_validationLevel < MB2_ValidationLevel.robust)
				{
					continue;
				}
				for (int m = l + 1; m < gos.Length; m++)
				{
					if (gos[l] == gos[m])
					{
						Debug.LogError("GameObject " + gos[l]?.ToString() + "appears twice in list of game objects to add");
						return false;
					}
				}
				if (!obj2MeshCombinerMap.ContainsKey(gos[l].GetInstanceID()))
				{
					continue;
				}
				bool flag = false;
				if (deleteGOinstanceIDs != null)
				{
					for (int n = 0; n < deleteGOinstanceIDs.Length; n++)
					{
						if (deleteGOinstanceIDs[n] == gos[l].GetInstanceID())
						{
							flag = true;
						}
					}
				}
				if (!flag)
				{
					Debug.LogError("GameObject " + gos[l]?.ToString() + " is already in the combined mesh " + gos[l].GetInstanceID());
					return false;
				}
			}
		}
		if (deleteGOinstanceIDs != null && _validationLevel >= MB2_ValidationLevel.robust)
		{
			for (int num2 = 0; num2 < deleteGOinstanceIDs.Length; num2++)
			{
				for (int num3 = num2 + 1; num3 < deleteGOinstanceIDs.Length; num3++)
				{
					if (deleteGOinstanceIDs[num2] == deleteGOinstanceIDs[num3])
					{
						Debug.LogError("GameObject " + deleteGOinstanceIDs[num2] + "appears twice in list of game objects to delete");
						return false;
					}
				}
				if (!obj2MeshCombinerMap.ContainsKey(deleteGOinstanceIDs[num2]))
				{
					Debug.LogWarning("GameObject with instance ID " + deleteGOinstanceIDs[num2] + " on the list of objects to delete is not in the combined mesh.");
				}
			}
		}
		return true;
	}

	private void _distributeAmongBakers(GameObject[] gos, int[] deleteGOinstanceIDs)
	{
		if (gos == null)
		{
			gos = empty;
		}
		if (deleteGOinstanceIDs == null)
		{
			deleteGOinstanceIDs = emptyIDs;
		}
		if (resultSceneObject == null)
		{
			resultSceneObject = new GameObject("CombinedMesh-" + base.name);
		}
		for (int i = 0; i < meshCombiners.Count; i++)
		{
			meshCombiners[i].extraSpace = _maxVertsInMesh - meshCombiners[i].combinedMesh.GetMesh().vertexCount;
		}
		for (int j = 0; j < deleteGOinstanceIDs.Length; j++)
		{
			CombinedMesh value = null;
			if (obj2MeshCombinerMap.TryGetValue(deleteGOinstanceIDs[j], out value))
			{
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					MB2_Log.LogDebug("MB2_MultiMeshCombiner.Removing " + deleteGOinstanceIDs[j] + " from meshCombiner " + meshCombiners.IndexOf(value));
				}
				value.combinedMesh.InstanceID2DGO(deleteGOinstanceIDs[j], out var dgoGameObject);
				value.numVertsInListToDelete += dgoGameObject.numVerts;
				value.gosToDelete.Add(deleteGOinstanceIDs[j]);
			}
			else
			{
				Debug.LogWarning("Object " + deleteGOinstanceIDs[j] + " in the list of objects to delete is not in the combined mesh.");
			}
		}
		for (int k = 0; k < gos.Length; k++)
		{
			GameObject gameObject = gos[k];
			int vertexCount = MB_Utility.GetMesh(gameObject).vertexCount;
			CombinedMesh combinedMesh = null;
			for (int l = 0; l < meshCombiners.Count; l++)
			{
				if (meshCombiners[l].extraSpace + meshCombiners[l].numVertsInListToDelete - meshCombiners[l].numVertsInListToAdd > vertexCount)
				{
					combinedMesh = meshCombiners[l];
					if (LOG_LEVEL >= MB2_LogLevel.debug)
					{
						MB2_Log.LogDebug("MB2_MultiMeshCombiner.Added " + gos[k]?.ToString() + " to combinedMesh " + l, LOG_LEVEL);
					}
					break;
				}
			}
			if (combinedMesh == null)
			{
				combinedMesh = new CombinedMesh(maxVertsInMesh, _resultSceneObject, _LOG_LEVEL);
				_setMBValues(combinedMesh.combinedMesh);
				meshCombiners.Add(combinedMesh);
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					MB2_Log.LogDebug("MB2_MultiMeshCombiner.Created new combinedMesh");
				}
			}
			combinedMesh.gosToAdd.Add(gameObject);
			combinedMesh.numVertsInListToAdd += vertexCount;
		}
	}

	private bool _bakeStep1(GameObject[] gos, int[] deleteGOinstanceIDs, bool disableRendererInSource)
	{
		for (int i = 0; i < meshCombiners.Count; i++)
		{
			CombinedMesh combinedMesh = meshCombiners[i];
			if (combinedMesh.combinedMesh.targetRenderer == null)
			{
				combinedMesh.combinedMesh.resultSceneObject = _resultSceneObject;
				combinedMesh.combinedMesh.BuildSceneMeshObject(gos, createNewChild: true);
				if (_LOG_LEVEL >= MB2_LogLevel.debug)
				{
					MB2_Log.LogDebug("BuildSO combiner {0} goID {1} targetRenID {2} meshID {3}", i, combinedMesh.combinedMesh.targetRenderer.gameObject.GetInstanceID(), combinedMesh.combinedMesh.targetRenderer.GetInstanceID(), combinedMesh.combinedMesh.GetMesh().GetInstanceID());
				}
			}
			else if (resultSceneObject != null && combinedMesh.combinedMesh.targetRenderer.transform.parent != resultSceneObject.transform)
			{
				Debug.LogError("targetRender objects must be children of resultSceneObject");
				return false;
			}
			if (combinedMesh.gosToAdd.Count > 0 || combinedMesh.gosToDelete.Count > 0)
			{
				combinedMesh.combinedMesh.AddDeleteGameObjectsByID(combinedMesh.gosToAdd.ToArray(), combinedMesh.gosToDelete.ToArray(), disableRendererInSource);
				if (_LOG_LEVEL >= MB2_LogLevel.debug)
				{
					MB2_Log.LogDebug("Baked combiner {0} obsAdded {1} objsRemoved {2} goID {3} targetRenID {4} meshID {5}", i, combinedMesh.gosToAdd.Count, combinedMesh.gosToDelete.Count, combinedMesh.combinedMesh.targetRenderer.gameObject.GetInstanceID(), combinedMesh.combinedMesh.targetRenderer.GetInstanceID(), combinedMesh.combinedMesh.GetMesh().GetInstanceID());
				}
			}
			Renderer renderer = combinedMesh.combinedMesh.targetRenderer;
			Mesh mesh = combinedMesh.combinedMesh.GetMesh();
			if (renderer is MeshRenderer)
			{
				renderer.gameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
			}
			else
			{
				((SkinnedMeshRenderer)renderer).sharedMesh = mesh;
			}
		}
		for (int j = 0; j < meshCombiners.Count; j++)
		{
			CombinedMesh combinedMesh2 = meshCombiners[j];
			for (int k = 0; k < combinedMesh2.gosToDelete.Count; k++)
			{
				obj2MeshCombinerMap.Remove(combinedMesh2.gosToDelete[k]);
			}
		}
		for (int l = 0; l < meshCombiners.Count; l++)
		{
			CombinedMesh combinedMesh3 = meshCombiners[l];
			for (int m = 0; m < combinedMesh3.gosToAdd.Count; m++)
			{
				obj2MeshCombinerMap.Add(combinedMesh3.gosToAdd[m].GetInstanceID(), combinedMesh3);
			}
			if (combinedMesh3.gosToAdd.Count > 0 || combinedMesh3.gosToDelete.Count > 0)
			{
				combinedMesh3.gosToDelete.Clear();
				combinedMesh3.gosToAdd.Clear();
				combinedMesh3.numVertsInListToDelete = 0;
				combinedMesh3.numVertsInListToAdd = 0;
				combinedMesh3.isDirty = true;
			}
		}
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			string text = "Meshes in combined:";
			for (int n = 0; n < meshCombiners.Count; n++)
			{
				text = text + " mesh" + n + "(" + meshCombiners[n].combinedMesh.GetObjectsInCombined().Count + ")\n";
			}
			text = text + "children in result: " + resultSceneObject.transform.childCount;
			MB2_Log.LogDebug(text, LOG_LEVEL);
		}
		if (meshCombiners.Count > 0)
		{
			return true;
		}
		return false;
	}

	public override void ClearBuffers()
	{
		for (int i = 0; i < meshCombiners.Count; i++)
		{
			meshCombiners[i].combinedMesh.ClearBuffers();
		}
		obj2MeshCombinerMap.Clear();
		_bakeStatus = MeshCombiningStatus.preAddDeleteOrUpdate;
	}

	public override void ClearMesh()
	{
		DestroyMesh();
		ClearBuffers();
	}

	public override void ClearMesh(MB2_EditorMethodsInterface editorMethods)
	{
		DestroyMeshEditor(editorMethods);
		ClearBuffers();
	}

	internal override void _DisposeRuntimeCreated()
	{
		for (int i = 0; i < meshCombiners.Count; i++)
		{
			meshCombiners[i].combinedMesh._DisposeRuntimeCreated();
		}
	}

	public override void DestroyMesh()
	{
		for (int i = 0; i < meshCombiners.Count; i++)
		{
			if (meshCombiners[i].combinedMesh.targetRenderer != null)
			{
				MB_Utility.Destroy(meshCombiners[i].combinedMesh.targetRenderer.gameObject);
			}
			meshCombiners[i].combinedMesh.Dispose();
		}
		obj2MeshCombinerMap.Clear();
		meshCombiners.Clear();
	}

	public override void DestroyMeshEditor(MB2_EditorMethodsInterface editorMethods)
	{
		editorMethods.Destroy(resultSceneObject);
		for (int i = 0; i < meshCombiners.Count; i++)
		{
			meshCombiners[i].combinedMesh.ClearMesh();
		}
		obj2MeshCombinerMap.Clear();
		meshCombiners.Clear();
	}

	private void _setMBValues(MB3_MeshCombinerSingle targ)
	{
		targ.validationLevel = _validationLevel;
		targ.textureBakeResults = textureBakeResults;
		targ.outputOption = MB2_OutputOptions.bakeIntoSceneObject;
		if (settingsHolder != null)
		{
			targ.settingsHolder = settingsHolder;
			return;
		}
		targ.renderType = renderType;
		targ.lightmapOption = lightmapOption;
		targ.doNorm = doNorm;
		targ.doTan = doTan;
		targ.doCol = doCol;
		targ.doUV = doUV;
		targ.doUV3 = doUV3;
		targ.doUV4 = doUV4;
		targ.doUV5 = doUV5;
		targ.doUV6 = doUV6;
		targ.doUV7 = doUV7;
		targ.doUV8 = doUV8;
		targ.doBlendShapes = doBlendShapes;
		targ.optimizeAfterBake = base.optimizeAfterBake;
		targ.pivotLocationType = pivotLocationType;
		targ.uv2UnwrappingParamsHardAngle = base.uv2UnwrappingParamsHardAngle;
		targ.uv2UnwrappingParamsPackMargin = base.uv2UnwrappingParamsPackMargin;
		targ.assignToMeshCustomizer = base.assignToMeshCustomizer;
	}

	public override List<Material> GetMaterialsOnTargetRenderer()
	{
		HashSet<Material> hashSet = new HashSet<Material>();
		for (int i = 0; i < meshCombiners.Count; i++)
		{
			hashSet.UnionWith(meshCombiners[i].combinedMesh.GetMaterialsOnTargetRenderer());
		}
		return new List<Material>(hashSet);
	}

	public override void CheckIntegrity()
	{
		if (MB_Utility.DO_INTEGRITY_CHECKS)
		{
			for (int i = 0; i < meshCombiners.Count; i++)
			{
				meshCombiners[i].combinedMesh.CheckIntegrity();
			}
		}
	}
}
