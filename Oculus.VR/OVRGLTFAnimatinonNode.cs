using System;
using System.Collections.Generic;
using System.Linq;
using OVRSimpleJSON;
using UnityEngine;

public class OVRGLTFAnimatinonNode
{
	private enum ThumbstickDirection
	{
		None,
		North,
		NorthEast,
		East,
		SouthEast,
		South,
		SouthWest,
		West,
		NorthWest
	}

	private enum OVRGLTFTransformType
	{
		None,
		Translation,
		Rotation,
		Scale,
		Weights
	}

	private enum OVRInterpolationType
	{
		None,
		LINEAR,
		STEP,
		CUBICSPLINE
	}

	private struct InputNodeState
	{
		public bool down;

		public float t;

		public Vector2 vecT;
	}

	private OVRGLTFInputNode m_intputNodeType;

	private JSONNode m_jsonData;

	private GameObject m_gameObj;

	private InputNodeState m_inputNodeState;

	private OVRGLTFAnimationNodeMorphTargetHandler m_morphTargetHandler;

	private List<Vector3> m_translations = new List<Vector3>();

	private List<Quaternion> m_rotations = new List<Quaternion>();

	private List<Vector3> m_scales = new List<Vector3>();

	private List<float> m_weights = new List<float>();

	private int m_additiveWeightIndex = -1;

	private static Dictionary<OVRGLTFInputNode, int> InputNodeKeyFrames = new Dictionary<OVRGLTFInputNode, int>
	{
		{
			OVRGLTFInputNode.Button_A_X,
			5
		},
		{
			OVRGLTFInputNode.Button_B_Y,
			8
		},
		{
			OVRGLTFInputNode.Button_Oculus_Menu,
			24
		},
		{
			OVRGLTFInputNode.Trigger_Grip,
			21
		},
		{
			OVRGLTFInputNode.Trigger_Front,
			16
		},
		{
			OVRGLTFInputNode.ThumbStick,
			0
		}
	};

	private static List<int> ThumbStickKeyFrames = new List<int> { 29, 39, 34, 40, 31, 36, 32, 37 };

	private static Vector2[] CardDirections = new Vector2[9]
	{
		new Vector2(0f, 0f),
		new Vector2(0f, 1f),
		new Vector2(1f, 1f),
		new Vector2(1f, 0f),
		new Vector2(1f, -1f),
		new Vector2(0f, -1f),
		new Vector2(-1f, -1f),
		new Vector2(-1f, 0f),
		new Vector2(-1f, 1f)
	};

	public OVRGLTFAnimatinonNode(OVRGLTFInputNode inputNodeType, GameObject gameObj, OVRGLTFAnimationNodeMorphTargetHandler morphTargetHandler)
	{
		m_intputNodeType = inputNodeType;
		m_gameObj = gameObj;
		m_morphTargetHandler = morphTargetHandler;
		m_translations.Add(CloneVector3(m_gameObj.transform.localPosition));
		m_rotations.Add(CloneQuaternion(m_gameObj.transform.localRotation));
		m_scales.Add(CloneVector3(m_gameObj.transform.localScale));
	}

	public void AddChannel(JSONNode channel, JSONNode samplers, OVRGLTFAccessor dataAccessor)
	{
		int asInt = channel["sampler"].AsInt;
		JSONNode jSONNode = channel["target"];
		JSONNode extras = channel["extras"];
		int asInt2 = jSONNode["node"].AsInt;
		OVRGLTFTransformType transformType = GetTransformType(jSONNode["path"].Value);
		ProcessAnimationSampler(samplers[asInt], asInt2, transformType, extras, dataAccessor);
	}

	public void UpdatePose(bool down)
	{
		if (m_inputNodeState.down != down)
		{
			m_inputNodeState.down = down;
			if (m_translations.Count > 1)
			{
				m_gameObj.transform.localPosition = (down ? m_translations[1] : m_translations[0]);
			}
			if (m_rotations.Count > 1)
			{
				m_gameObj.transform.localRotation = (down ? m_rotations[1] : m_rotations[0]);
			}
			if (m_scales.Count > 1)
			{
				SetScale(down ? m_scales[1] : m_scales[0]);
			}
		}
	}

