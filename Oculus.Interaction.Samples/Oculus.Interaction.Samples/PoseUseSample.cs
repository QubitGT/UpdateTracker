using System;
using Oculus.Interaction.Input;
using TMPro;
using UnityEngine;

namespace Oculus.Interaction.Samples;

public class PoseUseSample : MonoBehaviour
{
	[SerializeField]
	[Interface(typeof(IHmd), new Type[] { })]
	private UnityEngine.Object _hmd;

	[SerializeField]
	private ActiveStateSelector[] _poses;

	[SerializeField]
	private Material[] _onSelectIcons;

	[SerializeField]
	private GameObject _poseActiveVisualPrefab;

	private GameObject[] _poseActiveVisuals;

	private IHmd Hmd { get; set; }

	protected virtual void Awake()
	{
		Hmd = _hmd as IHmd;
	}

	protected virtual void Start()
	{
		_poseActiveVisuals = new GameObject[_poses.Length];
		for (int i = 0; i < _poses.Length; i++)
		{
			_poseActiveVisuals[i] = UnityEngine.Object.Instantiate(_poseActiveVisualPrefab);
			_poseActiveVisuals[i].GetComponentInChildren<TextMeshPro>().text = _poses[i].name;
			_poseActiveVisuals[i].GetComponentInChildren<ParticleSystemRenderer>().material = _onSelectIcons[i];
			_poseActiveVisuals[i].SetActive(value: false);
			int poseNumber = i;
			_poses[i].WhenSelected += delegate
			{
				ShowVisuals(poseNumber);
			};
			_poses[i].WhenUnselected += delegate
			{
				HideVisuals(poseNumber);
			};
		}
	}

	private void ShowVisuals(int poseNumber)
	{
		if (Hmd.TryGetRootPose(out var pose))
		{
			Vector3 position = pose.position + pose.forward;
			_poseActiveVisuals[poseNumber].transform.position = position;
			_poseActiveVisuals[poseNumber].transform.LookAt(2f * _poseActiveVisuals[poseNumber].transform.position - pose.position);
			HandRef[] components = _poses[poseNumber].GetComponents<HandRef>();
			Vector3 zero = Vector3.zero;
			HandRef[] array = components;
			foreach (HandRef obj in array)
			{
				obj.GetRootPose(out var pose2);
				Vector3 vector = ((obj.Handedness == Handedness.Left) ? pose2.right : (-pose2.right));
				zero += pose2.position + vector * 0.15f + Vector3.up * 0.02f;
			}
			_poseActiveVisuals[poseNumber].transform.position = zero / components.Length;
			_poseActiveVisuals[poseNumber].gameObject.SetActive(value: true);
		}
	}

	private void HideVisuals(int poseNumber)
	{
		_poseActiveVisuals[poseNumber].gameObject.SetActive(value: false);
	}
}
