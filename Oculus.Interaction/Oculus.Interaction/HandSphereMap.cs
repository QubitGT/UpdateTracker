using System.Collections.Generic;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public class HandSphereMap : MonoBehaviour, IHandSphereMap
{
	[SerializeField]
	public FromHandPrefabDataSource _handPrefabDataSource;

	private readonly List<HandSphere>[] _sourceSphereMap = new List<HandSphere>[26];

	protected virtual void Awake()
	{
		for (int i = 0; i < 26; i++)
		{
			_sourceSphereMap[i] = new List<HandSphere>();
		}
	}

	protected virtual void Start()
	{
		for (int i = 0; i < 26; i++)
		{
			List<HandSphere> list = _sourceSphereMap[i];
			HandJointId handJointId = (HandJointId)i;
			Transform transformFor = _handPrefabDataSource.GetTransformFor(handJointId);
			if (transformFor == null)
			{
				continue;
			}
			foreach (Transform item in transformFor)
			{
				if (!(item.name != "sphere") && item.gameObject.activeSelf)
				{
					Vector3 position = item.GetPose(Space.Self).position;
					list.Add(new HandSphere(position, item.lossyScale.x * 0.5f, handJointId));
					item.gameObject.SetActive(value: false);
				}
			}
		}
	}

	public void GetSpheres(Handedness handedness, HandJointId jointId, Pose jointPose, float scale, List<HandSphere> spheres)
	{
		bool flag = handedness != _handPrefabDataSource.Handedness;
		for (int i = 0; i < _sourceSphereMap[(int)jointId].Count; i++)
		{
			HandSphere handSphere = _sourceSphereMap[(int)jointId][i];
			Vector3 position = handSphere.Position * scale;
			if (flag)
			{
				position = HandMirroring.Mirror(in position);
			}
			Vector3 position2 = jointPose.position + jointPose.rotation * position;
			HandSphere item = new HandSphere(position2, handSphere.Radius * scale, handSphere.Joint);
			spheres.Add(item);
		}
	}
}
