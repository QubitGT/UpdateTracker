using System;
using System.Collections.Generic;
using Unity.Cinemachine.TargetTracking;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.Cinemachine;

[Obsolete("This is deprecated. Use Create -> Cinemachine -> FreeLook camera, or create a CinemachineCamera with appropriate components")]
[DisallowMultipleComponent]
[ExecuteAlways]
[ExcludeFromPreset]
[AddComponentMenu("")]
public class CinemachineFreeLook : CinemachineVirtualCameraBase, AxisState.IRequiresInput, ICinemachineMixer, ICinemachineCamera
{
	[Serializable]
	public struct Orbit
	{
		public float m_Height;

		public float m_Radius;

		public Orbit(float h, float r)
		{
			m_Height = h;
			m_Radius = r;
		}
	}

	[Serializable]
	private struct LegacyTransitionParams
	{
		[FormerlySerializedAs("m_PositionBlending")]
		public int m_BlendHint;

		public bool m_InheritPosition;

		public CinemachineLegacyCameraEvents.OnCameraLiveEvent m_OnCameraLive;
	}

	public delegate CinemachineVirtualCamera CreateRigDelegate(CinemachineFreeLook vcam, string name, CinemachineVirtualCamera copyFrom);

	public delegate void DestroyRigDelegate(GameObject rig);

	[Tooltip("Object for the camera children to look at (the aim target).")]
	[NoSaveDuringPlay]
	[VcamTargetProperty]
	public Transform m_LookAt;

	[Tooltip("Object for the camera children wants to move with (the body target).")]
	[NoSaveDuringPlay]
	[VcamTargetProperty]
	public Transform m_Follow;

	[Tooltip("If enabled, this lens setting will apply to all three child rigs, otherwise the child rig lens settings will be used")]
	[FormerlySerializedAs("m_UseCommonLensSetting")]
	public bool m_CommonLens = true;

	[Tooltip("Specifies the lens properties of this Virtual Camera.  This generally mirrors the Unity Camera's lens settings, and will be used to drive the Unity camera when the vcam is active")]
	[FormerlySerializedAs("m_LensAttributes")]
	public LegacyLensSettings m_Lens = LegacyLensSettings.Default;

	[Tooltip("Hint for transitioning to and from this CinemachineCamera.  Hints can be combined, although not all combinations make sense.  In the case of conflicting hints, Cinemachine will make an arbitrary choice.")]
	public CinemachineCore.BlendHints BlendHint;

	[Tooltip("This event fires when a transition occurs")]
	public CinemachineLegacyCameraEvents.OnCameraLiveEvent m_OnCameraLiveEvent = new CinemachineLegacyCameraEvents.OnCameraLiveEvent();

	[Header("Axis Control")]
	[Tooltip("The Vertical axis.  Value is 0..1.  Chooses how to blend the child rigs")]
	public AxisState m_YAxis = new AxisState(0f, 1f, wrap: false, rangeLocked: true, 2f, 0.2f, 0.1f, "Mouse Y", invert: false);

	[Tooltip("Controls how automatic recentering of the Y axis is accomplished")]
	public AxisState.Recentering m_YAxisRecentering = new AxisState.Recentering(enabled: false, 1f, 2f);

	[Tooltip("The Horizontal axis.  Value is -180...180.  This is passed on to the rigs' OrbitalTransposer component")]
	public AxisState m_XAxis = new AxisState(-180f, 180f, wrap: true, rangeLocked: false, 300f, 0.1f, 0.1f, "Mouse X", invert: true);

	[Tooltip("The definition of Forward.  Camera will follow behind.")]
	public CinemachineOrbitalTransposer.Heading m_Heading = new CinemachineOrbitalTransposer.Heading(CinemachineOrbitalTransposer.Heading.HeadingDefinition.TargetForward, 4, 0f);

	[Tooltip("Controls how automatic recentering of the X axis is accomplished")]
	public AxisState.Recentering m_RecenterToTargetHeading = new AxisState.Recentering(enabled: false, 1f, 2f);

