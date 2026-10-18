using System;
using Unity.Mathematics;

namespace UnityEngine.Splines;

[AddComponentMenu("Splines/Spline Animate")]
[ExecuteInEditMode]
public class SplineAnimate : SplineComponent
{
	public enum Method
	{
		Time,
		Speed
	}

	public enum LoopMode
	{
		[InspectorName("Once")]
		Once,
		[InspectorName("Loop Continuous")]
		Loop,
		[InspectorName("Ease In Then Continuous")]
		LoopEaseInOnce,
		[InspectorName("Ping Pong")]
		PingPong
	}

	public enum EasingMode
	{
		[InspectorName("None")]
		None,
		[InspectorName("Ease In Only")]
		EaseIn,
		[InspectorName("Ease Out Only")]
		EaseOut,
		[InspectorName("Ease In-Out")]
		EaseInOut
	}

	public enum AlignmentMode
	{
		[InspectorName("None")]
		None,
		[InspectorName("Spline Element")]
		SplineElement,
		[InspectorName("Spline Object")]
		SplineObject,
		[InspectorName("World Space")]
		World
	}

	[SerializeField]
	[Tooltip("The target spline to follow.")]
	private SplineContainer m_Target;

	[SerializeField]
	[Tooltip("Enable to have the animation start when the GameObject first loads.")]
	private bool m_PlayOnAwake = true;

	[SerializeField]
	[Tooltip("The loop mode that the animation uses. Loop modes cause the animation to repeat after it finishes. The following loop modes are available:.\nOnce - Traverse the spline once and stop at the end.\nLoop Continuous - Traverse the spline continuously without stopping.\nEase In Then Continuous - Traverse the spline repeatedly without stopping. If Ease In easing is enabled, apply easing to the first loop only.\nPing Pong - Traverse the spline continuously without stopping and reverse direction after an end of the spline is reached.\n")]
	private LoopMode m_LoopMode = LoopMode.Loop;

	[SerializeField]
	[Tooltip("The method used to animate the GameObject along the spline.\nTime - The spline is traversed in a given amount of seconds.\nSpeed - The spline is traversed at a given maximum speed.")]
	private Method m_Method;

	[SerializeField]
	[Tooltip("The period of time that it takes for the GameObject to complete its animation along the spline.")]
	private float m_Duration = 1f;

	[SerializeField]
	[Tooltip("The speed in meters/second that the GameObject animates along the spline at.")]
	private float m_MaxSpeed = 10f;

	[SerializeField]
	[Tooltip("The easing mode used when the GameObject animates along the spline.\nNone - Apply no easing to the animation. The animation speed is linear.\nEase In Only - Apply easing to the beginning of animation.\nEase Out Only - Apply easing to the end of animation.\nEase In-Out - Apply easing to the beginning and end of animation.\n")]
	private EasingMode m_EasingMode;

	[SerializeField]
	[Tooltip("The coordinate space that the GameObject's up and forward axes align to.")]
	private AlignmentMode m_AlignmentMode = AlignmentMode.SplineElement;

	[SerializeField]
	[Tooltip("Which axis of the GameObject is treated as the forward axis.")]
	private AlignAxis m_ObjectForwardAxis = AlignAxis.ZAxis;

	[SerializeField]
	[Tooltip("Which axis of the GameObject is treated as the up axis.")]
	private AlignAxis m_ObjectUpAxis = AlignAxis.YAxis;

	[SerializeField]
	[Tooltip("Normalized distance [0;1] offset along the spline at which the GameObject should be placed when the animation begins.")]
	private float m_StartOffset;

	[NonSerialized]
	private float m_StartOffsetT;

	private bool m_PlayOnAwakeHandledForSession;

	private float m_SplineLength = -1f;

	private bool m_Playing;

	private float m_NormalizedTime;

	private float m_ElapsedTime;

	private SplinePath<Spline> m_SplinePath;

