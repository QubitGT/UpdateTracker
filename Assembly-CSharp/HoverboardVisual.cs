using System;
using GorillaExtensions;
using GorillaLocomotion;
using TMPro;
using UnityEngine;
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

	private BoardSkinVisual skinVisual;

	private Vector3 interpolatedLocalPosition;

	private Quaternion interpolatedLocalRotation;

	[SerializeField]
	private float lerpIntoHandDuration;

	private float positionLerpSpeed;

	private float rotationLerpSpeed;

	private bool isCallbackActive;

	public Color boardColor => GetOrCreateSkinVisual().Color;

	public int boardSkinActorNumber { get; private set; }

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

	public BoardSkinVisual GetOrCreateSkinVisual()
	{
		if (skinVisual == null)
		{
			skinVisual = new BoardSkinVisual(base.gameObject, visualSlot, defaultVisualPrefab);
		}
		return skinVisual;
	}

	private void Awake()
	{
		GetOrCreateSkinVisual();
	}

	public void SetIsHeld(bool isHeldLeftHanded, Vector3 localPosition, Quaternion localRotation, Color boardColor, int skinActorNumber)
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
		GetOrCreateSkinVisual().SetColor(boardColor);
		SetSkinActorNumber(skinActorNumber);
	}

	public void SetSkinActorNumber(int actorNumber)
	{
		boardSkinActorNumber = actorNumber;
		if (IsHeld)
		{
			GetOrCreateSkinVisual().ApplySkin(FreeHoverboardManager.GetSkinDisplayName(actorNumber));
		}
	}

	public void RevertSkinIfOwnedBy(int actorNumber)
	{
		if (boardSkinActorNumber == actorNumber)
		{
			SetSkinActorNumber(0);
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
		GetOrCreateSkinVisual().Clear();
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

	private void OnEnable()
	{
		if (parentRig != null)
		{
			VRRig vRRig = parentRig;
			vRRig.OnCosmeticsActivated = (Action<VRRig>)Delegate.Combine(vRRig.OnCosmeticsActivated, new Action<VRRig>(HandleRigCosmeticsActivated));
		}
	}

	private void OnDisable()
	{
		if (parentRig != null)
		{
			VRRig vRRig = parentRig;
			vRRig.OnCosmeticsActivated = (Action<VRRig>)Delegate.Remove(vRRig.OnCosmeticsActivated, new Action<VRRig>(HandleRigCosmeticsActivated));
		}
		skinVisual?.Clear();
	}

	private void HandleRigCosmeticsActivated(VRRig _)
	{
		if (IsHeld)
		{
			SetSkinActorNumber(boardSkinActorNumber);
		}
	}

	public void DropFreeBoard()
	{
		FreeHoverboardManager.instance.SendDropBoardRPC(base.transform.position, base.transform.rotation, velocityEstimator.linearVelocity, velocityEstimator.angularVelocity, boardColor, parentRig.transform.lossyScale, boardSkinActorNumber);
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
