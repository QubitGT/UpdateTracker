using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction.HandGrab.Visuals;
using Oculus.Interaction.Input;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Oculus.Interaction.HandGrab.Recorder;

public class HandGrabPoseLiveRecorder : MonoBehaviour, IActiveState
{
	private struct RecorderStep
	{
		public HandGrabInteractable interactable;

		public HandPose RawHandPose { get; private set; }

		public Pose GrabPoint { get; private set; }

		public Rigidbody Item { get; private set; }

		public float HandScale { get; private set; }

		public RecorderStep(HandPose rawPose, Pose grabPoint, float scale, Rigidbody item)
		{
			RawHandPose = new HandPose(rawPose);
			GrabPoint = grabPoint;
			HandScale = scale;
			Item = item;
			interactable = null;
		}

		public void ClearInteractable()
		{
			if (interactable != null)
			{
				Object.Destroy(interactable.gameObject);
			}
		}
	}

	[SerializeField]
	private HandGrabInteractor _leftHand;

	[SerializeField]
	private HandGrabInteractor _rightHand;

	[HideInInspector]
	[SerializeField]
	[Tooltip("Prototypes of the static hands (ghosts) that visualize holding poses")]
	private HandGhostProvider _ghostProvider;

	[SerializeField]
	[Tooltip("Prototypes of the static hands (ghosts) that visualize holding poses")]
	private HandGhostProvider _handGhostProvider;

	[SerializeField]
	[Optional]
	private TimerUIControl _timerControl;

	[SerializeField]
	[Optional]
	private TextMeshPro _delayLabel;

	private RigidbodyDetector _leftDetector;

	private RigidbodyDetector _rightDetector;

	private WaitForSeconds _waitOneSeconds = new WaitForSeconds(1f);

	private Coroutine _delayedSnapRoutine;

	public UnityEvent WhenTimeStep;

	public UnityEvent WhenSnapshot;

	public UnityEvent WhenError;

	[Space]
	public UnityEvent<bool> WhenCanUndo;

	public UnityEvent<bool> WhenCanRedo;

	public UnityEvent WhenGrabAllowed;

	public UnityEvent WhenGrabDisallowed;

	private List<RecorderStep> _recorderSteps = new List<RecorderStep>();

	private int _currentStepIndex;

	private bool _grabbingEnabled = true;

	private HandGhostProvider GhostProvider => _handGhostProvider;

	private int CurrentStepIndex
	{
		get
		{
			return _currentStepIndex;
		}
		set
		{
			_currentStepIndex = value;
			WhenCanUndo?.Invoke(_currentStepIndex >= 0);
			WhenCanRedo?.Invoke(_currentStepIndex + 1 < _recorderSteps.Count);
		}
	}

	public bool Active => _grabbingEnabled;

	private void Awake()
	{
		_leftHand.InjectOptionalActiveState(this);
		_rightHand.InjectOptionalActiveState(this);
	}

	private void Start()
	{
		ClearSnapshot();
		_leftDetector = _leftHand.Rigidbody.gameObject.AddComponent<RigidbodyDetector>();
		_leftDetector.IgnoreBody(_rightHand.Rigidbody);
		_rightDetector = _rightHand.Rigidbody.gameObject.AddComponent<RigidbodyDetector>();
		_rightDetector.IgnoreBody(_leftHand.Rigidbody);
		CurrentStepIndex = -1;
		EnableGrabbing(enable: true);
	}

	public void Record()
	{
		ClearSnapshot();
		if (_timerControl != null)
		{
			_delayedSnapRoutine = StartCoroutine(DelayedSnapshot(_timerControl.DelaySeconds));
		}
		else
		{
			TakeSnapshot();
		}
	}

	private void ClearSnapshot()
	{
		if (_delayedSnapRoutine != null)
		{
			StopCoroutine(_delayedSnapRoutine);
			_delayedSnapRoutine = null;
		}
		_delayLabel.text = string.Empty;
	}

	private IEnumerator DelayedSnapshot(int seconds)
	{
		for (int i = seconds; i > 0; i--)
		{
			_delayLabel.text = i.ToString();
			WhenTimeStep?.Invoke();
			yield return _waitOneSeconds;
		}
		if (TakeSnapshot())
		{
			_delayLabel.text = "<size=10>Snap!";
			WhenSnapshot?.Invoke();
		}
		else
		{
			_delayLabel.text = "<size=10>Error";
			WhenError?.Invoke();
		}
		yield return _waitOneSeconds;
		_delayLabel.text = string.Empty;
	}

