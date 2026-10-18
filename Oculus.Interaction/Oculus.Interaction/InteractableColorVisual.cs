using System;
using System.Collections;
using UnityEngine;

namespace Oculus.Interaction;

public class InteractableColorVisual : MonoBehaviour
{
	[Serializable]
	public class ColorState
	{
		public Color Color = Color.white;

		public AnimationCurve ColorCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

		public float ColorTime = 0.1f;
	}

	[SerializeField]
	[Interface(typeof(IInteractableView), new Type[] { })]
	private UnityEngine.Object _interactableView;

	[SerializeField]
	private MaterialPropertyBlockEditor _editor;

	[SerializeField]
	private string _colorShaderPropertyName = "_Color";

	[SerializeField]
	private ColorState _normalColorState = new ColorState
	{
		Color = Color.white
	};

	[SerializeField]
	private ColorState _hoverColorState = new ColorState
	{
		Color = Color.blue
	};

	[SerializeField]
	private ColorState _selectColorState = new ColorState
	{
		Color = Color.green
	};

	[SerializeField]
	private ColorState _disabledColorState = new ColorState
	{
		Color = Color.grey
	};

	private Color _currentColor;

	private ColorState _target;

	private int _colorShaderID;

	private Coroutine _routine;

	private static readonly YieldInstruction _waiter = new WaitForEndOfFrame();

	protected bool _started;

	private IInteractableView InteractableView { get; set; }

	protected virtual void Awake()
	{
		InteractableView = _interactableView as IInteractableView;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		_colorShaderID = Shader.PropertyToID(_colorShaderPropertyName);
		UpdateVisual();
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		if (_started)
		{
			UpdateVisual();
			InteractableView.WhenStateChanged += UpdateVisualState;
		}
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			InteractableView.WhenStateChanged -= UpdateVisualState;
		}
	}

	private void UpdateVisualState(InteractableStateChangeArgs args)
	{
		UpdateVisual();
	}

	protected virtual void UpdateVisual()
	{
		ColorState colorState = ColorForState(InteractableView.State);
		if (colorState != _target)
		{
			_target = colorState;
			CancelRoutine();
			_routine = StartCoroutine(ChangeColor(colorState));
		}
	}

	private ColorState ColorForState(InteractableState state)
	{
		return state switch
		{
			InteractableState.Select => _selectColorState, 
			InteractableState.Hover => _hoverColorState, 
			InteractableState.Normal => _normalColorState, 
			InteractableState.Disabled => _disabledColorState, 
			_ => _normalColorState, 
		};
	}

	private IEnumerator ChangeColor(ColorState targetState)
	{
		Color startColor = _currentColor;
		float timer = 0f;
		do
		{
			timer += Time.deltaTime;
			float time = Mathf.Clamp01(timer / targetState.ColorTime);
			float t = targetState.ColorCurve.Evaluate(time);
			SetColor(Color.Lerp(startColor, targetState.Color, t));
			yield return _waiter;
		}
		while (timer <= targetState.ColorTime);
	}

	private void SetColor(Color color)
	{
		_currentColor = color;
		_editor.MaterialPropertyBlock.SetColor(_colorShaderID, color);
	}

	private void CancelRoutine()
	{
		if (_routine != null)
		{
			StopCoroutine(_routine);
			_routine = null;
		}
	}

	public void InjectAllInteractableColorVisual(IInteractableView interactableView, MaterialPropertyBlockEditor editor)
	{
		InjectInteractableView(interactableView);
		InjectMaterialPropertyBlockEditor(editor);
	}

	public void InjectInteractableView(IInteractableView interactableview)
	{
		_interactableView = interactableview as UnityEngine.Object;
		InteractableView = interactableview;
	}

	public void InjectMaterialPropertyBlockEditor(MaterialPropertyBlockEditor editor)
	{
		_editor = editor;
	}

	public void InjectOptionalColorShaderPropertyName(string colorShaderPropertyName)
	{
		_colorShaderPropertyName = colorShaderPropertyName;
	}

	public void InjectOptionalNormalColorState(ColorState normalColorState)
	{
		_normalColorState = normalColorState;
	}

	public void InjectOptionalHoverColorState(ColorState hoverColorState)
	{
		_hoverColorState = hoverColorState;
	}

	public void InjectOptionalSelectColorState(ColorState selectColorState)
	{
		_selectColorState = selectColorState;
	}
}
