using GorillaNetworking;
using GorillaTag;
using GorillaTag.CosmeticSystem;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class FreeHoverboardInstance : MonoBehaviour
{
	public int ownerActorNumber;

	public int boardIndex;

	[SerializeField]
	private Vector3 sphereCastCenter;

	[SerializeField]
	private float sphereCastRadius;

	[SerializeField]
	private LayerMask hoverRaycastMask;

	[SerializeField]
	private float hoverHeight;

	[SerializeField]
	private float hoverCastDistance = 1f;

	[SerializeField]
	private float hoverRotationLerp;

	[SerializeField]
	private float avelocityDragWhileHovering;

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

	private bool hasHoverPoint;

	private Vector3 hoverPoint;

	private Vector3 hoverNormal;

	public Rigidbody Rigidbody { get; private set; }

	private float AdjustedSphereCastRadius => sphereCastRadius * base.transform.lossyScale.y;

	private float AdjustedHoverHeight => hoverHeight * base.transform.lossyScale.y;

	private float AdjustedHoverCastDistance => hoverCastDistance * base.transform.lossyScale.y;

	public Color boardColor { get; private set; }

	private void Awake()
	{
		Rigidbody = GetComponent<Rigidbody>();
		Rigidbody.useGravity = false;
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
				colorMaterial.color = boardColor;
			}
		}
	}

	public void SetColor(Color col)
	{
		if (colorMaterial != null)
		{
			colorMaterial.color = col;
		}
		boardColor = col;
		activeCosmetic?.ApplyColor(col);
	}

	public void ApplySkin(string displayName)
	{
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
			return;
		}
		CosmeticSO cosmeticSO = CosmeticsController.instance?.GetCosmeticSOFromDisplayName(displayName);
		if (!(cosmeticSO == null) && cosmeticSO.info.functionalParts != null && cosmeticSO.info.functionalParts.Length != 0)
		{
			GTAssetRef<GameObject> prefabAssetRef = cosmeticSO.info.functionalParts[0].prefabAssetRef;
			skinLoadHandle = Addressables.LoadAssetAsync<GameObject>(prefabAssetRef);
			skinLoadHandle.Completed += delegate(AsyncOperationHandle<GameObject> handle)
			{
				OnSkinPrefabLoaded(handle, requestedVersion);
			};
		}
	}

	private void OnSkinPrefabLoaded(AsyncOperationHandle<GameObject> handle, int requestedVersion)
	{
		if (requestedVersion != skinLoadVersion || !base.gameObject.activeInHierarchy)
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
		}
	}

	private void ClearSkin()
	{
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

	private void OnDisable()
	{
		skinLoadVersion++;
		ClearSkin();
	}

	private void Update()
	{
		if (Physics.SphereCast(new Ray(base.transform.TransformPoint(sphereCastCenter), base.transform.TransformVector(Vector3.down)), AdjustedSphereCastRadius, out var hitInfo, AdjustedHoverCastDistance, hoverRaycastMask.value))
		{
			hasHoverPoint = true;
			hoverPoint = hitInfo.point;
			hoverNormal = hitInfo.normal;
		}
		else
		{
			hasHoverPoint = false;
		}
	}

	private void FixedUpdate()
	{
		Rigidbody.AddForce(Physics.gravity * base.transform.lossyScale.y, ForceMode.Acceleration);
		if (hasHoverPoint)
		{
			float num = Vector3.Dot(base.transform.TransformPoint(sphereCastCenter) - hoverPoint, hoverNormal);
			if (num < AdjustedHoverHeight)
			{
				Rigidbody.position += hoverNormal * (AdjustedHoverHeight - num);
				Rigidbody.linearVelocity = Vector3.ProjectOnPlane(Rigidbody.linearVelocity, hoverNormal);
				Vector3 vector = Quaternion.Inverse(Rigidbody.rotation) * Rigidbody.angularVelocity;
				vector.x *= avelocityDragWhileHovering;
				vector.z *= avelocityDragWhileHovering;
				Rigidbody.angularVelocity = Rigidbody.rotation * vector;
				Rigidbody.rotation = Quaternion.Lerp(Rigidbody.rotation, Quaternion.LookRotation(Vector3.ProjectOnPlane(Rigidbody.rotation * Vector3.forward, hoverNormal), hoverNormal), hoverRotationLerp);
			}
		}
	}
}