	private bool TakeSnapshot()
	{
		float bestDistance;
		Rigidbody rigidbody = FindNearestItem(_leftHand.Rigidbody, _leftDetector, out bestDistance);
		float bestDistance2;
		Rigidbody rigidbody2 = FindNearestItem(_rightHand.Rigidbody, _rightDetector, out bestDistance2);
		if (bestDistance < bestDistance2 && rigidbody != null)
		{
			return Record(_leftHand.Hand, rigidbody);
		}
		if (rigidbody2 != null)
		{
			return Record(_rightHand.Hand, rigidbody2);
		}
		Debug.LogError("No rigidbody detected near any hand");
		return false;
	}

	private Rigidbody FindNearestItem(Rigidbody handBody, RigidbodyDetector detector, out float bestDistance)
	{
		Vector3 worldCenterOfMass = handBody.worldCenterOfMass;
		float num = float.PositiveInfinity;
		Rigidbody result = null;
		foreach (Rigidbody intersectingBody in detector.IntersectingBodies)
		{
			float num2 = Vector3.Distance(intersectingBody.worldCenterOfMass, worldCenterOfMass);
			if (num2 < num)
			{
				num = num2;
				result = intersectingBody;
			}
		}
		bestDistance = num;
		return result;
	}

	public void Undo()
	{
		if (CurrentStepIndex >= 0)
		{
			_recorderSteps[CurrentStepIndex].ClearInteractable();
			CurrentStepIndex--;
		}
	}

	public void Redo()
	{
		if (CurrentStepIndex + 1 < _recorderSteps.Count)
		{
			CurrentStepIndex++;
			RecorderStep recorderStep = _recorderSteps[CurrentStepIndex];
			AddHandGrabPose(recorderStep, out recorderStep.interactable, out var handGrabPose);
			AttachGhost(handGrabPose, recorderStep.HandScale);
			_recorderSteps[CurrentStepIndex] = recorderStep;
		}
	}

	public void EnableGrabbing(bool enable)
	{
		_grabbingEnabled = enable;
		if (enable)
		{
			WhenGrabAllowed?.Invoke();
		}
		else
		{
			WhenGrabDisallowed?.Invoke();
		}
	}

	private bool Record(IHand hand, Rigidbody item)
	{
		HandPose handPose = TrackedPose(hand);
		if (handPose == null)
		{
			Debug.LogError("Tracked Pose could not be retrieved", this);
			return false;
		}
		if (!hand.GetRootPose(out var pose))
		{
			Debug.LogError("Hand Root pose could not be retrieved", this);
			return false;
		}
		Pose grabPoint = PoseUtils.DeltaScaled(item.transform, pose);
		RecorderStep recorderStep = new RecorderStep(handPose, grabPoint, hand.Scale, item);
		AddHandGrabPose(recorderStep, out recorderStep.interactable, out var handGrabPose);
		AttachGhost(handGrabPose, recorderStep.HandScale);
		int num = CurrentStepIndex + 1;
		if (num < _recorderSteps.Count)
		{
			_recorderSteps.RemoveRange(num, _recorderSteps.Count - num);
		}
		_recorderSteps.Add(recorderStep);
		CurrentStepIndex = _recorderSteps.Count - 1;
		return true;
	}

	private HandPose TrackedPose(IHand hand)
	{
		if (!hand.GetJointPosesLocal(out var localJointPoses))
		{
			return null;
		}
		HandPose handPose = new HandPose(hand.Handedness);
		for (int i = 0; i < FingersMetadata.HAND_JOINT_IDS.Length; i++)
		{
			HandJointId index = FingersMetadata.HAND_JOINT_IDS[i];
			handPose.JointRotations[i] = localJointPoses[index].rotation;
		}
		return handPose;
	}

	private void AddHandGrabPose(RecorderStep recorderStep, out HandGrabInteractable interactable, out HandGrabPose handGrabPose)
	{
		interactable = HandGrabUtils.CreateHandGrabInteractable(recorderStep.Item.transform);
		if (recorderStep.Item.TryGetComponent<Grabbable>(out var component))
		{
			interactable.InjectOptionalPointableElement(component);
		}
		HandGrabUtils.HandGrabPoseData handGrabPoseData = default(HandGrabUtils.HandGrabPoseData);
		handGrabPoseData.handPose = recorderStep.RawHandPose;
		handGrabPoseData.scale = recorderStep.HandScale / interactable.RelativeTo.lossyScale.x;
		handGrabPoseData.gripPose = recorderStep.GrabPoint;
		HandGrabUtils.HandGrabPoseData poseData = handGrabPoseData;
		handGrabPose = HandGrabUtils.LoadHandGrabPose(interactable, poseData);
	}

	private void AttachGhost(HandGrabPose point, float referenceScale)
	{
		if (!(GhostProvider == null))
		{
			HandGhost handGhost = Object.Instantiate(GhostProvider.GetHand(point.HandPose.Handedness), point.transform);
			handGhost.transform.localScale = Vector3.one * (referenceScale / point.transform.lossyScale.x);
			handGhost.SetPose(point);
		}
	}
}
