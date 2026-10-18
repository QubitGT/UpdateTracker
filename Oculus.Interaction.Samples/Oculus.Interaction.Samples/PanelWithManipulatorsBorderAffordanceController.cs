using System;
using System.Collections.Generic;
using Oculus.Interaction.HandGrab;
using UnityEngine;

namespace Oculus.Interaction.Samples;

public class PanelWithManipulatorsBorderAffordanceController : MonoBehaviour
{
	private enum RailState
	{
		Hidden,
		Hover,
		Selected
	}

	private enum AffordanceState
	{
		Hidden,
		Visible
	}

	[Serializable]
	private class Affordance
	{
		[SerializeField]
		[Tooltip("The parent transform of the geometry (i.e., visuals) which should be moved to place the capsule affordance")]
		private Transform _geometry;

		[SerializeField]
		[Tooltip("Then transform controlled by an animation whose X axis magnitude will be used to control the affordance's opacity")]
		private Transform _opacityTransform;

		[SerializeField]
		[Tooltip("The animators (canonically geometry and opacity) whose 'state' variables should be controlled by this affordance")]
		private Animator[] _animators;

		private AffordanceState _animationState;

		private Vector3 _lastKnownPositionParentSpace;

		public AffordanceState AnimationState
		{
			get
			{
				return _animationState;
			}
			set
			{
				if (value != _animationState)
				{
					_animationState = value;
					Animator[] animators = _animators;
					for (int i = 0; i < animators.Length; i++)
					{
						animators[i].SetInteger("state", (int)_animationState);
					}
				}
			}
		}

		public Vector3 LastKnownPositionParentSpace
		{
			get
			{
				return _lastKnownPositionParentSpace;
			}
			set
			{
				_lastKnownPositionParentSpace = value;
			}
		}

		public Transform Geometry => _geometry;

		public float Opacity => Mathf.Abs(_opacityTransform.localPosition.x);
	}

	private class FadePoint
	{
		public int affordanceIndex;

		public bool removeFlag;

		public FadePoint(int index)
		{
			affordanceIndex = index;
			removeFlag = false;
		}
	}

	[Header("Interactables")]
	[SerializeField]
	[Tooltip("The grab interactable for the slate itself (as opposed to the surrounding affordances)")]
	private GrabInteractable _grabInteractable;

	[SerializeField]
	[Tooltip("The hand grab interactable for the slate itself (as opposed to the surrounding affordances)")]
	private HandGrabInteractable _handGrabInteractable;

	[SerializeField]
	[Optional]
	[Tooltip("The hand grab interactable for the slate itself (as opposed to the surrounding affordances)")]
	private RayInteractable _rayInteractable;

	[Header("Panel Signals")]
	[SerializeField]
	[Tooltip("The state signaler for the SlateWithManipulators prefab")]
	private PanelWithManipulatorsStateSignaler _stateSignaler;

	[SerializeField]
	[Optional]
	[Tooltip("Holds the panel hover state")]
	private PanelHoverState _panelHoverState;

	[SerializeField]
	[Tooltip("The grabbable associated with the slate itself (i.e., the grabbable with One- and TwoGrabFreeTransformers")]
	private Grabbable _grabbale;

	[Space(10f)]
	[SerializeField]
	[Tooltip("The transform of one of the bones of the rail affordance (used in calculating capsule placement)")]
	private Transform _boneTransform;

	[SerializeField]
	[Tooltip("The radius of the arcs at the corners of the rail affordance (used in calculating capsule placement)")]
	private float _cornerArcRadius;

	[Space(10f)]
	[SerializeField]
	[Tooltip("The animator controlling the overall opacity of the rail affordance (note that this is independent of the localized opacities associated with the capsule affordances)")]
	private Animator _railOpacityAnimator;

	[SerializeField]
	[Tooltip("The transform being controlled by the rail opacity animator")]
	private Transform _railOpacityTransform;

	[SerializeField]
	[Tooltip("The capsule affordances")]
	private Affordance[] _affordances;

	[SerializeField]
	[Tooltip("The renderer controlling shading for the rail affordance")]
	private SkinnedMeshRenderer _railRenderer;

