using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Liv.Lck.Cosmetics;

public class LckGameObjectSwapCosmetic : LckCosmeticDependantBehaviourBase
{
	[Header("Skin Configuration")]
	[SerializeField]
	[Tooltip("The target GameObject to be replaced by the cosmetic skin.")]
	private GameObject _targetGameObject;

	[FormerlySerializedAs("_overridePlayerId")]
	[SerializeField]
	private string _playerId;

	private GameObject _instantiatedCosmetic;

	public Action<GameObject> OnCosmeticSpawned;

	public override string PlayerId
	{
		get
		{
			return _playerId;
		}
		set
		{
			_playerId = value;
		}
	}

	public override void Awake()
	{
		base.Awake();
	}

	public override void OnCosmeticReset()
	{
		if (_instantiatedCosmetic != null)
		{
			UnityEngine.Object.Destroy(_instantiatedCosmetic);
		}
	}

	public override void OnCosmeticLoaded(List<UnityEngine.Object> assets)
	{
		if (_targetGameObject == null)
		{
			LckLog.LogError("LCK: Target GameObject is not assigned on LckGameObjectSwapCosmetic. Cannot apply skin.", "OnCosmeticLoaded", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckGameObjectSwapCosmetic.cs", 47);
			return;
		}
		GameObject gameObject = assets.FirstOrDefault() as GameObject;
		if (gameObject == null)
		{
			LckLog.LogWarning("LCK: Expected a GameObject from the cosmetic bundle, but found none or it was invalid.", "OnCosmeticLoaded", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckGameObjectSwapCosmetic.cs", 55);
			return;
		}
		if (_instantiatedCosmetic != null)
		{
			UnityEngine.Object.Destroy(_instantiatedCosmetic);
		}
		_instantiatedCosmetic = UnityEngine.Object.Instantiate(gameObject, _targetGameObject.transform.parent);
		_instantiatedCosmetic.transform.localPosition = _targetGameObject.transform.localPosition;
		_instantiatedCosmetic.transform.localRotation = _targetGameObject.transform.localRotation;
		_instantiatedCosmetic.transform.localScale = _targetGameObject.transform.localScale;
		SetLayerRecursively(_instantiatedCosmetic, _targetGameObject.layer);
		_targetGameObject.SetActive(value: false);
		OnCosmeticSpawned?.Invoke(_instantiatedCosmetic);
		LckLog.Log("LCK: Applied cosmetic skin '" + gameObject.name + "' to replace '" + _targetGameObject.name + "'.", "OnCosmeticLoaded", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckGameObjectSwapCosmetic.cs", 75);
	}

	private void SetLayerRecursively(GameObject obj, int layer)
	{
		obj.layer = layer;
		Transform[] componentsInChildren = obj.GetComponentsInChildren<Transform>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].gameObject.layer = layer;
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (_instantiatedCosmetic != null)
		{
			UnityEngine.Object.Destroy(_instantiatedCosmetic);
		}
	}
}
