using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Cinemachine;

public struct CameraState
{
	public enum BlendHints
	{
		Nothing = 0,
		SphericalPositionBlend = 1,
		CylindricalPositionBlend = 2,
		ScreenSpaceAimWhenTargetsDiffer = 4,
		InheritPosition = 8,
		IgnoreLookAtTarget = 16,
		FreezeWhenBlendingOut = 32,
		NoPosition = 65536,
		NoOrientation = 131072,
		NoTransform = 196608,
		NoLens = 262144
	}

	public struct CustomBlendableItems
	{
		public struct Item
		{
			public UnityEngine.Object Custom;

			public float Weight;
		}

		internal Item m_Item0;

		internal Item m_Item1;

		internal Item m_Item2;

		internal Item m_Item3;

		internal List<Item> m_Overflow;

		internal int NumItems;
	}

	public LensSettings Lens;

	public Vector3 ReferenceUp;

	public Vector3 ReferenceLookAt;

	public static Vector3 kNoPoint = new Vector3(float.NaN, float.NaN, float.NaN);

	public Vector3 RawPosition;

	public Quaternion RawOrientation;

	public Quaternion RotationDampingBypass;

	public float ShotQuality;

	public Vector3 PositionCorrection;

	public Quaternion OrientationCorrection;

	public BlendHints BlendHint;

	internal CustomBlendableItems CustomBlendables;

	public static CameraState Default
	{
		get
		{
			CameraState result = default(CameraState);
			result.Lens = LensSettings.Default;
			result.ReferenceUp = Vector3.up;
			result.ReferenceLookAt = kNoPoint;
			result.RawPosition = Vector3.zero;
			result.RawOrientation = Quaternion.identity;
			result.ShotQuality = 1f;
			result.PositionCorrection = Vector3.zero;
			result.OrientationCorrection = Quaternion.identity;
			result.RotationDampingBypass = Quaternion.identity;
			result.BlendHint = BlendHints.Nothing;
			return result;
		}
	}

	public void AddCustomBlendable(CustomBlendableItems.Item b)
	{
		int num = this.FindCustomBlendable(b.Custom);
		if (num >= 0)
		{
			b.Weight += this.GetCustomBlendable(num).Weight;
		}
		else
		{
			num = CustomBlendables.NumItems++;
		}
		switch (num)
		{
		case 0:
			CustomBlendables.m_Item0 = b;
			return;
		case 1:
			CustomBlendables.m_Item1 = b;
			return;
		case 2:
			CustomBlendables.m_Item2 = b;
			return;
		case 3:
			CustomBlendables.m_Item3 = b;
			return;
		}
		num -= 4;
		ref List<CustomBlendableItems.Item> overflow = ref CustomBlendables.m_Overflow;
		if (overflow == null)
		{
			overflow = new List<CustomBlendableItems.Item>();
		}
		if (num < CustomBlendables.m_Overflow.Count)
		{
			CustomBlendables.m_Overflow[num] = b;
		}
		else
		{
			CustomBlendables.m_Overflow.Add(b);
		}
	}