	public void UpdatePose(float t, bool applyDeadZone = true)
	{
		if (applyDeadZone && Math.Abs(m_inputNodeState.t - t) < 0.05f)
		{
			return;
		}
		m_inputNodeState.t = t;
		if (m_translations.Count > 1)
		{
			m_gameObj.transform.localPosition = Vector3.Lerp(m_translations[0], m_translations[1], t);
		}
		if (m_rotations.Count > 1)
		{
			m_gameObj.transform.localRotation = Quaternion.Lerp(m_rotations[0], m_rotations[1], t);
		}
		if (m_scales.Count > 1)
		{
			SetScale(Vector3.Lerp(m_scales[0], m_scales[1], t));
		}
		if (m_morphTargetHandler == null || m_weights.Count <= 0)
		{
			return;
		}
		int num = m_morphTargetHandler.Weights.Length;
		if (m_additiveWeightIndex == -1)
		{
			for (int i = 0; i < num; i++)
			{
				m_morphTargetHandler.Weights[i] = Mathf.Lerp(m_weights[i], m_weights[i + num], t);
			}
		}
		else
		{
			m_morphTargetHandler.Weights[m_additiveWeightIndex] += Mathf.Lerp(m_weights[m_additiveWeightIndex], m_weights[m_additiveWeightIndex + num], t);
		}
		m_morphTargetHandler.MarkModified();
	}

	public void UpdatePose(Vector2 joystick)
	{
		if (Math.Abs((m_inputNodeState.vecT - joystick).magnitude) < 0.05f)
		{
			return;
		}
		m_inputNodeState.vecT.x = joystick.x;
		m_inputNodeState.vecT.y = joystick.y;
		if (m_rotations.Count != 9)
		{
			Debug.LogError("Wrong joystick animation data.");
			return;
		}
		Tuple<ThumbstickDirection, ThumbstickDirection> cardinalThumbsticks = GetCardinalThumbsticks(joystick);
		Vector2 cardinalWeights = GetCardinalWeights(joystick, cardinalThumbsticks);
		Quaternion quaternion = CloneQuaternion(m_rotations[0]);
		for (int i = 0; i < 2; i++)
		{
			float num = cardinalWeights[i];
			if (num != 0f)
			{
				int num2 = (int)(((i == 0) ? cardinalThumbsticks.Item1 : cardinalThumbsticks.Item2) - 1);
				Quaternion b = m_rotations[num2 + 1];
				quaternion = Quaternion.Slerp(quaternion, b, num);
			}
		}
		m_gameObj.transform.localRotation = quaternion;
		if (m_translations.Count > 1 || m_scales.Count > 1)
		{
			Debug.LogWarning("Unsupported pose.");
		}
	}

	private Tuple<ThumbstickDirection, ThumbstickDirection> GetCardinalThumbsticks(Vector2 joystick)
	{
		if (joystick.magnitude < 0.005f)
		{
			return new Tuple<ThumbstickDirection, ThumbstickDirection>(ThumbstickDirection.None, ThumbstickDirection.None);
		}
		if (joystick.x >= 0f)
		{
			if (joystick.y >= 0f)
			{
				if (joystick.y > joystick.x)
				{
					return new Tuple<ThumbstickDirection, ThumbstickDirection>(ThumbstickDirection.North, ThumbstickDirection.NorthEast);
				}
				return new Tuple<ThumbstickDirection, ThumbstickDirection>(ThumbstickDirection.NorthEast, ThumbstickDirection.East);
			}
			if (joystick.x > 0f - joystick.y)
			{
				return new Tuple<ThumbstickDirection, ThumbstickDirection>(ThumbstickDirection.East, ThumbstickDirection.SouthEast);
			}
			return new Tuple<ThumbstickDirection, ThumbstickDirection>(ThumbstickDirection.SouthEast, ThumbstickDirection.South);
		}
		if (joystick.y < 0f)
		{
			if (joystick.x > joystick.y)
			{
				return new Tuple<ThumbstickDirection, ThumbstickDirection>(ThumbstickDirection.South, ThumbstickDirection.SouthWest);
			}
			return new Tuple<ThumbstickDirection, ThumbstickDirection>(ThumbstickDirection.SouthWest, ThumbstickDirection.West);
		}
		if (0f - joystick.x > joystick.y)
		{
			return new Tuple<ThumbstickDirection, ThumbstickDirection>(ThumbstickDirection.West, ThumbstickDirection.NorthWest);
		}
		return new Tuple<ThumbstickDirection, ThumbstickDirection>(ThumbstickDirection.NorthWest, ThumbstickDirection.North);
	}

	private Vector2 GetCardinalWeights(Vector2 joystick, Tuple<ThumbstickDirection, ThumbstickDirection> cardinals)
	{
		if (cardinals.Item1 == ThumbstickDirection.None || cardinals.Item2 == ThumbstickDirection.None)
		{
			return new Vector2(0f, 0f);
		}
		Vector2 vector = CardDirections[(int)cardinals.Item1];
		Vector2 vector2 = CardDirections[(int)cardinals.Item2];
		float num = Vector2.Dot(vector, vector);
		float num2 = Vector2.Dot(vector, vector2);
		float num3 = Vector2.Dot(vector, joystick);
		float num4 = Vector2.Dot(vector2, vector2);
		float num5 = Vector2.Dot(vector2, joystick);
		float num6 = 1f / (num * num4 - num2 * num2);
		float x = (num4 * num3 - num2 * num5) * num6;
		float y = (num * num5 - num2 * num3) * num6;
		return new Vector2(x, y);
	}

