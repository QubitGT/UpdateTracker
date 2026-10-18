using UnityEngine;

namespace Oculus.Interaction.Locomotion;

public class TurnArrowVisuals : MonoBehaviour
{
	[Header("Visual renderers")]
	[Tooltip("Renderer for the Left arrow cone")]
	[SerializeField]
	private Renderer _leftArrow;

	[Tooltip("Renderer for the Right arrow cone")]
	[SerializeField]
	private Renderer _rightArrow;

	[Tooltip("TubeRenderer that will draw the rail of the left arrow")]
	[SerializeField]
	private TubeRenderer _leftRail;

	[Tooltip("TubeRenderer that will draw the rail of the right arrow")]
	[SerializeField]
	private TubeRenderer _rightRail;

	[Tooltip("TubeRenderer that will draw the trail of the right arrow")]
	[SerializeField]
	private TubeRenderer _leftTrail;

	[Tooltip("TubeRenderer that will draw the trail of the right arrow")]
	[SerializeField]
	private TubeRenderer _rightTrail;

	[Tooltip("Material block for the left arrow items so they can be controller")]
	[SerializeField]
	private MaterialPropertyBlockEditor _leftMaterialBlock;

	[Tooltip("Material block for the right arrow items so they can be controller")]
	[SerializeField]
	private MaterialPropertyBlockEditor _rightMaterialBlock;

	[Header("Visual parameters")]
	[Tooltip("Radius of the circle in which the arrows are circunscribed")]
	[SerializeField]
	private float _radius = 0.07f;

	[Tooltip("Gap, in degrees, left between the arrows")]
	[SerializeField]
	private float _margin = 2f;

	[Tooltip("Length, in degrees, of the trail of the arrows")]
	[SerializeField]
	private float _trailLength = 15f;

	[Tooltip("Max angle, in degrees, the arrows can follow when highlighted")]
	[SerializeField]
	private float _maxAngle = 45f;

	[Tooltip("Length of the transparent gap in the rail left by the arrow")]
	[SerializeField]
	private float _railGap = 0.005f;

	[Tooltip("Length, in degrees, that the arrows can grow when highlighted")]
	[SerializeField]
	private float _squeezeLength = 5f;

	[Header("Visual controllers")]
	[Tooltip("Color of the arrow when not active")]
	[SerializeField]
	private Color _disabledColor = new Color(1f, 1f, 1f, 0.2f);

	[Tooltip("Color of the arrow when active")]
	[SerializeField]
	private Color _enabledColor = new Color(1f, 1f, 1f, 0.6f);

	[Tooltip("Color of the arrow when highlighted")]
	[SerializeField]
	private Color _highligtedColor = new Color(1f, 1f, 1f, 1f);

	[Tooltip("If true, the current active arrow will")]
	[SerializeField]
	private bool _highLight;

	[Tooltip("This value controls wich arrow is active, <0 for the left and >0 for the right")]
	[SerializeField]
	private float _value;

	[Tooltip("Indicates how much the active arrow must grow")]
	[SerializeField]
	private float _progress;

	[Tooltip("Indicates wheter the active arrow should follow the rail")]
	[SerializeField]
	private bool _followArrow;

	private const float _degreesPerSegment = 1f;

	private static readonly Quaternion _rotationCorrectionLeft = Quaternion.Euler(0f, -90f, 0f);

	private static readonly int _colorShaderPropertyID = Shader.PropertyToID("_Color");

	protected bool _started;

	public float Radius => _radius;

	public float Margin => _margin;

	public float TrailLength => _trailLength;

	public float MaxAngle => _maxAngle;

	public float RailGap => _railGap;

	public float SqueezeLength => _squeezeLength;

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

	public bool HighLight
	{
		get
		{
			return _highLight;
		}
		set
		{
			_highLight = value;
		}
	}

	public float Value
	{
		get
		{
			return _value;
		}
		set
		{
			_value = value;
		}
	}

	public float Progress
	{
		get
		{
			return _progress;
		}
		set
		{
			_progress = value;
		}
	}

	public bool FollowArrow
	{
		get
		{
			return _followArrow;
		}
		set
		{
			_followArrow = value;
		}
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		InitializeVisuals();
		DisableVisuals();
		this.EndStart(ref _started);
	}

	protected virtual void OnDisable()
	{
		if (_started)
		{
			DisableVisuals();
		}
	}

	public void DisableVisuals()
	{
		_leftTrail.enabled = false;
		_rightTrail.enabled = false;
		_leftArrow.enabled = false;
		_rightArrow.enabled = false;
		_leftRail.enabled = false;
		_rightRail.enabled = false;
	}

	private void InitializeVisuals()
	{
		TubePoint[] points = InitializeSegment(new Vector2(_margin, _maxAngle + _squeezeLength));
		_leftTrail.RenderTube(points);
		_rightTrail.RenderTube(points);
		TubePoint[] points2 = InitializeSegment(new Vector2(_margin, _maxAngle));
		_leftRail.RenderTube(points2);
		_rightRail.RenderTube(points2);
	}

