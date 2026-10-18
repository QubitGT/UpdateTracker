using System;
using UnityEngine;

namespace Oculus.Interaction.Input;

public class ControllerAnimatedHand : MonoBehaviour, IDeltaTimeConsumer
{
	public enum AllowThumbUp
	{
		Always,
		GripRequired,
		TriggerAndGripRequired
	}

	[SerializeField]
	[Interface(typeof(IController), new Type[] { })]
	private UnityEngine.Object _controller;

	private IController Controller;

	[SerializeField]
	private Animator _animator;

	[SerializeField]
	[Tooltip("Indicates the input needed in order to perform a thumbs-up when the fist is closed")]
	private AllowThumbUp _allowThumbUp = AllowThumbUp.TriggerAndGripRequired;

	[Header("Animation Speed")]
	[SerializeField]
	[Tooltip("Speed of the index flex animation")]
	private float _animFlexGain = 35f;

	[SerializeField]
	[Tooltip("Speed of the pinch animation")]
	private float _animPinchGain = 35f;

	[SerializeField]
	[Tooltip("Speed of the point, slide and thumbs up animation")]
	private float _animPointAndThumbsUpGain = 20f;

	private const string ANIM_LAYER_NAME_POINT = "Point Layer";

	private const string ANIM_LAYER_NAME_THUMB = "Thumb Layer";

	private const string ANIM_PARAM_NAME_FLEX = "Flex";

	private const string ANIM_PARAM_NAME_PINCH = "Pinch";

	private const string ANIM_PARAM_NAME_INDEX_SLIDE = "IndexSlide";

	private const float TRIGGER_MAX = 0.95f;

	private int _animLayerIndexThumb = -1;

	private int _animLayerIndexPoint = -1;

	private int _animParamIndexFlex = Animator.StringToHash("Flex");

	private int _animParamPinch = Animator.StringToHash("Pinch");

	private int _animParamIndexSlide = Animator.StringToHash("IndexSlide");

	private bool _isGivingThumbsUp;

	private float _pointBlend;

	private float _slideBlend;

	private float _thumbsUpBlend;

	private float _pointTarget;

	private float _slideTarget;

	private float _animFlex;

	private float _animPinch;

	private bool _started;

	private Func<float> _deltaTimeProvider = () => Time.deltaTime;

	public AllowThumbUp AllowThumbUpMode
	{
		get
		{
			return _allowThumbUp;
		}
		set
		{
			_allowThumbUp = value;
		}
	}

	public float AnimFlexGain
	{
		get
		{
			return _animFlexGain;
		}
		set
		{
			_animFlexGain = value;
		}
	}

	public float AnimPinchGain
	{
		get
		{
			return _animPinchGain;
		}
		set
		{
			_animPinchGain = value;
		}
	}

	public float AnimPointAndThumbsUpGain
	{
		get
		{
			return _animPointAndThumbsUpGain;
		}
		set
		{
			_animPointAndThumbsUpGain = value;
		}
	}

	public Func<float> DeltaTimeProvider { get; set; } = () => Time.deltaTime;


	public void SetDeltaTimeProvider(Func<float> deltaTimeProvider)
	{
		_deltaTimeProvider = deltaTimeProvider;
	}

	protected virtual void Awake()
	{
		Controller = _controller as IController;
	}

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		_animLayerIndexPoint = _animator.GetLayerIndex("Point Layer");
		_animLayerIndexThumb = _animator.GetLayerIndex("Thumb Layer");
		this.EndStart(ref _started);
	}

	protected virtual void Update()
	{
		UpdateCapTouchStates();
		_pointBlend = Mathf.Lerp(_pointBlend, _pointTarget, _animPointAndThumbsUpGain * _deltaTimeProvider());
		_slideBlend = Mathf.Lerp(_slideBlend, _slideTarget, _animPointAndThumbsUpGain * _deltaTimeProvider());
		_thumbsUpBlend = Mathf.Lerp(_thumbsUpBlend, _isGivingThumbsUp ? 1 : 0, _animPointAndThumbsUpGain * _deltaTimeProvider());
		UpdateAnimStates();
	}

	private void UpdateCapTouchStates()
	{
		float trigger = Controller.ControllerInput.Trigger;
		float grip = Controller.ControllerInput.Grip;
		bool flag = (Controller.ControllerInput.ButtonUsageMask & (ControllerButtonUsage.PrimaryButton | ControllerButtonUsage.PrimaryTouch | ControllerButtonUsage.SecondaryButton | ControllerButtonUsage.SecondaryTouch | ControllerButtonUsage.Thumbrest)) != 0;
		bool flag2 = _allowThumbUp == AllowThumbUp.Always || (_allowThumbUp == AllowThumbUp.GripRequired && grip >= 0.95f) || (_allowThumbUp == AllowThumbUp.TriggerAndGripRequired && grip >= 0.95f && trigger >= 0.95f);
		_isGivingThumbsUp = flag2 && !flag;
		_pointTarget = 1f - trigger;
		_slideTarget = 0f;
	}

	private void UpdateAnimStates()
	{
		float grip = Controller.ControllerInput.Grip;
		_animFlex = Mathf.Lerp(_animFlex, grip, _animFlexGain * DeltaTimeProvider());
		_animator.SetFloat(_animParamIndexFlex, _animFlex);
		float trigger = Controller.ControllerInput.Trigger;
		_animPinch = Mathf.Lerp(_animPinch, trigger, _animPinchGain * DeltaTimeProvider());
		_animator.SetFloat(_animParamPinch, _animPinch);
		_animator.SetLayerWeight(_animLayerIndexPoint, _pointBlend);
		_animator.SetFloat(_animParamIndexSlide, _slideBlend);
		_animator.SetLayerWeight(_animLayerIndexThumb, _thumbsUpBlend);
	}

	public void InjectAllControllerAnimatedHand(IController controller, Animator animator)
	{
		InjectController(controller);
		InjectAnimator(animator);
	}

	public void InjectController(IController controller)
	{
		_controller = controller as UnityEngine.Object;
		Controller = controller;
	}

	public void InjectAnimator(Animator animator)
	{
		_animator = animator;
	}
}
