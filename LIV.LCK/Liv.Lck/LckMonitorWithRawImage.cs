using UnityEngine;
using UnityEngine.UI;

namespace Liv.Lck;

public class LckMonitorWithRawImage : LckMonitor
{
	[SerializeField]
	private RawImage _monitorImage;

	[SerializeField]
	private bool _correctImageSize;

	public override void SetRenderTexture(RenderTexture renderTexture)
	{
		base.SetRenderTexture(renderTexture);
		if (_monitorImage == null)
		{
			LckLog.LogWarning("LckMonitorWithRawImage has no Raw Image assigned.", "SetRenderTexture", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Components\\LckMonitorWithRawImage.cs", 20);
			return;
		}
		_monitorImage.texture = renderTexture;
		_monitorImage.color = Color.white;
		if (_correctImageSize && renderTexture != null)
		{
			_monitorImage.rectTransform.sizeDelta = new Vector2(renderTexture.width, renderTexture.height);
		}
	}

	private void OnDisable()
	{
		LckMediator.UnregisterMonitor(this);
		if (_monitorImage != null)
		{
			_monitorImage.color = Color.black;
			_monitorImage.texture = null;
		}
	}
}
