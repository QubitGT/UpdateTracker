using System.Collections.Generic;
using System.Threading.Tasks;
using Liv.Lck.Rendering;
using UnityEngine;

namespace Liv.Lck.GorillaTag;

public class LckFrameController : MonoBehaviour
{
	[SerializeField]
	private LckCompositionProfile _compositionProfile;

	[SerializeField]
	private GTLckController _qckController;

	[SerializeField]
	private GtScreenButton _overlayButton;

	[Header("Configuration")]
	[Tooltip("The name of the Overlay Frame Layer as defined in the Composition Profile.")]
	[SerializeField]
	private string _overlayLayerName = "Overlay Frame";

	[SerializeField]
	private List<ScheduledDuration> _defaultOnSchedules;

	[Header("Runtime Texture URLs (Optional)")]
	[SerializeField]
	private string _horizontalOverlayUrl;

	[SerializeField]
	private string _verticalOverlayUrl;

	private LckOverlayFrameLayer _overlayLayer;

	private async void Start()
	{
		if (_compositionProfile == null)
		{
			Debug.LogError("No CompositionProfile assigned!", this);
			base.enabled = false;
			return;
		}
		_overlayLayer = _compositionProfile.GetLayer<LckOverlayFrameLayer>(_overlayLayerName);
		if (_overlayLayer == null)
		{
			Debug.LogError("No OverlayFrameLayer named '" + _overlayLayerName + "' found in the profile " + _compositionProfile.name + "!", this);
			base.enabled = false;
			return;
		}
		await LoadAndApplyTextures();
		bool overlayEnabled = false;
		foreach (ScheduledDuration defaultOnSchedule in _defaultOnSchedules)
		{
			if (defaultOnSchedule.IsActive())
			{
				overlayEnabled = true;
				break;
			}
		}
		SetOverlayEnabled(overlayEnabled);
		_overlayButton.onTapStarted.AddListener(ToggleOverlay);
		_qckController.OnHorizontalModeChanged += OnHorizontalModeChanged;
		OnHorizontalModeChanged(isHorizontal: true);
	}

	private void OnDisable()
	{
		if (_overlayButton != null)
		{
			_overlayButton.onTapStarted.RemoveListener(ToggleOverlay);
		}
		if (_qckController != null)
		{
			_qckController.OnHorizontalModeChanged -= OnHorizontalModeChanged;
		}
	}

	private void OnHorizontalModeChanged(bool isHorizontal)
	{
		_compositionProfile.SetOrientation(isHorizontal);
	}

	private void SetOverlayEnabled(bool value)
	{
		_compositionProfile.SetLayerActive(_overlayLayerName, value);
		_qckController.SetOverlayEnabled(value);
		_overlayButton.IsActive = value;
	}

	private void ToggleOverlay()
	{
		SetOverlayEnabled(!_overlayLayer.IsActive);
	}

	private async Task LoadAndApplyTextures()
	{
		if (!string.IsNullOrEmpty(_horizontalOverlayUrl) && !string.IsNullOrEmpty(_verticalOverlayUrl))
		{
			Task<Texture> horizontalTask = LoadTexture(_horizontalOverlayUrl);
			Task<Texture> verticalTask = LoadTexture(_verticalOverlayUrl);
			await Task.WhenAll<Texture>(horizontalTask, verticalTask);
			if (horizontalTask.Result != null)
			{
				_overlayLayer.HorizontalTexture = horizontalTask.Result;
			}
			if (verticalTask.Result != null)
			{
				_overlayLayer.VerticalTexture = verticalTask.Result;
			}
		}
	}

	private async Task<Texture> LoadTexture(string url)
	{
		return null;
	}
}