	private Vector4[] _fadePoints;

	private MaterialPropertyBlock _materialPropertyBlock;

	private Dictionary<int, FadePoint> _points;

	private HashSet<int> _affordancesInUse;

	private List<int> _deletePointKeys;

	private static Vector3? _projectToRoundedBoxEdge(Vector3 worldSpacePoint, Transform targetTransform, Transform boneTransform, float arcRadius)
	{
		Vector3 vector = targetTransform.InverseTransformPointUnscaled(worldSpacePoint);
		vector.z = 0f;
		Vector3 vector2 = targetTransform.InverseTransformPointUnscaled(boneTransform.position);
		float num = Mathf.Sign(vector.x) * Mathf.Min(Mathf.Abs(vector2.x), Mathf.Abs(vector.x));
		float num2 = Mathf.Sign(vector.y) * Mathf.Min(Mathf.Abs(vector2.y), Mathf.Abs(vector.y));
		bool num3 = Mathf.Abs(Mathf.Abs(num) - Mathf.Abs(vector2.x)) < Mathf.Epsilon;
		bool flag = Mathf.Abs(Mathf.Abs(num2) - Mathf.Abs(vector2.y)) < Mathf.Epsilon;
		if (num3 || flag)
		{
			Vector3 vector3 = new Vector3(num, num2, 0f);
			Vector3 normalized = (vector - vector3).normalized;
			return targetTransform.TransformPointUnscaled(vector3 + normalized * arcRadius);
		}
		return null;
	}

	private void Start()
	{
		_materialPropertyBlock = new MaterialPropertyBlock();
		_fadePoints = new Vector4[4]
		{
			Vector4.zero,
			Vector4.zero,
			Vector4.zero,
			Vector4.zero
		};
		_points = new Dictionary<int, FadePoint>();
		_affordancesInUse = new HashSet<int>();
		_deletePointKeys = new List<int>();
		_grabInteractable.WhenStateChanged += HandleInteractableStateChanged;
		_handGrabInteractable.WhenStateChanged += HandleInteractableStateChanged;
		if (_rayInteractable != null)
		{
			_rayInteractable.WhenStateChanged += HandleInteractableStateChanged;
		}
		_railOpacityAnimator.SetInteger("state", 0);
		_stateSignaler.WhenStateChanged += HandleStateChanged;
		_grabbale.WhenPointerEventRaised += HandlePointerEvent;
	}

	private void OnDestroy()
	{
		_grabInteractable.WhenStateChanged -= HandleInteractableStateChanged;
		_handGrabInteractable.WhenStateChanged -= HandleInteractableStateChanged;
		if (_rayInteractable != null)
		{
			_rayInteractable.WhenStateChanged -= HandleInteractableStateChanged;
		}
		_stateSignaler.WhenStateChanged -= HandleStateChanged;
		_grabbale.WhenPointerEventRaised -= HandlePointerEvent;
	}

	private void CreateFadePoint(int eventIdentifier)
	{
		int num = -1;
		for (int i = 0; i < _affordances.Length; i++)
		{
			if (!_affordancesInUse.Contains(i))
			{
				num = i;
				_affordancesInUse.Add(i);
				break;
			}
		}
		if (num >= 0)
		{
			_points.Add(eventIdentifier, new FadePoint(num));
		}
	}

	private void HandlePointerEvent(PointerEvent evt)
	{
		switch (evt.Type)
		{
		case PointerEventType.Hover:
		case PointerEventType.Unselect:
		case PointerEventType.Move:
		{
			Vector3? vector = _projectToRoundedBoxEdge(evt.Pose.position, _grabbale.Transform, _boneTransform, _cornerArcRadius);
			Vector3 position = evt.Pose.position;
			if (vector.HasValue)
			{
				position = vector.Value;
			}
			if (!_points.ContainsKey(evt.Identifier))
			{
				CreateFadePoint(evt.Identifier);
				break;
			}
			_points[evt.Identifier].removeFlag = false;
			_affordances[_points[evt.Identifier].affordanceIndex].LastKnownPositionParentSpace = _grabbale.Transform.InverseTransformPoint(position);
			break;
		}
		case PointerEventType.Unhover:
		case PointerEventType.Cancel:
			if (_points.ContainsKey(evt.Identifier))
			{
				_points[evt.Identifier].removeFlag = true;
			}
			break;
		case PointerEventType.Select:
			break;
		}
	}