	private bool m_EndReached;

	internal static readonly string k_EmptyContainerError = "SplineAnimate does not have a valid SplineContainer set.";

	[Obsolete("Use Container instead.", false)]
	public SplineContainer splineContainer => Container;

	public SplineContainer Container
	{
		get
		{
			return m_Target;
		}
		set
		{
			m_Target = value;
			if (base.enabled && m_Target != null && m_Target.Splines != null)
			{
				for (int i = 0; i < m_Target.Splines.Count; i++)
				{
					OnSplineChange(m_Target.Splines[i], -1, SplineModification.Default);
				}
			}
			UpdateStartOffsetT();
		}
	}

	[Obsolete("Use PlayOnAwake instead.", false)]
	public bool playOnAwake => PlayOnAwake;

	public bool PlayOnAwake
	{
		get
		{
			return m_PlayOnAwake;
		}
		set
		{
			m_PlayOnAwake = value;
		}
	}

	[Obsolete("Use Loop instead.", false)]
	public LoopMode loopMode => Loop;

	public LoopMode Loop
	{
		get
		{
			return m_LoopMode;
		}
		set
		{
			m_LoopMode = value;
		}
	}

	[Obsolete("Use AnimationMethod instead.", false)]
	public Method method => AnimationMethod;

	public Method AnimationMethod
	{
		get
		{
			return m_Method;
		}
		set
		{
			m_Method = value;
		}
	}

	[Obsolete("Use Duration instead.", false)]
	public float duration => Duration;

	public float Duration
	{
		get
		{
			return m_Duration;
		}
		set
		{
			if (m_Method == Method.Time)
			{
				m_Duration = Mathf.Max(0f, value);
				CalculateMaxSpeed();
			}
		}
	}

	[Obsolete("Use MaxSpeed instead.", false)]
	public float maxSpeed => MaxSpeed;

	public float MaxSpeed
	{
		get
		{
			return m_MaxSpeed;
		}
		set
		{
			if (m_Method == Method.Speed)
			{
				m_MaxSpeed = Mathf.Max(0f, value);
				CalculateDuration();
			}
		}
	}

	[Obsolete("Use Easing instead.", false)]
	public EasingMode easingMode => Easing;

	public EasingMode Easing
	{
		get
		{
			return m_EasingMode;
		}
		set
		{
			m_EasingMode = value;
		}
	}

	[Obsolete("Use Alignment instead.", false)]
	public AlignmentMode alignmentMode => Alignment;

	public AlignmentMode Alignment
	{
		get
		{
			return m_AlignmentMode;
		}
		set
		{
			m_AlignmentMode = value;
		}
	}

	[Obsolete("Use ObjectForwardAxis instead.", false)]
	public AlignAxis objectForwardAxis => ObjectForwardAxis;

	public AlignAxis ObjectForwardAxis
	{
		get
		{
			return m_ObjectForwardAxis;
		}
		set
		{
			m_ObjectUpAxis = SetObjectAlignAxis(value, ref m_ObjectForwardAxis, m_ObjectUpAxis);
		}
	}

	[Obsolete("Use ObjectUpAxis instead.", false)]
	public AlignAxis objectUpAxis => ObjectUpAxis;

	public AlignAxis ObjectUpAxis
	{
		get
		{
			return m_ObjectUpAxis;
		}
		set
		{
			m_ObjectForwardAxis = SetObjectAlignAxis(value, ref m_ObjectUpAxis, m_ObjectForwardAxis);
		}
	}

	[Obsolete("Use NormalizedTime instead.", false)]
	public float normalizedTime => NormalizedTime;

	public float NormalizedTime
	{
		get
		{
			return m_NormalizedTime;
		}
		set
		{
			m_NormalizedTime = value;
			if (m_LoopMode == LoopMode.PingPong)
			{
				int num = (int)(m_ElapsedTime / m_Duration);
				m_ElapsedTime = m_Duration * m_NormalizedTime + ((num % 2 == 1) ? m_Duration : 0f);
			}
			else
			{
				m_ElapsedTime = m_Duration * m_NormalizedTime;
			}
			UpdateTransform();
		}
	}