	private void ProcessAnimationSampler(JSONNode samplerNode, int nodeId, OVRGLTFTransformType transformType, JSONNode extras, OVRGLTFAccessor _dataAccessor)
	{
		int asInt = samplerNode["output"].AsInt;
		if (ToOVRInterpolationType(samplerNode["interpolation"].Value) == OVRInterpolationType.None)
		{
			Debug.LogError("Unsupported interpolation type: " + samplerNode["interpolation"].Value);
			return;
		}
		int asInt2 = samplerNode["input"].AsInt;
		_dataAccessor.Seek(asInt2);
		float[] array = _dataAccessor.ReadFloat();
		if (array.Length > 2 && m_intputNodeType == OVRGLTFInputNode.None)
		{
			Debug.LogWarning("Unsupported keyframe count");
		}
		_dataAccessor.Seek(asInt);
		switch (transformType)
		{
		case OVRGLTFTransformType.Translation:
			CopyData(ref m_translations, _dataAccessor.ReadVector3(OVRGLTFLoader.GLTFToUnitySpace));
			break;
		case OVRGLTFTransformType.Rotation:
			CopyData(ref m_rotations, _dataAccessor.ReadQuaterion(OVRGLTFLoader.GLTFToUnitySpace_Rotation));
			break;
		case OVRGLTFTransformType.Scale:
			CopyData(ref m_scales, _dataAccessor.ReadVector3(Vector3.one));
			break;
		case OVRGLTFTransformType.Weights:
			CopyData(ref m_weights, _dataAccessor.ReadFloat());
			if (extras != null && extras["additiveWeightIndex"] != null)
			{
				m_additiveWeightIndex = extras["additiveWeightIndex"].AsInt;
			}
			if (m_morphTargetHandler != null)
			{
				m_morphTargetHandler.Weights = new float[m_weights.Count / array.Length];
			}
			break;
		default:
			Debug.LogError("Unsupported transform type: " + transformType);
			break;
		}
	}

	private OVRGLTFTransformType GetTransformType(string transform)
	{
		switch (transform)
		{
		case "translation":
			return OVRGLTFTransformType.Translation;
		case "rotation":
			return OVRGLTFTransformType.Rotation;
		case "scale":
			return OVRGLTFTransformType.Scale;
		case "weights":
			return OVRGLTFTransformType.Weights;
		case "none":
			return OVRGLTFTransformType.None;
		default:
			Debug.LogError("Unsupported transform type: " + transform);
			return OVRGLTFTransformType.None;
		}
	}

	private OVRInterpolationType ToOVRInterpolationType(string interpolationType)
	{
		switch (interpolationType)
		{
		case "LINEAR":
			return OVRInterpolationType.LINEAR;
		case "STEP":
			Debug.LogError("Unsupported interpolationType type." + interpolationType);
			return OVRInterpolationType.STEP;
		case "CUBICSPLINE":
			Debug.LogError("Unsupported interpolationType type." + interpolationType);
			return OVRInterpolationType.CUBICSPLINE;
		default:
			Debug.LogError("Unsupported interpolationType type." + interpolationType);
			return OVRInterpolationType.None;
		}
	}

	private void CopyData<T>(ref List<T> dest, T[] src)
	{
		if (m_intputNodeType == OVRGLTFInputNode.None)
		{
			dest = src.ToList();
			return;
		}
		if (m_intputNodeType == OVRGLTFInputNode.ThumbStick)
		{
			foreach (int thumbStickKeyFrame in ThumbStickKeyFrames)
			{
				if (thumbStickKeyFrame < src.Length)
				{
					dest.Add(src[thumbStickKeyFrame]);
				}
			}
			return;
		}
		int num = InputNodeKeyFrames[m_intputNodeType];
		if (num < src.Length)
		{
			dest.Add(src[num]);
		}
	}

	private Vector3 CloneVector3(Vector3 v)
	{
		return new Vector3(v.x, v.y, v.z);
	}

	private Quaternion CloneQuaternion(Quaternion q)
	{
		return new Quaternion(q.x, q.y, q.z, q.w);
	}

	private void SetScale(Vector3 scale)
	{
		m_gameObj.transform.localScale = scale;
		m_gameObj.SetActive(m_gameObj.transform.localScale != Vector3.zero);
	}
}