	[Header("Orbits")]
	[Tooltip("The coordinate space to use when interpreting the offset from the target.  This is also used to set the camera's Up vector, which will be maintained when aiming the camera.")]
	public BindingMode m_BindingMode = BindingMode.LazyFollow;

	[Tooltip("Controls how taut is the line that connects the rigs' orbits, which determines final placement on the Y axis")]
	[Range(0f, 1f)]
	[FormerlySerializedAs("m_SplineTension")]
	public float m_SplineCurvature = 0.2f;

	[Tooltip("The radius and height of the three orbiting rigs.")]
	public Orbit[] m_Orbits = new Orbit[3]
	{
		new Orbit(4.5f, 1.75f),
		new Orbit(2.5f, 3f),
		new Orbit(0.4f, 1.3f)
	};

	[SerializeField]
	[HideInInspector]
	[FormerlySerializedAs("m_HeadingBias")]
	private float m_LegacyHeadingBias = float.MaxValue;

	private bool mUseLegacyRigDefinitions;

	[SerializeField]
	[HideInInspector]
	private LegacyTransitionParams m_LegacyTransitions;

	private bool mIsDestroyed;

	private CameraState m_State = CameraState.Default;

	[SerializeField]
	[HideInInspector]
	[NoSaveDuringPlay]
	private CinemachineVirtualCamera[] m_Rigs = new CinemachineVirtualCamera[3];

	private CinemachineOrbitalTransposer[] mOrbitals;

	private CinemachineBlend mBlendA;

	private CinemachineBlend mBlendB;

	public static CreateRigDelegate CreateRigOverride;

	public static DestroyRigDelegate DestroyRigOverride;

	private float m_CachedXAxisHeading;

	private float m_LastHeadingUpdateFrame;

	private LensSettings m_LensSettings;

	private Orbit[] m_CachedOrbits;

	private float m_CachedTension;

	private Vector4[] m_CachedKnots;

	private Vector4[] m_CachedCtrl1;

	private Vector4[] m_CachedCtrl2;

	protected internal override bool IsDprecated => true;

	internal bool RigsAreCreated
	{
		get
		{
			if (m_Rigs != null)
			{
				return m_Rigs.Length == 3;
			}
			return false;
		}
	}

	public static string[] RigNames => new string[3] { "TopRig", "MiddleRig", "BottomRig" };

	public override bool PreviousStateIsValid
	{
		get
		{
			return base.PreviousStateIsValid;
		}
		set
		{
			if (!value)
			{
				int num = 0;
				while (m_Rigs != null && num < m_Rigs.Length)
				{
					if (m_Rigs[num] != null)
					{
						m_Rigs[num].PreviousStateIsValid = value;
					}
					num++;
				}
			}
			base.PreviousStateIsValid = value;
		}
	}

	public override CameraState State => m_State;

	public override Transform LookAt
	{
		get
		{
			return ResolveLookAt(m_LookAt);
		}
		set
		{
			m_LookAt = value;
		}
	}

	public override Transform Follow
	{
		get
		{
			return ResolveFollow(m_Follow);
		}
		set
		{
			m_Follow = value;
		}
	}

