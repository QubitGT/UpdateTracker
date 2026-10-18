using System.Collections.Generic;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public class TouchShadowHand
{
	public class GrabTouchInfo
	{
		public Vector3 offset;

		public bool grabbing;

		public bool[] grabbingFingers = new bool[5];

		public float grabT;
	}

	private readonly ShadowHand _shadowHand = new ShadowHand();

	private readonly IHandSphereMap _handSphereMap;

	private readonly Handedness _handedness;

	private readonly List<HandSphere> _spheres = new List<HandSphere>();

	private int _totalIterations = 10;

	private int _pushoutIterations = 10;

	public int Iterations;

	private List<int> _sphereHit = new List<int>();

	public ShadowHand ShadowHand => _shadowHand;

	public int TotalIterations
	{
		get
		{
			return _totalIterations;
		}
		set
		{
			_totalIterations = ((_totalIterations <= 0) ? 1 : _totalIterations);
		}
	}

	public int PushoutIterations
	{
		get
		{
			return _pushoutIterations;
		}
		set
		{
			_pushoutIterations = ((_pushoutIterations <= 0) ? 1 : _pushoutIterations);
		}
	}

	public TouchShadowHand(IHandSphereMap map, Handedness handedness, int iterations = 10)
	{
		_handSphereMap = map;
		_handedness = handedness;
		PushoutIterations = (TotalIterations = (Iterations = iterations));
	}

	public void SetShadowRootFromHand(ShadowHand hand)
	{
		Pose root = hand.GetRoot();
		_shadowHand.SetRoot(root);
		_shadowHand.SetRootScale(hand.GetRootScale());
	}

	public void SetShadowRootFromHands(ShadowHand from, ShadowHand to, float t)
	{
		Pose from2 = from.GetRoot();
		Pose to2 = to.GetRoot();
		from2.Lerp(in to2, t);
		_shadowHand.SetRoot(from2);
		_shadowHand.SetRootScale(from.GetRootScale());
	}

	public void SetShadowFingerFrom(int fingerIdx, ShadowHand from)
	{
		HandJointId[] array = FingersMetadata.FINGER_TO_JOINTS[fingerIdx];
		foreach (HandJointId handJointId in array)
		{
			Pose localPose = from.GetLocalPose(handJointId);
			_shadowHand.SetLocalPose(handJointId, localPose);
		}
	}

	private void SetShadowFingerFromLerp(int fingerIdx, ShadowHand from, ShadowHand to, float t)
	{
		HandJointId[] array = FingersMetadata.FINGER_TO_JOINTS[fingerIdx];
		foreach (HandJointId handJointId in array)
		{
			Pose from2 = from.GetLocalPose(handJointId);
			Pose to2 = to.GetLocalPose(handJointId);
			from2.Lerp(in to2, t);
			_shadowHand.SetLocalPose(handJointId, from2);
		}
	}

	private void SetShadowFingerFromLerps(int fingerIdx, ShadowHand from, ShadowHand to, float[] t)
	{
		HandJointId[] array = FingersMetadata.FINGER_TO_JOINTS[fingerIdx];
		for (int i = 0; i < array.Length; i++)
		{
			HandJointId handJointId = array[i];
			Pose from2 = from.GetLocalPose(handJointId);
			Pose to2 = to.GetLocalPose(handJointId);
			from2.Lerp(in to2, t[i]);
			_shadowHand.SetLocalPose(handJointId, from2);
		}
	}

	private void SetShadowFromLerpHands(ShadowHand from, ShadowHand to, float t)
	{
		Pose from2 = from.GetRoot();
		Pose to2 = to.GetRoot();
		from2.Lerp(in to2, t);
		_shadowHand.SetRoot(from2);
		_shadowHand.SetRootScale(from.GetRootScale());
		for (int i = 0; i < 26; i++)
		{
			Pose from3 = from.GetLocalPose((HandJointId)i);
			Pose to3 = to.GetLocalPose((HandJointId)i);
			from3.Lerp(in to3, t);
			_shadowHand.SetLocalPose((HandJointId)i, from3);
		}
	}

	private void LoadSpheresForFingerFromShadow(int fingerIdx, int jointIdx = 0)
	{
		HandJointId[] array = FingersMetadata.FINGER_TO_JOINTS[fingerIdx];
		_spheres.Clear();
		for (int i = jointIdx; i < array.Length; i++)
		{
			HandJointId handJointId = array[i];
			_handSphereMap.GetSpheres(_handedness, handJointId, _shadowHand.GetWorldPose(handJointId), _shadowHand.GetRootScale(), _spheres);
		}
	}

	private void LoadSpheresForHandFromShadow()
	{
		_spheres.Clear();
		for (int i = 0; i < 26; i++)
		{
			HandJointId handJointId = (HandJointId)i;
			_handSphereMap.GetSpheres(_handedness, handJointId, _shadowHand.GetWorldPose(handJointId), _shadowHand.GetRootScale(), _spheres);
		}
	}

	private bool CheckSphereCollision(ColliderGroup colliderGroup, Vector3 offset, List<int> sphereHit = null, List<int> sphereIndices = null)
	{
		bool result = false;
		sphereHit?.Clear();
		for (int i = 0; i < (sphereIndices?.Count ?? _spheres.Count); i++)
		{
			int num = sphereIndices?[i] ?? i;
			HandSphere handSphere = _spheres[num];
			if (!Collisions.IsSphereWithinCollider(handSphere.Position - offset, handSphere.Radius, colliderGroup.Bounds))
			{
				continue;
			}
			for (int j = 0; j < colliderGroup.Colliders.Count; j++)
			{
				if (Collisions.IsSphereWithinCollider(handSphere.Position - offset, handSphere.Radius, colliderGroup.Colliders[j]))
				{
					result = true;
					if (sphereHit == null)
					{
						return true;
					}
					sphereHit.Add(num);
					break;
				}
			}
		}
		return result;
	}

	public bool CheckFingerTouch(int fingerIdx, int jointIdx, ColliderGroup colliderGroup, Vector3 offset, List<int> sphereHit = null)
	{
		LoadSpheresForFingerFromShadow(fingerIdx, jointIdx);
		return CheckSphereCollision(colliderGroup, offset, sphereHit);
	}

	public void CheckTouchFingers(ShadowHand hand, ColliderGroup colliderGroup, GrabTouchInfo result)
	{
		_shadowHand.Copy(hand);
		for (int i = 0; i < 5; i++)
		{
			LoadSpheresForFingerFromShadow(i);
			_sphereHit.Clear();
			if (CheckFingerTouch(i, 0, colliderGroup, Vector3.zero, _sphereHit))
			{
				result.grabbingFingers[i] = true;
			}
		}
	}

	public bool GrabReleaseFinger(int fingerIdx, ShadowHand fromHand, ShadowHand toHand, ColliderGroup colliderGroup, Vector3 offset)
	{
		float num = 1f / (float)TotalIterations;
		float value = 0f;
		while (true)
		{
			value = Mathf.Clamp01(value);
			SetShadowFingerFromLerp(fingerIdx, fromHand, toHand, value);
			LoadSpheresForFingerFromShadow(fingerIdx);
			if (!CheckFingerTouch(fingerIdx, 0, colliderGroup, offset))
			{
				return true;
			}
			if (value == 1f)
			{
				break;
			}
			value += num;
		}
		return false;
	}

	public bool GrabConformFinger(int fingerIdx, ShadowHand fromHand, ShadowHand toHand, ColliderGroup colliderGroup, Vector3 offset)
	{
		float num = 1f / (float)TotalIterations;
		float[] array = new float[FingersMetadata.FINGER_TO_JOINT_INDEX.Length];
		bool[] array2 = new bool[FingersMetadata.FINGER_TO_JOINT_INDEX.Length];
		bool result = false;
		bool flag = false;
		bool flag2 = false;
		int num2 = 0;
		do
		{
			SetShadowFingerFromLerps(fingerIdx, fromHand, toHand, array);
			LoadSpheresForFingerFromShadow(fingerIdx);
			_sphereHit.Clear();
			if (CheckFingerTouch(fingerIdx, num2 + 1, colliderGroup, offset, _sphereHit))
			{
				for (int i = 0; i < _sphereHit.Count; i++)
				{
					HandJointId joint = _spheres[_sphereHit[i]].Joint;
					int num3 = FingersMetadata.JOINT_TO_FINGER_INDEX[(int)joint];
					for (int num4 = num3; num4 >= 0; num4--)
					{
						if (!array2[num4])
						{
							array2[num4] = true;
							result = true;
							if (num2 < num3)
							{
								num2 = num3;
							}
						}
					}
				}
			}
			for (int j = 0; j < array.Length; j++)
			{
				if (!array2[j])
				{
					flag2 = true;
					array[j] += num;
					if (array[j] > 1f)
					{
						array[j] = 1f;
						flag = true;
					}
				}
			}
		}
		while (!(!flag2 || flag));
		SetShadowFingerFromLerps(fingerIdx, fromHand, toHand, array);
		return result;
	}

	public void GrabConformFingers(ShadowHand fromHand, ShadowHand toHand, ColliderGroup colliderGroup, Vector3 offset)
	{
		for (int i = 0; i < 5; i++)
		{
			GrabConformFinger(i, fromHand, toHand, colliderGroup, offset);
		}
	}

	public bool PushoutFinger(int fingerIdx, ShadowHand from, ShadowHand to, ColliderGroup colliderGroup, Vector3 offset)
	{
		float num = 1f / (float)TotalIterations;
		float num2 = 0f;
		while (true)
		{
			if (num2 > 1f)
			{
				num2 = Mathf.Clamp01(num2);
			}
			SetShadowFingerFromLerp(fingerIdx, from, to, num2);
			LoadSpheresForFingerFromShadow(fingerIdx);
			if (!CheckFingerTouch(fingerIdx, 0, colliderGroup, offset))
			{
				return true;
			}
			if (num2 == 1f)
			{
				break;
			}
			num2 += num;
		}
		return false;
	}

	public void GrabTouchStep(ShadowHand from, ShadowHand to, ColliderGroup colliderGroup, int iteration, Vector3 colliderOffset, bool pushout, GrabTouchInfo result)
	{
		if (iteration > TotalIterations)
		{
			return;
		}
		float num = 1f / (float)TotalIterations;
		float num2 = Mathf.Clamp01((float)iteration * num);
		result.offset = colliderOffset;
		Pose root = from.GetRoot();
		Vector3 vector = (to.GetRoot().position - root.position) * num;
		SetShadowFromLerpHands(from, to, num2);
		LoadSpheresForHandFromShadow();
		_sphereHit.Clear();
		for (int i = 0; i < 5; i++)
		{
			result.grabbingFingers[i] = false;
		}
		if (!CheckSphereCollision(colliderGroup, result.offset, _sphereHit))
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		for (int j = 0; j < _sphereHit.Count; j++)
		{
			HandJointId joint = _spheres[_sphereHit[j]].Joint;
			int num3 = (int)HandJointUtils.JointToFingerList[(int)joint];
			if (num3 >= 0)
			{
				result.grabbingFingers[num3] = true;
			}
			if (num3 > 0)
			{
				flag = true;
			}
			else if (num3 == 0)
			{
				flag2 = true;
			}
			else
			{
				flag3 = true;
			}
		}
		if (flag && (flag2 || flag3))
		{
			result.grabbing = true;
			result.grabT = num2;
		}
		else
		{
			if (!pushout)
			{
				return;
			}
			Vector3 vector2 = default(Vector3);
			SetShadowFromLerpHands(from, to, Mathf.Clamp01(num2 + num));
			LoadSpheresForHandFromShadow();
			for (int k = 0; k < _spheres.Count; k++)
			{
				vector2 += _spheres[k].Position / _spheres.Count;
			}
			SetShadowFromLerpHands(from, to, Mathf.Clamp01(num2 - num));
			LoadSpheresForHandFromShadow();
			for (int l = 0; l < _spheres.Count; l++)
			{
				vector2 -= _spheres[l].Position / _spheres.Count;
			}
			float num4 = 0f;
			for (int m = 0; m < _sphereHit.Count; m++)
			{
				num4 += _spheres[_sphereHit[m]].Radius / (float)_sphereHit.Count;
			}
			Vector3 vector3 = num4 * (vector2 - vector).normalized;
			SetShadowFromLerpHands(from, to, num2);
			LoadSpheresForHandFromShadow();
			bool flag4 = false;
			for (int n = 0; n < PushoutIterations; n++)
			{
				result.offset += vector3;
				if (!CheckSphereCollision(colliderGroup, result.offset, null, _sphereHit))
				{
					flag4 = true;
					break;
				}
			}
			if (!flag4)
			{
				result.offset = Vector3.zero;
				result.grabbing = false;
				SetShadowFromLerpHands(from, to, 1f);
			}
		}
	}

	public void GrabTouch(ShadowHand fromHand, ShadowHand toHand, ColliderGroup colliderGroup, bool pushout, GrabTouchInfo result)
	{
		result.grabbing = false;
		result.offset = Vector3.zero;
		for (int i = 0; i <= Iterations; i++)
		{
			GrabTouchStep(fromHand, toHand, colliderGroup, i, result.offset, pushout, result);
			if (result.grabbing)
			{
				break;
			}
		}
	}

	public void GetJointsFromShadow(HandJointId[] jointIds, Pose[] outJoints, bool local)
	{
		for (int i = 0; i < jointIds.Length; i++)
		{
			outJoints[i] = (local ? _shadowHand.GetLocalPose(jointIds[i]) : _shadowHand.GetWorldPose(jointIds[i]));
		}
	}
}
