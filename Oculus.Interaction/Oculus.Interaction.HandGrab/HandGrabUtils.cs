using System;
using System.Collections.Generic;
using Oculus.Interaction.Grab;
using Oculus.Interaction.Grab.GrabSurfaces;
using Oculus.Interaction.GrabAPI;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.HandGrab;

public static class HandGrabUtils
{
	[Serializable]
	public struct HandGrabInteractableData
	{
		public List<HandGrabPoseData> poses;

		public GrabTypeFlags grabType;

		public HandAlignType handAlignment;

		public PoseMeasureParameters scoringModifier;

		public GrabbingRule pinchGrabRules;

		public GrabbingRule palmGrabRules;
	}

	[Serializable]
	public struct HandGrabPoseData
	{
		public Pose gripPose;

		public HandPose handPose;

		public float scale;
	}

	public static HandGrabInteractable CreateHandGrabInteractable(Transform parent, string name = null)
	{
		GameObject gameObject = new GameObject(name ?? "HandGrabInteractable");
		gameObject.transform.SetParent(parent, worldPositionStays: false);
		gameObject.SetActive(value: false);
		HandGrabInteractable handGrabInteractable = gameObject.AddComponent<HandGrabInteractable>();
		handGrabInteractable.InjectRigidbody(parent.GetComponentInParent<Rigidbody>());
		handGrabInteractable.InjectOptionalPointableElement(parent.GetComponentInParent<Grabbable>());
		gameObject.SetActive(value: true);
		return handGrabInteractable;
	}

	public static HandGrabPose CreateHandGrabPose(Transform parent, Transform relativeTo)
	{
		GameObject gameObject = new GameObject("HandGrabPose");
		gameObject.transform.SetParent(parent, worldPositionStays: false);
		HandGrabPose handGrabPose = gameObject.AddComponent<HandGrabPose>();
		handGrabPose.InjectAllHandGrabPose(relativeTo);
		return handGrabPose;
	}

	public static void MirrorHandGrabPose(HandGrabPose originalPoint, HandGrabPose mirrorPoint, Transform relativeTo)
	{
		Handedness handedness = ((originalPoint.HandPose.Handedness == Handedness.Left) ? Handedness.Right : Handedness.Left);
		HandGrabPoseData data = SaveHandGrabPoseData(originalPoint);
		HandPose handPose = data.handPose;
		handPose.Handedness = handedness;
		for (int i = 0; i < handPose.JointRotations.Length; i++)
		{
			handPose.JointRotations[i] = HandMirroring.Mirror(in handPose.JointRotations[i]);
		}
		if (originalPoint.SnapSurface != null)
		{
			data.gripPose = originalPoint.SnapSurface.MirrorPose(in data.gripPose, relativeTo);
		}
		else
		{
			Quaternion quaternion = Quaternion.Euler(180f, 0f, 180f);
			data.gripPose = HandMirroring.Mirror(data.gripPose);
			data.gripPose.position = quaternion * data.gripPose.position;
			data.gripPose.rotation = quaternion * data.gripPose.rotation;
		}
		LoadHandGrabPoseData(mirrorPoint, data, relativeTo);
		if (originalPoint.SnapSurface != null)
		{
			IGrabSurface surface = originalPoint.SnapSurface.CreateMirroredSurface(mirrorPoint.gameObject);
			mirrorPoint.InjectOptionalSurface(surface);
		}
	}

	private static HandGrabPoseData SaveHandGrabPoseData(HandGrabPose handGrabPose)
	{
		HandGrabPoseData result = default(HandGrabPoseData);
		result.handPose = new HandPose(handGrabPose.HandPose);
		result.scale = handGrabPose.RelativeScale;
		result.gripPose = handGrabPose.RelativePose;
		return result;
	}

	private static void LoadHandGrabPoseData(HandGrabPose handGrabPose, HandGrabPoseData data, Transform relativeTo)
	{
		handGrabPose.transform.localScale = Vector3.one * data.scale;
		Transform transform = handGrabPose.transform;
		Pose pose = PoseUtils.GlobalPoseScaled(relativeTo, data.gripPose);
		transform.SetPose(in pose);
		if (data.handPose != null)
		{
			handGrabPose.InjectOptionalHandPose(new HandPose(data.handPose));
		}
	}

	public static HandGrabInteractableData SaveData(HandGrabInteractable interactable)
	{
		List<HandGrabPoseData> list = new List<HandGrabPoseData>();
		foreach (HandGrabPose handGrabPose in interactable.HandGrabPoses)
		{
			list.Add(SaveHandGrabPoseData(handGrabPose));
		}
		HandGrabInteractableData result = default(HandGrabInteractableData);
		result.poses = list;
		result.scoringModifier = interactable.ScoreModifier;
		result.grabType = interactable.SupportedGrabTypes;
		result.handAlignment = interactable.HandAlignment;
		result.pinchGrabRules = interactable.PinchGrabRules;
		result.palmGrabRules = interactable.PalmGrabRules;
		return result;
	}

	public static void LoadData(HandGrabInteractable interactable, HandGrabInteractableData data)
	{
		interactable.InjectSupportedGrabTypes(data.grabType);
		interactable.InjectPinchGrabRules(data.pinchGrabRules);
		interactable.InjectPalmGrabRules(data.palmGrabRules);
		interactable.InjectOptionalScoreModifier(data.scoringModifier);
		interactable.HandAlignment = data.handAlignment;
		if (data.poses == null)
		{
			return;
		}
		List<HandGrabPose> list = new List<HandGrabPose>();
		foreach (HandGrabPoseData pose in data.poses)
		{
			list.Add(LoadHandGrabPose(interactable, pose));
		}
		interactable.InjectOptionalHandGrabPoses(list);
	}

	public static HandGrabPose LoadHandGrabPose(HandGrabInteractable interactable, HandGrabPoseData poseData)
	{
		HandGrabPose handGrabPose = CreateHandGrabPose(interactable.transform, interactable.RelativeTo);
		LoadHandGrabPoseData(handGrabPose, poseData, interactable.RelativeTo);
		interactable.HandGrabPoses.Add(handGrabPose);
		return handGrabPose;
	}
}