	protected internal override void PerformLegacyUpgrade(int streamedVersion)
	{
		base.PerformLegacyUpgrade(streamedVersion);
		if (streamedVersion >= 20221011)
		{
			return;
		}
		if (m_LegacyHeadingBias != float.MaxValue)
		{
			m_Heading.m_Bias = m_LegacyHeadingBias;
			m_LegacyHeadingBias = float.MaxValue;
			int heading = (int)m_Heading.m_Definition;
			if (m_RecenterToTargetHeading.LegacyUpgrade(ref heading, ref m_Heading.m_VelocityFilterStrength))
			{
				m_Heading.m_Definition = (CinemachineOrbitalTransposer.Heading.HeadingDefinition)heading;
			}
			mUseLegacyRigDefinitions = true;
		}
		if (m_LegacyTransitions.m_BlendHint != 0)
		{
			if (m_LegacyTransitions.m_BlendHint == 3)
			{
				BlendHint = CinemachineCore.BlendHints.ScreenSpaceAimWhenTargetsDiffer;
			}
			else
			{
				BlendHint = (CinemachineCore.BlendHints)m_LegacyTransitions.m_BlendHint;
			}
			m_LegacyTransitions.m_BlendHint = 0;
		}
		if (m_LegacyTransitions.m_InheritPosition)
		{
			BlendHint |= CinemachineCore.BlendHints.InheritPosition;
			m_LegacyTransitions.m_InheritPosition = false;
		}
		if (m_LegacyTransitions.m_OnCameraLive != null)
		{
			m_OnCameraLiveEvent = m_LegacyTransitions.m_OnCameraLive;
			m_LegacyTransitions.m_OnCameraLive = null;
		}
	}

	private void OnValidate()
	{
		m_YAxis.Validate();
		m_XAxis.Validate();
		m_RecenterToTargetHeading.Validate();
		m_YAxisRecentering.Validate();
		m_Lens.Validate();
		InvalidateRigCache();
	}

	public CinemachineVirtualCamera GetRig(int i)
	{
		if (!UpdateRigCache() || i < 0 || i >= 3)
		{
			return null;
		}
		return m_Rigs[i];
	}

	protected override void OnEnable()
	{
		mIsDestroyed = false;
		base.OnEnable();
		InvalidateRigCache();
		UpdateInputAxisProvider();
	}

	internal void UpdateInputAxisProvider()
	{
		m_XAxis.SetInputAxisProvider(0, null);
		m_YAxis.SetInputAxisProvider(1, null);
		AxisState.IInputAxisProvider component = GetComponent<AxisState.IInputAxisProvider>();
		if (component != null)
		{
			m_XAxis.SetInputAxisProvider(0, component);
			m_YAxis.SetInputAxisProvider(1, component);
		}
	}

	protected override void OnDestroy()
	{
		if (m_Rigs != null)
		{
			CinemachineVirtualCamera[] rigs = m_Rigs;
			foreach (CinemachineVirtualCamera cinemachineVirtualCamera in rigs)
			{
				if (cinemachineVirtualCamera != null && cinemachineVirtualCamera.gameObject != null)
				{
					cinemachineVirtualCamera.gameObject.hideFlags &= ~(HideFlags.HideInHierarchy | HideFlags.HideInInspector);
				}
			}
		}
		mIsDestroyed = true;
		base.OnDestroy();
	}

	private void OnTransformChildrenChanged()
	{
		InvalidateRigCache();
	}

	private void Reset()
	{
		DestroyRigs();
		UpdateRigCache();
		Priority = default(PrioritySettings);
		OutputChannel = OutputChannels.Default;
	}

	public bool IsLiveChild(ICinemachineCamera vcam, bool dominantChildOnly = false)
	{
		if (!RigsAreCreated)
		{
			return false;
		}
		float yAxisValue = GetYAxisValue();
		if (dominantChildOnly)
		{
			if (vcam == m_Rigs[0])
			{
				return yAxisValue > 0.666f;
			}
			if (vcam == m_Rigs[2])
			{
				return (double)yAxisValue < 0.333;
			}
			if (vcam == m_Rigs[1])
			{
				if (yAxisValue >= 0.333f)
				{
					return yAxisValue <= 0.666f;
				}
				return false;
			}
			return false;
		}
		if (vcam == m_Rigs[1])
		{
			return true;
		}
		if (yAxisValue < 0.5f)
		{
			return vcam == m_Rigs[2];
		}
		return vcam == m_Rigs[0];
	}

	public override void OnTargetObjectWarped(Transform target, Vector3 positionDelta)
	{
		UpdateRigCache();
		if (RigsAreCreated)
		{
			CinemachineVirtualCamera[] rigs = m_Rigs;
			for (int i = 0; i < rigs.Length; i++)
			{
				rigs[i].OnTargetObjectWarped(target, positionDelta);
			}
		}
		base.OnTargetObjectWarped(target, positionDelta);
	}

