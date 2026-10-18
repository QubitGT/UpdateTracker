using System;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

public class PanelHoverState : MonoBehaviour
{
	public List<Grabbable> grabbables = new List<Grabbable>();

	private bool hovered;

	public bool Hovered => hovered;

	public event Action<bool> WhenStateChanged = delegate
	{
	};

	private void Update()
	{
		bool flag = hovered;
		hovered = false;
		foreach (Grabbable grabbable in grabbables)
		{
			if (grabbable.PointsCount > 0)
			{
				hovered = true;
				break;
			}
		}
		if (flag != hovered)
		{
			this.WhenStateChanged(hovered);
		}
	}
}
