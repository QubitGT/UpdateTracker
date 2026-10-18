using System;
using System.Collections.Generic;
using Unity.XR.CoreUtils.Datums;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme;

[Serializable]
[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public abstract class BaseAffordanceTheme<T> : IEquatable<BaseAffordanceTheme<T>> where T : struct
{
	[SerializeField]
	[Tooltip("Curve used to evaluate the target value of the animation state according to the affordance state's transition amount value.")]
	private AnimationCurveDatumProperty m_StateAnimationCurve = new AnimationCurveDatumProperty(AnimationCurve.EaseInOut(0f, 0f, 1f, 1f));

	[SerializeField]
	[Tooltip("List of affordance states supported by this theme. The entry index is how states are mapped to their theme data.\nDo not re-order entries.")]
	private List<AffordanceThemeData<T>> m_List;

	public AnimationCurve animationCurve => m_StateAnimationCurve.Value;

	protected BaseAffordanceTheme()
	{
		m_List = new List<AffordanceThemeData<T>>
		{
			new AffordanceThemeData<T>
			{
				stateName = "disabled"
			},
			new AffordanceThemeData<T>
			{
				stateName = "idle"
			},
			new AffordanceThemeData<T>
			{
				stateName = "hovered"
			},
			new AffordanceThemeData<T>
			{
				stateName = "hoveredPriority"
			},
			new AffordanceThemeData<T>
			{
				stateName = "selected"
			},
			new AffordanceThemeData<T>
			{
				stateName = "activated"
			},
			new AffordanceThemeData<T>
			{
				stateName = "focused"
			}
		};
	}

	internal void ValidateTheme()
	{
		if (m_List == null)
		{
			return;
		}
		int count = m_List.Count;
		int num = AffordanceStateShortcuts.stateCount - count;
		if (num > 0)
		{
			AffordanceThemeData<T> affordanceThemeData = ((count >= 2) ? m_List[1] : new AffordanceThemeData<T>
			{
				stateName = "idle"
			});
			while (num-- > 0)
			{
				AffordanceThemeData<T> item = new AffordanceThemeData<T>
				{
					stateName = affordanceThemeData.stateName,
					animationStateStartValue = affordanceThemeData.animationStateStartValue,
					animationStateEndValue = affordanceThemeData.animationStateEndValue
				};
				m_List.Add(item);
				byte b = (byte)(m_List.Count - 1);
				string nameForIndex = AffordanceStateShortcuts.GetNameForIndex(b);
				m_List[b].stateName = nameForIndex;
				Debug.LogWarning($"Found missing state {b} \"{nameForIndex}\" in your affordance theme. Adding missing state with idle state data.");
			}
		}
	}

	public AffordanceThemeData<T> GetAffordanceThemeDataForIndex(byte stateIndex)
	{
		if (stateIndex >= m_List.Count)
		{
			return null;
		}
		return m_List[stateIndex];
	}

	public void SetAffordanceThemeDataList(List<AffordanceThemeData<T>> newList)
	{
		m_List.Clear();
		m_List.AddRange(newList);
	}

	public virtual void CopyFrom(BaseAffordanceTheme<T> other)
	{
		m_List = new List<AffordanceThemeData<T>>(other.m_List);
		m_StateAnimationCurve = other.m_StateAnimationCurve;
	}

	public void SetAnimationCurve(AnimationCurve newAnimationCurve)
	{
		m_StateAnimationCurve.Value = newAnimationCurve;
	}

	public bool Equals(BaseAffordanceTheme<T> other)
	{
		if (other == null)
		{
			return false;
		}
		if (this == other)
		{
			return true;
		}
		if (object.Equals(m_StateAnimationCurve, other.m_StateAnimationCurve))
		{
			return object.Equals(m_List, other.m_List);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((BaseAffordanceTheme<T>)obj);
	}

	public override int GetHashCode()
	{
		return (17 * 31 + m_StateAnimationCurve.GetHashCode()) * 31 + m_List.GetHashCode();
	}
}
