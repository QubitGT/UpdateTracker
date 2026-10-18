using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction.Body.Input;

public class ReadOnlyBodyJointPoses : IReadOnlyList<Pose>, IEnumerable<Pose>, IEnumerable, IReadOnlyCollection<Pose>
{
	private Pose[] _poses;

	public static ReadOnlyBodyJointPoses Empty { get; } = new ReadOnlyBodyJointPoses(Array.Empty<Pose>());


	public int Count => _poses.Length;

	public Pose this[int index] => _poses[index];

	public ref readonly Pose this[BodyJointId index] => ref _poses[(int)index];

	public ReadOnlyBodyJointPoses(Pose[] poses)
	{
		_poses = poses;
	}

	public IEnumerator<Pose> GetEnumerator()
	{
		Pose[] poses = _poses;
		for (int i = 0; i < poses.Length; i++)
		{
			yield return poses[i];
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
