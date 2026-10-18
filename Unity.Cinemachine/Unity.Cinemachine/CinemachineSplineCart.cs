using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Splines;

namespace Unity.Cinemachine;

[ExecuteAlways]
[DisallowMultipleComponent]
[AddComponentMenu("Cinemachine/Helpers/Cinemachine Spline Cart")]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineSplineCart.html")]
public class CinemachineSplineCart : MonoBehaviour, ISplineReferencer
{
	public enum UpdateMethods
	{
		Update,
		FixedUpdate,
		LateUpdate
	}

	[SerializeField]
	[FormerlySerializedAs("SplineSettings")]
	private SplineSettings m_SplineSettings = new SplineSettings
	{
		Units = PathIndexUnit.Normalized
	};

	[Tooltip("When to move the cart, if Speed is non-zero")]
	public UpdateMethods UpdateMethod;

	[FoldoutWithEnabledButton("Enabled")]
	[Tooltip("Controls how automatic dollying occurs.  A tracking target may be necessary to use this feature.")]
	public SplineAutoDolly AutomaticDolly;

	[Tooltip("Used only by Automatic Dolly settings that require it")]
	public Transform TrackingTarget;

	private CinemachineSplineRoll.RollCache m_RollCache;

	[SerializeField]
	[HideInInspector]
	[NoSaveDuringPlay]
	[FormerlySerializedAs("SplinePosition")]
	private float m_LegacyPosition = -1f;

	[SerializeField]
	[HideInInspector]
	[NoSaveDuringPlay]
	[FormerlySerializedAs("PositionUnits")]
	private PathIndexUnit m_LegacyUnits;

	[SerializeField]
	[HideInInspector]
	[NoSaveDuringPlay]
	[FormerlySerializedAs("Spline")]
	private SplineContainer m_LegacySpline;

	public ref SplineSettings SplineSettings => ref m_SplineSettings;

	public SplineContainer Spline
	{
		get
		{
			return m_SplineSettings.Spline;
		}
		set
		{
			m_SplineSettings.Spline = value;
		}
	}

	public float SplinePosition
	{
		get
		{
			return m_SplineSettings.Position;
		}
		set
		{
			m_SplineSettings.Position = value;
		}
	}

	public PathIndexUnit PositionUnits
	{
		get
		{
			return m_SplineSettings.Units;
		}
		set
		{
			m_SplineSettings.ChangeUnitPreservePosition(value);
		}
	}

	private void PerformLegacyUpgrade()
	{
		if (m_LegacyPosition != -1f)
		{
			m_SplineSettings.Position = m_LegacyPosition;
			m_SplineSettings.Units = m_LegacyUnits;
			m_LegacyPosition = -1f;
			m_LegacyUnits = PathIndexUnit.Distance;
		}
		if (m_LegacySpline != null)
		{
			m_SplineSettings.Spline = m_LegacySpline;
			m_LegacySpline = null;
		}
	}

	private void OnValidate()
	{
		PerformLegacyUpgrade();
		AutomaticDolly.Method?.Validate();
	}

	private void Reset()
	{
		m_SplineSettings = new SplineSettings
		{
			Units = PathIndexUnit.Normalized
		};
		UpdateMethod = UpdateMethods.Update;
		AutomaticDolly.Method = null;
		TrackingTarget = null;
	}

	private void OnEnable()
	{
		m_RollCache.Refresh(this);
		AutomaticDolly.Method?.Reset();
	}

	private void OnDisable()
	{
		SplineSettings.InvalidateCache();
	}

	private void FixedUpdate()
	{
		if (UpdateMethod == UpdateMethods.FixedUpdate)
		{
			UpdateCartPosition();
		}
	}

	private void Update()
	{
		if (!Application.isPlaying)
		{
			SetCartPosition(SplinePosition);
		}
		else if (UpdateMethod == UpdateMethods.Update)
		{
			UpdateCartPosition();
		}
	}

	private void LateUpdate()
	{
		if (!Application.isPlaying)
		{
			SetCartPosition(SplinePosition);
		}
		else if (UpdateMethod == UpdateMethods.LateUpdate)
		{
			UpdateCartPosition();
		}
	}

	private void UpdateCartPosition()
	{
		if (AutomaticDolly.Enabled && AutomaticDolly.Method != null)
		{
			SplinePosition = AutomaticDolly.Method.GetSplinePosition(this, TrackingTarget, Spline, SplinePosition, PositionUnits, Time.deltaTime);
		}
		SetCartPosition(SplinePosition);
	}

	private void SetCartPosition(float distanceAlongPath)
	{
		CachedScaledSpline cachedSpline = m_SplineSettings.GetCachedSpline();
		if (cachedSpline != null)
		{
			Spline spline = Spline.Splines[0];
			SplinePosition = cachedSpline.StandardizePosition(distanceAlongPath, PositionUnits, out var _);
			float tNormalized = spline.ConvertIndexUnit(SplinePosition, PositionUnits, PathIndexUnit.Normalized);
			cachedSpline.EvaluateSplineWithRoll(Spline.transform, tNormalized, m_RollCache.GetSplineRoll(this), out var position, out var rotation);
			base.transform.ConservativeSetPositionAndRotation(position, rotation);
		}
	}
}
