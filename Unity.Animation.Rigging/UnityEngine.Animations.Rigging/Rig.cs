using System.Collections.Generic;

namespace UnityEngine.Animations.Rigging;

[DisallowMultipleComponent]
[AddComponentMenu("Animation Rigging/Setup/Rig")]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.animation.rigging@1.3/manual/RiggingWorkflow.html#rig-component")]
public class Rig : MonoBehaviour, IRigEffectorHolder
{
	[SerializeField]
	[Range(0f, 1f)]
	private float m_Weight = 1f;

	[SerializeField]
	private List<RigEffectorData> m_Effectors = new List<RigEffectorData>();

	public float weight
	{
		get
		{
			return m_Weight;
		}
		set
		{
			m_Weight = Mathf.Clamp01(value);
		}
	}
}