	public override void ForceCameraPosition(Vector3 pos, Quaternion rot)
	{
		Vector3 referenceUp = State.ReferenceUp;
		m_YAxis.Value = GetYAxisClosestValue(pos, referenceUp);
		PreviousStateIsValid = true;
		base.transform.position = pos;
		base.transform.rotation = rot;
		m_State.RawPosition = pos;
		m_State.RawOrientation = rot;
		if (UpdateRigCache())
		{
			if (m_BindingMode != BindingMode.LazyFollow)
			{
				m_XAxis.Value = mOrbitals[1].GetAxisClosestValue(pos, referenceUp);
			}
			PushSettingsToRigs();
			for (int i = 0; i < 3; i++)
			{
				m_Rigs[i].ForceCameraPosition(pos, rot);
			}
			InternalUpdateCameraState(referenceUp, -1f);
		}
		base.ForceCameraPosition(pos, rot);
	}

	public override void InternalUpdateCameraState(Vector3 worldUp, float deltaTime)
	{
		UpdateTargetCache();
		UpdateRigCache();
		if (RigsAreCreated)
		{
			if (deltaTime < 0f)
			{
				PreviousStateIsValid = false;
			}
			m_State = CalculateNewState(worldUp, deltaTime);
			m_State.BlendHint = (CameraState.BlendHints)BlendHint;
			if (Follow != null)
			{
				Vector3 vector = State.RawPosition - base.transform.position;
				base.transform.position = State.RawPosition;
				m_Rigs[0].transform.position -= vector;
				m_Rigs[1].transform.position -= vector;
				m_Rigs[2].transform.position -= vector;
			}
			InvokePostPipelineStageCallback(this, CinemachineCore.Stage.Finalize, ref m_State, deltaTime);
			PreviousStateIsValid = true;
			if (PreviousStateIsValid && CinemachineCore.IsLive(this) && deltaTime >= 0f && m_YAxis.Update(deltaTime))
			{
				m_YAxisRecentering.CancelRecentering();
			}
			PushSettingsToRigs();
			if (m_BindingMode == BindingMode.LazyFollow)
			{
				m_XAxis.Value = 0f;
			}
		}
	}

	public override void OnTransitionFromCamera(ICinemachineCamera fromCam, Vector3 worldUp, float deltaTime)
	{
		base.OnTransitionFromCamera(fromCam, worldUp, deltaTime);
		if (!RigsAreCreated)
		{
			return;
		}
		InvokeOnTransitionInExtensions(fromCam, worldUp, deltaTime);
		if (fromCam != null && (State.BlendHint & CameraState.BlendHints.InheritPosition) != 0 && !CinemachineCore.IsLiveInBlend(this))
		{
			Vector3 pos = fromCam.State.RawPosition;
			if (fromCam is CinemachineFreeLook)
			{
				CinemachineFreeLook cinemachineFreeLook = fromCam as CinemachineFreeLook;
				CinemachineOrbitalTransposer cinemachineOrbitalTransposer = ((cinemachineFreeLook.mOrbitals != null) ? cinemachineFreeLook.mOrbitals[1] : null);
				if (cinemachineOrbitalTransposer != null)
				{
					pos = cinemachineOrbitalTransposer.GetTargetCameraPosition(worldUp);
				}
			}
			ForceCameraPosition(pos, fromCam.State.GetFinalOrientation());
		}
		if (false)
		{
			for (int i = 0; i < 3; i++)
			{
				m_Rigs[i].InternalUpdateCameraState(worldUp, deltaTime);
			}
			InternalUpdateCameraState(worldUp, deltaTime);
		}
		else
		{
			UpdateCameraState(worldUp, deltaTime);
		}
		m_OnCameraLiveEvent?.Invoke(this, fromCam);
	}

