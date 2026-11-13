using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Cysharp.Text;
using GorillaExtensions;
using GorillaLocomotion;
using GorillaNetworking;
using GorillaNetworking.Store;
using GorillaTag;
using GorillaTag.CosmeticSystem;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CosmeticsV2Spawner_Dirty : IDelayedExecListener
{
	private readonly struct LoadOpInfo
	{
		public readonly CosmeticAttachInfo attachInfo;

		public readonly CosmeticPart part;

		public readonly int partIndex;

		public readonly CosmeticSO cosmeticSO;

		public ref readonly CosmeticInfoV2 cosmeticInfoV2 => ref cosmeticSO.info;

		public LoadOpInfo(CosmeticAttachInfo attachInfo, CosmeticPart part, int partIndex, CosmeticSO cosmeticSO)
		{
			this.attachInfo = attachInfo;
			this.part = part;
			this.partIndex = partIndex;
			this.cosmeticSO = cosmeticSO;
		}
	}

	public struct VRRigData
	{
		public readonly VRRig vrRig;

		public readonly Transform[] boneXforms;

		public readonly BodyDockPositions bdPositionsComp;

		public readonly List<GameObject> vrRig_cosmetics;

		public readonly List<GameObject> vrRig_override;

		public readonly Transform parentOfDeactivatedHoldables;

		public int bdPositions_allObjects_length;

		public readonly List<GameObject> bdPositions_leftHandThrowables;

		public readonly List<GameObject> bdPositions_rightHandThrowables;

		public VRRigData(VRRig vrRig, Transform[] boneXforms)
		{
			this.vrRig = vrRig;
			this.boneXforms = boneXforms;
			if (!vrRig.transform.TryFindByPath("./**/Holdables", out parentOfDeactivatedHoldables))
			{
				UnityEngine.Debug.LogError("Could not find parent for deactivated holdables. Falling back to VRRig transform: \"" + vrRig.transform.GetPath() + "\"");
			}
			bdPositionsComp = vrRig.GetComponentInChildren<BodyDockPositions>(includeInactive: true);
			vrRig_cosmetics = new List<GameObject>(500);
			vrRig_override = new List<GameObject>(500);
			bdPositions_leftHandThrowables = new List<GameObject>(20);
			bdPositions_rightHandThrowables = new List<GameObject>(20);
			bdPositions_allObjects_length = 2000;
		}
	}

	private enum ERecordState
	{
		Loading,
		Loaded,
		Releasing
	}

	private sealed class SpawnedPart
	{
		public LoadOpInfo info;

		public AsyncOperationHandle<GameObject> handle;

		public CosmeticRecord owner;

		public bool failed;

		public GameObject root;

		public GameObject inner;

		public TransferrableObject transferrable;

		public int allObjectsIndex = -1;

		public SnowballThrowable throwable;

		public EHandedness throwableHand;

		public readonly List<CosmeticRefTarget> refTargets = new List<CosmeticRefTarget>(2);

		public bool IsOverride
		{
			get
			{
				ECosmeticPartType partType = info.part.partType;
				return partType == ECosmeticPartType.FirstPerson || partType == ECosmeticPartType.LocalRig;
			}
		}
	}

	private sealed class CosmeticRecord
	{
		public string playFabId;

		public int rigIndex;

		public VRRig rig;

		public CosmeticItemRegistry registry;

		public readonly List<SpawnedPart> parts = new List<SpawnedPart>(4);

		public int pending;

		public ERecordState state;

		public bool pinned;
	}

	private static CosmeticsV2Spawner_Dirty _instance;

	public static Action OnPostInstantiateAllPrefabs;

	[OnEnterPlay_SetNull]
	private static Transform _gDeactivatedSpawnParent;

	[OnEnterPlay_Set(0)]
	private static int _g_loadOpsCountCompleted = 0;

	private const int _k_maxActiveLoadOps = 1000000;

	private const int _k_maxTotalLoadOps = 1000000;

	private const int _k_delayedStatusCheckContextId = -100;

	[OnEnterPlay_SetNull]
	private static Dictionary<string, CosmeticSO> _g_cosmeticSOByID;

	[OnEnterPlay_Clear]
	private static readonly List<LoadOpInfo> _g_loadOpInfosScratch = new List<LoadOpInfo>(8);

	[OnEnterPlay_SetNull]
	private static SnowballMaker _gSnowballMakerLeft;

	[OnEnterPlay_Clear]
	private static readonly List<SnowballThrowable> _gSnowballMakerLeft_throwables = new List<SnowballThrowable>(20);

	[OnEnterPlay_SetNull]
	private static SnowballMaker _gSnowballMakerRight;

	[OnEnterPlay_Clear]
	private static readonly List<SnowballThrowable> _gSnowballMakerRight_throwables = new List<SnowballThrowable>(20);

	[OnEnterPlay_SetNull]
	private static GTPlayer g_gorillaPlayer;

	private static Stopwatch k_stopwatch = new Stopwatch();

	[OnEnterPlay_Clear]
	public static readonly List<VRRigData> _gVRRigDatas = new List<VRRigData>(20);

	private static Dictionary<VRRig, int> _gVRRigDatasIndexByRig = new Dictionary<VRRig, int>();

	[OnEnterPlay_Clear]
	private static Dictionary<int, string> materialIndexToSnowballThrowablePlayfabIdStringLeft;

	[OnEnterPlay_Clear]
	private static Dictionary<int, string> materialIndexToSnowballThrowablePlayfabIdStringRight;

	[OnEnterPlay_Clear]
	private static Dictionary<int, string> throwableIndexPlayfabIdStringRight;

	[OnEnterPlay_Clear]
	private static Dictionary<int, string> throwableIndexPlayfabIdStringLeft;

	[OnEnterPlay_SetNull]
	private static Dictionary<string, CosmeticRecord>[] _g_recordsByRig;

	[OnEnterPlay_Clear]
	private static readonly Dictionary<AsyncOperationHandle<GameObject>, SpawnedPart> _g_partByHandle = new Dictionary<AsyncOperationHandle<GameObject>, SpawnedPart>(512);

	[OnEnterPlay_SetNull]
	private static bool[] _g_releaseWhenLoaded;

	[OnEnterPlay_Clear]
	private static readonly List<CosmeticRecord> _g_releaseScratch = new List<CosmeticRecord>(64);

	[OnEnterPlay_Set(0)]
	private static int _g_releasedSinceAssetGC;

	[field: OnEnterPlay_Set(false)]
	public static bool isPrepared { get; private set; }

	void IDelayedExecListener.OnDelayedAction(int contextId)
	{
		if (contextId == -Mathf.Abs("_Step5_InitializeVRRigsAndCosmeticsControllerFinalize".GetHashCode()))
		{
			_Step5_InitializeVRRigsAndCosmeticsControllerFinalize();
		}
	}

	public static void PrepareLoadOpInfos()
	{
		if (isPrepared || ApplicationQuittingState.IsQuitting)
		{
			return;
		}
		if (_instance == null)
		{
			_instance = new CosmeticsV2Spawner_Dirty();
		}
		k_stopwatch.Restart();
		g_gorillaPlayer = UnityEngine.Object.FindAnyObjectByType<GTPlayer>();
		SnowballMaker[] componentsInChildren = g_gorillaPlayer.GetComponentsInChildren<SnowballMaker>(includeInactive: true);
		foreach (SnowballMaker snowballMaker in componentsInChildren)
		{
			if (snowballMaker.isLeftHand)
			{
				_gSnowballMakerLeft = snowballMaker;
			}
			else
			{
				_gSnowballMakerRight = snowballMaker;
			}
		}
		if (!CosmeticsController.hasInstance)
		{
			UnityEngine.Debug.LogError("(Should never happen) Cannot instantiate prefabs before cosmetics controller instance is available.");
			return;
		}
		if (!CosmeticsController.instance.v2_allCosmeticsInfoAssetRef.IsValid())
		{
			UnityEngine.Debug.LogError("(Should never happen) Cannot load prefabs before v2_allCosmeticsInfoAssetRef is loaded.");
			return;
		}
		if (!(CosmeticsController.instance.v2_allCosmeticsInfoAssetRef.Asset is AllCosmeticsArraySO allCosmeticsArraySO))
		{
			UnityEngine.Debug.LogError("(Should never happen) v2_allCosmeticsInfoAssetRef is valid but null.");
			return;
		}
		if (!GTHardCodedBones.TryGetBoneXforms(VRRig.LocalRig, out var outBoneXforms, out var outErrorMsg))
		{
			UnityEngine.Debug.LogError("CosmeticsV2Spawner_Dirty: Error getting bone Transforms from local VRRig: " + outErrorMsg, VRRig.LocalRig);
			return;
		}
		_gVRRigDatas.Add(new VRRigData(VRRig.LocalRig, outBoneXforms));
		_gVRRigDatasIndexByRig[VRRig.LocalRig] = 0;
		VRRig[] allRigs = VRRigCache.Instance.GetAllRigs();
		foreach (VRRig vRRig in allRigs)
		{
			if (!GTHardCodedBones.TryGetBoneXforms(vRRig, out var outBoneXforms2, out outErrorMsg))
			{
				UnityEngine.Debug.LogError("CosmeticsV2Spawner_Dirty: Error getting bone Transforms from cached VRRig: " + outErrorMsg, VRRig.LocalRig);
				return;
			}
			_gVRRigDatasIndexByRig[vRRig] = _gVRRigDatas.Count;
			_gVRRigDatas.Add(new VRRigData(vRRig, outBoneXforms2));
		}
		_gDeactivatedSpawnParent = GlobalDeactivatedSpawnRoot.GetOrCreate();
		GTDelayedExec.Add(_instance, 2f, -100);
		materialIndexToSnowballThrowablePlayfabIdStringLeft = new Dictionary<int, string>();
		materialIndexToSnowballThrowablePlayfabIdStringRight = new Dictionary<int, string>();
		throwableIndexPlayfabIdStringLeft = new Dictionary<int, string>();
		throwableIndexPlayfabIdStringRight = new Dictionary<int, string>();
		_g_cosmeticSOByID = new Dictionary<string, CosmeticSO>(allCosmeticsArraySO.sturdyAssetRefs.Length);
		_g_recordsByRig = new Dictionary<string, CosmeticRecord>[20];
		_g_releaseWhenLoaded = new bool[20];
		GTDirectAssetRef<CosmeticSO>[] sturdyAssetRefs = allCosmeticsArraySO.sturdyAssetRefs;
		foreach (GTDirectAssetRef<CosmeticSO> gTDirectAssetRef in sturdyAssetRefs)
		{
			CosmeticSO obj = gTDirectAssetRef.obj;
			_g_cosmeticSOByID.TryAdd(obj.info.playFabID, obj);
			_g_loadOpInfosScratch.Clear();
			_GetLoadOpInfos(obj, includeLocalRigParts: true, logMissingParts: true, _g_loadOpInfosScratch);
			if (!obj.info.isThrowable)
			{
				continue;
			}
			foreach (LoadOpInfo item in _g_loadOpInfosScratch)
			{
				_RegisterThrowableIndices(item);
			}
		}
		_g_loadOpInfosScratch.Clear();
		_Step4_PopulateAllArrays();
	}

	private static void _GetLoadOpInfos(CosmeticSO cosmeticSO, bool includeLocalRigParts, bool logMissingParts, List<LoadOpInfo> outLoadOpInfos)
	{
		CosmeticInfoV2 info = cosmeticSO.info;
		if (info.hasHoldableParts)
		{
			_AddPartLoadOpInfos(cosmeticSO, info.holdableParts, logMissingParts ? "wearable" : null, outLoadOpInfos);
		}
		if (info.hasFunctionalParts)
		{
			_AddPartLoadOpInfos(cosmeticSO, info.functionalParts, logMissingParts ? "functional" : null, outLoadOpInfos);
		}
		if (includeLocalRigParts)
		{
			if (info.hasFirstPersonViewParts)
			{
				_AddPartLoadOpInfos(cosmeticSO, info.firstPersonViewParts, logMissingParts ? "first person" : null, outLoadOpInfos);
			}
			if (info.hasLocalRigParts)
			{
				_AddPartLoadOpInfos(cosmeticSO, info.localRigParts, logMissingParts ? "local rig" : null, outLoadOpInfos);
			}
		}
	}

	private static void _AddPartLoadOpInfos(CosmeticSO cosmeticSO, CosmeticPart[] parts, string logMissingPartsKind, List<LoadOpInfo> outLoadOpInfos)
	{
		for (int i = 0; i < parts.Length; i++)
		{
			CosmeticPart part = parts[i];
			if (!part.prefabAssetRef.RuntimeKeyIsValid())
			{
				if (logMissingPartsKind != null)
				{
					GTDev.LogError("Cosmetic " + cosmeticSO.info.displayName + " has missing object reference in " + logMissingPartsKind + " parts, skipping load");
				}
				continue;
			}
			for (int j = 0; j < part.attachAnchors.Length; j++)
			{
				outLoadOpInfos.Add(new LoadOpInfo(part.attachAnchors[j], part, i, cosmeticSO));
				if (part.partType == ECosmeticPartType.Holdable)
				{
					break;
				}
			}
		}
	}

	private static void _RegisterThrowableIndices(LoadOpInfo loadOpInfo)
	{
		ref readonly CosmeticInfoV2 cosmeticInfoV = ref loadOpInfo.cosmeticInfoV2;
		if (GTHardCodedBones.GetHandednessFromBone(loadOpInfo.attachInfo.parentBone) == EHandedness.Right)
		{
			for (int i = 0; i < cosmeticInfoV.throwableMaterialGrabIndices.Length; i++)
			{
				int key = cosmeticInfoV.throwableMaterialGrabIndices[i];
				if (!materialIndexToSnowballThrowablePlayfabIdStringRight.ContainsKey(key))
				{
					materialIndexToSnowballThrowablePlayfabIdStringRight[key] = cosmeticInfoV.playFabID;
				}
			}
			throwableIndexPlayfabIdStringRight.TryAdd(cosmeticInfoV.throwableIndex, cosmeticInfoV.playFabID);
			return;
		}
		for (int j = 0; j < cosmeticInfoV.throwableMaterialGrabIndices.Length; j++)
		{
			int key2 = cosmeticInfoV.throwableMaterialGrabIndices[j];
			if (!materialIndexToSnowballThrowablePlayfabIdStringLeft.ContainsKey(key2))
			{
				materialIndexToSnowballThrowablePlayfabIdStringLeft[key2] = cosmeticInfoV.playFabID;
			}
		}
		throwableIndexPlayfabIdStringLeft.TryAdd(cosmeticInfoV.throwableIndex, cosmeticInfoV.playFabID);
	}

	public static bool GetPlayfabIdFromThrowableIndex(bool isLeft, int throwableIndex, out string playfabId)
	{
		if (throwableIndexPlayfabIdStringLeft == null || throwableIndexPlayfabIdStringRight == null)
		{
			playfabId = "null";
			return false;
		}
		if (isLeft && throwableIndexPlayfabIdStringLeft.TryGetValue(throwableIndex, out playfabId))
		{
			return true;
		}
		if (!isLeft && throwableIndexPlayfabIdStringRight.TryGetValue(throwableIndex, out playfabId))
		{
			return true;
		}
		playfabId = "null";
		return false;
	}

	public static bool GetThrowableIDFromMaterialIndex(bool isLeft, int matIndex, out string throwableId)
	{
		if (materialIndexToSnowballThrowablePlayfabIdStringLeft == null || materialIndexToSnowballThrowablePlayfabIdStringRight == null)
		{
			throwableId = "null";
			return false;
		}
		if (isLeft && materialIndexToSnowballThrowablePlayfabIdStringLeft.TryGetValue(matIndex, out throwableId))
		{
			return true;
		}
		if (!isLeft && materialIndexToSnowballThrowablePlayfabIdStringRight.TryGetValue(matIndex, out throwableId))
		{
			return true;
		}
		throwableId = "null";
		return false;
	}

	public static void ProcessLoadOpInfos(VRRig rig, string playfabId, CosmeticItemRegistry registry)
	{
		if (ApplicationQuittingState.IsQuitting || _g_recordsByRig == null || !_gVRRigDatasIndexByRig.TryGetValue(rig, out var value))
		{
			return;
		}
		if (rig.gameObject.activeInHierarchy)
		{
			_g_releaseWhenLoaded[value] = false;
		}
		Dictionary<string, CosmeticRecord>[] g_recordsByRig = _g_recordsByRig;
		int num = value;
		Dictionary<string, CosmeticRecord> dictionary = g_recordsByRig[num] ?? (g_recordsByRig[num] = new Dictionary<string, CosmeticRecord>());
		if (dictionary.ContainsKey(playfabId))
		{
			return;
		}
		CosmeticRecord cosmeticRecord = new CosmeticRecord
		{
			playFabId = playfabId,
			rigIndex = value,
			rig = rig,
			registry = registry,
			state = ERecordState.Loading,
			pinned = (playfabId == "Slingshot")
		};
		dictionary.Add(playfabId, cosmeticRecord);
		if (!_g_cosmeticSOByID.TryGetValue(playfabId, out var value2))
		{
			cosmeticRecord.state = ERecordState.Loaded;
			return;
		}
		_g_loadOpInfosScratch.Clear();
		_GetLoadOpInfos(value2, value == 0, logMissingParts: false, _g_loadOpInfosScratch);
		foreach (LoadOpInfo item in _g_loadOpInfosScratch)
		{
			cosmeticRecord.parts.Add(new SpawnedPart
			{
				info = item,
				owner = cosmeticRecord
			});
		}
		_g_loadOpInfosScratch.Clear();
		if (cosmeticRecord.parts.Count == 0)
		{
			cosmeticRecord.state = ERecordState.Loaded;
			return;
		}
		cosmeticRecord.pending = cosmeticRecord.parts.Count;
		for (int i = 0; i < cosmeticRecord.parts.Count; i++)
		{
			SpawnedPart spawnedPart = cosmeticRecord.parts[i];
			try
			{
				spawnedPart.handle = spawnedPart.info.part.prefabAssetRef.InstantiateAsync(_gDeactivatedSpawnParent);
			}
			catch (InvalidKeyException ex)
			{
				UnityEngine.Debug.LogError("CosmeticsV2Spawner_Dirty: Missing Addressable for " + $"\"{spawnedPart.info.cosmeticInfoV2.displayName}\" part index {spawnedPart.info.partIndex}. Skipping. {ex.Message}");
				spawnedPart.failed = true;
				cosmeticRecord.pending--;
				continue;
			}
			catch (ArgumentException ex2)
			{
				UnityEngine.Debug.LogError("CosmeticsV2Spawner_Dirty: Invalid Addressable key/config for " + $"\"{spawnedPart.info.cosmeticInfoV2.displayName}\" part index {spawnedPart.info.partIndex}. Skipping. {ex2.Message}");
				spawnedPart.failed = true;
				cosmeticRecord.pending--;
				continue;
			}
			_g_partByHandle[spawnedPart.handle] = spawnedPart;
			spawnedPart.handle.Completed += _Step3_HandleLoadOpCompleted;
		}
		if (cosmeticRecord.pending == 0)
		{
			_FinalizeRecord(cosmeticRecord);
		}
	}

	private static void _Step3_HandleLoadOpCompleted(AsyncOperationHandle<GameObject> loadOp)
	{
		if (ApplicationQuittingState.IsQuitting || !_g_partByHandle.TryGetValue(loadOp, out var value))
		{
			return;
		}
		_g_loadOpsCountCompleted++;
		if (loadOp.Status != AsyncOperationStatus.Succeeded)
		{
			UnityEngine.Debug.LogWarning("CosmeticsV2Spawner_Dirty: Failed to load part " + $"\"{value.info.cosmeticInfoV2.displayName}\" (key: {value.info.part.prefabAssetRef.RuntimeKey}). Skipping.");
			value.failed = true;
			_g_partByHandle.Remove(loadOp);
			if (loadOp.IsValid())
			{
				Addressables.Release(loadOp);
			}
		}
		else
		{
			try
			{
				_AttachPart(value, loadOp.Result);
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}
		if (--value.owner.pending == 0)
		{
			_FinalizeRecord(value.owner);
		}
	}

	private static void _AttachPart(SpawnedPart part, GameObject resultGObj)
	{
		LoadOpInfo info = part.info;
		ECosmeticSelectSide eCosmeticSelectSide = info.attachInfo.selectSide;
		string name = info.cosmeticInfoV2.playFabID;
		if (eCosmeticSelectSide != 0)
		{
			string playFabID = info.cosmeticInfoV2.playFabID;
			name = ZString.Concat(playFabID, eCosmeticSelectSide switch
			{
				ECosmeticSelectSide.Left => " LEFT.", 
				ECosmeticSelectSide.Right => " RIGHT.", 
				_ => "", 
			});
		}
		part.root = resultGObj;
		resultGObj.SetActive(value: false);
		Transform transform = resultGObj.transform;
		Transform transform2 = transform;
		CosmeticPart[] holdableParts = info.cosmeticInfoV2.holdableParts;
		if (holdableParts != null && holdableParts.Length > 0)
		{
			TransferrableObject componentInChildren = resultGObj.GetComponentInChildren<TransferrableObject>(includeInactive: true);
			if ((bool)componentInChildren && componentInChildren.gameObject != resultGObj)
			{
				transform2 = componentInChildren.transform;
				transform2.gameObject.SetActive(value: false);
				resultGObj.SetActive(value: true);
			}
		}
		if (info.cosmeticInfoV2.isThrowable)
		{
			SnowballThrowable componentInChildren2 = resultGObj.GetComponentInChildren<SnowballThrowable>(includeInactive: true);
			if ((bool)componentInChildren2 && componentInChildren2.gameObject != resultGObj)
			{
				transform2 = componentInChildren2.transform;
				transform2.gameObject.SetActive(value: false);
				resultGObj.SetActive(value: true);
			}
		}
		transform2.name = name;
		part.inner = transform2.gameObject;
		VRRigData value = _gVRRigDatas[part.owner.rigIndex];
		if (info.cosmeticInfoV2.category != CosmeticsController.CosmeticCategory.Collectable)
		{
			Transform transform3 = info.part.partType switch
			{
				ECosmeticPartType.Holdable => ((GTHardCodedBones.EBone)info.attachInfo.parentBone != GTHardCodedBones.EBone.body_AnchorFront_StowSlot) ? value.parentOfDeactivatedHoldables : value.boneXforms[(int)info.attachInfo.parentBone], 
				ECosmeticPartType.Functional => value.boneXforms[(int)info.attachInfo.parentBone], 
				ECosmeticPartType.FirstPerson => g_gorillaPlayer.CosmeticsHeadTarget, 
				ECosmeticPartType.LocalRig => value.boneXforms[(int)info.attachInfo.parentBone], 
				_ => throw new ArgumentOutOfRangeException("partType", "unhandled part type."), 
			};
			if ((bool)transform3)
			{
				transform.SetParent(transform3, worldPositionStays: false);
				transform.localPosition = info.attachInfo.offset.pos;
				transform.localRotation = info.attachInfo.offset.rot;
				transform.localScale = info.attachInfo.offset.scale;
			}
			else
			{
				UnityEngine.Debug.LogError($"Bone transform not found for cosmetic part type {info.part.partType}. Cosmetic: " + "\"" + info.cosmeticInfoV2.displayName + "\"," + $"part: \"{info.part.prefabAssetRef.RuntimeKey}\"");
			}
		}
		switch (info.part.partType)
		{
		case ECosmeticPartType.Holdable:
		{
			value.vrRig_cosmetics.Add(transform2.gameObject);
			HoldableObject componentInChildren3 = resultGObj.GetComponentInChildren<HoldableObject>(includeInactive: true);
			if (!(componentInChildren3 is SnowballThrowable throwable))
			{
				if (!(componentInChildren3 is TransferrableObject transferrableObject))
				{
					if ((object)componentInChildren3 != null)
					{
						throw new Exception("Encountered unexpected HoldableObject derived type on cosmetic part: \"" + info.cosmeticInfoV2.displayName + "\"");
					}
					break;
				}
				part.transferrable = transferrableObject;
				string playFabID2 = info.cosmeticInfoV2.playFabID;
				if (CosmeticsLegacyV1Info.TryGetBodyDockAllObjectsIndexes(playFabID2, out var bdAllIndexes))
				{
					if (info.partIndex < bdAllIndexes.Length && info.partIndex >= 0)
					{
						transferrableObject.myIndex = bdAllIndexes[info.partIndex];
					}
				}
				else if (playFabID2.Length >= 5 && playFabID2[0] == 'L')
				{
					if (playFabID2[1] != 'M')
					{
						throw new Exception("(this should never happen) A TransferrableObject cosmetic added sometime after 2024-06 does not use the expected PlayFabID format where the string starts with \"LM\" and ends with \".\". Path: " + transform2.GetPathQ());
					}
					string text = playFabID2;
					playFabID2 = ((text[text.Length - 1] == '.') ? playFabID2 : (playFabID2 + "."));
					int num = 224;
					transferrableObject.myIndex = num + CosmeticIDUtils.PlayFabIdToIndexInCategory(playFabID2);
				}
				else
				{
					transferrableObject.myIndex = -2;
					if (!(playFabID2 == "STICKABLE TARGET"))
					{
						UnityEngine.Debug.LogError("Cosmetic \"" + info.cosmeticInfoV2.displayName + "\" cannot derive `TransferrableObject.myIndex` from playFabId \"" + playFabID2 + "\" and so will not be included in `BodyDockPositions.allObjects` array.");
					}
				}
				if (transferrableObject is ProjectileWeapon projectileWeapon && info.cosmeticInfoV2.playFabID == "Slingshot")
				{
					value.vrRig.projectileWeapon = projectileWeapon;
				}
				if (transferrableObject.myIndex > 0 && transferrableObject.myIndex < value.bdPositions_allObjects_length)
				{
					value.bdPositionsComp._allObjects[transferrableObject.myIndex] = transferrableObject;
					part.allObjectsIndex = transferrableObject.myIndex;
					if (!value.vrRig.isOfflineVRRig)
					{
						value.bdPositionsComp.RefreshTransferrableItems();
					}
				}
			}
			else
			{
				part.throwable = throwable;
				part.throwableHand = AddPartToThrowableLists(info, part.owner.rigIndex, throwable);
			}
			break;
		}
		case ECosmeticPartType.Functional:
			value.vrRig_cosmetics.Add(transform2.gameObject);
			break;
		case ECosmeticPartType.FirstPerson:
		case ECosmeticPartType.LocalRig:
			value.vrRig_override.Add(transform2.gameObject);
			break;
		default:
			throw new ArgumentOutOfRangeException("Unexpected ECosmeticPartType value encountered: " + $"{info.part.partType}, " + $"int: {(int)info.part.partType}.");
		}
		_gVRRigDatas[part.owner.rigIndex] = value;
		CosmeticRefRegistry cosmeticReferences = value.vrRig.cosmeticReferences;
		CosmeticRefTarget[] componentsInChildren = resultGObj.GetComponentsInChildren<CosmeticRefTarget>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			cosmeticReferences.Register(componentsInChildren[i].id, componentsInChildren[i].gameObject);
			part.refTargets.Add(componentsInChildren[i]);
		}
	}

	private static void _FinalizeRecord(CosmeticRecord record)
	{
		if (record.state != 0)
		{
			return;
		}
		record.state = ERecordState.Loaded;
		CosmeticItemRegistry registry = record.registry;
		for (int i = 0; i < 2; i++)
		{
			bool flag = i == 0;
			foreach (SpawnedPart part in record.parts)
			{
				if (!(part.inner == null) && part.IsOverride == flag)
				{
					try
					{
						registry.InitializeCosmetic(part.inner, flag);
					}
					catch (Exception exception)
					{
						UnityEngine.Debug.LogError("CosmeticsV2Spawner_Dirty._FinalizeRecord: InitializeCosmetic" + (flag ? " (override)" : "") + " threw for \"" + part.inner.name + "\" on rig \"" + ((registry.Rig != null) ? registry.Rig.name : "<null rig>") + "\". Skipping this part so the rest still initialize. Exception follows:", part.inner);
						UnityEngine.Debug.LogException(exception, part.inner);
					}
				}
			}
		}
		foreach (SpawnedPart part2 in record.parts)
		{
			if (part2.inner == null)
			{
				continue;
			}
			ISpawnable[] componentsInChildren = part2.inner.GetComponentsInChildren<ISpawnable>(includeInactive: true);
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				if (!componentsInChildren[j].IsSpawned)
				{
					try
					{
						componentsInChildren[j].IsSpawned = true;
						componentsInChildren[j].CosmeticSelectedSide = part2.info.attachInfo.selectSide;
						componentsInChildren[j].OnSpawn(registry.Rig);
					}
					catch (Exception exception2)
					{
						UnityEngine.Debug.LogException(exception2);
					}
				}
			}
		}
		if (_g_releaseWhenLoaded[record.rigIndex] && !record.rig.gameObject.activeInHierarchy)
		{
			_Release(record);
			return;
		}
		registry.RefreshRig();
		if (registry.Rig != null && registry.Rig.myBodyDockPositions != null)
		{
			registry.Rig.myBodyDockPositions.RefreshTransferrableItems();
		}
	}

	public static void OnLocalPlayerLeftRoom()
	{
		if (_g_recordsByRig == null)
		{
			return;
		}
		_g_releaseScratch.Clear();
		for (int i = 1; i < _g_recordsByRig.Length; i++)
		{
			_g_releaseWhenLoaded[i] = true;
			Dictionary<string, CosmeticRecord> dictionary = _g_recordsByRig[i];
			if (dictionary == null)
			{
				continue;
			}
			foreach (CosmeticRecord value in dictionary.Values)
			{
				if (value.state == ERecordState.Loaded && !value.pinned)
				{
					_g_releaseScratch.Add(value);
				}
			}
		}
		for (int j = 0; j < _g_releaseScratch.Count; j++)
		{
			try
			{
				_Release(_g_releaseScratch[j]);
			}
			catch (Exception exception)
			{
				UnityEngine.Debug.LogException(exception);
			}
		}
		_g_releaseScratch.Clear();
	}

	private static void _Release(CosmeticRecord record)
	{
		if (record.state != ERecordState.Loaded || record.pinned)
		{
			return;
		}
		record.state = ERecordState.Releasing;
		VRRigData vrRigData = _gVRRigDatas[record.rigIndex];
		foreach (SpawnedPart part in record.parts)
		{
			if (part.inner == null)
			{
				continue;
			}
			ISpawnable[] componentsInChildren = part.inner.GetComponentsInChildren<ISpawnable>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (componentsInChildren[i].IsSpawned)
				{
					try
					{
						componentsInChildren[i].IsSpawned = false;
						componentsInChildren[i].OnDespawn();
					}
					catch (Exception exception)
					{
						UnityEngine.Debug.LogException(exception);
					}
				}
			}
		}
		record.registry.ReleaseCosmetic(record.playFabId, record.parts.ConvertAll((SpawnedPart p) => p.inner));
		foreach (SpawnedPart part2 in record.parts)
		{
			if (part2.inner != null)
			{
				vrRigData.vrRig_cosmetics.Remove(part2.inner);
				vrRigData.vrRig_override.Remove(part2.inner);
			}
			if (part2.allObjectsIndex >= 0 && part2.allObjectsIndex < vrRigData.bdPositionsComp._allObjects.Length && vrRigData.bdPositionsComp._allObjects[part2.allObjectsIndex] == part2.transferrable)
			{
				vrRigData.bdPositionsComp._allObjects[part2.allObjectsIndex] = null;
			}
			if (part2.throwable != null)
			{
				RemovePartFromThrowableLists(vrRigData, part2.throwable, part2.throwableHand);
			}
			if (part2.transferrable is ProjectileWeapon projectileWeapon && vrRigData.vrRig.projectileWeapon == projectileWeapon)
			{
				vrRigData.vrRig.projectileWeapon = _FindRigSlingshot(record.rigIndex);
			}
			foreach (CosmeticRefTarget refTarget in part2.refTargets)
			{
				if (refTarget != null)
				{
					vrRigData.vrRig.cosmeticReferences.Unregister(refTarget.id, refTarget.gameObject);
				}
			}
			if (part2.inner != null && part2.root != null && !part2.inner.transform.IsChildOf(part2.root.transform))
			{
				part2.inner.transform.SetParent(part2.root.transform, worldPositionStays: false);
			}
			if (!part2.failed && part2.handle.IsValid())
			{
				_g_partByHandle.Remove(part2.handle);
				Addressables.ReleaseInstance(part2.handle);
			}
		}
		_g_recordsByRig[record.rigIndex].Remove(record.playFabId);
		_g_releasedSinceAssetGC++;
	}

	private static ProjectileWeapon _FindRigSlingshot(int rigIndex)
	{
		if (_g_recordsByRig[rigIndex] == null || !_g_recordsByRig[rigIndex].TryGetValue("Slingshot", out var value))
		{
			return null;
		}
		foreach (SpawnedPart part in value.parts)
		{
			if (part.transferrable is ProjectileWeapon projectileWeapon && projectileWeapon != null)
			{
				return projectileWeapon;
			}
		}
		return null;
	}

	public static AsyncOperation UnloadReleasedCosmeticAssets()
	{
		if (_g_releasedSinceAssetGC == 0)
		{
			return null;
		}
		_g_releasedSinceAssetGC = 0;
		return Resources.UnloadUnusedAssets();
	}

	public static string DebugDumpLoadedCosmetics()
	{
		if (_g_recordsByRig == null)
		{
			return "Cosmetics not prepared yet.";
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < _g_recordsByRig.Length; i++)
		{
			Dictionary<string, CosmeticRecord> dictionary = _g_recordsByRig[i];
			if (dictionary == null || dictionary.Count == 0)
			{
				continue;
			}
			string text = ((i < _gVRRigDatas.Count) ? _gVRRigDatas[i].vrRig.name : "?");
			stringBuilder.Append(string.Format("Rig {0} \"{1}\"{2}: {3} cosmetics\n", i, text, _g_releaseWhenLoaded[i] ? " (releasing)" : "", dictionary.Count));
			foreach (CosmeticRecord value in dictionary.Values)
			{
				stringBuilder.Append(string.Format("    {0} {1}{2}, {3} parts\n", value.playFabId, value.state, value.pinned ? " pinned" : "", value.parts.Count));
			}
		}
		stringBuilder.Append($"Releases since last asset GC: {_g_releasedSinceAssetGC}");
		return stringBuilder.ToString();
	}

	private static EHandedness AddPartToThrowableLists(LoadOpInfo loadOpInfo, int rigIndex, SnowballThrowable throwable)
	{
		VRRigData vRRigData = _gVRRigDatas[rigIndex];
		EHandedness handednessFromBone = GTHardCodedBones.GetHandednessFromBone(loadOpInfo.attachInfo.parentBone);
		bool flag = vRRigData.vrRig == _gVRRigDatas[0].vrRig;
		throwable.SpawnOffset = loadOpInfo.attachInfo.offset;
		switch (handednessFromBone)
		{
		case EHandedness.Left:
			ResizeAndSetAtIndex(vRRigData.bdPositions_leftHandThrowables, throwable.gameObject, throwable.throwableMakerIndex);
			if (flag)
			{
				ResizeAndSetAtIndex(_gSnowballMakerLeft_throwables, throwable, throwable.throwableMakerIndex);
			}
			vRRigData.bdPositionsComp.leftHandThrowables = vRRigData.bdPositions_leftHandThrowables.ToArray();
			_gSnowballMakerLeft.SetupThrowables(_gSnowballMakerLeft_throwables.ToArray());
			break;
		case EHandedness.Right:
			ResizeAndSetAtIndex(vRRigData.bdPositions_rightHandThrowables, throwable.gameObject, throwable.throwableMakerIndex);
			if (flag)
			{
				ResizeAndSetAtIndex(_gSnowballMakerRight_throwables, throwable, throwable.throwableMakerIndex);
			}
			vRRigData.bdPositionsComp.rightHandThrowables = vRRigData.bdPositions_rightHandThrowables.ToArray();
			_gSnowballMakerRight.SetupThrowables(_gSnowballMakerRight_throwables.ToArray());
			break;
		case EHandedness.None:
			throw new ArgumentException("Encountered throwable cosmetic \"" + loadOpInfo.cosmeticInfoV2.displayName + "\" where handedness " + $"could not be determined from bone `{loadOpInfo.attachInfo.parentBone}`. " + "Path: \"" + throwable.transform.GetPath() + "\"");
		default:
			throw new ArgumentOutOfRangeException("Unexpected ECosmeticSelectSide value encountered: " + $"{handednessFromBone}, " + $"int: {(int)handednessFromBone}.");
		}
		return handednessFromBone;
	}

	private static void RemovePartFromThrowableLists(VRRigData vrRigData, SnowballThrowable throwable, EHandedness handedness)
	{
		List<GameObject> list = ((handedness == EHandedness.Left) ? vrRigData.bdPositions_leftHandThrowables : vrRigData.bdPositions_rightHandThrowables);
		int throwableMakerIndex = throwable.throwableMakerIndex;
		if (throwableMakerIndex >= 0 && throwableMakerIndex < list.Count && !(list[throwableMakerIndex] != throwable.gameObject))
		{
			list[throwableMakerIndex] = null;
			if (handedness == EHandedness.Left)
			{
				vrRigData.bdPositionsComp.leftHandThrowables = list.ToArray();
			}
			else
			{
				vrRigData.bdPositionsComp.rightHandThrowables = list.ToArray();
			}
		}
	}

	private static void ResizeAndSetAtIndex<T>(List<T> list, T item, int index)
	{
		if (index >= list.Count)
		{
			int num = index - list.Count + 1;
			for (int i = 0; i < num; i++)
			{
				list.Add(default(T));
			}
		}
		list[index] = item;
	}

	private static void _Step4_PopulateAllArrays()
	{
		foreach (VRRigData gVRRigData in _gVRRigDatas)
		{
			gVRRigData.bdPositionsComp._allObjects = new TransferrableObject[2000];
		}
		GTDelayedExec.Add(_instance, 1f, -Mathf.Abs("_Step5_InitializeVRRigsAndCosmeticsControllerFinalize".GetHashCode()));
	}

	private static void _Step5_InitializeVRRigsAndCosmeticsControllerFinalize()
	{
		CosmeticsController.instance.UpdateWardrobeModelsAndButtons();
		try
		{
			OnPostInstantiateAllPrefabs?.Invoke();
		}
		catch (Exception exception)
		{
			UnityEngine.Debug.LogException(exception);
		}
		try
		{
			CosmeticsController.instance.InitializeCosmeticStands();
		}
		catch (Exception exception2)
		{
			UnityEngine.Debug.LogException(exception2);
		}
		try
		{
			CosmeticsController.instance.UpdateWornCosmetics();
			StartupRerun();
		}
		catch (Exception exception3)
		{
			UnityEngine.Debug.LogException(exception3);
		}
		foreach (VRRigData gVRRigData in _gVRRigDatas)
		{
			try
			{
				if (gVRRigData.bdPositionsComp.isActiveAndEnabled)
				{
					gVRRigData.bdPositionsComp.RefreshTransferrableItems();
				}
			}
			catch (Exception exception4)
			{
				UnityEngine.Debug.LogException(exception4, gVRRigData.vrRig);
			}
		}
		try
		{
			StoreController.instance.InitalizeCosmeticStands();
		}
		catch (Exception exception5)
		{
			UnityEngine.Debug.LogException(exception5);
		}
		isPrepared = true;
		k_stopwatch.Stop();
		UnityEngine.Debug.Log("_Step5_InitializeVRRigsAndCosmeticsControllerFinalize" + $": Done preparing cosmetics system in {(double)k_stopwatch.ElapsedMilliseconds / 1000.0:0.0000} seconds.");
		static async void StartupRerun()
		{
			await Awaitable.WaitForSecondsAsync(2f);
			CosmeticsController.instance.UpdateWornCosmetics();
		}
	}

	public static VRRigData RigDataForRig(VRRig rig)
	{
		return _gVRRigDatas[_gVRRigDatasIndexByRig[rig]];
	}
}
