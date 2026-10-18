using System;
using System.Collections.Generic;
using UnityEngine.Localization.PropertyVariants.TrackedProperties;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UnityEngine.Localization.PropertyVariants.TrackedObjects;

[Serializable]
[DisplayName("Transform", null)]
[CustomTrackedObject(typeof(Transform), false)]
public class TrackedTransform : TrackedObject
{
	private Vector3 m_PositionToApply;

	private Quaternion m_RotationToApply;

	private Vector3 m_ScaleToApply;

	private Dictionary<string, Action<float>> m_PropertyHandlers;

	protected virtual void AddPropertyHandlers(Dictionary<string, Action<float>> handlers)
	{
		handlers["m_LocalPosition.x"] = delegate(float val)
		{
			m_PositionToApply.x = val;
		};
		handlers["m_LocalPosition.y"] = delegate(float val)
		{
			m_PositionToApply.y = val;
		};
		handlers["m_LocalPosition.z"] = delegate(float val)
		{
			m_PositionToApply.z = val;
		};
		handlers["m_LocalRotation.x"] = delegate(float val)
		{
			m_RotationToApply.x = val;
		};
		handlers["m_LocalRotation.y"] = delegate(float val)
		{
			m_RotationToApply.y = val;
		};
		handlers["m_LocalRotation.z"] = delegate(float val)
		{
			m_RotationToApply.z = val;
		};
		handlers["m_LocalRotation.w"] = delegate(float val)
		{
			m_RotationToApply.w = val;
		};
		handlers["m_LocalScale.x"] = delegate(float val)
		{
			m_ScaleToApply.x = val;
		};
		handlers["m_LocalScale.y"] = delegate(float val)
		{
			m_ScaleToApply.y = val;
		};
		handlers["m_LocalScale.z"] = delegate(float val)
		{
			m_ScaleToApply.z = val;
		};
	}

	public override bool CanTrackProperty(string propertyPath)
	{
		if (!propertyPath.StartsWith("m_LocalEulerAnglesHint"))
		{
			return !(propertyPath == "m_RootOrder");
		}
		return false;
	}

	public override AsyncOperationHandle ApplyLocale(Locale variantLocale, Locale defaultLocale)
	{
		Transform transform = (Transform)base.Target;
		if (m_PropertyHandlers == null)
		{
			m_PropertyHandlers = new Dictionary<string, Action<float>>();
			AddPropertyHandlers(m_PropertyHandlers);
		}
		m_PositionToApply = transform.localPosition;
		m_RotationToApply = transform.localRotation;
		m_ScaleToApply = transform.localScale;
		LocaleIdentifier identifier = variantLocale.Identifier;
		LocaleIdentifier fallback = ((defaultLocale != null) ? defaultLocale.Identifier : default(LocaleIdentifier));
		foreach (ITrackedProperty trackedProperty in base.TrackedProperties)
		{
			if (((FloatTrackedProperty)trackedProperty).GetValue(identifier, fallback, out var foundValue) && m_PropertyHandlers.TryGetValue(trackedProperty.PropertyPath, out var value))
			{
				value(foundValue);
			}
		}
		transform.localScale = m_ScaleToApply;
		transform.localPosition = m_PositionToApply;
		transform.localRotation = m_RotationToApply;
		return default(AsyncOperationHandle);
	}
}