	bool AxisState.IRequiresInput.RequiresInput()
	{
		return true;
	}

	private float GetYAxisClosestValue(Vector3 cameraPos, Vector3 up)
	{
		if (Follow != null)
		{
			Vector3 vector = Quaternion.FromToRotation(up, Vector3.up) * (cameraPos - Follow.position);
			Vector3 vector2 = vector;
			vector2.y = 0f;
			if (!vector2.AlmostZero())
			{
				vector = Quaternion.AngleAxis(UnityVectorExtensions.SignedAngle(vector2, Vector3.back, Vector3.up), Vector3.up) * vector;
			}
			vector.x = 0f;
			return SteepestDescent(vector.normalized * (cameraPos - Follow.position).magnitude);
		}
		return m_YAxis.Value;
	}

	private float SteepestDescent(Vector3 cameraOffset)
	{
		float num = InitialGuess();
		for (int i = 0; i < 5; i++)
		{
			float num2 = AngleFunction(num);
			float num3 = SlopeOfAngleFunction(num);
			if (Mathf.Abs(num3) < 0.005f || Mathf.Abs(num2) < 0.005f)
			{
				break;
			}
			num = Mathf.Clamp01(num - num2 / num3);
		}
		return num;
		float AngleFunction(float input)
		{
			Vector3 localPositionForCameraFromInput = GetLocalPositionForCameraFromInput(input);
			return Mathf.Abs(UnityVectorExtensions.SignedAngle(cameraOffset, localPositionForCameraFromInput, Vector3.right));
		}
		float InitialGuess()
		{
			UpdateCachedSpline();
			float best = 0.5f;
			float bestAngle = AngleFunction(best);
			for (int j = 0; j <= 5; j++)
			{
				float num5 = (float)j * 0.1f;
				ChooseBestAngle(0.5f + num5);
				ChooseBestAngle(0.5f - num5);
			}
			return best;
			void ChooseBestAngle(float x)
			{
				float num6 = AngleFunction(x);
				if (num6 < bestAngle)
				{
					bestAngle = num6;
					best = x;
				}
			}
		}
		float SlopeOfAngleFunction(float input)
		{
			float num4 = AngleFunction(input - 0.005f);
			return (AngleFunction(input + 0.005f) - num4) / 0.01f;
		}
	}

	private void InvalidateRigCache()
	{
		mOrbitals = null;
	}

	private void DestroyRigs()
	{
		List<CinemachineVirtualCamera> list = new List<CinemachineVirtualCamera>(3);
		for (int i = 0; i < RigNames.Length; i++)
		{
			foreach (Transform item in base.transform)
			{
				if (item.gameObject.name == RigNames[i])
				{
					list.Add(item.GetComponent<CinemachineVirtualCamera>());
				}
			}
		}
		foreach (CinemachineVirtualCamera item2 in list)
		{
			if (!(item2 != null))
			{
				continue;
			}
			if (DestroyRigOverride != null)
			{
				DestroyRigOverride(item2.gameObject);
				continue;
			}
			item2.DestroyPipeline();
			UnityEngine.Object.Destroy(item2);
			if (!RuntimeUtility.IsPrefab(base.gameObject))
			{
				UnityEngine.Object.Destroy(item2.gameObject);
			}
		}
		mOrbitals = null;
		m_Rigs = null;
	}

