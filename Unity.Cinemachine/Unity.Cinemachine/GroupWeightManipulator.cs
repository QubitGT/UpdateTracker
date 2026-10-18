using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.Cinemachine;

[RequireComponent(typeof(CinemachineTargetGroup))]
[ExecuteAlways]
[AddComponentMenu("Cinemachine/Helpers/Cinemachine Group Weight Manipulator")]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/GroupWeightManipulator.html")]
public class GroupWeightManipulator : MonoBehaviour
{
	[Tooltip("The weight of the group member at index 0")]
	[FormerlySerializedAs("m_Weight0")]
	public float Weight0 = 1f;

	[Tooltip("The weight of the group member at index 1")]
	[FormerlySerializedAs("m_Weight1")]
	public float Weight1 = 1f;

	[Tooltip("The weight of the group member at index 2")]
	[FormerlySerializedAs("m_Weight2")]
	public float Weight2 = 1f;

	[Tooltip("The weight of the group member at index 3")]
	[FormerlySerializedAs("m_Weight3")]
	public float Weight3 = 1f;

	[Tooltip("The weight of the group member at index 4")]
	[FormerlySerializedAs("m_Weight4")]
	public float Weight4 = 1f;

	[Tooltip("The weight of the group member at index 5")]
	[FormerlySerializedAs("m_Weight5")]
	public float Weight5 = 1f;

	[Tooltip("The weight of the group member at index 6")]
	[FormerlySerializedAs("m_Weight6")]
	public float Weight6 = 1f;

	[Tooltip("The weight of the group member at index 7")]
	[FormerlySerializedAs("m_Weight7")]
	public float Weight7 = 1f;

	private CinemachineTargetGroup m_Group;

	private void Start()
	{
		TryGetComponent<CinemachineTargetGroup>(out m_Group);
	}

	private void OnValidate()
	{
		Weight0 = Mathf.Max(0f, Weight0);
		Weight1 = Mathf.Max(0f, Weight1);
		Weight2 = Mathf.Max(0f, Weight2);
		Weight3 = Mathf.Max(0f, Weight3);
		Weight4 = Mathf.Max(0f, Weight4);
		Weight5 = Mathf.Max(0f, Weight5);
		Weight6 = Mathf.Max(0f, Weight6);
		Weight7 = Mathf.Max(0f, Weight7);
	}

	private void Update()
	{
		if (m_Group != null)
		{
			UpdateWeights();
		}
	}

	private void UpdateWeights()
	{
		List<CinemachineTargetGroup.Target> targets = m_Group.Targets;
		int num = targets.Count - 1;
		if (num < 0)
		{
			return;
		}
		targets[0].Weight = Weight0;
		if (num < 1)
		{
			return;
		}
		targets[1].Weight = Weight1;
		if (num < 2)
		{
			return;
		}
		targets[2].Weight = Weight2;
		if (num < 3)
		{
			return;
		}
		targets[3].Weight = Weight3;
		if (num < 4)
		{
			return;
		}
		targets[4].Weight = Weight4;
		if (num < 5)
		{
			return;
		}
		targets[5].Weight = Weight5;
		if (num >= 6)
		{
			targets[6].Weight = Weight6;
			if (num >= 7)
			{
				targets[7].Weight = Weight7;
			}
		}
	}
}