	private void SetRailAnimatorState()
	{
		RailState railState = RailState.Hidden;
		railState = (((object)_panelHoverState == null) ? RailState.Hover : (_panelHoverState.Hovered ? RailState.Hover : RailState.Hidden));
		if (_grabbale.SelectingPoints.Count > 0)
		{
			railState = RailState.Selected;
		}
		bool flag = !(_rayInteractable != null) || _rayInteractable.State == InteractableState.Disabled;
		if (_grabInteractable.State == InteractableState.Disabled && _handGrabInteractable.State == InteractableState.Disabled && flag)
		{
			railState = RailState.Hidden;
		}
		_railOpacityAnimator.SetInteger("state", (int)railState);
	}

	private void UpdateFadePoints()
	{
		if (_points.Count <= 0)
		{
			return;
		}
		_deletePointKeys.Clear();
		foreach (KeyValuePair<int, FadePoint> point in _points)
		{
			FadePoint value = point.Value;
			Affordance affordance = _affordances[value.affordanceIndex];
			affordance.AnimationState = ((!value.removeFlag) ? AffordanceState.Visible : AffordanceState.Hidden);
			if (value.removeFlag && affordance.Opacity < 0.05f)
			{
				_deletePointKeys.Add(point.Key);
			}
		}
		foreach (int deletePointKey in _deletePointKeys)
		{
			_points.Remove(deletePointKey, out var value2);
			_affordancesInUse.Remove(value2.affordanceIndex);
		}
	}

	private void UpdateMaterialProperties()
	{
		_materialPropertyBlock.SetFloat("_OpacityMultiplier", _railOpacityTransform.localPosition.x);
		_materialPropertyBlock.SetFloat("_SelectedOpacityParam", _railOpacityTransform.localPosition.y);
		int num = 0;
		Affordance[] affordances = _affordances;
		foreach (Affordance affordance in affordances)
		{
			if (num >= _fadePoints.Length)
			{
				break;
			}
			Vector3 position = _grabbale.Transform.TransformPoint(affordance.LastKnownPositionParentSpace);
			affordance.Geometry.position = position;
			_fadePoints[num].x = position.x;
			_fadePoints[num].y = position.y;
			_fadePoints[num].z = position.z;
			_fadePoints[num].w = affordance.Opacity;
			num++;
		}
		_materialPropertyBlock.SetVectorArray("_WorldSpaceFadePoints", _fadePoints);
		_materialPropertyBlock.SetInteger("_UsedPointCount", num);
		_railRenderer.SetPropertyBlock(_materialPropertyBlock);
	}

	private void Update()
	{
		SetRailAnimatorState();
		UpdateFadePoints();
		UpdateMaterialProperties();
	}

	private void HandleInteractableStateChanged(InteractableStateChangeArgs args)
	{
		if (args.NewState == InteractableState.Select)
		{
			_stateSignaler.CurrentState = PanelWithManipulatorsStateSignaler.State.Selected;
		}
		else if (args.PreviousState == InteractableState.Select)
		{
			_stateSignaler.CurrentState = PanelWithManipulatorsStateSignaler.State.Default;
		}
	}

	private void HandleStateChanged(PanelWithManipulatorsStateSignaler.State state)
	{
		if (state != 0)
		{
			bool flag = !(_rayInteractable != null) || _rayInteractable.State != InteractableState.Select;
			if (_grabInteractable.State != InteractableState.Select && _handGrabInteractable.State != InteractableState.Select && flag)
			{
				_grabInteractable.enabled = false;
				_handGrabInteractable.enabled = false;
				if (_rayInteractable != null)
				{
					_rayInteractable.enabled = false;
				}
			}
		}
		else
		{
			_grabInteractable.enabled = true;
			_handGrabInteractable.enabled = true;
			if (_rayInteractable != null)
			{
				_rayInteractable.enabled = true;
			}
		}
	}
}