	private CinemachineVirtualCamera[] CreateRigs(CinemachineVirtualCamera[] copyFrom)
	{
		float[] array = new float[3] { 0.5f, 0.55f, 0.6f };
		mOrbitals = null;
		m_Rigs = null;
		CinemachineVirtualCamera[] array2 = new CinemachineVirtualCamera[3];
		for (int i = 0; i < array2.Length; i++)
		{
			CinemachineVirtualCamera cinemachineVirtualCamera = ((copyFrom != null && copyFrom.Length > i) ? copyFrom[i] : null);
			if (CreateRigOverride != null)
			{
				array2[i] = CreateRigOverride(this, RigNames[i], cinemachineVirtualCamera);
			}
			else
			{
				GameObject gameObject = null;
				foreach (Transform item in base.transform)
				{
					if (item.gameObject.name == RigNames[i])
					{
						gameObject = item.gameObject;
						break;
					}
				}
				if (gameObject == null && !RuntimeUtility.IsPrefab(base.gameObject))
				{
					gameObject = new GameObject(RigNames[i]);
					gameObject.transform.parent = base.transform;
				}
				if (gameObject == null)
				{
					array2[i] = null;
				}
				else
				{
					array2[i] = gameObject.AddComponent<CinemachineVirtualCamera>();
					array2[i].AddCinemachineComponent<CinemachineOrbitalTransposer>();
					array2[i].AddCinemachineComponent<CinemachineComposer>();
				}
			}
			if (!(array2[i] != null))
			{
				continue;
			}
			array2[i].InvalidateComponentPipeline();
			CinemachineOrbitalTransposer cinemachineOrbitalTransposer = array2[i].GetCinemachineComponent<CinemachineOrbitalTransposer>();
			if (cinemachineOrbitalTransposer == null)
			{
				cinemachineOrbitalTransposer = array2[i].AddCinemachineComponent<CinemachineOrbitalTransposer>();
			}
			if (cinemachineVirtualCamera == null)
			{
				cinemachineOrbitalTransposer.m_YawDamping = 0f;
				CinemachineComposer cinemachineComponent = array2[i].GetCinemachineComponent<CinemachineComposer>();
				if (cinemachineComponent != null)
				{
					cinemachineComponent.m_HorizontalDamping = (cinemachineComponent.m_VerticalDamping = 0f);
					cinemachineComponent.m_ScreenX = 0.5f;
					cinemachineComponent.m_ScreenY = array[i];
					cinemachineComponent.m_DeadZoneWidth = (cinemachineComponent.m_DeadZoneHeight = 0f);
					cinemachineComponent.m_SoftZoneWidth = (cinemachineComponent.m_SoftZoneHeight = 0.6f);
					cinemachineComponent.m_BiasX = (cinemachineComponent.m_BiasY = 0f);
				}
			}
		}
		return array2;
	}

