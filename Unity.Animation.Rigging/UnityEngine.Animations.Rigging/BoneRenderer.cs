namespace UnityEngine.Animations.Rigging;

[ExecuteInEditMode]
[AddComponentMenu("Animation Rigging/Setup/Bone Renderer")]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.animation.rigging@1.3/manual/RiggingWorkflow.html#bone-renderer-component")]
public class BoneRenderer : MonoBehaviour
{
	public enum BoneShape
	{
		Line,
		Pyramid,
		Box
	}

	public BoneShape boneShape = BoneShape.Pyramid;

	public bool drawBones = true;

	public bool drawTripods;

	[Range(0.01f, 5f)]
	public float boneSize = 1f;

	[Range(0.01f, 5f)]
	public float tripodSize = 1f;

	public Color boneColor = new Color(0f, 0f, 1f, 0.5f);

	[SerializeField]
	private Transform[] m_Transforms;

	public Transform[] transforms => m_Transforms;
}
