using GorillaExtensions;
using GorillaLocomotion;
using GorillaNetworking;
using GorillaTag;
using GorillaTag.CosmeticSystem;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;

public class HoverboardVisual : MonoBehaviour, ICallBack
{
	[SerializeField]
	private VRRig parentRig;

	[SerializeField]
	private GorillaVelocityEstimator velocityEstimator;

	[SerializeField]
	[FormerlySerializedAs("audio")]
	private HoverboardAudio hoverboardAudio;

	[SerializeField]
	private HoverboardHandle handlePosition;

	[SerializeField]
	private float grindHapticStrength;

	[SerializeField]
	private float grindHapticDuration;

	[SerializeField]
	private float carveHapticStrength;

	[SerializeField]
	private float carveHapticDuration;

	[SerializeField]
	private InteractionPoint handleInteractionPoint;

	[SerializeField]
	private TextMeshPro racePositionReadout;

	[SerializeField]
	private TextMeshPro raceLapsReadout;

	[SerializeField]
	private Transform visualSlot;

	[SerializeField]
	private GameObject defaultVisualPrefab;

	private Material colorMaterial;

	private GameObject defaultVisualInstance;

	private GameObject activeSkinInstance;

	private HoverboardCosmetic activeCosmetic;

	private AsyncOperationHandle<GameObject> skinLoadHandle;

	private int skinLoadVersion;

	private string currentSkinDisplayName;

	private Vector3 interpolatedLocalPosition;

	private Quaternion interpolatedLocalRotation;

	[SerializeField]
	private float lerpIntoHandDuration;

	private float positionLerpSpeed;

	private float rotationLerpSpeed;

	private bool isCallbackActive;

	public Color boardColor { get; private set; }

	public bool IsHeld { get; private set; }

	public bool IsLeftHanded { get; private set; }

	public Vector3 NominalLocalPosition { get; private set; }

	public Quaternion NominalLocalRotation { get; private set; }

	private Transform NominalParentTransform
	{
		get
		{
			if (!IsHeld)
			{
				return base.transform.parent;
			}
			return (IsLeftHanded ? parentRig.leftHand : parentRig.rightHand).rigTarget.transform;
		}
	}

	private void Awake()
	{
		SpawnDefaultVisual();
	}

	private void SpawnDefaultVisual()
	{
		if (!(defaultVisualInstance != null) && !(defaultVisualPrefab == null))
		{
			defaultVisualInstance = Object.Instantiate(defaultVisualPrefab, visualSlot);
			defaultVisualInstance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
			MeshRenderer componentInChildren = defaultVisualInstance.GetComponentInChildren<MeshRenderer>();
			if (componentInChildren != null)
			{
				Material[] sharedMaterials = componentInChildren.sharedMaterials;
				colorMaterial = new Material(sharedMaterials[1]);
				sharedMaterials[1] = colorMaterial;
				componentInChildren.sharedMaterials = sharedMaterials;
			}
		}
	}

	public void SetIsHeld(bool isHeldLeftHanded, Vector3 localPosition, Quaternion localRotation, Color boardColor)
	{
		if (!isCallbackActive)
		{
			parentRig.AddLateUpdateCallback(this);
			isCallbackActive = true;
		}
		IsHeld = true;
		base.gameObject.SetActive(value: true);
		IsLeftHanded = isHeldLeftHanded;
		NominalLocalPosition = localPosition;
		NominalLocalRotation = localRotation;
		Transform nominalParentTransform = NominalParentTransform;
		interpolatedLocalPosition = nominalParentTransform.InverseTransformPoint(base.transform.position);
		interpolatedLocalRotation = nominalParentTransform.InverseTransformRotation(base.transform.rotation);
		positionLerpSpeed = (interpolatedLocalPosition - NominalLocalPosition).magnitude / lerpIntoHandDuration;
		(Quaternion.Inverse(interpolatedLocalRotation) * NominalLocalRotation).ToAngleAxis(out var angle, out var _);
		rotationLerpSpeed = angle / lerpIntoHandDuration;
		if (parentRig.isLocal)
		{
			GTPlayer.Instance.SetHoverActive(enable: true);
		}
		if (colorMaterial != null)
		{
			colorMaterial.color = boardColor;
		}
		this.boardColor = boardColor;
		if (parentRig.isLocal)
		{
			string displayName = ((CosmeticsController.instance != null) ? CosmeticsController.instance.currentWornSet.items[32].displayName : null);
			ApplySkin(displayName);
		}
	}

	public void SetNotHeld(bool isLeftHanded)
	{
		IsLeftHanded = isLeftHanded;
		SetNotHeld();
	}

	public void SetNotHeld()
	{
		bool isHeld = IsHeld;
		ClearSkin();
		base.gameObject.SetActive(value: false);
		IsHeld = false;
		interpolatedLocalPosition = base.transform.localPosition;
		interpolatedLocalRotation = base.transform.localRotation;
		positionLerpSpeed = (interpolatedLocalPosition - NominalLocalPosition).magnitude / lerpIntoHandDuration;
		(Quaternion.Inverse(interpolatedLocalRotation) * NominalLocalRotation).ToAngleAxis(out var angle, out var _);
		rotationLerpSpeed = angle / lerpIntoHandDuration;
		if (!isHeld)
		{
			base.transform.position = base.transform.parent.TransformPoint(NominalLocalPosition);
			base.transform.rotation = base.transform.parent.TransformRotation(NominalLocalRotation);
		}
		if (parentRig.isLocal)
		{
			GTPlayer.Instance.SetHoverActive(enable: false);
		}
		hoverboardAudio.Stop();
	}