	[Obsolete("Use ElapsedTime instead.", false)]
	public float elapsedTime => ElapsedTime;

	public float ElapsedTime
	{
		get
		{
			return m_ElapsedTime;
		}
		set
		{
			m_ElapsedTime = value;
			CalculateNormalizedTime(0f);
			UpdateTransform();
		}
	}

	public float StartOffset
	{
		get
		{
			return m_StartOffset;
		}
		set
		{
			if (m_SplineLength < 0f)
			{
				RebuildSplinePath();
			}
			m_StartOffset = Mathf.Clamp01(value);
			UpdateStartOffsetT();
		}
	}

	internal float StartOffsetT => m_StartOffsetT;

	[Obsolete("Use IsPlaying instead.", false)]
	public bool isPlaying => IsPlaying;

	public bool IsPlaying => m_Playing;

	[Obsolete("Use Updated instead.", false)]
	public event Action<Vector3, Quaternion> onUpdated;

	public event Action<Vector3, Quaternion> Updated;

	public event Action Completed;

	private void Awake()
	{
		m_PlayOnAwakeHandledForSession = false;
		RecalculateAnimationParameters();
	}

	private void OnEnable()
	{
		RecalculateAnimationParameters();
		Spline.Changed += OnSplineChange;
		if (!m_PlayOnAwakeHandledForSession)
		{
			Restart(m_PlayOnAwake);
			m_PlayOnAwakeHandledForSession = true;
		}
	}

	private void OnDisable()
	{
		Spline.Changed -= OnSplineChange;
	}

	private void OnValidate()
	{
		m_Duration = Mathf.Max(0f, m_Duration);
		m_MaxSpeed = Mathf.Max(0f, m_MaxSpeed);
		RecalculateAnimationParameters();
	}

	internal void RecalculateAnimationParameters()
	{
		RebuildSplinePath();
		switch (m_Method)
		{
		case Method.Time:
			CalculateMaxSpeed();
			break;
		case Method.Speed:
			CalculateDuration();
			break;
		default:
			Debug.Log($"{m_Method} animation method is not supported!", this);
			break;
		}
	}

	private bool IsNullOrEmptyContainer()
	{
		if (m_Target == null || m_Target.Splines.Count == 0)
		{
			if (Application.isPlaying)
			{
				Debug.LogError(k_EmptyContainerError, this);
			}
			return true;
		}
		return false;
	}

	public void Play()
	{
		if (!IsNullOrEmptyContainer())
		{
			m_Playing = true;
		}
	}

	public void Pause()
	{
		m_Playing = false;
	}

	public void Restart(bool autoplay)
	{
		if (!(Container == null) && !IsNullOrEmptyContainer())
		{
			m_Playing = false;
			m_ElapsedTime = 0f;
			NormalizedTime = 0f;
			switch (m_Method)
			{
			case Method.Time:
				CalculateMaxSpeed();
				break;
			case Method.Speed:
				CalculateDuration();
				break;
			default:
				Debug.Log($"{m_Method} animation method is not supported!", this);
				break;
			}
			UpdateTransform();
			UpdateStartOffsetT();
			if (autoplay)
			{
				Play();
			}
		}
	}

	public void Update()
	{
		if (m_Playing && (m_LoopMode != 0 || !(m_NormalizedTime >= 1f)))
		{
			float deltaTime = Time.deltaTime;
			CalculateNormalizedTime(deltaTime);
			UpdateTransform();
		}
	}

