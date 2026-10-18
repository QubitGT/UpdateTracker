using System;
using System.Collections;
using Fusion;
using UnityEngine;
using UnityEngine.UI;

namespace Meta.XR.MultiplayerBlocks.Fusion;

[NetworkBehaviourWeaved(65)]
public class PlayerNameTagFusion : NetworkBehaviour
{
	[WeaverGenerated]
	[SerializeField]
	[DefaultForProperty("OculusName", 0, 65)]
	[DrawIf("IsEditorWritable", true, CompareOperator.Equal, DrawIfMode.ReadOnly)]
	private NetworkString<_64> _OculusName;

	[SerializeField]
	private Text nameTag;

	[SerializeField]
	private GameObject nameTagGO;

	[SerializeField]
	private GameObject nameTagPanel;

	[SerializeField]
	private Transform nameTagContainer;

	[SerializeField]
	private float heightOffset = 0.3f;

	private Transform _centerEye;

	[Networked]
	[OnChangedRender("OnPlayerNameChange")]
	[NetworkedWeaved(0, 65)]
	public unsafe NetworkString<_64> OculusName
	{
		get
		{
			if (base.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing PlayerNameTagFusion.OculusName. Networked properties can only be accessed when Spawned() has been called.");
			}
			return *(NetworkString<_64>*)((byte*)base.Ptr + 0);
		}
		set
		{
			if (base.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing PlayerNameTagFusion.OculusName. Networked properties can only be accessed when Spawned() has been called.");
			}
			*(NetworkString<_64>*)((byte*)base.Ptr + 0) = value;
		}
	}

	private void Start()
	{
		nameTagGO.SetActive(!base.Object.HasStateAuthority);
		OnPlayerNameChange();
		if ((bool)OVRManager.instance)
		{
			_centerEye = OVRManager.instance.GetComponentInChildren<OVRCameraRig>().centerEyeAnchor;
		}
	}

	private IEnumerator UpdateNameUI(string playerName)
	{
		nameTag.text = playerName;
		yield return new WaitForFixedUpdate();
		VerticalLayoutGroup component = nameTagPanel.GetComponent<VerticalLayoutGroup>();
		component.enabled = false;
		component.enabled = true;
	}

	private void OnPlayerNameChange()
	{
		StartCoroutine(UpdateNameUI(OculusName.ToString()));
	}

	public override void FixedUpdateNetwork()
	{
		if (base.Object.HasStateAuthority)
		{
			Vector3 position = base.transform.position;
			nameTagContainer.localPosition = new Vector3(position.x, Mathf.Sin(Time.time * 2f), position.z) * 0.005f;
			Vector3 position2 = _centerEye.transform.position;
			position2.y += heightOffset;
			position = Vector3.Lerp(position, position2, 0.1f);
			base.transform.position = position;
		}
	}

	private void Update()
	{
		if (!base.Object.HasStateAuthority && _centerEye != null)
		{
			nameTagGO.transform.LookAt(_centerEye.position, Vector3.up);
		}
	}

	[WeaverGenerated]
	public override void CopyBackingFieldsToState(bool P_0)
	{
		OculusName = _OculusName;
	}

	[WeaverGenerated]
	public override void CopyStateToBackingFields()
	{
		_OculusName = OculusName;
	}
}
