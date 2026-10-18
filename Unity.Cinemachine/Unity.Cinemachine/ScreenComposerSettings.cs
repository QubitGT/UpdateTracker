using System;
using UnityEngine;

namespace Unity.Cinemachine;

[Serializable]
public struct ScreenComposerSettings
{
	[Serializable]
	public struct DeadZoneSettings
	{
		public bool Enabled;

		[Tooltip("The camera will not adjust if the target is within this range of the screen position.  Full screen size is 1.")]
		[DelayedVector]
		public Vector2 Size;
	}

	[Serializable]
	public struct HardLimitSettings
	{
		public bool Enabled;

		[Tooltip("The target will not be allowed to be outside this region. When the target is within this region, the camera will gradually adjust to re-align towards the desired position, depending on the damping speed.  Full screen size is 1")]
		[DelayedVector]
		public Vector2 Size;

		[Tooltip("A zero Offset means that the hard limits will be centered around the target screen position.  A nonzero Offset will uncenter the hard limits relative to the target screen position.")]
		[DelayedVector]
		public Vector2 Offset;
	}

	[Tooltip("Screen position for target. The camera will adjust to position the tracked object here.  0 is screen center, and +0.5 or -0.5 is screen edge")]
	[DelayedVector]
	public Vector2 ScreenPosition;

	[Tooltip("The camera will not adjust if the target is within this range of the screen position")]
	[FoldoutWithEnabledButton("Enabled")]
	public DeadZoneSettings DeadZone;

	[Tooltip("The target will not be allowed to be outside this region. When the target is within this region, the camera will gradually adjust to re-align towards the desired position, depending on the damping speed")]
	[FoldoutWithEnabledButton("Enabled")]
	public HardLimitSettings HardLimits;

	public Vector2 EffectiveDeadZoneSize
	{
		get
		{
			if (!DeadZone.Enabled)
			{
				return Vector2.zero;
			}
			return DeadZone.Size;
		}
	}

	public Vector2 EffectiveHardLimitSize
	{
		get
		{
			if (!HardLimits.Enabled)
			{
				return new Vector2(3f, 3f);
			}
			return HardLimits.Size;
		}
	}

	public Rect DeadZoneRect
	{
		get
		{
			Vector2 effectiveDeadZoneSize = EffectiveDeadZoneSize;
			return new Rect(ScreenPosition - effectiveDeadZoneSize * 0.5f + new Vector2(0.5f, 0.5f), effectiveDeadZoneSize);
		}
		set
		{
			Vector2 size = EffectiveDeadZoneSize;
			if (DeadZone.Enabled)
			{
				size = new Vector2(Mathf.Clamp(value.width, 0f, 2f), Mathf.Clamp(value.height, 0f, 2f));
				DeadZone.Size = size;
			}
			ScreenPosition = new Vector2(Mathf.Clamp(value.x - 0.5f + size.x * 0.5f, -1.5f, 1.5f), Mathf.Clamp(value.y - 0.5f + size.y * 0.5f, -1.5f, 1.5f));
			HardLimits.Size = new Vector2(Mathf.Clamp(HardLimits.Size.x, size.x, 3f), Mathf.Clamp(HardLimits.Size.y, size.y, 3f));
		}
	}

	public Rect HardLimitsRect
	{
		get
		{
			if (!HardLimits.Enabled)
			{
				return new Rect(-EffectiveHardLimitSize * 0.5f, EffectiveHardLimitSize);
			}
			Rect result = new Rect(ScreenPosition - HardLimits.Size * 0.5f + new Vector2(0.5f, 0.5f), HardLimits.Size);
			Vector2 effectiveDeadZoneSize = EffectiveDeadZoneSize;
			result.position += new Vector2(HardLimits.Offset.x * 0.5f * (HardLimits.Size.x - effectiveDeadZoneSize.x), HardLimits.Offset.y * 0.5f * (HardLimits.Size.y - effectiveDeadZoneSize.y));
			return result;
		}
		set
		{
			HardLimits.Size.x = Mathf.Clamp(value.width, 0f, 6f);
			HardLimits.Size.y = Mathf.Clamp(value.height, 0f, 6f);
			DeadZone.Size.x = Mathf.Min(DeadZone.Size.x, HardLimits.Size.x);
			DeadZone.Size.y = Mathf.Min(DeadZone.Size.y, HardLimits.Size.y);
		}
	}

	public static ScreenComposerSettings Default
	{
		get
		{
			ScreenComposerSettings result = default(ScreenComposerSettings);
			result.DeadZone = new DeadZoneSettings
			{
				Enabled = false,
				Size = new Vector2(0.2f, 0.2f)
			};
			result.HardLimits = new HardLimitSettings
			{
				Enabled = false,
				Size = new Vector2(0.8f, 0.8f)
			};
			return result;
		}
	}

	public void Validate()
	{
		ScreenPosition.x = Mathf.Clamp(ScreenPosition.x, -1.5f, 1.5f);
		ScreenPosition.y = Mathf.Clamp(ScreenPosition.y, -1.5f, 1.5f);
		DeadZone.Size.x = Mathf.Clamp(DeadZone.Size.x, 0f, 2f);
		DeadZone.Size.y = Mathf.Clamp(DeadZone.Size.y, 0f, 2f);
		HardLimits.Size = new Vector2(Mathf.Clamp(HardLimits.Size.x, DeadZone.Size.x, 3f), Mathf.Clamp(HardLimits.Size.y, DeadZone.Size.y, 3f));
		HardLimits.Offset.x = Mathf.Clamp(HardLimits.Offset.x, -1f, 1f);
		HardLimits.Offset.y = Mathf.Clamp(HardLimits.Offset.y, -1f, 1f);
	}

	public static ScreenComposerSettings Lerp(in ScreenComposerSettings a, in ScreenComposerSettings b, float t)
	{
		ScreenComposerSettings result = default(ScreenComposerSettings);
		result.ScreenPosition = Vector2.Lerp(a.ScreenPosition, b.ScreenPosition, t);
		result.DeadZone = new DeadZoneSettings
		{
			Enabled = (a.DeadZone.Enabled || b.DeadZone.Enabled),
			Size = Vector2.Lerp(a.EffectiveDeadZoneSize, b.EffectiveDeadZoneSize, t)
		};
		result.HardLimits = new HardLimitSettings
		{
			Enabled = (a.HardLimits.Enabled || b.HardLimits.Enabled),
			Size = Vector2.Lerp(a.EffectiveHardLimitSize, b.EffectiveHardLimitSize, t),
			Offset = Vector2.Lerp(a.HardLimits.Offset, b.HardLimits.Offset, t)
		};
		return result;
	}

	public static bool Approximately(in ScreenComposerSettings a, in ScreenComposerSettings b)
	{
		if (Mathf.Approximately(a.ScreenPosition.x, b.ScreenPosition.x) && Mathf.Approximately(a.ScreenPosition.y, b.ScreenPosition.y) && Mathf.Approximately(a.EffectiveDeadZoneSize.x, b.EffectiveDeadZoneSize.x) && Mathf.Approximately(a.EffectiveDeadZoneSize.y, b.EffectiveDeadZoneSize.y) && Mathf.Approximately(a.EffectiveHardLimitSize.x, b.EffectiveHardLimitSize.x) && Mathf.Approximately(a.EffectiveHardLimitSize.y, b.EffectiveHardLimitSize.y) && Mathf.Approximately(a.HardLimits.Offset.x, b.HardLimits.Offset.x))
		{
			return Mathf.Approximately(a.HardLimits.Offset.y, b.HardLimits.Offset.y);
		}
		return false;
	}
}
