using System.Collections.Generic;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction.Samples;

public class MRPassThroughHandVisualize : MonoBehaviour
{
	[SerializeField]
	private List<Transform> _eyeAnchors;

	[SerializeField]
	private HandVisual _handVisual;

	[Header("Raycast Properties")]
	[SerializeField]
	private LayerMask _layer;

	[SerializeField]
	private float _sphereRadius;

	[SerializeField]
	private float _castDistance;

	[Header("Material Properties")]
	[SerializeField]
	private MaterialPropertyBlockEditor[] _handMaterialPropertyBlocks;

	[SerializeField]
	private float _opacity;

	[SerializeField]
	private float _outlineOpacity;

	[SerializeField]
	private float _animationSpeed;

	private float _currentOpacity;

	private float _currentOutlineOpacity;

	private readonly int _opacityId = Shader.PropertyToID("_Opacity");

	private readonly int _outlineOpacityId = Shader.PropertyToID("_OutlineOpacity");

	private (Vector3, float) _palmTarget;

	private readonly HandJointId[] _handJointTargets = new HandJointId[10]
	{
		HandJointId.HandIndex2,
		HandJointId.HandIndex3,
		HandJointId.HandThumb2,
		HandJointId.HandThumb3,
		HandJointId.HandMiddle2,
		HandJointId.HandMiddle3,
		HandJointId.HandRing2,
		HandJointId.HandRing3,
		HandJointId.HandPinky2,
		HandJointId.HandPinky3
	};

	private Ray[] _eyeRays;

	private bool _started;

	private void Start()
	{
		this.BeginStart(ref _started);
		this.EndStart(ref _started);
		_eyeRays = new Ray[_eyeAnchors.Count];
		_currentOpacity = _opacity;
		_currentOutlineOpacity = _outlineOpacity;
		List<Vector3> list = new List<Vector3>
		{
			_handVisual.GetJointPose(HandJointId.HandWristRoot, Space.World).position,
			_handVisual.GetJointPose(HandJointId.HandThumb1, Space.World).position,
			_handVisual.GetJointPose(HandJointId.HandIndex1, Space.World).position,
			_handVisual.GetJointPose(HandJointId.HandMiddle1, Space.World).position,
			_handVisual.GetJointPose(HandJointId.HandRing1, Space.World).position,
			_handVisual.GetJointPose(HandJointId.HandPinky1, Space.World).position
		};
		Vector3 zero = Vector3.zero;
		foreach (Vector3 item2 in list)
		{
			zero += item2;
		}
		zero *= 1f / (float)list.Count;
		Vector3 item = _handVisual.GetTransformByHandJointId(HandJointId.HandWristRoot).InverseTransformPoint(zero);
		float num = 0f;
		foreach (Vector3 item3 in list)
		{
			num = Mathf.Max(num, Vector3.Distance(zero, item3));
		}
		_palmTarget = (item, num * 0.65f);
	}

	private bool SphereCast(Vector3 target, float radius)
	{
		for (int i = 0; i < _eyeAnchors.Count; i++)
		{
			Vector3 position = _eyeAnchors[i].position;
			Vector3 normalized = (target - position).normalized;
			_eyeRays[i] = new Ray(position, normalized);
		}
		Ray[] eyeRays = _eyeRays;
		for (int j = 0; j < eyeRays.Length; j++)
		{
			if (Physics.SphereCast(eyeRays[j], radius, _castDistance, _layer))
			{
				return true;
			}
		}
		return false;
	}

	private bool SphereCastAllTargets()
	{
		Vector3 target = _handVisual.GetTransformByHandJointId(HandJointId.HandWristRoot).TransformPoint(_palmTarget.Item1);
		if (SphereCast(target, _palmTarget.Item2))
		{
			return true;
		}
		HandJointId[] handJointTargets = _handJointTargets;
		foreach (HandJointId jointId in handJointTargets)
		{
			if (SphereCast(_handVisual.GetJointPose(jointId, Space.World).position, _sphereRadius))
			{
				return true;
			}
		}
		return false;
	}

	private void UpdateMaterialPropertyBlock(bool sphereCastHit)
	{
		float b = (sphereCastHit ? _opacity : 0f);
		float b2 = (sphereCastHit ? _outlineOpacity : 0f);
		float t = _animationSpeed * Time.deltaTime;
		_currentOpacity = Mathf.Lerp(_currentOpacity, b, t);
		_currentOutlineOpacity = Mathf.Lerp(_currentOutlineOpacity, b2, t);
		MaterialPropertyBlockEditor[] handMaterialPropertyBlocks = _handMaterialPropertyBlocks;
		foreach (MaterialPropertyBlockEditor obj in handMaterialPropertyBlocks)
		{
			obj.MaterialPropertyBlock.SetFloat(_opacityId, _currentOpacity);
			obj.MaterialPropertyBlock.SetFloat(_outlineOpacityId, _currentOutlineOpacity);
		}
	}

	private void Update()
	{
		if (MRPassthrough.PassThrough.IsPassThroughOn)
		{
			if (_eyeAnchors != null && !(_handVisual == null))
			{
				UpdateMaterialPropertyBlock(SphereCastAllTargets());
			}
			return;
		}
		MaterialPropertyBlockEditor[] handMaterialPropertyBlocks = _handMaterialPropertyBlocks;
		foreach (MaterialPropertyBlockEditor obj in handMaterialPropertyBlocks)
		{
			obj.MaterialPropertyBlock.SetFloat(_opacityId, _opacity);
			obj.MaterialPropertyBlock.SetFloat(_outlineOpacityId, _outlineOpacity);
		}
	}
}
