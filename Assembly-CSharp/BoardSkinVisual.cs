using GorillaExtensions;
using GorillaNetworking;
using GorillaTag;
using GorillaTag.CosmeticSystem;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class BoardSkinVisual
{
	private struct TransferableSkin
	{
		public string displayName;

		public GameObject instance;

		public HoverboardCosmetic cosmetic;

		public AsyncOperationHandle<GameObject> handle;

		public bool IsValid
		{
			get
			{
				if (!(instance != null))
				{
					return HasLoadedPrefab;
				}
				return true;
			}
		}

		public bool HasLoadedPrefab
		{
			get
			{
				if (handle.IsValid() && handle.Status == AsyncOperationStatus.Succeeded)
				{
					return handle.Result != null;
				}
				return false;
			}
		}

		public void Destroy()
		{
			if (instance != null)
			{
				Object.Destroy(instance);
			}
			if (handle.IsValid())
			{
				Addressables.Release(handle);
			}
			this = default(TransferableSkin);
		}
	}

	private readonly GameObject owner;

	private readonly Transform visualSlot;

	private readonly GameObject defaultVisualInstance;

	private readonly Material colorMaterial;

	private GameObject activeSkinInstance;

	private HoverboardCosmetic activeCosmetic;

	private AsyncOperationHandle<GameObject> skinLoadHandle;

	private int skinLoadVersion;

	private string requestedSkinDisplayName;

	private TransferableSkin pendingSkin;

	public Color Color { get; private set; }

	public string SkinDisplayName => requestedSkinDisplayName;

	public BoardSkinVisual(GameObject owner, Transform visualSlot, GameObject defaultVisualPrefab)
	{
		this.owner = owner;
		this.visualSlot = visualSlot;
		if (!(defaultVisualPrefab == null))
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

	public void SetColor(Color color)
	{
		if (colorMaterial != null)
		{
			colorMaterial.color = color;
		}
		if (color != Color)
		{
			Color = color;
			activeCosmetic?.ApplyColor(color);
		}
	}

	public void ApplySkin(string displayName)
	{
		if (string.IsNullOrEmpty(displayName) || displayName == "NOTHING")
		{
			displayName = null;
		}
		if (pendingSkin.IsValid)
		{
			bool flag = displayName == requestedSkinDisplayName && activeSkinInstance != null;
			if (pendingSkin.displayName == displayName && !flag)
			{
				TryAttachPendingSkin();
				return;
			}
			pendingSkin.Destroy();
		}
		if (displayName == requestedSkinDisplayName)
		{
			return;
		}
		DestroyActiveSkin();
		requestedSkinDisplayName = displayName;
		if (displayName == null)
		{
			return;
		}
		CosmeticSO cosmeticSO = CosmeticsController.instance?.GetCosmeticSOFromDisplayName(displayName);
		if (!cosmeticSO.IsNull() && !(cosmeticSO.info.category != CosmeticsController.CosmeticCategory.Hoverboard) && cosmeticSO.info.functionalParts != null && cosmeticSO.info.functionalParts.Length != 0)
		{
			int requestedVersion = skinLoadVersion;
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
		if (requestedVersion != skinLoadVersion)
		{
			return;
		}
		if (owner == null || !owner.activeInHierarchy || handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
		{
			if (handle.IsValid())
			{
				Addressables.Release(handle);
			}
			skinLoadHandle = default(AsyncOperationHandle<GameObject>);
			requestedSkinDisplayName = null;
		}
		else
		{
			SetDefaultVisualActive(active: false);
			activeSkinInstance = Object.Instantiate(handle.Result, visualSlot);
			activeSkinInstance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
			activeCosmetic = activeSkinInstance.GetComponent<HoverboardCosmetic>();
			activeCosmetic?.Apply(Color);
		}
	}

	public bool TryTransferSkinTo(BoardSkinVisual target)
	{
		if (!TryDetachSkin(out var skin))
		{
			return false;
		}
		if (skin.HasLoadedPrefab)
		{
			Object.Destroy(skin.instance);
			skin.instance = null;
			skin.cosmetic = null;
		}
		target.AttachSkin(target.WithInstance(skin));
		return true;
	}

	public bool TryTransferSkinToPending(BoardSkinVisual target)
	{
		if (!TryDetachSkin(out var skin))
		{
			return false;
		}
		target.SetPendingSkin(skin);
		return true;
	}

	private bool TryDetachSkin(out TransferableSkin skin)
	{
		if (activeSkinInstance == null)
		{
			skin = default(TransferableSkin);
			return false;
		}
		skin = new TransferableSkin
		{
			displayName = requestedSkinDisplayName,
			instance = activeSkinInstance,
			cosmetic = activeCosmetic,
			handle = skinLoadHandle
		};
		activeSkinInstance = null;
		activeCosmetic = null;
		skinLoadHandle = default(AsyncOperationHandle<GameObject>);
		DestroyActiveSkin();
		return true;
	}

	private void AttachSkin(TransferableSkin skin)
	{
		DestroyActiveSkin();
		requestedSkinDisplayName = skin.displayName;
		skinLoadHandle = skin.handle;
		activeSkinInstance = skin.instance;
		activeCosmetic = skin.cosmetic;
		activeSkinInstance.transform.SetParent(visualSlot, worldPositionStays: false);
		activeSkinInstance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		activeSkinInstance.SetActive(value: true);
		SetDefaultVisualActive(active: false);
		activeCosmetic?.Apply(Color);
	}

	private void SetPendingSkin(TransferableSkin skin)
	{
		pendingSkin.Destroy();
		if (skin.instance != null)
		{
			Object.Destroy(skin.instance);
		}
		skin.instance = null;
		skin.cosmetic = null;
		if (!skin.HasLoadedPrefab)
		{
			skin.Destroy();
		}
		else
		{
			pendingSkin = skin;
		}
	}

	public bool TryAttachPendingSkin()
	{
		if (!pendingSkin.IsValid)
		{
			return false;
		}
		TransferableSkin skin = pendingSkin;
		pendingSkin = default(TransferableSkin);
		AttachSkin(WithInstance(skin));
		return true;
	}

	private TransferableSkin WithInstance(TransferableSkin skin)
	{
		if (skin.instance == null)
		{
			skin.instance = Object.Instantiate(skin.handle.Result, visualSlot);
			skin.cosmetic = skin.instance.GetComponent<HoverboardCosmetic>();
		}
		return skin;
	}

	public void Clear()
	{
		pendingSkin.Destroy();
		DestroyActiveSkin();
	}

	private void DestroyActiveSkin()
	{
		skinLoadVersion++;
		requestedSkinDisplayName = null;
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
		SetDefaultVisualActive(active: true);
	}

	private void SetDefaultVisualActive(bool active)
	{
		if (defaultVisualInstance != null)
		{
			defaultVisualInstance.SetActive(active);
		}
	}
}