	private void CalculateNormalizedTime(float deltaTime)
	{
		float previousTime = m_ElapsedTime;
		m_ElapsedTime += deltaTime;
		float num = m_Duration;
		float num2 = 0f;
		switch (m_LoopMode)
		{
		case LoopMode.Once:
			num2 = Mathf.Min(m_ElapsedTime, num);
			break;
		case LoopMode.Loop:
			num2 = m_ElapsedTime % num;
			UpdateEndReached(previousTime, num);
			break;
		case LoopMode.LoopEaseInOnce:
			if ((m_EasingMode == EasingMode.EaseIn || m_EasingMode == EasingMode.EaseInOut) && m_ElapsedTime >= num)
			{
				num *= 0.5f;
			}
			num2 = m_ElapsedTime % num;
			UpdateEndReached(previousTime, num);
			break;
		case LoopMode.PingPong:
			num2 = Mathf.PingPong(m_ElapsedTime, num);
			UpdateEndReached(previousTime, num);
			break;
		default:
			Debug.Log($"{m_LoopMode} animation loop mode is not supported!", this);
			break;
		}
		num2 /= num;
		if (m_LoopMode == LoopMode.LoopEaseInOnce)
		{
			if ((m_EasingMode == EasingMode.EaseIn || m_EasingMode == EasingMode.EaseInOut) && m_ElapsedTime < num)
			{
				num2 = EaseInQuadratic(num2);
			}
		}
		else
		{
			switch (m_EasingMode)
			{
			case EasingMode.EaseIn:
				num2 = EaseInQuadratic(num2);
				break;
			case EasingMode.EaseOut:
				num2 = EaseOutQuadratic(num2);
				break;
			case EasingMode.EaseInOut:
				num2 = EaseInOutQuadratic(num2);
				break;
			}
		}
		m_NormalizedTime = ((num2 == 0f) ? 0f : (Mathf.Floor(m_NormalizedTime) + num2));
		if (m_NormalizedTime >= 1f && m_LoopMode == LoopMode.Once)
		{
			m_EndReached = true;
			m_Playing = false;
		}
	}

	private void UpdateEndReached(float previousTime, float currentDuration)
	{
		m_EndReached = Mathf.FloorToInt(previousTime / currentDuration) < Mathf.FloorToInt(m_ElapsedTime / currentDuration);
	}

	private void UpdateStartOffsetT()
	{
		if (m_SplinePath != null)
		{
			m_StartOffsetT = m_SplinePath.ConvertIndexUnit(m_StartOffset * m_SplineLength, PathIndexUnit.Distance, PathIndexUnit.Normalized);
		}
	}

	private void UpdateTransform()
	{
		if (!(m_Target == null))
		{
			EvaluatePositionAndRotation(out var position, out var rotation);
			base.transform.position = position;
			if (m_AlignmentMode != 0)
			{
				base.transform.rotation = rotation;
			}
			this.onUpdated?.Invoke(position, rotation);
			this.Updated?.Invoke(position, rotation);
			if (m_EndReached)
			{
				m_EndReached = false;
				this.Completed?.Invoke();
			}
		}
	}

