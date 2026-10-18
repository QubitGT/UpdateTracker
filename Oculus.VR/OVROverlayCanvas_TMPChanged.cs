using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OVROverlayCanvas_TMPChanged : MonoBehaviour
{
	public OVROverlayCanvas TargetCanvas;

	private static Dictionary<GameObject, OVROverlayCanvas> _textObjectToCanvas = new Dictionary<GameObject, OVROverlayCanvas>();

	[RuntimeInitializeOnLoadMethod]
	private static void OnLoad()
	{
		TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
	}

	private static void OnTextChanged(Object target)
	{
		if (target is TMP_Text tMP_Text && _textObjectToCanvas.TryGetValue(tMP_Text.gameObject, out var value))
		{
			value.SetFrameDirty();
		}
	}

	protected void OnEnable()
	{
		_textObjectToCanvas.Add(base.gameObject, TargetCanvas);
	}

	protected void OnDisable()
	{
		_textObjectToCanvas.Remove(base.gameObject);
	}
}