	public static CameraState Lerp(in CameraState stateA, in CameraState stateB, float t)
	{
		t = Mathf.Clamp01(t);
		float t2 = t;
		CameraState cameraState = default(CameraState);
		if ((stateA.BlendHint & stateB.BlendHint & BlendHints.NoPosition) != 0)
		{
			cameraState.BlendHint |= BlendHints.NoPosition;
		}
		if ((stateA.BlendHint & stateB.BlendHint & BlendHints.NoOrientation) != 0)
		{
			cameraState.BlendHint |= BlendHints.NoOrientation;
		}
		if ((stateA.BlendHint & stateB.BlendHint & BlendHints.NoLens) != 0)
		{
			cameraState.BlendHint |= BlendHints.NoLens;
		}
		if (((stateA.BlendHint | stateB.BlendHint) & BlendHints.SphericalPositionBlend) != 0)
		{
			cameraState.BlendHint |= BlendHints.SphericalPositionBlend;
		}
		if (((stateA.BlendHint | stateB.BlendHint) & BlendHints.CylindricalPositionBlend) != 0)
		{
			cameraState.BlendHint |= BlendHints.CylindricalPositionBlend;
		}
		if (((stateA.BlendHint | stateB.BlendHint) & BlendHints.FreezeWhenBlendingOut) != 0)
		{
			cameraState.BlendHint |= BlendHints.FreezeWhenBlendingOut;
		}
		if (((stateA.BlendHint | stateB.BlendHint) & BlendHints.NoLens) == 0)
		{
			cameraState.Lens = LensSettings.Lerp(stateA.Lens, stateB.Lens, t);
		}
		else if ((stateA.BlendHint & stateB.BlendHint & BlendHints.NoLens) == 0)
		{
			if ((stateA.BlendHint & BlendHints.NoLens) != 0)
			{
				cameraState.Lens = stateB.Lens;
			}
			else
			{
				cameraState.Lens = stateA.Lens;
			}
		}
		cameraState.ReferenceUp = Vector3.Slerp(stateA.ReferenceUp, stateB.ReferenceUp, t);
		cameraState.ShotQuality = Mathf.Lerp(stateA.ShotQuality, stateB.ShotQuality, t);
		cameraState.PositionCorrection = ApplyPosBlendHint(stateA.PositionCorrection, stateA.BlendHint, stateB.PositionCorrection, stateB.BlendHint, cameraState.PositionCorrection, Vector3.Lerp(stateA.PositionCorrection, stateB.PositionCorrection, t));
		cameraState.OrientationCorrection = ApplyRotBlendHint(stateA.OrientationCorrection, stateA.BlendHint, stateB.OrientationCorrection, stateB.BlendHint, cameraState.OrientationCorrection, Quaternion.Slerp(stateA.OrientationCorrection, stateB.OrientationCorrection, t));
		if (!stateA.HasLookAt() || !stateB.HasLookAt())
		{
			cameraState.ReferenceLookAt = kNoPoint;
		}
		else
		{
			float fieldOfView = stateA.Lens.FieldOfView;
			float fieldOfView2 = stateB.Lens.FieldOfView;
			if (((stateA.BlendHint | stateB.BlendHint) & BlendHints.NoLens) == 0 && !cameraState.Lens.Orthographic && !Mathf.Approximately(fieldOfView, fieldOfView2))
			{
				LensSettings lens = cameraState.Lens;
				lens.FieldOfView = InterpolateFOV(fieldOfView, fieldOfView2, Mathf.Max((stateA.ReferenceLookAt - stateA.GetCorrectedPosition()).magnitude, stateA.Lens.NearClipPlane), Mathf.Max((stateB.ReferenceLookAt - stateB.GetCorrectedPosition()).magnitude, stateB.Lens.NearClipPlane), t);
				cameraState.Lens = lens;
				t2 = Mathf.Abs((lens.FieldOfView - fieldOfView) / (fieldOfView2 - fieldOfView));
			}
			cameraState.ReferenceLookAt = Vector3.Lerp(stateA.ReferenceLookAt, stateB.ReferenceLookAt, t2);
		}
		cameraState.RawPosition = ApplyPosBlendHint(stateA.RawPosition, stateA.BlendHint, stateB.RawPosition, stateB.BlendHint, cameraState.RawPosition, InterpolatePosition(stateA.RawPosition, stateA.ReferenceLookAt, stateB.RawPosition, stateB.ReferenceLookAt, t, cameraState.BlendHint, cameraState.ReferenceUp));
		if (cameraState.HasLookAt() && ((stateA.BlendHint | stateB.BlendHint) & BlendHints.ScreenSpaceAimWhenTargetsDiffer) != 0)
		{
			cameraState.ReferenceLookAt = cameraState.RawPosition + Vector3.Slerp(stateA.ReferenceLookAt - cameraState.RawPosition, stateB.ReferenceLookAt - cameraState.RawPosition, t2);
		}
		Quaternion blended = cameraState.RawOrientation;
		if (((stateA.BlendHint | stateB.BlendHint) & BlendHints.NoOrientation) == 0)
		{
			Vector3 vector = Vector3.zero;
			if (cameraState.HasLookAt() && Quaternion.Angle(stateA.RawOrientation, stateB.RawOrientation) > 0.0001f)
			{
				vector = cameraState.ReferenceLookAt - cameraState.GetCorrectedPosition();
			}
			if (vector.AlmostZero() || ((stateA.BlendHint | stateB.BlendHint) & BlendHints.IgnoreLookAtTarget) != 0)
			{
				blended = Quaternion.Slerp(stateA.RawOrientation, stateB.RawOrientation, t);
			}
			else
			{
				Vector3 vector2 = cameraState.ReferenceUp;
				vector.Normalize();
				if (Vector3.Cross(vector, vector2).AlmostZero())
				{
					blended = Quaternion.Slerp(stateA.RawOrientation, stateB.RawOrientation, t);
					vector2 = blended * Vector3.up;
				}
				blended = Quaternion.LookRotation(vector, vector2);
				Vector2 a = -stateA.RawOrientation.GetCameraRotationToTarget(stateA.ReferenceLookAt - stateA.GetCorrectedPosition(), vector2);
				Vector2 b = -stateB.RawOrientation.GetCameraRotationToTarget(stateB.ReferenceLookAt - stateB.GetCorrectedPosition(), vector2);
				blended = blended.ApplyCameraRotation(Vector2.Lerp(a, b, t2), vector2);
			}
		}
		cameraState.RawOrientation = ApplyRotBlendHint(stateA.RawOrientation, stateA.BlendHint, stateB.RawOrientation, stateB.BlendHint, cameraState.RawOrientation, blended);
		for (int i = 0; i < stateA.CustomBlendables.NumItems; i++)
		{
			CustomBlendableItems.Item customBlendable = stateA.GetCustomBlendable(i);
			customBlendable.Weight *= 1f - t;
			if (customBlendable.Weight > 0f)
			{
				cameraState.AddCustomBlendable(customBlendable);
			}
		}
		for (int j = 0; j < stateB.CustomBlendables.NumItems; j++)
		{
			CustomBlendableItems.Item customBlendable2 = stateB.GetCustomBlendable(j);
			customBlendable2.Weight *= t;
			if (customBlendable2.Weight > 0f)
			{
				cameraState.AddCustomBlendable(customBlendable2);
			}
		}
		return cameraState;
	}

