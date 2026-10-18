using System.Collections.Generic;
using Oculus.Interaction.Input;
using UnityEngine;

namespace Oculus.Interaction;

public class HandFingerMaskGenerator
{
	private static readonly int[] _fingerLinesID = new int[5]
	{
		Shader.PropertyToID("_ThumbLine"),
		Shader.PropertyToID("_IndexLine"),
		Shader.PropertyToID("_MiddleLine"),
		Shader.PropertyToID("_RingLine"),
		Shader.PropertyToID("_PinkyLine")
	};

	private static readonly int[] _palmFingerLinesID = new int[5]
	{
		Shader.PropertyToID("_PalmThumbLine"),
		Shader.PropertyToID("_PalmIndexLine"),
		Shader.PropertyToID("_PalmMiddleLine"),
		Shader.PropertyToID("_PalmRingLine"),
		Shader.PropertyToID("_PalmPinkyLine")
	};

	private static float HandednessMultiplier(Handedness hand)
	{
		if (hand == Handedness.Right)
		{
			return 1f;
		}
		return -1f;
	}

	private static List<Vector2> GenerateModelUV(Handedness handedness, Mesh sharedHandMesh, out Vector2 minPosition, out Vector2 maxPosition)
	{
		List<Vector3> list = new List<Vector3>();
		sharedHandMesh.GetVertices(list);
		minPosition = new Vector2(list[0].x, list[0].z);
		maxPosition = new Vector2(list[0].x, list[0].z);
		for (int i = 0; i < list.Count; i++)
		{
			Vector3 value = list[i] * HandednessMultiplier(handedness);
			Vector2 rhs = new Vector2(value.x, value.z);
			minPosition = Vector2.Min(minPosition, rhs);
			maxPosition = Vector2.Max(maxPosition, rhs);
			list[i] = value;
		}
		List<Vector2> list2 = new List<Vector2>();
		Vector2 vector = maxPosition - minPosition;
		float num = Mathf.Max(vector.x, vector.y);
		foreach (Vector3 item2 in list)
		{
			Vector2 item = (new Vector2(item2.x, item2.z) - minPosition) / num;
			list2.Add(item);
		}
		return list2;
	}

	private static Vector2 GetPositionOnRegion(HandVisual handVisual, HandJointId jointId, Vector2 minRegion, float sideLength)
	{
		IHand hand = handVisual.Hand;
		Pose pose = handVisual.Joints[(int)jointId].GetPose();
		Vector3 vector = handVisual.Root.InverseTransformPoint(pose.position);
		return (new Vector2(vector.x, vector.z) * HandednessMultiplier(hand.Handedness) - minRegion) / sideLength;
	}

	private static Vector4[] GenerateFingerLines(HandVisual handVisual, Vector2 minPosition, float maxLength, float[] lineScale)
	{
		Vector4 vector = GenerateLineData(handVisual, HandJointId.HandThumbTip, HandJointId.HandThumb1, minPosition, maxLength, lineScale[0]);
		Vector4 vector2 = GenerateLineData(handVisual, HandJointId.HandIndexTip, HandJointId.HandIndex1, minPosition, maxLength, lineScale[1]);
		Vector4 vector3 = GenerateLineData(handVisual, HandJointId.HandMiddleTip, HandJointId.HandMiddle1, minPosition, maxLength, lineScale[2]);
		Vector4 vector4 = GenerateLineData(handVisual, HandJointId.HandRingTip, HandJointId.HandRing1, minPosition, maxLength, lineScale[3]);
		Vector4 vector5 = GenerateLineData(handVisual, HandJointId.HandPinkyTip, HandJointId.HandPinky1, minPosition, maxLength, lineScale[4]);
		return new Vector4[5] { vector, vector2, vector3, vector4, vector5 };
	}

	private static Vector4 GenerateLineData(HandVisual handVisual, HandJointId jointIdStart, HandJointId jointIdEnd, Vector2 minRegion, float sideLength, float lineScale)
	{
		Vector2 positionOnRegion = GetPositionOnRegion(handVisual, jointIdStart, minRegion, sideLength);
		Vector2 positionOnRegion2 = GetPositionOnRegion(handVisual, jointIdEnd, minRegion, sideLength);
		positionOnRegion2 = Vector2.LerpUnclamped(positionOnRegion, positionOnRegion2, lineScale);
		return new Vector4(positionOnRegion.x, positionOnRegion.y, positionOnRegion2.x, positionOnRegion2.y);
	}

	private static void SetGlowModelUV(SkinnedMeshRenderer handRenderer, Handedness handedness, out Vector2 minPosition, out Vector2 maxPosition)
	{
		Mesh sharedMesh = handRenderer.sharedMesh;
		List<Vector2> uvs = GenerateModelUV(handedness, sharedMesh, out minPosition, out maxPosition);
		sharedMesh.SetUVs(1, uvs);
		sharedMesh.UploadMeshData(markNoLongerReadable: false);
	}

	private static void SetFingerMaskUniforms(HandVisual handVisual, MaterialPropertyBlock materialPropertyBlock, Vector2 minPosition, Vector2 maxPosition)
	{
		Vector2 vector = maxPosition - minPosition;
		float maxLength = Mathf.Max(vector.x, vector.y);
		float[] lineScale = new float[5] { 0.9f, 0.91f, 0.9f, 0.87f, 0.87f };
		Vector4[] array = GenerateFingerLines(handVisual, minPosition, maxLength, lineScale);
		array[0].z = Mathf.Lerp(array[0].z, array[0].x, 0.3f);
		array[0].x = array[0].z;
		float[] lineScale2 = new float[5] { 1.2f, 1.25f, 1.25f, 1.25f, 1.25f };
		Vector4[] array2 = GenerateFingerLines(handVisual, minPosition, maxLength, lineScale2);
		float num = Mathf.Abs(array2[0].x - array2[0].z) * 0.1f;
		array2[0].z += num;
		for (int i = 0; i < 5; i++)
		{
			materialPropertyBlock.SetVector(_fingerLinesID[i], array[i]);
			materialPropertyBlock.SetVector(_palmFingerLinesID[i], array2[i]);
		}
	}

	public static void GenerateFingerMask(SkinnedMeshRenderer handRenderer, HandVisual handVisual, MaterialPropertyBlock materialPropertyBlock)
	{
		SetGlowModelUV(handRenderer, handVisual.Hand.Handedness, out var minPosition, out var maxPosition);
		SetFingerMaskUniforms(handVisual, materialPropertyBlock, minPosition, maxPosition);
	}
}
