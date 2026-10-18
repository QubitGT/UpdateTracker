using System.Collections;
using Oculus.Interaction.HandGrab;
using UnityEngine;

namespace Oculus.Interaction.Samples;

public class Slingshot : MonoBehaviour, ITransformer
{
	[SerializeField]
	private Pose _neutralPose;

	[SerializeField]
	private Transform _holder;

	[SerializeField]
	private Transform _leftRubberPoint;

	[SerializeField]
	private Transform _rightRubberPoint;

	[SerializeField]
	private float _rubberAngle = 60f;

	[SerializeField]
	private AnimationCurve _translationResistance;

	[SerializeField]
	private AnimationCurve _aimingResistance;

	[SerializeField]
	private float _springForce = 0.1f;

	[SerializeField]
	private float _damping = 0.95f;

	[SerializeField]
	private float _slingshotStrength = 10f;

	[SerializeField]
	private HandGrabInteractable[] _handGrabInteractables;

	[Header("Feedback")]
	[SerializeField]
	private AudioSource _audioSource;

	[SerializeField]
	private AudioClip _stretchAudioClip;

	[SerializeField]
	private AnimationCurve _strecthAudioPitch;

	[SerializeField]
	private AnimationCurve _stretchAudioStep;

	private IGrabbable _grabbable;

	private Pose _grabDeltaInLocalSpace;

	private bool _isGrabbed;

	private Vector3 _positionVelocity = Vector3.zero;

	private float _rotationVelocity;

	private SlingshotPellet _loadedPellet;

	private WaitForSeconds _hapticsWait = new WaitForSeconds(0.05f);

	private float _lastTensionStep;

	private float _lastTensionTime;

	private const float _tensionStepLength = 0.1f;

	private void OnTriggerEnter(Collider other)
	{
		if (!(_loadedPellet != null) && other.TryGetComponent<SlingshotPellet>(out var component))
		{
			HandlePelletSnapped(component);
		}
	}

	private void HandlePelletSnapped(SlingshotPellet pellet)
	{
		HandGrabInteractor handGrabber = pellet.HandGrabber;
		if (handGrabber == null || handGrabber.State != InteractorState.Select)
		{
			return;
		}
		HandGrabInteractable[] handGrabInteractables = _handGrabInteractables;
		foreach (HandGrabInteractable handGrabInteractable in handGrabInteractables)
		{
			if (handGrabber.CanInteractWith(handGrabInteractable))
			{
				handGrabber.ForceRelease();
				handGrabber.ForceSelect(handGrabInteractable, allowManualRelease: true);
				_loadedPellet = pellet;
				_loadedPellet.Attach();
				break;
			}
		}
	}

	public void Initialize(IGrabbable grabbable)
	{
		_grabbable = grabbable;
	}

	public void BeginTransform()
	{
		Pose pose = _grabbable.GrabPoints[0];
		Transform transform = _grabbable.Transform;
		_grabDeltaInLocalSpace = new Pose(transform.InverseTransformVector(pose.position - transform.position), Quaternion.Inverse(pose.rotation) * transform.rotation);
		_isGrabbed = true;
		_positionVelocity = Vector3.zero;
		_rotationVelocity = 0f;
		CurveHolder(_rubberAngle);
	}

	public void UpdateTransform()
	{
		Pose pose = _grabbable.GrabPoints[0];
		Transform transform = _grabbable.Transform;
		Vector3 localPosition = transform.localPosition;
		Vector3 position = pose.position - transform.TransformVector(_grabDeltaInLocalSpace.position);
		Quaternion rotation = pose.rotation * _grabDeltaInLocalSpace.rotation;
		Transform parent = transform.parent;
		Pose to = new Pose(position, rotation);
		Pose pose2 = parent.Delta(in to);
		Vector3 vector = pose2.position - _neutralPose.position;
		float num = Vector3.Distance(localPosition, _neutralPose.position);
		float num2 = vector.magnitude;
		if (num2 > num)
		{
			float maxDelta = _translationResistance.Evaluate(num) * Time.deltaTime;
			num2 = Mathf.MoveTowards(num, num2, maxDelta);
		}
		Vector3 normalized = Vector3.ProjectOnPlane(vector, Vector3.right).normalized;
		vector = Vector3.Slerp(vector, normalized, _aimingResistance.Evaluate(num)).normalized;
		Vector3 localPoint = (transform.localPosition = _neutralPose.position + vector * num2);
		num = Tension(localPoint);
		float t = _aimingResistance.Evaluate(num);
		Quaternion b = Quaternion.LookRotation(-vector, pose2.up);
		transform.localRotation = Quaternion.SlerpUnclamped(pose2.rotation, b, t);
		OnStretch(num);
	}

	public void EndTransform()
	{
		_isGrabbed = false;
		if (_loadedPellet != null)
		{
			Vector3 force = SlingshotLaunchForce();
			_loadedPellet.Eject(force);
			_loadedPellet = null;
		}
		CurveHolder(0f);
	}

	private void Update()
	{
		if (!_isGrabbed)
		{
			Transform transform = base.transform;
			Vector3 vector = (_neutralPose.position - transform.localPosition) * _springForce;
			_positionVelocity = _positionVelocity * _damping + vector * Time.deltaTime;
			transform.localPosition += _positionVelocity;
			transform.localRotation.ToAngleAxis(out var angle, out var axis);
			if (angle > 180f)
			{
				angle -= 360f;
			}
			_rotationVelocity = _rotationVelocity * _damping + angle * _springForce * Time.deltaTime;
			transform.localRotation = Quaternion.AngleAxis(_rotationVelocity, -axis.normalized) * transform.localRotation;
		}
	}

	private void LateUpdate()
	{
		if (_loadedPellet != null)
		{
			_loadedPellet.Move(_holder);
		}
	}

	private void CurveHolder(float angle)
	{
		_rightRubberPoint.localEulerAngles = Vector3.up * angle;
		_leftRubberPoint.localEulerAngles = -Vector3.up * angle;
	}

	private float Tension(Vector3 localPoint)
	{
		return Vector3.Distance(localPoint, _neutralPose.position);
	}

	private Vector3 SlingshotLaunchForce()
	{
		Transform transform = _grabbable.Transform;
		float num = Tension(transform.localPosition);
		return (transform.parent.position - transform.position).normalized * num * _slingshotStrength;
	}

	public void OnStretch(float currentTension)
	{
		if (Mathf.Abs(_lastTensionStep - currentTension) > _stretchAudioStep.Evaluate(currentTension) && Time.unscaledTime - _lastTensionTime > 0.1f)
		{
			PlayStretchAudio(currentTension);
			PlayStretchHaptics(currentTension);
			_lastTensionStep = currentTension;
			_lastTensionTime = Time.unscaledTime;
		}
	}

	private void PlayStretchAudio(float tension)
	{
		float pitch = _strecthAudioPitch.Evaluate(tension);
		_audioSource.pitch = pitch;
		_audioSource.PlayOneShot(_stretchAudioClip, 1f);
	}

	private void PlayStretchHaptics(float tension)
	{
		float pitch = _strecthAudioPitch.Evaluate(tension);
		StartCoroutine(HapticsRoutine(pitch));
	}

	private IEnumerator HapticsRoutine(float pitch)
	{
		OVRInput.Controller controllers = OVRInput.Controller.Touch;
		OVRInput.SetControllerVibration(pitch * 0.5f, pitch * 0.2f, controllers);
		yield return _hapticsWait;
		OVRInput.SetControllerVibration(0f, 0f, controllers);
	}
}
