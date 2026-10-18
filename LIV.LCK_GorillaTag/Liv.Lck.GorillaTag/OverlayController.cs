using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Liv.Lck.GorillaTag;

public class OverlayController : MonoBehaviour
{
	[Header("Dependencies")]
	[SerializeField]
	private GTLckController _qckController;

	[SerializeField]
	private GtScreenButton _overlayButton;

	[SerializeField]
	private List<ScheduledDuration> _defaultOnScheduls;

	[Header("Overlay Settings")]
	[SerializeField]
	private Texture _horizontalOverlayTexture;

	[SerializeField]
	private Texture _verticalOverlayTexture;

	private bool _isOverlayEnabled;

	private void OnEnable()
	{
		_overlayButton.onTapStarted.AddListener(ToggleOverlay);
	}

	private void OnDisable()
	{
		_overlayButton.onTapStarted.RemoveListener(ToggleOverlay);
	}

	private void Start()
	{
		if (_horizontalOverlayTexture == null || _verticalOverlayTexture == null)
		{
			Debug.Log("Disabling overlay because of failure to load textures");
			return;
		}
		foreach (ScheduledDuration defaultOnSchedul in _defaultOnScheduls)
		{
			if (defaultOnSchedul.IsActive())
			{
				_isOverlayEnabled = true;
				break;
			}
		}
		SetOverlayEnabled(_isOverlayEnabled);
	}

	private void OnHorizontalModeChanged(bool value)
	{
	}

	private void SetOverlayEnabled(bool value)
	{
	}

	private void ToggleOverlay()
	{
		_isOverlayEnabled = !_isOverlayEnabled;
		SetOverlayEnabled(_isOverlayEnabled);
	}

	private async Task<Texture> LoadTexture(string url)
	{
		using UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
		UnityWebRequestAsyncOperation asyncOperation = request.SendWebRequest();
		while (!asyncOperation.isDone)
		{
			await Task.Yield();
		}
		if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
		{
			Debug.Log("Failure to load overlay texture from " + url + ": " + request.error);
			return null;
		}
		return DownloadHandlerTexture.GetContent(request);
	}
}
