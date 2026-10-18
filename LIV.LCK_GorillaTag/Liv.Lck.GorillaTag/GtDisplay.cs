using UnityEngine;

namespace Liv.Lck.GorillaTag;

public class GtDisplay : MonoBehaviour
{
	[Header("Elements")]
	[SerializeField]
	private Transform _meshBodyTransform;

	[SerializeField]
	private RectTransform _canvasTransform;

	private Vector3 _initialMeshBodyPosition;

	private Vector3 _initialMeshBodyScale;

	private Vector3 _initialCanvasPosition;

	private Vector3 _initialCanvasScale;

	private Vector3 _targetMeshBodyPosition;

	private Vector3 _targetMeshBodyScale;

	private Vector3 _targetCanvasPosition;

	private Vector3 _targetCanvasScale;

	private void Awake()
	{
		_initialMeshBodyPosition = _meshBodyTransform.localPosition;
		_initialMeshBodyScale = _meshBodyTransform.localScale;
		Vector3 vector = new Vector3(-0.264f, 0.108f, 0f);
		_targetMeshBodyPosition = _initialMeshBodyPosition + vector;
		_targetMeshBodyScale = new Vector3(_initialMeshBodyScale.x * 1.3461539f, _initialMeshBodyScale.y * 1.3461539f, _initialMeshBodyScale.z);
		_initialCanvasPosition = _canvasTransform.localPosition;
		_initialCanvasScale = _canvasTransform.localScale;
		_targetCanvasPosition = _initialCanvasPosition + vector;
		_targetCanvasScale = new Vector3(_initialCanvasScale.x * 1.3461539f, _initialCanvasScale.y * 1.3461539f, _initialCanvasScale.z);
	}

	public void Maximize()
	{
		_meshBodyTransform.localPosition = _targetMeshBodyPosition;
		_meshBodyTransform.localScale = _targetMeshBodyScale;
		_canvasTransform.localPosition = _targetCanvasPosition;
		_canvasTransform.localScale = _targetCanvasScale;
	}

	public void Minimize()
	{
		_meshBodyTransform.localPosition = _initialMeshBodyPosition;
		_meshBodyTransform.localScale = _initialMeshBodyScale;
		_canvasTransform.localPosition = _initialCanvasPosition;
		_canvasTransform.localScale = _initialCanvasScale;
	}
}
