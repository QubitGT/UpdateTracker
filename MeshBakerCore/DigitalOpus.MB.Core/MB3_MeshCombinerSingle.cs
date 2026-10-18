using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Scripting;

namespace DigitalOpus.MB.Core;

[Serializable]
public class MB3_MeshCombinerSingle : MB3_MeshCombiner
{
	internal class MB_MeshCombinerSingle_BlendShapeProcessor
	{
		private MB3_MeshCombinerSingle combiner;

		private MBBlendShape[] nblendShapes;

		private bool _disposed;

		protected void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					combiner = null;
					nblendShapes = null;
				}
				_disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
		}

		public MB_MeshCombinerSingle_BlendShapeProcessor(MB3_MeshCombinerSingle cm)
		{
			combiner = cm;
		}

		public static MBBlendShape[] GetBlendShapes(Mesh m, GameObject gameObject, Dictionary<int, MeshChannels> meshID2MeshChannels)
		{
			if (!meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out var value))
			{
				value = new MeshChannels();
				meshID2MeshChannels.Add(m.GetInstanceID(), value);
			}
			if (value.blendShapes == null)
			{
				MBBlendShape[] array = new MBBlendShape[m.blendShapeCount];
				int vertexCount = m.vertexCount;
				for (int i = 0; i < array.Length; i++)
				{
					MBBlendShape mBBlendShape = (array[i] = new MBBlendShape());
					mBBlendShape.frames = new MBBlendShapeFrame[MBVersion.GetBlendShapeFrameCount(m, i)];
					mBBlendShape.name = m.GetBlendShapeName(i);
					mBBlendShape.indexInSource = i;
					mBBlendShape.gameObject = gameObject;
					for (int j = 0; j < mBBlendShape.frames.Length; j++)
					{
						MBBlendShapeFrame mBBlendShapeFrame = (mBBlendShape.frames[j] = new MBBlendShapeFrame());
						mBBlendShapeFrame.frameWeight = MBVersion.GetBlendShapeFrameWeight(m, i, j);
						mBBlendShapeFrame.vertices = new Vector3[vertexCount];
						mBBlendShapeFrame.normals = new Vector3[vertexCount];
						mBBlendShapeFrame.tangents = new Vector3[vertexCount];
						MBVersion.GetBlendShapeFrameVertices(m, i, j, mBBlendShapeFrame.vertices, mBBlendShapeFrame.normals, mBBlendShapeFrame.tangents);
					}
				}
				value.blendShapes = array;
				return value.blendShapes;
			}
			MBBlendShape[] array2 = new MBBlendShape[value.blendShapes.Length];
			for (int k = 0; k < array2.Length; k++)
			{
				array2[k] = new MBBlendShape();
				array2[k].name = value.blendShapes[k].name;
				array2[k].indexInSource = value.blendShapes[k].indexInSource;
				array2[k].frames = value.blendShapes[k].frames;
				array2[k].gameObject = gameObject;
			}
			return array2;
		}

		internal void ApplyBlendShapeFramesToMeshAndBuildMap(int newVertCount)
		{
			Renderer targetRenderer = combiner._targetRenderer;
			Mesh mesh = combiner._mesh;
			if (combiner.blendShapes.Length != nblendShapes.Length)
			{
				combiner.blendShapes = new MBBlendShape[nblendShapes.Length];
			}
			Vector3[] array = new Vector3[newVertCount];
			Vector3[] array2 = new Vector3[newVertCount];
			Vector3[] array3 = new Vector3[newVertCount];
			((SkinnedMeshRenderer)targetRenderer).sharedMesh = null;
			MBVersion.ClearBlendShapes(mesh);
			for (int i = 0; i < nblendShapes.Length; i++)
			{
				MBBlendShape mBBlendShape = nblendShapes[i];
				MB_DynamicGameObject mB_DynamicGameObject = combiner.instance2Combined_MapGet(mBBlendShape.gameObject);
				if (mB_DynamicGameObject != null)
				{
					int vertIdx = mB_DynamicGameObject.vertIdx;
					for (int j = 0; j < mBBlendShape.frames.Length; j++)
					{
						MBBlendShapeFrame mBBlendShapeFrame = mBBlendShape.frames[j];
						Array.Copy(mBBlendShapeFrame.vertices, 0, array, vertIdx, mBBlendShapeFrame.vertices.Length);
						Array.Copy(mBBlendShapeFrame.normals, 0, array2, vertIdx, mBBlendShapeFrame.normals.Length);
						Array.Copy(mBBlendShapeFrame.tangents, 0, array3, vertIdx, mBBlendShapeFrame.tangents.Length);
						MBVersion.AddBlendShapeFrame(mesh, _ConvertBlendShapeNameToOutputName(mBBlendShape.name) + mBBlendShape.gameObject.GetInstanceID(), mBBlendShapeFrame.frameWeight, array, array2, array3);
						_ZeroArray(array, vertIdx, mBBlendShapeFrame.vertices.Length);
						_ZeroArray(array2, vertIdx, mBBlendShapeFrame.normals.Length);
						_ZeroArray(array3, vertIdx, mBBlendShapeFrame.tangents.Length);
					}
				}
				else
				{
					UnityEngine.Debug.LogError("InstanceID in blend shape that was not in instance2combinedMap");
				}
				combiner.blendShapes[i] = mBBlendShape;
			}
			((SkinnedMeshRenderer)targetRenderer).sharedMesh = null;
			((SkinnedMeshRenderer)targetRenderer).sharedMesh = mesh;
			if (combiner.settings.doBlendShapes)
			{
				MB_BlendShape2CombinedMap mB_BlendShape2CombinedMap = targetRenderer.GetComponent<MB_BlendShape2CombinedMap>();
				if (mB_BlendShape2CombinedMap == null)
				{
					mB_BlendShape2CombinedMap = targetRenderer.gameObject.AddComponent<MB_BlendShape2CombinedMap>();
				}
				SerializableSourceBlendShape2Combined map = mB_BlendShape2CombinedMap.GetMap();
				_BuildSrcShape2CombinedMap(combiner, map, nblendShapes);
			}
		}

		public void AllocateBlendShapeArrayIfNecessary(int nBlendShapeSize)
		{
			if (combiner.settings.doBlendShapes)
			{
				nblendShapes = new MBBlendShape[nBlendShapeSize];
			}
		}

		public void AssignNewBlendShapesToCombinerIfNecessary()
		{
			if (combiner.settings.doBlendShapes)
			{
				combiner.blendShapes = nblendShapes;
			}
		}

		public void CopyBlendShapesInCurrentMeshIfNecessary(ref int targBlendShapeIdx, MB_DynamicGameObject dgo)
		{
			if (combiner.settings.doBlendShapes)
			{
				Array.Copy(combiner.blendShapes, dgo.blendShapeIdx, nblendShapes, targBlendShapeIdx, dgo.numBlendShapes);
				dgo.blendShapeIdx = targBlendShapeIdx;
				targBlendShapeIdx += dgo.numBlendShapes;
			}
		}

		public void CopyBlendShapesForNewMeshIfNecessary(ref int targBlendShapeIdx, MB_DynamicGameObject dgo, Mesh mesh, IMeshChannelsCacheTaggingInterface meshChannelCache)
		{
			if (combiner.settings.doBlendShapes)
			{
				int index = targBlendShapeIdx;
				MBBlendShape[] blendShapes = meshChannelCache.GetBlendShapes(mesh, dgo.gameObject.GetInstanceID(), dgo.gameObject);
				blendShapes.CopyTo(nblendShapes, index);
				dgo.blendShapeIdx = targBlendShapeIdx;
				targBlendShapeIdx += blendShapes.Length;
			}
		}

		private static string _ConvertBlendShapeNameToOutputName(string bs)
		{
			return bs.Split('.')[^1];
		}

		internal void ApplyBlendShapeFramesToMeshAndBuildMap_MergeBlendShapesWithTheSameName(int newVertCount)
		{
			Renderer targetRenderer = combiner._targetRenderer;
			Mesh mesh = combiner._mesh;
			Vector3[] array = new Vector3[newVertCount];
			Vector3[] array2 = new Vector3[newVertCount];
			Vector3[] array3 = new Vector3[newVertCount];
			MBVersion.ClearBlendShapes(mesh);
			bool flag = false;
			Dictionary<string, List<MBBlendShape>> dictionary = new Dictionary<string, List<MBBlendShape>>();
			for (int i = 0; i < nblendShapes.Length; i++)
			{
				MBBlendShape mBBlendShape = nblendShapes[i];
				string key = _ConvertBlendShapeNameToOutputName(mBBlendShape.name);
				if (!dictionary.TryGetValue(key, out var value))
				{
					value = new List<MBBlendShape>();
					dictionary.Add(key, value);
				}
				value.Add(mBBlendShape);
				if (value.Count > 1 && value[0].frames.Length != mBBlendShape.frames.Length)
				{
					UnityEngine.Debug.LogError("BlendShapes with the same name must have the same number of frames.");
					flag = true;
				}
			}
			if (flag)
			{
				return;
			}
			if (combiner.blendShapes.Length != nblendShapes.Length)
			{
				combiner.blendShapes = new MBBlendShape[dictionary.Keys.Count];
			}
			int num = 0;
			foreach (string key2 in dictionary.Keys)
			{
				List<MBBlendShape> list = dictionary[key2];
				MBBlendShape mBBlendShape2 = list[0];
				int num2 = mBBlendShape2.frames.Length;
				int num3 = 0;
				int num4 = 0;
				string text = "";
				for (int j = 0; j < num2; j++)
				{
					float frameWeight = mBBlendShape2.frames[j].frameWeight;
					for (int k = 0; k < list.Count; k++)
					{
						MBBlendShape mBBlendShape3 = list[k];
						int vertIdx = combiner.instance2Combined_MapGet(mBBlendShape3.gameObject).vertIdx;
						MBBlendShapeFrame mBBlendShapeFrame = mBBlendShape3.frames[j];
						Array.Copy(mBBlendShapeFrame.vertices, 0, array, vertIdx, mBBlendShapeFrame.vertices.Length);
						Array.Copy(mBBlendShapeFrame.normals, 0, array2, vertIdx, mBBlendShapeFrame.normals.Length);
						Array.Copy(mBBlendShapeFrame.tangents, 0, array3, vertIdx, mBBlendShapeFrame.tangents.Length);
						if (j == 0)
						{
							num3 += mBBlendShapeFrame.vertices.Length;
							text = text + mBBlendShape3.gameObject.name + " " + vertIdx + ":" + (vertIdx + mBBlendShapeFrame.vertices.Length) + ", ";
						}
					}
					num4 += list.Count;
					MBVersion.AddBlendShapeFrame(mesh, key2, frameWeight, array, array2, array3);
					_ZeroArray(array, 0, array.Length);
					_ZeroArray(array2, 0, array2.Length);
					_ZeroArray(array3, 0, array3.Length);
				}
				combiner.blendShapes[num] = mBBlendShape2;
				num++;
			}
			((SkinnedMeshRenderer)targetRenderer).sharedMesh = null;
			((SkinnedMeshRenderer)targetRenderer).sharedMesh = mesh;
			if (combiner.settings.doBlendShapes)
			{
				MB_BlendShape2CombinedMap mB_BlendShape2CombinedMap = targetRenderer.GetComponent<MB_BlendShape2CombinedMap>();
				if (mB_BlendShape2CombinedMap == null)
				{
					mB_BlendShape2CombinedMap = targetRenderer.gameObject.AddComponent<MB_BlendShape2CombinedMap>();
				}
				SerializableSourceBlendShape2Combined map = mB_BlendShape2CombinedMap.GetMap();
				_BuildSrcShape2CombinedMap(combiner, map, combiner.blendShapes);
			}
		}

		private static void _BuildSrcShape2CombinedMap(MB3_MeshCombinerSingle combiner, SerializableSourceBlendShape2Combined map, MBBlendShape[] bs)
		{
			MBBlendShape[] blendShapes = combiner.blendShapes;
			Renderer targetRenderer = combiner._targetRenderer;
			if (combiner._mesh != null && combiner._mesh.blendShapeCount != combiner.blendShapes.Length)
			{
				UnityEngine.Debug.LogError("Blend shapes in combiner did not match blend shapes in mesh. Map will probably be invalid.");
			}
			GameObject[] array = new GameObject[bs.Length];
			int[] array2 = new int[bs.Length];
			GameObject[] array3 = new GameObject[bs.Length];
			int[] array4 = new int[bs.Length];
			for (int i = 0; i < blendShapes.Length; i++)
			{
				array[i] = blendShapes[i].gameObject;
				array2[i] = blendShapes[i].indexInSource;
				array3[i] = targetRenderer.gameObject;
				array4[i] = i;
			}
			map.SetBuffers(array, array2, array3, array4);
		}

		private static void _ZeroArray(Vector3[] arr, int idx, int length)
		{
			int num = idx + length;
			for (int i = idx; i < num; i++)
			{
				arr[i] = Vector3.zero;
			}
		}
	}

	public class MB_MeshCombinerSingle_BoneProcessor : MB_IMeshCombinerSingle_BoneProcessor, IDisposable
	{
		private MB3_MeshCombinerSingle combiner;

		private List<MB_DynamicGameObject>[] boneIdx2dgoMap;

		private HashSet<int> boneIdxsToDelete = new HashSet<int>();

		private HashSet<BoneAndBindpose> bonesToAdd = new HashSet<BoneAndBindpose>();

		private Dictionary<BoneAndBindpose, int> boneAndBindPose2idx = new Dictionary<BoneAndBindpose, int>();

		private Transform[] oldBonesPreviousBake;

		private Matrix4x4[] oldBindPosesPreviousBake;

		private Transform[] nbones;

		private Matrix4x4[] nbindPoses;

		private BoneWeight[] nboneWeights;

		private BoneWeight[] boneWeights = new BoneWeight[0];

		private int _newBonesStartAtIdx;

		private bool _disposed;

		private bool _didSetup;

		protected void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					combiner = null;
					boneIdx2dgoMap = null;
					boneIdxsToDelete = null;
					bonesToAdd = null;
					boneAndBindPose2idx = null;
					boneWeights = null;
				}
				_disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
		}

		public int GetNewBonesSize()
		{
			if (nbones != null)
			{
				return nbones.Length;
			}
			return 0;
		}

		public MB_MeshCombinerSingle_BoneProcessor(MB3_MeshCombinerSingle cm)
		{
			combiner = cm;
			oldBonesPreviousBake = combiner.bones;
			oldBindPosesPreviousBake = combiner.bindPoses;
		}

		public HashSet<BoneAndBindpose> GetBonesToAdd()
		{
			return bonesToAdd;
		}

		public int GetNumBonesToDelete()
		{
			return boneIdxsToDelete.Count;
		}

		public void BuildBoneIdx2DGOMapIfNecessary(int[] _goToDelete)
		{
			_didSetup = false;
			if (combiner.settings.renderType == MB_RenderType.skinnedMeshRenderer)
			{
				if (_goToDelete != null && _goToDelete.Length != 0)
				{
					boneIdx2dgoMap = _buildBoneIdx2dgoMap();
				}
				for (int i = 0; i < oldBonesPreviousBake.Length; i++)
				{
					BoneAndBindpose key = new BoneAndBindpose(oldBonesPreviousBake[i], oldBindPosesPreviousBake[i]);
					boneAndBindPose2idx.Add(key, i);
				}
				_didSetup = true;
			}
		}

		public void RemoveBonesForDgosWeAreDeleting(MB_DynamicGameObject dgo)
		{
			for (int i = 0; i < dgo.indexesOfBonesUsed.Length; i++)
			{
				int num = dgo.indexesOfBonesUsed[i];
				List<MB_DynamicGameObject> list = boneIdx2dgoMap[num];
				if (list.Contains(dgo))
				{
					list.Remove(dgo);
					if (list.Count == 0)
					{
						boneIdxsToDelete.Add(num);
					}
				}
			}
		}

		public void AllocateAndSetupSMRDataStructures(List<MB_DynamicGameObject> toAddDGOs, List<MB_DynamicGameObject> mbDynamicObjectsInCombinedMesh, int newVertSize, IVertexAndTriangleProcessor vertexAndTriangleProcessor)
		{
			if (combiner.settings.renderType == MB_RenderType.skinnedMeshRenderer)
			{
				_CollectSkinningDataForDGOsInCombinedMesh(toAddDGOs);
				int newBonesLength = GetNewBonesLength();
				nbones = new Transform[newBonesLength];
				nbindPoses = new Matrix4x4[newBonesLength];
				nboneWeights = new BoneWeight[newVertSize];
				_newBonesStartAtIdx = oldBindPosesPreviousBake.Length - GetNumBonesToDelete();
				boneWeights = combiner._mesh.boneWeights;
			}
		}

		public void UpdateGameObjects_ReadBoneWeightInfoFromCombinedMesh()
		{
			if (combiner.settings.renderType != MB_RenderType.skinnedMeshRenderer)
			{
				return;
			}
			boneWeights = combiner._mesh.boneWeights;
			if (combiner.mbDynamicObjectsInCombinedMesh.Count <= 0 || combiner.mbDynamicObjectsInCombinedMesh[0].indexesOfBonesUsed.Length != 0 || combiner.settings.renderType != MB_RenderType.skinnedMeshRenderer || boneWeights == null || boneWeights.Length == 0)
			{
				return;
			}
			for (int i = 0; i < combiner.mbDynamicObjectsInCombinedMesh.Count; i++)
			{
				MB_DynamicGameObject mB_DynamicGameObject = combiner.mbDynamicObjectsInCombinedMesh[i];
				HashSet<int> hashSet = new HashSet<int>();
				for (int j = mB_DynamicGameObject.vertIdx; j < mB_DynamicGameObject.vertIdx + mB_DynamicGameObject.numVerts; j++)
				{
					if (boneWeights[j].weight0 > 0f)
					{
						hashSet.Add(boneWeights[j].boneIndex0);
					}
					if (boneWeights[j].weight1 > 0f)
					{
						hashSet.Add(boneWeights[j].boneIndex1);
					}
					if (boneWeights[j].weight2 > 0f)
					{
						hashSet.Add(boneWeights[j].boneIndex2);
					}
					if (boneWeights[j].weight3 > 0f)
					{
						hashSet.Add(boneWeights[j].boneIndex3);
					}
				}
				mB_DynamicGameObject.indexesOfBonesUsed = new int[hashSet.Count];
				hashSet.CopyTo(mB_DynamicGameObject.indexesOfBonesUsed);
			}
			if (combiner.LOG_LEVEL >= MB2_LogLevel.debug)
			{
				UnityEngine.Debug.Log("Baker used old systems that duplicated bones. Upgrading to new system by building indexesOfBonesUsed");
			}
		}

		public int GetNewBonesLength()
		{
			if (combiner.settings.renderType == MB_RenderType.skinnedMeshRenderer)
			{
				return oldBindPosesPreviousBake.Length + bonesToAdd.Count - boneIdxsToDelete.Count;
			}
			return 0;
		}

		internal void _CollectSkinningDataForDGOsInCombinedMesh(List<MB_DynamicGameObject> objsToAdd)
		{
			for (int i = 0; i < objsToAdd.Count; i++)
			{
				MB_DynamicGameObject mB_DynamicGameObject = objsToAdd[i];
				CollectBonesToAddForDGO(mB_DynamicGameObject, MB_Utility.GetRenderer(mB_DynamicGameObject.gameObject), combiner.settings.smrNoExtraBonesWhenCombiningMeshRenderers);
			}
		}

		public bool CollectBonesToAddForDGO(MB_DynamicGameObject dgo, Renderer r, bool noExtraBonesForMeshRenderers)
		{
			bool flag = true;
			MeshChannelsCache meshChannelsCache = (MeshChannelsCache)combiner._meshChannelsCache;
			List<Matrix4x4> list = (dgo._tmpSMR_CachedBindposes = meshChannelsCache.GetBindposes(r, out dgo.isSkinnedMeshWithBones));
			dgo._tmpSMR_CachedBoneWeights = meshChannelsCache.GetBoneWeights(r, dgo.numVerts, dgo.isSkinnedMeshWithBones);
			Transform[] array = (dgo._tmpSMR_CachedBones = combiner._getBones(r, dgo.isSkinnedMeshWithBones));
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == null)
				{
					UnityEngine.Debug.LogError("Source mesh r had a 'null' bone. Bones must not be null: " + r);
					flag = false;
				}
			}
			if (!flag)
			{
				return flag;
			}
			if (noExtraBonesForMeshRenderers && MB_Utility.GetRenderer(dgo.gameObject) is MeshRenderer)
			{
				bool flag2 = false;
				BoneAndBindpose boneAndBindpose = default(BoneAndBindpose);
				Transform parent = dgo.gameObject.transform.parent;
				while (parent != null)
				{
					foreach (BoneAndBindpose key in boneAndBindPose2idx.Keys)
					{
						if (key.bone == parent)
						{
							boneAndBindpose = key;
							flag2 = true;
							break;
						}
					}
					foreach (BoneAndBindpose item in bonesToAdd)
					{
						if (item.bone == parent)
						{
							boneAndBindpose = item;
							flag2 = true;
							break;
						}
					}
					if (flag2)
					{
						break;
					}
					parent = parent.parent;
				}
				if (flag2)
				{
					array[0] = boneAndBindpose.bone;
					list[0] = boneAndBindpose.bindPose;
				}
			}
			int[] array2 = new int[array.Length];
			for (int j = 0; j < array2.Length; j++)
			{
				array2[j] = j;
			}
			for (int k = 0; k < array.Length; k++)
			{
				bool flag3 = false;
				int num = array2[k];
				BoneAndBindpose boneAndBindpose2 = new BoneAndBindpose(array[num], list[num]);
				if (boneAndBindPose2idx.TryGetValue(boneAndBindpose2, out var value) && array[num] == oldBonesPreviousBake[value] && !boneIdxsToDelete.Contains(value) && list[num] == oldBindPosesPreviousBake[value])
				{
					flag3 = true;
				}
				if (!flag3 && !bonesToAdd.Contains(boneAndBindpose2))
				{
					bonesToAdd.Add(boneAndBindpose2);
				}
			}
			dgo._tmpSMR_srcMeshBoneIdx2masterListBoneIdx = array2;
			return flag;
		}

		private List<MB_DynamicGameObject>[] _buildBoneIdx2dgoMap()
		{
			List<MB_DynamicGameObject>[] array = new List<MB_DynamicGameObject>[oldBonesPreviousBake.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new List<MB_DynamicGameObject>();
			}
			for (int j = 0; j < combiner.mbDynamicObjectsInCombinedMesh.Count; j++)
			{
				MB_DynamicGameObject mB_DynamicGameObject = combiner.mbDynamicObjectsInCombinedMesh[j];
				for (int k = 0; k < mB_DynamicGameObject.indexesOfBonesUsed.Length; k++)
				{
					array[mB_DynamicGameObject.indexesOfBonesUsed[k]].Add(mB_DynamicGameObject);
				}
			}
			return array;
		}

		public void CopyBonesWeAreKeepingToNewBonesArrayAndAdjustBWIndexes(int totalDeleteVerts)
		{
			if (boneIdxsToDelete.Count > 0)
			{
				int[] array = new int[boneIdxsToDelete.Count];
				boneIdxsToDelete.CopyTo(array);
				Array.Sort(array);
				int[] array2 = new int[oldBonesPreviousBake.Length];
				int num = 0;
				int num2 = 0;
				for (int i = 0; i < oldBonesPreviousBake.Length; i++)
				{
					if (num2 < array.Length && array[num2] == i)
					{
						num2++;
						array2[i] = -1;
						continue;
					}
					array2[i] = num;
					nbones[num] = oldBonesPreviousBake[i];
					nbindPoses[num] = oldBindPosesPreviousBake[i];
					num++;
				}
				int num3 = boneWeights.Length - totalDeleteVerts;
				for (int j = 0; j < num3; j++)
				{
					BoneWeight boneWeight = nboneWeights[j];
					boneWeight.boneIndex0 = array2[boneWeight.boneIndex0];
					boneWeight.boneIndex1 = array2[boneWeight.boneIndex1];
					boneWeight.boneIndex2 = array2[boneWeight.boneIndex2];
					boneWeight.boneIndex3 = array2[boneWeight.boneIndex3];
					nboneWeights[j] = boneWeight;
				}
				for (int k = 0; k < combiner.mbDynamicObjectsInCombinedMesh.Count; k++)
				{
					MB_DynamicGameObject mB_DynamicGameObject = combiner.mbDynamicObjectsInCombinedMesh[k];
					for (int l = 0; l < mB_DynamicGameObject.indexesOfBonesUsed.Length; l++)
					{
						mB_DynamicGameObject.indexesOfBonesUsed[l] = array2[mB_DynamicGameObject.indexesOfBonesUsed[l]];
					}
				}
			}
			else
			{
				Array.Copy(oldBonesPreviousBake, nbones, oldBonesPreviousBake.Length);
				Array.Copy(oldBindPosesPreviousBake, nbindPoses, oldBindPosesPreviousBake.Length);
			}
		}

		public void InsertNewBonesIntoBonesArray()
		{
			if (combiner.settings.renderType != MB_RenderType.skinnedMeshRenderer)
			{
				return;
			}
			boneWeights = nboneWeights;
			combiner.bindPoses = nbindPoses;
			combiner.bones = nbones;
			int num = 0;
			foreach (BoneAndBindpose item in GetBonesToAdd())
			{
				int num2 = _newBonesStartAtIdx + num;
				nbones[num2] = item.bone;
				nbindPoses[num2] = item.bindPose;
				num++;
			}
		}

		public void AddBonesToNewBonesArrayAndAdjustBWIndexes1(MB_DynamicGameObject dgo, int vertsIdx)
		{
			Transform[] tmpSMR_CachedBones = dgo._tmpSMR_CachedBones;
			List<Matrix4x4> tmpSMR_CachedBindposes = dgo._tmpSMR_CachedBindposes;
			BoneWeight[] tmpSMR_CachedBoneWeights = dgo._tmpSMR_CachedBoneWeights;
			int[] array = new int[tmpSMR_CachedBones.Length];
			for (int i = 0; i < dgo._tmpSMR_srcMeshBoneIdx2masterListBoneIdx.Length; i++)
			{
				for (int j = 0; j < nbones.Length; j++)
				{
					if (tmpSMR_CachedBones[i] == nbones[j] && tmpSMR_CachedBindposes[i] == nbindPoses[j])
					{
						array[i] = j;
						break;
					}
				}
			}
			for (int k = 0; k < tmpSMR_CachedBoneWeights.Length; k++)
			{
				int num = vertsIdx + k;
				nboneWeights[num].boneIndex0 = array[tmpSMR_CachedBoneWeights[k].boneIndex0];
				nboneWeights[num].boneIndex1 = array[tmpSMR_CachedBoneWeights[k].boneIndex1];
				nboneWeights[num].boneIndex2 = array[tmpSMR_CachedBoneWeights[k].boneIndex2];
				nboneWeights[num].boneIndex3 = array[tmpSMR_CachedBoneWeights[k].boneIndex3];
				nboneWeights[num].weight0 = tmpSMR_CachedBoneWeights[k].weight0;
				nboneWeights[num].weight1 = tmpSMR_CachedBoneWeights[k].weight1;
				nboneWeights[num].weight2 = tmpSMR_CachedBoneWeights[k].weight2;
				nboneWeights[num].weight3 = tmpSMR_CachedBoneWeights[k].weight3;
			}
			for (int l = 0; l < dgo._tmpSMR_srcMeshBoneIdx2masterListBoneIdx.Length; l++)
			{
				dgo._tmpSMR_srcMeshBoneIdx2masterListBoneIdx[l] = array[dgo._tmpSMR_srcMeshBoneIdx2masterListBoneIdx[l]];
			}
			dgo.indexesOfBonesUsed = dgo._tmpSMR_srcMeshBoneIdx2masterListBoneIdx;
			dgo._tmpSMR_srcMeshBoneIdx2masterListBoneIdx = null;
			dgo._tmpSMR_CachedBones = null;
			dgo._tmpSMR_CachedBindposes = null;
			dgo._tmpSMR_CachedBoneWeights = null;
		}

		public void UpdateGameObjects_UpdateBWIndexes(MB_DynamicGameObject dgo)
		{
			Transform[] bones = MBVersion.GetBones(dgo._renderer, dgo.isSkinnedMeshWithBones);
			BoneWeight[] array = ((MeshChannelsCache)combiner._meshChannelsCache).GetBoneWeights(dgo._renderer, dgo.numVerts, dgo.isSkinnedMeshWithBones);
			int num = dgo.vertIdx;
			bool flag = false;
			for (int i = 0; i < array.Length; i++)
			{
				if (bones[array[i].boneIndex0] != oldBonesPreviousBake[boneWeights[num].boneIndex0])
				{
					flag = true;
					break;
				}
				boneWeights[num].weight0 = array[i].weight0;
				boneWeights[num].weight1 = array[i].weight1;
				boneWeights[num].weight2 = array[i].weight2;
				boneWeights[num].weight3 = array[i].weight3;
				num++;
			}
			if (flag)
			{
				UnityEngine.Debug.LogError("Detected that some of the boneweights reference different bones than when initial added. Boneweights must reference the same bones " + dgo.name);
			}
		}

		public void CopyVertsNormsTansToBuffers(MB_DynamicGameObject dgo, MB_IMeshBakerSettings settings, int vertsIdx, NativeSlice<Vector3> nnorms, NativeSlice<Vector4> ntangs, NativeSlice<Vector3> nverts, NativeSlice<Vector3> normals, NativeSlice<Vector4> tangents, NativeSlice<Vector3> verts)
		{
			UnityEngine.Debug.LogError("The simple bone processor doesn't use this.");
		}

		public void CopyVertsNormsTansToBuffers(MB_DynamicGameObject dgo, MB_IMeshBakerSettings settings, int vertsIdx, Vector3[] nnorms, Vector4[] ntangs, Vector3[] nverts, Vector3[] normals, Vector4[] tangents, Vector3[] verts)
		{
			bool flag = dgo._renderer is MeshRenderer;
			if (settings.smrNoExtraBonesWhenCombiningMeshRenderers && flag && dgo._tmpSMR_CachedBones[0] != dgo.gameObject.transform)
			{
				Matrix4x4 matrix4x = dgo._tmpSMR_CachedBindposes[0].inverse * dgo._tmpSMR_CachedBones[0].worldToLocalMatrix * dgo.gameObject.transform.localToWorldMatrix;
				Matrix4x4 matrix4x2 = matrix4x;
				float num2 = (matrix4x2[2, 3] = 0f);
				float value = (matrix4x2[1, 3] = num2);
				matrix4x2[0, 3] = value;
				matrix4x2 = matrix4x2.inverse.transpose;
				for (int i = 0; i < dgo._mesh.vertexCount; i++)
				{
					int num4 = vertsIdx + i;
					if (verts != null)
					{
						verts[vertsIdx + i] = matrix4x.MultiplyPoint3x4(nverts[i]);
					}
					if (settings.doNorm && nnorms != null)
					{
						normals[num4] = matrix4x2.MultiplyPoint3x4(nnorms[i]).normalized;
					}
					if (settings.doTan && ntangs != null)
					{
						float w = ntangs[i].w;
						tangents[num4] = matrix4x2.MultiplyPoint3x4(ntangs[i]).normalized;
						tangents[num4].w = w;
					}
				}
			}
			else
			{
				if (settings.doNorm)
				{
					nnorms?.CopyTo(normals, vertsIdx);
				}
				if (settings.doTan)
				{
					ntangs?.CopyTo(tangents, vertsIdx);
				}
				if (verts != null)
				{
					nverts.CopyTo(verts, vertsIdx);
				}
			}
		}

		public void DisposeOfTemporarySMRData()
		{
			if (boneIdxsToDelete != null)
			{
				boneIdxsToDelete.Clear();
			}
			if (boneAndBindPose2idx != null)
			{
				boneAndBindPose2idx.Clear();
			}
			boneIdxsToDelete = null;
			boneAndBindPose2idx = null;
			boneIdx2dgoMap = null;
		}

		public void CopyBoneWeightsFromMeshForDGOsInCombined(MB_DynamicGameObject dgo, int targVidx)
		{
			Array.Copy(boneWeights, dgo.vertIdx, nboneWeights, targVidx, dgo.numVerts);
		}

		public void ApplySMRdataToMeshToBuffer()
		{
		}

		public void ApplySMRdataToMesh(MB3_MeshCombinerSingle combiner, Mesh mesh)
		{
			mesh.bindposes = combiner.bindPoses;
			mesh.boneWeights = boneWeights;
		}

		public bool GetCachedSMRMeshData(MB_DynamicGameObject dgo)
		{
			return true;
		}

		public bool DB_CheckIntegrity()
		{
			if (combiner.settings.renderType == MB_RenderType.skinnedMeshRenderer)
			{
				for (int i = 0; i < combiner.mbDynamicObjectsInCombinedMesh.Count; i++)
				{
					MB_DynamicGameObject mB_DynamicGameObject = combiner.mbDynamicObjectsInCombinedMesh[i];
					HashSet<int> hashSet = new HashSet<int>();
					HashSet<int> hashSet2 = new HashSet<int>();
					for (int j = mB_DynamicGameObject.vertIdx; j < mB_DynamicGameObject.vertIdx + mB_DynamicGameObject.numVerts; j++)
					{
						hashSet.Add(boneWeights[j].boneIndex0);
						hashSet.Add(boneWeights[j].boneIndex1);
						hashSet.Add(boneWeights[j].boneIndex2);
						hashSet.Add(boneWeights[j].boneIndex3);
					}
					for (int k = 0; k < mB_DynamicGameObject.indexesOfBonesUsed.Length; k++)
					{
						hashSet2.Add(mB_DynamicGameObject.indexesOfBonesUsed[k]);
					}
					hashSet2.ExceptWith(hashSet);
					if (hashSet2.Count > 0)
					{
						UnityEngine.Debug.LogError("The bone indexes were not the same. " + hashSet.Count + " " + hashSet2.Count);
					}
					for (int l = 0; l < mB_DynamicGameObject.indexesOfBonesUsed.Length; l++)
					{
						if (l < 0 || l > oldBonesPreviousBake.Length)
						{
							UnityEngine.Debug.LogError("Bone index was out of bounds.");
						}
					}
					if (mB_DynamicGameObject.indexesOfBonesUsed.Length < 1)
					{
						UnityEngine.Debug.Log("DGO had no bones");
					}
				}
			}
			return true;
		}
	}

	public class MB_MeshCombinerSingle_BoneProcessorNewAPI : MB_IMeshCombinerSingle_BoneProcessor, IDisposable
	{
		private MB2_LogLevel LOG_LEVEL;

		private bool _initialized;

		private bool _disposed;

		private MB3_MeshCombinerSingle combiner;

		private HashSet<BoneAndBindpose> bonesToAddAndInCombined = new HashSet<BoneAndBindpose>();

		private List<BoneAndBindpose> masterList = new List<BoneAndBindpose>();

		private Matrix4x4[] nBindPoses;

		private Transform[] nbones;

		private int boneWeightSize;

		private int targBoneWeightIdx;

		private Dictionary<MB_DynamicGameObject, int> dgo2firstIdxInBoneWeightsArray = new Dictionary<MB_DynamicGameObject, int>();

		private NativeArray<byte> bonesPerVertex_nvarr;

		private NativeArray<BoneWeight1> boneWeight1s_nvarr;

		public MB_MeshCombinerSingle_BoneProcessorNewAPI(MB3_MeshCombinerSingle cm)
		{
			targBoneWeightIdx = 0;
			boneWeightSize = 0;
			combiner = cm;
			LOG_LEVEL = cm.LOG_LEVEL;
		}

		public int GetNewBonesSize()
		{
			return masterList.Count;
		}

		public void BuildBoneIdx2DGOMapIfNecessary(int[] _goToDelete)
		{
			_initialized = false;
			masterList.Clear();
			if (combiner.settings.renderType == MB_RenderType.skinnedMeshRenderer)
			{
				_initialized = true;
			}
		}

		public void RemoveBonesForDgosWeAreDeleting(MB_DynamicGameObject dgo)
		{
		}

		public bool GetCachedSMRMeshData(MB_DynamicGameObject dgo)
		{
			bool result = true;
			Renderer renderer = dgo._renderer;
			MeshChannelsCache_NativeArray meshChannelsCache_NativeArray = (MeshChannelsCache_NativeArray)combiner._meshChannelsCache;
			dgo._tmpSMR_CachedBindposes = meshChannelsCache_NativeArray.GetBindposes(renderer, out dgo.isSkinnedMeshWithBones);
			int count = dgo._tmpSMR_CachedBindposes.Count;
			dgo._tmpSMR_CachedBoneWeightData = meshChannelsCache_NativeArray.GetBoneWeightData(renderer, count, dgo.isSkinnedMeshWithBones);
			dgo.numBoneWeights = dgo._tmpSMR_CachedBoneWeightData.boneWeights.Length;
			Transform[] array = (dgo._tmpSMR_CachedBones = combiner._getBones(renderer, dgo.isSkinnedMeshWithBones));
			if (array.Length > count)
			{
				Array.Resize(ref dgo._tmpSMR_CachedBones, count);
				array = dgo._tmpSMR_CachedBones;
			}
			if (array.Length < count)
			{
				UnityEngine.Debug.LogWarning(dgo.name + " SkinnedMeshRenderer had fewer bones than mesh had bindposes. Mesh may not deform properly: " + array.Length + "  " + count);
			}
			dgo._tmpSMR_CachedBoneAndBindPose = new BoneAndBindpose[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == null)
				{
					UnityEngine.Debug.LogError("Source mesh r had a 'null' bone. Bones must not be null: " + renderer);
					result = false;
				}
			}
			if (combiner.settings.smrNoExtraBonesWhenCombiningMeshRenderers)
			{
				for (int j = 0; j < array.Length; j++)
				{
					BoneAndBindpose item = new BoneAndBindpose(array[j], dgo._tmpSMR_CachedBindposes[j]);
					bonesToAddAndInCombined.Add(item);
				}
			}
			if (LOG_LEVEL >= MB2_LogLevel.trace)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("GetCachedSMRMeshData for : " + dgo.name);
				stringBuilder.AppendLine("   _tmpSMR_CachedBindposes: " + dgo._tmpSMR_CachedBindposes.Count);
				stringBuilder.AppendLine("   _tmpSMR_CachedBoneAndBindPose: " + dgo._tmpSMR_CachedBoneAndBindPose.Length);
				stringBuilder.AppendLine("   _tmpSMR_CachedBones: " + dgo._tmpSMR_CachedBones.Length);
				stringBuilder.AppendLine("   _tmpSMR_CachedBoneWeightData: " + dgo._tmpSMR_CachedBoneWeightData.boneWeights.Length);
				UnityEngine.Debug.Log(stringBuilder.ToString());
			}
			return result;
		}

		public void AllocateAndSetupSMRDataStructures(List<MB_DynamicGameObject> dgosToAdd, List<MB_DynamicGameObject> dgosInCombinedMesh, int newVertSize, IVertexAndTriangleProcessor vertexAndTriangleProcessor)
		{
			if (combiner.settings.renderType == MB_RenderType.skinnedMeshRenderer)
			{
				MeshChannelsCache_NativeArray meshChannelsCache = (MeshChannelsCache_NativeArray)combiner._meshChannelsCache;
				_CollectSkinningDataForDGOsInCombinedMesh(dgosToAdd, dgosInCombinedMesh, meshChannelsCache);
				_BuildMasterBonesArray(dgosToAdd, dgosInCombinedMesh);
				_AllocateNewArraysForCombinedMesh(newVertSize, vertexAndTriangleProcessor);
			}
		}

		public void UpdateGameObjects_ReadBoneWeightInfoFromCombinedMesh()
		{
			if (combiner.settings.renderType != MB_RenderType.skinnedMeshRenderer)
			{
				return;
			}
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				UnityEngine.Debug.Log("UpdateGameObjects_ReadBoneWeightInfoFromCombinedMesh");
			}
			NativeArray<BoneWeight1> allBoneWeights = combiner._mesh.GetAllBoneWeights();
			NativeArray<byte> bonesPerVertex = combiner._mesh.GetBonesPerVertex();
			boneWeight1s_nvarr = new NativeArray<BoneWeight1>(allBoneWeights, Allocator.Persistent);
			bonesPerVertex_nvarr = new NativeArray<byte>(bonesPerVertex, Allocator.Persistent);
			dgo2firstIdxInBoneWeightsArray.Clear();
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			MB_DynamicGameObject mB_DynamicGameObject = combiner.mbDynamicObjectsInCombinedMesh[num3];
			dgo2firstIdxInBoneWeightsArray[mB_DynamicGameObject] = 0;
			for (int i = 0; i < combiner._mesh.vertexCount; i++)
			{
				if (num2 >= mB_DynamicGameObject.numVerts)
				{
					num3++;
					mB_DynamicGameObject = combiner.mbDynamicObjectsInCombinedMesh[num3];
					dgo2firstIdxInBoneWeightsArray[mB_DynamicGameObject] = num;
					if (num3 == combiner.mbDynamicObjectsInCombinedMesh.Count - 1)
					{
						break;
					}
					num2 = 0;
				}
				num += bonesPerVertex_nvarr[i];
				num2++;
			}
		}

		public void CopyBoneWeightsFromMeshForDGOsInCombined(MB_DynamicGameObject dgo, int targVidx)
		{
			AddBonesToNewBonesArrayAndAdjustBWIndexes1(dgo, targVidx);
		}

		public void AddBonesToNewBonesArrayAndAdjustBWIndexes1(MB_DynamicGameObject dgo, int firstVertexIdxForThisDGO)
		{
			int[] tmpSMR_srcMeshBoneIdx2masterListBoneIdx = dgo._tmpSMR_srcMeshBoneIdx2masterListBoneIdx;
			int num = 0;
			for (int i = 0; i < dgo.numVerts; i++)
			{
				byte b = dgo._tmpSMR_CachedBoneWeightData.bonesPerVertex[i];
				bonesPerVertex_nvarr[firstVertexIdxForThisDGO + i] = b;
				for (int j = 0; j < b; j++)
				{
					BoneWeight1 value = dgo._tmpSMR_CachedBoneWeightData.boneWeights[num];
					value.boneIndex = tmpSMR_srcMeshBoneIdx2masterListBoneIdx[value.boneIndex];
					boneWeight1s_nvarr[targBoneWeightIdx + num] = value;
					num++;
				}
			}
			for (int k = 0; k < dgo._tmpSMR_srcMeshBoneIdx2masterListBoneIdx.Length; k++)
			{
				int num2 = dgo._tmpSMR_srcMeshBoneIdx2masterListBoneIdx[k];
				dgo._tmpSMR_srcMeshBoneIdx2masterListBoneIdx[k] = num2;
			}
			dgo.indexesOfBonesUsed = dgo._tmpSMR_srcMeshBoneIdx2masterListBoneIdx;
			targBoneWeightIdx += dgo.numBoneWeights;
			if (LOG_LEVEL >= MB2_LogLevel.trace)
			{
				UnityEngine.Debug.Log("AddBonesToNewBonesArrayAndAdjustBWIndexes1  " + dgo.name + "  remapped indexes for " + dgo._tmpSMR_CachedBoneWeightData.boneWeights.Length + "  boneweigts.");
			}
			dgo._tmpSMR_srcMeshBoneIdx2masterListBoneIdx = null;
			dgo._tmpSMR_CachedBones = null;
			dgo._tmpSMR_CachedBindposes = null;
			dgo._tmpSMR_CachedBoneWeights = null;
			dgo._tmpSMR_CachedBoneAndBindPose = null;
		}

		public void CopyBonesWeAreKeepingToNewBonesArrayAndAdjustBWIndexes(int totalDeleteVerts)
		{
		}

		public void CopyVertsNormsTansToBuffers(MB_DynamicGameObject dgo, MB_IMeshBakerSettings settings, int vertsIdx, Vector3[] nnorms, Vector4[] ntangs, Vector3[] nverts, Vector3[] normals, Vector4[] tangents, Vector3[] verts)
		{
			UnityEngine.Debug.LogError("TODO should call the non-native array version of this");
		}

		public void CopyVertsNormsTansToBuffers(MB_DynamicGameObject dgo, MB_IMeshBakerSettings settings, int vertsIdx, NativeSlice<Vector3> nnorms, NativeSlice<Vector4> ntangs, NativeSlice<Vector3> nverts, NativeSlice<Vector3> normals, NativeSlice<Vector4> tangents, NativeSlice<Vector3> verts)
		{
			bool flag = dgo._renderer is MeshRenderer;
			if (settings.smrNoExtraBonesWhenCombiningMeshRenderers && flag && dgo._tmpSMR_CachedBones[0] != dgo.gameObject.transform)
			{
				Matrix4x4 matrix4x = dgo._tmpSMR_CachedBindposes[0].inverse * dgo._tmpSMR_CachedBones[0].worldToLocalMatrix * dgo.gameObject.transform.localToWorldMatrix;
				Matrix4x4 matrix4x2 = matrix4x;
				float num2 = (matrix4x2[2, 3] = 0f);
				float value = (matrix4x2[1, 3] = num2);
				matrix4x2[0, 3] = value;
				matrix4x2 = matrix4x2.inverse.transpose;
				for (int i = 0; i < dgo._mesh.vertexCount; i++)
				{
					int index = vertsIdx + i;
					verts[vertsIdx + i] = matrix4x.MultiplyPoint3x4(nverts[i]);
					if (settings.doNorm)
					{
						normals[index] = matrix4x2.MultiplyPoint3x4(nnorms[i]).normalized;
					}
					if (settings.doTan)
					{
						float w = ntangs[i].w;
						Vector4 value2 = matrix4x2.MultiplyPoint3x4(ntangs[i]).normalized;
						value2.w = w;
						tangents[index] = value2;
					}
				}
			}
			else
			{
				if (settings.doNorm)
				{
					MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopyTo(nnorms, normals, vertsIdx);
				}
				if (settings.doTan)
				{
					MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopyTo(ntangs, tangents, vertsIdx);
				}
				MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopyTo(nverts, verts, vertsIdx);
			}
		}

		public void InsertNewBonesIntoBonesArray()
		{
			if (combiner.settings.renderType == MB_RenderType.skinnedMeshRenderer)
			{
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					UnityEngine.Debug.Log("InsertNewBonesIntoBonesArray ");
				}
				combiner.bindPoses = nBindPoses;
				combiner.bones = nbones;
				return;
			}
			if (combiner.bindPoses == null || combiner.bindPoses.Length != 0)
			{
				combiner.bindPoses = new Matrix4x4[0];
			}
			if (combiner.bones == null || combiner.bones.Length != 0)
			{
				combiner.bones = new Transform[0];
			}
		}

		public void ApplySMRdataToMeshToBuffer()
		{
			if (LOG_LEVEL >= MB2_LogLevel.trace)
			{
				UnityEngine.Debug.Log("ApplySMRdataToMeshToBuffer ");
			}
		}

		public void ApplySMRdataToMesh(MB3_MeshCombinerSingle combiner, Mesh mesh)
		{
			mesh.bindposes = nBindPoses;
			mesh.SetBoneWeights(bonesPerVertex_nvarr, boneWeight1s_nvarr);
			nBindPoses = null;
			nbones = null;
			bonesPerVertex_nvarr.Dispose();
			boneWeight1s_nvarr.Dispose();
		}

		public void UpdateGameObjects_UpdateBWIndexes(MB_DynamicGameObject dgo)
		{
			NativeArray<BoneWeight1> boneWeights = dgo._tmpSMR_CachedBoneWeightData.boneWeights;
			bool flag = false;
			int num = dgo2firstIdxInBoneWeightsArray[dgo];
			for (int i = 0; i < boneWeights.Length; i++)
			{
				BoneWeight1 value = boneWeights[i];
				value.boneIndex = dgo.indexesOfBonesUsed[value.boneIndex];
				boneWeight1s_nvarr[num] = value;
				num++;
			}
			if (flag)
			{
				UnityEngine.Debug.LogError("Detected that some of the boneweights reference different bones than when initial added. Boneweights must reference the same bones " + dgo.name);
			}
		}

		protected void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}
			if (disposing)
			{
				if (boneWeight1s_nvarr.IsCreated)
				{
					boneWeight1s_nvarr.Dispose();
				}
				if (bonesPerVertex_nvarr.IsCreated)
				{
					bonesPerVertex_nvarr.Dispose();
				}
			}
			_disposed = true;
		}

		public void Dispose()
		{
			Dispose(disposing: true);
		}

		public void DisposeOfTemporarySMRData()
		{
			if (bonesToAddAndInCombined != null)
			{
				bonesToAddAndInCombined.Clear();
			}
			if (masterList != null)
			{
				masterList.Clear();
			}
			if (dgo2firstIdxInBoneWeightsArray != null)
			{
				dgo2firstIdxInBoneWeightsArray.Clear();
			}
			for (int i = 0; i < combiner.mbDynamicObjectsInCombinedMesh.Count; i++)
			{
				MB_DynamicGameObject mB_DynamicGameObject = combiner.mbDynamicObjectsInCombinedMesh[i];
				mB_DynamicGameObject._tmpSMR_srcMeshBoneIdx2masterListBoneIdx = null;
				mB_DynamicGameObject._tmpSMR_CachedBindposes = null;
				mB_DynamicGameObject._tmpSMR_CachedBoneAndBindPose = null;
				mB_DynamicGameObject._tmpSMR_CachedBones = null;
				mB_DynamicGameObject._tmpSMR_CachedBoneWeightData.Dispose();
				mB_DynamicGameObject._tmpSMR_CachedBoneWeights = null;
			}
		}

		internal void _AllocateNewArraysForCombinedMesh(int newVertSize, IVertexAndTriangleProcessor vertexAndTriangleProcessor)
		{
			if (boneWeight1s_nvarr.IsCreated)
			{
				boneWeight1s_nvarr.Dispose();
			}
			if (bonesPerVertex_nvarr.IsCreated)
			{
				bonesPerVertex_nvarr.Dispose();
			}
			boneWeight1s_nvarr = new NativeArray<BoneWeight1>(boneWeightSize, Allocator.Persistent);
			bonesPerVertex_nvarr = new NativeArray<byte>(newVertSize, Allocator.Persistent);
			nBindPoses = new Matrix4x4[masterList.Count];
			nbones = new Transform[masterList.Count];
			for (int i = 0; i < masterList.Count; i++)
			{
				nBindPoses[i] = masterList[i].bindPose;
				nbones[i] = masterList[i].bone;
			}
			if (LOG_LEVEL >= MB2_LogLevel.trace)
			{
				UnityEngine.Debug.Log("  _AllocateNewArraysForCombinedMesh boneWeight1s_nvarr:" + boneWeight1s_nvarr.Length + " bonesPerVertex_nvarr:" + bonesPerVertex_nvarr.Length + "  numBones: " + masterList.Count);
			}
			targBoneWeightIdx = 0;
		}

		private bool _CollectBonesToAddForDGO_Pass2(MB_DynamicGameObject dgo, bool noExtraBonesForMeshRenderers)
		{
			bool result = true;
			List<Matrix4x4> tmpSMR_CachedBindposes = dgo._tmpSMR_CachedBindposes;
			Transform[] tmpSMR_CachedBones = dgo._tmpSMR_CachedBones;
			if (noExtraBonesForMeshRenderers && dgo._renderer is MeshRenderer)
			{
				bool flag = false;
				BoneAndBindpose boneAndBindpose = default(BoneAndBindpose);
				Transform parent = dgo.gameObject.transform.parent;
				while (parent != null)
				{
					foreach (BoneAndBindpose item in bonesToAddAndInCombined)
					{
						if (item.bone == parent)
						{
							boneAndBindpose = item;
							flag = true;
							break;
						}
					}
					if (flag)
					{
						break;
					}
					parent = parent.parent;
				}
				if (flag)
				{
					tmpSMR_CachedBones[0] = boneAndBindpose.bone;
					tmpSMR_CachedBindposes[0] = boneAndBindpose.bindPose;
				}
			}
			for (int i = 0; i < tmpSMR_CachedBones.Length; i++)
			{
				if (dgo._tmpSMR_CachedBoneWeightData.UsedBoneIdxsInSrcMesh[i])
				{
					BoneAndBindpose boneAndBindpose2 = new BoneAndBindpose(tmpSMR_CachedBones[i], tmpSMR_CachedBindposes[i]);
					dgo._tmpSMR_CachedBoneAndBindPose[i] = boneAndBindpose2;
				}
			}
			return result;
		}

		private int _BuildMasterBonesArray(List<MB_DynamicGameObject> dgosToAdd, List<MB_DynamicGameObject> dgosInCombinedMesh)
		{
			boneWeightSize = 0;
			Dictionary<BoneAndBindpose, int> dictionary = new Dictionary<BoneAndBindpose, int>();
			masterList.Clear();
			StringBuilder stringBuilder = null;
			if (LOG_LEVEL >= MB2_LogLevel.trace)
			{
				stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("_BuildMasterBonesArray");
			}
			for (int i = 0; i < dgosInCombinedMesh.Count; i++)
			{
				if (dgosInCombinedMesh[i]._beingDeleted)
				{
					continue;
				}
				MB_DynamicGameObject mB_DynamicGameObject = dgosInCombinedMesh[i];
				boneWeightSize += mB_DynamicGameObject.numBoneWeights;
				int num = mB_DynamicGameObject._tmpSMR_CachedBoneAndBindPose.Length;
				int[] array = new int[num];
				int num2 = 0;
				for (int j = 0; j < num; j++)
				{
					if (mB_DynamicGameObject._tmpSMR_CachedBoneWeightData.UsedBoneIdxsInSrcMesh[j])
					{
						BoneAndBindpose boneAndBindpose = mB_DynamicGameObject._tmpSMR_CachedBoneAndBindPose[j];
						if (!dictionary.TryGetValue(boneAndBindpose, out var value))
						{
							dictionary.Add(boneAndBindpose, masterList.Count);
							value = masterList.Count;
							num2++;
							masterList.Add(boneAndBindpose);
						}
						array[j] = value;
					}
				}
				mB_DynamicGameObject._tmpSMR_srcMeshBoneIdx2masterListBoneIdx = array;
				if (LOG_LEVEL >= MB2_LogLevel.trace)
				{
					stringBuilder.AppendLine(mB_DynamicGameObject.name + "  addedToMasterList: " + num2 + "    srcMeshBoneIdx2masterListBoneIdx: " + array.Length);
				}
			}
			for (int k = 0; k < dgosToAdd.Count; k++)
			{
				MB_DynamicGameObject mB_DynamicGameObject2 = dgosToAdd[k];
				boneWeightSize += mB_DynamicGameObject2.numBoneWeights;
				int num3 = mB_DynamicGameObject2._tmpSMR_CachedBoneAndBindPose.Length;
				int[] array2 = new int[num3];
				for (int l = 0; l < num3; l++)
				{
					if (mB_DynamicGameObject2._tmpSMR_CachedBoneWeightData.UsedBoneIdxsInSrcMesh[l])
					{
						BoneAndBindpose boneAndBindpose2 = mB_DynamicGameObject2._tmpSMR_CachedBoneAndBindPose[l];
						if (!dictionary.TryGetValue(boneAndBindpose2, out var value2))
						{
							dictionary.Add(boneAndBindpose2, masterList.Count);
							value2 = masterList.Count;
							masterList.Add(boneAndBindpose2);
						}
						array2[l] = value2;
					}
				}
				mB_DynamicGameObject2._tmpSMR_srcMeshBoneIdx2masterListBoneIdx = array2;
				if (LOG_LEVEL >= MB2_LogLevel.trace)
				{
					stringBuilder.AppendLine(mB_DynamicGameObject2.name + "    srcMeshBoneIdx2masterListBoneIdx: " + array2.Length);
				}
			}
			if (LOG_LEVEL >= MB2_LogLevel.trace)
			{
				stringBuilder.AppendLine("Master List Length: " + masterList.Count);
				UnityEngine.Debug.Log(stringBuilder);
			}
			return masterList.Count;
		}

		internal void _CollectSkinningDataForDGOsInCombinedMesh(List<MB_DynamicGameObject> dgosAdding, List<MB_DynamicGameObject> dgosInCombinedMesh, MeshChannelsCache_NativeArray meshChannelsCache)
		{
			for (int i = 0; i < dgosAdding.Count; i++)
			{
				MB_DynamicGameObject dgo = dgosAdding[i];
				_CollectBonesToAddForDGO_Pass2(dgo, combiner.settings.smrNoExtraBonesWhenCombiningMeshRenderers);
			}
			int num = 0;
			for (int j = 0; j < dgosInCombinedMesh.Count; j++)
			{
				MB_DynamicGameObject mB_DynamicGameObject = dgosInCombinedMesh[j];
				if (!mB_DynamicGameObject._beingDeleted)
				{
					num++;
					_CollectBonesToAddForDGO_Pass2(mB_DynamicGameObject, combiner.settings.smrNoExtraBonesWhenCombiningMeshRenderers);
				}
			}
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				UnityEngine.Debug.Log("_CollectSkinningDataForDGOsInCombinedMesh: dgosAdding:" + dgosAdding.Count + " dgosInCombined:" + num);
			}
		}

		public bool DB_CheckIntegrity()
		{
			return true;
		}
	}

	public enum MeshCreationConditions
	{
		NoMesh,
		CreatedInEditor,
		CreatedAtRuntime,
		AssignedByUser
	}

	[Serializable]
	public struct BufferDataFromPreviousBake
	{
		public int numVertsBaked;

		public Vector3 meshVerticesShift;

		public bool meshVerticiesWereShifted;
	}

	[Serializable]
	public class SerializableIntArray
	{
		[SerializeField]
		public int[] data;

		public SerializableIntArray()
		{
			data = new int[0];
		}

		public SerializableIntArray(int len)
		{
			data = new int[len];
		}
	}

	public struct BoneWeightDataForMesh
	{
		private bool _disposed;

		public bool initialized;

		public bool weMustDispose;

		public NativeArray<byte> bonesPerVertex;

		public NativeArray<BoneWeight1> boneWeights;

		public bool[] UsedBoneIdxsInSrcMesh;

		public int numUsedbones;

		internal void Dispose()
		{
			Dispose(disposing: true);
		}

		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				_disposed = true;
				initialized = false;
				if (bonesPerVertex.IsCreated && weMustDispose)
				{
					bonesPerVertex.Dispose();
				}
				if (boneWeights.IsCreated && weMustDispose)
				{
					boneWeights.Dispose();
				}
			}
		}
	}

	[Serializable]
	public class MB_DynamicGameObject : IComparable<MB_DynamicGameObject>
	{
		public int instanceID;

		public GameObject gameObject;

		public string name;

		public int vertIdx;

		public int blendShapeIdx;

		public int numVerts;

		public int numBlendShapes;

		public int numBoneWeights;

		public bool isSkinnedMeshWithBones;

		public int[] indexesOfBonesUsed = new int[0];

		public int lightmapIndex = -1;

		public Vector4 lightmapTilingOffset = new Vector4(1f, 1f, 0f, 0f);

		public Vector3 meshSize = Vector3.one;

		public bool show = true;

		public bool invertTriangles;

		public int[] submeshTriIdxs;

		public int[] submeshNumTris;

		public int[] targetSubmeshIdxs;

		public Rect[] uvRects;

		public Rect[] encapsulatingRect;

		public Rect[] sourceMaterialTiling;

		public Rect[] obUVRects;

		public int[] textureArraySliceIdx;

		public Material[] sourceSharedMaterials;

		[NonSerialized]
		internal bool _initialized;

		[NonSerialized]
		internal bool _beingDeleted;

		[NonSerialized]
		internal Mesh _mesh;

		[NonSerialized]
		internal Renderer _renderer;

		[NonSerialized]
		internal SerializableIntArray[] _tmpSubmeshTris;

		[NonSerialized]
		internal Transform[] _tmpSMR_CachedBones;

		[NonSerialized]
		internal List<Matrix4x4> _tmpSMR_CachedBindposes;

		[NonSerialized]
		internal BoneAndBindpose[] _tmpSMR_CachedBoneAndBindPose;

		[NonSerialized]
		internal int[] _tmpSMR_srcMeshBoneIdx2masterListBoneIdx;

		[NonSerialized]
		internal BoneWeight[] _tmpSMR_CachedBoneWeights;

		[NonSerialized]
		internal BoneWeightDataForMesh _tmpSMR_CachedBoneWeightData;

		public bool Initialize(bool beingDeleted)
		{
			_initialized = true;
			_beingDeleted = beingDeleted;
			if (!beingDeleted)
			{
				_mesh = MB_Utility.GetMesh(gameObject);
				_renderer = MB_Utility.GetRenderer(gameObject);
				if (_mesh != null)
				{
					return _renderer != null;
				}
				return false;
			}
			return true;
		}

		public bool InitializeNew(bool beingDeleted, GameObject go)
		{
			gameObject = go;
			name = $"{gameObject.ToString()} {gameObject.GetInstanceID()}";
			if (go == null)
			{
				return false;
			}
			instanceID = gameObject.GetInstanceID();
			return Initialize(beingDeleted);
		}

		public void UnInitialize()
		{
			_initialized = false;
			_beingDeleted = false;
			_mesh = null;
			_renderer = null;
		}

		public int CompareTo(MB_DynamicGameObject b)
		{
			return vertIdx - b.vertIdx;
		}
	}

	public class MeshChannels : IDisposable
	{
		private bool _disposed;

		public Vector3[] vertices;

		public Vector3[] normals;

		public Vector4[] tangents;

		public Vector2[] uv0raw;

		public Vector2[] uv0modified;

		public Vector2[] uv2raw;

		public Vector2[] uv2modified;

		public Vector2[] uv3;

		public Vector2[] uv4;

		public Vector2[] uv5;

		public Vector2[] uv6;

		public Vector2[] uv7;

		public Vector2[] uv8;

		public Color[] colors;

		public BoneWeight[] boneWeights;

		public List<Matrix4x4> bindPoses = new List<Matrix4x4>(128);

		public int[] triangles;

		public MBBlendShape[] blendShapes;

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
				_disposed = true;
				vertices = null;
				normals = null;
				tangents = null;
				uv0raw = null;
				uv0modified = null;
				uv2raw = null;
				uv2modified = null;
				uv3 = null;
				uv4 = null;
				uv5 = null;
				uv6 = null;
				uv7 = null;
				uv8 = null;
				colors = null;
				boneWeights = null;
				bindPoses = null;
				triangles = null;
				blendShapes = null;
			}
		}
	}

	[Serializable]
	public class MBBlendShapeFrame
	{
		public float frameWeight;

		public Vector3[] vertices;

		public Vector3[] normals;

		public Vector3[] tangents;
	}

	[Serializable]
	public class MBBlendShape
	{
		public GameObject gameObject;

		public string name;

		public int indexInSource;

		public MBBlendShapeFrame[] frames;
	}

	public struct BoneAndBindpose
	{
		public Transform bone;

		public Matrix4x4 bindPose;

		public BoneAndBindpose(Transform t, Matrix4x4 bp)
		{
			bone = t;
			bindPose = bp;
		}

		public override bool Equals(object obj)
		{
			if (obj is BoneAndBindpose && bone == ((BoneAndBindpose)obj).bone && bindPose == ((BoneAndBindpose)obj).bindPose)
			{
				return true;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (bone.GetInstanceID() % int.MaxValue) ^ (int)bindPose[0, 0];
		}
	}

	public interface IMeshChannelsCacheTaggingInterface
	{
		void Dispose();

		bool HasCollectedMeshData();

		void CollectChannelDataForAllMeshesInList(List<MB_DynamicGameObject> toUpdateDGOs, List<MB_DynamicGameObject> toAddDGOs, MB_MeshVertexChannelFlags newChannels, MB_RenderType renderType, bool doBlendShapes);

		MBBlendShape[] GetBlendShapes(Mesh mesh, int instanceID, GameObject gameObject);

		bool hasOutOfBoundsUVs(Mesh m, ref MB_Utility.MeshAnalysisResult mar, int submeshIdx);
	}

	public class MeshChannelsCache : IDisposable, IMeshChannelsCacheTaggingInterface
	{
		private MB2_LogLevel LOG_LEVEL;

		private MB2_LightmapOptions lightmapOption;

		protected Dictionary<int, MeshChannels> meshID2MeshChannels = new Dictionary<int, MeshChannels>();

		private bool _collectedMeshData;

		private bool _disposed;

		private Vector2 _HALF_UV = new Vector2(0.5f, 0.5f);

		public MeshChannelsCache(MB2_LogLevel ll, MB2_LightmapOptions lo)
		{
			LOG_LEVEL = ll;
			lightmapOption = lo;
		}

		public void Dispose()
		{
			Dispose(disposing: true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}
			foreach (MeshChannels value in meshID2MeshChannels.Values)
			{
				value.Dispose();
			}
			_collectedMeshData = false;
			_disposed = true;
		}

		public bool HasCollectedMeshData()
		{
			return _collectedMeshData;
		}

		public bool hasOutOfBoundsUVs(Mesh m, ref MB_Utility.MeshAnalysisResult mar, int submeshIdx)
		{
			return MB_Utility.hasOutOfBoundsUVs(GetUv0Raw(m), m, ref mar, submeshIdx);
		}

		internal Vector3[] GetVertices(Mesh m)
		{
			if (!meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out var value))
			{
				UnityEngine.Debug.LogError("Could not find mesh in the MeshChannelsCache." + m);
			}
			return value.vertices;
		}

		internal Vector3[] GetNormals(Mesh m)
		{
			meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out var value);
			return value.normals;
		}

		internal Vector4[] GetTangents(Mesh m)
		{
			meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out var value);
			return value.tangents;
		}

		internal Vector2[] GetUv0Raw(Mesh m)
		{
			meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out var value);
			return value.uv0raw;
		}

		internal Vector2[] GetUv0Modified(Mesh m)
		{
			meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out var value);
			return value.uv0modified;
		}

		internal Vector2[] GetUv2Modified(Mesh m)
		{
			meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out var value);
			return value.uv2modified;
		}

		internal Vector2[] GetUVChannel(int channel, Mesh m)
		{
			meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out var value);
			switch (channel)
			{
			case 0:
				return value.uv0raw;
			case 2:
				return value.uv2raw;
			case 3:
				return value.uv3;
			case 4:
				return value.uv4;
			case 5:
				return value.uv5;
			case 6:
				return value.uv6;
			case 7:
				return value.uv7;
			case 8:
				return value.uv8;
			default:
				UnityEngine.Debug.LogError("Error mesh channel " + channel + " not supported");
				return null;
			}
		}

		internal Color[] GetColors(Mesh m)
		{
			meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out var value);
			return value.colors;
		}

		public void CollectChannelDataForAllMeshesInList(List<MB_DynamicGameObject> toUpdateDGOs, List<MB_DynamicGameObject> toAddDGOs, MB_MeshVertexChannelFlags newChannels, MB_RenderType renderType, bool doBlendShapes)
		{
			bool flag = (newChannels & MB_MeshVertexChannelFlags.vertex) == MB_MeshVertexChannelFlags.vertex;
			bool flag2 = (newChannels & MB_MeshVertexChannelFlags.normal) == MB_MeshVertexChannelFlags.normal;
			bool flag3 = (newChannels & MB_MeshVertexChannelFlags.tangent) == MB_MeshVertexChannelFlags.tangent;
			bool flag4 = (newChannels & MB_MeshVertexChannelFlags.uv0) == MB_MeshVertexChannelFlags.uv0;
			bool flag5 = (newChannels & MB_MeshVertexChannelFlags.uv2) == MB_MeshVertexChannelFlags.uv2;
			bool flag6 = (newChannels & MB_MeshVertexChannelFlags.uv3) == MB_MeshVertexChannelFlags.uv3;
			bool flag7 = (newChannels & MB_MeshVertexChannelFlags.uv4) == MB_MeshVertexChannelFlags.uv4;
			bool flag8 = (newChannels & MB_MeshVertexChannelFlags.uv5) == MB_MeshVertexChannelFlags.uv5;
			bool flag9 = (newChannels & MB_MeshVertexChannelFlags.uv6) == MB_MeshVertexChannelFlags.uv6;
			bool flag10 = (newChannels & MB_MeshVertexChannelFlags.uv7) == MB_MeshVertexChannelFlags.uv7;
			bool flag11 = (newChannels & MB_MeshVertexChannelFlags.uv8) == MB_MeshVertexChannelFlags.uv8;
			bool flag12 = (newChannels & MB_MeshVertexChannelFlags.colors) == MB_MeshVertexChannelFlags.colors;
			List<MB_DynamicGameObject> list = new List<MB_DynamicGameObject>();
			list.AddRange(toUpdateDGOs);
			list.AddRange(toAddDGOs);
			for (int i = 0; i < list.Count; i++)
			{
				MB_DynamicGameObject mB_DynamicGameObject = list[i];
				Mesh mesh = mB_DynamicGameObject._mesh;
				if (meshID2MeshChannels.ContainsKey(mesh.GetInstanceID()))
				{
					continue;
				}
				MeshChannels meshChannels = new MeshChannels();
				meshID2MeshChannels.Add(mesh.GetInstanceID(), meshChannels);
				if (flag)
				{
					meshChannels.vertices = mesh.vertices;
				}
				if (flag4)
				{
					meshChannels.uv0raw = _getMeshUVs(mesh);
				}
				if (flag5)
				{
					meshChannels.uv2raw = _getMeshUV2s(mesh, ref meshChannels.uv2modified);
				}
				if (flag2)
				{
					meshChannels.normals = _getMeshNormals(mesh);
				}
				if (flag3)
				{
					meshChannels.tangents = _getMeshTangents(mesh);
				}
				if (flag6)
				{
					meshChannels.uv3 = MBVersion.GetMeshChannel(3, mesh, LOG_LEVEL);
				}
				if (flag7)
				{
					meshChannels.uv4 = MBVersion.GetMeshChannel(4, mesh, LOG_LEVEL);
				}
				if (flag8)
				{
					meshChannels.uv5 = MBVersion.GetMeshChannel(5, mesh, LOG_LEVEL);
				}
				if (flag9)
				{
					meshChannels.uv6 = MBVersion.GetMeshChannel(6, mesh, LOG_LEVEL);
				}
				if (flag10)
				{
					meshChannels.uv7 = MBVersion.GetMeshChannel(7, mesh, LOG_LEVEL);
				}
				if (flag11)
				{
					meshChannels.uv8 = MBVersion.GetMeshChannel(8, mesh, LOG_LEVEL);
				}
				if (flag12)
				{
					meshChannels.colors = _getMeshColors(mesh);
				}
				if (renderType != MB_RenderType.skinnedMeshRenderer)
				{
					continue;
				}
				Renderer renderer = mB_DynamicGameObject._renderer;
				_getBindPoses(renderer, meshChannels.bindPoses, out var isSkinnedMeshWithBones);
				meshChannels.boneWeights = _getBoneWeights(renderer, mesh.vertexCount, isSkinnedMeshWithBones);
				if (!doBlendShapes)
				{
					continue;
				}
				MBBlendShape[] array = new MBBlendShape[mesh.blendShapeCount];
				int vertexCount = mesh.vertexCount;
				for (int j = 0; j < array.Length; j++)
				{
					MBBlendShape mBBlendShape = (array[j] = new MBBlendShape());
					mBBlendShape.frames = new MBBlendShapeFrame[MBVersion.GetBlendShapeFrameCount(mesh, j)];
					mBBlendShape.name = mesh.GetBlendShapeName(j);
					mBBlendShape.indexInSource = j;
					mBBlendShape.gameObject = mB_DynamicGameObject.gameObject;
					for (int k = 0; k < mBBlendShape.frames.Length; k++)
					{
						MBBlendShapeFrame mBBlendShapeFrame = (mBBlendShape.frames[k] = new MBBlendShapeFrame());
						mBBlendShapeFrame.frameWeight = MBVersion.GetBlendShapeFrameWeight(mesh, j, k);
						mBBlendShapeFrame.vertices = new Vector3[vertexCount];
						mBBlendShapeFrame.normals = new Vector3[vertexCount];
						mBBlendShapeFrame.tangents = new Vector3[vertexCount];
						MBVersion.GetBlendShapeFrameVertices(mesh, j, k, mBBlendShapeFrame.vertices, mBBlendShapeFrame.normals, mBBlendShapeFrame.tangents);
					}
				}
				meshChannels.blendShapes = array;
			}
			_collectedMeshData = true;
		}

		internal List<Matrix4x4> GetBindposes(Renderer r, out bool isSkinnedMeshWithBones)
		{
			Mesh mesh = MB_Utility.GetMesh(r.gameObject);
			meshID2MeshChannels.TryGetValue(mesh.GetInstanceID(), out var value);
			if (r is SkinnedMeshRenderer && value.bindPoses.Count > 0)
			{
				isSkinnedMeshWithBones = true;
			}
			else
			{
				isSkinnedMeshWithBones = false;
				_ = r is SkinnedMeshRenderer;
			}
			return value.bindPoses;
		}

		internal BoneWeight[] GetBoneWeights(Renderer r, int numVertsInMeshBeingAdded, bool isSkinnedMeshWithBones)
		{
			Mesh mesh = MB_Utility.GetMesh(r.gameObject);
			meshID2MeshChannels.TryGetValue(mesh.GetInstanceID(), out var value);
			return value.boneWeights;
		}

		public MBBlendShape[] GetBlendShapes(Mesh m, int gameObjectID, GameObject gameObject)
		{
			meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out var value);
			MBBlendShape[] array = new MBBlendShape[value.blendShapes.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new MBBlendShape();
				array[i].name = value.blendShapes[i].name;
				array[i].indexInSource = value.blendShapes[i].indexInSource;
				array[i].frames = value.blendShapes[i].frames;
				array[i].gameObject = gameObject;
			}
			return array;
		}

		private Color[] _getMeshColors(Mesh m)
		{
			Color[] array = m.colors;
			if (array.Length == 0)
			{
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					MB2_Log.LogDebug("Mesh " + m?.ToString() + " has no colors. Generating");
				}
				if (LOG_LEVEL >= MB2_LogLevel.warn)
				{
					UnityEngine.Debug.LogWarning("Mesh " + m?.ToString() + " didn't have colors. Generating an array of white colors");
				}
				array = new Color[m.vertexCount];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = Color.white;
				}
			}
			return array;
		}

		private Vector3[] _getMeshNormals(Mesh m)
		{
			Vector3[] normals = m.normals;
			if (normals.Length == 0)
			{
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					MB2_Log.LogDebug("Mesh " + m?.ToString() + " has no normals. Generating");
				}
				if (LOG_LEVEL >= MB2_LogLevel.warn)
				{
					UnityEngine.Debug.LogWarning("Mesh " + m?.ToString() + " didn't have normals. Generating normals.");
				}
				Mesh mesh = UnityEngine.Object.Instantiate(m);
				mesh.RecalculateNormals();
				normals = mesh.normals;
				MB_Utility.Destroy(mesh);
			}
			return normals;
		}

		private Vector4[] _getMeshTangents(Mesh m)
		{
			Vector4[] array = m.tangents;
			if (array.Length == 0)
			{
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					MB2_Log.LogDebug("Mesh " + m?.ToString() + " has no tangents. Generating");
				}
				if (LOG_LEVEL >= MB2_LogLevel.warn)
				{
					UnityEngine.Debug.LogWarning("Mesh " + m?.ToString() + " didn't have tangents. Generating tangents.");
				}
				Vector3[] vertices = m.vertices;
				Vector2[] uv0Raw = GetUv0Raw(m);
				Vector3[] normals = _getMeshNormals(m);
				array = new Vector4[m.vertexCount];
				for (int i = 0; i < m.subMeshCount; i++)
				{
					int[] triangles = m.GetTriangles(i);
					_generateTangents(triangles, vertices, uv0Raw, normals, array);
				}
			}
			return array;
		}

		private Vector2[] _getMeshUVs(Mesh m)
		{
			Vector2[] array = m.uv;
			if (array.Length == 0)
			{
				array = new Vector2[m.vertexCount];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = _HALF_UV;
				}
			}
			return array;
		}

		private Vector2[] _getMeshUV2s(Mesh m, ref Vector2[] uv2modified)
		{
			Vector2[] uv = m.uv2;
			if (uv.Length == 0)
			{
				uv2modified = new Vector2[m.vertexCount];
				for (int i = 0; i < uv2modified.Length; i++)
				{
					uv2modified[i] = _HALF_UV;
				}
			}
			return uv;
		}

		private static void _getBindPoses(Renderer r, List<Matrix4x4> poses, out bool isSkinnedMeshWithBones)
		{
			poses.Clear();
			isSkinnedMeshWithBones = r is SkinnedMeshRenderer;
			if (r is SkinnedMeshRenderer)
			{
				Mesh mesh = MB_Utility.GetMesh(r.gameObject);
				mesh.GetBindposes(poses);
				if (poses.Count == 0)
				{
					if (mesh.blendShapeCount > 0)
					{
						isSkinnedMeshWithBones = false;
					}
					else
					{
						UnityEngine.Debug.LogError("Skinned mesh " + r?.ToString() + " had no bindposes AND no blend shapes");
					}
				}
			}
			if (r is MeshRenderer || (r is SkinnedMeshRenderer && !isSkinnedMeshWithBones))
			{
				poses.Clear();
				poses.Add(Matrix4x4.identity);
			}
			if (poses == null || poses.Count == 0)
			{
				UnityEngine.Debug.LogError("Could not _getBindPoses. Object does not have a renderer");
			}
		}

		private static BoneWeight[] _getBoneWeights(Renderer r, int numVertsInMeshBeingAdded, bool isSkinnedMeshWithBones)
		{
			if (isSkinnedMeshWithBones)
			{
				return ((SkinnedMeshRenderer)r).sharedMesh.boneWeights;
			}
			if (r is MeshRenderer || (r is SkinnedMeshRenderer && !isSkinnedMeshWithBones))
			{
				BoneWeight boneWeight = default(BoneWeight);
				int num2 = (boneWeight.boneIndex3 = 0);
				int num4 = (boneWeight.boneIndex2 = num2);
				int boneIndex = (boneWeight.boneIndex1 = num4);
				boneWeight.boneIndex0 = boneIndex;
				boneWeight.weight0 = 1f;
				float num7 = (boneWeight.weight3 = 0f);
				float weight = (boneWeight.weight2 = num7);
				boneWeight.weight1 = weight;
				BoneWeight[] array = new BoneWeight[numVertsInMeshBeingAdded];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = boneWeight;
				}
				return array;
			}
			UnityEngine.Debug.LogError("Could not _getBoneWeights. Object does not have a renderer");
			return null;
		}

		private void _generateTangents(int[] triangles, Vector3[] verts, Vector2[] uvs, Vector3[] normals, Vector4[] outTangents)
		{
			int num = triangles.Length;
			int num2 = verts.Length;
			Vector3[] array = new Vector3[num2];
			Vector3[] array2 = new Vector3[num2];
			for (int i = 0; i < num; i += 3)
			{
				int num3 = triangles[i];
				int num4 = triangles[i + 1];
				int num5 = triangles[i + 2];
				Vector3 vector = verts[num3];
				Vector3 vector2 = verts[num4];
				Vector3 vector3 = verts[num5];
				Vector2 vector4 = uvs[num3];
				Vector2 vector5 = uvs[num4];
				Vector2 vector6 = uvs[num5];
				float num6 = vector2.x - vector.x;
				float num7 = vector3.x - vector.x;
				float num8 = vector2.y - vector.y;
				float num9 = vector3.y - vector.y;
				float num10 = vector2.z - vector.z;
				float num11 = vector3.z - vector.z;
				float num12 = vector5.x - vector4.x;
				float num13 = vector6.x - vector4.x;
				float num14 = vector5.y - vector4.y;
				float num15 = vector6.y - vector4.y;
				float num16 = num12 * num15 - num13 * num14;
				if (num16 == 0f)
				{
					UnityEngine.Debug.LogError("Could not compute tangents. All UVs need to form a valid triangles in UV space. If any UV triangles are collapsed, tangents cannot be generated.");
					return;
				}
				float num17 = 1f / num16;
				Vector3 vector7 = new Vector3((num15 * num6 - num14 * num7) * num17, (num15 * num8 - num14 * num9) * num17, (num15 * num10 - num14 * num11) * num17);
				Vector3 vector8 = new Vector3((num12 * num7 - num13 * num6) * num17, (num12 * num9 - num13 * num8) * num17, (num12 * num11 - num13 * num10) * num17);
				array[num3] += vector7;
				array[num4] += vector7;
				array[num5] += vector7;
				array2[num3] += vector8;
				array2[num4] += vector8;
				array2[num5] += vector8;
			}
			for (int j = 0; j < num2; j++)
			{
				Vector3 vector9 = normals[j];
				Vector3 vector10 = array[j];
				Vector3 normalized = (vector10 - vector9 * Vector3.Dot(vector9, vector10)).normalized;
				outTangents[j] = new Vector4(normalized.x, normalized.y, normalized.z);
				outTangents[j].w = ((Vector3.Dot(Vector3.Cross(vector9, vector10), array2[j]) < 0f) ? (-1f) : 1f);
			}
		}
	}

	public interface IVertexAndTriangleProcessor : IDisposable
	{
		MB_MeshVertexChannelFlags channels { get; }

		bool IsInitialized();

		bool IsDisposed();

		void Init(MB3_MeshCombinerSingle combiner, MB_MeshVertexChannelFlags newChannels, int vertexCount, int[] newSubmeshTrisSize, int uvChannelWithExtraParameter, IMeshChannelsCacheTaggingInterface meshChannelsCache, bool loadDataFromCombinedMesh, MB2_LogLevel logLevel);

		void InitShowHide(MB3_MeshCombinerSingle combiner);

		void InitFromMeshCombiner(MB3_MeshCombinerSingle combiner, MB_MeshVertexChannelFlags newChannels, int uvChannelWithExtraParameter);

		int GetVertexCount();

		int GetSubmeshCount();

		void TransferOwnershipOfSerializableBuffersToCombiner(MB3_MeshCombinerSingle c, MB_MeshVertexChannelFlags channelsToTransfer, BufferDataFromPreviousBake serializableBufferData);

		void CopyArraysFromPreviousBakeBuffersToNewBuffers(MB_DynamicGameObject dgo, ref IVertexAndTriangleProcessor iOldBuffers, int destStartVertIdx, int triangleIdxAdjustment, int[] targSubmeshTidx, MB2_LogLevel LOG_LEVEL);

		void CopyFromDGOMeshToBuffers(MB_DynamicGameObject dgo, int destStartVertsIdx, MB_MeshVertexChannelFlags channelsToUpdate, bool updateTris, bool updateBWdata, MB_IMeshBakerSettings settings, MB_IMeshCombinerSingle_BoneProcessor boneProcessor, int[] targSubmeshTidx, MB2_TextureBakeResults textureBakeResults, UVAdjuster_Atlas uvAdjuster, MB2_LogLevel LOG_LEVEL, IMeshChannelsCacheTaggingInterface meshChannelCache);

		void AssignBuffersToMesh(Mesh mesh, MB_IMeshBakerSettings settings, MB2_TextureBakeResults textureBakeResults, MB_MeshVertexChannelFlags channelsToWriteToMesh, bool doWriteTrisToMesh, IAssignToMeshCustomizer assignToMeshCustomizer, List<MB_DynamicGameObject> mbDynamicObjectsInCombinedMesh, out BufferDataFromPreviousBake serializableBufferData, out SerializableIntArray[] submeshTrisToUse, out int numNonZeroLengthSubmeshes);

		void AssignTriangleDataForSubmeshes(Mesh mesh, List<MB_DynamicGameObject> mbDynamicObjectsInCombinedMesh, ref BufferDataFromPreviousBake serializableBufferData, out SerializableIntArray[] submeshTrisToUse, out int numNonZeroLengthSubmeshes);

		void AssignTriangleDataForSubmeshes_ShowHide(Mesh mesh, List<MB_DynamicGameObject> mbDynamicObjectsInCombinedMesh, ref BufferDataFromPreviousBake serializableBufferData, out SerializableIntArray[] submeshTrisToUse, out int numNonZeroLengthSubmeshes);

		void CopyUV2unchangedToSeparateRects(List<MB_DynamicGameObject> mbDynamicObjectsInCombinedMesh, float uv2UnwrappingParamsPackMargin);

		int[] GetTriangleSizes();
	}

	public class MB_MeshCombinerSingle_SubCombiner
	{
		public static void instance2Combined_MapAdd(ref Dictionary<GameObject, MB_DynamicGameObject> _instance2combined_map, GameObject gameObjectID, MB_DynamicGameObject dgo)
		{
			_instance2combined_map.Add(gameObjectID, dgo);
		}

		public static void instance2Combined_MapRemove(ref Dictionary<GameObject, MB_DynamicGameObject> _instance2combined_map, GameObject gameObjectID)
		{
			_instance2combined_map.Remove(gameObjectID);
		}

		internal static bool _ShowHideGameObjects(MB3_MeshCombinerSingle c)
		{
			c._vertexAndTriProcessor.InitShowHide(c);
			return true;
		}

		internal static bool _AddToCombined(MB3_MeshCombinerSingle c, MB_MeshVertexChannelFlags newChannels, int totalAddVerts, int totalDeleteVerts, int numResultMats, int totalAddBlendShapes, int totalDeleteBlendShapes, int[] totalAddSubmeshTris, int[] totalDeleteSubmeshTris, int[] _goToDelete, List<MB_DynamicGameObject> toAddDGOs, GameObject[] _goToAdd, UVAdjuster_Atlas uvAdjuster, ref IVertexAndTriangleProcessor oldMeshData, Stopwatch sw)
		{
			MB_IMeshCombinerSingle_BoneProcessor boneProcessor = c._boneProcessor;
			MB_MeshCombinerSingle_BlendShapeProcessor blendShapeProcessor = c._blendShapeProcessor;
			IMeshChannelsCacheTaggingInterface meshChannelsCache = c._meshChannelsCache;
			MB_IMeshBakerSettings settings = c.settings;
			MB2_LogLevel lOG_LEVEL = c.LOG_LEVEL;
			MB2_TextureBakeResults textureBakeResults = c.textureBakeResults;
			List<MB_DynamicGameObject> mbDynamicObjectsInCombinedMesh = c.mbDynamicObjectsInCombinedMesh;
			List<GameObject> objectsInCombinedMesh = c.objectsInCombinedMesh;
			Dictionary<GameObject, MB_DynamicGameObject> _instance2combined_map = c._instance2combined_map;
			int uvChannelWithExtraParameter = ((c.settings.assignToMeshCustomizer == null || !(c.settings.assignToMeshCustomizer is IAssignToMeshCustomizer_NativeArrays)) ? (-1) : ((IAssignToMeshCustomizer_NativeArrays)c.settings.assignToMeshCustomizer).UVchannelWithExtraParameter());
			c.db_addDeleteGameObjects_InitFromMeshCombiner.Start();
			int num;
			int[] array;
			if (!settings.clearBuffersAfterBake && mbDynamicObjectsInCombinedMesh.Count > 0)
			{
				oldMeshData.InitFromMeshCombiner(c, newChannels, uvChannelWithExtraParameter);
				num = oldMeshData.GetVertexCount();
				array = oldMeshData.GetTriangleSizes();
			}
			else
			{
				num = 0;
				array = new int[numResultMats];
			}
			c.db_addDeleteGameObjects_InitFromMeshCombiner.Stop();
			c.db_addDeleteGameObjects_Init.Start();
			int num2 = num + totalAddVerts - totalDeleteVerts;
			int nBlendShapeSize = 0;
			if (settings.doBlendShapes)
			{
				nBlendShapeSize = c.blendShapes.Length + totalAddBlendShapes - totalDeleteBlendShapes;
			}
			if (lOG_LEVEL >= MB2_LogLevel.debug)
			{
				UnityEngine.Debug.Log("Verts adding:" + totalAddVerts + " deleting:" + totalDeleteVerts + " submeshes:" + numResultMats + " blendShapes:" + nBlendShapeSize);
			}
			int[] array2 = new int[numResultMats];
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i] = array[i] + totalAddSubmeshTris[i] - totalDeleteSubmeshTris[i];
				if (lOG_LEVEL >= MB2_LogLevel.debug)
				{
					MB2_Log.LogDebug("    submesh :" + i + " already contains:" + array[i] + " tris to be Added:" + totalAddSubmeshTris[i] + " tris to be Deleted:" + totalDeleteSubmeshTris[i]);
				}
			}
			if (num2 >= MBVersion.MaxMeshVertexCount())
			{
				UnityEngine.Debug.LogError("Cannot add objects. Resulting mesh will have more than " + MBVersion.MaxMeshVertexCount() + " vertices. Try using a Multi-MeshBaker component. This will split the combined mesh into several meshes. You don't have to re-configure the MB2_TextureBaker. Just remove the MB2_MeshBaker component and add a MB2_MultiMeshBaker component.");
				return false;
			}
			IVertexAndTriangleProcessor vertexAndTriProcessor = c._vertexAndTriProcessor;
			vertexAndTriProcessor.Init(c, newChannels, num2, array2, uvChannelWithExtraParameter, meshChannelsCache, loadDataFromCombinedMesh: false, c.LOG_LEVEL);
			boneProcessor.AllocateAndSetupSMRDataStructures(toAddDGOs, mbDynamicObjectsInCombinedMesh, num2, c._vertexAndTriProcessor);
			blendShapeProcessor.AllocateBlendShapeArrayIfNecessary(nBlendShapeSize);
			c.db_addDeleteGameObjects_Init.Stop();
			if (lOG_LEVEL >= MB2_LogLevel.debug)
			{
				UnityEngine.Debug.Log("Allocating buffers: " + vertexAndTriProcessor.channels.ToString() + "  vertexCount:" + num2);
			}
			mbDynamicObjectsInCombinedMesh.Sort();
			int targBlendShapeIdx = 0;
			int num3 = 0;
			int[] array3 = new int[numResultMats];
			int num4 = 0;
			c.db_addDeleteGameObjects_CopyArraysFromPreviousBakeBuffersToNewBuffers.Start();
			if (!settings.clearBuffersAfterBake && mbDynamicObjectsInCombinedMesh.Count > 0)
			{
				for (int j = 0; j < mbDynamicObjectsInCombinedMesh.Count; j++)
				{
					MB_DynamicGameObject mB_DynamicGameObject = mbDynamicObjectsInCombinedMesh[j];
					if (!mB_DynamicGameObject._beingDeleted)
					{
						if (lOG_LEVEL >= MB2_LogLevel.debug)
						{
							MB2_Log.LogDebug("Copying obj in combined arrays idx:" + j, lOG_LEVEL);
						}
						vertexAndTriProcessor.CopyArraysFromPreviousBakeBuffersToNewBuffers(mB_DynamicGameObject, ref oldMeshData, num3, num4, array3, lOG_LEVEL);
						if (settings.doBlendShapes)
						{
							blendShapeProcessor.CopyBlendShapesInCurrentMeshIfNecessary(ref targBlendShapeIdx, mB_DynamicGameObject);
						}
						if (settings.renderType == MB_RenderType.skinnedMeshRenderer)
						{
							boneProcessor.CopyBoneWeightsFromMeshForDGOsInCombined(mB_DynamicGameObject, num3);
						}
						mB_DynamicGameObject.vertIdx = num3;
						for (int k = 0; k < array3.Length; k++)
						{
							mB_DynamicGameObject.submeshTriIdxs[k] = array3[k];
							array3[k] += mB_DynamicGameObject.submeshNumTris[k];
						}
						num3 += mB_DynamicGameObject.numVerts;
					}
					else
					{
						if (lOG_LEVEL >= MB2_LogLevel.debug)
						{
							MB2_Log.LogDebug("Not copying obj: " + j, lOG_LEVEL);
						}
						num4 += mB_DynamicGameObject.numVerts;
					}
				}
				if (settings.renderType == MB_RenderType.skinnedMeshRenderer)
				{
					boneProcessor.CopyBonesWeAreKeepingToNewBonesArrayAndAdjustBWIndexes(totalDeleteVerts);
				}
				for (int num5 = mbDynamicObjectsInCombinedMesh.Count - 1; num5 >= 0; num5--)
				{
					if (mbDynamicObjectsInCombinedMesh[num5]._beingDeleted)
					{
						instance2Combined_MapRemove(ref _instance2combined_map, mbDynamicObjectsInCombinedMesh[num5].gameObject);
						objectsInCombinedMesh.RemoveAt(num5);
						mbDynamicObjectsInCombinedMesh.RemoveAt(num5);
					}
				}
			}
			c.db_addDeleteGameObjects_CopyArraysFromPreviousBakeBuffersToNewBuffers.Stop();
			c.db_addDeleteGameObjects_CopyFromDGOMeshToBuffers.Start();
			if (settings.renderType == MB_RenderType.skinnedMeshRenderer)
			{
				boneProcessor.InsertNewBonesIntoBonesArray();
			}
			for (int l = 0; l < toAddDGOs.Count; l++)
			{
				MB_DynamicGameObject mB_DynamicGameObject2 = toAddDGOs[l];
				GameObject gameObject = _goToAdd[l];
				int vertsIdx = num3;
				Mesh mesh = mB_DynamicGameObject2._mesh;
				bool updateBWdata = false;
				vertexAndTriProcessor.CopyFromDGOMeshToBuffers(mB_DynamicGameObject2, num3, vertexAndTriProcessor.channels, updateTris: true, updateBWdata, settings, boneProcessor, array3, textureBakeResults, uvAdjuster, lOG_LEVEL, meshChannelsCache);
				int subMeshCount = mesh.subMeshCount;
				if (mB_DynamicGameObject2.uvRects.Length < subMeshCount)
				{
					if (lOG_LEVEL >= MB2_LogLevel.debug)
					{
						MB2_Log.LogDebug("Mesh " + mB_DynamicGameObject2.name + " has more submeshes than materials");
					}
				}
				else if (mB_DynamicGameObject2.uvRects.Length > subMeshCount && lOG_LEVEL >= MB2_LogLevel.warn)
				{
					UnityEngine.Debug.LogWarning("Mesh " + mB_DynamicGameObject2.name + " has fewer submeshes than materials");
				}
				if (settings.doBlendShapes)
				{
					blendShapeProcessor.CopyBlendShapesForNewMeshIfNecessary(ref targBlendShapeIdx, mB_DynamicGameObject2, mesh, meshChannelsCache);
				}
				if (settings.renderType == MB_RenderType.skinnedMeshRenderer)
				{
					boneProcessor.AddBonesToNewBonesArrayAndAdjustBWIndexes1(mB_DynamicGameObject2, vertsIdx);
				}
				mB_DynamicGameObject2.vertIdx = num3;
				instance2Combined_MapAdd(ref _instance2combined_map, gameObject, mB_DynamicGameObject2);
				objectsInCombinedMesh.Add(gameObject);
				mbDynamicObjectsInCombinedMesh.Add(mB_DynamicGameObject2);
				num3 += mB_DynamicGameObject2.numVerts;
				for (int m = 0; m < mB_DynamicGameObject2._tmpSubmeshTris.Length; m++)
				{
					mB_DynamicGameObject2._tmpSubmeshTris[m] = null;
				}
				mB_DynamicGameObject2._tmpSubmeshTris = null;
				if (lOG_LEVEL >= MB2_LogLevel.debug)
				{
					MB2_Log.LogDebug("Added to combined:" + mB_DynamicGameObject2.name + " verts:" + vertexAndTriProcessor.GetVertexCount() + " bindPoses:" + boneProcessor.GetNewBonesSize(), lOG_LEVEL);
				}
			}
			if (settings.lightmapOption == MB2_LightmapOptions.copy_UV2_unchanged_to_separate_rects)
			{
				vertexAndTriProcessor.CopyUV2unchangedToSeparateRects(mbDynamicObjectsInCombinedMesh, settings.uv2UnwrappingParamsPackMargin);
			}
			if (lOG_LEVEL >= MB2_LogLevel.debug)
			{
				MB2_Log.LogDebug("===== _addToCombined completed. Verts in buffer: " + vertexAndTriProcessor.GetVertexCount() + " time(ms): " + sw.ElapsedMilliseconds, lOG_LEVEL);
			}
			c.db_addDeleteGameObjects_CopyFromDGOMeshToBuffers.Stop();
			return true;
		}

		public static bool _UpdateGameObjects(MB3_MeshCombinerSingle combiner, List<MB_DynamicGameObject> dgosToUpdate, MB_MeshVertexChannelFlags newChannels, bool updateVertices, bool updateNormals, bool updateTangents, bool updateUV, bool updateUV2, bool updateUV3, bool updateUV4, bool updateUV5, bool updateUV6, bool updateUV7, bool updateUV8, bool updateColors, bool updateSkinningInfo, UVAdjuster_Atlas uVAdjuster, MB2_LogLevel LOG_LEVEL)
		{
			IMeshChannelsCacheTaggingInterface meshChannelsCache = combiner._meshChannelsCache;
			MB_IMeshBakerSettings settings = combiner.settings;
			IVertexAndTriangleProcessor vertexAndTriProcessor = combiner._vertexAndTriProcessor;
			vertexAndTriProcessor.Init(uvChannelWithExtraParameter: (combiner.settings.assignToMeshCustomizer == null || !(combiner.settings.assignToMeshCustomizer is IAssignToMeshCustomizer_NativeArrays)) ? (-1) : ((IAssignToMeshCustomizer_NativeArrays)combiner.settings.assignToMeshCustomizer).UVchannelWithExtraParameter(), combiner: combiner, newChannels: newChannels, vertexCount: combiner._mesh.vertexCount, newSubmeshTrisSize: new int[0], meshChannelsCache: combiner._meshChannelsCache, loadDataFromCombinedMesh: true, logLevel: LOG_LEVEL);
			if (settings.renderType == MB_RenderType.skinnedMeshRenderer && updateSkinningInfo)
			{
				combiner._boneProcessor.UpdateGameObjects_ReadBoneWeightInfoFromCombinedMesh();
			}
			MB_MeshVertexChannelFlags channelsToUpdate = (updateVertices ? MB_MeshVertexChannelFlags.vertex : MB_MeshVertexChannelFlags.none) | (updateNormals ? MB_MeshVertexChannelFlags.normal : MB_MeshVertexChannelFlags.none) | (updateTangents ? MB_MeshVertexChannelFlags.tangent : MB_MeshVertexChannelFlags.none) | (updateColors ? MB_MeshVertexChannelFlags.colors : MB_MeshVertexChannelFlags.none) | (updateUV ? MB_MeshVertexChannelFlags.uv0 : MB_MeshVertexChannelFlags.none) | (updateUV2 ? MB_MeshVertexChannelFlags.uv2 : MB_MeshVertexChannelFlags.none) | (updateUV3 ? MB_MeshVertexChannelFlags.uv3 : MB_MeshVertexChannelFlags.none) | (updateUV4 ? MB_MeshVertexChannelFlags.uv4 : MB_MeshVertexChannelFlags.none) | (updateUV5 ? MB_MeshVertexChannelFlags.uv5 : MB_MeshVertexChannelFlags.none) | (updateUV6 ? MB_MeshVertexChannelFlags.uv6 : MB_MeshVertexChannelFlags.none) | (updateUV7 ? MB_MeshVertexChannelFlags.uv7 : MB_MeshVertexChannelFlags.none) | (updateUV8 ? MB_MeshVertexChannelFlags.uv8 : MB_MeshVertexChannelFlags.none);
			for (int i = 0; i < dgosToUpdate.Count; i++)
			{
				MB_DynamicGameObject mB_DynamicGameObject = dgosToUpdate[i];
				bool updateBWdata = settings.renderType == MB_RenderType.skinnedMeshRenderer && updateSkinningInfo;
				vertexAndTriProcessor.CopyFromDGOMeshToBuffers(mB_DynamicGameObject, mB_DynamicGameObject.vertIdx, channelsToUpdate, updateTris: false, updateBWdata, settings, combiner._boneProcessor, null, combiner.textureBakeResults, uVAdjuster, LOG_LEVEL, meshChannelsCache);
				mB_DynamicGameObject.UnInitialize();
			}
			combiner._bakeStatus = MeshCombiningStatus.readyForApply;
			if (settings.renderType == MB_RenderType.skinnedMeshRenderer)
			{
				((SkinnedMeshRenderer)combiner.targetRenderer).sharedMesh = null;
				((SkinnedMeshRenderer)combiner.targetRenderer).sharedMesh = combiner._mesh;
			}
			return true;
		}

		public static bool Apply(MB3_MeshCombinerSingle combiner, GenerateUV2Delegate uv2GenerationMethod)
		{
			MB_IMeshBakerSettings settings = combiner.settings;
			bool bones = false;
			if (settings.renderType == MB_RenderType.skinnedMeshRenderer)
			{
				bones = true;
			}
			return Apply(combiner, triangles: true, vertices: true, settings.doNorm, settings.doTan, settings.doUV, MeshBakerSettingsUtility.DoUV2getDataFromSourceMeshes(ref settings), settings.doUV3, settings.doUV4, settings.doUV5, settings.doUV6, settings.doUV7, settings.doUV8, settings.doCol, bones, settings.doBlendShapes, suppressClearMesh: false, uv2GenerationMethod);
		}

		public static bool Apply(MB3_MeshCombinerSingle combiner, bool triangles, bool vertices, bool normals, bool tangents, bool uvs, bool uv2, bool uv3, bool uv4, bool colors, bool bones = false, bool blendShapesFlag = false, GenerateUV2Delegate uv2GenerationMethod = null)
		{
			return Apply(combiner, triangles, vertices, normals, tangents, uvs, uv2, uv3, uv4, uv5: false, uv6: false, uv7: false, uv8: false, colors, bones, blendShapesFlag, suppressClearMesh: false, uv2GenerationMethod);
		}

		internal static bool Apply(MB3_MeshCombinerSingle combiner, bool triangles, bool vertices, bool normals, bool tangents, bool uvs, bool uv2, bool uv3, bool uv4, bool uv5, bool uv6, bool uv7, bool uv8, bool colors, bool bones = false, bool blendShapesFlag = false, bool suppressClearMesh = false, GenerateUV2Delegate uv2GenerationMethod = null)
		{
			MB2_LogLevel lOG_LEVEL = combiner.LOG_LEVEL;
			MB2_ValidationLevel validationLevel = combiner._validationLevel;
			MB_IMeshBakerSettings settings = combiner.settings;
			bool flag = false;
			if (bones && combiner._boneProcessor == null)
			{
				UnityEngine.Debug.LogError("Apply was called with 'bones = true', but the meshCombiner did not contain valid bone data. Was AddDelete(...), Update(...) or ShowHide() called with 'renderType = skinnedMeshRenderer'?");
				flag = true;
			}
			if (validationLevel >= MB2_ValidationLevel.quick && !combiner.ValidateTargRendererAndMeshAndResultSceneObj())
			{
				flag = true;
			}
			if (combiner._bakeStatus != MeshCombiningStatus.readyForApply)
			{
				UnityEngine.Debug.LogError("Apply was called when combiner was not in 'readyForApply' state. Did you call AddDelete(), Update() or ShowHide()");
				flag = true;
			}
			if (combiner._vertexAndTriProcessor != null && combiner._vertexAndTriProcessor.IsDisposed() && combiner._vertexAndTriProcessor.IsInitialized())
			{
				UnityEngine.Debug.LogError("Apply was called with bad meshDataBuffer");
				flag = true;
			}
			if (flag)
			{
				return false;
			}
			Mesh mesh = combiner._mesh;
			Renderer targetRenderer = combiner.targetRenderer;
			MB2_TextureBakeResults textureBakeResults = combiner._textureBakeResults;
			MB2_TextureBakeResults textureBakeResults2 = combiner.textureBakeResults;
			Stopwatch stopwatch = null;
			if (lOG_LEVEL >= MB2_LogLevel.debug)
			{
				stopwatch = new Stopwatch();
				stopwatch.Start();
			}
			if (mesh != null)
			{
				IVertexAndTriangleProcessor vertexAndTriProcessor = combiner._vertexAndTriProcessor;
				if (lOG_LEVEL >= MB2_LogLevel.trace)
				{
					UnityEngine.Debug.Log($"Apply called:\n tri={triangles}\n vert={vertices}\n norm={normals}\n tan={tangents}\n uv={uvs}\n col={colors}\n uv3={uv3}\n uv4={uv4}\n uv2={uv2}\n bone={bones}\n blendShape{blendShapesFlag}\n meshID={mesh.GetInstanceID()}\n");
				}
				if (!suppressClearMesh && (triangles || mesh.vertexCount != vertexAndTriProcessor.GetVertexCount()))
				{
					bool justClearTriangles = triangles && !vertices && !normals && !tangents && !uvs && !colors && !uv3 && !uv4 && !uv2 && !bones;
					MBVersion.SetMeshIndexFormatAndClearMesh(mesh, vertexAndTriProcessor.GetVertexCount(), vertices, justClearTriangles);
				}
				MB_MeshVertexChannelFlags mB_MeshVertexChannelFlags = MB_MeshVertexChannelFlags.none;
				bool flag2 = false;
				if (vertices)
				{
					mB_MeshVertexChannelFlags |= MB_MeshVertexChannelFlags.vertex;
				}
				if (triangles && (bool)textureBakeResults)
				{
					if (textureBakeResults == null)
					{
						UnityEngine.Debug.LogError("Texture Bake Result was not set.");
					}
					else
					{
						flag2 = true;
					}
				}
				if (normals)
				{
					if (settings.doNorm)
					{
						mB_MeshVertexChannelFlags |= MB_MeshVertexChannelFlags.normal;
					}
					else
					{
						UnityEngine.Debug.LogError("normal flag was set in Apply but MeshBaker didn't generate normals");
					}
				}
				if (tangents)
				{
					if (settings.doTan)
					{
						mB_MeshVertexChannelFlags |= MB_MeshVertexChannelFlags.tangent;
					}
					else
					{
						UnityEngine.Debug.LogError("tangent flag was set in Apply but MeshBaker didn't generate tangents");
					}
				}
				if (colors)
				{
					if (settings.doCol)
					{
						mB_MeshVertexChannelFlags |= MB_MeshVertexChannelFlags.colors;
					}
					else
					{
						UnityEngine.Debug.LogError("color flag was set in Apply but MeshBaker didn't generate colors");
					}
				}
				if (uvs)
				{
					if (settings.doUV)
					{
						mB_MeshVertexChannelFlags |= MB_MeshVertexChannelFlags.uv0;
					}
					else
					{
						UnityEngine.Debug.LogError("uv flag was set in Apply but MeshBaker didn't generate uvs");
					}
				}
				if (uv2)
				{
					if (MeshBakerSettingsUtility.DoUV2getDataFromSourceMeshes(ref settings))
					{
						mB_MeshVertexChannelFlags |= MB_MeshVertexChannelFlags.uv2;
					}
					else
					{
						UnityEngine.Debug.LogError("uv2 flag was set in Apply but lightmapping option was set to " + settings.lightmapOption);
					}
				}
				if (uv3)
				{
					if (settings.doUV3)
					{
						mB_MeshVertexChannelFlags |= MB_MeshVertexChannelFlags.uv3;
					}
					else
					{
						UnityEngine.Debug.LogError("uv3 flag was set in Apply but MeshBaker didn't generate uv3s");
					}
				}
				if (uv4)
				{
					if (settings.doUV4)
					{
						mB_MeshVertexChannelFlags |= MB_MeshVertexChannelFlags.uv4;
					}
					else
					{
						UnityEngine.Debug.LogError("uv4 flag was set in Apply but MeshBaker didn't generate uv4s");
					}
				}
				if (uv5)
				{
					if (settings.doUV5)
					{
						mB_MeshVertexChannelFlags |= MB_MeshVertexChannelFlags.uv5;
					}
					else
					{
						UnityEngine.Debug.LogError("uv5 flag was set in Apply but MeshBaker didn't generate uv5s");
					}
				}
				if (uv6)
				{
					if (settings.doUV6)
					{
						mB_MeshVertexChannelFlags |= MB_MeshVertexChannelFlags.uv6;
					}
					else
					{
						UnityEngine.Debug.LogError("uv6 flag was set in Apply but MeshBaker didn't generate uv6s");
					}
				}
				if (uv7)
				{
					if (settings.doUV7)
					{
						mB_MeshVertexChannelFlags |= MB_MeshVertexChannelFlags.uv7;
					}
					else
					{
						UnityEngine.Debug.LogError("uv7 flag was set in Apply but MeshBaker didn't generate uv7s");
					}
				}
				if (uv8)
				{
					if (settings.doUV8)
					{
						mB_MeshVertexChannelFlags |= MB_MeshVertexChannelFlags.uv8;
					}
					else
					{
						UnityEngine.Debug.LogError("uv8 flag was set in Apply but MeshBaker didn't generate uv8s");
					}
				}
				if (bones)
				{
					combiner._boneProcessor.ApplySMRdataToMeshToBuffer();
				}
				vertexAndTriProcessor.AssignBuffersToMesh(mesh, settings, textureBakeResults2, mB_MeshVertexChannelFlags, flag2, settings.assignToMeshCustomizer, combiner.mbDynamicObjectsInCombinedMesh, out var serializableBufferData, out var submeshTrisToUse, out var numNonZeroLengthSubmeshes);
				vertexAndTriProcessor.TransferOwnershipOfSerializableBuffersToCombiner(combiner, vertexAndTriProcessor.channels, serializableBufferData);
				vertexAndTriProcessor.Dispose();
				if ((mB_MeshVertexChannelFlags & MB_MeshVertexChannelFlags.vertex) == MB_MeshVertexChannelFlags.vertex)
				{
					targetRenderer.transform.position = serializableBufferData.meshVerticesShift;
				}
				if (flag2)
				{
					_UpdateMaterialsOnTargetRenderer(combiner.textureBakeResults, combiner.targetRenderer, submeshTrisToUse, numNonZeroLengthSubmeshes);
				}
				bool flag3 = false;
				if (settings.renderType != MB_RenderType.skinnedMeshRenderer && settings.lightmapOption == MB2_LightmapOptions.generate_new_UV2_layout)
				{
					if (uv2GenerationMethod != null)
					{
						uv2GenerationMethod(mesh, settings.uv2UnwrappingParamsHardAngle, settings.uv2UnwrappingParamsPackMargin);
						if (lOG_LEVEL >= MB2_LogLevel.trace)
						{
							UnityEngine.Debug.Log("generating new UV2 layout for the combined mesh ");
						}
					}
					else
					{
						UnityEngine.Debug.LogError("No GenerateUV2Delegate method was supplied. UV2 cannot be generated.");
					}
					flag3 = true;
				}
				else if (settings.renderType == MB_RenderType.skinnedMeshRenderer && settings.lightmapOption == MB2_LightmapOptions.generate_new_UV2_layout && lOG_LEVEL >= MB2_LogLevel.warn)
				{
					UnityEngine.Debug.LogWarning("UV2 cannot be generated for SkinnedMeshRenderer objects.");
				}
				if (settings.renderType != MB_RenderType.skinnedMeshRenderer && settings.lightmapOption == MB2_LightmapOptions.generate_new_UV2_layout && !flag3)
				{
					UnityEngine.Debug.LogError("Failed to generate new UV2 layout. Only works in editor.");
				}
				if (bones)
				{
					combiner._boneProcessor.ApplySMRdataToMesh(combiner, mesh);
					combiner._boneProcessor.Dispose();
					combiner._boneProcessor = null;
				}
				if (blendShapesFlag)
				{
					combiner._blendShapeProcessor.AssignNewBlendShapesToCombinerIfNecessary();
					if (settings.smrMergeBlendShapesWithSameNames)
					{
						combiner._blendShapeProcessor.ApplyBlendShapeFramesToMeshAndBuildMap_MergeBlendShapesWithTheSameName(combiner._mesh.vertexCount);
					}
					else
					{
						combiner._blendShapeProcessor.ApplyBlendShapeFramesToMeshAndBuildMap(combiner._mesh.vertexCount);
					}
					combiner._blendShapeProcessor.Dispose();
					combiner._blendShapeProcessor = null;
				}
				if (triangles || vertices)
				{
					if (lOG_LEVEL >= MB2_LogLevel.trace)
					{
						UnityEngine.Debug.Log("recalculating bounds on mesh.");
					}
					mesh.RecalculateBounds();
				}
				if (settings.optimizeAfterBake && !Application.isPlaying)
				{
					MBVersion.OptimizeMesh(mesh);
				}
				combiner._SetLightmapIndexIfPreserveLightmapping(targetRenderer);
				if (settings.renderType == MB_RenderType.skinnedMeshRenderer)
				{
					if (combiner._mesh.vertexCount == 0)
					{
						if (lOG_LEVEL >= MB2_LogLevel.debug)
						{
							UnityEngine.Debug.Log(" combined mesh had zero vertices. Disabling combined SkinnedMeshRenderer.");
						}
						targetRenderer.enabled = false;
					}
					else
					{
						targetRenderer.enabled = true;
						SkinnedMeshRenderer skinnedMeshRenderer = (SkinnedMeshRenderer)targetRenderer;
						bool updateWhenOffscreen = skinnedMeshRenderer.updateWhenOffscreen;
						skinnedMeshRenderer.updateWhenOffscreen = true;
						skinnedMeshRenderer.updateWhenOffscreen = updateWhenOffscreen;
						skinnedMeshRenderer.sharedMesh = null;
						skinnedMeshRenderer.sharedMesh = mesh;
						skinnedMeshRenderer.bones = combiner.bones;
						if (lOG_LEVEL >= MB2_LogLevel.debug)
						{
							UnityEngine.Debug.Log(" Applying bones and mesh to SkinnedMeshRenderer component  numbones: " + combiner.bones.Length);
						}
						MB3_MeshCombiner.UpdateSkinnedMeshApproximateBoundsFromBoundsStatic(combiner.objectsInCombinedMesh, skinnedMeshRenderer);
					}
				}
				combiner._boneProcessor = null;
			}
			else
			{
				UnityEngine.Debug.LogError("Need to add objects to this meshbaker before calling Apply or ApplyAll");
			}
			if (lOG_LEVEL >= MB2_LogLevel.debug)
			{
				UnityEngine.Debug.Log("Apply Complete time: " + stopwatch.ElapsedMilliseconds + " vertices: " + mesh.vertexCount);
			}
			combiner._bakeStatus = MeshCombiningStatus.preAddDeleteOrUpdate;
			if (settings.clearBuffersAfterBake)
			{
				combiner.ClearBuffers();
			}
			return true;
		}

		public static bool ApplyShowHide(MB3_MeshCombinerSingle combiner)
		{
			MB_IMeshBakerSettings settings = combiner.settings;
			Renderer targetRenderer = combiner.targetRenderer;
			bool flag = false;
			if (combiner._bakeStatus != MeshCombiningStatus.readyForApply)
			{
				UnityEngine.Debug.LogError("Apply was called when combiner was not in 'readyForApply' state. Did you call AddDelete(), Update() or ShowHide()");
				flag = true;
			}
			if (combiner._vertexAndTriProcessor != null && combiner._vertexAndTriProcessor.IsDisposed() && combiner._vertexAndTriProcessor.IsInitialized())
			{
				UnityEngine.Debug.LogError("Apply was called with bad meshDataBuffer");
				flag = true;
			}
			if (flag)
			{
				return false;
			}
			IVertexAndTriangleProcessor vertexAndTriProcessor = combiner._vertexAndTriProcessor;
			BufferDataFromPreviousBake serializableBufferData = combiner.bufferDataFromPrevious;
			vertexAndTriProcessor.AssignTriangleDataForSubmeshes_ShowHide(combiner._mesh, combiner.mbDynamicObjectsInCombinedMesh, ref serializableBufferData, out var submeshTrisToUse, out var numNonZeroLengthSubmeshes);
			vertexAndTriProcessor.TransferOwnershipOfSerializableBuffersToCombiner(combiner, MB_MeshVertexChannelFlags.none, serializableBufferData);
			vertexAndTriProcessor.Dispose();
			_UpdateMaterialsOnTargetRenderer(combiner.textureBakeResults, combiner.targetRenderer, submeshTrisToUse, numNonZeroLengthSubmeshes);
			if (settings.renderType == MB_RenderType.skinnedMeshRenderer)
			{
				if (combiner._mesh.vertexCount == 0)
				{
					if (combiner.LOG_LEVEL >= MB2_LogLevel.debug)
					{
						UnityEngine.Debug.Log(" combined mesh had zero vertices. Disabling combined SkinnedMeshRenderer.");
					}
					targetRenderer.enabled = false;
				}
				else
				{
					targetRenderer.enabled = true;
					SkinnedMeshRenderer skinnedMeshRenderer = (SkinnedMeshRenderer)targetRenderer;
					bool updateWhenOffscreen = skinnedMeshRenderer.updateWhenOffscreen;
					skinnedMeshRenderer.updateWhenOffscreen = true;
					skinnedMeshRenderer.updateWhenOffscreen = updateWhenOffscreen;
					skinnedMeshRenderer.sharedMesh = null;
					skinnedMeshRenderer.sharedMesh = combiner._mesh;
					skinnedMeshRenderer.bones = combiner.bones;
					if (combiner.LOG_LEVEL >= MB2_LogLevel.debug)
					{
						UnityEngine.Debug.Log(" Applying bones and mesh to SkinnedMeshRenderer component  numbones: " + combiner.bones.Length);
					}
					MB3_MeshCombiner.UpdateSkinnedMeshApproximateBoundsFromBoundsStatic(combiner.objectsInCombinedMesh, skinnedMeshRenderer);
				}
			}
			if (combiner.LOG_LEVEL >= MB2_LogLevel.trace)
			{
				UnityEngine.Debug.Log("ApplyShowHide");
			}
			return true;
		}
	}

	public class UVAdjuster_Atlas
	{
		private MB2_TextureBakeResults textureBakeResults;

		private MB2_LogLevel LOG_LEVEL;

		private int[] numTimesMatAppearsInAtlas;

		private MB_MaterialAndUVRect[] matsAndSrcUVRect;

		private bool compareNamesWhenComparingMaterials;

		public UVAdjuster_Atlas(MB2_TextureBakeResults tbr, MB2_LogLevel ll)
		{
			textureBakeResults = tbr;
			LOG_LEVEL = ll;
			matsAndSrcUVRect = tbr.materialsAndUVRects;
			compareNamesWhenComparingMaterials = false;
			if (MBVersion.IsUsingAddressables() && Application.isPlaying)
			{
				compareNamesWhenComparingMaterials = true;
			}
			else
			{
				compareNamesWhenComparingMaterials = false;
			}
			numTimesMatAppearsInAtlas = new int[matsAndSrcUVRect.Length];
			for (int i = 0; i < matsAndSrcUVRect.Length; i++)
			{
				if (numTimesMatAppearsInAtlas[i] > 1)
				{
					continue;
				}
				int num = 1;
				for (int j = i + 1; j < matsAndSrcUVRect.Length; j++)
				{
					if (matsAndSrcUVRect[i].material == matsAndSrcUVRect[j].material)
					{
						num++;
					}
				}
				numTimesMatAppearsInAtlas[i] = num;
				if (num <= 1)
				{
					continue;
				}
				for (int k = i + 1; k < matsAndSrcUVRect.Length; k++)
				{
					if (matsAndSrcUVRect[i].material == matsAndSrcUVRect[k].material)
					{
						numTimesMatAppearsInAtlas[k] = num;
					}
				}
			}
		}

		public bool MapSharedMaterialsToAtlasRects(Material[] sharedMaterials, bool checkTargetSubmeshIdxsFromPreviousBake, Mesh m, IMeshChannelsCacheTaggingInterface meshChannelsCache, Dictionary<int, MB_Utility.MeshAnalysisResult[]> meshAnalysisResultsCache, OrderedDictionary sourceMats2submeshIdx_map, GameObject go, MB_DynamicGameObject dgoOut)
		{
			MB_TextureTilingTreatment[] array = new MB_TextureTilingTreatment[sharedMaterials.Length];
			Rect[] array2 = new Rect[sharedMaterials.Length];
			Rect[] array3 = new Rect[sharedMaterials.Length];
			Rect[] array4 = new Rect[sharedMaterials.Length];
			int[] array5 = new int[sharedMaterials.Length];
			string errorMsg = "";
			for (int i = 0; i < sharedMaterials.Length; i++)
			{
				object obj = null;
				foreach (DictionaryEntry item in sourceMats2submeshIdx_map)
				{
					if (IsSameMaterialInTextureBakeResult(sharedMaterials[i], (Material)item.Key))
					{
						obj = (int)item.Value;
					}
				}
				if (obj == null)
				{
					UnityEngine.Debug.LogError("Source object " + go.name + " used a material " + sharedMaterials[i]?.ToString() + " that was not in the baked materials.");
					if (sharedMaterials[i].name.Contains("(Instance)"))
					{
						UnityEngine.Debug.LogError("The material may be a duplicate of a material that was baked. Materials on a Renderer can be duplicated if the .material field is accessed by a script.");
					}
					return false;
				}
				int num = (int)obj;
				if (checkTargetSubmeshIdxsFromPreviousBake && num != dgoOut.targetSubmeshIdxs[i])
				{
					UnityEngine.Debug.LogError($"Update failed for object {go.name}. Material {sharedMaterials[i]} is mapped to a different submesh in the combined mesh than the previous material. This is not supported. Try using AddDelete.");
					return false;
				}
				if (!TryMapMaterialToUVRect(sharedMaterials[i], m, i, num, meshChannelsCache, meshAnalysisResultsCache, out array[i], out array2[i], out array3[i], out array4[i], out array5[i], ref errorMsg, LOG_LEVEL))
				{
					UnityEngine.Debug.LogError(errorMsg);
					return false;
				}
			}
			dgoOut.uvRects = array2;
			dgoOut.encapsulatingRect = array3;
			dgoOut.sourceMaterialTiling = array4;
			dgoOut.textureArraySliceIdx = array5;
			return true;
		}

		public bool IsSameMaterialInTextureBakeResult(Material a, Material b)
		{
			if (a == b)
			{
				return true;
			}
			if (compareNamesWhenComparingMaterials && a != null && b != null && a.name.Equals(b.name))
			{
				return true;
			}
			return false;
		}

		public bool TryMapMaterialToUVRect(Material mat, Mesh m, int submeshIdx, int idxInResultMats, IMeshChannelsCacheTaggingInterface meshChannelCache, Dictionary<int, MB_Utility.MeshAnalysisResult[]> meshAnalysisCache, out MB_TextureTilingTreatment tilingTreatment, out Rect rectInAtlas, out Rect encapsulatingRectOut, out Rect sourceMaterialTilingOut, out int sliceIdx, ref string errorMsg, MB2_LogLevel logLevel)
		{
			if (textureBakeResults.version < MB2_TextureBakeResults.VERSION)
			{
				textureBakeResults.UpgradeToCurrentVersion(textureBakeResults);
			}
			tilingTreatment = MB_TextureTilingTreatment.unknown;
			if (textureBakeResults.materialsAndUVRects.Length == 0)
			{
				errorMsg = "The 'Texture Bake Result' needs to be re-baked to be compatible with this version of Mesh Baker. Please re-bake using the MB3_TextureBaker.";
				rectInAtlas = default(Rect);
				encapsulatingRectOut = default(Rect);
				sourceMaterialTilingOut = default(Rect);
				sliceIdx = -1;
				return false;
			}
			if (mat == null)
			{
				rectInAtlas = default(Rect);
				encapsulatingRectOut = default(Rect);
				sourceMaterialTilingOut = default(Rect);
				sliceIdx = -1;
				errorMsg = $"Mesh {m.name} Had no material on submesh {submeshIdx} cannot map to a material in the atlas";
				return false;
			}
			if (submeshIdx >= m.subMeshCount)
			{
				errorMsg = "Submesh index is greater than the number of submeshes";
				rectInAtlas = default(Rect);
				encapsulatingRectOut = default(Rect);
				sourceMaterialTilingOut = default(Rect);
				sliceIdx = -1;
				return false;
			}
			int num = -1;
			for (int i = 0; i < matsAndSrcUVRect.Length; i++)
			{
				if (IsSameMaterialInTextureBakeResult(mat, matsAndSrcUVRect[i].material))
				{
					num = i;
					break;
				}
			}
			if (num == -1)
			{
				rectInAtlas = default(Rect);
				encapsulatingRectOut = default(Rect);
				sourceMaterialTilingOut = default(Rect);
				sliceIdx = -1;
				errorMsg = $"Material {mat.name} could not be found in the Texture Bake Result";
				return false;
			}
			if (!textureBakeResults.GetConsiderMeshUVs(idxInResultMats, mat))
			{
				if (numTimesMatAppearsInAtlas[num] != 1)
				{
					UnityEngine.Debug.LogError("There is a problem with this TextureBakeResults. FixOutOfBoundsUVs is false and a material appears more than once: " + matsAndSrcUVRect[num].material?.ToString() + " appears: " + numTimesMatAppearsInAtlas[num]);
				}
				MB_MaterialAndUVRect mB_MaterialAndUVRect = matsAndSrcUVRect[num];
				rectInAtlas = mB_MaterialAndUVRect.atlasRect;
				tilingTreatment = mB_MaterialAndUVRect.tilingTreatment;
				encapsulatingRectOut = mB_MaterialAndUVRect.GetEncapsulatingRect();
				sourceMaterialTilingOut = mB_MaterialAndUVRect.GetMaterialTilingRect();
				sliceIdx = mB_MaterialAndUVRect.textureArraySliceIdx;
				return true;
			}
			if (!meshAnalysisCache.TryGetValue(m.GetInstanceID(), out var value))
			{
				value = new MB_Utility.MeshAnalysisResult[m.subMeshCount];
				for (int j = 0; j < m.subMeshCount; j++)
				{
					meshChannelCache.hasOutOfBoundsUVs(m, ref value[j], j);
				}
				meshAnalysisCache.Add(m.GetInstanceID(), value);
			}
			bool flag = false;
			Rect rect = new Rect(0f, 0f, 0f, 0f);
			Rect rect2 = new Rect(0f, 0f, 0f, 0f);
			if (logLevel >= MB2_LogLevel.trace)
			{
				UnityEngine.Debug.Log(string.Format("Trying to find a rectangle in atlas capable of holding tiled sampling rect for mesh {0} using material {1} meshUVrect={2}", m, mat, value[submeshIdx].uvRect.ToString("f5")));
			}
			for (int k = num; k < matsAndSrcUVRect.Length; k++)
			{
				MB_MaterialAndUVRect mB_MaterialAndUVRect2 = matsAndSrcUVRect[k];
				if (!IsSameMaterialInTextureBakeResult(mat, mB_MaterialAndUVRect2.material))
				{
					continue;
				}
				if (mB_MaterialAndUVRect2.allPropsUseSameTiling)
				{
					rect = mB_MaterialAndUVRect2.allPropsUseSameTiling_samplingEncapsulatinRect;
					rect2 = mB_MaterialAndUVRect2.allPropsUseSameTiling_sourceMaterialTiling;
				}
				else
				{
					rect = mB_MaterialAndUVRect2.propsUseDifferntTiling_srcUVsamplingRect;
					rect2 = new Rect(0f, 0f, 1f, 1f);
				}
				if (MB2_TextureBakeResults.IsMeshAndMaterialRectEnclosedByAtlasRect(mB_MaterialAndUVRect2.tilingTreatment, value[submeshIdx].uvRect, rect2, rect, logLevel))
				{
					if (logLevel >= MB2_LogLevel.trace)
					{
						UnityEngine.Debug.Log("Found rect in atlas capable of containing tiled sampling rect for mesh " + m?.ToString() + " at idx=" + k);
					}
					num = k;
					flag = true;
					break;
				}
			}
			if (flag)
			{
				MB_MaterialAndUVRect mB_MaterialAndUVRect3 = matsAndSrcUVRect[num];
				rectInAtlas = mB_MaterialAndUVRect3.atlasRect;
				tilingTreatment = mB_MaterialAndUVRect3.tilingTreatment;
				encapsulatingRectOut = mB_MaterialAndUVRect3.GetEncapsulatingRect();
				sourceMaterialTilingOut = mB_MaterialAndUVRect3.GetMaterialTilingRect();
				sliceIdx = mB_MaterialAndUVRect3.textureArraySliceIdx;
				return true;
			}
			rectInAtlas = default(Rect);
			encapsulatingRectOut = default(Rect);
			sourceMaterialTilingOut = default(Rect);
			sliceIdx = -1;
			errorMsg = $"Objects To Be Combined mesh {m.name} uses material {mat} on submesh {submeshIdx}. This material requires a rectangle in the atlas that tiles the texture {value[submeshIdx].uvRect.ToString()}. However, MeshBaker could not find a rectangle in the atlas that can contain this tiled rectangle.\n\nTo explain in greater detail, suppose there are two meshes:\n\n - A single-brick mesh that uses a small UV rectangle in a brick-wall.png texture.\n - A brick-wall mesh that tiles the same brick-wall.png texture three times.\n\nIf TextureBaker is used to bake a texture atlas that includes only the single-brick mesh (NOT the brick-wall mesh) and the \"considerUVs\" feature is used, then the TextureBaker will copy only the small UV rectangle (the single brick) with the brick-wall.png texture to the texture atlas.\n\nTHE PROBLEM: If one now attempts to use the same atlas in a MeshBaker-bake with the brick-wall-mesh, this will not work because the brick-wall mesh requires more of the brick-wall.png texture than was copied to the atlas. The brick-wall mesh needs the entire brick-wall.png texture tiled three times.\n\nTHE SOLUTION: To resolve this issue, both the \"single-brick mesh\" and the \"brick-wall mesh\" in the original texture bake, then the TextureBaker will copy the entire brick-wall.png to the atlas tiled three times. This atlas rectangle will work for both the single-brick mesh and the brick-wall mesh.";
			return false;
		}
	}

	public struct VertexAndTriangleProcessor : IVertexAndTriangleProcessor, IDisposable
	{
		private bool _disposed;

		private bool _isInitialized;

		internal MB2_LogLevel LOG_LEVEL;

		private Vector3[] verticies;

		private Vector3[] normals;

		private Vector4[] tangents;

		private Color[] colors;

		private Vector2[] uv0s;

		private float[] uvsSliceIdx;

		private Vector2[] uv2s;

		private Vector2[] uv3s;

		private Vector2[] uv4s;

		private Vector2[] uv5s;

		private Vector2[] uv6s;

		private Vector2[] uv7s;

		private Vector2[] uv8s;

		private SerializableIntArray[] submeshTris;

		public MB_MeshVertexChannelFlags channels { get; private set; }

		public void Dispose()
		{
			if (!_disposed)
			{
				_isInitialized = false;
				channels = MB_MeshVertexChannelFlags.none;
				verticies = null;
				normals = null;
				tangents = null;
				colors = null;
				uv0s = null;
				uvsSliceIdx = null;
				uv2s = null;
				uv3s = null;
				uv4s = null;
				uv5s = null;
				uv6s = null;
				uv7s = null;
				uv8s = null;
				submeshTris = null;
				_disposed = true;
			}
		}

		public bool IsInitialized()
		{
			return _isInitialized;
		}

		public bool IsDisposed()
		{
			return _disposed;
		}

		public void Init(MB3_MeshCombinerSingle combiner, MB_MeshVertexChannelFlags newChannels, int vertexCount, int[] newSubmeshTrisSize, int uvChannelWithExtraParameter, IMeshChannelsCacheTaggingInterface meshChannelsCache, bool loadDataFromCombinedMesh, MB2_LogLevel logLevel)
		{
			if (loadDataFromCombinedMesh)
			{
				InitFromMeshCombiner(combiner, newChannels, uvChannelWithExtraParameter);
			}
			else
			{
				channels = newChannels;
				if ((channels & MB_MeshVertexChannelFlags.vertex) != 0)
				{
					verticies = new Vector3[vertexCount];
				}
				if ((channels & MB_MeshVertexChannelFlags.normal) != 0)
				{
					normals = new Vector3[vertexCount];
				}
				if ((channels & MB_MeshVertexChannelFlags.tangent) != 0)
				{
					tangents = new Vector4[vertexCount];
				}
				if ((channels & MB_MeshVertexChannelFlags.colors) != 0)
				{
					colors = new Color[vertexCount];
				}
				if ((channels & MB_MeshVertexChannelFlags.uv0) != 0)
				{
					uv0s = new Vector2[vertexCount];
				}
				if ((channels & MB_MeshVertexChannelFlags.nuvsSliceIdx) != 0)
				{
					uvsSliceIdx = new float[vertexCount];
				}
				if ((channels & MB_MeshVertexChannelFlags.uv2) != 0)
				{
					uv2s = new Vector2[vertexCount];
				}
				if ((channels & MB_MeshVertexChannelFlags.uv3) != 0)
				{
					uv3s = new Vector2[vertexCount];
				}
				if ((channels & MB_MeshVertexChannelFlags.uv4) != 0)
				{
					uv4s = new Vector2[vertexCount];
				}
				if ((channels & MB_MeshVertexChannelFlags.uv5) != 0)
				{
					uv5s = new Vector2[vertexCount];
				}
				if ((channels & MB_MeshVertexChannelFlags.uv6) != 0)
				{
					uv6s = new Vector2[vertexCount];
				}
				if ((channels & MB_MeshVertexChannelFlags.uv7) != 0)
				{
					uv7s = new Vector2[vertexCount];
				}
				if ((channels & MB_MeshVertexChannelFlags.uv8) != 0)
				{
					uv8s = new Vector2[vertexCount];
				}
				submeshTris = new SerializableIntArray[newSubmeshTrisSize.Length];
				for (int i = 0; i < newSubmeshTrisSize.Length; i++)
				{
					submeshTris[i] = new SerializableIntArray(newSubmeshTrisSize[i]);
				}
			}
			_isInitialized = true;
		}

		public void InitShowHide(MB3_MeshCombinerSingle combiner)
		{
			channels = MB_MeshVertexChannelFlags.none;
			submeshTris = combiner.submeshTris;
			_isInitialized = true;
		}

		public void InitFromMeshCombiner(MB3_MeshCombinerSingle combiner, MB_MeshVertexChannelFlags newChannels, int uvChannelWithExtraParameter)
		{
			if (combiner.channelsLastBake != newChannels)
			{
				if (combiner.channelsLastBake == MB_MeshVertexChannelFlags.none && combiner.verts.Length != 0)
				{
					combiner.channelsLastBake = newChannels;
				}
				else
				{
					UnityEngine.Debug.LogError("Shouldn't change channels between bakes. \n" + combiner.channelsLastBake.ToString() + " \n" + newChannels);
				}
			}
			channels = combiner.channelsLastBake;
			if ((channels & MB_MeshVertexChannelFlags.vertex) != 0)
			{
				verticies = combiner.verts;
			}
			if ((channels & MB_MeshVertexChannelFlags.normal) != 0)
			{
				normals = combiner.normals;
			}
			if ((channels & MB_MeshVertexChannelFlags.tangent) != 0)
			{
				tangents = combiner.tangents;
			}
			if ((channels & MB_MeshVertexChannelFlags.colors) != 0)
			{
				colors = combiner.colors;
			}
			if ((channels & MB_MeshVertexChannelFlags.uv0) != 0)
			{
				uv0s = combiner.uvs;
			}
			if ((channels & MB_MeshVertexChannelFlags.nuvsSliceIdx) != 0)
			{
				uvsSliceIdx = combiner.uvsSliceIdx;
			}
			if ((channels & MB_MeshVertexChannelFlags.uv2) != 0)
			{
				uv2s = combiner.uv2s;
			}
			if ((channels & MB_MeshVertexChannelFlags.uv3) != 0)
			{
				uv3s = combiner.uv3s;
			}
			if ((channels & MB_MeshVertexChannelFlags.uv4) != 0)
			{
				uv4s = combiner.uv4s;
			}
			if ((channels & MB_MeshVertexChannelFlags.uv5) != 0)
			{
				uv5s = combiner.uv5s;
			}
			if ((channels & MB_MeshVertexChannelFlags.uv6) != 0)
			{
				uv6s = combiner.uv6s;
			}
			if ((channels & MB_MeshVertexChannelFlags.uv7) != 0)
			{
				uv7s = combiner.uv7s;
			}
			if ((channels & MB_MeshVertexChannelFlags.uv8) != 0)
			{
				uv8s = combiner.uv8s;
			}
			submeshTris = combiner.submeshTris;
			_isInitialized = true;
		}

		public int GetVertexCount()
		{
			return verticies.Length;
		}

		public int GetSubmeshCount()
		{
			return submeshTris.Length;
		}

		public void TransferOwnershipOfSerializableBuffersToCombiner(MB3_MeshCombinerSingle c, MB_MeshVertexChannelFlags channelsToTransfer, BufferDataFromPreviousBake serializableBufferData)
		{
			c.channelsLastBake = channels;
			if ((channelsToTransfer & MB_MeshVertexChannelFlags.vertex) != 0)
			{
				c.verts = verticies;
			}
			if ((channelsToTransfer & MB_MeshVertexChannelFlags.normal) != 0)
			{
				c.normals = normals;
			}
			if ((channelsToTransfer & MB_MeshVertexChannelFlags.tangent) != 0)
			{
				c.tangents = tangents;
			}
			if ((channelsToTransfer & MB_MeshVertexChannelFlags.uv0) != 0)
			{
				c.uvs = uv0s;
			}
			if ((channelsToTransfer & MB_MeshVertexChannelFlags.nuvsSliceIdx) != 0)
			{
				c.uvsSliceIdx = uvsSliceIdx;
			}
			if ((channelsToTransfer & MB_MeshVertexChannelFlags.uv2) != 0)
			{
				c.uv2s = uv2s;
			}
			if ((channelsToTransfer & MB_MeshVertexChannelFlags.uv3) != 0)
			{
				c.uv3s = uv3s;
			}
			if ((channelsToTransfer & MB_MeshVertexChannelFlags.uv4) != 0)
			{
				c.uv4s = uv4s;
			}
			if ((channelsToTransfer & MB_MeshVertexChannelFlags.uv5) != 0)
			{
				c.uv5s = uv5s;
			}
			if ((channelsToTransfer & MB_MeshVertexChannelFlags.uv6) != 0)
			{
				c.uv6s = uv6s;
			}
			if ((channelsToTransfer & MB_MeshVertexChannelFlags.uv7) != 0)
			{
				c.uv7s = uv7s;
			}
			if ((channelsToTransfer & MB_MeshVertexChannelFlags.uv8) != 0)
			{
				c.uv8s = uv8s;
			}
			if ((channelsToTransfer & MB_MeshVertexChannelFlags.colors) != 0)
			{
				c.colors = colors;
			}
			c.submeshTris = submeshTris;
			c.bufferDataFromPrevious = serializableBufferData;
			verticies = null;
			normals = null;
			tangents = null;
			uv0s = null;
			uvsSliceIdx = null;
			uv2s = null;
			uv3s = null;
			uv4s = null;
			uv5s = null;
			uv6s = null;
			uv7s = null;
			uv8s = null;
			colors = null;
			submeshTris = null;
			_isInitialized = false;
		}

		public void CopyArraysFromPreviousBakeBuffersToNewBuffers(MB_DynamicGameObject dgo, ref IVertexAndTriangleProcessor iOldBuffers, int destStartVertIdx, int triangleIdxAdjustment, int[] targSubmeshTidx, MB2_LogLevel LOG_LEVEL)
		{
			VertexAndTriangleProcessor vertexAndTriangleProcessor = (VertexAndTriangleProcessor)(object)iOldBuffers;
			int vertIdx = dgo.vertIdx;
			int numVerts = dgo.numVerts;
			if ((channels & MB_MeshVertexChannelFlags.vertex) == MB_MeshVertexChannelFlags.vertex)
			{
				Array.Copy(vertexAndTriangleProcessor.verticies, vertIdx, verticies, destStartVertIdx, numVerts);
			}
			if ((channels & MB_MeshVertexChannelFlags.normal) == MB_MeshVertexChannelFlags.normal)
			{
				Array.Copy(vertexAndTriangleProcessor.normals, vertIdx, normals, destStartVertIdx, numVerts);
			}
			if ((channels & MB_MeshVertexChannelFlags.tangent) == MB_MeshVertexChannelFlags.tangent)
			{
				Array.Copy(vertexAndTriangleProcessor.tangents, vertIdx, tangents, destStartVertIdx, numVerts);
			}
			if ((channels & MB_MeshVertexChannelFlags.uv0) == MB_MeshVertexChannelFlags.uv0)
			{
				Array.Copy(vertexAndTriangleProcessor.uv0s, vertIdx, uv0s, destStartVertIdx, numVerts);
			}
			if ((channels & MB_MeshVertexChannelFlags.nuvsSliceIdx) == MB_MeshVertexChannelFlags.nuvsSliceIdx)
			{
				Array.Copy(vertexAndTriangleProcessor.uvsSliceIdx, vertIdx, uvsSliceIdx, destStartVertIdx, numVerts);
			}
			if ((channels & MB_MeshVertexChannelFlags.uv2) == MB_MeshVertexChannelFlags.uv2)
			{
				Array.Copy(vertexAndTriangleProcessor.uv2s, vertIdx, uv2s, destStartVertIdx, numVerts);
			}
			if ((channels & MB_MeshVertexChannelFlags.uv3) == MB_MeshVertexChannelFlags.uv3)
			{
				Array.Copy(vertexAndTriangleProcessor.uv3s, vertIdx, uv3s, destStartVertIdx, numVerts);
			}
			if ((channels & MB_MeshVertexChannelFlags.uv4) == MB_MeshVertexChannelFlags.uv4)
			{
				Array.Copy(vertexAndTriangleProcessor.uv4s, vertIdx, uv4s, destStartVertIdx, numVerts);
			}
			if ((channels & MB_MeshVertexChannelFlags.uv5) == MB_MeshVertexChannelFlags.uv5)
			{
				Array.Copy(vertexAndTriangleProcessor.uv5s, vertIdx, uv5s, destStartVertIdx, numVerts);
			}
			if ((channels & MB_MeshVertexChannelFlags.uv6) == MB_MeshVertexChannelFlags.uv6)
			{
				Array.Copy(vertexAndTriangleProcessor.uv6s, vertIdx, uv6s, destStartVertIdx, numVerts);
			}
			if ((channels & MB_MeshVertexChannelFlags.uv7) == MB_MeshVertexChannelFlags.uv7)
			{
				Array.Copy(vertexAndTriangleProcessor.uv7s, vertIdx, uv7s, destStartVertIdx, numVerts);
			}
			if ((channels & MB_MeshVertexChannelFlags.uv8) == MB_MeshVertexChannelFlags.uv8)
			{
				Array.Copy(vertexAndTriangleProcessor.uv8s, vertIdx, uv8s, destStartVertIdx, numVerts);
			}
			if ((channels & MB_MeshVertexChannelFlags.colors) == MB_MeshVertexChannelFlags.colors)
			{
				Array.Copy(vertexAndTriangleProcessor.colors, vertIdx, colors, destStartVertIdx, numVerts);
			}
			for (int i = 0; i < submeshTris.Length; i++)
			{
				int[] data = vertexAndTriangleProcessor.submeshTris[i].data;
				int num = dgo.submeshTriIdxs[i];
				int num2 = dgo.submeshNumTris[i];
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					MB2_Log.LogDebug("    Adjusting submesh triangles submesh:" + i + " startIdx:" + num + " num:" + num2 + " nsubmeshTris:" + submeshTris.Length + " targSubmeshTidx:" + targSubmeshTidx.Length, LOG_LEVEL);
				}
				for (int j = num; j < num + num2; j++)
				{
					data[j] -= triangleIdxAdjustment;
				}
				Array.Copy(data, num, submeshTris[i].data, targSubmeshTidx[i], num2);
			}
		}

		public void CopyFromDGOMeshToBuffers(MB_DynamicGameObject dgo, int destStartVertsIdx, MB_MeshVertexChannelFlags channelsToUpdate, bool updateTris, bool updateBWdata, MB_IMeshBakerSettings settings, MB_IMeshCombinerSingle_BoneProcessor boneProcessor, int[] targSubmeshTidx, MB2_TextureBakeResults textureBakeResults, UVAdjuster_Atlas uvAdjuster, MB2_LogLevel LOG_LEVEL, IMeshChannelsCacheTaggingInterface meshChannelCacheParam)
		{
			MeshChannelsCache meshChannelsCache = (MeshChannelsCache)meshChannelCacheParam;
			bool flag = (channels & MB_MeshVertexChannelFlags.vertex) == MB_MeshVertexChannelFlags.vertex && (channelsToUpdate & MB_MeshVertexChannelFlags.vertex) == MB_MeshVertexChannelFlags.vertex;
			bool flag2 = (channels & MB_MeshVertexChannelFlags.normal) == MB_MeshVertexChannelFlags.normal && (channelsToUpdate & MB_MeshVertexChannelFlags.normal) == MB_MeshVertexChannelFlags.normal;
			bool flag3 = (channels & MB_MeshVertexChannelFlags.tangent) == MB_MeshVertexChannelFlags.tangent && (channelsToUpdate & MB_MeshVertexChannelFlags.tangent) == MB_MeshVertexChannelFlags.tangent;
			if (flag || flag2 || flag3)
			{
				Vector3[] array = null;
				Vector3[] array2 = null;
				Vector4[] array3 = null;
				if (flag)
				{
					array = meshChannelsCache.GetVertices(dgo._mesh);
				}
				if (flag2)
				{
					array2 = meshChannelsCache.GetNormals(dgo._mesh);
				}
				if (flag3)
				{
					array3 = meshChannelsCache.GetTangents(dgo._mesh);
				}
				if (settings.renderType != MB_RenderType.skinnedMeshRenderer)
				{
					_LocalToWorld(dgo.gameObject.transform, flag2, flag3, destStartVertsIdx, array, array2, array3, verticies, normals, tangents);
				}
				else
				{
					boneProcessor.CopyVertsNormsTansToBuffers(dgo, settings, destStartVertsIdx, array2, array3, array, normals, tangents, verticies);
				}
			}
			if ((channels & MB_MeshVertexChannelFlags.uv0) == MB_MeshVertexChannelFlags.uv0 && (channelsToUpdate & MB_MeshVertexChannelFlags.uv0) == MB_MeshVertexChannelFlags.uv0)
			{
				_copyAndAdjustUVsFromMesh(textureBakeResults, dgo, dgo._mesh, 0, destStartVertsIdx, uv0s, uvsSliceIdx, meshChannelsCache, LOG_LEVEL, textureBakeResults);
			}
			if ((channels & MB_MeshVertexChannelFlags.uv2) == MB_MeshVertexChannelFlags.uv2 && (channelsToUpdate & MB_MeshVertexChannelFlags.uv2) == MB_MeshVertexChannelFlags.uv2)
			{
				_CopyAndAdjustUV2FromMesh(settings, meshChannelsCache, dgo, destStartVertsIdx, LOG_LEVEL);
			}
			if ((channels & MB_MeshVertexChannelFlags.uv3) == MB_MeshVertexChannelFlags.uv3 && (channelsToUpdate & MB_MeshVertexChannelFlags.uv3) == MB_MeshVertexChannelFlags.uv3)
			{
				meshChannelsCache.GetUVChannel(3, dgo._mesh).CopyTo(uv3s, destStartVertsIdx);
			}
			if ((channels & MB_MeshVertexChannelFlags.uv4) == MB_MeshVertexChannelFlags.uv4 && (channelsToUpdate & MB_MeshVertexChannelFlags.uv4) == MB_MeshVertexChannelFlags.uv4)
			{
				meshChannelsCache.GetUVChannel(4, dgo._mesh).CopyTo(uv4s, destStartVertsIdx);
			}
			if ((channels & MB_MeshVertexChannelFlags.uv5) == MB_MeshVertexChannelFlags.uv5 && (channelsToUpdate & MB_MeshVertexChannelFlags.uv5) == MB_MeshVertexChannelFlags.uv5)
			{
				meshChannelsCache.GetUVChannel(5, dgo._mesh).CopyTo(uv5s, destStartVertsIdx);
			}
			if ((channels & MB_MeshVertexChannelFlags.uv6) == MB_MeshVertexChannelFlags.uv6 && (channelsToUpdate & MB_MeshVertexChannelFlags.uv6) == MB_MeshVertexChannelFlags.uv6)
			{
				meshChannelsCache.GetUVChannel(6, dgo._mesh).CopyTo(uv6s, destStartVertsIdx);
			}
			if ((channels & MB_MeshVertexChannelFlags.uv7) == MB_MeshVertexChannelFlags.uv7 && (channelsToUpdate & MB_MeshVertexChannelFlags.uv7) == MB_MeshVertexChannelFlags.uv7)
			{
				meshChannelsCache.GetUVChannel(7, dgo._mesh).CopyTo(uv7s, destStartVertsIdx);
			}
			if ((channels & MB_MeshVertexChannelFlags.uv8) == MB_MeshVertexChannelFlags.uv8 && (channelsToUpdate & MB_MeshVertexChannelFlags.uv8) == MB_MeshVertexChannelFlags.uv8)
			{
				meshChannelsCache.GetUVChannel(8, dgo._mesh).CopyTo(uv8s, destStartVertsIdx);
			}
			if ((channels & MB_MeshVertexChannelFlags.colors) == MB_MeshVertexChannelFlags.colors && (channelsToUpdate & MB_MeshVertexChannelFlags.colors) == MB_MeshVertexChannelFlags.colors)
			{
				meshChannelsCache.GetColors(dgo._mesh).CopyTo(colors, destStartVertsIdx);
			}
			if (updateBWdata)
			{
				boneProcessor.UpdateGameObjects_UpdateBWIndexes(dgo);
			}
			if (!updateTris)
			{
				return;
			}
			for (int i = 0; i < targSubmeshTidx.Length; i++)
			{
				dgo.submeshTriIdxs[i] = targSubmeshTidx[i];
			}
			for (int j = 0; j < dgo._tmpSubmeshTris.Length; j++)
			{
				int[] data = dgo._tmpSubmeshTris[j].data;
				if (destStartVertsIdx != 0)
				{
					for (int k = 0; k < data.Length; k++)
					{
						data[k] += destStartVertsIdx;
					}
				}
				if (dgo.invertTriangles)
				{
					for (int l = 0; l < data.Length; l += 3)
					{
						int num = data[l];
						data[l] = data[l + 1];
						data[l + 1] = num;
					}
				}
				int num2 = dgo.targetSubmeshIdxs[j];
				data.CopyTo(submeshTris[num2].data, targSubmeshTidx[num2]);
				dgo.submeshNumTris[num2] += data.Length;
				targSubmeshTidx[num2] += data.Length;
			}
		}

		public void AssignBuffersToMesh(Mesh mesh, MB_IMeshBakerSettings settings, MB2_TextureBakeResults textureBakeResults, MB_MeshVertexChannelFlags channelsToWriteToMesh, bool doWriteTrisToMesh, IAssignToMeshCustomizer assignToMeshCustomizer, List<MB_DynamicGameObject> mbDynamicObjectsInCombinedMesh, out BufferDataFromPreviousBake serializableBufferData, out SerializableIntArray[] submeshTrisToUse, out int numNonZeroLengthSubmeshes)
		{
			if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.vertex) == MB_MeshVertexChannelFlags.vertex)
			{
				AdjustVertsToWriteAccordingToPivotPositionIfNecessary(settings.pivotLocationType, settings.renderType, settings.clearBuffersAfterBake, settings.pivotLocation, out serializableBufferData, out var verts2Write);
				mesh.vertices = verts2Write;
			}
			else
			{
				serializableBufferData.numVertsBaked = mesh.vertexCount;
				serializableBufferData.meshVerticesShift = Vector3.zero;
				serializableBufferData.meshVerticiesWereShifted = false;
			}
			if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.normal) == MB_MeshVertexChannelFlags.normal)
			{
				mesh.normals = normals;
			}
			if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.tangent) == MB_MeshVertexChannelFlags.tangent)
			{
				mesh.tangents = tangents;
			}
			if (assignToMeshCustomizer != null)
			{
				IAssignToMeshCustomizer_SimpleAPI assignToMeshCustomizer_SimpleAPI = (IAssignToMeshCustomizer_SimpleAPI)assignToMeshCustomizer;
				if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv0) == MB_MeshVertexChannelFlags.uv0)
				{
					assignToMeshCustomizer_SimpleAPI.meshAssign_UV0(0, settings, textureBakeResults, mesh, uv0s, uvsSliceIdx);
				}
				if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv2) == MB_MeshVertexChannelFlags.uv2)
				{
					assignToMeshCustomizer_SimpleAPI.meshAssign_UV2(2, settings, textureBakeResults, mesh, uv2s, uvsSliceIdx);
				}
				if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv3) == MB_MeshVertexChannelFlags.uv3)
				{
					assignToMeshCustomizer_SimpleAPI.meshAssign_UV3(3, settings, textureBakeResults, mesh, uv3s, uvsSliceIdx);
				}
				if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv4) == MB_MeshVertexChannelFlags.uv4)
				{
					assignToMeshCustomizer_SimpleAPI.meshAssign_UV4(4, settings, textureBakeResults, mesh, uv4s, uvsSliceIdx);
				}
				if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv5) == MB_MeshVertexChannelFlags.uv5)
				{
					assignToMeshCustomizer_SimpleAPI.meshAssign_UV5(5, settings, textureBakeResults, mesh, uv5s, uvsSliceIdx);
				}
				if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv6) == MB_MeshVertexChannelFlags.uv6)
				{
					assignToMeshCustomizer_SimpleAPI.meshAssign_UV6(6, settings, textureBakeResults, mesh, uv6s, uvsSliceIdx);
				}
				if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv7) == MB_MeshVertexChannelFlags.uv7)
				{
					assignToMeshCustomizer_SimpleAPI.meshAssign_UV7(7, settings, textureBakeResults, mesh, uv7s, uvsSliceIdx);
				}
				if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv8) == MB_MeshVertexChannelFlags.uv8)
				{
					assignToMeshCustomizer_SimpleAPI.meshAssign_UV8(8, settings, textureBakeResults, mesh, uv8s, uvsSliceIdx);
				}
				if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.colors) == MB_MeshVertexChannelFlags.colors)
				{
					assignToMeshCustomizer_SimpleAPI.meshAssign_colors(settings, textureBakeResults, mesh, colors, uvsSliceIdx);
				}
			}
			else
			{
				if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv0) == MB_MeshVertexChannelFlags.uv0)
				{
					MBVersion.MeshAssignUVChannel(0, mesh, uv0s);
				}
				if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv2) == MB_MeshVertexChannelFlags.uv2)
				{
					MBVersion.MeshAssignUVChannel(2, mesh, uv2s);
				}
				if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv3) == MB_MeshVertexChannelFlags.uv3)
				{
					MBVersion.MeshAssignUVChannel(3, mesh, uv3s);
				}
				if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv4) == MB_MeshVertexChannelFlags.uv4)
				{
					MBVersion.MeshAssignUVChannel(4, mesh, uv4s);
				}
				if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv5) == MB_MeshVertexChannelFlags.uv5)
				{
					MBVersion.MeshAssignUVChannel(5, mesh, uv5s);
				}
				if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv6) == MB_MeshVertexChannelFlags.uv6)
				{
					MBVersion.MeshAssignUVChannel(6, mesh, uv6s);
				}
				if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv7) == MB_MeshVertexChannelFlags.uv7)
				{
					MBVersion.MeshAssignUVChannel(7, mesh, uv7s);
				}
				if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv8) == MB_MeshVertexChannelFlags.uv8)
				{
					MBVersion.MeshAssignUVChannel(8, mesh, uv8s);
				}
				if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.colors) == MB_MeshVertexChannelFlags.colors)
				{
					mesh.colors = colors;
				}
			}
			if (doWriteTrisToMesh)
			{
				AssignTriangleDataForSubmeshes(mesh, mbDynamicObjectsInCombinedMesh, ref serializableBufferData, out submeshTrisToUse, out numNonZeroLengthSubmeshes);
				return;
			}
			submeshTrisToUse = null;
			numNonZeroLengthSubmeshes = -1;
		}

		public void AssignTriangleDataForSubmeshes(Mesh mesh, List<MB_DynamicGameObject> mbDynamicObjectsInCombinedMesh, ref BufferDataFromPreviousBake serializableBufferData, out SerializableIntArray[] submeshTrisToUse, out int numNonZeroLengthSubmeshes)
		{
			submeshTrisToUse = GetSubmeshTrisWithShowHideApplied(mbDynamicObjectsInCombinedMesh);
			int numIndexes = 0;
			numNonZeroLengthSubmeshes = _NumNonZeroLengthSubmeshTris(submeshTrisToUse, out numIndexes);
			mesh.subMeshCount = numNonZeroLengthSubmeshes;
			int num = 0;
			for (int i = 0; i < submeshTrisToUse.Length; i++)
			{
				if (submeshTrisToUse[i].data.Length != 0)
				{
					mesh.SetTriangles(submeshTrisToUse[i].data, num);
					num++;
				}
			}
		}

		public void AssignTriangleDataForSubmeshes_ShowHide(Mesh mesh, List<MB_DynamicGameObject> mbDynamicObjectsInCombinedMesh, ref BufferDataFromPreviousBake serializableBufferData, out SerializableIntArray[] submeshTrisToUse, out int numNonZeroLengthSubmeshes)
		{
			AssignTriangleDataForSubmeshes(mesh, mbDynamicObjectsInCombinedMesh, ref serializableBufferData, out submeshTrisToUse, out numNonZeroLengthSubmeshes);
		}

		private void AdjustVertsToWriteAccordingToPivotPositionIfNecessary(MB_MeshPivotLocation pivotLocationType, MB_RenderType renderType, bool clearBuffersAfterBake, Vector3 pivotLocation_wld, out BufferDataFromPreviousBake serializableBufferData, out Vector3[] verts2Write)
		{
			verts2Write = verticies;
			serializableBufferData.numVertsBaked = verticies.Length;
			if (verticies.Length != 0)
			{
				if (renderType == MB_RenderType.skinnedMeshRenderer)
				{
					serializableBufferData.numVertsBaked = verticies.Length;
					serializableBufferData.meshVerticesShift = Vector3.zero;
					serializableBufferData.meshVerticiesWereShifted = false;
					return;
				}
				switch (pivotLocationType)
				{
				case MB_MeshPivotLocation.worldOrigin:
					serializableBufferData.numVertsBaked = verticies.Length;
					serializableBufferData.meshVerticesShift = Vector3.zero;
					serializableBufferData.meshVerticiesWereShifted = false;
					break;
				case MB_MeshPivotLocation.boundsCenter:
				case MB_MeshPivotLocation.customLocation:
				{
					Vector3 vector4;
					if (pivotLocationType == MB_MeshPivotLocation.boundsCenter)
					{
						Vector3 vector = verticies[0];
						Vector3 vector2 = verticies[0];
						for (int i = 1; i < verticies.Length; i++)
						{
							Vector3 vector3 = verticies[i];
							if (vector.x < vector3.x)
							{
								vector.x = vector3.x;
							}
							if (vector.y < vector3.y)
							{
								vector.y = vector3.y;
							}
							if (vector.z < vector3.z)
							{
								vector.z = vector3.z;
							}
							if (vector2.x > vector3.x)
							{
								vector2.x = vector3.x;
							}
							if (vector2.y > vector3.y)
							{
								vector2.y = vector3.y;
							}
							if (vector2.z > vector3.z)
							{
								vector2.z = vector3.z;
							}
						}
						vector4 = (vector + vector2) * 0.5f;
					}
					else
					{
						vector4 = pivotLocation_wld;
					}
					if (!clearBuffersAfterBake)
					{
						verts2Write = new Vector3[verticies.Length];
					}
					for (int j = 0; j < verticies.Length; j++)
					{
						verts2Write[j] = verticies[j] - vector4;
					}
					serializableBufferData.numVertsBaked = verticies.Length;
					serializableBufferData.meshVerticesShift = vector4;
					serializableBufferData.meshVerticiesWereShifted = true;
					break;
				}
				default:
					UnityEngine.Debug.LogError("Unsupported Pivot Location Type: " + pivotLocationType);
					serializableBufferData.numVertsBaked = verticies.Length;
					serializableBufferData.meshVerticesShift = Vector3.zero;
					serializableBufferData.meshVerticiesWereShifted = false;
					break;
				}
			}
			else
			{
				serializableBufferData.numVertsBaked = verticies.Length;
				serializableBufferData.meshVerticesShift = Vector3.zero;
				serializableBufferData.meshVerticiesWereShifted = false;
			}
		}

		private static int _NumNonZeroLengthSubmeshTris(SerializableIntArray[] subTris, out int numIndexes)
		{
			numIndexes = 0;
			int num = 0;
			for (int i = 0; i < subTris.Length; i++)
			{
				if (subTris[i].data.Length != 0)
				{
					num++;
					numIndexes += subTris[i].data.Length;
				}
			}
			return num;
		}

		private void _copyAndAdjustUVsFromMesh(MB2_TextureBakeResults tbr, MB_DynamicGameObject dgo, Mesh mesh, int uvChannel, int vertsIdx, Vector2[] uvsOut, float[] uvsSliceIdx, MeshChannelsCache meshChannelsCache, MB2_LogLevel LOG_LEVEL, MB2_TextureBakeResults textureBakeResults)
		{
			Vector2[] uVChannel = meshChannelsCache.GetUVChannel(uvChannel, mesh);
			int[] array = new int[uVChannel.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = -1;
			}
			bool flag = false;
			bool flag2 = tbr.resultType == MB2_TextureBakeResults.ResultType.textureArray;
			for (int j = 0; j < dgo.targetSubmeshIdxs.Length; j++)
			{
				int[] array2 = ((dgo._tmpSubmeshTris == null) ? mesh.GetTriangles(j) : dgo._tmpSubmeshTris[j].data);
				float num = dgo.textureArraySliceIdx[j];
				int idxInSrcMats = dgo.targetSubmeshIdxs[j];
				if (LOG_LEVEL >= MB2_LogLevel.trace)
				{
					UnityEngine.Debug.Log($"Build UV transform for mesh {dgo.name} submesh {j} encapsulatingRect {dgo.encapsulatingRect[j]}");
				}
				Rect rect = MB3_TextureCombinerMerging.BuildTransformMeshUV2AtlasRect(textureBakeResults.GetConsiderMeshUVs(idxInSrcMats, dgo.sourceSharedMaterials[j]), dgo.uvRects[j], (dgo.obUVRects == null || dgo.obUVRects.Length == 0) ? new Rect(0f, 0f, 1f, 1f) : dgo.obUVRects[j], dgo.sourceMaterialTiling[j], dgo.encapsulatingRect[j]);
				foreach (int num2 in array2)
				{
					if (array[num2] == -1)
					{
						array[num2] = j;
						Vector2 vector = uVChannel[num2];
						vector.x = rect.x + vector.x * rect.width;
						vector.y = rect.y + vector.y * rect.height;
						int num3 = vertsIdx + num2;
						uvsOut[num3] = vector;
						if (flag2)
						{
							uvsSliceIdx[num3] = num;
						}
					}
					if (array[num2] != j)
					{
						flag = true;
					}
				}
			}
			if (flag && LOG_LEVEL >= MB2_LogLevel.warn)
			{
				UnityEngine.Debug.LogWarning(dgo.name + "has submeshes which share verticies. Adjusted uvs may not map correctly in combined atlas.");
			}
			if (LOG_LEVEL >= MB2_LogLevel.trace)
			{
				UnityEngine.Debug.Log($"_copyAndAdjustUVsFromMesh copied {uVChannel.Length} verts");
			}
		}

		private void _CopyAndAdjustUV2FromMesh(MB_IMeshBakerSettings settings, MeshChannelsCache meshChannelsCache, MB_DynamicGameObject dgo, int vertsIdx, MB2_LogLevel LOG_LEVEL)
		{
			Vector2[] array = meshChannelsCache.GetUVChannel(2, dgo._mesh);
			if (settings.lightmapOption == MB2_LightmapOptions.preserve_current_lightmapping)
			{
				if (array == null || array.Length == 0)
				{
					Vector2[] uVChannel = meshChannelsCache.GetUVChannel(0, dgo._mesh);
					if (uVChannel != null && uVChannel.Length != 0)
					{
						array = uVChannel;
					}
					else
					{
						if (LOG_LEVEL >= MB2_LogLevel.warn)
						{
							UnityEngine.Debug.LogWarning("Mesh " + dgo._mesh?.ToString() + " didn't have uv2s. Generating uv2s.");
						}
						array = meshChannelsCache.GetUv2Modified(dgo._mesh);
					}
				}
				Vector4 lightmapTilingOffset = dgo.lightmapTilingOffset;
				Vector2 vector = new Vector2(lightmapTilingOffset.x, lightmapTilingOffset.y);
				Vector2 vector2 = new Vector2(lightmapTilingOffset.z, lightmapTilingOffset.w);
				Vector2 vector3 = default(Vector2);
				for (int i = 0; i < array.Length; i++)
				{
					vector3.x = vector.x * array[i].x;
					vector3.y = vector.y * array[i].y;
					uv2s[vertsIdx + i] = vector2 + vector3;
				}
				if (LOG_LEVEL >= MB2_LogLevel.trace)
				{
					UnityEngine.Debug.Log("_copyAndAdjustUV2FromMesh copied and modify for preserve current lightmapping " + array.Length);
				}
				return;
			}
			if (array == null || array.Length == 0)
			{
				if (LOG_LEVEL >= MB2_LogLevel.warn)
				{
					UnityEngine.Debug.LogWarning("Mesh " + dgo._mesh?.ToString() + " didn't have uv2s. Generating uv2s.");
				}
				if (settings.lightmapOption == MB2_LightmapOptions.copy_UV2_unchanged_to_separate_rects && (array == null || array.Length == 0))
				{
					UnityEngine.Debug.LogError("Mesh " + dgo._mesh?.ToString() + " did not have a UV2 channel. Nothing to copy when trying to copy UV2 to separate rects. The combined mesh will not lightmap properly. Try using generate new uv2 layout.");
				}
				array = meshChannelsCache.GetUv2Modified(dgo._mesh);
			}
			array.CopyTo(uv2s, vertsIdx);
			if (LOG_LEVEL >= MB2_LogLevel.trace)
			{
				UnityEngine.Debug.Log("_copyAndAdjustUV2FromMesh copied without modifying " + array.Length);
			}
		}

		public void CopyUV2unchangedToSeparateRects(List<MB_DynamicGameObject> mbDynamicObjectsInCombinedMesh, float uv2UnwrappingParamsPackMargin)
		{
			int num = Mathf.CeilToInt(8192f * uv2UnwrappingParamsPackMargin);
			if (num < 1)
			{
				num = 1;
			}
			List<Vector2> list = new List<Vector2>(mbDynamicObjectsInCombinedMesh.Count);
			float[] array = new float[mbDynamicObjectsInCombinedMesh.Count];
			Rect[] array2 = new Rect[mbDynamicObjectsInCombinedMesh.Count];
			float num2 = 0f;
			for (int i = 0; i < mbDynamicObjectsInCombinedMesh.Count; i++)
			{
				MB_DynamicGameObject mB_DynamicGameObject = mbDynamicObjectsInCombinedMesh[i];
				float num3 = 1f;
				if (Application.isEditor && mB_DynamicGameObject._renderer is MeshRenderer)
				{
					num3 = MBVersion.GetScaleInLightmap((MeshRenderer)mB_DynamicGameObject._renderer);
					if (num3 <= 0f)
					{
						num3 = 1f;
					}
				}
				float magnitude = mB_DynamicGameObject.meshSize.magnitude;
				array[i] = num3 * magnitude;
				num2 += array[i];
			}
			for (int j = 0; j < array.Length; j++)
			{
				MB_DynamicGameObject mB_DynamicGameObject2 = mbDynamicObjectsInCombinedMesh[j];
				int num4 = mB_DynamicGameObject2.vertIdx + mB_DynamicGameObject2.numVerts;
				float x;
				float num5 = (x = uv2s[mB_DynamicGameObject2.vertIdx].x);
				float y;
				float num6 = (y = uv2s[mB_DynamicGameObject2.vertIdx].y);
				for (int k = mB_DynamicGameObject2.vertIdx; k < num4; k++)
				{
					if (uv2s[k].x < num5)
					{
						num5 = uv2s[k].x;
					}
					if (uv2s[k].x > x)
					{
						x = uv2s[k].x;
					}
					if (uv2s[k].y < num6)
					{
						num6 = uv2s[k].y;
					}
					if (uv2s[k].y > y)
					{
						y = uv2s[k].y;
					}
				}
				array2[j] = new Rect(num5, num6, x - num5, y - num6);
				array[j] /= num2;
				Vector2 item = new Vector2(array2[j].width, array2[j].height) * (array[j] * 8192f);
				list.Add(item);
			}
			AtlasPackingResult atlasPackingResult = new MB2_TexturePackerRegular
			{
				atlasMustBePowerOfTwo = false
			}.GetRects(list, 8192, 8192, num)[0];
			Vector2 vector = default(Vector2);
			for (int l = 0; l < mbDynamicObjectsInCombinedMesh.Count; l++)
			{
				MB_DynamicGameObject mB_DynamicGameObject3 = mbDynamicObjectsInCombinedMesh[l];
				int num7 = mB_DynamicGameObject3.vertIdx + mB_DynamicGameObject3.numVerts;
				Rect rect = array2[l];
				Rect rect2 = atlasPackingResult.rects[l];
				for (int m = mB_DynamicGameObject3.vertIdx; m < num7; m++)
				{
					vector.x = (uv2s[m].x - rect.x) / rect.width * rect2.width + rect2.x;
					vector.y = (uv2s[m].y - rect.y) / rect.height * rect2.height + rect2.y;
					uv2s[m] = vector;
				}
				if (atlasPackingResult.atlasX == atlasPackingResult.atlasY)
				{
					continue;
				}
				if (atlasPackingResult.atlasX < atlasPackingResult.atlasY)
				{
					float num8 = (float)atlasPackingResult.atlasX / (float)atlasPackingResult.atlasY;
					for (int n = mB_DynamicGameObject3.vertIdx; n < num7; n++)
					{
						Vector2 vector2 = uv2s[n];
						vector2.x *= num8;
						uv2s[n] = vector2;
					}
				}
				else
				{
					float num9 = (float)atlasPackingResult.atlasY / (float)atlasPackingResult.atlasX;
					for (int num10 = mB_DynamicGameObject3.vertIdx; num10 < num7; num10++)
					{
						Vector2 vector3 = uv2s[num10];
						vector3.y *= num9;
						uv2s[num10] = vector3;
					}
				}
			}
		}

		private SerializableIntArray[] GetSubmeshTrisWithShowHideApplied(List<MB_DynamicGameObject> mbDynamicObjectsInCombinedMesh)
		{
			bool flag = false;
			for (int i = 0; i < mbDynamicObjectsInCombinedMesh.Count; i++)
			{
				if (!mbDynamicObjectsInCombinedMesh[i].show)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				int[] array = new int[submeshTris.Length];
				SerializableIntArray[] array2 = new SerializableIntArray[submeshTris.Length];
				for (int j = 0; j < mbDynamicObjectsInCombinedMesh.Count; j++)
				{
					MB_DynamicGameObject mB_DynamicGameObject = mbDynamicObjectsInCombinedMesh[j];
					if (mB_DynamicGameObject.show)
					{
						for (int k = 0; k < mB_DynamicGameObject.submeshNumTris.Length; k++)
						{
							array[k] += mB_DynamicGameObject.submeshNumTris[k];
						}
					}
				}
				for (int l = 0; l < array2.Length; l++)
				{
					array2[l] = new SerializableIntArray(array[l]);
				}
				int[] array3 = new int[array2.Length];
				for (int m = 0; m < mbDynamicObjectsInCombinedMesh.Count; m++)
				{
					MB_DynamicGameObject mB_DynamicGameObject2 = mbDynamicObjectsInCombinedMesh[m];
					if (!mB_DynamicGameObject2.show)
					{
						continue;
					}
					for (int n = 0; n < submeshTris.Length; n++)
					{
						int[] data = submeshTris[n].data;
						int num = mB_DynamicGameObject2.submeshTriIdxs[n];
						int num2 = num + mB_DynamicGameObject2.submeshNumTris[n];
						for (int num3 = num; num3 < num2; num3++)
						{
							array2[n].data[array3[n]] = data[num3];
							array3[n]++;
						}
					}
				}
				return array2;
			}
			return submeshTris;
		}

		public int[] GetTriangleSizes()
		{
			int[] array = new int[submeshTris.Length];
			for (int i = 0; i < submeshTris.Length; i++)
			{
				array[i] = submeshTris[i].data.Length;
			}
			return array;
		}

		private void _LocalToWorld(Transform t, bool doNorm, bool doTan, int destStartVertsIdx, Vector3[] dgoMeshVerts, Vector3[] dgoMeshNorms, Vector4[] dgoMeshTans, Vector3[] verticies, Vector3[] normals, Vector4[] tangents)
		{
			Vector3 lossyScale = t.lossyScale;
			if (lossyScale == Vector3.one)
			{
				_LocalToWorld_TR(t.rotation, t.position, doNorm, doTan, destStartVertsIdx, dgoMeshVerts, dgoMeshNorms, dgoMeshTans, verticies, normals, tangents);
			}
			else if (lossyScale.x > Mathf.Epsilon && lossyScale.y > Mathf.Epsilon && lossyScale.z > Mathf.Epsilon)
			{
				Matrix4x4 wld_X_local = t.localToWorldMatrix;
				_LocalToWorldMatrix_TRS(ref wld_X_local, doNorm, doTan, destStartVertsIdx, dgoMeshVerts, dgoMeshNorms, dgoMeshTans, verticies, normals, tangents);
			}
			else
			{
				_LocalToWorld_TRS(t.rotation, t.position, t.lossyScale, doNorm, doTan, destStartVertsIdx, dgoMeshVerts, dgoMeshNorms, dgoMeshTans, verticies, normals, tangents);
			}
		}

		private static void _LocalToWorldMatrix_TRS(ref Matrix4x4 wld_X_local, bool doNorm, bool doTan, int destStartVertsIdx, Vector3[] dgoMeshVerts, Vector3[] dgoMeshNorms, Vector4[] dgoMeshTans, Vector3[] verticies, Vector3[] normals, Vector4[] tangents)
		{
			Matrix4x4 matrix4x = Matrix4x4.zero;
			if (doNorm || doTan)
			{
				matrix4x = wld_X_local;
				float num2 = (matrix4x[2, 3] = 0f);
				float value = (matrix4x[1, 3] = num2);
				matrix4x[0, 3] = value;
				matrix4x = matrix4x.inverse.transpose;
			}
			for (int i = 0; i < dgoMeshVerts.Length; i++)
			{
				int num4 = destStartVertsIdx + i;
				verticies[num4] = wld_X_local.MultiplyPoint3x4(dgoMeshVerts[i]);
				if (doNorm)
				{
					normals[num4] = matrix4x.MultiplyPoint3x4(dgoMeshNorms[i]).normalized;
				}
				if (doTan)
				{
					float w = dgoMeshTans[i].w;
					Vector4 vector = matrix4x.MultiplyPoint3x4(dgoMeshTans[i]).normalized;
					vector.w = w;
					tangents[num4] = vector;
				}
			}
		}

		private static void _LocalToWorld_TR(Quaternion wld_Rot_local, Vector3 position_wld, bool doNorm, bool doTan, int destStartVertsIdx, Vector3[] dgoMeshVerts_local, Vector3[] dgoMeshNorms_local, Vector4[] dgoMeshTans_local, Vector3[] verticies, Vector3[] normals, Vector4[] tangents)
		{
			for (int i = 0; i < dgoMeshVerts_local.Length; i++)
			{
				int num = destStartVertsIdx + i;
				Vector3 vector = dgoMeshVerts_local[i];
				vector = wld_Rot_local * vector;
				vector += position_wld;
				verticies[num] = vector;
				if (doNorm)
				{
					Vector3 vector2 = dgoMeshNorms_local[i];
					vector2 = wld_Rot_local * vector2;
					normals[num] = vector2;
				}
				if (doTan)
				{
					Vector3 vector3 = dgoMeshTans_local[i];
					float w = dgoMeshTans_local[i].w;
					vector3 = wld_Rot_local * vector3;
					Vector4 vector4 = vector3;
					vector4.w = w;
					tangents[num] = vector4;
				}
			}
		}

		private static void _LocalToWorld_TRS(Quaternion wld_Rot_local, Vector3 position_wld, Vector3 scale, bool doNorm, bool doTan, int destStartVertsIdx, Vector3[] dgoMeshVerts_local, Vector3[] dgoMeshNorms_local, Vector4[] dgoMeshTans_local, Vector3[] verticies, Vector3[] normals, Vector4[] tangents)
		{
			Vector3 one = Vector3.one;
			if (doNorm || doTan)
			{
				one.x = ((scale.x < Mathf.Epsilon) ? 0f : (1f / scale.x));
				one.y = ((scale.y < Mathf.Epsilon) ? 0f : (1f / scale.y));
				one.z = ((scale.z < Mathf.Epsilon) ? 0f : (1f / scale.z));
			}
			for (int i = 0; i < dgoMeshVerts_local.Length; i++)
			{
				int num = destStartVertsIdx + i;
				Vector3 vector = dgoMeshVerts_local[i];
				vector.x *= scale.x;
				vector.y *= scale.y;
				vector.z *= scale.z;
				vector = wld_Rot_local * vector;
				vector += position_wld;
				verticies[num] = vector;
				if (doNorm)
				{
					Vector3 vector2 = dgoMeshNorms_local[i];
					vector2.x *= one.x;
					vector2.y *= one.y;
					vector2.z *= one.z;
					vector2 = wld_Rot_local * vector2;
					vector2.Normalize();
					normals[num] = vector2;
				}
				if (doTan)
				{
					Vector3 vector3 = dgoMeshTans_local[i];
					float w = dgoMeshTans_local[i].w;
					vector3.x *= one.x;
					vector3.y *= one.y;
					vector3.z *= one.z;
					vector3 = wld_Rot_local * vector3;
					vector3.Normalize();
					tangents[num] = new Vector4(vector3.x, vector3.y, vector3.z, w);
				}
			}
		}
	}

	public class MeshChannelsCache_NativeArray : IDisposable, IMeshChannelsCacheTaggingInterface
	{
		private MB2_LogLevel LOG_LEVEL;

		private MB2_LightmapOptions lightmapOption;

		protected Dictionary<int, MeshChannelsNativeArray> meshID2MeshChannels = new Dictionary<int, MeshChannelsNativeArray>();

		private bool _collectedMeshData;

		private bool _disposed;

		private Vector2 _HALF_UV = new Vector2(0.5f, 0.5f);

		public MeshChannelsCache_NativeArray(MB2_LogLevel ll, MB2_LightmapOptions lo)
		{
			LOG_LEVEL = ll;
			lightmapOption = lo;
		}

		public void Dispose()
		{
			Dispose(disposing: true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}
			foreach (MeshChannelsNativeArray value in meshID2MeshChannels.Values)
			{
				value.Dispose();
			}
			_collectedMeshData = false;
			_disposed = true;
		}

		public bool HasCollectedMeshData()
		{
			return _collectedMeshData;
		}

		public bool hasOutOfBoundsUVs(Mesh m, ref MB_Utility.MeshAnalysisResult mar, int submeshIdx)
		{
			return MB_Utility.hasOutOfBoundsUVs(GetUv0RawAsNativeArray(m), m, ref mar, submeshIdx);
		}

		internal NativeArray<Vector3> GetVerticiesAsNativeArray(Mesh m)
		{
			if (!meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out var value))
			{
				UnityEngine.Debug.LogError("Could not find mesh in the MeshChannelsCache." + m);
			}
			return value.vertcies_NativeArray;
		}

		internal NativeArray<Vector3> GetNormalsAsNativeArray(Mesh m)
		{
			if (!meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out var value))
			{
				UnityEngine.Debug.LogError("Could not find mesh in the MeshChannelsCache." + m);
			}
			return value.normals_NativeArray;
		}

		internal NativeArray<Vector4> GetTangentsAsNativeArray(Mesh m)
		{
			if (!meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out var value))
			{
				UnityEngine.Debug.LogError("Could not find mesh in the MeshChannelsCache." + m);
			}
			return value.tangents_NativeArray;
		}

		internal NativeArray<Vector2> GetUv0RawAsNativeArray(Mesh m)
		{
			meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out var value);
			return value.uv0raw_NativeArray;
		}

		internal NativeArray<Vector2> GetUv0ModifiedAsNativeArray(Mesh m)
		{
			meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out var value);
			if (!value.uv0modified_NativeArray.IsCreated)
			{
				value.uv0modified_NativeArray = new NativeArray<Vector2>(value.vertcies_NativeArray.Length, Allocator.Temp);
			}
			return value.uv0modified_NativeArray;
		}

		internal NativeArray<Vector2> GetUv2ModifiedAsNativeArray(Mesh m)
		{
			meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out var value);
			if (!value.uv2modified_NativeArray.IsCreated)
			{
				value.uv2modified_NativeArray = new NativeArray<Vector2>(value.vertcies_NativeArray.Length, Allocator.Temp);
			}
			return value.uv2modified_NativeArray;
		}

		internal NativeArray<Vector2> GetUVChannelAsNativeArray(int channel, Mesh m)
		{
			meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out var value);
			switch (channel)
			{
			case 0:
				return value.uv0raw_NativeArray;
			case 2:
				return value.uv2raw_NativeArray;
			case 3:
				return value.uv3_NativeArray;
			case 4:
				return value.uv4_NativeArray;
			case 5:
				return value.uv5_NativeArray;
			case 6:
				return value.uv6_NativeArray;
			case 7:
				return value.uv7_NativeArray;
			case 8:
				return value.uv8_NativeArray;
			default:
				UnityEngine.Debug.LogError("Error mesh channel " + channel + " not supported");
				return default(NativeArray<Vector2>);
			}
		}

		internal NativeArray<Color> GetColorsAsNativeArray(Mesh m)
		{
			meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out var value);
			return value.colors_NativeArray;
		}

		public void CollectChannelDataForAllMeshesInList(List<MB_DynamicGameObject> toUpdateDGOs, List<MB_DynamicGameObject> toAddDGOs, MB_MeshVertexChannelFlags newChannels, MB_RenderType renderType, bool doBlendShapes)
		{
			bool flag = (newChannels & MB_MeshVertexChannelFlags.vertex) == MB_MeshVertexChannelFlags.vertex;
			bool flag2 = (newChannels & MB_MeshVertexChannelFlags.normal) == MB_MeshVertexChannelFlags.normal;
			bool flag3 = (newChannels & MB_MeshVertexChannelFlags.tangent) == MB_MeshVertexChannelFlags.tangent;
			bool flag4 = (newChannels & MB_MeshVertexChannelFlags.uv0) == MB_MeshVertexChannelFlags.uv0;
			bool flag5 = (newChannels & MB_MeshVertexChannelFlags.uv2) == MB_MeshVertexChannelFlags.uv2;
			bool flag6 = (newChannels & MB_MeshVertexChannelFlags.uv3) == MB_MeshVertexChannelFlags.uv3;
			bool flag7 = (newChannels & MB_MeshVertexChannelFlags.uv4) == MB_MeshVertexChannelFlags.uv4;
			bool flag8 = (newChannels & MB_MeshVertexChannelFlags.uv5) == MB_MeshVertexChannelFlags.uv5;
			bool flag9 = (newChannels & MB_MeshVertexChannelFlags.uv6) == MB_MeshVertexChannelFlags.uv6;
			bool flag10 = (newChannels & MB_MeshVertexChannelFlags.uv7) == MB_MeshVertexChannelFlags.uv7;
			bool flag11 = (newChannels & MB_MeshVertexChannelFlags.uv8) == MB_MeshVertexChannelFlags.uv8;
			bool flag12 = (newChannels & MB_MeshVertexChannelFlags.colors) == MB_MeshVertexChannelFlags.colors;
			List<MB_DynamicGameObject> list = new List<MB_DynamicGameObject>();
			list.AddRange(toUpdateDGOs);
			list.AddRange(toAddDGOs);
			for (int i = 0; i < list.Count; i++)
			{
				MB_DynamicGameObject mB_DynamicGameObject = list[i];
				Mesh mesh = mB_DynamicGameObject._mesh;
				if (meshID2MeshChannels.ContainsKey(mesh.GetInstanceID()))
				{
					continue;
				}
				MeshChannelsNativeArray meshChannelsNativeArray = new MeshChannelsNativeArray();
				meshID2MeshChannels.Add(mesh.GetInstanceID(), meshChannelsNativeArray);
				if (flag)
				{
					meshChannelsNativeArray.vertcies_NativeArray = new NativeArray<Vector3>(mesh.vertices, Allocator.Temp);
				}
				if (flag4)
				{
					meshChannelsNativeArray.uv0raw_NativeArray = new NativeArray<Vector2>(_getMeshUVs(mesh), Allocator.Temp);
				}
				if (flag5)
				{
					meshChannelsNativeArray.uv2raw_NativeArray = new NativeArray<Vector2>(_getMeshUV2s(mesh, ref meshChannelsNativeArray.uv2modified_NativeArray), Allocator.Temp);
				}
				if (flag2)
				{
					meshChannelsNativeArray.normals_NativeArray = new NativeArray<Vector3>(_getMeshNormals(mesh), Allocator.Temp);
				}
				if (flag3)
				{
					meshChannelsNativeArray.tangents_NativeArray = new NativeArray<Vector4>(_getMeshTangents(mesh), Allocator.Temp);
				}
				if (flag6)
				{
					meshChannelsNativeArray.uv3_NativeArray = new NativeArray<Vector2>(MBVersion.GetMeshChannel(3, mesh, LOG_LEVEL), Allocator.Temp);
				}
				if (flag7)
				{
					meshChannelsNativeArray.uv4_NativeArray = new NativeArray<Vector2>(MBVersion.GetMeshChannel(4, mesh, LOG_LEVEL), Allocator.Temp);
				}
				if (flag8)
				{
					meshChannelsNativeArray.uv5_NativeArray = new NativeArray<Vector2>(MBVersion.GetMeshChannel(5, mesh, LOG_LEVEL), Allocator.Temp);
				}
				if (flag9)
				{
					meshChannelsNativeArray.uv6_NativeArray = new NativeArray<Vector2>(MBVersion.GetMeshChannel(6, mesh, LOG_LEVEL), Allocator.Temp);
				}
				if (flag10)
				{
					meshChannelsNativeArray.uv7_NativeArray = new NativeArray<Vector2>(MBVersion.GetMeshChannel(7, mesh, LOG_LEVEL), Allocator.Temp);
				}
				if (flag11)
				{
					meshChannelsNativeArray.uv8_NativeArray = new NativeArray<Vector2>(MBVersion.GetMeshChannel(8, mesh, LOG_LEVEL), Allocator.Temp);
				}
				if (flag12)
				{
					meshChannelsNativeArray.colors_NativeArray = new NativeArray<Color>(_getMeshColors(mesh), Allocator.Temp);
				}
				if (renderType != MB_RenderType.skinnedMeshRenderer)
				{
					continue;
				}
				bool isSkinnedMeshWithBones = false;
				Renderer renderer = mB_DynamicGameObject._renderer;
				if (meshChannelsNativeArray.bindPoses == null || meshChannelsNativeArray.bindPoses.Count == 0)
				{
					_getBindPoses(renderer, meshChannelsNativeArray.bindPoses, out isSkinnedMeshWithBones);
					_getBoneWeightData(ref meshChannelsNativeArray.boneWeightData, renderer, meshChannelsNativeArray.bindPoses.Count, isSkinnedMeshWithBones);
				}
				if (!doBlendShapes)
				{
					continue;
				}
				MBBlendShape[] array = new MBBlendShape[mesh.blendShapeCount];
				int vertexCount = mesh.vertexCount;
				for (int j = 0; j < array.Length; j++)
				{
					MBBlendShape mBBlendShape = (array[j] = new MBBlendShape());
					mBBlendShape.frames = new MBBlendShapeFrame[MBVersion.GetBlendShapeFrameCount(mesh, j)];
					mBBlendShape.name = mesh.GetBlendShapeName(j);
					mBBlendShape.indexInSource = j;
					mBBlendShape.gameObject = mB_DynamicGameObject.gameObject;
					for (int k = 0; k < mBBlendShape.frames.Length; k++)
					{
						MBBlendShapeFrame mBBlendShapeFrame = (mBBlendShape.frames[k] = new MBBlendShapeFrame());
						mBBlendShapeFrame.frameWeight = MBVersion.GetBlendShapeFrameWeight(mesh, j, k);
						mBBlendShapeFrame.vertices = new Vector3[vertexCount];
						mBBlendShapeFrame.normals = new Vector3[vertexCount];
						mBBlendShapeFrame.tangents = new Vector3[vertexCount];
						MBVersion.GetBlendShapeFrameVertices(mesh, j, k, mBBlendShapeFrame.vertices, mBBlendShapeFrame.normals, mBBlendShapeFrame.tangents);
					}
				}
				meshChannelsNativeArray.blendShapes = array;
			}
			_collectedMeshData = true;
		}

		internal List<Matrix4x4> GetBindposes(Renderer r, out bool isSkinnedMeshWithBones)
		{
			Mesh mesh = MB_Utility.GetMesh(r.gameObject);
			meshID2MeshChannels.TryGetValue(mesh.GetInstanceID(), out var value);
			if (r is SkinnedMeshRenderer && value.bindPoses.Count > 0)
			{
				isSkinnedMeshWithBones = true;
			}
			else
			{
				isSkinnedMeshWithBones = false;
				_ = r is SkinnedMeshRenderer;
			}
			return value.bindPoses;
		}

		internal BoneWeightDataForMesh GetBoneWeightData(Renderer r, int numbones, bool isSkinnedMeshWithBones)
		{
			Mesh mesh = MB_Utility.GetMesh(r.gameObject);
			meshID2MeshChannels.TryGetValue(mesh.GetInstanceID(), out var value);
			return value.boneWeightData;
		}

		public MBBlendShape[] GetBlendShapes(Mesh m, int gameObjectID, GameObject gameObject)
		{
			meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out var value);
			MBBlendShape[] array = new MBBlendShape[value.blendShapes.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = new MBBlendShape();
				array[i].name = value.blendShapes[i].name;
				array[i].indexInSource = value.blendShapes[i].indexInSource;
				array[i].frames = value.blendShapes[i].frames;
				array[i].gameObject = gameObject;
			}
			return array;
		}

		private Color[] _getMeshColors(Mesh m)
		{
			Color[] array = m.colors;
			if (array.Length == 0)
			{
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					MB2_Log.LogDebug("Mesh " + m?.ToString() + " has no colors. Generating");
				}
				if (LOG_LEVEL >= MB2_LogLevel.warn)
				{
					UnityEngine.Debug.LogWarning("Mesh " + m?.ToString() + " didn't have colors. Generating an array of white colors");
				}
				array = new Color[m.vertexCount];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = Color.white;
				}
			}
			return array;
		}

		private Vector3[] _getMeshNormals(Mesh m)
		{
			Vector3[] normals = m.normals;
			if (normals.Length == 0)
			{
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					MB2_Log.LogDebug("Mesh " + m?.ToString() + " has no normals. Generating");
				}
				if (LOG_LEVEL >= MB2_LogLevel.warn)
				{
					UnityEngine.Debug.LogWarning("Mesh " + m?.ToString() + " didn't have normals. Generating normals.");
				}
				Mesh mesh = UnityEngine.Object.Instantiate(m);
				mesh.RecalculateNormals();
				normals = mesh.normals;
				MB_Utility.Destroy(mesh);
			}
			return normals;
		}

		private Vector4[] _getMeshTangents(Mesh m)
		{
			Vector4[] array = m.tangents;
			if (array.Length == 0)
			{
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					MB2_Log.LogDebug("Mesh " + m?.ToString() + " has no tangents. Generating");
				}
				if (LOG_LEVEL >= MB2_LogLevel.warn)
				{
					UnityEngine.Debug.LogWarning("Mesh " + m?.ToString() + " didn't have tangents. Generating tangents.");
				}
				Vector3[] vertices = m.vertices;
				NativeArray<Vector2> uv0Raw = GetUv0Raw(m);
				Vector3[] normals = _getMeshNormals(m);
				array = new Vector4[m.vertexCount];
				for (int i = 0; i < m.subMeshCount; i++)
				{
					int[] triangles = m.GetTriangles(i);
					_generateTangents(triangles, vertices, uv0Raw, normals, array);
				}
			}
			return array;
		}

		private Vector2[] _getMeshUVs(Mesh m)
		{
			Vector2[] array = m.uv;
			if (array.Length == 0)
			{
				array = new Vector2[m.vertexCount];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = _HALF_UV;
				}
			}
			return array;
		}

		private Vector2[] _getMeshUV2s(Mesh m, ref NativeArray<Vector2> uv2modified)
		{
			Vector2[] uv = m.uv2;
			if (uv.Length == 0)
			{
				uv2modified = new NativeArray<Vector2>(m.vertexCount, Allocator.TempJob);
				for (int i = 0; i < uv2modified.Length; i++)
				{
					uv2modified[i] = _HALF_UV;
				}
			}
			return uv;
		}

		private static void _getBindPoses(Renderer r, List<Matrix4x4> poses, out bool isSkinnedMeshWithBones)
		{
			poses.Clear();
			isSkinnedMeshWithBones = r is SkinnedMeshRenderer;
			if (r is SkinnedMeshRenderer)
			{
				Mesh mesh = MB_Utility.GetMesh(r.gameObject);
				mesh.GetBindposes(poses);
				if (poses.Count == 0)
				{
					if (mesh.blendShapeCount > 0)
					{
						isSkinnedMeshWithBones = false;
					}
					else
					{
						UnityEngine.Debug.LogError("Skinned mesh " + r?.ToString() + " had no bindposes AND no blend shapes");
					}
				}
			}
			if (r is MeshRenderer || (r is SkinnedMeshRenderer && !isSkinnedMeshWithBones))
			{
				poses.Clear();
				poses.Add(Matrix4x4.identity);
			}
			if (poses == null || poses.Count == 0)
			{
				UnityEngine.Debug.LogError("Could not _getBindPoses. Object does not have a renderer");
			}
		}

		private static void _getBoneWeightData(ref BoneWeightDataForMesh bwd, Renderer r, int numBones, bool isSkinnedMeshWithBones)
		{
			if (isSkinnedMeshWithBones)
			{
				Mesh sharedMesh = ((SkinnedMeshRenderer)r).sharedMesh;
				bwd.initialized = true;
				bwd.weMustDispose = false;
				bwd.bonesPerVertex = sharedMesh.GetBonesPerVertex();
				bwd.boneWeights = sharedMesh.GetAllBoneWeights();
			}
			else if (r is MeshRenderer || (r is SkinnedMeshRenderer && !isSkinnedMeshWithBones))
			{
				Mesh mesh = MB_Utility.GetMesh(r.gameObject);
				bwd.initialized = true;
				bwd.weMustDispose = true;
				bwd.boneWeights = new NativeArray<BoneWeight1>(mesh.vertexCount, Allocator.Temp);
				bwd.bonesPerVertex = new NativeArray<byte>(mesh.vertexCount, Allocator.Temp);
				BoneWeight1 value = default(BoneWeight1);
				value.boneIndex = 0;
				value.weight = 1f;
				for (int i = 0; i < mesh.vertexCount; i++)
				{
					bwd.bonesPerVertex[i] = 1;
					bwd.boneWeights[i] = value;
				}
			}
			else
			{
				UnityEngine.Debug.LogError("Could not _getBoneWeights. Object does not have a renderer");
			}
			bwd.UsedBoneIdxsInSrcMesh = new bool[numBones];
			for (int j = 0; j < bwd.boneWeights.Length; j++)
			{
				bwd.UsedBoneIdxsInSrcMesh[bwd.boneWeights[j].boneIndex] = true;
			}
			bwd.numUsedbones = 0;
			for (int k = 0; k < bwd.UsedBoneIdxsInSrcMesh.Length; k++)
			{
				if (bwd.UsedBoneIdxsInSrcMesh[k])
				{
					bwd.numUsedbones++;
				}
			}
		}

		internal NativeArray<Vector2> GetUv0Raw(Mesh m)
		{
			meshID2MeshChannels.TryGetValue(m.GetInstanceID(), out var value);
			return value.uv0raw_NativeArray;
		}

		private static BoneWeight[] _getBoneWeights(Renderer r, int numVertsInMeshBeingAdded, bool isSkinnedMeshWithBones)
		{
			if (isSkinnedMeshWithBones)
			{
				return ((SkinnedMeshRenderer)r).sharedMesh.boneWeights;
			}
			if (r is MeshRenderer || (r is SkinnedMeshRenderer && !isSkinnedMeshWithBones))
			{
				BoneWeight boneWeight = default(BoneWeight);
				int num2 = (boneWeight.boneIndex3 = 0);
				int num4 = (boneWeight.boneIndex2 = num2);
				int boneIndex = (boneWeight.boneIndex1 = num4);
				boneWeight.boneIndex0 = boneIndex;
				boneWeight.weight0 = 1f;
				float num7 = (boneWeight.weight3 = 0f);
				float weight = (boneWeight.weight2 = num7);
				boneWeight.weight1 = weight;
				BoneWeight[] array = new BoneWeight[numVertsInMeshBeingAdded];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = boneWeight;
				}
				return array;
			}
			UnityEngine.Debug.LogError("Could not _getBoneWeights. Object does not have a renderer");
			return null;
		}

		private void _generateTangents(int[] triangles, Vector3[] verts, NativeArray<Vector2> uvs, Vector3[] normals, Vector4[] outTangents)
		{
			int num = triangles.Length;
			int num2 = verts.Length;
			Vector3[] array = new Vector3[num2];
			Vector3[] array2 = new Vector3[num2];
			for (int i = 0; i < num; i += 3)
			{
				int num3 = triangles[i];
				int num4 = triangles[i + 1];
				int num5 = triangles[i + 2];
				Vector3 vector = verts[num3];
				Vector3 vector2 = verts[num4];
				Vector3 vector3 = verts[num5];
				Vector2 vector4 = uvs[num3];
				Vector2 vector5 = uvs[num4];
				Vector2 vector6 = uvs[num5];
				float num6 = vector2.x - vector.x;
				float num7 = vector3.x - vector.x;
				float num8 = vector2.y - vector.y;
				float num9 = vector3.y - vector.y;
				float num10 = vector2.z - vector.z;
				float num11 = vector3.z - vector.z;
				float num12 = vector5.x - vector4.x;
				float num13 = vector6.x - vector4.x;
				float num14 = vector5.y - vector4.y;
				float num15 = vector6.y - vector4.y;
				float num16 = num12 * num15 - num13 * num14;
				if (num16 == 0f)
				{
					UnityEngine.Debug.LogError("Could not compute tangents. All UVs need to form a valid triangles in UV space. If any UV triangles are collapsed, tangents cannot be generated.");
					return;
				}
				float num17 = 1f / num16;
				Vector3 vector7 = new Vector3((num15 * num6 - num14 * num7) * num17, (num15 * num8 - num14 * num9) * num17, (num15 * num10 - num14 * num11) * num17);
				Vector3 vector8 = new Vector3((num12 * num7 - num13 * num6) * num17, (num12 * num9 - num13 * num8) * num17, (num12 * num11 - num13 * num10) * num17);
				array[num3] += vector7;
				array[num4] += vector7;
				array[num5] += vector7;
				array2[num3] += vector8;
				array2[num4] += vector8;
				array2[num5] += vector8;
			}
			for (int j = 0; j < num2; j++)
			{
				Vector3 vector9 = normals[j];
				Vector3 vector10 = array[j];
				Vector3 normalized = (vector10 - vector9 * Vector3.Dot(vector9, vector10)).normalized;
				outTangents[j] = new Vector4(normalized.x, normalized.y, normalized.z);
				outTangents[j].w = ((Vector3.Dot(Vector3.Cross(vector9, vector10), array2[j]) < 0f) ? (-1f) : 1f);
			}
		}
	}

	public class MeshChannelsNativeArray : IDisposable
	{
		private bool _disposed;

		public NativeArray<Vector3> vertcies_NativeArray;

		public NativeArray<Vector3> normals_NativeArray;

		public NativeArray<Vector4> tangents_NativeArray;

		public NativeArray<Color> colors_NativeArray;

		public NativeArray<Vector2> uv0raw_NativeArray;

		public NativeArray<Vector2> uv0modified_NativeArray;

		public NativeArray<Vector2> uv2raw_NativeArray;

		public NativeArray<Vector2> uv2modified_NativeArray;

		public NativeArray<Vector2> uv3_NativeArray;

		public NativeArray<Vector2> uv4_NativeArray;

		public NativeArray<Vector2> uv5_NativeArray;

		public NativeArray<Vector2> uv6_NativeArray;

		public NativeArray<Vector2> uv7_NativeArray;

		public NativeArray<Vector2> uv8_NativeArray;

		public List<Matrix4x4> bindPoses = new List<Matrix4x4>(128);

		public BoneWeightDataForMesh boneWeightData;

		public MBBlendShape[] blendShapes;

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
				_disposed = true;
				boneWeightData.Dispose();
				if (vertcies_NativeArray.IsCreated)
				{
					vertcies_NativeArray.Dispose();
				}
				if (normals_NativeArray.IsCreated)
				{
					normals_NativeArray.Dispose();
				}
				if (tangents_NativeArray.IsCreated)
				{
					tangents_NativeArray.Dispose();
				}
				if (colors_NativeArray.IsCreated)
				{
					colors_NativeArray.Dispose();
				}
				if (uv0raw_NativeArray.IsCreated)
				{
					uv0raw_NativeArray.Dispose();
				}
				if (uv0modified_NativeArray.IsCreated)
				{
					uv0modified_NativeArray.Dispose();
				}
				if (uv2raw_NativeArray.IsCreated)
				{
					uv2raw_NativeArray.Dispose();
				}
				if (uv2modified_NativeArray.IsCreated)
				{
					uv2modified_NativeArray.Dispose();
				}
				if (uv3_NativeArray.IsCreated)
				{
					uv3_NativeArray.Dispose();
				}
				if (uv4_NativeArray.IsCreated)
				{
					uv4_NativeArray.Dispose();
				}
				if (uv5_NativeArray.IsCreated)
				{
					uv5_NativeArray.Dispose();
				}
				if (uv6_NativeArray.IsCreated)
				{
					uv6_NativeArray.Dispose();
				}
				if (uv7_NativeArray.IsCreated)
				{
					uv7_NativeArray.Dispose();
				}
				if (uv8_NativeArray.IsCreated)
				{
					uv8_NativeArray.Dispose();
				}
			}
		}
	}

	public struct MB_MeshCombinerSingle_MeshNativeArrayHelper
	{
		public struct SIZER_4
		{
			public unsafe fixed byte data[4];
		}

		public struct SIZER_8
		{
			public unsafe fixed byte data[8];
		}

		public struct SIZER_12
		{
			public unsafe fixed byte data[12];
		}

		public struct SIZER_16
		{
			public unsafe fixed byte data[16];
		}

		public struct SIZER_20
		{
			public unsafe fixed byte data[20];
		}

		public struct SIZER_24
		{
			public unsafe fixed byte data[24];
		}

		public struct SIZER_28
		{
			public unsafe fixed byte data[28];
		}

		public struct SIZER_32
		{
			public unsafe fixed byte data[32];
		}

		public struct SIZER_36
		{
			public unsafe fixed byte data[36];
		}

		public struct SIZER_40
		{
			public unsafe fixed byte data[40];
		}

		public struct SIZER_44
		{
			public unsafe fixed byte data[44];
		}

		public struct SIZER_48
		{
			public unsafe fixed byte data[48];
		}

		public struct SIZER_52
		{
			public unsafe fixed byte data[52];
		}

		public struct SIZER_56
		{
			public unsafe fixed byte data[56];
		}

		public struct SIZER_60
		{
			public unsafe fixed byte data[60];
		}

		public struct SIZER_64
		{
			public unsafe fixed byte data[64];
		}

		public struct SIZER_68
		{
			public unsafe fixed byte data[68];
		}

		public struct SIZER_72
		{
			public unsafe fixed byte data[72];
		}

		public struct SIZER_76
		{
			public unsafe fixed byte data[72];
		}

		public struct SIZER_80
		{
			public unsafe fixed byte data[80];
		}

		public struct SIZER_84
		{
			public unsafe fixed byte data[84];
		}

		public struct SIZER_88
		{
			public unsafe fixed byte data[88];
		}

		public struct SIZER_92
		{
			public unsafe fixed byte data[92];
		}

		public struct SIZER_96
		{
			public unsafe fixed byte data[96];
		}

		public struct SIZER_100
		{
			public unsafe fixed byte data[100];
		}

		public struct SIZER_104
		{
			public unsafe fixed byte data[104];
		}

		public struct SIZER_108
		{
			public unsafe fixed byte data[108];
		}

		public struct SIZER_112
		{
			public unsafe fixed byte data[112];
		}

		public struct SIZER_116
		{
			public unsafe fixed byte data[116];
		}

		public struct SIZER_120
		{
			public unsafe fixed byte data[120];
		}

		public struct SIZER_124
		{
			public unsafe fixed byte data[124];
		}

		public struct SIZER_128
		{
			public unsafe fixed byte data[128];
		}

		public struct SIZER_132
		{
			public unsafe fixed byte data[132];
		}

		public struct SIZER_136
		{
			public unsafe fixed byte data[136];
		}

		public struct SIZER_140
		{
			public unsafe fixed byte data[140];
		}

		public struct SIZER_144
		{
			public unsafe fixed byte data[144];
		}

		public struct SIZER_148
		{
			public unsafe fixed byte data[148];
		}

		public struct SIZER_152
		{
			public unsafe fixed byte data[152];
		}

		private static Type[] _TypeForStride;

		public Mesh.MeshDataArray dataArray;

		public Mesh.MeshData data;

		public int vertexCount;

		[Preserve]
		public void _ENSURE_IL2CPP_CREATES_NECESSARY_CODE(ref Mesh.MeshData m)
		{
			UnityEngine.Debug.LogError("This should never be called directly. It is only here to ensure these methodes are generated by the il2cpp compiler and not stripped so that they can be found by reflection.");
			NativeArray<SIZER_4> vertexData = m.GetVertexData<SIZER_4>();
			NativeSlice<SIZER_4> nativeSlice = new NativeSlice<SIZER_4>(vertexData);
			nativeSlice.SliceWithStride<Vector2>(0);
			nativeSlice.SliceWithStride<Vector3>(0);
			nativeSlice.SliceWithStride<Vector4>(0);
			nativeSlice.SliceWithStride<Color32>(0);
			NativeArray<SIZER_8> vertexData2 = m.GetVertexData<SIZER_8>();
			NativeSlice<SIZER_8> nativeSlice2 = new NativeSlice<SIZER_8>(vertexData2);
			nativeSlice2.SliceWithStride<Vector2>(0);
			nativeSlice2.SliceWithStride<Vector3>(0);
			nativeSlice2.SliceWithStride<Vector4>(0);
			nativeSlice2.SliceWithStride<Color32>(0);
			NativeArray<SIZER_12> vertexData3 = m.GetVertexData<SIZER_12>();
			NativeSlice<SIZER_12> nativeSlice3 = new NativeSlice<SIZER_12>(vertexData3);
			nativeSlice3.SliceWithStride<Vector2>(0);
			nativeSlice3.SliceWithStride<Vector3>(0);
			nativeSlice3.SliceWithStride<Vector4>(0);
			nativeSlice3.SliceWithStride<Color32>(0);
			NativeArray<SIZER_16> vertexData4 = m.GetVertexData<SIZER_16>();
			NativeSlice<SIZER_16> nativeSlice4 = new NativeSlice<SIZER_16>(vertexData4);
			nativeSlice4.SliceWithStride<Vector2>(0);
			nativeSlice4.SliceWithStride<Vector3>(0);
			nativeSlice4.SliceWithStride<Vector4>(0);
			nativeSlice4.SliceWithStride<Color32>(0);
			NativeArray<SIZER_20> vertexData5 = m.GetVertexData<SIZER_20>();
			NativeSlice<SIZER_20> nativeSlice5 = new NativeSlice<SIZER_20>(vertexData5);
			nativeSlice5.SliceWithStride<Vector2>(0);
			nativeSlice5.SliceWithStride<Vector3>(0);
			nativeSlice5.SliceWithStride<Vector4>(0);
			nativeSlice5.SliceWithStride<Color32>(0);
			NativeArray<SIZER_24> vertexData6 = m.GetVertexData<SIZER_24>();
			NativeSlice<SIZER_24> nativeSlice6 = new NativeSlice<SIZER_24>(vertexData6);
			nativeSlice6.SliceWithStride<Vector2>(0);
			nativeSlice6.SliceWithStride<Vector3>(0);
			nativeSlice6.SliceWithStride<Vector4>(0);
			nativeSlice6.SliceWithStride<Color32>(0);
			NativeArray<SIZER_28> vertexData7 = m.GetVertexData<SIZER_28>();
			NativeSlice<SIZER_28> nativeSlice7 = new NativeSlice<SIZER_28>(vertexData7);
			nativeSlice7.SliceWithStride<Vector2>(0);
			nativeSlice7.SliceWithStride<Vector3>(0);
			nativeSlice7.SliceWithStride<Vector4>(0);
			nativeSlice7.SliceWithStride<Color32>(0);
			NativeArray<SIZER_32> vertexData8 = m.GetVertexData<SIZER_32>();
			NativeSlice<SIZER_32> nativeSlice8 = new NativeSlice<SIZER_32>(vertexData8);
			nativeSlice8.SliceWithStride<Vector2>(0);
			nativeSlice8.SliceWithStride<Vector3>(0);
			nativeSlice8.SliceWithStride<Vector4>(0);
			nativeSlice8.SliceWithStride<Color32>(0);
			NativeArray<SIZER_36> vertexData9 = m.GetVertexData<SIZER_36>();
			NativeSlice<SIZER_36> nativeSlice9 = new NativeSlice<SIZER_36>(vertexData9);
			nativeSlice9.SliceWithStride<Vector2>(0);
			nativeSlice9.SliceWithStride<Vector3>(0);
			nativeSlice9.SliceWithStride<Vector4>(0);
			nativeSlice9.SliceWithStride<Color32>(0);
			NativeArray<SIZER_40> vertexData10 = m.GetVertexData<SIZER_40>();
			NativeSlice<SIZER_40> nativeSlice10 = new NativeSlice<SIZER_40>(vertexData10);
			nativeSlice10.SliceWithStride<Vector2>(0);
			nativeSlice10.SliceWithStride<Vector3>(0);
			nativeSlice10.SliceWithStride<Vector4>(0);
			nativeSlice10.SliceWithStride<Color32>(0);
			NativeArray<SIZER_44> vertexData11 = m.GetVertexData<SIZER_44>();
			NativeSlice<SIZER_44> nativeSlice11 = new NativeSlice<SIZER_44>(vertexData11);
			nativeSlice11.SliceWithStride<Vector2>(0);
			nativeSlice11.SliceWithStride<Vector3>(0);
			nativeSlice11.SliceWithStride<Vector4>(0);
			nativeSlice11.SliceWithStride<Color32>(0);
			NativeArray<SIZER_48> vertexData12 = m.GetVertexData<SIZER_48>();
			NativeSlice<SIZER_48> nativeSlice12 = new NativeSlice<SIZER_48>(vertexData12);
			nativeSlice12.SliceWithStride<Vector2>(0);
			nativeSlice12.SliceWithStride<Vector3>(0);
			nativeSlice12.SliceWithStride<Vector4>(0);
			nativeSlice12.SliceWithStride<Color32>(0);
			NativeArray<SIZER_52> vertexData13 = m.GetVertexData<SIZER_52>();
			NativeSlice<SIZER_52> nativeSlice13 = new NativeSlice<SIZER_52>(vertexData13);
			nativeSlice13.SliceWithStride<Vector2>(0);
			nativeSlice13.SliceWithStride<Vector3>(0);
			nativeSlice13.SliceWithStride<Vector4>(0);
			nativeSlice13.SliceWithStride<Color32>(0);
			NativeArray<SIZER_56> vertexData14 = m.GetVertexData<SIZER_56>();
			NativeSlice<SIZER_56> nativeSlice14 = new NativeSlice<SIZER_56>(vertexData14);
			nativeSlice14.SliceWithStride<Vector2>(0);
			nativeSlice14.SliceWithStride<Vector3>(0);
			nativeSlice14.SliceWithStride<Vector4>(0);
			nativeSlice14.SliceWithStride<Color32>(0);
			NativeArray<SIZER_60> vertexData15 = m.GetVertexData<SIZER_60>();
			NativeSlice<SIZER_60> nativeSlice15 = new NativeSlice<SIZER_60>(vertexData15);
			nativeSlice15.SliceWithStride<Vector2>(0);
			nativeSlice15.SliceWithStride<Vector3>(0);
			nativeSlice15.SliceWithStride<Vector4>(0);
			nativeSlice15.SliceWithStride<Color32>(0);
			NativeArray<SIZER_64> vertexData16 = m.GetVertexData<SIZER_64>();
			NativeSlice<SIZER_64> nativeSlice16 = new NativeSlice<SIZER_64>(vertexData16);
			nativeSlice16.SliceWithStride<Vector2>(0);
			nativeSlice16.SliceWithStride<Vector3>(0);
			nativeSlice16.SliceWithStride<Vector4>(0);
			nativeSlice16.SliceWithStride<Color32>(0);
			NativeArray<SIZER_68> vertexData17 = m.GetVertexData<SIZER_68>();
			NativeSlice<SIZER_68> nativeSlice17 = new NativeSlice<SIZER_68>(vertexData17);
			nativeSlice17.SliceWithStride<Vector2>(0);
			nativeSlice17.SliceWithStride<Vector3>(0);
			nativeSlice17.SliceWithStride<Vector4>(0);
			nativeSlice17.SliceWithStride<Color32>(0);
			NativeArray<SIZER_72> vertexData18 = m.GetVertexData<SIZER_72>();
			NativeSlice<SIZER_72> nativeSlice18 = new NativeSlice<SIZER_72>(vertexData18);
			nativeSlice18.SliceWithStride<Vector2>(0);
			nativeSlice18.SliceWithStride<Vector3>(0);
			nativeSlice18.SliceWithStride<Vector4>(0);
			nativeSlice18.SliceWithStride<Color32>(0);
			NativeArray<SIZER_76> vertexData19 = m.GetVertexData<SIZER_76>();
			NativeSlice<SIZER_76> nativeSlice19 = new NativeSlice<SIZER_76>(vertexData19);
			nativeSlice19.SliceWithStride<Vector2>(0);
			nativeSlice19.SliceWithStride<Vector3>(0);
			nativeSlice19.SliceWithStride<Vector4>(0);
			nativeSlice19.SliceWithStride<Color32>(0);
			NativeArray<SIZER_80> vertexData20 = m.GetVertexData<SIZER_80>();
			NativeSlice<SIZER_80> nativeSlice20 = new NativeSlice<SIZER_80>(vertexData20);
			nativeSlice20.SliceWithStride<Vector2>(0);
			nativeSlice20.SliceWithStride<Vector3>(0);
			nativeSlice20.SliceWithStride<Vector4>(0);
			nativeSlice20.SliceWithStride<Color32>(0);
			NativeArray<SIZER_84> vertexData21 = m.GetVertexData<SIZER_84>();
			NativeSlice<SIZER_84> nativeSlice21 = new NativeSlice<SIZER_84>(vertexData21);
			nativeSlice21.SliceWithStride<Vector2>(0);
			nativeSlice21.SliceWithStride<Vector3>(0);
			nativeSlice21.SliceWithStride<Vector4>(0);
			nativeSlice21.SliceWithStride<Color32>(0);
			NativeArray<SIZER_88> vertexData22 = m.GetVertexData<SIZER_88>();
			NativeSlice<SIZER_88> nativeSlice22 = new NativeSlice<SIZER_88>(vertexData22);
			nativeSlice22.SliceWithStride<Vector2>(0);
			nativeSlice22.SliceWithStride<Vector3>(0);
			nativeSlice22.SliceWithStride<Vector4>(0);
			nativeSlice22.SliceWithStride<Color32>(0);
			NativeArray<SIZER_92> vertexData23 = m.GetVertexData<SIZER_92>();
			NativeSlice<SIZER_92> nativeSlice23 = new NativeSlice<SIZER_92>(vertexData23);
			nativeSlice23.SliceWithStride<Vector2>(0);
			nativeSlice23.SliceWithStride<Vector3>(0);
			nativeSlice23.SliceWithStride<Vector4>(0);
			nativeSlice23.SliceWithStride<Color32>(0);
			NativeArray<SIZER_96> vertexData24 = m.GetVertexData<SIZER_96>();
			NativeSlice<SIZER_96> nativeSlice24 = new NativeSlice<SIZER_96>(vertexData24);
			nativeSlice24.SliceWithStride<Vector2>(0);
			nativeSlice24.SliceWithStride<Vector3>(0);
			nativeSlice24.SliceWithStride<Vector4>(0);
			nativeSlice24.SliceWithStride<Color32>(0);
			NativeArray<SIZER_100> vertexData25 = m.GetVertexData<SIZER_100>();
			NativeSlice<SIZER_100> nativeSlice25 = new NativeSlice<SIZER_100>(vertexData25);
			nativeSlice25.SliceWithStride<Vector2>(0);
			nativeSlice25.SliceWithStride<Vector3>(0);
			nativeSlice25.SliceWithStride<Vector4>(0);
			nativeSlice25.SliceWithStride<Color32>(0);
			NativeArray<SIZER_104> vertexData26 = m.GetVertexData<SIZER_104>();
			NativeSlice<SIZER_104> nativeSlice26 = new NativeSlice<SIZER_104>(vertexData26);
			nativeSlice26.SliceWithStride<Vector2>(0);
			nativeSlice26.SliceWithStride<Vector3>(0);
			nativeSlice26.SliceWithStride<Vector4>(0);
			nativeSlice26.SliceWithStride<Color32>(0);
			NativeArray<SIZER_108> vertexData27 = m.GetVertexData<SIZER_108>();
			NativeSlice<SIZER_108> nativeSlice27 = new NativeSlice<SIZER_108>(vertexData27);
			nativeSlice27.SliceWithStride<Vector2>(0);
			nativeSlice27.SliceWithStride<Vector3>(0);
			nativeSlice27.SliceWithStride<Vector4>(0);
			nativeSlice27.SliceWithStride<Color32>(0);
			NativeArray<SIZER_112> vertexData28 = m.GetVertexData<SIZER_112>();
			NativeSlice<SIZER_112> nativeSlice28 = new NativeSlice<SIZER_112>(vertexData28);
			nativeSlice28.SliceWithStride<Vector2>(0);
			nativeSlice28.SliceWithStride<Vector3>(0);
			nativeSlice28.SliceWithStride<Vector4>(0);
			nativeSlice28.SliceWithStride<Color32>(0);
			NativeArray<SIZER_116> vertexData29 = m.GetVertexData<SIZER_116>();
			NativeSlice<SIZER_116> nativeSlice29 = new NativeSlice<SIZER_116>(vertexData29);
			nativeSlice29.SliceWithStride<Vector2>(0);
			nativeSlice29.SliceWithStride<Vector3>(0);
			nativeSlice29.SliceWithStride<Vector4>(0);
			nativeSlice29.SliceWithStride<Color32>(0);
			NativeArray<SIZER_120> vertexData30 = m.GetVertexData<SIZER_120>();
			NativeSlice<SIZER_120> nativeSlice30 = new NativeSlice<SIZER_120>(vertexData30);
			nativeSlice30.SliceWithStride<Vector2>(0);
			nativeSlice30.SliceWithStride<Vector3>(0);
			nativeSlice30.SliceWithStride<Vector4>(0);
			nativeSlice30.SliceWithStride<Color32>(0);
			NativeArray<SIZER_124> vertexData31 = m.GetVertexData<SIZER_124>();
			NativeSlice<SIZER_124> nativeSlice31 = new NativeSlice<SIZER_124>(vertexData31);
			nativeSlice31.SliceWithStride<Vector2>(0);
			nativeSlice31.SliceWithStride<Vector3>(0);
			nativeSlice31.SliceWithStride<Vector4>(0);
			nativeSlice31.SliceWithStride<Color32>(0);
			NativeArray<SIZER_128> vertexData32 = m.GetVertexData<SIZER_128>();
			NativeSlice<SIZER_128> nativeSlice32 = new NativeSlice<SIZER_128>(vertexData32);
			nativeSlice32.SliceWithStride<Vector2>(0);
			nativeSlice32.SliceWithStride<Vector3>(0);
			nativeSlice32.SliceWithStride<Vector4>(0);
			nativeSlice32.SliceWithStride<Color32>(0);
			NativeArray<SIZER_132> vertexData33 = m.GetVertexData<SIZER_132>();
			NativeSlice<SIZER_132> nativeSlice33 = new NativeSlice<SIZER_132>(vertexData33);
			nativeSlice33.SliceWithStride<Vector2>(0);
			nativeSlice33.SliceWithStride<Vector3>(0);
			nativeSlice33.SliceWithStride<Vector4>(0);
			nativeSlice33.SliceWithStride<Color32>(0);
			NativeArray<SIZER_136> vertexData34 = m.GetVertexData<SIZER_136>();
			NativeSlice<SIZER_136> nativeSlice34 = new NativeSlice<SIZER_136>(vertexData34);
			nativeSlice34.SliceWithStride<Vector2>(0);
			nativeSlice34.SliceWithStride<Vector3>(0);
			nativeSlice34.SliceWithStride<Vector4>(0);
			nativeSlice34.SliceWithStride<Color32>(0);
			NativeArray<SIZER_140> vertexData35 = m.GetVertexData<SIZER_140>();
			NativeSlice<SIZER_140> nativeSlice35 = new NativeSlice<SIZER_140>(vertexData35);
			nativeSlice35.SliceWithStride<Vector2>(0);
			nativeSlice35.SliceWithStride<Vector3>(0);
			nativeSlice35.SliceWithStride<Vector4>(0);
			nativeSlice35.SliceWithStride<Color32>(0);
			NativeArray<SIZER_144> vertexData36 = m.GetVertexData<SIZER_144>();
			NativeSlice<SIZER_144> nativeSlice36 = new NativeSlice<SIZER_144>(vertexData36);
			nativeSlice36.SliceWithStride<Vector2>(0);
			nativeSlice36.SliceWithStride<Vector3>(0);
			nativeSlice36.SliceWithStride<Vector4>(0);
			nativeSlice36.SliceWithStride<Color32>(0);
			NativeArray<SIZER_148> vertexData37 = m.GetVertexData<SIZER_148>();
			NativeSlice<SIZER_148> nativeSlice37 = new NativeSlice<SIZER_148>(vertexData37);
			nativeSlice37.SliceWithStride<Vector2>(0);
			nativeSlice37.SliceWithStride<Vector3>(0);
			nativeSlice37.SliceWithStride<Vector4>(0);
			nativeSlice37.SliceWithStride<Color32>(0);
			NativeArray<SIZER_152> vertexData38 = m.GetVertexData<SIZER_152>();
			NativeSlice<SIZER_152> nativeSlice38 = new NativeSlice<SIZER_152>(vertexData38);
			nativeSlice38.SliceWithStride<Vector2>(0);
			nativeSlice38.SliceWithStride<Vector3>(0);
			nativeSlice38.SliceWithStride<Vector4>(0);
			nativeSlice38.SliceWithStride<Color32>(0);
		}

		public static int CalcStride(MB_MeshVertexChannelFlags channels, int uvChannelWithExtraParameter, out int strideVertexBuffer, out int strideUVbuffer)
		{
			strideVertexBuffer = 0;
			strideUVbuffer = 0;
			if ((channels & MB_MeshVertexChannelFlags.vertex) == MB_MeshVertexChannelFlags.vertex)
			{
				strideVertexBuffer += 12;
			}
			if ((channels & MB_MeshVertexChannelFlags.normal) == MB_MeshVertexChannelFlags.normal)
			{
				strideVertexBuffer += 12;
			}
			if ((channels & MB_MeshVertexChannelFlags.tangent) == MB_MeshVertexChannelFlags.tangent)
			{
				strideVertexBuffer += 16;
			}
			if ((channels & MB_MeshVertexChannelFlags.colors) == MB_MeshVertexChannelFlags.colors)
			{
				strideUVbuffer += 16;
			}
			if ((channels & MB_MeshVertexChannelFlags.uv0) == MB_MeshVertexChannelFlags.uv0)
			{
				strideUVbuffer += 8;
			}
			if ((channels & MB_MeshVertexChannelFlags.uv2) == MB_MeshVertexChannelFlags.uv2)
			{
				strideUVbuffer += 8;
			}
			if ((channels & MB_MeshVertexChannelFlags.uv3) == MB_MeshVertexChannelFlags.uv3)
			{
				strideUVbuffer += 8;
			}
			if ((channels & MB_MeshVertexChannelFlags.uv4) == MB_MeshVertexChannelFlags.uv4)
			{
				strideUVbuffer += 8;
			}
			if ((channels & MB_MeshVertexChannelFlags.uv5) == MB_MeshVertexChannelFlags.uv5)
			{
				strideUVbuffer += 8;
			}
			if ((channels & MB_MeshVertexChannelFlags.uv6) == MB_MeshVertexChannelFlags.uv6)
			{
				strideUVbuffer += 8;
			}
			if ((channels & MB_MeshVertexChannelFlags.uv7) == MB_MeshVertexChannelFlags.uv7)
			{
				strideUVbuffer += 8;
			}
			if ((channels & MB_MeshVertexChannelFlags.uv8) == MB_MeshVertexChannelFlags.uv8)
			{
				strideUVbuffer += 8;
			}
			_ = channels & MB_MeshVertexChannelFlags.blendWeight;
			_ = 8192;
			if (uvChannelWithExtraParameter >= 0)
			{
				strideUVbuffer += 4;
			}
			return strideVertexBuffer + strideUVbuffer;
		}

		public static void Init(MB_MeshVertexChannelFlags channels, VertexAttributeDescriptor[] vertexAttributes, ref VertexAndTriangleProcessorNativeArray nativeSlices, int vertexCount, int[] submeshCount, int uvChannelWithExtraParameter)
		{
			CalcStride(channels, uvChannelWithExtraParameter, out var strideVertexBuffer, out var strideUVbuffer);
			int num = 0;
			int num2 = 0;
			int stream = 1;
			int stream2 = 2;
			int num3 = 0;
			num2 = num3;
			num++;
			num3++;
			if (strideUVbuffer > 0)
			{
				stream = num3;
				num3++;
				num++;
			}
			int num4 = 0;
			if ((channels & MB_MeshVertexChannelFlags.vertex) == MB_MeshVertexChannelFlags.vertex)
			{
				vertexAttributes[num4] = new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, num2);
				num4++;
			}
			if ((channels & MB_MeshVertexChannelFlags.normal) == MB_MeshVertexChannelFlags.normal)
			{
				vertexAttributes[num4] = new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3, num2);
				num4++;
			}
			if ((channels & MB_MeshVertexChannelFlags.tangent) == MB_MeshVertexChannelFlags.tangent)
			{
				vertexAttributes[num4] = new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4, num2);
				num4++;
			}
			if ((channels & MB_MeshVertexChannelFlags.colors) == MB_MeshVertexChannelFlags.colors)
			{
				vertexAttributes[num4] = new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 4, stream);
				num4++;
			}
			if ((channels & MB_MeshVertexChannelFlags.uv0) == MB_MeshVertexChannelFlags.uv0)
			{
				int dimension = ((uvChannelWithExtraParameter == 0) ? 3 : 2);
				vertexAttributes[num4] = new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, dimension, stream);
				num4++;
			}
			if ((channels & MB_MeshVertexChannelFlags.uv2) == MB_MeshVertexChannelFlags.uv2)
			{
				int dimension2 = ((uvChannelWithExtraParameter == 1) ? 3 : 2);
				vertexAttributes[num4] = new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, dimension2, stream);
				num4++;
			}
			if ((channels & MB_MeshVertexChannelFlags.uv3) == MB_MeshVertexChannelFlags.uv3)
			{
				int dimension3 = ((uvChannelWithExtraParameter == 2) ? 3 : 2);
				vertexAttributes[num4] = new VertexAttributeDescriptor(VertexAttribute.TexCoord2, VertexAttributeFormat.Float32, dimension3, stream);
				num4++;
			}
			if ((channels & MB_MeshVertexChannelFlags.uv4) == MB_MeshVertexChannelFlags.uv4)
			{
				int dimension4 = ((uvChannelWithExtraParameter == 3) ? 3 : 2);
				vertexAttributes[num4] = new VertexAttributeDescriptor(VertexAttribute.TexCoord3, VertexAttributeFormat.Float32, dimension4, stream);
				num4++;
			}
			if ((channels & MB_MeshVertexChannelFlags.uv5) == MB_MeshVertexChannelFlags.uv5)
			{
				int dimension5 = ((uvChannelWithExtraParameter == 4) ? 3 : 2);
				vertexAttributes[num4] = new VertexAttributeDescriptor(VertexAttribute.TexCoord4, VertexAttributeFormat.Float32, dimension5, stream);
				num4++;
			}
			if ((channels & MB_MeshVertexChannelFlags.uv6) == MB_MeshVertexChannelFlags.uv6)
			{
				int dimension6 = ((uvChannelWithExtraParameter == 5) ? 3 : 2);
				vertexAttributes[num4] = new VertexAttributeDescriptor(VertexAttribute.TexCoord5, VertexAttributeFormat.Float32, dimension6, stream);
				num4++;
			}
			if ((channels & MB_MeshVertexChannelFlags.uv7) == MB_MeshVertexChannelFlags.uv7)
			{
				int dimension7 = ((uvChannelWithExtraParameter == 6) ? 3 : 2);
				vertexAttributes[num4] = new VertexAttributeDescriptor(VertexAttribute.TexCoord6, VertexAttributeFormat.Float32, dimension7, stream);
				num4++;
			}
			if ((channels & MB_MeshVertexChannelFlags.uv8) == MB_MeshVertexChannelFlags.uv8)
			{
				int dimension8 = ((uvChannelWithExtraParameter == 7) ? 3 : 2);
				vertexAttributes[num4] = new VertexAttributeDescriptor(VertexAttribute.TexCoord7, VertexAttributeFormat.Float32, dimension8, stream);
				num4++;
			}
			if ((channels & MB_MeshVertexChannelFlags.blendWeight) == MB_MeshVertexChannelFlags.blendWeight)
			{
				vertexAttributes[num4] = new VertexAttributeDescriptor(VertexAttribute.BlendWeight, VertexAttributeFormat.UNorm16, 4, stream2);
				num4++;
				vertexAttributes[num4] = new VertexAttributeDescriptor(VertexAttribute.BlendIndices, VertexAttributeFormat.UInt16, 4, stream2);
				num4++;
			}
			AllocateWriteableMeshData(ref nativeSlices, vertexAttributes, vertexCount, num);
			SetupNativeSlices(ref nativeSlices, strideVertexBuffer, strideUVbuffer, uvChannelWithExtraParameter);
			nativeSlices.triangleBuffer = nativeSlices.data.GetIndexData<ushort>();
		}

		public static void AllocateWriteableMeshData(ref VertexAndTriangleProcessorNativeArray nativeSlices, VertexAttributeDescriptor[] channels, int vertexCount, int numBuffers)
		{
			nativeSlices.dataArray = Mesh.AllocateWritableMeshData(1);
			nativeSlices.dataArrayAllocated = true;
			nativeSlices.data = nativeSlices.dataArray[0];
			if (nativeSlices.LOG_LEVEL >= MB2_LogLevel.debug)
			{
				string text = "Allocating VertexChannels for combined mesh: ";
				for (int i = 0; i < channels.Length; i++)
				{
					string text2 = text;
					VertexAttributeDescriptor vertexAttributeDescriptor = channels[i];
					text = text2 + "\n   " + vertexAttributeDescriptor.ToString();
				}
				UnityEngine.Debug.Log(text);
			}
			nativeSlices.data.SetVertexBufferParams(vertexCount, channels);
		}

		public unsafe static void SetupNativeSlices(ref VertexAndTriangleProcessorNativeArray nativeSlices, int strideVertexData, int strideUVdata, int uvChannelWithExtraParameter)
		{
			ref Mesh.MeshData reference = ref nativeSlices.data;
			nativeSlices.bufferStride_0 = strideVertexData;
			nativeSlices.bufferStride_1 = strideUVdata;
			int num = 0;
			Type type = (nativeSlices.rawSliceSizerType_0 = _TypeForStride[strideVertexData]);
			object obj = reference.GetType().GetMethod("GetVertexData", new Type[1] { typeof(int) }).MakeGenericMethod(type)
				.Invoke(reference, new object[1] { num });
			Type type2 = typeof(NativeSlice<>).MakeGenericType(type);
			nativeSlices.rawSliceVertexStream_0 = Activator.CreateInstance(type2, obj);
			int num2 = (int)nativeSlices.rawSliceVertexStream_0.GetType().GetProperty("Length").GetValue(nativeSlices.rawSliceVertexStream_0, null);
			nativeSlices.vertexCount = num2;
			MethodInfo method = nativeSlices.rawSliceVertexStream_0.GetType().GetMethod("SliceWithStride", new Type[1] { typeof(int) });
			int num3 = 0;
			if ((nativeSlices.channels & MB_MeshVertexChannelFlags.vertex) == MB_MeshVertexChannelFlags.vertex)
			{
				MethodInfo methodInfo = method.MakeGenericMethod(typeof(Vector3));
				nativeSlices.verticies = (NativeSlice<Vector3>)methodInfo.Invoke(nativeSlices.rawSliceVertexStream_0, new object[1] { num3 });
				num3 += sizeof(Vector3);
			}
			if ((nativeSlices.channels & MB_MeshVertexChannelFlags.normal) == MB_MeshVertexChannelFlags.normal)
			{
				MethodInfo methodInfo2 = method.MakeGenericMethod(typeof(Vector3));
				nativeSlices.normals = (NativeSlice<Vector3>)methodInfo2.Invoke(nativeSlices.rawSliceVertexStream_0, new object[1] { num3 });
				num3 += sizeof(Vector3);
			}
			if ((nativeSlices.channels & MB_MeshVertexChannelFlags.tangent) == MB_MeshVertexChannelFlags.tangent)
			{
				MethodInfo methodInfo3 = method.MakeGenericMethod(typeof(Vector4));
				nativeSlices.tangents = (NativeSlice<Vector4>)methodInfo3.Invoke(nativeSlices.rawSliceVertexStream_0, new object[1] { num3 });
				num3 += sizeof(Vector4);
			}
			if (strideUVdata <= 0)
			{
				return;
			}
			num++;
			Type type3 = (nativeSlices.rawSliceSizerType_1 = _TypeForStride[strideUVdata]);
			object obj2 = reference.GetType().GetMethod("GetVertexData", new Type[1] { typeof(int) }).MakeGenericMethod(type3)
				.Invoke(reference, new object[1] { num });
			Type type4 = typeof(NativeSlice<>).MakeGenericType(type3);
			nativeSlices.rawSliceVertexStream_1 = Activator.CreateInstance(type4, obj2);
			MethodInfo method2 = nativeSlices.rawSliceVertexStream_1.GetType().GetMethod("SliceWithStride", new Type[1] { typeof(int) });
			int num4 = 0;
			if ((nativeSlices.channels & MB_MeshVertexChannelFlags.colors) == MB_MeshVertexChannelFlags.colors)
			{
				MethodInfo methodInfo4 = method2.MakeGenericMethod(typeof(Color));
				nativeSlices.colors = (NativeSlice<Color>)methodInfo4.Invoke(nativeSlices.rawSliceVertexStream_1, new object[1] { num4 });
				num4 += sizeof(Color);
			}
			if ((nativeSlices.channels & MB_MeshVertexChannelFlags.uv0) == MB_MeshVertexChannelFlags.uv0)
			{
				MethodInfo methodInfo5 = method2.MakeGenericMethod(typeof(Vector2));
				nativeSlices.uv0s = (NativeSlice<Vector2>)methodInfo5.Invoke(nativeSlices.rawSliceVertexStream_1, new object[1] { num4 });
				if (uvChannelWithExtraParameter == 0)
				{
					methodInfo5 = method2.MakeGenericMethod(typeof(Vector3));
					nativeSlices.uvsWithExtraIndex = (NativeSlice<Vector3>)methodInfo5.Invoke(nativeSlices.rawSliceVertexStream_1, new object[1] { num4 });
					methodInfo5 = method2.MakeGenericMethod(typeof(float));
					num4 += sizeof(Vector2);
					nativeSlices.uvsSliceIdx = (NativeSlice<float>)methodInfo5.Invoke(nativeSlices.rawSliceVertexStream_1, new object[1] { num4 });
					num4 += 4;
				}
				else
				{
					num4 += sizeof(Vector2);
				}
			}
			if ((nativeSlices.channels & MB_MeshVertexChannelFlags.uv2) == MB_MeshVertexChannelFlags.uv2)
			{
				MethodInfo methodInfo6 = method2.MakeGenericMethod(typeof(Vector2));
				nativeSlices.uv2s = (NativeSlice<Vector2>)methodInfo6.Invoke(nativeSlices.rawSliceVertexStream_1, new object[1] { num4 });
				if (uvChannelWithExtraParameter == 1)
				{
					methodInfo6 = method2.MakeGenericMethod(typeof(Vector3));
					nativeSlices.uvsWithExtraIndex = (NativeSlice<Vector3>)methodInfo6.Invoke(nativeSlices.rawSliceVertexStream_1, new object[1] { num4 });
					methodInfo6 = method2.MakeGenericMethod(typeof(float));
					num4 += sizeof(Vector2);
					nativeSlices.uvsSliceIdx = (NativeSlice<float>)methodInfo6.Invoke(nativeSlices.rawSliceVertexStream_1, new object[1] { num4 });
					num4 += 4;
				}
				else
				{
					num4 += sizeof(Vector2);
				}
			}
			if ((nativeSlices.channels & MB_MeshVertexChannelFlags.uv3) == MB_MeshVertexChannelFlags.uv3)
			{
				MethodInfo methodInfo7 = method2.MakeGenericMethod(typeof(Vector2));
				nativeSlices.uv3s = (NativeSlice<Vector2>)methodInfo7.Invoke(nativeSlices.rawSliceVertexStream_1, new object[1] { num4 });
				if (uvChannelWithExtraParameter == 2)
				{
					methodInfo7 = method2.MakeGenericMethod(typeof(Vector3));
					nativeSlices.uvsWithExtraIndex = (NativeSlice<Vector3>)methodInfo7.Invoke(nativeSlices.rawSliceVertexStream_1, new object[1] { num4 });
					methodInfo7 = method2.MakeGenericMethod(typeof(float));
					num4 += sizeof(Vector2);
					nativeSlices.uvsSliceIdx = (NativeSlice<float>)methodInfo7.Invoke(nativeSlices.rawSliceVertexStream_1, new object[1] { num4 });
					num4 += 4;
				}
				else
				{
					num4 += sizeof(Vector2);
				}
			}
			if ((nativeSlices.channels & MB_MeshVertexChannelFlags.uv4) == MB_MeshVertexChannelFlags.uv4)
			{
				MethodInfo methodInfo8 = method2.MakeGenericMethod(typeof(Vector2));
				nativeSlices.uv4s = (NativeSlice<Vector2>)methodInfo8.Invoke(nativeSlices.rawSliceVertexStream_1, new object[1] { num4 });
				if (uvChannelWithExtraParameter == 3)
				{
					methodInfo8 = method2.MakeGenericMethod(typeof(Vector3));
					nativeSlices.uvsWithExtraIndex = (NativeSlice<Vector3>)methodInfo8.Invoke(nativeSlices.rawSliceVertexStream_1, new object[1] { num4 });
					methodInfo8 = method2.MakeGenericMethod(typeof(float));
					num4 += sizeof(Vector2);
					nativeSlices.uvsSliceIdx = (NativeSlice<float>)methodInfo8.Invoke(nativeSlices.rawSliceVertexStream_1, new object[1] { num4 });
					num4 += 4;
				}
				else
				{
					num4 += sizeof(Vector2);
				}
			}
			if ((nativeSlices.channels & MB_MeshVertexChannelFlags.uv5) == MB_MeshVertexChannelFlags.uv5)
			{
				MethodInfo methodInfo9 = method2.MakeGenericMethod(typeof(Vector2));
				nativeSlices.uv5s = (NativeSlice<Vector2>)methodInfo9.Invoke(nativeSlices.rawSliceVertexStream_1, new object[1] { num4 });
				if (uvChannelWithExtraParameter == 4)
				{
					methodInfo9 = method2.MakeGenericMethod(typeof(Vector3));
					nativeSlices.uvsWithExtraIndex = (NativeSlice<Vector3>)methodInfo9.Invoke(nativeSlices.rawSliceVertexStream_1, new object[1] { num4 });
					methodInfo9 = method2.MakeGenericMethod(typeof(float));
					num4 += sizeof(Vector2);
					nativeSlices.uvsSliceIdx = (NativeSlice<float>)methodInfo9.Invoke(nativeSlices.rawSliceVertexStream_1, new object[1] { num4 });
					num4 += 4;
				}
				else
				{
					num4 += sizeof(Vector2);
				}
			}
			if ((nativeSlices.channels & MB_MeshVertexChannelFlags.uv6) == MB_MeshVertexChannelFlags.uv6)
			{
				MethodInfo methodInfo10 = method2.MakeGenericMethod(typeof(Vector2));
				nativeSlices.uv6s = (NativeSlice<Vector2>)methodInfo10.Invoke(nativeSlices.rawSliceVertexStream_1, new object[1] { num4 });
				if (uvChannelWithExtraParameter == 5)
				{
					methodInfo10 = method2.MakeGenericMethod(typeof(Vector3));
					nativeSlices.uvsWithExtraIndex = (NativeSlice<Vector3>)methodInfo10.Invoke(nativeSlices.rawSliceVertexStream_1, new object[1] { num4 });
					methodInfo10 = method2.MakeGenericMethod(typeof(float));
					num4 += sizeof(Vector2);
					nativeSlices.uvsSliceIdx = (NativeSlice<float>)methodInfo10.Invoke(nativeSlices.rawSliceVertexStream_1, new object[1] { num4 });
					num4 += 4;
				}
				else
				{
					num4 += sizeof(Vector2);
				}
			}
			if ((nativeSlices.channels & MB_MeshVertexChannelFlags.uv7) == MB_MeshVertexChannelFlags.uv7)
			{
				MethodInfo methodInfo11 = method2.MakeGenericMethod(typeof(Vector2));
				nativeSlices.uv7s = (NativeSlice<Vector2>)methodInfo11.Invoke(nativeSlices.rawSliceVertexStream_1, new object[1] { num4 });
				if (uvChannelWithExtraParameter == 6)
				{
					methodInfo11 = method2.MakeGenericMethod(typeof(Vector3));
					nativeSlices.uvsWithExtraIndex = (NativeSlice<Vector3>)methodInfo11.Invoke(nativeSlices.rawSliceVertexStream_1, new object[1] { num4 });
					methodInfo11 = method2.MakeGenericMethod(typeof(float));
					num4 += sizeof(Vector2);
					nativeSlices.uvsSliceIdx = (NativeSlice<float>)methodInfo11.Invoke(nativeSlices.rawSliceVertexStream_1, new object[1] { num4 });
					num4 += 4;
				}
				else
				{
					num4 += sizeof(Vector2);
				}
			}
			if ((nativeSlices.channels & MB_MeshVertexChannelFlags.uv8) == MB_MeshVertexChannelFlags.uv8)
			{
				MethodInfo methodInfo12 = method2.MakeGenericMethod(typeof(Vector2));
				nativeSlices.uv8s = (NativeSlice<Vector2>)methodInfo12.Invoke(nativeSlices.rawSliceVertexStream_1, new object[1] { num4 });
				if (uvChannelWithExtraParameter == 7)
				{
					methodInfo12 = method2.MakeGenericMethod(typeof(Vector3));
					nativeSlices.uvsWithExtraIndex = (NativeSlice<Vector3>)methodInfo12.Invoke(nativeSlices.rawSliceVertexStream_1, new object[1] { num4 });
					methodInfo12 = method2.MakeGenericMethod(typeof(float));
					num4 += sizeof(Vector2);
					nativeSlices.uvsSliceIdx = (NativeSlice<float>)methodInfo12.Invoke(nativeSlices.rawSliceVertexStream_1, new object[1] { num4 });
					num4 += 4;
				}
				else
				{
					num4 += sizeof(Vector2);
				}
			}
		}

		public static void NativeSliceCopyFrom(object toHereSlice, Type toHereSizerType, object fromHereSlice, Type fromHereSizerType)
		{
			Type type = typeof(NativeSlice<>).MakeGenericType(fromHereSizerType);
			type.GetMethod("CopyFrom", new Type[1] { type }).Invoke(toHereSlice, new object[1] { fromHereSlice });
		}

		public static void NativeSliceCopy<T>(NativeSlice<T> srcArray, int srcStartIdx, NativeSlice<T> destArray, int destStartIdx, int length) where T : struct
		{
			NativeSlice<T> slice = srcArray.Slice(srcStartIdx, length);
			destArray.Slice(destStartIdx, length).CopyFrom(slice);
		}

		public static void NativeSliceCopyTo<T>(NativeSlice<T> srcArray, NativeSlice<T> destArray, int destStartIdx) where T : struct
		{
			destArray.Slice(destStartIdx, srcArray.Length).CopyFrom(srcArray);
		}

		static MB_MeshCombinerSingle_MeshNativeArrayHelper()
		{
			Type[] array = new Type[153];
			array[4] = typeof(SIZER_4);
			array[8] = typeof(SIZER_8);
			array[12] = typeof(SIZER_12);
			array[16] = typeof(SIZER_16);
			array[20] = typeof(SIZER_20);
			array[24] = typeof(SIZER_24);
			array[28] = typeof(SIZER_28);
			array[32] = typeof(SIZER_32);
			array[36] = typeof(SIZER_36);
			array[40] = typeof(SIZER_40);
			array[44] = typeof(SIZER_44);
			array[48] = typeof(SIZER_48);
			array[52] = typeof(SIZER_52);
			array[56] = typeof(SIZER_56);
			array[60] = typeof(SIZER_60);
			array[64] = typeof(SIZER_64);
			array[68] = typeof(SIZER_68);
			array[72] = typeof(SIZER_72);
			array[76] = typeof(SIZER_76);
			array[80] = typeof(SIZER_80);
			array[84] = typeof(SIZER_84);
			array[88] = typeof(SIZER_88);
			array[92] = typeof(SIZER_92);
			array[96] = typeof(SIZER_96);
			array[100] = typeof(SIZER_100);
			array[104] = typeof(SIZER_104);
			array[108] = typeof(SIZER_108);
			array[112] = typeof(SIZER_112);
			array[116] = typeof(SIZER_116);
			array[120] = typeof(SIZER_120);
			array[124] = typeof(SIZER_124);
			array[128] = typeof(SIZER_128);
			array[132] = typeof(SIZER_132);
			array[136] = typeof(SIZER_136);
			array[140] = typeof(SIZER_140);
			array[144] = typeof(SIZER_144);
			array[148] = typeof(SIZER_148);
			array[152] = typeof(SIZER_152);
			_TypeForStride = array;
		}
	}

	public struct VertexAndTriangleProcessorNativeArray : IVertexAndTriangleProcessor, IDisposable
	{
		private bool _disposed;

		private bool _isInitialized;

		internal MB2_LogLevel LOG_LEVEL;

		internal VertexAttributeDescriptor[] vertexAttributes;

		internal bool dataArrayAllocated;

		internal Mesh.MeshDataArray dataArray;

		internal Mesh.MeshData data;

		internal int vertexCount;

		internal NativeArray<Vector3> verticiesModified;

		internal NativeSlice<Vector3> verticies;

		internal NativeSlice<Vector3> normals;

		internal NativeSlice<Vector4> tangents;

		internal NativeSlice<Color> colors;

		internal NativeSlice<Vector2> uv0s;

		internal NativeSlice<Vector2> uv2s;

		internal NativeSlice<Vector2> uv3s;

		internal NativeSlice<Vector2> uv4s;

		internal NativeSlice<Vector2> uv5s;

		internal NativeSlice<Vector2> uv6s;

		internal NativeSlice<Vector2> uv7s;

		internal NativeSlice<Vector2> uv8s;

		internal NativeSlice<float> uvsSliceIdx;

		internal NativeSlice<Vector3> uvsWithExtraIndex;

		private SerializableIntArray[] submeshTris;

		internal NativeArray<ushort> triangleBuffer;

		internal int bufferStride_0;

		internal int bufferStride_1;

		internal int bufferStride_2;

		internal Type rawSliceSizerType_0;

		internal Type rawSliceSizerType_1;

		internal object rawSliceVertexStream_0;

		internal object rawSliceVertexStream_1;

		public MB_MeshVertexChannelFlags channels { get; private set; }

		public void Dispose()
		{
			if (!_disposed)
			{
				_isInitialized = false;
				channels = MB_MeshVertexChannelFlags.none;
				if (dataArrayAllocated)
				{
					dataArray.Dispose();
					dataArrayAllocated = false;
				}
				if (verticiesModified.IsCreated)
				{
					verticiesModified.Dispose();
				}
				submeshTris = null;
				_disposed = true;
			}
		}

		public bool IsInitialized()
		{
			return _isInitialized;
		}

		public bool IsDisposed()
		{
			return _disposed;
		}

		public void Init(MB3_MeshCombinerSingle combiner, MB_MeshVertexChannelFlags newChannels, int vertexCount, int[] newSubmeshTrisSize, int uvChannelWithExtraParameter, IMeshChannelsCacheTaggingInterface meshChannelsCache, bool loadDataFromCombinedMesh, MB2_LogLevel logLevel)
		{
			channels = newChannels;
			LOG_LEVEL = logLevel;
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			if ((channels & MB_MeshVertexChannelFlags.vertex) == MB_MeshVertexChannelFlags.vertex)
			{
				num++;
			}
			if ((channels & MB_MeshVertexChannelFlags.normal) == MB_MeshVertexChannelFlags.normal)
			{
				num++;
			}
			if ((channels & MB_MeshVertexChannelFlags.tangent) == MB_MeshVertexChannelFlags.tangent)
			{
				num++;
			}
			if ((channels & MB_MeshVertexChannelFlags.colors) == MB_MeshVertexChannelFlags.colors)
			{
				num2++;
			}
			if ((channels & MB_MeshVertexChannelFlags.uv0) == MB_MeshVertexChannelFlags.uv0)
			{
				num2++;
			}
			if ((channels & MB_MeshVertexChannelFlags.uv2) == MB_MeshVertexChannelFlags.uv2)
			{
				num2++;
			}
			if ((channels & MB_MeshVertexChannelFlags.uv3) == MB_MeshVertexChannelFlags.uv3)
			{
				num2++;
			}
			if ((channels & MB_MeshVertexChannelFlags.uv4) == MB_MeshVertexChannelFlags.uv4)
			{
				num2++;
			}
			if ((channels & MB_MeshVertexChannelFlags.uv5) == MB_MeshVertexChannelFlags.uv5)
			{
				num2++;
			}
			if ((channels & MB_MeshVertexChannelFlags.uv6) == MB_MeshVertexChannelFlags.uv6)
			{
				num2++;
			}
			if ((channels & MB_MeshVertexChannelFlags.uv7) == MB_MeshVertexChannelFlags.uv7)
			{
				num2++;
			}
			if ((channels & MB_MeshVertexChannelFlags.uv8) == MB_MeshVertexChannelFlags.uv8)
			{
				num2++;
			}
			if ((channels & MB_MeshVertexChannelFlags.blendWeight) == MB_MeshVertexChannelFlags.blendWeight)
			{
				num3++;
			}
			if ((channels & MB_MeshVertexChannelFlags.blendIndices) == MB_MeshVertexChannelFlags.blendIndices)
			{
				num3++;
			}
			vertexAttributes = new VertexAttributeDescriptor[num + num2 + num3];
			MB_MeshCombinerSingle_MeshNativeArrayHelper.Init(channels, vertexAttributes, ref this, vertexCount, newSubmeshTrisSize, uvChannelWithExtraParameter);
			if (loadDataFromCombinedMesh)
			{
				submeshTris = combiner.submeshTris;
				VertexAndTriangleProcessorNativeArray vertexAndTriangleProcessorNativeArray = default(VertexAndTriangleProcessorNativeArray);
				vertexAndTriangleProcessorNativeArray.InitFromMeshCombiner(combiner, channels, -1);
				if (vertexAndTriangleProcessorNativeArray.bufferStride_0 > 0)
				{
					MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopyFrom(rawSliceVertexStream_0, rawSliceSizerType_0, vertexAndTriangleProcessorNativeArray.rawSliceVertexStream_0, vertexAndTriangleProcessorNativeArray.rawSliceSizerType_0);
				}
				if (vertexAndTriangleProcessorNativeArray.bufferStride_1 > 0)
				{
					MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopyFrom(rawSliceVertexStream_1, rawSliceSizerType_1, vertexAndTriangleProcessorNativeArray.rawSliceVertexStream_1, vertexAndTriangleProcessorNativeArray.rawSliceSizerType_1);
				}
				if (vertexAndTriangleProcessorNativeArray.data.indexFormat == IndexFormat.UInt16)
				{
					NativeArray<ushort> indexData = vertexAndTriangleProcessorNativeArray.data.GetIndexData<ushort>();
					data.SetIndexBufferParams(indexData.Length, IndexFormat.UInt16);
					data.GetIndexData<ushort>().CopyFrom(indexData);
					data.subMeshCount = vertexAndTriangleProcessorNativeArray.data.subMeshCount;
					for (int i = 0; i < data.subMeshCount; i++)
					{
						SubMeshDescriptor subMesh = vertexAndTriangleProcessorNativeArray.data.GetSubMesh(i);
						data.SetSubMesh(i, subMesh);
					}
				}
				else
				{
					NativeArray<uint> indexData2 = vertexAndTriangleProcessorNativeArray.data.GetIndexData<uint>();
					data.SetIndexBufferParams(indexData2.Length, IndexFormat.UInt32);
					data.GetIndexData<uint>().CopyFrom(indexData2);
					data.subMeshCount = vertexAndTriangleProcessorNativeArray.data.subMeshCount;
					for (int j = 0; j < data.subMeshCount; j++)
					{
						SubMeshDescriptor subMesh2 = vertexAndTriangleProcessorNativeArray.data.GetSubMesh(j);
						data.SetSubMesh(j, subMesh2);
					}
				}
				vertexAndTriangleProcessorNativeArray.Dispose();
			}
			else
			{
				submeshTris = new SerializableIntArray[newSubmeshTrisSize.Length];
				for (int k = 0; k < newSubmeshTrisSize.Length; k++)
				{
					submeshTris[k] = new SerializableIntArray(newSubmeshTrisSize[k]);
				}
			}
			_isInitialized = true;
		}

		public void InitShowHide(MB3_MeshCombinerSingle combiner)
		{
			channels = combiner.channelsLastBake;
			submeshTris = combiner.submeshTris;
			_isInitialized = true;
		}

		public void InitFromMeshCombiner(MB3_MeshCombinerSingle combiner, MB_MeshVertexChannelFlags newChannels, int uvChannelWithExtraParameter)
		{
			if (combiner.channelsLastBake != newChannels)
			{
				if (combiner.channelsLastBake == MB_MeshVertexChannelFlags.none && combiner.verts.Length != 0)
				{
					combiner.channelsLastBake = newChannels;
				}
				else
				{
					UnityEngine.Debug.LogError("Shouldn't change channels between bakes. \n" + combiner.channelsLastBake.ToString() + " \n" + newChannels);
				}
			}
			channels = combiner.channelsLastBake;
			dataArray = Mesh.AcquireReadOnlyMeshData(combiner._mesh);
			dataArrayAllocated = true;
			data = dataArray[0];
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				string text = "Vertex attributes in combined mesh: ";
				for (int i = 0; i < combiner._mesh.vertexAttributeCount; i++)
				{
					VertexAttributeDescriptor vertexAttribute = combiner._mesh.GetVertexAttribute(i);
					string[] obj = new string[5]
					{
						text,
						"\n    ",
						i.ToString(),
						"  VertexAttribute: ",
						null
					};
					VertexAttributeDescriptor vertexAttributeDescriptor = vertexAttribute;
					obj[4] = vertexAttributeDescriptor.ToString();
					text = string.Concat(obj);
				}
				UnityEngine.Debug.Log(text);
			}
			MB_MeshCombinerSingle_MeshNativeArrayHelper.CalcStride(channels, uvChannelWithExtraParameter, out var strideVertexBuffer, out var strideUVbuffer);
			MB_MeshCombinerSingle_MeshNativeArrayHelper.SetupNativeSlices(ref this, strideVertexBuffer, strideUVbuffer, uvChannelWithExtraParameter);
			if (combiner.bufferDataFromPrevious.meshVerticiesWereShifted)
			{
				verticiesModified = new NativeArray<Vector3>(verticies.Length, Allocator.Temp);
				Vector3 meshVerticesShift = combiner.bufferDataFromPrevious.meshVerticesShift;
				for (int j = 0; j < verticies.Length; j++)
				{
					verticiesModified[j] = verticies[j] + meshVerticesShift;
				}
				verticies = verticiesModified.Slice();
			}
			submeshTris = combiner.submeshTris;
			_isInitialized = true;
		}

		public void ApplyDataBufferToMesh(Mesh m)
		{
			data.subMeshCount = 1;
			data.SetSubMesh(0, new SubMeshDescriptor(0, triangleBuffer.Length));
			Mesh.ApplyAndDisposeWritableMeshData(dataArray, m);
			dataArrayAllocated = false;
			m.RecalculateBounds();
		}

		public int GetVertexCount()
		{
			return verticies.Length;
		}

		public int GetSubmeshCount()
		{
			return submeshTris.Length;
		}

		public void TransferOwnershipOfSerializableBuffersToCombiner(MB3_MeshCombinerSingle c, MB_MeshVertexChannelFlags channelsToTransfer, BufferDataFromPreviousBake serializableBufferData)
		{
			c.channelsLastBake = channels;
			c.bufferDataFromPrevious = serializableBufferData;
			c.submeshTris = submeshTris;
			submeshTris = null;
			_isInitialized = false;
		}

		public void CopyArraysFromPreviousBakeBuffersToNewBuffers(MB_DynamicGameObject dgo, ref IVertexAndTriangleProcessor iOldBuffers, int destStartVertIdx, int triangleIdxAdjustment, int[] targSubmeshTidx, MB2_LogLevel LOG_LEVEL)
		{
			VertexAndTriangleProcessorNativeArray vertexAndTriangleProcessorNativeArray = (VertexAndTriangleProcessorNativeArray)(object)iOldBuffers;
			int vertIdx = dgo.vertIdx;
			int numVerts = dgo.numVerts;
			if ((channels & MB_MeshVertexChannelFlags.vertex) == MB_MeshVertexChannelFlags.vertex)
			{
				MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopy(vertexAndTriangleProcessorNativeArray.verticies, vertIdx, verticies, destStartVertIdx, numVerts);
			}
			if ((channels & MB_MeshVertexChannelFlags.normal) == MB_MeshVertexChannelFlags.normal)
			{
				MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopy(vertexAndTriangleProcessorNativeArray.normals, vertIdx, normals, destStartVertIdx, numVerts);
			}
			if ((channels & MB_MeshVertexChannelFlags.tangent) == MB_MeshVertexChannelFlags.tangent)
			{
				MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopy(vertexAndTriangleProcessorNativeArray.tangents, vertIdx, tangents, destStartVertIdx, numVerts);
			}
			if ((channels & MB_MeshVertexChannelFlags.uv0) == MB_MeshVertexChannelFlags.uv0)
			{
				MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopy(vertexAndTriangleProcessorNativeArray.uv0s, vertIdx, uv0s, destStartVertIdx, numVerts);
			}
			if ((channels & MB_MeshVertexChannelFlags.nuvsSliceIdx) == MB_MeshVertexChannelFlags.nuvsSliceIdx)
			{
				MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopy(vertexAndTriangleProcessorNativeArray.uvsSliceIdx, vertIdx, uvsSliceIdx, destStartVertIdx, numVerts);
			}
			if ((channels & MB_MeshVertexChannelFlags.uv2) == MB_MeshVertexChannelFlags.uv2)
			{
				MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopy(vertexAndTriangleProcessorNativeArray.uv2s, vertIdx, uv2s, destStartVertIdx, numVerts);
			}
			if ((channels & MB_MeshVertexChannelFlags.uv3) == MB_MeshVertexChannelFlags.uv3)
			{
				MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopy(vertexAndTriangleProcessorNativeArray.uv3s, vertIdx, uv3s, destStartVertIdx, numVerts);
			}
			if ((channels & MB_MeshVertexChannelFlags.uv4) == MB_MeshVertexChannelFlags.uv4)
			{
				MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopy(vertexAndTriangleProcessorNativeArray.uv4s, vertIdx, uv4s, destStartVertIdx, numVerts);
			}
			if ((channels & MB_MeshVertexChannelFlags.uv5) == MB_MeshVertexChannelFlags.uv5)
			{
				MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopy(vertexAndTriangleProcessorNativeArray.uv5s, vertIdx, uv5s, destStartVertIdx, numVerts);
			}
			if ((channels & MB_MeshVertexChannelFlags.uv6) == MB_MeshVertexChannelFlags.uv6)
			{
				MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopy(vertexAndTriangleProcessorNativeArray.uv6s, vertIdx, uv6s, destStartVertIdx, numVerts);
			}
			if ((channels & MB_MeshVertexChannelFlags.uv7) == MB_MeshVertexChannelFlags.uv7)
			{
				MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopy(vertexAndTriangleProcessorNativeArray.uv7s, vertIdx, uv7s, destStartVertIdx, numVerts);
			}
			if ((channels & MB_MeshVertexChannelFlags.uv8) == MB_MeshVertexChannelFlags.uv8)
			{
				MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopy(vertexAndTriangleProcessorNativeArray.uv8s, vertIdx, uv8s, destStartVertIdx, numVerts);
			}
			if ((channels & MB_MeshVertexChannelFlags.colors) == MB_MeshVertexChannelFlags.colors)
			{
				MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopy(vertexAndTriangleProcessorNativeArray.colors, vertIdx, colors, destStartVertIdx, numVerts);
			}
			for (int i = 0; i < submeshTris.Length; i++)
			{
				int[] array = vertexAndTriangleProcessorNativeArray.submeshTris[i].data;
				int num = dgo.submeshTriIdxs[i];
				int num2 = dgo.submeshNumTris[i];
				if (LOG_LEVEL >= MB2_LogLevel.debug)
				{
					MB2_Log.LogDebug("    Adjusting submesh triangles submesh:" + i + " startIdx:" + num + " num:" + num2 + " nsubmeshTris:" + submeshTris.Length + " targSubmeshTidx:" + targSubmeshTidx.Length, LOG_LEVEL);
				}
				for (int j = num; j < num + num2; j++)
				{
					array[j] -= triangleIdxAdjustment;
				}
				Array.Copy(array, num, submeshTris[i].data, targSubmeshTidx[i], num2);
			}
		}

		public void CopyFromDGOMeshToBuffers(MB_DynamicGameObject dgo, int destStartVertsIdx, MB_MeshVertexChannelFlags channelsToUpdate, bool updateTris, bool updateBWdata, MB_IMeshBakerSettings settings, MB_IMeshCombinerSingle_BoneProcessor boneProcessor, int[] targSubmeshTidx, MB2_TextureBakeResults textureBakeResults, UVAdjuster_Atlas uvAdjuster, MB2_LogLevel LOG_LEVEL, IMeshChannelsCacheTaggingInterface meshChannelCacheParam)
		{
			MeshChannelsCache_NativeArray meshChannelsCache_NativeArray = (MeshChannelsCache_NativeArray)meshChannelCacheParam;
			bool flag = (channels & MB_MeshVertexChannelFlags.vertex) == MB_MeshVertexChannelFlags.vertex && (channelsToUpdate & MB_MeshVertexChannelFlags.vertex) == MB_MeshVertexChannelFlags.vertex;
			bool flag2 = (channels & MB_MeshVertexChannelFlags.normal) == MB_MeshVertexChannelFlags.normal && (channelsToUpdate & MB_MeshVertexChannelFlags.normal) == MB_MeshVertexChannelFlags.normal;
			bool flag3 = (channels & MB_MeshVertexChannelFlags.tangent) == MB_MeshVertexChannelFlags.tangent && (channelsToUpdate & MB_MeshVertexChannelFlags.tangent) == MB_MeshVertexChannelFlags.tangent;
			if (flag || flag2 || flag3)
			{
				NativeArray<Vector3> nativeArray = default(NativeArray<Vector3>);
				NativeArray<Vector3> nativeArray2 = default(NativeArray<Vector3>);
				NativeArray<Vector4> nativeArray3 = default(NativeArray<Vector4>);
				if (flag)
				{
					nativeArray = meshChannelsCache_NativeArray.GetVerticiesAsNativeArray(dgo._mesh);
				}
				if (flag2)
				{
					nativeArray2 = meshChannelsCache_NativeArray.GetNormalsAsNativeArray(dgo._mesh);
				}
				if (flag3)
				{
					nativeArray3 = meshChannelsCache_NativeArray.GetTangentsAsNativeArray(dgo._mesh);
				}
				if (settings.renderType != MB_RenderType.skinnedMeshRenderer)
				{
					_LocalToWorld(dgo.gameObject.transform, flag2, flag3, destStartVertsIdx, nativeArray, nativeArray2, nativeArray3, verticies, normals, tangents);
				}
				else
				{
					boneProcessor.CopyVertsNormsTansToBuffers(dgo, settings, destStartVertsIdx, nativeArray2, nativeArray3, nativeArray, normals, tangents, verticies);
				}
			}
			if ((channels & MB_MeshVertexChannelFlags.uv0) == MB_MeshVertexChannelFlags.uv0 && (channelsToUpdate & MB_MeshVertexChannelFlags.uv0) == MB_MeshVertexChannelFlags.uv0)
			{
				_copyAndAdjustUVsFromMesh(textureBakeResults, dgo, dgo._mesh, 0, destStartVertsIdx, uv0s, uvsSliceIdx, meshChannelsCache_NativeArray, LOG_LEVEL, textureBakeResults);
			}
			if ((channels & MB_MeshVertexChannelFlags.uv2) == MB_MeshVertexChannelFlags.uv2 && (channelsToUpdate & MB_MeshVertexChannelFlags.uv2) == MB_MeshVertexChannelFlags.uv2)
			{
				_CopyAndAdjustUV2FromMesh(settings, meshChannelsCache_NativeArray, dgo, destStartVertsIdx, LOG_LEVEL);
			}
			if ((channels & MB_MeshVertexChannelFlags.uv3) == MB_MeshVertexChannelFlags.uv3 && (channelsToUpdate & MB_MeshVertexChannelFlags.uv3) == MB_MeshVertexChannelFlags.uv3)
			{
				MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopyTo(meshChannelsCache_NativeArray.GetUVChannelAsNativeArray(3, dgo._mesh), uv3s, destStartVertsIdx);
			}
			if ((channels & MB_MeshVertexChannelFlags.uv4) == MB_MeshVertexChannelFlags.uv4 && (channelsToUpdate & MB_MeshVertexChannelFlags.uv4) == MB_MeshVertexChannelFlags.uv4)
			{
				MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopyTo(meshChannelsCache_NativeArray.GetUVChannelAsNativeArray(4, dgo._mesh), uv4s, destStartVertsIdx);
			}
			if ((channels & MB_MeshVertexChannelFlags.uv5) == MB_MeshVertexChannelFlags.uv5 && (channelsToUpdate & MB_MeshVertexChannelFlags.uv5) == MB_MeshVertexChannelFlags.uv5)
			{
				MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopyTo(meshChannelsCache_NativeArray.GetUVChannelAsNativeArray(5, dgo._mesh), uv5s, destStartVertsIdx);
			}
			if ((channels & MB_MeshVertexChannelFlags.uv6) == MB_MeshVertexChannelFlags.uv6 && (channelsToUpdate & MB_MeshVertexChannelFlags.uv6) == MB_MeshVertexChannelFlags.uv6)
			{
				MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopyTo(meshChannelsCache_NativeArray.GetUVChannelAsNativeArray(6, dgo._mesh), uv6s, destStartVertsIdx);
			}
			if ((channels & MB_MeshVertexChannelFlags.uv7) == MB_MeshVertexChannelFlags.uv7 && (channelsToUpdate & MB_MeshVertexChannelFlags.uv7) == MB_MeshVertexChannelFlags.uv7)
			{
				MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopyTo(meshChannelsCache_NativeArray.GetUVChannelAsNativeArray(7, dgo._mesh), uv7s, destStartVertsIdx);
			}
			if ((channels & MB_MeshVertexChannelFlags.uv8) == MB_MeshVertexChannelFlags.uv8 && (channelsToUpdate & MB_MeshVertexChannelFlags.uv8) == MB_MeshVertexChannelFlags.uv8)
			{
				MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopyTo(meshChannelsCache_NativeArray.GetUVChannelAsNativeArray(8, dgo._mesh), uv8s, destStartVertsIdx);
			}
			if ((channels & MB_MeshVertexChannelFlags.colors) == MB_MeshVertexChannelFlags.colors && (channelsToUpdate & MB_MeshVertexChannelFlags.colors) == MB_MeshVertexChannelFlags.colors)
			{
				MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopyTo(meshChannelsCache_NativeArray.GetColorsAsNativeArray(dgo._mesh), colors, destStartVertsIdx);
			}
			if (updateBWdata)
			{
				boneProcessor.UpdateGameObjects_UpdateBWIndexes(dgo);
			}
			if (!updateTris)
			{
				return;
			}
			for (int i = 0; i < targSubmeshTidx.Length; i++)
			{
				dgo.submeshTriIdxs[i] = targSubmeshTidx[i];
			}
			for (int j = 0; j < dgo._tmpSubmeshTris.Length; j++)
			{
				int[] array = dgo._tmpSubmeshTris[j].data;
				if (destStartVertsIdx != 0)
				{
					for (int k = 0; k < array.Length; k++)
					{
						array[k] += destStartVertsIdx;
					}
				}
				if (dgo.invertTriangles)
				{
					for (int l = 0; l < array.Length; l += 3)
					{
						int num = array[l];
						array[l] = array[l + 1];
						array[l + 1] = num;
					}
				}
				int num2 = dgo.targetSubmeshIdxs[j];
				array.CopyTo(submeshTris[num2].data, targSubmeshTidx[num2]);
				dgo.submeshNumTris[num2] += array.Length;
				targSubmeshTidx[num2] += array.Length;
			}
		}

		public void AssignBuffersToMesh(Mesh mesh, MB_IMeshBakerSettings settings, MB2_TextureBakeResults textureBakeResults, MB_MeshVertexChannelFlags channelsToWriteToMesh, bool doWriteTrisToMesh, IAssignToMeshCustomizer assignToMeshCustomizer, List<MB_DynamicGameObject> mbDynamicObjectsInCombinedMesh, out BufferDataFromPreviousBake serializableBufferData, out SerializableIntArray[] submeshTrisToUse, out int numNonZeroLengthSubmeshes)
		{
			if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.vertex) == MB_MeshVertexChannelFlags.vertex)
			{
				AdjustVertsToWriteAccordingToPivotPositionIfNecessary(settings.pivotLocationType, settings.renderType, settings.clearBuffersAfterBake, settings.pivotLocation, out serializableBufferData);
			}
			else
			{
				serializableBufferData.numVertsBaked = data.vertexCount;
				serializableBufferData.meshVerticesShift = Vector3.zero;
				serializableBufferData.meshVerticiesWereShifted = false;
			}
			if (assignToMeshCustomizer != null)
			{
				IAssignToMeshCustomizer_NativeArrays assignToMeshCustomizer_NativeArrays = (IAssignToMeshCustomizer_NativeArrays)assignToMeshCustomizer;
				if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv0) == MB_MeshVertexChannelFlags.uv0)
				{
					assignToMeshCustomizer_NativeArrays.meshAssign_UV(0, settings, textureBakeResults, uvsWithExtraIndex, uvsSliceIdx);
				}
				if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv2) == MB_MeshVertexChannelFlags.uv2)
				{
					assignToMeshCustomizer_NativeArrays.meshAssign_UV(1, settings, textureBakeResults, uvsWithExtraIndex, uvsSliceIdx);
				}
				if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv3) == MB_MeshVertexChannelFlags.uv3)
				{
					assignToMeshCustomizer_NativeArrays.meshAssign_UV(2, settings, textureBakeResults, uvsWithExtraIndex, uvsSliceIdx);
				}
				if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv4) == MB_MeshVertexChannelFlags.uv4)
				{
					assignToMeshCustomizer_NativeArrays.meshAssign_UV(3, settings, textureBakeResults, uvsWithExtraIndex, uvsSliceIdx);
				}
				if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv5) == MB_MeshVertexChannelFlags.uv5)
				{
					assignToMeshCustomizer_NativeArrays.meshAssign_UV(4, settings, textureBakeResults, uvsWithExtraIndex, uvsSliceIdx);
				}
				if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv6) == MB_MeshVertexChannelFlags.uv6)
				{
					assignToMeshCustomizer_NativeArrays.meshAssign_UV(5, settings, textureBakeResults, uvsWithExtraIndex, uvsSliceIdx);
				}
				if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv7) == MB_MeshVertexChannelFlags.uv7)
				{
					assignToMeshCustomizer_NativeArrays.meshAssign_UV(6, settings, textureBakeResults, uvsWithExtraIndex, uvsSliceIdx);
				}
				if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.uv8) == MB_MeshVertexChannelFlags.uv8)
				{
					assignToMeshCustomizer_NativeArrays.meshAssign_UV(7, settings, textureBakeResults, uvsWithExtraIndex, uvsSliceIdx);
				}
				if ((channelsToWriteToMesh & MB_MeshVertexChannelFlags.colors) == MB_MeshVertexChannelFlags.colors)
				{
					assignToMeshCustomizer_NativeArrays.meshAssign_colors(settings, textureBakeResults, colors, uvsSliceIdx);
				}
			}
			else if (textureBakeResults.resultType == MB2_TextureBakeResults.ResultType.textureArray)
			{
				UnityEngine.Debug.LogError("No AssignToMeshCustomizer was assigned.");
			}
			if (doWriteTrisToMesh)
			{
				AssignTriangleDataForSubmeshes(mesh, mbDynamicObjectsInCombinedMesh, ref serializableBufferData, out submeshTrisToUse, out numNonZeroLengthSubmeshes);
			}
			else
			{
				submeshTrisToUse = null;
				numNonZeroLengthSubmeshes = -1;
			}
			Mesh.ApplyAndDisposeWritableMeshData(dataArray, mesh);
			dataArrayAllocated = false;
		}

		public void AssignTriangleDataForSubmeshes(Mesh mmesh, List<MB_DynamicGameObject> mbDynamicObjectsInCombinedMesh, ref BufferDataFromPreviousBake serializableBufferData, out SerializableIntArray[] submeshTrisToUse, out int numNonZeroLengthSubmeshes)
		{
			submeshTrisToUse = GetSubmeshTrisWithShowHideApplied(mbDynamicObjectsInCombinedMesh);
			numNonZeroLengthSubmeshes = _NumNonZeroLengthSubmeshTris(submeshTrisToUse, out var numIndexes);
			IndexFormat indexFormat = ((numIndexes > 65535) ? IndexFormat.UInt32 : IndexFormat.UInt16);
			data.SetIndexBufferParams(numIndexes, indexFormat);
			if (indexFormat == IndexFormat.UInt16)
			{
				int num = 0;
				int num2 = 0;
				NativeArray<ushort> indexData = data.GetIndexData<ushort>();
				for (int i = 0; i < submeshTrisToUse.Length; i++)
				{
					if (submeshTrisToUse[i].data.Length != 0)
					{
						SerializableIntArray serializableIntArray = submeshTrisToUse[i];
						for (int j = 0; j < serializableIntArray.data.Length; j++)
						{
							indexData[num2 + j] = (ushort)serializableIntArray.data[j];
						}
						num++;
						num2 += serializableIntArray.data.Length;
					}
				}
			}
			else
			{
				int num3 = 0;
				int num4 = 0;
				NativeArray<uint> indexData2 = data.GetIndexData<uint>();
				for (int k = 0; k < submeshTrisToUse.Length; k++)
				{
					if (submeshTrisToUse[k].data.Length != 0)
					{
						SerializableIntArray serializableIntArray2 = submeshTrisToUse[k];
						for (int l = 0; l < serializableIntArray2.data.Length; l++)
						{
							indexData2[num4 + l] = (uint)serializableIntArray2.data[l];
						}
						num3++;
						num4 += serializableIntArray2.data.Length;
					}
				}
			}
			data.subMeshCount = numNonZeroLengthSubmeshes;
			int num5 = 0;
			int num6 = 0;
			for (int m = 0; m < submeshTrisToUse.Length; m++)
			{
				if (submeshTrisToUse[m].data.Length != 0)
				{
					SerializableIntArray serializableIntArray3 = submeshTrisToUse[m];
					SubMeshDescriptor desc = new SubMeshDescriptor(num6, serializableIntArray3.data.Length);
					data.SetSubMesh(num5, desc);
					num5++;
					num6 += serializableIntArray3.data.Length;
				}
			}
		}

		public void AssignTriangleDataForSubmeshes_ShowHide(Mesh mesh, List<MB_DynamicGameObject> mbDynamicObjectsInCombinedMesh, ref BufferDataFromPreviousBake serializableBufferData, out SerializableIntArray[] submeshTrisToUse, out int numNonZeroLengthSubmeshes)
		{
			submeshTrisToUse = GetSubmeshTrisWithShowHideApplied(mbDynamicObjectsInCombinedMesh);
			numNonZeroLengthSubmeshes = _NumNonZeroLengthSubmeshTris(submeshTrisToUse, out var numIndexes);
			IndexFormat indexFormat = ((numIndexes > 65535) ? IndexFormat.UInt32 : IndexFormat.UInt16);
			mesh.subMeshCount = 1;
			mesh.SetIndexBufferParams(numIndexes, indexFormat);
			if (indexFormat == IndexFormat.UInt16)
			{
				int num = 0;
				int num2 = 0;
				NativeArray<ushort> nativeArray = new NativeArray<ushort>(numIndexes, Allocator.Temp);
				for (int i = 0; i < submeshTrisToUse.Length; i++)
				{
					if (submeshTrisToUse[i].data.Length != 0)
					{
						SerializableIntArray serializableIntArray = submeshTrisToUse[i];
						for (int j = 0; j < serializableIntArray.data.Length; j++)
						{
							nativeArray[num2 + j] = (ushort)serializableIntArray.data[j];
						}
						num++;
						num2 += serializableIntArray.data.Length;
					}
				}
				mesh.SetIndexBufferData(nativeArray, 0, 0, nativeArray.Length, MeshUpdateFlags.DontValidateIndices);
				if (nativeArray.IsCreated)
				{
					nativeArray.Dispose();
				}
			}
			else
			{
				int num3 = 0;
				int num4 = 0;
				NativeArray<uint> nativeArray2 = new NativeArray<uint>(numIndexes, Allocator.Temp);
				for (int k = 0; k < submeshTrisToUse.Length; k++)
				{
					if (submeshTrisToUse[k].data.Length != 0)
					{
						SerializableIntArray serializableIntArray2 = submeshTrisToUse[k];
						for (int l = 0; l < serializableIntArray2.data.Length; l++)
						{
							nativeArray2[num4 + l] = (uint)serializableIntArray2.data[l];
						}
						num3++;
						num4 += serializableIntArray2.data.Length;
					}
				}
				mesh.SetIndexBufferData(nativeArray2, 0, 0, nativeArray2.Length, MeshUpdateFlags.DontValidateIndices);
				if (nativeArray2.IsCreated)
				{
					nativeArray2.Dispose();
				}
			}
			mesh.subMeshCount = numNonZeroLengthSubmeshes;
			int num5 = 0;
			int num6 = 0;
			for (int m = 0; m < submeshTrisToUse.Length; m++)
			{
				if (submeshTrisToUse[m].data.Length != 0)
				{
					SerializableIntArray serializableIntArray3 = submeshTrisToUse[m];
					SubMeshDescriptor desc = new SubMeshDescriptor(num6, serializableIntArray3.data.Length);
					mesh.SetSubMesh(num5, desc);
					num5++;
					num6 += serializableIntArray3.data.Length;
				}
			}
		}

		private void AdjustVertsToWriteAccordingToPivotPositionIfNecessary(MB_MeshPivotLocation pivotLocationType, MB_RenderType renderType, bool clearBuffersAfterBake, Vector3 pivotLocation_wld, out BufferDataFromPreviousBake serializableBufferData)
		{
			serializableBufferData.numVertsBaked = data.vertexCount;
			if (verticies.Length > 0)
			{
				if (renderType == MB_RenderType.skinnedMeshRenderer)
				{
					serializableBufferData.meshVerticesShift = Vector3.zero;
					serializableBufferData.meshVerticiesWereShifted = false;
					return;
				}
				switch (pivotLocationType)
				{
				case MB_MeshPivotLocation.worldOrigin:
					serializableBufferData.meshVerticesShift = Vector3.zero;
					serializableBufferData.meshVerticiesWereShifted = false;
					break;
				case MB_MeshPivotLocation.boundsCenter:
				case MB_MeshPivotLocation.customLocation:
				{
					Vector3 vector4;
					if (pivotLocationType == MB_MeshPivotLocation.boundsCenter)
					{
						Vector3 vector = verticies[0];
						Vector3 vector2 = verticies[0];
						for (int i = 1; i < verticies.Length; i++)
						{
							Vector3 vector3 = verticies[i];
							if (vector.x < vector3.x)
							{
								vector.x = vector3.x;
							}
							if (vector.y < vector3.y)
							{
								vector.y = vector3.y;
							}
							if (vector.z < vector3.z)
							{
								vector.z = vector3.z;
							}
							if (vector2.x > vector3.x)
							{
								vector2.x = vector3.x;
							}
							if (vector2.y > vector3.y)
							{
								vector2.y = vector3.y;
							}
							if (vector2.z > vector3.z)
							{
								vector2.z = vector3.z;
							}
						}
						vector4 = (vector + vector2) * 0.5f;
					}
					else
					{
						vector4 = pivotLocation_wld;
					}
					for (int j = 0; j < verticies.Length; j++)
					{
						verticies[j] -= vector4;
					}
					serializableBufferData.meshVerticesShift = vector4;
					serializableBufferData.meshVerticiesWereShifted = true;
					break;
				}
				default:
					UnityEngine.Debug.LogError("Unsupported Pivot Location Type: " + pivotLocationType);
					serializableBufferData.meshVerticesShift = Vector3.zero;
					serializableBufferData.meshVerticiesWereShifted = false;
					break;
				}
			}
			else
			{
				serializableBufferData.meshVerticesShift = Vector3.zero;
				serializableBufferData.meshVerticiesWereShifted = false;
			}
		}

		private static int _NumNonZeroLengthSubmeshTris(SerializableIntArray[] subTris, out int numIndexes)
		{
			numIndexes = 0;
			int num = 0;
			for (int i = 0; i < subTris.Length; i++)
			{
				if (subTris[i].data.Length != 0)
				{
					num++;
					numIndexes += subTris[i].data.Length;
				}
			}
			return num;
		}

		private void _copyAndAdjustUVsFromMesh(MB2_TextureBakeResults tbr, MB_DynamicGameObject dgo, Mesh mesh, int uvChannel, int vertsIdx, NativeSlice<Vector2> uvsOut, NativeSlice<float> uvsSliceIdx, MeshChannelsCache_NativeArray meshChannelsCache, MB2_LogLevel LOG_LEVEL, MB2_TextureBakeResults textureBakeResults)
		{
			NativeArray<Vector2> uVChannelAsNativeArray = meshChannelsCache.GetUVChannelAsNativeArray(uvChannel, mesh);
			int[] array = new int[uVChannelAsNativeArray.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = -1;
			}
			bool flag = false;
			bool flag2 = tbr.resultType == MB2_TextureBakeResults.ResultType.textureArray;
			for (int j = 0; j < dgo.targetSubmeshIdxs.Length; j++)
			{
				int[] array2 = ((dgo._tmpSubmeshTris == null) ? mesh.GetTriangles(j) : dgo._tmpSubmeshTris[j].data);
				float value = dgo.textureArraySliceIdx[j];
				int idxInSrcMats = dgo.targetSubmeshIdxs[j];
				if (LOG_LEVEL >= MB2_LogLevel.trace)
				{
					UnityEngine.Debug.Log($"Build UV transform for mesh {dgo.name} submesh {j} encapsulatingRect {dgo.encapsulatingRect[j]}");
				}
				Rect rect = MB3_TextureCombinerMerging.BuildTransformMeshUV2AtlasRect(textureBakeResults.GetConsiderMeshUVs(idxInSrcMats, dgo.sourceSharedMaterials[j]), dgo.uvRects[j], (dgo.obUVRects == null || dgo.obUVRects.Length == 0) ? new Rect(0f, 0f, 1f, 1f) : dgo.obUVRects[j], dgo.sourceMaterialTiling[j], dgo.encapsulatingRect[j]);
				foreach (int num in array2)
				{
					if (array[num] == -1)
					{
						array[num] = j;
						Vector2 value2 = uVChannelAsNativeArray[num];
						value2.x = rect.x + value2.x * rect.width;
						value2.y = rect.y + value2.y * rect.height;
						int index = vertsIdx + num;
						uvsOut[index] = value2;
						if (flag2)
						{
							uvsSliceIdx[index] = value;
						}
					}
					if (array[num] != j)
					{
						flag = true;
					}
				}
			}
			if (flag && LOG_LEVEL >= MB2_LogLevel.warn)
			{
				UnityEngine.Debug.LogWarning(dgo.name + "has submeshes which share verticies. Adjusted uvs may not map correctly in combined atlas.");
			}
			if (LOG_LEVEL >= MB2_LogLevel.trace)
			{
				UnityEngine.Debug.Log($"_copyAndAdjustUVsFromMesh copied {uVChannelAsNativeArray.Length} verts");
			}
		}

		private void _CopyAndAdjustUV2FromMesh(MB_IMeshBakerSettings settings, MeshChannelsCache_NativeArray meshChannelsCache, MB_DynamicGameObject dgo, int vertsIdx, MB2_LogLevel LOG_LEVEL)
		{
			NativeArray<Vector2> nativeArray = meshChannelsCache.GetUVChannelAsNativeArray(2, dgo._mesh);
			if (settings.lightmapOption == MB2_LightmapOptions.preserve_current_lightmapping)
			{
				if (nativeArray.Length == 0)
				{
					NativeArray<Vector2> uVChannelAsNativeArray = meshChannelsCache.GetUVChannelAsNativeArray(0, dgo._mesh);
					if (uVChannelAsNativeArray.Length > 0)
					{
						nativeArray = uVChannelAsNativeArray;
					}
					else
					{
						if (LOG_LEVEL >= MB2_LogLevel.warn)
						{
							UnityEngine.Debug.LogWarning("Mesh " + dgo._mesh?.ToString() + " didn't have uv2s. Generating uv2s.");
						}
						nativeArray = meshChannelsCache.GetUv2ModifiedAsNativeArray(dgo._mesh);
					}
				}
				Vector4 lightmapTilingOffset = dgo.lightmapTilingOffset;
				Vector2 vector = new Vector2(lightmapTilingOffset.x, lightmapTilingOffset.y);
				Vector2 vector2 = new Vector2(lightmapTilingOffset.z, lightmapTilingOffset.w);
				Vector2 vector3 = default(Vector2);
				for (int i = 0; i < nativeArray.Length; i++)
				{
					vector3.x = vector.x * nativeArray[i].x;
					vector3.y = vector.y * nativeArray[i].y;
					uv2s[vertsIdx + i] = vector2 + vector3;
				}
				if (LOG_LEVEL >= MB2_LogLevel.trace)
				{
					UnityEngine.Debug.Log("_copyAndAdjustUV2FromMesh copied and modify for preserve current lightmapping " + nativeArray.Length);
				}
				return;
			}
			if (nativeArray.Length == 0)
			{
				if (LOG_LEVEL >= MB2_LogLevel.warn)
				{
					UnityEngine.Debug.LogWarning("Mesh " + dgo._mesh?.ToString() + " didn't have uv2s. Generating uv2s.");
				}
				if (settings.lightmapOption == MB2_LightmapOptions.copy_UV2_unchanged_to_separate_rects && nativeArray.Length == 0)
				{
					UnityEngine.Debug.LogError("Mesh " + dgo._mesh?.ToString() + " did not have a UV2 channel. Nothing to copy when trying to copy UV2 to separate rects. The combined mesh will not lightmap properly. Try using generate new uv2 layout.");
				}
				nativeArray = meshChannelsCache.GetUv2ModifiedAsNativeArray(dgo._mesh);
			}
			MB_MeshCombinerSingle_MeshNativeArrayHelper.NativeSliceCopyTo(nativeArray, uv2s, vertsIdx);
			if (LOG_LEVEL >= MB2_LogLevel.trace)
			{
				UnityEngine.Debug.Log("_copyAndAdjustUV2FromMesh copied without modifying " + nativeArray.Length);
			}
		}

		public void CopyUV2unchangedToSeparateRects(List<MB_DynamicGameObject> mbDynamicObjectsInCombinedMesh, float uv2UnwrappingParamsPackMargin)
		{
			int num = Mathf.CeilToInt(8192f * uv2UnwrappingParamsPackMargin);
			if (num < 1)
			{
				num = 1;
			}
			List<Vector2> list = new List<Vector2>(mbDynamicObjectsInCombinedMesh.Count);
			float[] array = new float[mbDynamicObjectsInCombinedMesh.Count];
			Rect[] array2 = new Rect[mbDynamicObjectsInCombinedMesh.Count];
			float num2 = 0f;
			for (int i = 0; i < mbDynamicObjectsInCombinedMesh.Count; i++)
			{
				MB_DynamicGameObject mB_DynamicGameObject = mbDynamicObjectsInCombinedMesh[i];
				float num3 = 1f;
				if (Application.isEditor && mB_DynamicGameObject._renderer is MeshRenderer)
				{
					num3 = MBVersion.GetScaleInLightmap((MeshRenderer)mB_DynamicGameObject._renderer);
					if (num3 <= 0f)
					{
						num3 = 1f;
					}
				}
				float magnitude = mB_DynamicGameObject.meshSize.magnitude;
				array[i] = num3 * magnitude;
				num2 += array[i];
			}
			for (int j = 0; j < array.Length; j++)
			{
				MB_DynamicGameObject mB_DynamicGameObject2 = mbDynamicObjectsInCombinedMesh[j];
				int num4 = mB_DynamicGameObject2.vertIdx + mB_DynamicGameObject2.numVerts;
				float x;
				float num5 = (x = uv2s[mB_DynamicGameObject2.vertIdx].x);
				float y;
				float num6 = (y = uv2s[mB_DynamicGameObject2.vertIdx].y);
				for (int k = mB_DynamicGameObject2.vertIdx; k < num4; k++)
				{
					if (uv2s[k].x < num5)
					{
						num5 = uv2s[k].x;
					}
					if (uv2s[k].x > x)
					{
						x = uv2s[k].x;
					}
					if (uv2s[k].y < num6)
					{
						num6 = uv2s[k].y;
					}
					if (uv2s[k].y > y)
					{
						y = uv2s[k].y;
					}
				}
				array2[j] = new Rect(num5, num6, x - num5, y - num6);
				array[j] /= num2;
				Vector2 item = new Vector2(array2[j].width, array2[j].height) * (array[j] * 8192f);
				list.Add(item);
			}
			AtlasPackingResult atlasPackingResult = new MB2_TexturePackerRegular
			{
				atlasMustBePowerOfTwo = false
			}.GetRects(list, 8192, 8192, num)[0];
			Vector2 value = default(Vector2);
			for (int l = 0; l < mbDynamicObjectsInCombinedMesh.Count; l++)
			{
				MB_DynamicGameObject mB_DynamicGameObject3 = mbDynamicObjectsInCombinedMesh[l];
				int num7 = mB_DynamicGameObject3.vertIdx + mB_DynamicGameObject3.numVerts;
				Rect rect = array2[l];
				Rect rect2 = atlasPackingResult.rects[l];
				for (int m = mB_DynamicGameObject3.vertIdx; m < num7; m++)
				{
					value.x = (uv2s[m].x - rect.x) / rect.width * rect2.width + rect2.x;
					value.y = (uv2s[m].y - rect.y) / rect.height * rect2.height + rect2.y;
					uv2s[m] = value;
				}
				if (atlasPackingResult.atlasX == atlasPackingResult.atlasY)
				{
					continue;
				}
				if (atlasPackingResult.atlasX < atlasPackingResult.atlasY)
				{
					float num8 = (float)atlasPackingResult.atlasX / (float)atlasPackingResult.atlasY;
					for (int n = mB_DynamicGameObject3.vertIdx; n < num7; n++)
					{
						Vector2 value2 = uv2s[n];
						value2.x *= num8;
						uv2s[n] = value2;
					}
				}
				else
				{
					float num9 = (float)atlasPackingResult.atlasY / (float)atlasPackingResult.atlasX;
					for (int num10 = mB_DynamicGameObject3.vertIdx; num10 < num7; num10++)
					{
						Vector2 value3 = uv2s[num10];
						value3.y *= num9;
						uv2s[num10] = value3;
					}
				}
			}
		}

		private SerializableIntArray[] GetSubmeshTrisWithShowHideApplied(List<MB_DynamicGameObject> mbDynamicObjectsInCombinedMesh)
		{
			bool flag = false;
			for (int i = 0; i < mbDynamicObjectsInCombinedMesh.Count; i++)
			{
				if (!mbDynamicObjectsInCombinedMesh[i].show)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				int[] array = new int[submeshTris.Length];
				SerializableIntArray[] array2 = new SerializableIntArray[submeshTris.Length];
				for (int j = 0; j < mbDynamicObjectsInCombinedMesh.Count; j++)
				{
					MB_DynamicGameObject mB_DynamicGameObject = mbDynamicObjectsInCombinedMesh[j];
					if (mB_DynamicGameObject.show)
					{
						for (int k = 0; k < mB_DynamicGameObject.submeshNumTris.Length; k++)
						{
							array[k] += mB_DynamicGameObject.submeshNumTris[k];
						}
					}
				}
				for (int l = 0; l < array2.Length; l++)
				{
					array2[l] = new SerializableIntArray(array[l]);
				}
				int[] array3 = new int[array2.Length];
				for (int m = 0; m < mbDynamicObjectsInCombinedMesh.Count; m++)
				{
					MB_DynamicGameObject mB_DynamicGameObject2 = mbDynamicObjectsInCombinedMesh[m];
					if (!mB_DynamicGameObject2.show)
					{
						continue;
					}
					for (int n = 0; n < submeshTris.Length; n++)
					{
						int[] array4 = submeshTris[n].data;
						int num = mB_DynamicGameObject2.submeshTriIdxs[n];
						int num2 = num + mB_DynamicGameObject2.submeshNumTris[n];
						for (int num3 = num; num3 < num2; num3++)
						{
							array2[n].data[array3[n]] = array4[num3];
							array3[n]++;
						}
					}
				}
				return array2;
			}
			return submeshTris;
		}

		public int[] GetTriangleSizes()
		{
			int[] array = new int[submeshTris.Length];
			for (int i = 0; i < submeshTris.Length; i++)
			{
				array[i] = submeshTris[i].data.Length;
			}
			return array;
		}

		private void _LocalToWorld(Transform t, bool doNorm, bool doTan, int destStartVertsIdx, NativeArray<Vector3> dgoMeshVerts, NativeArray<Vector3> dgoMeshNorms, NativeArray<Vector4> dgoMeshTans, NativeSlice<Vector3> verticies, NativeSlice<Vector3> normals, NativeSlice<Vector4> tangents)
		{
			Vector3 lossyScale = t.lossyScale;
			if (lossyScale == Vector3.one)
			{
				_LocalToWorld_TR(t.rotation, t.position, doNorm, doTan, destStartVertsIdx, dgoMeshVerts, dgoMeshNorms, dgoMeshTans, verticies, normals, tangents);
			}
			else if (lossyScale.x > Mathf.Epsilon && lossyScale.y > Mathf.Epsilon && lossyScale.z > Mathf.Epsilon)
			{
				Matrix4x4 wld_X_local = t.localToWorldMatrix;
				_LocalToWorldMatrix_TRS(ref wld_X_local, doNorm, doTan, destStartVertsIdx, dgoMeshVerts, dgoMeshNorms, dgoMeshTans, verticies, normals, tangents);
			}
			else
			{
				_LocalToWorld_TRS(t.rotation, t.position, t.lossyScale, doNorm, doTan, destStartVertsIdx, dgoMeshVerts, dgoMeshNorms, dgoMeshTans, verticies, normals, tangents);
			}
		}

		private static void _LocalToWorldMatrix_TRS(ref Matrix4x4 wld_X_local, bool doNorm, bool doTan, int destStartVertsIdx, NativeSlice<Vector3> dgoMeshVerts, NativeSlice<Vector3> dgoMeshNorms, NativeSlice<Vector4> dgoMeshTans, NativeSlice<Vector3> verticies, NativeSlice<Vector3> normals, NativeSlice<Vector4> tangents)
		{
			Matrix4x4 matrix4x = Matrix4x4.zero;
			if (doNorm || doTan)
			{
				matrix4x = wld_X_local;
				float num2 = (matrix4x[2, 3] = 0f);
				float value = (matrix4x[1, 3] = num2);
				matrix4x[0, 3] = value;
				matrix4x = matrix4x.inverse.transpose;
			}
			for (int i = 0; i < dgoMeshVerts.Length; i++)
			{
				int index = destStartVertsIdx + i;
				verticies[index] = wld_X_local.MultiplyPoint3x4(dgoMeshVerts[i]);
				if (doNorm)
				{
					normals[index] = matrix4x.MultiplyPoint3x4(dgoMeshNorms[i]).normalized;
				}
				if (doTan)
				{
					float w = dgoMeshTans[i].w;
					Vector4 value2 = matrix4x.MultiplyPoint3x4(dgoMeshTans[i]).normalized;
					value2.w = w;
					tangents[index] = value2;
				}
			}
		}

		private static void _LocalToWorld_TR(Quaternion wld_Rot_local, Vector3 position_wld, bool doNorm, bool doTan, int destStartVertsIdx, NativeSlice<Vector3> dgoMeshVerts_local, NativeSlice<Vector3> dgoMeshNorms_local, NativeSlice<Vector4> dgoMeshTans_local, NativeSlice<Vector3> verticies, NativeSlice<Vector3> normals, NativeSlice<Vector4> tangents)
		{
			for (int i = 0; i < dgoMeshVerts_local.Length; i++)
			{
				int index = destStartVertsIdx + i;
				Vector3 vector = dgoMeshVerts_local[i];
				vector = wld_Rot_local * vector;
				vector += position_wld;
				verticies[index] = vector;
				if (doNorm)
				{
					Vector3 vector2 = dgoMeshNorms_local[i];
					vector2 = wld_Rot_local * vector2;
					normals[index] = vector2;
				}
				if (doTan)
				{
					Vector3 vector3 = dgoMeshTans_local[i];
					float w = dgoMeshTans_local[i].w;
					vector3 = wld_Rot_local * vector3;
					Vector4 value = vector3;
					value.w = w;
					tangents[index] = value;
				}
			}
		}

		private static void _LocalToWorld_TRS(Quaternion wld_Rot_local, Vector3 position_wld, Vector3 scale, bool doNorm, bool doTan, int destStartVertsIdx, NativeSlice<Vector3> dgoMeshVerts_local, NativeSlice<Vector3> dgoMeshNorms_local, NativeSlice<Vector4> dgoMeshTans_local, NativeSlice<Vector3> verticies, NativeSlice<Vector3> normals, NativeSlice<Vector4> tangents)
		{
			Vector3 one = Vector3.one;
			if (doNorm || doTan)
			{
				one.x = ((scale.x < Mathf.Epsilon) ? 0f : (1f / scale.x));
				one.y = ((scale.y < Mathf.Epsilon) ? 0f : (1f / scale.y));
				one.z = ((scale.z < Mathf.Epsilon) ? 0f : (1f / scale.z));
			}
			for (int i = 0; i < dgoMeshVerts_local.Length; i++)
			{
				int index = destStartVertsIdx + i;
				Vector3 vector = dgoMeshVerts_local[i];
				vector.x *= scale.x;
				vector.y *= scale.y;
				vector.z *= scale.z;
				vector = wld_Rot_local * vector;
				vector += position_wld;
				verticies[index] = vector;
				if (doNorm)
				{
					Vector3 vector2 = dgoMeshNorms_local[i];
					vector2.x *= one.x;
					vector2.y *= one.y;
					vector2.z *= one.z;
					vector2 = wld_Rot_local * vector2;
					vector2.Normalize();
					normals[index] = vector2;
				}
				if (doTan)
				{
					Vector3 vector3 = dgoMeshTans_local[i];
					float w = dgoMeshTans_local[i].w;
					vector3.x *= one.x;
					vector3.y *= one.y;
					vector3.z *= one.z;
					vector3 = wld_Rot_local * vector3;
					vector3.Normalize();
					tangents[index] = new Vector4(vector3.x, vector3.y, vector3.z, w);
				}
			}
		}
	}

	public Stopwatch db_showHideGameObjects = new Stopwatch();

	public Stopwatch db_addDeleteGameObjects = new Stopwatch();

	public Stopwatch db_addDeleteGameObjects_CollectMeshData = new Stopwatch();

	public Stopwatch db_addDeleteGameObjects_CollectMeshData_a = new Stopwatch();

	public Stopwatch db_addDeleteGameObjects_CollectMeshData_b = new Stopwatch();

	public Stopwatch db_addDeleteGameObjects_CollectMeshData_c = new Stopwatch();

	public Stopwatch db_addDeleteGameObjects_InitFromMeshCombiner = new Stopwatch();

	public Stopwatch db_addDeleteGameObjects_Init = new Stopwatch();

	public Stopwatch db_addDeleteGameObjects_CopyArraysFromPreviousBakeBuffersToNewBuffers = new Stopwatch();

	public Stopwatch db_addDeleteGameObjects_CopyFromDGOMeshToBuffers = new Stopwatch();

	public Stopwatch db_apply = new Stopwatch();

	public Stopwatch db_applyShowHide = new Stopwatch();

	public Stopwatch db_updateGameObjects = new Stopwatch();

	[SerializeField]
	protected List<GameObject> objectsInCombinedMesh = new List<GameObject>();

	[SerializeField]
	private int lightmapIndex = -1;

	[SerializeField]
	public List<MB_DynamicGameObject> mbDynamicObjectsInCombinedMesh = new List<MB_DynamicGameObject>();

	private Dictionary<GameObject, MB_DynamicGameObject> _instance2combined_map = new Dictionary<GameObject, MB_DynamicGameObject>();

	[SerializeField]
	private MB_MeshVertexChannelFlags channelsLastBake;

	[SerializeField]
	private Vector3[] verts = new Vector3[0];

	[SerializeField]
	private Vector3[] normals = new Vector3[0];

	[SerializeField]
	private Vector4[] tangents = new Vector4[0];

	[SerializeField]
	private Vector2[] uvs = new Vector2[0];

	[SerializeField]
	private float[] uvsSliceIdx = new float[0];

	[SerializeField]
	private Vector2[] uv2s = new Vector2[0];

	[SerializeField]
	private Vector2[] uv3s = new Vector2[0];

	[SerializeField]
	private Vector2[] uv4s = new Vector2[0];

	[SerializeField]
	private Vector2[] uv5s = new Vector2[0];

	[SerializeField]
	private Vector2[] uv6s = new Vector2[0];

	[SerializeField]
	private Vector2[] uv7s = new Vector2[0];

	[SerializeField]
	private Vector2[] uv8s = new Vector2[0];

	[SerializeField]
	private Color[] colors = new Color[0];

	[SerializeField]
	private SerializableIntArray[] submeshTris = new SerializableIntArray[0];

	[SerializeField]
	private Matrix4x4[] bindPoses = new Matrix4x4[0];

	[SerializeField]
	private Transform[] bones = new Transform[0];

	[SerializeField]
	internal MBBlendShape[] blendShapes = new MBBlendShape[0];

	[SerializeField]
	internal BufferDataFromPreviousBake bufferDataFromPrevious;

	[SerializeField]
	private MeshCreationConditions _meshBirth;

	[SerializeField]
	private Mesh _mesh;

	internal IVertexAndTriangleProcessor _vertexAndTriProcessor;

	protected MB_IMeshCombinerSingle_BoneProcessor _boneProcessor;

	internal MB_MeshCombinerSingle_BlendShapeProcessor _blendShapeProcessor;

	protected IMeshChannelsCacheTaggingInterface _meshChannelsCache;

	private GameObject[] empty = new GameObject[0];

	private int[] emptyIDs = new int[0];

	public override MB2_TextureBakeResults textureBakeResults
	{
		set
		{
			if (GetVertexCount() > 0 && _textureBakeResults != value && _textureBakeResults != null && LOG_LEVEL >= MB2_LogLevel.warn)
			{
				UnityEngine.Debug.LogWarning("If Texture Bake Result is changed then objects currently in combined mesh may be invalid.");
			}
			_textureBakeResults = value;
		}
	}

	public override MB_RenderType renderType
	{
		set
		{
			if (value == MB_RenderType.skinnedMeshRenderer && _renderType == MB_RenderType.meshRenderer && GetVertexCount() > 0 && (bones == null || bones.Length == 0))
			{
				UnityEngine.Debug.LogError("Can't set the render type to SkinnedMeshRenderer without clearing the mesh first. Try deleting the CombinedMesh scene object.");
			}
			_renderType = value;
		}
	}

	public override GameObject resultSceneObject
	{
		set
		{
			if (_resultSceneObject != value && _resultSceneObject != null)
			{
				_targetRenderer = null;
				if (_mesh != null && LOG_LEVEL >= MB2_LogLevel.warn)
				{
					UnityEngine.Debug.LogWarning("Result Scene Object was changed when this mesh baker component had a reference to a mesh. If mesh is being used by another object make sure to reset the mesh to none before baking to avoid overwriting the other mesh.");
				}
			}
			_resultSceneObject = value;
		}
	}

	public void StartProfile()
	{
		db_showHideGameObjects.Reset();
		db_addDeleteGameObjects.Reset();
		db_apply.Reset();
		db_applyShowHide.Reset();
		db_updateGameObjects.Reset();
	}

	public void PrintProfileInfo()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Timings  " + ((base.settings.meshAPI == MB_MeshCombineAPIType.betaNativeArrayAPI) ? "  newMeshAPI " : " oldMeshAPI"));
		stringBuilder.AppendLine("db_showHideGameObjects " + db_showHideGameObjects.Elapsed.Seconds);
		stringBuilder.AppendLine("db_addDeleteGameObjects " + db_addDeleteGameObjects.Elapsed.Seconds);
		stringBuilder.AppendLine("db_apply " + db_apply.Elapsed.Seconds);
		stringBuilder.AppendLine("db_applyShowHide " + db_applyShowHide.Elapsed.Seconds);
		stringBuilder.AppendLine("db_updateGameObjects " + db_updateGameObjects.Elapsed.Seconds);
		UnityEngine.Debug.Log(stringBuilder.ToString());
	}

	protected override void Dispose(bool disposing)
	{
		if (!IsDisposed())
		{
			base.Dispose(disposing);
			if (_boneProcessor != null)
			{
				_boneProcessor.DisposeOfTemporarySMRData();
				_boneProcessor.Dispose();
				_boneProcessor = null;
			}
			if (_blendShapeProcessor != null)
			{
				_blendShapeProcessor.Dispose();
				_blendShapeProcessor = null;
			}
			if (_meshChannelsCache != null)
			{
				_meshChannelsCache.Dispose();
				_meshChannelsCache = null;
			}
			if (_vertexAndTriProcessor != null)
			{
				_vertexAndTriProcessor.Dispose();
			}
		}
	}

	public int GetVertexCount()
	{
		return verts.Length;
	}

	private MB_DynamicGameObject instance2Combined_MapGet(GameObject gameObjectID)
	{
		return _instance2combined_map[gameObjectID];
	}

	private void instance2Combined_MapAdd(GameObject gameObjectID, MB_DynamicGameObject dgo)
	{
		_instance2combined_map.Add(gameObjectID, dgo);
	}

	private void instance2Combined_MapRemove(GameObject gameObjectID)
	{
		_instance2combined_map.Remove(gameObjectID);
	}

	private bool instance2Combined_MapTryGetValue(GameObject gameObjectID, out MB_DynamicGameObject dgo)
	{
		return _instance2combined_map.TryGetValue(gameObjectID, out dgo);
	}

	private int instance2Combined_MapCount()
	{
		return _instance2combined_map.Count;
	}

	private void instance2Combined_MapClear()
	{
		_instance2combined_map.Clear();
	}

	private bool instance2Combined_MapContainsKey(GameObject gameObjectID)
	{
		return _instance2combined_map.ContainsKey(gameObjectID);
	}

	public bool InstanceID2DGO(int instanceID, out MB_DynamicGameObject dgoGameObject)
	{
		for (int i = 0; i < mbDynamicObjectsInCombinedMesh.Count; i++)
		{
			if (mbDynamicObjectsInCombinedMesh[i].gameObject != null)
			{
				if (mbDynamicObjectsInCombinedMesh[i].gameObject.GetInstanceID() == instanceID)
				{
					dgoGameObject = mbDynamicObjectsInCombinedMesh[i];
					return true;
				}
			}
			else if (mbDynamicObjectsInCombinedMesh[i].instanceID == instanceID)
			{
				dgoGameObject = mbDynamicObjectsInCombinedMesh[i];
				return true;
			}
		}
		UnityEngine.Debug.LogError("Could not find a cached game object matching InstanceID: " + instanceID);
		dgoGameObject = null;
		return false;
	}

	public override int GetNumObjectsInCombined()
	{
		return mbDynamicObjectsInCombinedMesh.Count;
	}

	public override List<GameObject> GetObjectsInCombined()
	{
		List<GameObject> list = new List<GameObject>();
		list.AddRange(objectsInCombinedMesh);
		return list;
	}

	public Mesh GetMesh()
	{
		if (_mesh == null)
		{
			_mesh = _NewMesh();
		}
		return _mesh;
	}

	public MeshCreationConditions SetMesh(Mesh m)
	{
		if (m == null)
		{
			_meshBirth = MeshCreationConditions.NoMesh;
		}
		else
		{
			_meshBirth = MeshCreationConditions.AssignedByUser;
		}
		_mesh = m;
		return _meshBirth;
	}

	public Transform[] GetBones()
	{
		return bones;
	}

	public override int GetLightmapIndex()
	{
		if (base.settings.lightmapOption == MB2_LightmapOptions.generate_new_UV2_layout || base.settings.lightmapOption == MB2_LightmapOptions.preserve_current_lightmapping)
		{
			return lightmapIndex;
		}
		return -1;
	}

	private bool _Initialize(int numResultMats)
	{
		if (mbDynamicObjectsInCombinedMesh.Count == 0)
		{
			lightmapIndex = -1;
		}
		if (_mesh == null)
		{
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				MB2_Log.LogDebug("_initialize Creating new Mesh");
			}
			_mesh = GetMesh();
		}
		if (instance2Combined_MapCount() != mbDynamicObjectsInCombinedMesh.Count)
		{
			instance2Combined_MapClear();
			for (int i = 0; i < mbDynamicObjectsInCombinedMesh.Count; i++)
			{
				if (mbDynamicObjectsInCombinedMesh[i] != null)
				{
					if (mbDynamicObjectsInCombinedMesh[i].gameObject == null)
					{
						UnityEngine.Debug.LogError("This MeshBaker contains information from a previous bake that is incomlete. It may have been baked by a previous version of Mesh Baker. If you are trying to update/modify a previously baked combined mesh. Try doing the original bake.");
						return false;
					}
					instance2Combined_MapAdd(mbDynamicObjectsInCombinedMesh[i].gameObject, mbDynamicObjectsInCombinedMesh[i]);
				}
			}
		}
		if (LOG_LEVEL >= MB2_LogLevel.trace)
		{
			UnityEngine.Debug.Log($"_initialize numObjsInCombined={mbDynamicObjectsInCombinedMesh.Count}");
		}
		return true;
	}

	private bool _collectMaterialTriangles(Mesh m, MB_DynamicGameObject dgo, Material[] sharedMaterials, OrderedDictionary sourceMats2submeshIdx_map)
	{
		int num = m.subMeshCount;
		if (sharedMaterials.Length < num)
		{
			num = sharedMaterials.Length;
		}
		dgo._tmpSubmeshTris = new SerializableIntArray[num];
		dgo.targetSubmeshIdxs = new int[num];
		for (int i = 0; i < num; i++)
		{
			if (_textureBakeResults.doMultiMaterial || _textureBakeResults.resultType == MB2_TextureBakeResults.ResultType.textureArray)
			{
				if (!sourceMats2submeshIdx_map.Contains(sharedMaterials[i]))
				{
					UnityEngine.Debug.LogError("Object " + dgo.name + " has a material that was not found in the result materials maping. " + sharedMaterials[i]);
					return false;
				}
				dgo.targetSubmeshIdxs[i] = (int)sourceMats2submeshIdx_map[sharedMaterials[i]];
			}
			else
			{
				dgo.targetSubmeshIdxs[i] = 0;
			}
			dgo._tmpSubmeshTris[i] = new SerializableIntArray();
			dgo._tmpSubmeshTris[i].data = m.GetTriangles(i);
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				MB2_Log.LogDebug("Collecting triangles for: " + dgo.name + " submesh:" + i + " maps to submesh:" + dgo.targetSubmeshIdxs[i] + " added:" + dgo._tmpSubmeshTris[i].data.Length, LOG_LEVEL);
			}
		}
		return true;
	}

	private bool _collectOutOfBoundsUVRects2(Mesh m, MB_DynamicGameObject dgo, Material[] sharedMaterials, OrderedDictionary sourceMats2submeshIdx_map, Dictionary<int, MB_Utility.MeshAnalysisResult[]> meshAnalysisResults)
	{
		if (_textureBakeResults == null)
		{
			UnityEngine.Debug.LogError("Need to bake textures into combined material");
			return false;
		}
		if (!meshAnalysisResults.TryGetValue(m.GetInstanceID(), out var value))
		{
			int subMeshCount = m.subMeshCount;
			value = new MB_Utility.MeshAnalysisResult[subMeshCount];
			for (int i = 0; i < subMeshCount; i++)
			{
				_meshChannelsCache.hasOutOfBoundsUVs(m, ref value[i], i);
			}
			meshAnalysisResults.Add(m.GetInstanceID(), value);
		}
		int num = sharedMaterials.Length;
		if (num > m.subMeshCount)
		{
			num = m.subMeshCount;
		}
		dgo.obUVRects = new Rect[num];
		for (int j = 0; j < num; j++)
		{
			int idxInSrcMats = dgo.targetSubmeshIdxs[j];
			if (_textureBakeResults.GetConsiderMeshUVs(idxInSrcMats, sharedMaterials[j]))
			{
				dgo.obUVRects[j] = value[j].uvRect;
			}
		}
		return true;
	}

	private bool _validateTextureBakeResults()
	{
		if (_textureBakeResults == null)
		{
			UnityEngine.Debug.LogError("Texture Bake Results is null. Can't combine meshes.");
			return false;
		}
		if (_textureBakeResults.materialsAndUVRects == null || _textureBakeResults.materialsAndUVRects.Length == 0)
		{
			UnityEngine.Debug.LogError("Texture Bake Results has no materials in material to sourceUVRect map. Try baking materials. Can't combine meshes. If you are trying to combine meshes without combining materials, try removing the Texture Bake Result.");
			return false;
		}
		if (_textureBakeResults.NumResultMaterials() == 0)
		{
			UnityEngine.Debug.LogError("Texture Bake Results has no result materials. Try baking materials. Can't combine meshes.");
			return false;
		}
		return true;
	}

	internal bool _ShowHide(GameObject[] goToShow, GameObject[] goToHide)
	{
		if (goToShow == null)
		{
			goToShow = empty;
		}
		if (goToHide == null)
		{
			goToHide = empty;
		}
		int numResultMats = _textureBakeResults.NumResultMaterials();
		if (!_Initialize(numResultMats))
		{
			return false;
		}
		for (int i = 0; i < goToHide.Length; i++)
		{
			if (!instance2Combined_MapContainsKey(goToHide[i]))
			{
				if (LOG_LEVEL >= MB2_LogLevel.warn)
				{
					UnityEngine.Debug.LogWarning("Trying to hide an object " + goToHide[i]?.ToString() + " that is not in combined mesh. Did you initially bake with 'clear buffers after bake' enabled?");
				}
				return false;
			}
		}
		for (int j = 0; j < goToShow.Length; j++)
		{
			if (!instance2Combined_MapContainsKey(goToShow[j]))
			{
				if (LOG_LEVEL >= MB2_LogLevel.warn)
				{
					UnityEngine.Debug.LogWarning("Trying to show an object " + goToShow[j]?.ToString() + " that is not in combined mesh. Did you initially bake with 'clear buffers after bake' enabled?");
				}
				return false;
			}
		}
		for (int k = 0; k < goToHide.Length; k++)
		{
			_instance2combined_map[goToHide[k]].show = false;
		}
		for (int l = 0; l < goToShow.Length; l++)
		{
			_instance2combined_map[goToShow[l]].show = true;
		}
		if (_vertexAndTriProcessor != null && !_vertexAndTriProcessor.IsDisposed())
		{
			_vertexAndTriProcessor.Dispose();
		}
		bool flag = _UseNativeArrayAPIorNot();
		_vertexAndTriProcessor = Create_VertexAndTriangleProcessor(flag);
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			if (flag)
			{
				UnityEngine.Debug.Log("using NativeArray mesh API");
			}
			else
			{
				UnityEngine.Debug.Log("using simple mesh API");
			}
		}
		bool flag2 = false;
		try
		{
			flag2 = MB_MeshCombinerSingle_SubCombiner._ShowHideGameObjects(this);
			if (flag2)
			{
				_bakeStatus = MeshCombiningStatus.readyForApply;
			}
		}
		catch
		{
			flag2 = false;
			throw;
		}
		return flag2;
	}

	internal bool _AddToCombined(GameObject[] goToAdd, int[] goToDelete, bool disableRendererInSource)
	{
		Stopwatch stopwatch = null;
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			stopwatch = new Stopwatch();
			stopwatch.Start();
		}
		if (!_validateTextureBakeResults())
		{
			return false;
		}
		if (!ValidateTargRendererAndMeshAndResultSceneObj())
		{
			return false;
		}
		if (outputOption != MB2_OutputOptions.bakeMeshAssetsInPlace && base.settings.renderType == MB_RenderType.skinnedMeshRenderer && (_targetRenderer == null || !(_targetRenderer is SkinnedMeshRenderer)))
		{
			UnityEngine.Debug.LogError("Target renderer must be set and must be a SkinnedMeshRenderer");
			return false;
		}
		if (base.settings.doBlendShapes && base.settings.renderType != MB_RenderType.skinnedMeshRenderer)
		{
			UnityEngine.Debug.LogError("If doBlendShapes is set then RenderType must be skinnedMeshRenderer.");
			return false;
		}
		GameObject[] array = ((goToAdd != null) ? ((GameObject[])goToAdd.Clone()) : empty);
		int[] array2 = ((goToDelete != null) ? ((int[])goToDelete.Clone()) : emptyIDs);
		if (_mesh == null)
		{
			DestroyMesh();
		}
		int numResultMats = _textureBakeResults.NumResultMaterials();
		if (!_Initialize(numResultMats))
		{
			return false;
		}
		if (_mesh.vertexCount > 0 && _instance2combined_map.Count == 0)
		{
			UnityEngine.Debug.LogWarning("There were vertices in the combined mesh but nothing in the MeshBaker buffers. If you are trying to bake in the editor and modify at runtime, make sure 'Clear Buffers After Bake' is unchecked.");
		}
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("==== Calling _addToCombined objs adding:" + array.Length + " objs deleting:" + array2.Length + " fixOutOfBounds:" + textureBakeResults.DoAnyResultMatsUseConsiderMeshUVs() + " doMultiMaterial:" + textureBakeResults.doMultiMaterial + " disableRenderersInSource:" + disableRendererInSource);
			if (LOG_LEVEL >= MB2_LogLevel.trace)
			{
				for (int i = 0; i < array.Length; i++)
				{
					stringBuilder.AppendLine("    adding obj[" + i + "]=" + array[i]);
				}
				HashSet<int> hashSet = new HashSet<int>(array2);
				for (int j = 0; j < objectsInCombinedMesh.Count; j++)
				{
					if (!hashSet.Contains(objectsInCombinedMesh[j].gameObject.GetInstanceID()))
					{
						stringBuilder.AppendLine("    keeping in combined:" + objectsInCombinedMesh[j]);
					}
					else
					{
						stringBuilder.AppendLine("    deleting in combined:" + objectsInCombinedMesh[j]);
					}
				}
			}
			UnityEngine.Debug.Log(stringBuilder);
		}
		if (_textureBakeResults.NumResultMaterials() == 0)
		{
			UnityEngine.Debug.LogError("No resultMaterials in this TextureBakeResults. Try baking textures.");
			return false;
		}
		if (!base.settings.clearBuffersAfterBake && mbDynamicObjectsInCombinedMesh.Count > 0)
		{
			if (_mesh == null)
			{
				UnityEngine.Debug.LogError("Trying to add and delete to a combined mesh that was previously baked but the mesh is null.");
				return false;
			}
			if (_mesh.vertexCount != bufferDataFromPrevious.numVertsBaked)
			{
				UnityEngine.Debug.LogError("Trying to add and delete to a combined mesh that was previously baked but the mesh vertex count is different. " + _mesh.vertexCount + " != " + bufferDataFromPrevious.numVertsBaked);
				return false;
			}
		}
		OrderedDictionary orderedDictionary = BuildSourceMatsToSubmeshIdxMap(numResultMats);
		if (orderedDictionary == null)
		{
			return false;
		}
		bool uvsSliceIdx_w = base.settings.doUV && textureBakeResults.resultType == MB2_TextureBakeResults.ResultType.textureArray;
		MB_MeshVertexChannelFlags meshChannelsAsFlags = MeshBakerSettingsUtility.GetMeshChannelsAsFlags(base.settings, doVerts: true, uvsSliceIdx_w);
		if (!base.settings.clearBuffersAfterBake && channelsLastBake != 0 && mbDynamicObjectsInCombinedMesh.Count > 0 && channelsLastBake != meshChannelsAsFlags)
		{
			UnityEngine.Debug.LogError("There is data in the combined mesh and channels have changed since previous bake. Can't bake:\n channelsLastBake:" + channelsLastBake.ToString() + "\n channels current bake: " + meshChannelsAsFlags);
			return false;
		}
		if (_vertexAndTriProcessor != null && !_vertexAndTriProcessor.IsDisposed())
		{
			_vertexAndTriProcessor.Dispose();
		}
		bool flag = _UseNativeArrayAPIorNot();
		_meshChannelsCache = Create_MeshChannelsCache(flag, LOG_LEVEL, base.settings.lightmapOption);
		_vertexAndTriProcessor = Create_VertexAndTriangleProcessor(flag);
		IVertexAndTriangleProcessor oldMeshData = Create_VertexAndTriangleProcessor(flag);
		_blendShapeProcessor = new MB_MeshCombinerSingle_BlendShapeProcessor(this);
		_boneProcessor = Create_BoneProcessor(flag);
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			if (flag)
			{
				UnityEngine.Debug.Log("using NativeArray mesh API");
			}
			else
			{
				UnityEngine.Debug.Log("using simple mesh API");
			}
		}
		bool flag2 = false;
		try
		{
			return __AddToCombined(array, array2, disableRendererInSource, numResultMats, orderedDictionary, ref oldMeshData, meshChannelsAsFlags, stopwatch);
		}
		catch
		{
			flag2 = false;
			throw;
		}
		finally
		{
			_meshChannelsCache.Dispose();
			_boneProcessor.DisposeOfTemporarySMRData();
			oldMeshData.Dispose();
			for (int k = 0; k < mbDynamicObjectsInCombinedMesh.Count; k++)
			{
				mbDynamicObjectsInCombinedMesh[k].UnInitialize();
			}
		}
	}

	internal bool __AddToCombined(GameObject[] _goToAdd, int[] _goToDelete, bool disableRendererInSource, int numResultMats, OrderedDictionary sourceMats2submeshIdx_map, ref IVertexAndTriangleProcessor oldMeshData, MB_MeshVertexChannelFlags newChannels, Stopwatch sw)
	{
		if (textureBakeResults.resultType == MB2_TextureBakeResults.ResultType.textureArray && base.settings.assignToMeshCustomizer == null)
		{
			UnityEngine.Debug.LogError("Baking combined mesh failed because textures were baked into TextureArrays and no AssignToMeshCustomizer was assigned in the Mesh Baker Settings.");
			return false;
		}
		UVAdjuster_Atlas uVAdjuster_Atlas = new UVAdjuster_Atlas(textureBakeResults, LOG_LEVEL);
		List<MB_DynamicGameObject> list = new List<MB_DynamicGameObject>();
		int i;
		for (i = 0; i < _goToAdd.Length; i++)
		{
			if (!instance2Combined_MapContainsKey(_goToAdd[i]) || Array.FindIndex(_goToDelete, (int o) => o == _goToAdd[i].GetInstanceID()) != -1)
			{
				MB_DynamicGameObject mB_DynamicGameObject = new MB_DynamicGameObject();
				mB_DynamicGameObject.InitializeNew(beingDeleted: false, _goToAdd[i]);
				if (mB_DynamicGameObject._renderer == null)
				{
					UnityEngine.Debug.LogError("Object " + mB_DynamicGameObject.gameObject.name + " does not have a Renderer");
					_goToAdd[i] = null;
					return false;
				}
				Material[] array = mB_DynamicGameObject._renderer.sharedMaterials;
				if (LOG_LEVEL >= MB2_LogLevel.trace)
				{
					UnityEngine.Debug.Log($"Getting {array.Length} shared materials for {mB_DynamicGameObject.gameObject}");
				}
				if (array == null)
				{
					UnityEngine.Debug.LogError("Object " + mB_DynamicGameObject.name + " does not have a Renderer");
					_goToAdd[i] = null;
					return false;
				}
				Mesh mesh = mB_DynamicGameObject._mesh;
				if (mesh == null)
				{
					UnityEngine.Debug.LogError("Object " + mB_DynamicGameObject.gameObject.name + " MeshFilter or SkinnedMeshRenderer had no mesh");
					_goToAdd[i] = null;
					return false;
				}
				if (MBVersion.IsRunningAndMeshNotReadWriteable(mesh))
				{
					UnityEngine.Debug.LogError("Object " + mB_DynamicGameObject.gameObject.name + " Mesh Importer has read/write flag set to 'false'. This needs to be set to 'true' in order to read data from this mesh.");
					_goToAdd[i] = null;
					return false;
				}
				if (array.Length > mesh.subMeshCount)
				{
					Array.Resize(ref array, mesh.subMeshCount);
				}
				if (_goToAdd[i] != null)
				{
					list.Add(mB_DynamicGameObject);
					mB_DynamicGameObject.name = $"{_goToAdd[i].ToString()} {_goToAdd[i].GetInstanceID()}";
					mB_DynamicGameObject.instanceID = _goToAdd[i].GetInstanceID();
					mB_DynamicGameObject.gameObject = _goToAdd[i];
					mB_DynamicGameObject.numVerts = mesh.vertexCount;
					mB_DynamicGameObject.sourceSharedMaterials = array;
				}
			}
			else
			{
				if (LOG_LEVEL >= MB2_LogLevel.warn)
				{
					UnityEngine.Debug.LogWarning("Object " + _goToAdd[i].name + " has already been added. This MeshBaker may have been baked previously with 'Clear Buffers After Bake' unchecked. You can clear the buffers by checking 'Clear Buffers After Bake' and baking. If you want to update a combined mesh by baking several times, you should uncheck 'Clear Buffers After Bake'.");
				}
				_goToAdd[i] = null;
			}
		}
		for (int j = 0; j < mbDynamicObjectsInCombinedMesh.Count; j++)
		{
			if (!mbDynamicObjectsInCombinedMesh[j]._beingDeleted)
			{
				MB_DynamicGameObject mB_DynamicGameObject2 = mbDynamicObjectsInCombinedMesh[j];
				if (!mB_DynamicGameObject2.Initialize(beingDeleted: false))
				{
					UnityEngine.Debug.LogError("Object " + mB_DynamicGameObject2.gameObject.name + " does not have a Renderer");
					return false;
				}
			}
		}
		db_addDeleteGameObjects_CollectMeshData.Start();
		db_addDeleteGameObjects_CollectMeshData_a.Start();
		_meshChannelsCache.CollectChannelDataForAllMeshesInList(mbDynamicObjectsInCombinedMesh, list, newChannels, base.settings.renderType, base.settings.doBlendShapes);
		db_addDeleteGameObjects_CollectMeshData_a.Stop();
		int num = 0;
		int[] array2 = new int[numResultMats];
		int num2 = 0;
		_boneProcessor.BuildBoneIdx2DGOMapIfNecessary(_goToDelete);
		for (int k = 0; k < _goToDelete.Length; k++)
		{
			MB_DynamicGameObject dgoGameObject = null;
			InstanceID2DGO(_goToDelete[k], out dgoGameObject);
			if (dgoGameObject != null)
			{
				dgoGameObject.Initialize(beingDeleted: true);
				num += dgoGameObject.numVerts;
				num2 += dgoGameObject.numBlendShapes;
				if (base.settings.renderType == MB_RenderType.skinnedMeshRenderer)
				{
					_boneProcessor.RemoveBonesForDgosWeAreDeleting(dgoGameObject);
				}
				for (int l = 0; l < dgoGameObject.submeshNumTris.Length; l++)
				{
					array2[l] += dgoGameObject.submeshNumTris[l];
				}
			}
			else if (LOG_LEVEL >= MB2_LogLevel.warn)
			{
				UnityEngine.Debug.LogWarning("Trying to delete an object that is not in combined mesh");
			}
		}
		db_addDeleteGameObjects_CollectMeshData_b.Start();
		for (int m = 0; m < mbDynamicObjectsInCombinedMesh.Count; m++)
		{
			if (!mbDynamicObjectsInCombinedMesh[m]._beingDeleted)
			{
				MB_DynamicGameObject mB_DynamicGameObject3 = mbDynamicObjectsInCombinedMesh[m];
				if (base.settings.renderType == MB_RenderType.skinnedMeshRenderer && !_boneProcessor.GetCachedSMRMeshData(mB_DynamicGameObject3))
				{
					UnityEngine.Debug.LogError("Object " + mB_DynamicGameObject3.gameObject.name + " could not retrieve skinning data");
					return false;
				}
			}
		}
		db_addDeleteGameObjects_CollectMeshData_b.Stop();
		db_addDeleteGameObjects_CollectMeshData.Stop();
		Dictionary<int, MB_Utility.MeshAnalysisResult[]> dictionary = new Dictionary<int, MB_Utility.MeshAnalysisResult[]>();
		int num3 = 0;
		int[] array3 = new int[numResultMats];
		int num4 = 0;
		for (int n = 0; n < list.Count; n++)
		{
			MB_DynamicGameObject mB_DynamicGameObject4 = list[n];
			Mesh mesh2 = mB_DynamicGameObject4._mesh;
			Material[] sourceSharedMaterials = mB_DynamicGameObject4.sourceSharedMaterials;
			if (!uVAdjuster_Atlas.MapSharedMaterialsToAtlasRects(sourceSharedMaterials, checkTargetSubmeshIdxsFromPreviousBake: false, mesh2, _meshChannelsCache, dictionary, sourceMats2submeshIdx_map, mB_DynamicGameObject4.gameObject, mB_DynamicGameObject4))
			{
				_goToAdd[n] = null;
				return false;
			}
			if (!(_goToAdd[n] != null))
			{
				continue;
			}
			if (base.settings.doBlendShapes)
			{
				mB_DynamicGameObject4.numBlendShapes = mesh2.blendShapeCount;
			}
			Renderer renderer = mB_DynamicGameObject4._renderer;
			if (lightmapIndex == -1)
			{
				lightmapIndex = renderer.lightmapIndex;
			}
			if (base.settings.lightmapOption == MB2_LightmapOptions.preserve_current_lightmapping)
			{
				if (lightmapIndex != renderer.lightmapIndex && LOG_LEVEL >= MB2_LogLevel.warn)
				{
					UnityEngine.Debug.LogWarning("Object " + mB_DynamicGameObject4.gameObject.name + " has a different lightmap index. Lightmapping will not work.");
				}
				if (!MBVersion.GetActive(mB_DynamicGameObject4.gameObject) && LOG_LEVEL >= MB2_LogLevel.warn)
				{
					UnityEngine.Debug.LogWarning("Object " + mB_DynamicGameObject4.gameObject.name + " is inactive. Can only get lightmap index of active objects.");
				}
				if (renderer.lightmapIndex == -1 && LOG_LEVEL >= MB2_LogLevel.warn)
				{
					UnityEngine.Debug.LogWarning("Object " + mB_DynamicGameObject4.gameObject.name + " does not have an index to a lightmap.");
				}
			}
			mB_DynamicGameObject4.lightmapIndex = renderer.lightmapIndex;
			mB_DynamicGameObject4.lightmapTilingOffset = MBVersion.GetLightmapTilingOffset(renderer);
			if (!_collectMaterialTriangles(mesh2, mB_DynamicGameObject4, sourceSharedMaterials, sourceMats2submeshIdx_map))
			{
				return false;
			}
			mB_DynamicGameObject4.meshSize = renderer.bounds.size;
			mB_DynamicGameObject4.submeshNumTris = new int[numResultMats];
			mB_DynamicGameObject4.submeshTriIdxs = new int[numResultMats];
			mB_DynamicGameObject4.sourceSharedMaterials = sourceSharedMaterials;
			if (textureBakeResults.DoAnyResultMatsUseConsiderMeshUVs() && !_collectOutOfBoundsUVRects2(mesh2, mB_DynamicGameObject4, sourceSharedMaterials, sourceMats2submeshIdx_map, dictionary))
			{
				return false;
			}
			if (base.settings.renderType == MB_RenderType.skinnedMeshRenderer)
			{
				db_addDeleteGameObjects_CollectMeshData.Start();
				db_addDeleteGameObjects_CollectMeshData_c.Start();
				if (!_boneProcessor.GetCachedSMRMeshData(mB_DynamicGameObject4))
				{
					return false;
				}
				db_addDeleteGameObjects_CollectMeshData_c.Stop();
				db_addDeleteGameObjects_CollectMeshData.Stop();
			}
			if (base.settings.assignToMeshCustomizer != null)
			{
				if (_UseNativeArrayAPIorNot())
				{
					if (!(base.settings.assignToMeshCustomizer is IAssignToMeshCustomizer_NativeArrays))
					{
						UnityEngine.Debug.LogError("Assign To Mesh Customizer must implement IAssignToMeshCustomizer_NativeArrays");
						return false;
					}
				}
				else if (!(base.settings.assignToMeshCustomizer is IAssignToMeshCustomizer_SimpleAPI))
				{
					UnityEngine.Debug.LogError("Assign To Mesh Customizer must implemennt IAssignToMeshCustomizer_SimpleAPI");
					return false;
				}
			}
			num3 += mB_DynamicGameObject4.numVerts;
			num4 += mB_DynamicGameObject4.numBlendShapes;
			for (int num5 = 0; num5 < mB_DynamicGameObject4._tmpSubmeshTris.Length; num5++)
			{
				array3[mB_DynamicGameObject4.targetSubmeshIdxs[num5]] += mB_DynamicGameObject4._tmpSubmeshTris[num5].data.Length;
			}
			mB_DynamicGameObject4.invertTriangles = IsMirrored(mB_DynamicGameObject4.gameObject.transform.localToWorldMatrix);
		}
		for (int num6 = 0; num6 < _goToAdd.Length; num6++)
		{
			if (_goToAdd[num6] != null && disableRendererInSource)
			{
				MB_Utility.DisableRendererInSource(_goToAdd[num6]);
				if (LOG_LEVEL == MB2_LogLevel.trace)
				{
					UnityEngine.Debug.Log("Disabling renderer on " + _goToAdd[num6].name + " id=" + _goToAdd[num6].GetInstanceID());
				}
			}
		}
		bool num7 = MB_MeshCombinerSingle_SubCombiner._AddToCombined(this, newChannels, num3, num, numResultMats, num4, num2, array3, array2, _goToDelete, list, _goToAdd, uVAdjuster_Atlas, ref oldMeshData, sw);
		if (num7)
		{
			_bakeStatus = MeshCombiningStatus.readyForApply;
		}
		return num7;
	}

	private Transform[] _getBones(Renderer r, bool isSkinnedMeshWithBones)
	{
		return MBVersion.GetBones(r, isSkinnedMeshWithBones);
	}

	public override bool Apply(GenerateUV2Delegate uv2GenerationMethod)
	{
		db_apply.Start();
		bool result = MB_MeshCombinerSingle_SubCombiner.Apply(this, uv2GenerationMethod);
		db_apply.Stop();
		return result;
	}

	public virtual void ApplyShowHide()
	{
		db_applyShowHide.Start();
		if (_validationLevel < MB2_ValidationLevel.quick || ValidateTargRendererAndMeshAndResultSceneObj())
		{
			MB_MeshCombinerSingle_SubCombiner.ApplyShowHide(this);
			db_applyShowHide.Stop();
		}
	}

	public override bool Apply(bool triangles, bool vertices, bool normals, bool tangents, bool uvs, bool uv2, bool uv3, bool uv4, bool colors, bool bones = false, bool blendShapesFlag = false, GenerateUV2Delegate uv2GenerationMethod = null)
	{
		db_apply.Start();
		bool result = MB_MeshCombinerSingle_SubCombiner.Apply(this, triangles, vertices, normals, tangents, uvs, uv2, uv3, uv4, colors, bones, blendShapesFlag, uv2GenerationMethod);
		db_apply.Stop();
		return result;
	}

	public override bool Apply(bool triangles, bool vertices, bool normals, bool tangents, bool uvs, bool uv2, bool uv3, bool uv4, bool uv5, bool uv6, bool uv7, bool uv8, bool colors, bool bones = false, bool blendShapesFlag = false, GenerateUV2Delegate uv2GenerationMethod = null)
	{
		db_apply.Start();
		bool result = MB_MeshCombinerSingle_SubCombiner.Apply(this, triangles, vertices, normals, tangents, uvs, uv2, uv3, uv4, uv5, uv6, uv7, uv8, colors, bones, blendShapesFlag, suppressClearMesh: false, uv2GenerationMethod);
		db_apply.Stop();
		return result;
	}

	public override bool UpdateGameObjects(GameObject[] gos, bool recalcBounds, bool updateVertices, bool updateNormals, bool updateTangents, bool updateUV, bool updateUV2, bool updateUV3, bool updateUV4, bool updateColors, bool updateSkinningInfo)
	{
		db_updateGameObjects.Start();
		bool result = _UpdateGameObjects(gos, recalcBounds, updateVertices, updateNormals, updateTangents, updateUV, updateUV2, updateUV3, updateUV4, updateUV5: false, updateUV6: false, updateUV7: false, updateUV8: false, updateColors, updateSkinningInfo);
		db_updateGameObjects.Stop();
		return result;
	}

	public override bool UpdateGameObjects(GameObject[] gos, bool recalcBounds, bool updateVertices, bool updateNormals, bool updateTangents, bool updateUV, bool updateUV2, bool updateUV3, bool updateUV4, bool updateUV5, bool updateUV6, bool updateUV7, bool updateUV8, bool updateColors, bool updateSkinningInfo)
	{
		db_updateGameObjects.Start();
		bool result = _UpdateGameObjects(gos, recalcBounds, updateVertices, updateNormals, updateTangents, updateUV, updateUV2, updateUV3, updateUV4, updateUV5, updateUV6, updateUV7, updateUV8, updateColors, updateSkinningInfo);
		db_updateGameObjects.Stop();
		return result;
	}

	internal bool _UpdateGameObjects(GameObject[] gos, bool recalcBounds, bool updateVertices, bool updateNormals, bool updateTangents, bool updateUV, bool updateUV2, bool updateUV3, bool updateUV4, bool updateUV5, bool updateUV6, bool updateUV7, bool updateUV8, bool updateColors, bool updateSkinningInfo)
	{
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			UnityEngine.Debug.Log("UpdateGameObjects called on " + gos.Length + " objects.");
		}
		int numResultMats = 1;
		if (textureBakeResults.doMultiMaterial)
		{
			numResultMats = textureBakeResults.NumResultMaterials();
		}
		if (!_Initialize(numResultMats))
		{
			return false;
		}
		bool uvsSliceIdx_w = base.settings.doUV && textureBakeResults.resultType == MB2_TextureBakeResults.ResultType.textureArray;
		MB_MeshVertexChannelFlags meshChannelsAsFlags = MeshBakerSettingsUtility.GetMeshChannelsAsFlags(base.settings, doVerts: true, uvsSliceIdx_w);
		if (channelsLastBake != meshChannelsAsFlags)
		{
			UnityEngine.Debug.LogError("Channels changed since previous bake. Can't Update GameObjects.");
			return false;
		}
		if (_bakeStatus != 0)
		{
			UnityEngine.Debug.LogError("Bake Status of combiner was not 'preAddDeleteOrUpdate'. This can happen if AddDeleteGameObjects or UpdateGameObjects is called twice without calling Apply. You can call 'ClearBuffers' to reset the combiner.");
			return false;
		}
		if (_mesh.vertexCount > 0 && _instance2combined_map.Count == 0)
		{
			UnityEngine.Debug.LogWarning("There were vertices in the combined mesh but nothing in the MeshBaker buffers. If you are trying to bake in the editor and modify at runtime, make sure 'Clear Buffers After Bake' is unchecked.");
		}
		if (base.settings.assignToMeshCustomizer != null)
		{
			if (_UseNativeArrayAPIorNot())
			{
				if (!(base.settings.assignToMeshCustomizer is IAssignToMeshCustomizer_NativeArrays))
				{
					UnityEngine.Debug.LogError("Assign To Mesh Customizer must implement IAssignToMeshCustomizer_NativeArrays");
					return false;
				}
			}
			else if (!(base.settings.assignToMeshCustomizer is IAssignToMeshCustomizer_SimpleAPI))
			{
				UnityEngine.Debug.LogError("Assign To Mesh Customizer must implemennt IAssignToMeshCustomizer_SimpleAPI");
				return false;
			}
		}
		UVAdjuster_Atlas uVAdjuster = null;
		OrderedDictionary orderedDictionary = null;
		Dictionary<int, MB_Utility.MeshAnalysisResult[]> meshAnalysisResultsCache = null;
		if (updateUV)
		{
			orderedDictionary = BuildSourceMatsToSubmeshIdxMap(numResultMats);
			if (orderedDictionary == null)
			{
				return false;
			}
			uVAdjuster = new UVAdjuster_Atlas(textureBakeResults, LOG_LEVEL);
			meshAnalysisResultsCache = new Dictionary<int, MB_Utility.MeshAnalysisResult[]>();
		}
		if (_vertexAndTriProcessor != null && !_vertexAndTriProcessor.IsDisposed())
		{
			_vertexAndTriProcessor.Dispose();
		}
		_blendShapeProcessor = new MB_MeshCombinerSingle_BlendShapeProcessor(this);
		bool flag = _UseNativeArrayAPIorNot();
		_meshChannelsCache = Create_MeshChannelsCache(flag, LOG_LEVEL, base.settings.lightmapOption);
		_vertexAndTriProcessor = Create_VertexAndTriangleProcessor(flag);
		_boneProcessor = Create_BoneProcessor(flag);
		if (LOG_LEVEL >= MB2_LogLevel.debug)
		{
			if (flag)
			{
				UnityEngine.Debug.Log("using NativeArray mesh API");
			}
			else
			{
				UnityEngine.Debug.Log("using simple mesh API");
			}
		}
		bool flag2 = true;
		try
		{
			return flag2 && __UpdateGameObjects(gos, recalcBounds, meshChannelsAsFlags, updateVertices, updateNormals, updateTangents, updateUV, updateUV2, updateUV3, updateUV4, updateUV5, updateUV6, updateUV7, updateUV8, updateColors, updateSkinningInfo, meshAnalysisResultsCache, orderedDictionary, uVAdjuster);
		}
		catch
		{
			flag2 = false;
			throw;
		}
		finally
		{
			_meshChannelsCache.Dispose();
			_boneProcessor.DisposeOfTemporarySMRData();
			for (int i = 0; i < mbDynamicObjectsInCombinedMesh.Count; i++)
			{
				MB_DynamicGameObject mB_DynamicGameObject = mbDynamicObjectsInCombinedMesh[i];
				if (mB_DynamicGameObject._initialized)
				{
					mB_DynamicGameObject.UnInitialize();
				}
			}
		}
	}

	private bool __UpdateGameObjects(GameObject[] gos, bool recalcBounds, MB_MeshVertexChannelFlags newChannels, bool updateVertices, bool updateNormals, bool updateTangents, bool updateUV, bool updateUV2, bool updateUV3, bool updateUV4, bool updateUV5, bool updateUV6, bool updateUV7, bool updateUV8, bool updateColors, bool updateSkinningInfo, Dictionary<int, MB_Utility.MeshAnalysisResult[]> meshAnalysisResultsCache, OrderedDictionary sourceMats2submeshIdx_map, UVAdjuster_Atlas uVAdjuster)
	{
		List<MB_DynamicGameObject> list = new List<MB_DynamicGameObject>();
		for (int i = 0; i < gos.Length; i++)
		{
			MB_DynamicGameObject mB_DynamicGameObject = _instance2combined_map[gos[i]];
			if (!mB_DynamicGameObject.Initialize(beingDeleted: false))
			{
				UnityEngine.Debug.LogError("Object " + mB_DynamicGameObject.name + " could not be initialized");
				return false;
			}
			list.Add(mB_DynamicGameObject);
			if (mB_DynamicGameObject._mesh == null)
			{
				UnityEngine.Debug.LogError("Object " + mB_DynamicGameObject.name + " had no renderer");
				return false;
			}
			if (mB_DynamicGameObject._renderer == null)
			{
				UnityEngine.Debug.LogError("Object " + mB_DynamicGameObject.name + " had no renderer");
				return false;
			}
			Mesh mesh = mB_DynamicGameObject._mesh;
			if (mB_DynamicGameObject.numVerts != mesh.vertexCount)
			{
				UnityEngine.Debug.LogError("Object " + mB_DynamicGameObject.gameObject.name + " source mesh has been modified since being added. To update it must have the same number of verts");
				return false;
			}
		}
		for (int j = 0; j < mbDynamicObjectsInCombinedMesh.Count; j++)
		{
			if (!mbDynamicObjectsInCombinedMesh[j]._beingDeleted)
			{
				MB_DynamicGameObject mB_DynamicGameObject2 = mbDynamicObjectsInCombinedMesh[j];
				if (!mB_DynamicGameObject2.Initialize(beingDeleted: false))
				{
					UnityEngine.Debug.LogError("Object " + mB_DynamicGameObject2.gameObject.name + " does not have a Renderer");
					return false;
				}
			}
		}
		_meshChannelsCache.CollectChannelDataForAllMeshesInList(mbDynamicObjectsInCombinedMesh, list, newChannels, base.settings.renderType, base.settings.doBlendShapes);
		for (int k = 0; k < gos.Length; k++)
		{
			MB_DynamicGameObject mB_DynamicGameObject3 = _instance2combined_map[gos[k]];
			if (base.settings.doUV && updateUV)
			{
				Material[] sharedMaterials = mB_DynamicGameObject3._renderer.sharedMaterials;
				if (!uVAdjuster.MapSharedMaterialsToAtlasRects(sharedMaterials, checkTargetSubmeshIdxsFromPreviousBake: true, mB_DynamicGameObject3._mesh, _meshChannelsCache, meshAnalysisResultsCache, sourceMats2submeshIdx_map, mB_DynamicGameObject3.gameObject, mB_DynamicGameObject3))
				{
					return false;
				}
			}
		}
		_boneProcessor.BuildBoneIdx2DGOMapIfNecessary(null);
		bool num = MB_MeshCombinerSingle_SubCombiner._UpdateGameObjects(this, list, newChannels, updateVertices, updateNormals, updateTangents, updateUV, updateUV2, updateUV3, updateUV4, updateUV5, updateUV6, updateUV7, updateUV8, updateColors, updateSkinningInfo, uVAdjuster, LOG_LEVEL);
		if (num && recalcBounds)
		{
			_mesh.RecalculateBounds();
		}
		return num;
	}

	public bool ShowHideGameObjects(GameObject[] toShow, GameObject[] toHide)
	{
		db_showHideGameObjects.Start();
		if (textureBakeResults == null)
		{
			UnityEngine.Debug.LogError("TextureBakeResults must be set.");
			return false;
		}
		bool result = _ShowHide(toShow, toHide);
		db_showHideGameObjects.Stop();
		return result;
	}

	public override bool AddDeleteGameObjects(GameObject[] gos, GameObject[] deleteGOs, bool disableRendererInSource = true)
	{
		db_addDeleteGameObjects.Start();
		int[] array = null;
		if (deleteGOs != null)
		{
			array = new int[deleteGOs.Length];
			for (int i = 0; i < deleteGOs.Length; i++)
			{
				if (deleteGOs[i] == null)
				{
					UnityEngine.Debug.LogError("The " + i + "th object on the list of objects to delete is 'Null'");
				}
				else
				{
					array[i] = deleteGOs[i].GetInstanceID();
				}
			}
		}
		bool result = AddDeleteGameObjectsByID(gos, array, disableRendererInSource);
		db_addDeleteGameObjects.Stop();
		return result;
	}

	public override bool AddDeleteGameObjectsByID(GameObject[] gos, int[] deleteGOinstanceIDs, bool disableRendererInSource)
	{
		db_addDeleteGameObjects.Start();
		if (validationLevel > MB2_ValidationLevel.none)
		{
			if (gos != null)
			{
				for (int i = 0; i < gos.Length; i++)
				{
					if (gos[i] == null)
					{
						UnityEngine.Debug.LogError("The " + i + "th object on the list of objects to combine is 'None'. Use Command-Delete on Mac OS X; Delete or Shift-Delete on Windows to remove this one element.");
						return false;
					}
					if (validationLevel < MB2_ValidationLevel.robust)
					{
						continue;
					}
					for (int j = i + 1; j < gos.Length; j++)
					{
						if (gos[i] == gos[j])
						{
							UnityEngine.Debug.LogError("GameObject " + gos[i]?.ToString() + " appears twice in list of game objects to add");
							return false;
						}
					}
				}
			}
			if (deleteGOinstanceIDs != null)
			{
				bool flag = true;
				HashSet<int> hashSet = new HashSet<int>(deleteGOinstanceIDs);
				for (int k = 0; k < mbDynamicObjectsInCombinedMesh.Count; k++)
				{
					if (!hashSet.Contains(mbDynamicObjectsInCombinedMesh[k].instanceID))
					{
						flag = false;
						break;
					}
				}
				if (!flag)
				{
					for (int l = 0; l < mbDynamicObjectsInCombinedMesh.Count; l++)
					{
						if (mbDynamicObjectsInCombinedMesh[l].gameObject == null)
						{
							UnityEngine.Debug.LogError("An instanceID to be deleted does not match any of the cached instanceIDs from the bake and the  corresponding source game object has already been deleted. This can happen if objects were baked, then the scene was saved, closed, opened and a delete bake is attempted. Try deleting a source object from the baked mesh by passing in the source game object instead of the instance ID");
							return false;
						}
						mbDynamicObjectsInCombinedMesh[l].instanceID = mbDynamicObjectsInCombinedMesh[l].gameObject.GetInstanceID();
					}
				}
				if (validationLevel >= MB2_ValidationLevel.robust)
				{
					for (int m = 0; m < deleteGOinstanceIDs.Length; m++)
					{
						for (int n = m + 1; n < deleteGOinstanceIDs.Length; n++)
						{
							if (deleteGOinstanceIDs[m] == deleteGOinstanceIDs[n])
							{
								UnityEngine.Debug.LogError("GameObject " + deleteGOinstanceIDs[m] + "appears twice in list of game objects to delete");
								return false;
							}
						}
					}
				}
			}
		}
		if (_bakeStatus != 0)
		{
			UnityEngine.Debug.LogError("Bake Status of combiner was not 'preAddDeleteOrUpdate'. This can happen if AddDeleteGameObjects or UpdateGameObjects is called twice without calling Apply. You can call 'ClearBuffers' to reset the combiner.");
			return false;
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
		BuildSceneMeshObject(gos);
		if (!_AddToCombined(gos, deleteGOinstanceIDs, disableRendererInSource))
		{
			UnityEngine.Debug.LogError("Failed to add/delete objects to combined mesh");
			return false;
		}
		db_addDeleteGameObjects.Stop();
		return true;
	}

	public override bool CombinedMeshContains(GameObject go)
	{
		return objectsInCombinedMesh.Contains(go);
	}

	public override void ClearBuffers()
	{
		bones = new Transform[0];
		bindPoses = new Matrix4x4[0];
		blendShapes = new MBBlendShape[0];
		mbDynamicObjectsInCombinedMesh.Clear();
		objectsInCombinedMesh.Clear();
		if (_vertexAndTriProcessor != null && !_vertexAndTriProcessor.IsDisposed())
		{
			_vertexAndTriProcessor.Dispose();
		}
		_vertexAndTriProcessor = Create_VertexAndTriangleProcessor(_UseNativeArrayAPIorNot());
		verts = new Vector3[0];
		normals = new Vector3[0];
		tangents = new Vector4[0];
		uvs = new Vector2[0];
		uvsSliceIdx = new float[0];
		uv2s = new Vector2[0];
		uv3s = new Vector2[0];
		uv4s = new Vector2[0];
		uv5s = new Vector2[0];
		uv6s = new Vector2[0];
		uv7s = new Vector2[0];
		uv8s = new Vector2[0];
		colors = new Color[0];
		submeshTris = new SerializableIntArray[0];
		if (submeshTris != null)
		{
			for (int i = 0; i < submeshTris.Length; i++)
			{
				if (submeshTris[i].data == null)
				{
					submeshTris[i].data = new int[0];
				}
				else if (submeshTris[i].data.Length != 0)
				{
					submeshTris[i].data = new int[0];
				}
			}
			submeshTris = null;
		}
		instance2Combined_MapClear();
		if (_usingTemporaryTextureBakeResult)
		{
			MB_Utility.Destroy(_textureBakeResults);
			_textureBakeResults = null;
			_usingTemporaryTextureBakeResult = false;
		}
		_bakeStatus = MeshCombiningStatus.preAddDeleteOrUpdate;
		if (LOG_LEVEL >= MB2_LogLevel.trace)
		{
			MB2_Log.LogDebug("ClearBuffers called");
		}
	}

	private Mesh _NewMesh()
	{
		if (Application.isPlaying)
		{
			_meshBirth = MeshCreationConditions.CreatedAtRuntime;
		}
		else
		{
			_meshBirth = MeshCreationConditions.CreatedInEditor;
		}
		return new Mesh();
	}

	public override void ClearMesh()
	{
		if (_mesh != null)
		{
			MBVersion.MeshClear(_mesh, t: false);
		}
		else
		{
			_mesh = _NewMesh();
		}
		ClearBuffers();
	}

	public override void ClearMesh(MB2_EditorMethodsInterface editorMethods)
	{
		ClearMesh();
	}

	internal override void _DisposeRuntimeCreated()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		if (_meshBirth == MeshCreationConditions.CreatedAtRuntime)
		{
			if (!MBVersion.IsAssetInProject(_mesh))
			{
				UnityEngine.Object.Destroy(_mesh);
			}
		}
		else if (_meshBirth == MeshCreationConditions.AssignedByUser)
		{
			_mesh = null;
		}
		ClearBuffers();
	}

	public override void DestroyMesh()
	{
		if (_mesh != null)
		{
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				MB2_Log.LogDebug("Destroying Mesh");
			}
			MB_Utility.Destroy(_mesh);
			_meshBirth = MeshCreationConditions.NoMesh;
		}
		ClearBuffers();
	}

	public override void DestroyMeshEditor(MB2_EditorMethodsInterface editorMethods)
	{
		if (_mesh != null && editorMethods != null && !Application.isPlaying)
		{
			if (LOG_LEVEL >= MB2_LogLevel.debug)
			{
				MB2_Log.LogDebug("Destroying Mesh");
			}
			editorMethods.Destroy(_mesh);
		}
		ClearBuffers();
	}

	public bool ValidateTargRendererAndMeshAndResultSceneObj()
	{
		if (_resultSceneObject == null)
		{
			if (_LOG_LEVEL >= MB2_LogLevel.error)
			{
				UnityEngine.Debug.LogError("Result Scene Object was not set.");
			}
			return false;
		}
		if (_targetRenderer == null)
		{
			if (_LOG_LEVEL >= MB2_LogLevel.error)
			{
				UnityEngine.Debug.LogError("Target Renderer was not set.");
			}
			return false;
		}
		if (_resultSceneObject != null && _targetRenderer.transform.parent != _resultSceneObject.transform)
		{
			if (_LOG_LEVEL >= MB2_LogLevel.error)
			{
				UnityEngine.Debug.LogError("Target Renderer game object is not a child of Result Scene Object.");
			}
			return false;
		}
		if (base.settings.renderType == MB_RenderType.skinnedMeshRenderer && !(_targetRenderer is SkinnedMeshRenderer))
		{
			if (_LOG_LEVEL >= MB2_LogLevel.error)
			{
				UnityEngine.Debug.LogError("Render Type is skinned mesh renderer but Target Renderer is not.");
			}
			return false;
		}
		if (base.settings.renderType == MB_RenderType.meshRenderer)
		{
			if (!(_targetRenderer is MeshRenderer))
			{
				if (_LOG_LEVEL >= MB2_LogLevel.error)
				{
					UnityEngine.Debug.LogError("Render Type is mesh renderer but Target Renderer is not.");
				}
				return false;
			}
			MeshFilter component = _targetRenderer.GetComponent<MeshFilter>();
			if (_mesh != component.sharedMesh)
			{
				if (_LOG_LEVEL >= MB2_LogLevel.error)
				{
					UnityEngine.Debug.LogError("Target renderer mesh is not equal to mesh.");
				}
				return false;
			}
		}
		return true;
	}

	private OrderedDictionary BuildSourceMatsToSubmeshIdxMap(int numResultMats)
	{
		OrderedDictionary orderedDictionary = new OrderedDictionary();
		for (int i = 0; i < numResultMats; i++)
		{
			List<Material> sourceMaterialsUsedByResultMaterial = _textureBakeResults.GetSourceMaterialsUsedByResultMaterial(i);
			for (int j = 0; j < sourceMaterialsUsedByResultMaterial.Count; j++)
			{
				if (sourceMaterialsUsedByResultMaterial[j] == null)
				{
					UnityEngine.Debug.LogError("Found null material in source materials for combined mesh materials " + i);
					return null;
				}
				if (!orderedDictionary.Contains(sourceMaterialsUsedByResultMaterial[j]))
				{
					orderedDictionary.Add(sourceMaterialsUsedByResultMaterial[j], i);
				}
			}
		}
		return orderedDictionary;
	}

	internal Renderer BuildSceneHierarchPreBake(MB3_MeshCombinerSingle mom, GameObject root, Mesh m, bool createNewChild = false, GameObject[] objsToBeAdded = null)
	{
		if (mom._LOG_LEVEL >= MB2_LogLevel.trace)
		{
			UnityEngine.Debug.Log("Building Scene Hierarchy createNewChild=" + createNewChild);
		}
		MeshFilter meshFilter = null;
		MeshRenderer meshRenderer = null;
		SkinnedMeshRenderer skinnedMeshRenderer = null;
		Transform transform = null;
		if (root == null)
		{
			UnityEngine.Debug.LogError("root was null.");
			return null;
		}
		if (mom.textureBakeResults == null)
		{
			UnityEngine.Debug.LogError("textureBakeResults must be set.");
			return null;
		}
		if (root.GetComponent<Renderer>() != null)
		{
			UnityEngine.Debug.LogError("root game object cannot have a renderer component");
			return null;
		}
		if (!createNewChild)
		{
			if (mom.targetRenderer != null && mom.targetRenderer.transform.parent == root.transform)
			{
				transform = mom.targetRenderer.transform;
			}
			else
			{
				Renderer[] componentsInChildren = root.GetComponentsInChildren<Renderer>(includeInactive: true);
				if (componentsInChildren.Length == 1)
				{
					if (componentsInChildren[0].transform.parent != root.transform)
					{
						UnityEngine.Debug.LogError("Target Renderer is not an immediate child of Result Scene Object. Try using a game object with no children as the Result Scene Object..");
					}
					transform = componentsInChildren[0].transform;
				}
			}
		}
		if (transform != null && transform.parent != root.transform)
		{
			transform = null;
		}
		GameObject gameObject;
		if (transform == null)
		{
			gameObject = new GameObject(mom.name + "-mesh");
			gameObject.transform.parent = root.transform;
			transform = gameObject.transform;
		}
		transform.parent = root.transform;
		gameObject = transform.gameObject;
		if (base.settings.renderType == MB_RenderType.skinnedMeshRenderer)
		{
			MeshRenderer component = gameObject.GetComponent<MeshRenderer>();
			if (component != null)
			{
				MB_Utility.Destroy(component);
			}
			MeshFilter component2 = gameObject.GetComponent<MeshFilter>();
			if (component2 != null)
			{
				MB_Utility.Destroy(component2);
			}
			skinnedMeshRenderer = gameObject.GetComponent<SkinnedMeshRenderer>();
			if (skinnedMeshRenderer == null)
			{
				skinnedMeshRenderer = gameObject.AddComponent<SkinnedMeshRenderer>();
			}
		}
		else
		{
			SkinnedMeshRenderer component3 = gameObject.GetComponent<SkinnedMeshRenderer>();
			if (component3 != null)
			{
				MB_Utility.Destroy(component3);
			}
			meshFilter = gameObject.GetComponent<MeshFilter>();
			if (meshFilter == null)
			{
				meshFilter = gameObject.AddComponent<MeshFilter>();
			}
			meshRenderer = gameObject.GetComponent<MeshRenderer>();
			if (meshRenderer == null)
			{
				meshRenderer = gameObject.AddComponent<MeshRenderer>();
			}
		}
		if (base.settings.renderType == MB_RenderType.skinnedMeshRenderer)
		{
			skinnedMeshRenderer.bones = mom.GetBones();
			bool updateWhenOffscreen = skinnedMeshRenderer.updateWhenOffscreen;
			skinnedMeshRenderer.updateWhenOffscreen = true;
			skinnedMeshRenderer.updateWhenOffscreen = updateWhenOffscreen;
		}
		_ConfigureSceneHierarch(mom, root, meshRenderer, meshFilter, skinnedMeshRenderer, m, objsToBeAdded);
		if (base.settings.renderType == MB_RenderType.skinnedMeshRenderer)
		{
			return skinnedMeshRenderer;
		}
		return meshRenderer;
	}

	private static void _ConfigureSceneHierarch(MB3_MeshCombinerSingle mom, GameObject root, MeshRenderer mr, MeshFilter mf, SkinnedMeshRenderer smr, Mesh m, GameObject[] objsToBeAdded = null)
	{
		GameObject gameObject;
		if (mom.settings.renderType == MB_RenderType.skinnedMeshRenderer)
		{
			gameObject = smr.gameObject;
			smr.lightmapIndex = mom.GetLightmapIndex();
		}
		else
		{
			gameObject = mr.gameObject;
			mf.sharedMesh = m;
			mom._SetLightmapIndexIfPreserveLightmapping(mr);
		}
		if (mom.settings.lightmapOption == MB2_LightmapOptions.preserve_current_lightmapping || mom.settings.lightmapOption == MB2_LightmapOptions.generate_new_UV2_layout)
		{
			gameObject.isStatic = true;
		}
		if (objsToBeAdded == null || objsToBeAdded.Length == 0 || !(objsToBeAdded[0] != null))
		{
			return;
		}
		bool flag = true;
		bool flag2 = true;
		string tag = objsToBeAdded[0].tag;
		int layer = objsToBeAdded[0].layer;
		for (int i = 0; i < objsToBeAdded.Length; i++)
		{
			if (objsToBeAdded[i] != null)
			{
				if (!objsToBeAdded[i].tag.Equals(tag))
				{
					flag = false;
				}
				if (objsToBeAdded[i].layer != layer)
				{
					flag2 = false;
				}
			}
		}
		if (flag)
		{
			root.tag = tag;
			gameObject.tag = tag;
		}
		if (flag2)
		{
			root.layer = layer;
			gameObject.layer = layer;
		}
	}

	private void _SetLightmapIndexIfPreserveLightmapping(Renderer tr)
	{
		tr.lightmapIndex = GetLightmapIndex();
		tr.lightmapScaleOffset = new Vector4(1f, 1f, 0f, 0f);
		if (base.settings.lightmapOption == MB2_LightmapOptions.preserve_current_lightmapping)
		{
			MB_PreserveLightmapData mB_PreserveLightmapData = tr.gameObject.GetComponent<MB_PreserveLightmapData>();
			if (mB_PreserveLightmapData == null)
			{
				mB_PreserveLightmapData = tr.gameObject.AddComponent<MB_PreserveLightmapData>();
			}
			mB_PreserveLightmapData.lightmapIndex = GetLightmapIndex();
			mB_PreserveLightmapData.lightmapScaleOffset = new Vector4(1f, 1f, 0f, 0f);
		}
	}

	public void BuildSceneMeshObject(GameObject[] gos = null, bool createNewChild = false)
	{
		if (_resultSceneObject == null)
		{
			_resultSceneObject = new GameObject("CombinedMesh-" + base.name);
		}
		_targetRenderer = BuildSceneHierarchPreBake(this, _resultSceneObject, GetMesh(), createNewChild, gos);
	}

	private bool IsMirrored(Matrix4x4 tm)
	{
		Vector3 lhs = tm.GetRow(0);
		Vector3 rhs = tm.GetRow(1);
		Vector3 rhs2 = tm.GetRow(2);
		lhs.Normalize();
		rhs.Normalize();
		rhs2.Normalize();
		if (!(Vector3.Dot(Vector3.Cross(lhs, rhs), rhs2) >= 0f))
		{
			return true;
		}
		return false;
	}

	public override void CheckIntegrity()
	{
		if (MB_Utility.DO_INTEGRITY_CHECKS)
		{
			if (_boneProcessor != null)
			{
				_boneProcessor.DB_CheckIntegrity();
			}
			if (base.settings.doBlendShapes && base.settings.renderType != MB_RenderType.skinnedMeshRenderer)
			{
				UnityEngine.Debug.LogError("Blend shapes can only be used with skinned meshes.");
			}
		}
	}

	public override List<Material> GetMaterialsOnTargetRenderer()
	{
		List<Material> list = new List<Material>();
		if (_targetRenderer != null)
		{
			list.AddRange(_targetRenderer.sharedMaterials);
		}
		return list;
	}

	private bool _UseNativeArrayAPIorNot()
	{
		if (base.settings.meshAPI == MB_MeshCombineAPIType.betaNativeArrayAPI)
		{
			return true;
		}
		return false;
	}

	public MB_IMeshCombinerSingle_BoneProcessor Create_BoneProcessor(bool doNativeArrays)
	{
		if (doNativeArrays)
		{
			return new MB_MeshCombinerSingle_BoneProcessorNewAPI(this);
		}
		return new MB_MeshCombinerSingle_BoneProcessor(this);
	}

	public static IVertexAndTriangleProcessor Create_VertexAndTriangleProcessor(bool doNativeArrays)
	{
		IVertexAndTriangleProcessor vertexAndTriangleProcessor = null;
		if (doNativeArrays)
		{
			return default(VertexAndTriangleProcessorNativeArray);
		}
		return default(VertexAndTriangleProcessor);
	}

	public static IMeshChannelsCacheTaggingInterface Create_MeshChannelsCache(bool doNativeArrays, MB2_LogLevel LOG_LEVEL, MB2_LightmapOptions lightmapOption)
	{
		IMeshChannelsCacheTaggingInterface meshChannelsCacheTaggingInterface = null;
		if (doNativeArrays)
		{
			return new MeshChannelsCache_NativeArray(LOG_LEVEL, lightmapOption);
		}
		return new MeshChannelsCache(LOG_LEVEL, lightmapOption);
	}

	public override void UpdateSkinnedMeshApproximateBounds()
	{
		UpdateSkinnedMeshApproximateBoundsFromBounds();
	}

	public override void UpdateSkinnedMeshApproximateBoundsFromBones()
	{
		if (outputOption == MB2_OutputOptions.bakeMeshAssetsInPlace)
		{
			if (LOG_LEVEL >= MB2_LogLevel.warn)
			{
				UnityEngine.Debug.LogWarning("Can't UpdateSkinnedMeshApproximateBounds when output type is bakeMeshAssetsInPlace");
			}
		}
		else if (bones.Length == 0)
		{
			if (GetVertexCount() > 0 && LOG_LEVEL >= MB2_LogLevel.warn)
			{
				UnityEngine.Debug.LogWarning("No bones in SkinnedMeshRenderer. Could not UpdateSkinnedMeshApproximateBounds.");
			}
		}
		else if (_targetRenderer == null)
		{
			if (LOG_LEVEL >= MB2_LogLevel.warn)
			{
				UnityEngine.Debug.LogWarning("Target Renderer is not set. No point in calling UpdateSkinnedMeshApproximateBounds.");
			}
		}
		else if (!_targetRenderer.GetType().Equals(typeof(SkinnedMeshRenderer)))
		{
			if (LOG_LEVEL >= MB2_LogLevel.warn)
			{
				UnityEngine.Debug.LogWarning("Target Renderer is not a SkinnedMeshRenderer. No point in calling UpdateSkinnedMeshApproximateBounds.");
			}
		}
		else
		{
			MB3_MeshCombiner.UpdateSkinnedMeshApproximateBoundsFromBonesStatic(bones, (SkinnedMeshRenderer)targetRenderer);
		}
	}

	public override void UpdateSkinnedMeshApproximateBoundsFromBounds()
	{
		if (outputOption == MB2_OutputOptions.bakeMeshAssetsInPlace)
		{
			if (LOG_LEVEL >= MB2_LogLevel.warn)
			{
				UnityEngine.Debug.LogWarning("Can't UpdateSkinnedMeshApproximateBoundsFromBounds when output type is bakeMeshAssetsInPlace");
			}
		}
		else if (GetVertexCount() == 0 || mbDynamicObjectsInCombinedMesh.Count == 0)
		{
			if (GetVertexCount() > 0 && LOG_LEVEL >= MB2_LogLevel.warn)
			{
				UnityEngine.Debug.LogWarning("Nothing in SkinnedMeshRenderer. CoulddoBlendShapes not UpdateSkinnedMeshApproximateBoundsFromBounds.");
			}
		}
		else if (_targetRenderer == null)
		{
			if (LOG_LEVEL >= MB2_LogLevel.warn)
			{
				UnityEngine.Debug.LogWarning("Target Renderer is not set. No point in calling UpdateSkinnedMeshApproximateBoundsFromBounds.");
			}
		}
		else if (!_targetRenderer.GetType().Equals(typeof(SkinnedMeshRenderer)))
		{
			if (LOG_LEVEL >= MB2_LogLevel.warn)
			{
				UnityEngine.Debug.LogWarning("Target Renderer is not a SkinnedMeshRenderer. No point in calling UpdateSkinnedMeshApproximateBoundsFromBounds.");
			}
		}
		else
		{
			MB3_MeshCombiner.UpdateSkinnedMeshApproximateBoundsFromBoundsStatic(objectsInCombinedMesh, (SkinnedMeshRenderer)targetRenderer);
		}
	}

	private static void _UpdateMaterialsOnTargetRenderer(MB2_TextureBakeResults textureBakeResults, Renderer targetRenderer, SerializableIntArray[] subTris, int numNonZeroLengthSubmeshTris)
	{
		if (subTris.Length != textureBakeResults.NumResultMaterials())
		{
			UnityEngine.Debug.LogError("Mismatch between number of submeshes and number of result materials " + subTris.Length + " " + textureBakeResults.NumResultMaterials());
		}
		Material[] array = new Material[numNonZeroLengthSubmeshTris];
		int num = 0;
		for (int i = 0; i < subTris.Length; i++)
		{
			if (subTris[i].data.Length != 0)
			{
				array[num] = textureBakeResults.GetCombinedMaterialForSubmesh(i);
				num++;
			}
		}
		targetRenderer.materials = array;
	}
}
