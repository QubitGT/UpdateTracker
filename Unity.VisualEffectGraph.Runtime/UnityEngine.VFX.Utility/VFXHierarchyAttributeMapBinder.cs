using System.Collections.Generic;

namespace UnityEngine.VFX.Utility;

[AddComponentMenu("VFX/Property Binders/Hierarchy to Attribute Map Binder")]
[VFXBinder("Point Cache/Hierarchy to Attribute Map")]
internal class VFXHierarchyAttributeMapBinder : VFXBinderBase
{
	public enum RadiusMode
	{
		Fixed,
		Interpolate
	}

	private struct Bone
	{
		public Transform source;

		public float sourceRadius;

		public Transform target;

		public float targetRadius;
	}

	[VFXPropertyBinding(new string[] { "System.UInt32" })]
	[SerializeField]
	protected ExposedProperty m_BoneCount = "BoneCount";

	[VFXPropertyBinding(new string[] { "UnityEngine.Texture2D" })]
	[SerializeField]
	protected ExposedProperty m_PositionMap = "PositionMap";

	[VFXPropertyBinding(new string[] { "UnityEngine.Texture2D" })]
	[SerializeField]
	protected ExposedProperty m_TargetPositionMap = "TargetPositionMap";

	[VFXPropertyBinding(new string[] { "UnityEngine.Texture2D" })]
	[SerializeField]
	protected ExposedProperty m_RadiusPositionMap = "RadiusPositionMap";

	public Transform HierarchyRoot;

	public float DefaultRadius = 0.1f;

	public uint MaximumDepth = 3u;

	public RadiusMode Radius;

	private Texture2D position;

	private Texture2D targetPosition;

	private Texture2D radius;

	private List<Bone> bones;

	protected override void OnEnable()
	{
		base.OnEnable();
		UpdateHierarchy();
	}

	private void OnValidate()
	{
		UpdateHierarchy();
	}

	private void UpdateHierarchy()
	{
		bones = ChildrenOf(HierarchyRoot, MaximumDepth);
		int count = bones.Count;
		position = new Texture2D(count, 1, TextureFormat.RGBAHalf, mipChain: false, linear: true);
		targetPosition = new Texture2D(count, 1, TextureFormat.RGBAHalf, mipChain: false, linear: true);
		radius = new Texture2D(count, 1, TextureFormat.RHalf, mipChain: false, linear: true);
		UpdateData();
	}

	private List<Bone> ChildrenOf(Transform source, uint depth)
	{
		List<Bone> list = new List<Bone>();
		if (source == null)
		{
			return list;
		}
		foreach (Transform item in source)
		{
			list.Add(new Bone
			{
				source = source.transform,
				target = item.transform,
				sourceRadius = DefaultRadius,
				targetRadius = DefaultRadius
			});
			if (depth != 0)
			{
				list.AddRange(ChildrenOf(item, depth - 1));
			}
		}
		return list;
	}

	private void UpdateData()
	{
		int count = bones.Count;
		if (position.width == count)
		{
			List<Color> list = new List<Color>();
			List<Color> list2 = new List<Color>();
			List<Color> list3 = new List<Color>();
			for (int i = 0; i < count; i++)
			{
				Bone bone = bones[i];
				list.Add(new Color(bone.source.position.x, bone.source.position.y, bone.source.position.z, 1f));
				list2.Add(new Color(bone.target.position.x, bone.target.position.y, bone.target.position.z, 1f));
				list3.Add(new Color(bone.sourceRadius, 0f, 0f, 1f));
			}
			position.SetPixels(list.ToArray());
			targetPosition.SetPixels(list2.ToArray());
			radius.SetPixels(list3.ToArray());
			position.Apply();
			targetPosition.Apply();
			radius.Apply();
		}
	}

	public override bool IsValid(VisualEffect component)
	{
		if (HierarchyRoot != null && component.HasTexture(m_PositionMap) && component.HasTexture(m_TargetPositionMap) && component.HasTexture(m_RadiusPositionMap))
		{
			return component.HasUInt(m_BoneCount);
		}
		return false;
	}

	public override void UpdateBinding(VisualEffect component)
	{
		UpdateData();
		component.SetTexture(m_PositionMap, position);
		component.SetTexture(m_TargetPositionMap, targetPosition);
		component.SetTexture(m_RadiusPositionMap, radius);
		component.SetUInt(m_BoneCount, (uint)bones.Count);
	}

	public override string ToString()
	{
		return string.Format("Hierarchy: {0} -> {1}", (HierarchyRoot == null) ? "(null)" : HierarchyRoot.name, m_PositionMap);
	}
}
