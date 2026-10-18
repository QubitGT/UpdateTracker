using System.Collections.Generic;
using UnityEngine;

namespace Liv.Lck.GorillaTag;

public class GtSettingsSectionGroup : MonoBehaviour
{
	[SerializeField]
	private List<SettingsSectionController> _sections;

	public void EvaluateMode(CameraMode mode)
	{
		foreach (SettingsSectionController section in _sections)
		{
			section.EvaluateMode(mode);
		}
	}
}
