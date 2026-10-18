using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UnityEngine.XR.Interaction.Toolkit.UI;

public class TrackedDeviceEventData : PointerEventData
{
	public List<Vector3> rayPoints { get; set; }

	public int rayHitIndex { get; set; }

	public LayerMask layerMask { get; set; }

	public IUIInteractor interactor
	{
		get
		{
			XRUIInputModule xRUIInputModule = base.currentInputModule as XRUIInputModule;
			if (xRUIInputModule != null)
			{
				return xRUIInputModule.GetInteractor(base.pointerId);
			}
			return null;
		}
	}

	internal Vector3 pressWorldPosition { get; set; }

	public TrackedDeviceEventData(EventSystem eventSystem)
		: base(eventSystem)
	{
	}
}
