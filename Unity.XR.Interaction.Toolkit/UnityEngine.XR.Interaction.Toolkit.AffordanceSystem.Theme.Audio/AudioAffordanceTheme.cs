using System;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme.Audio;

[Serializable]
[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public class AudioAffordanceTheme
{
	[SerializeField]
	private List<AudioAffordanceThemeData> m_List;

	protected AudioAffordanceTheme()
	{
		m_List = new List<AudioAffordanceThemeData>
		{
			new AudioAffordanceThemeData
			{
				stateName = "disabled"
			},
			new AudioAffordanceThemeData
			{
				stateName = "idle"
			},
			new AudioAffordanceThemeData
			{
				stateName = "hovered"
			},
			new AudioAffordanceThemeData
			{
				stateName = "hoveredPriority"
			},
			new AudioAffordanceThemeData
			{
				stateName = "selected"
			},
			new AudioAffordanceThemeData
			{
				stateName = "activated"
			},
			new AudioAffordanceThemeData
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
			AudioAffordanceThemeData audioAffordanceThemeData = ((count >= 2) ? m_List[1] : new AudioAffordanceThemeData
			{
				stateName = "idle"
			});
			while (num-- > 0)
			{
				AudioAffordanceThemeData item = new AudioAffordanceThemeData
				{
					stateName = audioAffordanceThemeData.stateName,
					stateEntered = audioAffordanceThemeData.stateEntered,
					stateExited = audioAffordanceThemeData.stateExited
				};
				m_List.Add(item);
				byte b = (byte)(m_List.Count - 1);
				string nameForIndex = AffordanceStateShortcuts.GetNameForIndex(b);
				m_List[b].stateName = nameForIndex;
				Debug.LogWarning($"Found missing state {b} \"{nameForIndex}\" in your affordance theme. Adding missing state with idle state data.");
			}
		}
	}

	public AudioAffordanceThemeData GetAffordanceThemeDataForIndex(byte stateIndex)
	{
		if (stateIndex >= m_List.Count)
		{
			return null;
		}
		return m_List[stateIndex];
	}
}