	private bool UpdateRigCache()
	{
		if (mIsDestroyed)
		{
			return false;
		}
		if (mOrbitals != null && mOrbitals.Length == 3)
		{
			return true;
		}
		m_CachedXAxisHeading = 0f;
		m_Rigs = null;
		mOrbitals = null;
		List<CinemachineVirtualCamera> list = LocateExistingRigs(forceOrbital: false);
		if (list == null || list.Count != 3)
		{
			DestroyRigs();
			CreateRigs(null);
			list = LocateExistingRigs(forceOrbital: true);
		}
		if (list != null && list.Count == 3)
		{
			m_Rigs = list.ToArray();
		}
		if (RigsAreCreated)
		{
			mOrbitals = new CinemachineOrbitalTransposer[m_Rigs.Length];
			for (int i = 0; i < m_Rigs.Length; i++)
			{
				mOrbitals[i] = m_Rigs[i].GetCinemachineComponent<CinemachineOrbitalTransposer>();
			}
			CinemachineVirtualCamera[] rigs = m_Rigs;
			foreach (CinemachineVirtualCamera cinemachineVirtualCamera in rigs)
			{
				if (!(cinemachineVirtualCamera == null))
				{
					cinemachineVirtualCamera.m_ExcludedPropertiesInInspector = ((!m_CommonLens) ? new string[8] { "m_Script", "Header", "Extensions", "Priority", "OutputChannel", "m_Transitions", "m_Follow", "m_StandbyUpdate" } : new string[9] { "m_Script", "Header", "Extensions", "Priority", "OutputChannel", "m_Transitions", "m_Follow", "m_StandbyUpdate", "m_Lens" });
					cinemachineVirtualCamera.m_LockStageInInspector = new CinemachineCore.Stage[1];
				}
			}
			mBlendA = new CinemachineBlend
			{
				CamA = m_Rigs[1],
				CamB = m_Rigs[0],
				BlendCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f),
				Duration = 1f
			};
			mBlendB = new CinemachineBlend
			{
				CamA = m_Rigs[2],
				CamB = m_Rigs[1],
				BlendCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f),
				Duration = 1f
			};
			return true;
		}
		return false;
	}

	private List<CinemachineVirtualCamera> LocateExistingRigs(bool forceOrbital)
	{
		m_CachedXAxisHeading = m_XAxis.Value;
		m_LastHeadingUpdateFrame = -1f;
		List<CinemachineVirtualCamera> list = new List<CinemachineVirtualCamera>(3);
		foreach (Transform item in base.transform)
		{
			CinemachineVirtualCamera component = item.GetComponent<CinemachineVirtualCamera>();
			if (!(component != null))
			{
				continue;
			}
			GameObject gameObject = item.gameObject;
			for (int i = 0; i < RigNames.Length; i++)
			{
				if (!(gameObject.name != RigNames[i]))
				{
					CinemachineOrbitalTransposer cinemachineOrbitalTransposer = component.GetCinemachineComponent<CinemachineOrbitalTransposer>();
					if (cinemachineOrbitalTransposer == null && forceOrbital)
					{
						cinemachineOrbitalTransposer = component.AddCinemachineComponent<CinemachineOrbitalTransposer>();
					}
					if (cinemachineOrbitalTransposer != null)
					{
						cinemachineOrbitalTransposer.m_HeadingIsDriven = true;
						cinemachineOrbitalTransposer.HideOffsetInInspector = true;
						cinemachineOrbitalTransposer.m_XAxis.m_InputAxisName = string.Empty;
						cinemachineOrbitalTransposer.HeadingUpdater = UpdateXAxisHeading;
						cinemachineOrbitalTransposer.m_RecenterToTargetHeading.m_enabled = false;
						component.StandbyUpdate = StandbyUpdate;
						list.Add(component);
					}
				}
			}
		}
		return list;
	}

	private float UpdateXAxisHeading(CinemachineOrbitalTransposer orbital, float deltaTime, Vector3 up)
	{
		if (this == null)
		{
			return 0f;
		}
		if (m_LastHeadingUpdateFrame != (float)CinemachineCore.CurrentUpdateFrame)
		{
			m_LastHeadingUpdateFrame = CinemachineCore.CurrentUpdateFrame;
			float value = m_XAxis.Value;
			m_CachedXAxisHeading = orbital.UpdateHeading(PreviousStateIsValid ? deltaTime : (-1f), up, ref m_XAxis, ref m_RecenterToTargetHeading, CinemachineCore.IsLive(this));
			if (m_BindingMode == BindingMode.LazyFollow)
			{
				m_XAxis.Value = value;
			}
		}
		return m_CachedXAxisHeading;
	}

	private void PushSettingsToRigs()
	{
		for (int i = 0; i < m_Rigs.Length; i++)
		{
			if (m_CommonLens)
			{
				m_Rigs[i].m_Lens = m_Lens;
			}
			if (mUseLegacyRigDefinitions)
			{
				mUseLegacyRigDefinitions = false;
				m_Orbits[i].m_Height = mOrbitals[i].m_FollowOffset.y;
				m_Orbits[i].m_Radius = 0f - mOrbitals[i].m_FollowOffset.z;
				if (m_Rigs[i].Follow != null)
				{
					Follow = m_Rigs[i].Follow;
				}
			}
			m_Rigs[i].Follow = null;
			m_Rigs[i].StandbyUpdate = StandbyUpdate;
			m_Rigs[i].FollowTargetAttachment = FollowTargetAttachment;
			m_Rigs[i].LookAtTargetAttachment = LookAtTargetAttachment;
			if (!PreviousStateIsValid)
			{
				m_Rigs[i].PreviousStateIsValid = false;
				m_Rigs[i].transform.position = base.transform.position;
				m_Rigs[i].transform.rotation = base.transform.rotation;
			}
			mOrbitals[i].m_FollowOffset = GetLocalPositionForCameraFromInput(GetYAxisValue());
			mOrbitals[i].m_BindingMode = m_BindingMode;
			mOrbitals[i].m_Heading = m_Heading;
			mOrbitals[i].m_XAxis.Value = m_XAxis.Value;
			if (m_BindingMode == BindingMode.LazyFollow)
			{
				m_Rigs[i].SetStateRawPosition(State.RawPosition);
			}
		}
	}

	private float GetYAxisValue()
	{
		float num = m_YAxis.m_MaxValue - m_YAxis.m_MinValue;
		if (!(num > 0.0001f))
		{
			return 0.5f;
		}
		return m_YAxis.Value / num;
	}

	private CameraState CalculateNewState(Vector3 worldUp, float deltaTime)
	{
		m_LensSettings = m_Lens.ToLensSettings();
		CameraState result = PullStateFromVirtualCamera(worldUp, ref m_LensSettings);
		m_YAxisRecentering.DoRecentering(ref m_YAxis, deltaTime, 0.5f);
		float yAxisValue = GetYAxisValue();
		if (yAxisValue > 0.5f)
		{
			if (mBlendA != null)
			{
				mBlendA.TimeInBlend = (yAxisValue - 0.5f) * 2f;
				mBlendA.UpdateCameraState(worldUp, deltaTime);
				result = mBlendA.State;
			}
		}
		else if (mBlendB != null)
		{
			mBlendB.TimeInBlend = yAxisValue * 2f;
			mBlendB.UpdateCameraState(worldUp, deltaTime);
			result = mBlendB.State;
		}
		return result;
	}

	public Vector3 GetLocalPositionForCameraFromInput(float t)
	{
		if (mOrbitals == null)
		{
			return Vector3.zero;
		}
		UpdateCachedSpline();
		int num = 1;
		if (t > 0.5f)
		{
			t -= 0.5f;
			num = 2;
		}
		return SplineHelpers.Bezier3(t * 2f, m_CachedKnots[num], m_CachedCtrl1[num], m_CachedCtrl2[num], m_CachedKnots[num + 1]);
	}

	private void UpdateCachedSpline()
	{
		bool flag = m_CachedOrbits != null && m_CachedOrbits.Length == 3 && m_CachedTension == m_SplineCurvature;
		for (int i = 0; i < 3 && flag; i++)
		{
			flag = m_CachedOrbits[i].m_Height == m_Orbits[i].m_Height && m_CachedOrbits[i].m_Radius == m_Orbits[i].m_Radius;
		}
		if (!flag)
		{
			float splineCurvature = m_SplineCurvature;
			m_CachedKnots = new Vector4[5];
			m_CachedCtrl1 = new Vector4[5];
			m_CachedCtrl2 = new Vector4[5];
			m_CachedKnots[1] = new Vector4(0f, m_Orbits[2].m_Height, 0f - m_Orbits[2].m_Radius, 0f);
			m_CachedKnots[2] = new Vector4(0f, m_Orbits[1].m_Height, 0f - m_Orbits[1].m_Radius, 0f);
			m_CachedKnots[3] = new Vector4(0f, m_Orbits[0].m_Height, 0f - m_Orbits[0].m_Radius, 0f);
			m_CachedKnots[0] = Vector4.Lerp(m_CachedKnots[1], Vector4.zero, splineCurvature);
			m_CachedKnots[4] = Vector4.Lerp(m_CachedKnots[3], Vector4.zero, splineCurvature);
			SplineHelpers.ComputeSmoothControlPoints(ref m_CachedKnots, ref m_CachedCtrl1, ref m_CachedCtrl2);
			m_CachedOrbits = new Orbit[3];
			for (int j = 0; j < 3; j++)
			{
				m_CachedOrbits[j] = m_Orbits[j];
			}
			m_CachedTension = m_SplineCurvature;
		}
	}
}