	void ICallBack.CallBack()
	{
		Transform nominalParentTransform = NominalParentTransform;
		if ((interpolatedLocalPosition - NominalLocalPosition).IsShorterThan(0.01f))
		{
			base.transform.position = nominalParentTransform.TransformPoint(NominalLocalPosition);
			base.transform.rotation = nominalParentTransform.TransformRotation(NominalLocalRotation);
			if (!IsHeld)
			{
				parentRig.RemoveLateUpdateCallback(this);
				isCallbackActive = false;
			}
		}
		else
		{
			interpolatedLocalPosition = Vector3.MoveTowards(interpolatedLocalPosition, NominalLocalPosition, positionLerpSpeed * Time.deltaTime);
			interpolatedLocalRotation = Quaternion.RotateTowards(interpolatedLocalRotation, NominalLocalRotation, rotationLerpSpeed * Time.deltaTime);
			base.transform.position = nominalParentTransform.TransformPoint(interpolatedLocalPosition);
			base.transform.rotation = nominalParentTransform.TransformRotation(interpolatedLocalRotation);
		}
		if (IsHeld)
		{
			if (parentRig.isLocal)
			{
				GTPlayer.Instance.SetHoverboardPosRot(base.transform.position, base.transform.rotation);
			}
			else
			{
				hoverboardAudio.UpdateAudioLoop(parentRig.LatestVelocity().magnitude, 0f, 0f, 0f);
			}
		}
	}

	public void PlayGrindHaptic()
	{
		if (IsHeld)
		{
			GorillaTagger.Instance.StartVibration(IsLeftHanded, grindHapticStrength, grindHapticDuration);
		}
	}

	public void PlayCarveHaptic(float carveForce)
	{
		if (IsHeld)
		{
			GorillaTagger.Instance.StartVibration(IsLeftHanded, carveForce * carveHapticStrength, carveHapticDuration);
		}
	}

	public void ProxyGrabHandle(bool isLeftHand)
	{
		EquipmentInteractor.instance.UpdateHandEquipment(handlePosition, isLeftHand);
	}

	private void OnDisable()
	{
		skinLoadVersion++;
		ClearSkin();
	}

	public void ApplySkin(string displayName)
	{
		if (displayName == currentSkinDisplayName && activeSkinInstance != null)
		{
			return;
		}
		skinLoadVersion++;
		int requestedVersion = skinLoadVersion;
		if (skinLoadHandle.IsValid())
		{
			Addressables.Release(skinLoadHandle);
			skinLoadHandle = default(AsyncOperationHandle<GameObject>);
		}
		if (activeSkinInstance != null)
		{
			Object.Destroy(activeSkinInstance);
			activeSkinInstance = null;
			activeCosmetic = null;
		}
		defaultVisualInstance?.SetActive(value: true);
		if (colorMaterial != null)
		{
			colorMaterial.color = boardColor;
		}
		if (string.IsNullOrEmpty(displayName) || displayName == "NOTHING")
		{
			currentSkinDisplayName = null;
			return;
		}
		CosmeticSO cosmeticSO = CosmeticsController.instance?.GetCosmeticSOFromDisplayName(displayName);
		if (!(cosmeticSO == null) && cosmeticSO.info.functionalParts != null && cosmeticSO.info.functionalParts.Length != 0)
		{
			GTAssetRef<GameObject> prefabAssetRef = cosmeticSO.info.functionalParts[0].prefabAssetRef;
			skinLoadHandle = Addressables.LoadAssetAsync<GameObject>(prefabAssetRef);
			skinLoadHandle.Completed += delegate(AsyncOperationHandle<GameObject> handle)
			{
				OnSkinPrefabLoaded(handle, requestedVersion, displayName);
			};
		}
	}

	private void OnSkinPrefabLoaded(AsyncOperationHandle<GameObject> handle, int requestedVersion, string displayName)
	{
		if (requestedVersion != skinLoadVersion || !base.gameObject.activeSelf)
		{
			if (handle.IsValid())
			{
				Addressables.Release(handle);
			}
		}
		else if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
		{
			if (handle.IsValid())
			{
				Addressables.Release(handle);
			}
			skinLoadHandle = default(AsyncOperationHandle<GameObject>);
		}
		else
		{
			defaultVisualInstance?.SetActive(value: false);
			activeSkinInstance = Object.Instantiate(handle.Result, visualSlot);
			activeSkinInstance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
			activeCosmetic = activeSkinInstance.GetComponent<HoverboardCosmetic>();
			activeCosmetic?.Apply(boardColor);
			currentSkinDisplayName = displayName;
		}
	}

	private void ClearSkin()
	{
		currentSkinDisplayName = null;
		if (skinLoadHandle.IsValid())
		{
			Addressables.Release(skinLoadHandle);
			skinLoadHandle = default(AsyncOperationHandle<GameObject>);
		}
		if (activeSkinInstance != null)
		{
			Object.Destroy(activeSkinInstance);
			activeSkinInstance = null;
			activeCosmetic = null;
		}
		defaultVisualInstance?.SetActive(value: true);
	}

	public void DropFreeBoard()
	{
		FreeHoverboardManager.instance.SendDropBoardRPC(base.transform.position, base.transform.rotation, velocityEstimator.linearVelocity, velocityEstimator.angularVelocity, boardColor, parentRig.transform.lossyScale);
	}

	public void SetRaceDisplay(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			racePositionReadout.gameObject.SetActive(value: false);
			return;
		}
		racePositionReadout.gameObject.SetActive(value: true);
		racePositionReadout.text = text;
	}

	public void SetRaceLapsDisplay(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			raceLapsReadout.gameObject.SetActive(value: false);
			return;
		}
		raceLapsReadout.gameObject.SetActive(value: true);
		raceLapsReadout.text = text;
	}
}
