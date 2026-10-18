using System;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.Cinemachine;

public interface ICinemachineCamera
{
	public struct ActivationEventParams
	{
		public ICinemachineMixer Origin;

		public ICinemachineCamera OutgoingCamera;

		public ICinemachineCamera IncomingCamera;

		public bool IsCut;

		public Vector3 WorldUp;

		public float DeltaTime;
	}

	[Serializable]
	public class ActivationEvent : UnityEvent<ActivationEventParams>
	{
	}

	string Name { get; }

	string Description { get; }

	CameraState State { get; }

	bool IsValid { get; }

	ICinemachineMixer ParentCamera { get; }

	void UpdateCameraState(Vector3 worldUp, float deltaTime);

	void OnCameraActivated(ActivationEventParams evt);
}