	public void UpdateVisual()
	{
		UpdateArrows(Value);
		UpdateColors(HighLight, Value);
	}

	private void UpdateArrows(float value)
	{
		float a = Mathf.Lerp(0f, _maxAngle, Mathf.Abs(value));
		bool flag = value < 0f;
		bool followArrow = _followArrow;
		float num = Mathf.Lerp(0f, _squeezeLength, _progress);
		_leftTrail.enabled = true;
		_rightTrail.enabled = true;
		_leftArrow.enabled = true;
		_rightArrow.enabled = true;
		_rightRail.enabled = !flag;
		_leftRail.enabled = flag;
		a = Mathf.Max(a, _trailLength);
		UpdateArrowPosition(flag ? _trailLength : (a + num), _rightArrow.transform);
		RotateTrail((followArrow && !flag) ? (a - _trailLength) : 0f, _rightTrail);
		UpdateTrail(flag ? _trailLength : ((followArrow ? _trailLength : a) + num), _rightTrail);
		UpdateArrowPosition((!flag) ? (0f - _trailLength) : (0f - a - num), _leftArrow.transform);
		RotateTrail((followArrow && flag) ? (0f - a + _trailLength) : 0f, _leftTrail);
		UpdateTrail((!flag) ? _trailLength : ((followArrow ? _trailLength : a) + num), _leftTrail);
		UpdateRail(a, num, flag ? _leftRail : _rightRail);
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

	private void UpdateRail(float angle, float extra, TubeRenderer rail)
	{
		float totalLength = rail.TotalLength;
		float num = (angle - _trailLength - _margin) / _maxAngle;
		float num2 = (_maxAngle - angle - extra - _margin) / _maxAngle;
		float num3 = _railGap + rail.Feather;
		rail.StartFadeThresold = totalLength * num - num3;
		rail.EndFadeThresold = totalLength * num2 - num3;
		rail.InvertThreshold = true;
		rail.RedrawFadeThresholds();
	}

	private void UpdateColors(bool isSelection, float value)
	{
		_leftMaterialBlock.MaterialPropertyBlock.SetColor(_colorShaderPropertyID, (!(value < 0f)) ? _disabledColor : (isSelection ? _highligtedColor : _enabledColor));
		_rightMaterialBlock.MaterialPropertyBlock.SetColor(_colorShaderPropertyID, (!(value > 0f)) ? _disabledColor : (isSelection ? _highligtedColor : _enabledColor));
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

	public void InjectAllTurnArrowVisuals(Renderer leftArrow, Renderer rightArrow, TubeRenderer leftRail, TubeRenderer rightRail, TubeRenderer leftTrail, TubeRenderer rightTrail, MaterialPropertyBlockEditor leftMaterialBlock, MaterialPropertyBlockEditor rightMaterialBlock, float radius, float margin, float trailLength, float maxAngle, float railGap, float squeezeLength)
	{
		InjectLeftArrow(leftArrow);
		InjectRightArrow(rightArrow);
		InjectLeftRail(leftRail);
		InjectRightRail(rightRail);
		InjectLeftTrail(leftTrail);
		InjectRightTrail(rightTrail);
		InjectLeftMaterialBlock(leftMaterialBlock);
		InjectRightMaterialBlock(rightMaterialBlock);
		InjectRadius(radius);
		InjectMargin(margin);
		InjectTrailLength(trailLength);
		InjectMaxAngle(maxAngle);
		InjectRailGap(railGap);
		InjectSqueezeLength(squeezeLength);
	}

	public void InjectLeftArrow(Renderer leftArrow)
	{
		_leftArrow = leftArrow;
	}

	public void InjectRightArrow(Renderer rightArrow)
	{
		_rightArrow = rightArrow;
	}

	public void InjectLeftRail(TubeRenderer leftRail)
	{
		_leftRail = leftRail;
	}

	public void InjectRightRail(TubeRenderer rightRail)
	{
		_rightRail = rightRail;
	}

	public void InjectLeftTrail(TubeRenderer leftTrail)
	{
		_leftTrail = leftTrail;
	}

	public void InjectRightTrail(TubeRenderer rightTrail)
	{
		_rightTrail = rightTrail;
	}

	public void InjectLeftMaterialBlock(MaterialPropertyBlockEditor leftMaterialBlock)
	{
		_leftMaterialBlock = leftMaterialBlock;
	}

	public void InjectRightMaterialBlock(MaterialPropertyBlockEditor rightMaterialBlock)
	{
		_rightMaterialBlock = rightMaterialBlock;
	}

	public void InjectRadius(float radius)
	{
		_radius = radius;
	}

	public void InjectMargin(float margin)
	{
		_margin = margin;
	}

	public void InjectTrailLength(float trailLength)
	{
		_trailLength = trailLength;
	}

	public void InjectMaxAngle(float maxAngle)
	{
		_maxAngle = maxAngle;
	}

	public void InjectRailGap(float railGap)
	{
		_railGap = railGap;
	}

	public void InjectSqueezeLength(float squeezeLength)
	{
		_squeezeLength = squeezeLength;
	}
}
