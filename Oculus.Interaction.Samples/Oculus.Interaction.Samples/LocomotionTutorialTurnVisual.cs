using UnityEngine;

namespace Oculus.Interaction.Samples;

public class LocomotionTutorialTurnVisual : MonoBehaviour
{
	[SerializeField]
	[Range(-1f, 1f)]
	private float _value;

	[SerializeField]
	[Range(0f, 1f)]
	private float _progress;

	[Header("Visual renderers")]
	[SerializeField]
	private Renderer _leftArrow;

	[SerializeField]
	private Renderer _rightArrow;

	[SerializeField]
	private TubeRenderer _leftTrail;

	[SerializeField]
	private TubeRenderer _rightTrail;

	[SerializeField]
	private MaterialPropertyBlockEditor _leftMaterialBlock;

	[SerializeField]
	private MaterialPropertyBlockEditor _rightMaterialBlock;

	[Header("Visual parameters")]
	[SerializeField]
	private float _verticalOffset = 0.02f;

	[SerializeField]
	private float _radius = 0.07f;

	[SerializeField]
	private float _margin = 2f;

	[SerializeField]
	private float _trailLength = 15f;

	[SerializeField]
	private float _maxAngle = 45f;

	[SerializeField]
	private float _railGap = 0.005f;

	[SerializeField]
	private float _squeezeLength = 5f;

	[SerializeField]
	private Color _disabledColor = new Color(1f, 1f, 1f, 0.2f);

	[SerializeField]
	private Color _enabledColor = new Color(1f, 1f, 1f, 0.6f);

	[SerializeField]
	private Color _highligtedColor = new Color(1f, 1f, 1f, 1f);

	private const float _degreesPerSegment = 1f;

	private static readonly Quaternion _rotationCorrectionLeft = Quaternion.Euler(0f, -90f, 0f);

	private static readonly int _colorShaderPropertyID = Shader.PropertyToID("_Color");

	protected bool _started;

	public float VerticalOffset
	{
		get
		{
			return _verticalOffset;
		}
		set
		{
			_verticalOffset = value;
		}
	}

	public Color DisabledColor
	{
		get
		{
			return _disabledColor;
		}
		set
		{
			_disabledColor = value;
		}
	}

	public Color EnabledColor
	{
		get
		{
			return _enabledColor;
		}
		set
		{
			_enabledColor = value;
		}
	}

	public Color HighligtedColor
	{
		get
		{
			return _highligtedColor;
		}
		set
		{
			_highligtedColor = value;
		}
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		InitializeVisuals();
		this.EndStart(ref _started);
	}

	protected virtual void OnEnable()
	{
		_leftTrail.enabled = true;
		_rightTrail.enabled = true;
		_leftArrow.enabled = true;
		_rightArrow.enabled = true;
	}

	protected virtual void OnDisable()
	{
		_leftTrail.enabled = false;
		_rightTrail.enabled = false;
		_leftArrow.enabled = false;
		_rightArrow.enabled = false;
	}

	protected virtual void Update()
	{
		UpdateArrows();
		UpdateColors();
	}

	private void InitializeVisuals()
	{
		TubePoint[] points = InitializeSegment(new Vector2(_margin, _maxAngle + _squeezeLength));
		_leftTrail.RenderTube(points);
		_rightTrail.RenderTube(points);
	}

	private void UpdateArrows()
	{
		float value = _value;
		float a = Mathf.Lerp(0f, _maxAngle, Mathf.Abs(value));
		bool flag = value < 0f;
		bool flag2 = value > 0f;
		bool flag3 = false;
		float num = Mathf.Lerp(0f, _squeezeLength, _progress);
		a = Mathf.Max(a, _trailLength);
		UpdateArrowPosition(flag2 ? (a + num) : _trailLength, _rightArrow.transform);
		RotateTrail((flag3 && flag2) ? (a - _trailLength) : 0f, _rightTrail);
		UpdateTrail(flag2 ? ((flag3 ? _trailLength : a) + num) : _trailLength, _rightTrail);
		UpdateArrowPosition(flag ? (0f - a - num) : (0f - _trailLength), _leftArrow.transform);
		RotateTrail((flag3 && flag) ? (0f - a + _trailLength) : 0f, _leftTrail);
		UpdateTrail(flag ? ((flag3 ? _trailLength : a) + num) : _trailLength, _leftTrail);
	}

	private void UpdateArrowPosition(float angle, Transform arrow)
	{
		Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.up);
		arrow.localPosition = quaternion * Vector3.forward * _radius;
		arrow.localRotation = quaternion * _rotationCorrectionLeft;
	}

	private void RotateTrail(float angle, TubeRenderer trail)
	{
		trail.transform.localRotation = Quaternion.AngleAxis(angle, Vector3.up);
	}

	private void UpdateTrail(float angle, TubeRenderer trail)
	{
		float num = _maxAngle + _squeezeLength;
		float totalLength = trail.TotalLength;
		float num2 = -100f;
		float num3 = (num - angle - _margin) / num;
		trail.StartFadeThresold = totalLength * num2;
		trail.EndFadeThresold = totalLength * num3;
		trail.InvertThreshold = false;
		trail.RedrawFadeThresholds();
	}

	private void UpdateColors()
	{
		bool num = Mathf.Abs(_progress) >= 1f;
		bool flag = _value < 0f;
		bool flag2 = _value > 0f;
		Color color = (num ? _highligtedColor : _enabledColor);
		_leftMaterialBlock.MaterialPropertyBlock.SetColor(_colorShaderPropertyID, flag ? color : _disabledColor);
		_rightMaterialBlock.MaterialPropertyBlock.SetColor(_colorShaderPropertyID, flag2 ? color : _disabledColor);
		_leftMaterialBlock.UpdateMaterialPropertyBlock();
		_rightMaterialBlock.UpdateMaterialPropertyBlock();
	}

	private TubePoint[] InitializeSegment(Vector2 minMax)
	{
		float x = minMax.x;
		int num = Mathf.RoundToInt(Mathf.Repeat(minMax.y - x, 360f) / 1f);
		TubePoint[] array = new TubePoint[num];
		float num2 = 1f / (float)num;
		for (int i = 0; i < num; i++)
		{
			Quaternion quaternion = Quaternion.AngleAxis((float)(-i) * 1f - x, Vector3.up);
			array[i] = new TubePoint
			{
				position = quaternion * Vector3.forward * _radius,
				rotation = quaternion * _rotationCorrectionLeft,
				relativeLength = (float)i * num2
			};
		}
		return array;
	}
}
