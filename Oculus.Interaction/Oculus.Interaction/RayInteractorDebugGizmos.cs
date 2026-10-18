using UnityEngine;

namespace Oculus.Interaction;

public class RayInteractorDebugGizmos : MonoBehaviour
{
	[SerializeField]
	private RayInteractor _rayInteractor;

	[SerializeField]
	private float _rayWidth = 0.01f;

	[SerializeField]
	private Color _normalColor = Color.red;

	[SerializeField]
	private Color _hoverColor = Color.blue;

	[SerializeField]
	private Color _selectColor = Color.green;

	public float RayWidth
	{
		get
		{
			return _rayWidth;
		}
		set
		{
			_rayWidth = value;
		}
	}

	public Color NormalColor
	{
		get
		{
			return _normalColor;
		}
		set
		{
			_normalColor = value;
		}
	}

	public Color HoverColor
	{
		get
		{
			return _hoverColor;
		}
		set
		{
			_hoverColor = value;
		}
	}

	public Color SelectColor
	{
		get
		{
			return _selectColor;
		}
		set
		{
			_selectColor = value;
		}
	}

	protected virtual void Start()
	{
	}

	private void LateUpdate()
	{
		if (_rayInteractor.State != InteractorState.Disabled)
		{
			switch (_rayInteractor.State)
			{
			case InteractorState.Normal:
				DebugGizmos.Color = _normalColor;
				break;
			case InteractorState.Hover:
				DebugGizmos.Color = _hoverColor;
				break;
			case InteractorState.Select:
				DebugGizmos.Color = _selectColor;
				break;
			case InteractorState.Disabled:
				return;
			}
			DebugGizmos.LineWidth = _rayWidth;
			DebugGizmos.DrawLine(_rayInteractor.Origin, _rayInteractor.End);
		}
	}

	public void InjectAllRayInteractorDebugGizmos(RayInteractor rayInteractor)
	{
		InjectRayInteractor(rayInteractor);
	}

	public void InjectRayInteractor(RayInteractor rayInteractor)
	{
		_rayInteractor = rayInteractor;
	}
}
