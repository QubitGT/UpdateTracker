using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace Liv.Lck;

public class LckHideObjectFromCamera : MonoBehaviour
{
	[SerializeField]
	private Camera _targetCamera;

	[SerializeField]
	private string _hiddenLayerName = "HideInRecording";

	private int _hiddenLayer;

	private int _originalLayer;

	private bool _dirty;

	private void OnEnable()
	{
		RenderPipelineManager.beginCameraRendering += BeginCameraRendering;
		RenderPipelineManager.endCameraRendering += EndCameraRendering;
		_hiddenLayer = LayerMask.NameToLayer(_hiddenLayerName);
		_originalLayer = base.gameObject.layer;
		HideCanvases(base.transform);
	}

	private void EndCameraRendering(ScriptableRenderContext arg1, Camera cameraBeingRendered)
	{
		if (!(cameraBeingRendered != _targetCamera) && _dirty)
		{
			FrameCleanup();
		}
	}

	private void HideCanvases(Transform parent)
	{
		foreach (Transform item in parent)
		{
			if ((bool)item.GetComponent<Canvas>())
			{
				item.gameObject.layer = _hiddenLayer;
			}
			if ((bool)item.GetComponent<TextMeshPro>())
			{
				item.gameObject.layer = _hiddenLayer;
			}
			HideCanvases(item);
		}
	}

	private void BeginCameraRendering(ScriptableRenderContext scriptableRenderContext, Camera cameraBeingRendered)
	{
		if (cameraBeingRendered == _targetCamera)
		{
			SetLayerRecursively(base.gameObject, _hiddenLayer);
			_dirty = true;
		}
	}

	private void FrameCleanup()
	{
		if (_dirty)
		{
			SetLayerRecursively(base.gameObject, _originalLayer);
			_dirty = false;
		}
	}

	private void SetLayerRecursively(GameObject obj, int newLayer)
	{
		if (obj == null)
		{
			return;
		}
		if (!obj.GetComponent<Canvas>())
		{
			obj.layer = newLayer;
		}
		foreach (Transform item in obj.transform)
		{
			if (item != null)
			{
				SetLayerRecursively(item.gameObject, newLayer);
			}
		}
	}

	private void OnDisable()
	{
		RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;
		RenderPipelineManager.endCameraRendering += EndCameraRendering;
	}
}
