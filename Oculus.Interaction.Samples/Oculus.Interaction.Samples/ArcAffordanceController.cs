using UnityEngine;

namespace Oculus.Interaction.Samples;

public class ArcAffordanceController : MonoBehaviour
{
	[SerializeField]
	[Tooltip("The animator controlling the curvature of the affordance")]
	private Animator _animator;

	[SerializeField]
	[Tooltip("The transform from which world-space distance will be calculated; intuitively, 'the center of the arc's circle'")]
	private Transform _pivot;

	[SerializeField]
	[Tooltip("The function converting distance (from the pivot, a world-space observation) into curvature (an animation parameter)")]
	private AnimationCurve _distanceToCurvatureCurve;

	[SerializeField]
	[Tooltip("The renderer for the arc affordance, on which transparency values must be set.")]
	private SkinnedMeshRenderer _renderer;

	[SerializeField]
	[Tooltip("The bone at the 'top' end of the arc's armature")]
	private Transform _topBone;

	[SerializeField]
	[Tooltip("The bone at the 'bottom' end of the arc's armature")]
	private Transform _bottomBone;

	private Vector4[] _endPositions;

	private void Start()
	{
		_endPositions = new Vector4[2];
		_endPositions[0].w = 1f;
		_endPositions[1].w = 1f;
	}

	private void Update()
	{
		_animator.SetFloat("curvature", _distanceToCurvatureCurve.Evaluate(Vector3.Distance(base.transform.position, _pivot.position)));
		_endPositions[0].x = _topBone.position.x;
		_endPositions[0].y = _topBone.position.y;
		_endPositions[0].z = _topBone.position.z;
		_endPositions[1].x = _bottomBone.position.x;
		_endPositions[1].y = _bottomBone.position.y;
		_endPositions[1].z = _bottomBone.position.z;
		_renderer.material.SetVectorArray("_WorldSpaceFadePoints", _endPositions);
	}
}