	private static float InterpolateFOV(float fovA, float fovB, float dA, float dB, float t)
	{
		float a = dA * 2f * Mathf.Tan(fovA * (MathF.PI / 180f) / 2f);
		float b = dB * 2f * Mathf.Tan(fovB * (MathF.PI / 180f) / 2f);
		float num = Mathf.Lerp(a, b, t);
		float value = 179f;
		float num2 = Mathf.Lerp(dA, dB, t);
		if (num2 > 0.0001f)
		{
			value = 2f * Mathf.Atan(num / (2f * num2)) * 57.29578f;
		}
		return Mathf.Clamp(value, Mathf.Min(fovA, fovB), Mathf.Max(fovA, fovB));
	}

	private static Vector3 ApplyPosBlendHint(Vector3 posA, BlendHints hintA, Vector3 posB, BlendHints hintB, Vector3 original, Vector3 blended)
	{
		if (((hintA | hintB) & BlendHints.NoPosition) == 0)
		{
			return blended;
		}
		if ((hintA & hintB & BlendHints.NoPosition) != 0)
		{
			return original;
		}
		if ((hintA & BlendHints.NoPosition) != 0)
		{
			return posB;
		}
		return posA;
	}

	private static Quaternion ApplyRotBlendHint(Quaternion rotA, BlendHints hintA, Quaternion rotB, BlendHints hintB, Quaternion original, Quaternion blended)
	{
		if (((hintA | hintB) & BlendHints.NoOrientation) == 0)
		{
			return blended;
		}
		if ((hintA & hintB & BlendHints.NoOrientation) != 0)
		{
			return original;
		}
		if ((hintA & BlendHints.NoOrientation) != 0)
		{
			return rotB;
		}
		return rotA;
	}

	private static Vector3 InterpolatePosition(Vector3 posA, Vector3 pivotA, Vector3 posB, Vector3 pivotB, float t, BlendHints blendHint, Vector3 up)
	{
		if (pivotA == pivotA && pivotB == pivotB)
		{
			if ((blendHint & BlendHints.CylindricalPositionBlend) != 0)
			{
				Vector3 vector = Vector3.ProjectOnPlane(posA - pivotA, up);
				Vector3 vector2 = Vector3.ProjectOnPlane(posB - pivotB, up);
				Vector3 vector3 = Vector3.Slerp(vector, vector2, t);
				posA = posA - vector + vector3;
				posB = posB - vector2 + vector3;
			}
			else if ((blendHint & BlendHints.SphericalPositionBlend) != 0)
			{
				Vector3 vector4 = Vector3.Slerp(posA - pivotA, posB - pivotB, t);
				posA = pivotA + vector4;
				posB = pivotB + vector4;
			}
		}
		return Vector3.Lerp(posA, posB, t);
	}
}
