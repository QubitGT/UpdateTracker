using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AOT;
using Unity.Burst;
using Unity.Mathematics;
using Unity.XR.CoreUtils;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Internal;

namespace UnityEngine.XR.Interaction.Toolkit.Gaze;

[MovedFrom("UnityEngine.XR.Interaction.Toolkit")]
[DisallowMultipleComponent]
[AddComponentMenu("XR/XR Gaze Assistance", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Gaze.XRGazeAssistance.html")]
[DefaultExecutionOrder(-29980)]
[BurstCompile]
public class XRGazeAssistance : MonoBehaviour, IXRAimAssist
{
	[Serializable]
	public sealed class InteractorData
	{
		[SerializeField]
		[RequireInterface(typeof(IXRRayProvider))]
		[Tooltip("The interactor that can fall back to gaze data.")]
		private Object m_Interactor;

		[SerializeField]
		[Tooltip("Changes mediation behavior to account for teleportation controls.")]
		private bool m_TeleportRay;

		private bool m_Initialized;

		private IXRRayProvider m_RayProvider;

		private IXRSelectInteractor m_SelectInteractor;

		private bool m_RestoreVisuals;

		private XRInteractorLineVisual m_LineVisual;

		private bool m_HasLineVisual;

		private Transform m_OriginalRayOrigin;

		private Transform m_OriginalAttach;

		private Transform m_OriginalVisualLineOrigin;

		private bool m_OriginalOverrideVisualLineOrigin;

		private Transform m_FallbackRayOrigin;

		private Transform m_FallbackAttach;

		private Transform m_FallbackVisualLineOrigin;

		public Object interactor
		{
			get
			{
				return m_Interactor;
			}
			set
			{
				m_Interactor = value;
			}
		}

		public bool teleportRay
		{
			get
			{
				return m_TeleportRay;
			}
			set
			{
				m_TeleportRay = value;
			}
		}

		public bool fallback { get; private set; }

		internal void Initialize()
		{
			if (m_Initialized)
			{
				return;
			}
			m_RayProvider = m_Interactor as IXRRayProvider;
			m_SelectInteractor = m_Interactor as IXRSelectInteractor;
			if (m_RayProvider == null || m_SelectInteractor == null)
			{
				Debug.LogWarning("No ray and select interactor found!");
				return;
			}
			m_OriginalRayOrigin = m_RayProvider.GetOrCreateRayOrigin();
			m_OriginalAttach = m_RayProvider.GetOrCreateAttachTransform();
			Transform transform = m_SelectInteractor.transform;
			string name = transform.gameObject.name;
			m_FallbackRayOrigin = new GameObject("Gaze Assistance [" + name + "] Ray Origin").transform;
			m_FallbackAttach = new GameObject("Gaze Assistance [" + name + "] Attach").transform;
			m_FallbackRayOrigin.parent = m_OriginalRayOrigin.parent;
			m_FallbackAttach.parent = m_FallbackRayOrigin;
			m_HasLineVisual = transform.TryGetComponent<XRInteractorLineVisual>(out m_LineVisual);
			if (m_HasLineVisual)
			{
				m_FallbackVisualLineOrigin = new GameObject("Gaze Assistance [" + name + "] Visual Origin").transform;
				m_FallbackVisualLineOrigin.parent = m_FallbackRayOrigin.parent;
			}
			m_Initialized = true;
		}

		internal void UpdateFallbackRayOrigin(Transform gazeTransform)
		{
			if (m_Initialized && fallback)
			{
				m_FallbackRayOrigin.SetWorldPose(gazeTransform.GetWorldPose());
			}
		}

		internal void UpdateLineVisualOrigin()
		{
			if (m_Initialized && m_HasLineVisual && fallback)
			{
				TransformExtensions.SetWorldPose(pose: (!m_OriginalOverrideVisualLineOrigin || !(m_OriginalVisualLineOrigin != null)) ? (m_TeleportRay ? new Pose(m_OriginalRayOrigin.position, m_FallbackRayOrigin.rotation) : m_OriginalRayOrigin.GetWorldPose()) : (m_TeleportRay ? new Pose(m_OriginalVisualLineOrigin.position, m_FallbackRayOrigin.rotation) : m_OriginalVisualLineOrigin.GetWorldPose()), transform: m_FallbackVisualLineOrigin);
			}
		}

		internal bool UpdateFallbackState(Transform gazeTransform, float fallbackDivergence, bool selectionLocked)
		{
			if (!m_Initialized)
			{
				return false;
			}
			bool flag = !selectionLocked && Vector3.Angle(gazeTransform.forward, m_OriginalRayOrigin.forward) > fallbackDivergence;
			if (!m_SelectInteractor.isSelectActive)
			{
				if (flag && !fallback)
				{
					if (m_HasLineVisual)
					{
						m_OriginalOverrideVisualLineOrigin = m_LineVisual.overrideInteractorLineOrigin;
						m_OriginalVisualLineOrigin = m_LineVisual.lineOriginTransform;
						m_LineVisual.overrideInteractorLineOrigin = true;
						m_LineVisual.lineOriginTransform = m_FallbackVisualLineOrigin;
					}
					m_RayProvider.SetRayOrigin(m_FallbackRayOrigin);
					m_RayProvider.SetAttachTransform(m_FallbackAttach);
				}
				else if (!flag && fallback)
				{
					if (m_HasLineVisual)
					{
						m_LineVisual.overrideInteractorLineOrigin = m_OriginalOverrideVisualLineOrigin;
						m_LineVisual.lineOriginTransform = m_OriginalVisualLineOrigin;
					}
					m_RayProvider.SetRayOrigin(m_OriginalRayOrigin);
					m_RayProvider.SetAttachTransform(m_OriginalAttach);
					if (!m_TeleportRay)
					{
						m_RestoreVisuals = true;
					}
				}
				fallback = flag;
			}
			if (fallback)
			{
				Pose worldPose = gazeTransform.GetWorldPose();
				if (!m_TeleportRay && m_SelectInteractor.isSelectActive && m_SelectInteractor.hasSelection)
				{
					float t = Mathf.Clamp01((m_FallbackAttach.position - worldPose.position).magnitude / 0.5f);
					Pose worldPose2 = m_OriginalRayOrigin.GetWorldPose();
					m_FallbackRayOrigin.SetPositionAndRotation(Vector3.Lerp(worldPose2.position, worldPose.position, t), Quaternion.Lerp(worldPose2.rotation, worldPose.rotation, t));
					if (m_HasLineVisual)
					{
						m_LineVisual.enabled = true;
					}
					return true;
				}
				if (m_HasLineVisual && !m_TeleportRay)
				{
					m_LineVisual.enabled = false;
				}
			}
			return false;
		}

		internal void RestoreVisuals()
		{
			if (m_RestoreVisuals && m_HasLineVisual && !fallback)
			{
				m_LineVisual.enabled = true;
			}
			m_RestoreVisuals = false;
		}
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void GetAssistedVelocityInternal_0000101E_0024PostfixBurstDelegate(in Vector3 source, in Vector3 target, in Vector3 velocity, float gravity, float maxAngle, float requiredSpeed, float maxSpeedPercent, float assistPercent, float epsilon, out Vector3 adjustedVelocity);

	internal static class GetAssistedVelocityInternal_0000101E_0024BurstDirectCall
	{
		private static IntPtr Pointer;

		[BurstDiscard]
		private static void GetFunctionPointerDiscard(ref IntPtr P_0)
		{
			if (Pointer == (IntPtr)0)
			{
				Pointer = BurstCompiler.CompileFunctionPointer<GetAssistedVelocityInternal_0000101E_0024PostfixBurstDelegate>(GetAssistedVelocityInternal).Value;
			}
			P_0 = Pointer;
		}

		private static IntPtr GetFunctionPointer()
		{
			nint result = 0;
			GetFunctionPointerDiscard(ref result);
			return result;
		}

		public unsafe static void Invoke(in Vector3 source, in Vector3 target, in Vector3 velocity, float gravity, float maxAngle, float requiredSpeed, float maxSpeedPercent, float assistPercent, float epsilon, out Vector3 adjustedVelocity)
		{
			if (BurstCompiler.IsEnabled)
			{
				IntPtr functionPointer = GetFunctionPointer();
				if (functionPointer != (IntPtr)0)
				{
					((delegate* unmanaged[Cdecl]<ref Vector3, ref Vector3, ref Vector3, float, float, float, float, float, float, ref Vector3, void>)functionPointer)(ref source, ref target, ref velocity, gravity, maxAngle, requiredSpeed, maxSpeedPercent, assistPercent, epsilon, ref adjustedVelocity);
					return;
				}
			}
			GetAssistedVelocityInternal_0024BurstManaged(in source, in target, in velocity, gravity, maxAngle, requiredSpeed, maxSpeedPercent, assistPercent, epsilon, out adjustedVelocity);
		}
	}

	private const float k_MinAttachDistance = 0.5f;

	private const float k_MinFallbackDivergence = 0f;

	private const float k_MaxFallbackDivergence = 90f;

	private const float k_MinAimAssistRequiredAngle = 0f;

	private const float k_MaxAimAssistRequiredAngle = 90f;

	[SerializeField]
	[Tooltip("Eye data source used as fallback data and to determine if fallback data should be used.")]
	private XRGazeInteractor m_GazeInteractor;

	[SerializeField]
	[Range(0f, 90f)]
	[Tooltip("How far an interactor must point away from the user's view area before eye gaze will be used instead.")]
	private float m_FallbackDivergence = 60f;

	[SerializeField]
	[Tooltip("If the eye reticle should be hidden when all interactors are using their original data.")]
	private bool m_HideCursorWithNoActiveRays = true;

	[SerializeField]
	[Tooltip("Interactors that can fall back to gaze data.")]
	private List<InteractorData> m_RayInteractors = new List<InteractorData>();

	[SerializeField]
	[Tooltip("How far projectiles can aim outside of eye gaze and still be considered for aim assist.")]
	[Range(0f, 90f)]
	private float m_AimAssistRequiredAngle = 30f;

	[SerializeField]
	[Tooltip("How fast a projectile must be moving to be considered for aim assist.")]
	private float m_AimAssistRequiredSpeed = 0.25f;

	[SerializeField]
	[Tooltip("How much of the corrected aim velocity to use, as a percentage.")]
	[Range(0f, 1f)]
	private float m_AimAssistPercent = 0.8f;

	[SerializeField]
	[Tooltip("How much additional speed a projectile can receive from aim assistance, as a percentage.")]
	private float m_AimAssistMaxSpeedPercent = 10f;

	private InteractorData m_SelectingInteractorData;

	private XRInteractorReticleVisual m_GazeReticleVisual;

	private bool m_HasGazeReticleVisual;

	public XRGazeInteractor gazeInteractor
	{
		get
		{
			return m_GazeInteractor;
		}
		set
		{
			m_GazeInteractor = value;
		}
	}

	public float fallbackDivergence
	{
		get
		{
			return m_FallbackDivergence;
		}
		set
		{
			m_FallbackDivergence = Mathf.Clamp(value, 0f, 90f);
		}
	}

	public bool hideCursorWithNoActiveRays
	{
		get
		{
			return m_HideCursorWithNoActiveRays;
		}
		set
		{
			m_HideCursorWithNoActiveRays = value;
		}
	}

	public List<InteractorData> rayInteractors
	{
		get
		{
			return m_RayInteractors;
		}
		set
		{
			m_RayInteractors = value;
		}
	}

	public float aimAssistRequiredAngle
	{
		get
		{
			return m_AimAssistRequiredAngle;
		}
		set
		{
			m_AimAssistRequiredAngle = Mathf.Clamp(value, 0f, 90f);
		}
	}

	public float aimAssistRequiredSpeed
	{
		get
		{
			return m_AimAssistRequiredSpeed;
		}
		set
		{
			m_AimAssistRequiredSpeed = value;
		}
	}

	public float aimAssistPercent
	{
		get
		{
			return m_AimAssistPercent;
		}
		set
		{
			m_AimAssistPercent = Mathf.Clamp01(value);
		}
	}

	public float aimAssistMaxSpeedPercent
	{
		get
		{
			return m_AimAssistMaxSpeedPercent;
		}
		set
		{
			m_AimAssistMaxSpeedPercent = value;
		}
	}

	private void Initialize()
	{
		if (m_GazeInteractor != null)
		{
			m_HasGazeReticleVisual = m_GazeInteractor.TryGetComponent<XRInteractorReticleVisual>(out m_GazeReticleVisual);
			for (int i = 0; i < m_RayInteractors.Count; i++)
			{
				m_RayInteractors[i].Initialize();
			}
		}
		else
		{
			Debug.LogError($"Gaze Interactor not set or missing on {this}. Disabling this XR Gaze Assistance component.", this);
			base.enabled = false;
		}
	}

	protected void OnEnable()
	{
		Application.onBeforeRender += OnBeforeRender;
	}

	protected void OnDisable()
	{
		Application.onBeforeRender -= OnBeforeRender;
	}

	protected void Start()
	{
		Initialize();
	}

	protected void Update()
	{
		Transform rayOriginTransform = m_GazeInteractor.rayOriginTransform;
		for (int i = 0; i < m_RayInteractors.Count; i++)
		{
			InteractorData interactorData = m_RayInteractors[i];
			interactorData.RestoreVisuals();
			interactorData.UpdateFallbackRayOrigin(rayOriginTransform);
		}
	}

	protected void LateUpdate()
	{
		if (!m_GazeInteractor.isActiveAndEnabled)
		{
			return;
		}
		Transform rayOriginTransform = m_GazeInteractor.rayOriginTransform;
		if (m_SelectingInteractorData != null && !m_SelectingInteractorData.UpdateFallbackState(rayOriginTransform, m_FallbackDivergence, selectionLocked: false))
		{
			m_SelectingInteractorData = null;
		}
		bool flag = false;
		for (int i = 0; i < m_RayInteractors.Count; i++)
		{
			InteractorData interactorData = m_RayInteractors[i];
			if (interactorData.fallback)
			{
				flag = true;
			}
			if (interactorData != m_SelectingInteractorData && interactorData.UpdateFallbackState(rayOriginTransform, m_FallbackDivergence, m_SelectingInteractorData != null))
			{
				m_SelectingInteractorData = interactorData;
			}
		}
		if (m_HideCursorWithNoActiveRays && m_HasGazeReticleVisual)
		{
			bool flag2 = m_SelectingInteractorData != null;
			m_GazeReticleVisual.enabled = flag && !flag2;
		}
	}

	[BeforeRenderOrder(95)]
	private void OnBeforeRender()
	{
		for (int i = 0; i < m_RayInteractors.Count; i++)
		{
			m_RayInteractors[i].UpdateLineVisualOrigin();
		}
	}

	public Vector3 GetAssistedVelocity(in Vector3 source, in Vector3 velocity, float gravity)
	{
		Vector3 target = m_GazeInteractor.rayEndPoint;
		GetAssistedVelocityInternal(in source, in target, in velocity, gravity, m_AimAssistRequiredAngle, m_AimAssistRequiredSpeed, m_AimAssistMaxSpeedPercent, m_AimAssistPercent, Mathf.Epsilon, out var adjustedVelocity);
		return adjustedVelocity;
	}

	public Vector3 GetAssistedVelocity(in Vector3 source, in Vector3 velocity, float gravity, float maxAngle)
	{
		Vector3 target = m_GazeInteractor.rayEndPoint;
		GetAssistedVelocityInternal(in source, in target, in velocity, gravity, maxAngle, m_AimAssistRequiredSpeed, m_AimAssistMaxSpeedPercent, m_AimAssistPercent, Mathf.Epsilon, out var adjustedVelocity);
		return adjustedVelocity;
	}

	[BurstCompile]
	[MonoPInvokeCallback(typeof(UnityEngine_002EXR_002EInteraction_002EToolkit_002EGaze_002EGetAssistedVelocityInternal_0000101E_0024PostfixBurstDelegate))]
	private static void GetAssistedVelocityInternal(in Vector3 source, in Vector3 target, in Vector3 velocity, float gravity, float maxAngle, float requiredSpeed, float maxSpeedPercent, float assistPercent, float epsilon, out Vector3 adjustedVelocity)
	{
		GetAssistedVelocityInternal_0000101E_0024BurstDirectCall.Invoke(in source, in target, in velocity, gravity, maxAngle, requiredSpeed, maxSpeedPercent, assistPercent, epsilon, out adjustedVelocity);
	}

	Vector3 IXRAimAssist.GetAssistedVelocity(in Vector3 source, in Vector3 velocity, float gravity)
	{
		return GetAssistedVelocity(in source, in velocity, gravity);
	}

	Vector3 IXRAimAssist.GetAssistedVelocity(in Vector3 source, in Vector3 velocity, float gravity, float maxAngle)
	{
		return GetAssistedVelocity(in source, in velocity, gravity, maxAngle);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[BurstCompile]
	internal static void GetAssistedVelocityInternal_0024BurstManaged(in Vector3 source, in Vector3 target, in Vector3 velocity, float gravity, float maxAngle, float requiredSpeed, float maxSpeedPercent, float assistPercent, float epsilon, out Vector3 adjustedVelocity)
	{
		Vector3 vector = target - source;
		float num = math.length(velocity);
		float3 @float = math.normalize(velocity);
		float3 float2 = math.normalize(vector);
		if (Vector3.Angle(@float, float2) > maxAngle)
		{
			adjustedVelocity = velocity;
			return;
		}
		if (gravity < epsilon)
		{
			adjustedVelocity = float2 * num;
			return;
		}
		if (num < requiredSpeed)
		{
			adjustedVelocity = velocity;
			return;
		}
		float3 x = vector;
		x.y = 0f;
		float num2 = math.length(x);
		if (num2 < epsilon)
		{
			adjustedVelocity = velocity;
			return;
		}
		float2 float3 = new float2(math.sqrt(0.5f * gravity * (num2 * num2) / (num2 - vector.y)), 0f);
		float3.y = float3.x;
		float2 float4 = new float2(float3.x, 0f);
		if (vector.y < 0f)
		{
			float4.x = math.sqrt(0.5f * gravity * num2 * num2 / (0f - vector.y));
		}
		else
		{
			float4.x *= 2f;
			float4.y = float4.x * (vector.y + 0.5f * gravity * (num2 / float4.x) * (num2 / float4.x)) / num2;
		}
		float num3 = math.length(float3);
		float num4 = math.length(float4);
		float num5 = math.abs(num3 - num);
		float num6 = math.abs(num4 - num);
		if (velocity.y <= 0f)
		{
			num6 *= 0.25f;
		}
		float2 x2 = ((num5 < num6) ? float3 : float4);
		x2 = math.normalize(x2) * math.min(math.length(x2), maxSpeedPercent * num);
		float3 x3 = math.normalize(x) * x2.x;
		x3.y = x2.y;
		adjustedVelocity = Vector3.Slerp(@float, math.normalize(x3), assistPercent) * math.lerp(num, math.length(x3), assistPercent);
	}
}
