using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Splines;

namespace Unity.Cinemachine;

[ExecuteAlways]
[SaveDuringPlay]
[CameraPipeline(CinemachineCore.Stage.Aim)]
[AddComponentMenu("Cinemachine/Procedural/Rotation Control/Cinemachine Spline Dolly LookAt Targets")]
[DisallowMultipleComponent]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineSplineDollyLookAtTargets.html")]
public class CinemachineSplineDollyLookAtTargets : CinemachineComponentBase
{
	[Serializable]
	public struct Item
	{
		[Tooltip("The target object to look at.  It may be None, in which case the Offset will specify a point in world space.")]
		public Transform LookAt;

		[Tooltip("The offset (in local coords) from the LookAt target's origin.  If LookAt target is None, this will specify a world-space point.")]
		public Vector3 Offset;

		[Tooltip("Controls how to ease in and out of this data point.  A value of 0 will linearly interpolate between LookAt points, while a value of 1 will slow down and briefly pause the rotation to look at the target.")]
		[Range(0f, 1f)]
		public float Easing;

		public Vector3 WorldLookAt
		{
			readonly get
			{
				if (!(LookAt == null))
				{
					return LookAt.TransformPoint(Offset);
				}
				return Offset;
			}
			set
			{
				Offset = ((LookAt == null) ? value : LookAt.InverseTransformPoint(value));
			}
		}
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	internal struct LerpItem : IInterpolator<Item>
	{
		public Item Interpolate(Item a, Item b, float t)
		{
			float num = t * t;
			float num2 = 1f - t;
			t = 3f * num2 * num2 * t * Mathf.Lerp(0.3333f, 0f, a.Easing) + 3f * num2 * num * Mathf.Lerp(0.6666f, 1f, b.Easing) + t * num;
			Item result = default(Item);
			result.Offset = Vector3.Lerp(a.WorldLookAt, b.WorldLookAt, t);
			return result;
		}
	}

	[Tooltip("LookAt targets for the camera at specific positions on the Spline")]
	public SplineData<Item> Targets = new SplineData<Item>
	{
		DefaultValue = new Item
		{
			Easing = 1f
		}
	};

	public override bool IsValid
	{
		get
		{
			SplineContainer spline;
			CinemachineSplineDolly dolly;
			if (base.enabled && Targets != null)
			{
				return GetGetSplineAndDolly(out spline, out dolly);
			}
			return false;
		}
	}

	public override CinemachineCore.Stage Stage => CinemachineCore.Stage.Aim;

	private void Reset()
	{
		Targets = new SplineData<Item>
		{
			DefaultValue = new Item
			{
				Easing = 1f
			}
		};
	}

	public override void MutateCameraState(ref CameraState state, float deltaTime)
	{
		if (!GetGetSplineAndDolly(out var _, out var dolly))
		{
			return;
		}
		CachedScaledSpline cachedSpline = dolly.SplineSettings.GetCachedSpline();
		Item item = Targets.Evaluate(cachedSpline, dolly.CameraPosition, dolly.PositionUnits, default(LerpItem));
		Vector3 vector = item.WorldLookAt - state.RawPosition;
		if (vector.sqrMagnitude > 0.0001f)
		{
			Vector3 vector2 = state.ReferenceUp;
			if (Vector3.Cross(vector, vector2).sqrMagnitude < 0.0001f)
			{
				vector2 = state.RawOrientation * Vector3.back;
				if (Vector3.Cross(vector, vector2).sqrMagnitude < 0.0001f)
				{
					vector2 = state.RawOrientation * Vector3.left;
				}
			}
			state.RawOrientation = Quaternion.LookRotation(vector, vector2);
		}
		state.ReferenceLookAt = item.Offset;
	}

	internal bool GetGetSplineAndDolly(out SplineContainer spline, out CinemachineSplineDolly dolly)
	{
		dolly = null;
		if (this != null && TryGetComponent<CinemachineSplineDolly>(out dolly))
		{
			spline = dolly.Spline;
			if (spline != null)
			{
				return spline.Spline != null;
			}
			return false;
		}
		spline = null;
		return false;
	}
}