	private void EvaluatePositionAndRotation(out Vector3 position, out Quaternion rotation)
	{
		float loopInterpolation = GetLoopInterpolation(offset: true);
		position = m_Target.EvaluatePosition(m_SplinePath, loopInterpolation);
		rotation = Quaternion.identity;
		float3 axis = GetAxis(m_ObjectForwardAxis);
		Quaternion quaternion = Quaternion.Inverse(Quaternion.LookRotation(upwards: GetAxis(m_ObjectUpAxis), forward: axis));
		if (m_AlignmentMode != 0)
		{
			Vector3 vector = Vector3.forward;
			Vector3 vector2 = Vector3.up;
			switch (m_AlignmentMode)
			{
			case AlignmentMode.SplineElement:
				vector = m_Target.EvaluateTangent(m_SplinePath, loopInterpolation);
				if (Vector3.Magnitude(vector) <= Mathf.Epsilon)
				{
					vector = ((!(loopInterpolation < 1f)) ? ((Vector3)m_Target.EvaluateTangent(m_SplinePath, loopInterpolation - 0.01f)) : ((Vector3)m_Target.EvaluateTangent(m_SplinePath, Mathf.Min(1f, loopInterpolation + 0.01f))));
				}
				vector.Normalize();
				vector2 = m_Target.EvaluateUpVector(m_SplinePath, loopInterpolation);
				break;
			case AlignmentMode.SplineObject:
			{
				Quaternion rotation2 = m_Target.transform.rotation;
				vector = rotation2 * vector;
				vector2 = rotation2 * vector2;
				break;
			}
			default:
				Debug.Log($"{m_AlignmentMode} animation alignment mode is not supported!", this);
				break;
			}
			if (math.all(math.isfinite(vector) & math.isfinite(vector2)))
			{
				rotation = Quaternion.LookRotation(vector, vector2) * quaternion;
			}
			else
			{
				Debug.LogError("Trying to EvaluatePositionAndRotation with invalid parameters. Please check the SplineAnimate component.", this);
			}
		}
		else
		{
			rotation = base.transform.rotation;
		}
	}

	private void CalculateDuration()
	{
		if (m_SplineLength < 0f)
		{
			RebuildSplinePath();
		}
		if (m_SplineLength >= 0f)
		{
			switch (m_EasingMode)
			{
			case EasingMode.None:
				m_Duration = m_SplineLength / m_MaxSpeed;
				break;
			case EasingMode.EaseIn:
			case EasingMode.EaseOut:
			case EasingMode.EaseInOut:
				m_Duration = 2f * m_SplineLength / m_MaxSpeed;
				break;
			default:
				Debug.Log($"{m_EasingMode} animation easing mode is not supported!", this);
				break;
			}
		}
	}

	private void CalculateMaxSpeed()
	{
		if (m_SplineLength < 0f)
		{
			RebuildSplinePath();
		}
		if (m_SplineLength >= 0f)
		{
			switch (m_EasingMode)
			{
			case EasingMode.None:
				m_MaxSpeed = m_SplineLength / m_Duration;
				break;
			case EasingMode.EaseIn:
			case EasingMode.EaseOut:
			case EasingMode.EaseInOut:
				m_MaxSpeed = 2f * m_SplineLength / m_Duration;
				break;
			default:
				Debug.Log($"{m_EasingMode} animation easing mode is not supported!", this);
				break;
			}
		}
	}

	private void RebuildSplinePath()
	{
		if (m_Target != null)
		{
			m_SplinePath = new SplinePath<Spline>(m_Target.Splines);
			m_SplineLength = ((m_SplinePath != null) ? m_SplinePath.GetLength() : 0f);
		}
	}

	private AlignAxis SetObjectAlignAxis(AlignAxis newValue, ref AlignAxis targetAxis, AlignAxis otherAxis)
	{
		if (newValue == otherAxis)
		{
			otherAxis = targetAxis;
			targetAxis = newValue;
		}
		else if ((int)newValue % 3 != (int)otherAxis % 3)
		{
			targetAxis = newValue;
		}
		return otherAxis;
	}

	private void OnSplineChange(Spline spline, int knotIndex, SplineModification modificationType)
	{
		RecalculateAnimationParameters();
	}

	internal float GetLoopInterpolation(bool offset)
	{
		float num = 0f;
		float num2 = NormalizedTime + (offset ? m_StartOffsetT : 0f);
		if (Mathf.Floor(num2) == num2)
		{
			return Mathf.Clamp01(num2);
		}
		return num2 % 1f;
	}

	private float EaseInQuadratic(float t)
	{
		return t * t;
	}

	private float EaseOutQuadratic(float t)
	{
		return t * (2f - t);
	}

	private float EaseInOutQuadratic(float t)
	{
		float num = 2f * t * t;
		if (t > 0.5f)
		{
			num = 4f * t - num - 1f;
		}
		return num;
	}
}
