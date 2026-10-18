using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine.Serialization;

namespace UnityEngine.Splines;

[ExecuteInEditMode]
[AddComponentMenu("Splines/Spline Instantiate")]
public class SplineInstantiate : SplineComponent
{
	public enum OffsetSpace
	{
		[InspectorName("Spline Element")]
		Spline,
		[InspectorName("Spline Object")]
		Local,
		[InspectorName("World Space")]
		World,
		[InspectorName("Instantiated Object")]
		Object
	}

	[Serializable]
	internal struct Vector3Offset
	{
		[Flags]
		public enum Setup
		{
			None = 0,
			HasOffset = 1,
			HasCustomSpace = 2
		}

		public Setup setup;

		public Vector3 min;

		public Vector3 max;

		public bool randomX;

		public bool randomY;

		public bool randomZ;

		public OffsetSpace space;

		public bool hasOffset => (setup & Setup.HasOffset) != 0;

		public bool hasCustomSpace => (setup & Setup.HasCustomSpace) != 0;

		internal Vector3 GetNextOffset()
		{
			if ((setup & Setup.HasOffset) != 0)
			{
				return new Vector3(randomX ? Random.Range(min.x, max.x) : min.x, randomY ? Random.Range(min.y, max.y) : min.y, randomZ ? Random.Range(min.z, max.z) : min.z);
			}
			return Vector3.zero;
		}

		internal void CheckMinMaxValidity()
		{
			max.x = Mathf.Max(min.x, max.x);
			max.y = Mathf.Max(min.y, max.y);
			max.z = Mathf.Max(min.z, max.z);
		}

		internal void CheckMinMax()
		{
			CheckMinMaxValidity();
			if (max.magnitude > 0f)
			{
				setup |= Setup.HasOffset;
			}
			else
			{
				setup &= ~Setup.HasOffset;
			}
		}

		internal void CheckCustomSpace(Space instanceSpace)
		{
			if (space == (OffsetSpace)instanceSpace)
			{
				setup &= ~Setup.HasCustomSpace;
			}
			else
			{
				setup |= Setup.HasCustomSpace;
			}
		}
	}

	[Serializable]
	public struct InstantiableItem
	{
		[HideInInspector]
		[Obsolete("Use Prefab instead.", false)]
		public GameObject prefab;

		[FormerlySerializedAs("prefab")]
		public GameObject Prefab;

		[HideInInspector]
		[Obsolete("Use Probability instead.", false)]
		public float probability;

		[FormerlySerializedAs("probability")]
		public float Probability;
	}

	public enum Method
	{
		[InspectorName("Instance Count")]
		InstanceCount,
		[InspectorName("Spline Distance")]
		SpacingDistance,
		[InspectorName("Linear Distance")]
		LinearDistance
	}

	public enum Space
	{
		[InspectorName("Spline Element")]
		Spline,
		[InspectorName("Spline Object")]
		Local,
		[InspectorName("World Space")]
		World
	}

	[SerializeField]
	private SplineContainer m_Container;

	[SerializeField]
	private List<InstantiableItem> m_ItemsToInstantiate = new List<InstantiableItem>();

	[SerializeField]
	private Method m_Method = Method.SpacingDistance;

	[SerializeField]
	private Space m_Space;

	[SerializeField]
	private Vector2 m_Spacing = new Vector2(1f, 1f);

	[SerializeField]
	private AlignAxis m_Up = AlignAxis.YAxis;

	[SerializeField]
	private AlignAxis m_Forward = AlignAxis.ZAxis;

	[SerializeField]
	private Vector3Offset m_PositionOffset;

	[SerializeField]
	private Vector3Offset m_RotationOffset;

	[SerializeField]
	private Vector3Offset m_ScaleOffset;

	[SerializeField]
	[HideInInspector]
	[FormerlySerializedAs("m_Instances")]
	private List<GameObject> m_DeprecatedInstances = new List<GameObject>();

	private const string k_InstancesRootName = "root-";

	private GameObject m_InstancesRoot;

	private readonly List<GameObject> m_Instances = new List<GameObject>();

	private bool m_InstancesCacheDirty;

	[SerializeField]
	private bool m_AutoRefresh = true;

	private InstantiableItem m_CurrentItem;

	private bool m_SplineDirty;

	private float m_MaxProbability = 1f;

	[SerializeField]
	private int m_Seed;

	private List<float> m_TimesCache = new List<float>();

	private List<float> m_LengthsCache = new List<float>();

	[Obsolete("Use Container instead.", false)]
	public SplineContainer container => Container;

	public SplineContainer Container
	{
		get
		{
			return m_Container;
		}
		set
		{
			m_Container = value;
		}
	}

	public InstantiableItem[] itemsToInstantiate
	{
		get
		{
			return m_ItemsToInstantiate.ToArray();
		}
		set
		{
			m_DeprecatedInstances.AddRange(m_Instances);
			m_ItemsToInstantiate.Clear();
			m_ItemsToInstantiate.AddRange(value);
		}
	}

	[Obsolete("Use InstantiateMethod instead.", false)]
	public Method method => InstantiateMethod;

	public Method InstantiateMethod
	{
		get
		{
			return m_Method;
		}
		set
		{
			m_Method = value;
		}
	}

	[Obsolete("Use CoordinateSpace instead.", false)]
	public Space space => CoordinateSpace;

	public Space CoordinateSpace
	{
		get
		{
			return m_Space;
		}
		set
		{
			m_Space = value;
		}
	}

	public float MinSpacing
	{
		get
		{
			return m_Spacing.x;
		}
		set
		{
			m_Spacing = new Vector2(value, m_Spacing.y);
			ValidateSpacing();
		}
	}

	public float MaxSpacing
	{
		get
		{
			return m_Spacing.y;
		}
		set
		{
			m_Spacing = new Vector2(m_Spacing.x, value);
			ValidateSpacing();
		}
	}

	[Obsolete("Use UpAxis instead.", false)]
	public AlignAxis upAxis => UpAxis;

	public AlignAxis UpAxis
	{
		get
		{
			return m_Up;
		}
		set
		{
			m_Up = value;
		}
	}

	[Obsolete("Use ForwardAxis instead.", false)]
	public AlignAxis forwardAxis => ForwardAxis;

	public AlignAxis ForwardAxis
	{
		get
		{
			return m_Forward;
		}
		set
		{
			m_Forward = value;
			ValidateAxis();
		}
	}

	[Obsolete("Use MinPositionOffset instead.", false)]
	public Vector3 minPositionOffset => MinPositionOffset;

	public Vector3 MinPositionOffset
	{
		get
		{
			return m_PositionOffset.min;
		}
		set
		{
			m_PositionOffset.min = value;
			m_PositionOffset.CheckMinMax();
		}
	}

	[Obsolete("Use MaxPositionOffset instead.", false)]
	public Vector3 maxPositionOffset => MaxPositionOffset;

	public Vector3 MaxPositionOffset
	{
		get
		{
			return m_PositionOffset.max;
		}
		set
		{
			m_PositionOffset.max = value;
			m_PositionOffset.CheckMinMax();
		}
	}

	[Obsolete("Use PositionSpace instead.", false)]
	public OffsetSpace positionSpace => PositionSpace;

	public OffsetSpace PositionSpace
	{
		get
		{
			return m_PositionOffset.space;
		}
		set
		{
			m_PositionOffset.space = value;
			m_PositionOffset.CheckCustomSpace(m_Space);
		}
	}

	[Obsolete("Use MinRotationOffset instead.", false)]
	public Vector3 minRotationOffset => MinRotationOffset;

	public Vector3 MinRotationOffset
	{
		get
		{
			return m_RotationOffset.min;
		}
		set
		{
			m_RotationOffset.min = value;
			m_RotationOffset.CheckMinMax();
		}
	}

	[Obsolete("Use MaxRotationOffset instead.", false)]
	public Vector3 maxRotationOffset => MaxRotationOffset;

	public Vector3 MaxRotationOffset
	{
		get
		{
			return m_RotationOffset.max;
		}
		set
		{
			m_RotationOffset.max = value;
			m_RotationOffset.CheckMinMax();
		}
	}

	[Obsolete("Use RotationSpace instead.", false)]
	public OffsetSpace rotationSpace => RotationSpace;

	public OffsetSpace RotationSpace
	{
		get
		{
			return m_RotationOffset.space;
		}
		set
		{
			m_RotationOffset.space = value;
			m_RotationOffset.CheckCustomSpace(m_Space);
		}
	}

	[Obsolete("Use MinScaleOffset instead.", false)]
	public Vector3 minScaleOffset => MinScaleOffset;

	public Vector3 MinScaleOffset
	{
		get
		{
			return m_ScaleOffset.min;
		}
		set
		{
			m_ScaleOffset.min = value;
			m_ScaleOffset.CheckMinMax();
		}
	}

	[Obsolete("Use MaxScaleOffset instead.", false)]
	public Vector3 maxScaleOffset => MaxScaleOffset;

	public Vector3 MaxScaleOffset
	{
		get
		{
			return m_ScaleOffset.max;
		}
		set
		{
			m_ScaleOffset.max = value;
			m_ScaleOffset.CheckMinMax();
		}
	}

	[Obsolete("Use ScaleSpace instead.", false)]
	public OffsetSpace scaleSpace => ScaleSpace;

	public OffsetSpace ScaleSpace
	{
		get
		{
			return m_ScaleOffset.space;
		}
		set
		{
			m_ScaleOffset.space = value;
			m_ScaleOffset.CheckCustomSpace(m_Space);
		}
	}

	internal GameObject InstancesRoot => m_InstancesRoot;

	private Transform instancesRootTransform
	{
		get
		{
			if (m_InstancesRoot == null)
			{
				m_InstancesRoot = new GameObject("root-" + GetInstanceID());
				m_InstancesRoot.hideFlags |= HideFlags.HideAndDontSave;
				m_InstancesRoot.transform.parent = base.transform;
				m_InstancesRoot.transform.localPosition = Vector3.zero;
				m_InstancesRoot.transform.localRotation = Quaternion.identity;
			}
			return m_InstancesRoot.transform;
		}
	}

	internal List<GameObject> instances => m_Instances;

	private float maxProbability
	{
		get
		{
			return m_MaxProbability;
		}
		set
		{
			if (m_MaxProbability != value)
			{
				m_MaxProbability = value;
				m_InstancesCacheDirty = true;
			}
		}
	}

	public int Seed
	{
		get
		{
			return m_Seed;
		}
		set
		{
			m_Seed = value;
			m_InstancesCacheDirty = true;
		}
	}

	private void OnEnable()
	{
		if (m_Seed == 0)
		{
			m_Seed = GetInstanceID();
		}
		CheckChildrenValidity();
		Spline.Changed += OnSplineChanged;
		UpdateInstances();
	}

	private void OnDisable()
	{
		Spline.Changed -= OnSplineChanged;
		Clear();
	}

	private void UndoRedoPerformed()
	{
		m_InstancesCacheDirty = true;
		m_SplineDirty = true;
	}

	private void OnValidate()
	{
		ValidateSpacing();
		m_SplineDirty = m_AutoRefresh;
		EnsureItemsValidity();
		m_PositionOffset.CheckMinMaxValidity();
		m_RotationOffset.CheckMinMaxValidity();
		m_ScaleOffset.CheckMinMaxValidity();
	}

	private void EnsureItemsValidity()
	{
		float num = 0f;
		for (int i = 0; i < m_ItemsToInstantiate.Count; i++)
		{
			InstantiableItem value = m_ItemsToInstantiate[i];
			if (value.Prefab != null)
			{
				if (base.transform.IsChildOf(value.Prefab.transform))
				{
					Debug.LogWarning("Instantiating a parent of the SplineInstantiate object itself is not permitted (" + value.Prefab.name + " is a parent of " + base.transform.gameObject.name + ").", this);
					value.Prefab = null;
					m_ItemsToInstantiate[i] = value;
				}
				else
				{
					num += value.Probability;
				}
			}
		}
		maxProbability = num;
	}

	private void CheckChildrenValidity()
	{
		List<int> list = (from sInstantiate in GetComponents<SplineInstantiate>()
			select sInstantiate.GetInstanceID()).ToList();
		for (int num = base.transform.childCount - 1; num >= 0; num--)
		{
			GameObject gameObject = base.transform.GetChild(num).gameObject;
			if (gameObject.name.StartsWith("root-"))
			{
				bool flag = true;
				foreach (int item in list)
				{
					if (gameObject.name.Equals("root-" + item))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					Object.Destroy(gameObject);
				}
			}
		}
	}

	private void ValidateSpacing()
	{
		float num = Mathf.Max(0.1f, m_Spacing.x);
		if (m_Method != Method.LinearDistance)
		{
			float b = (float.IsNaN(m_Spacing.y) ? num : Mathf.Max(0.1f, m_Spacing.y));
			m_Spacing = new Vector2(num, Mathf.Max(num, b));
		}
		else if (m_Method == Method.LinearDistance)
		{
			float y = (float.IsNaN(m_Spacing.y) ? m_Spacing.y : num);
			m_Spacing = new Vector2(num, y);
		}
	}

	private void ValidateAxis()
	{
		if (m_Forward == m_Up || m_Forward == (AlignAxis)((int)(m_Up + 3) % 6))
		{
			m_Forward = (AlignAxis)((int)(m_Forward + 1) % 6);
		}
	}

	internal void SetSplineDirty(Spline spline)
	{
		if (m_Container != null && m_Container.Splines.Contains(spline) && m_AutoRefresh)
		{
			UpdateInstances();
		}
	}

	private void InitContainer()
	{
		if (m_Container == null)
		{
			m_Container = GetComponent<SplineContainer>();
		}
	}

	public void Clear()
	{
		SetDirty();
		TryClearCache();
	}

	public void SetDirty()
	{
		m_InstancesCacheDirty = true;
	}

	private void TryClearCache()
	{
		if (!m_InstancesCacheDirty)
		{
			for (int i = 0; i < m_Instances.Count; i++)
			{
				if (m_Instances[i] == null)
				{
					m_InstancesCacheDirty = true;
					break;
				}
			}
		}
		if (m_InstancesCacheDirty)
		{
			for (int num = m_Instances.Count - 1; num >= 0; num--)
			{
				Object.Destroy(m_Instances[num]);
			}
			Object.Destroy(m_InstancesRoot);
			m_Instances.Clear();
			m_InstancesCacheDirty = false;
		}
	}

	private void ClearDeprecatedInstances()
	{
		foreach (GameObject deprecatedInstance in m_DeprecatedInstances)
		{
			Object.Destroy(deprecatedInstance);
		}
		m_DeprecatedInstances.Clear();
	}

	public void Randomize()
	{
		Seed = Random.Range(int.MinValue, int.MaxValue);
		m_SplineDirty = true;
	}

	private void Update()
	{
		if (m_SplineDirty)
		{
			UpdateInstances();
		}
	}

	public void UpdateInstances()
	{
		ClearDeprecatedInstances();
		TryClearCache();
		if (m_Container == null)
		{
			InitContainer();
		}
		if (m_Container == null || m_Container.Splines.Count == 0 || m_ItemsToInstantiate.Count == 0)
		{
			return;
		}
		Random.State state = Random.state;
		Random.InitState(m_Seed);
		int num = 0;
		int num2 = 0;
		m_LengthsCache.Clear();
		float num3 = 0f;
		float num4 = 0f;
		for (int i = 0; i < m_Container.Splines.Count; i++)
		{
			float num5 = m_Container.CalculateLength(i);
			m_LengthsCache.Add(num5);
			num4 += num5;
		}
		float num6 = Random.Range(m_Spacing.x, m_Spacing.y);
		float num7 = 0f;
		float num8 = 0f;
		if (m_Method == Method.InstanceCount)
		{
			if (num6 == 1f)
			{
				num7 = num4 / 2f;
			}
			else if (num6 < 1f)
			{
				num7 = num4 + 1f;
			}
			num8 = ((m_Container.Splines.Count != 1) ? (num4 / (float)((int)num6 - 1)) : (num4 / (float)(m_Container.Splines[0].Closed ? ((int)num6) : ((int)num6 - 1))));
		}
		EnsureItemsValidity();
		for (int j = 0; j < m_Container.Splines.Count; j++)
		{
			Spline spline = m_Container.Splines[j];
			using NativeSpline spline2 = new NativeSpline(spline, m_Container.transform.localToWorldMatrix, Allocator.TempJob);
			num3 = m_LengthsCache[j];
			bool flag = false;
			if (m_Method == Method.InstanceCount)
			{
				if (num7 > num3 + 0.001f && num7 <= num4 + 0.001f)
				{
					num7 -= num3;
					flag = true;
				}
			}
			else
			{
				num7 = 0f;
			}
			m_TimesCache.Clear();
			int num9 = 0;
			while (num7 <= num3 + 0.001f && !flag && SpawnPrefab(num))
			{
				m_TimesCache.Add(num7 / num3);
				if (m_Method == Method.SpacingDistance)
				{
					num6 = Random.Range(m_Spacing.x, m_Spacing.y);
					num7 += num6;
				}
				else if (m_Method == Method.InstanceCount)
				{
					if (num6 > 1f)
					{
						float num10 = num7;
						num7 += num8;
						if (num10 < num3 && num7 > num3 + 0.001f)
						{
							num7 -= num3;
							flag = true;
						}
					}
					else
					{
						num7 += num4;
					}
				}
				else if (m_Method == Method.LinearDistance)
				{
					if (float.IsNaN(m_Spacing.y))
					{
						MeshFilter meshFilter = m_Instances[num].GetComponent<MeshFilter>();
						Vector3 vector = Vector3.right;
						if (m_Forward == AlignAxis.ZAxis || m_Forward == AlignAxis.NegativeZAxis)
						{
							vector = Vector3.forward;
						}
						if (m_Forward == AlignAxis.YAxis || m_Forward == AlignAxis.NegativeYAxis)
						{
							vector = Vector3.up;
						}
						if (meshFilter == null)
						{
							meshFilter = m_Instances[num].GetComponentInChildren<MeshFilter>();
							if (meshFilter != null)
							{
								vector = Vector3.Scale(meshFilter.transform.InverseTransformDirection(m_Instances[num].transform.TransformDirection(vector)), meshFilter.transform.lossyScale);
							}
						}
						if (meshFilter != null)
						{
							Bounds bounds = meshFilter.sharedMesh.bounds;
							MeshFilter[] componentsInChildren = meshFilter.GetComponentsInChildren<MeshFilter>();
							for (int k = 0; k < componentsInChildren.Length; k++)
							{
								Bounds bounds2 = componentsInChildren[k].sharedMesh.bounds;
								bounds.size = new Vector3(Mathf.Max(bounds.size.x, bounds2.size.x), Mathf.Max(bounds.size.z, bounds2.size.z), Mathf.Max(bounds.size.z, bounds2.size.z));
							}
							num6 = Vector3.Scale(bounds.size, vector).magnitude;
						}
					}
					else
					{
						num6 = Random.Range(m_Spacing.x, m_Spacing.y);
					}
					spline2.GetPointAtLinearDistance(m_TimesCache[num9], num6, out var resultPointT);
					num7 = ((resultPointT >= 1f) ? (num3 + 1f) : (resultPointT * num3));
				}
				num++;
				num9++;
			}
			for (int num11 = m_Instances.Count - 1; num11 >= num; num11--)
			{
				if (m_Instances[num11] != null)
				{
					Object.Destroy(m_Instances[num11]);
					m_Instances.RemoveAt(num11);
				}
			}
			for (int l = num2; l < num; l++)
			{
				GameObject gameObject = m_Instances[l];
				float t = m_TimesCache[l - num2];
				spline2.Evaluate(t, out var position, out var tangent, out var upVector);
				gameObject.transform.position = position;
				if (m_Method == Method.LinearDistance)
				{
					tangent = spline2.EvaluatePosition((l + 1 < num) ? m_TimesCache[l + 1 - num2] : 1f) - position;
				}
				float3 @float = math.normalizesafe(upVector);
				float3 float2 = math.normalizesafe(tangent);
				if (m_Space == Space.World)
				{
					@float = Vector3.up;
					float2 = Vector3.forward;
				}
				else if (m_Space == Space.Local)
				{
					@float = base.transform.TransformDirection(Vector3.up);
					float2 = base.transform.TransformDirection(Vector3.forward);
				}
				float3 forward = math.normalizesafe(GetAxis(m_Forward));
				float3 up = math.normalizesafe(GetAxis(m_Up));
				Quaternion quaternion = Quaternion.Inverse(Unity.Mathematics.quaternion.LookRotationSafe(forward, up));
				gameObject.transform.rotation = Unity.Mathematics.quaternion.LookRotationSafe(float2, @float) * quaternion;
				float3 customUp = @float;
				float3 customForward = float2;
				if (m_PositionOffset.hasOffset)
				{
					if (m_PositionOffset.hasCustomSpace)
					{
						GetCustomSpaceAxis(m_PositionOffset.space, upVector, tangent, gameObject.transform, out customUp, out customForward);
					}
					Vector3 nextOffset = m_PositionOffset.GetNextOffset();
					Vector3 normalized = Vector3.Cross(customUp, customForward).normalized;
					gameObject.transform.position += nextOffset.x * normalized + nextOffset.y * (Vector3)customUp + nextOffset.z * (Vector3)customForward;
				}
				if (m_ScaleOffset.hasOffset)
				{
					customUp = @float;
					customForward = float2;
					if (m_ScaleOffset.hasCustomSpace)
					{
						GetCustomSpaceAxis(m_ScaleOffset.space, upVector, tangent, gameObject.transform, out customUp, out customForward);
					}
					customUp = gameObject.transform.InverseTransformDirection(customUp).normalized;
					customForward = gameObject.transform.InverseTransformDirection(customForward).normalized;
					Vector3 nextOffset2 = m_ScaleOffset.GetNextOffset();
					Vector3 normalized2 = Vector3.Cross(customUp, customForward).normalized;
					gameObject.transform.localScale += nextOffset2.x * normalized2 + nextOffset2.y * (Vector3)customUp + nextOffset2.z * (Vector3)customForward;
				}
				if (!m_RotationOffset.hasOffset)
				{
					continue;
				}
				customUp = @float;
				customForward = float2;
				if (m_RotationOffset.hasCustomSpace)
				{
					GetCustomSpaceAxis(m_RotationOffset.space, upVector, tangent, gameObject.transform, out customUp, out customForward);
					if (m_RotationOffset.space == OffsetSpace.Object)
					{
						quaternion = Unity.Mathematics.quaternion.identity;
					}
				}
				Vector3 nextOffset3 = m_RotationOffset.GetNextOffset();
				Vector3 normalized3 = Vector3.Cross(customUp, customForward).normalized;
				customForward = Quaternion.AngleAxis(nextOffset3.y, customUp) * Quaternion.AngleAxis(nextOffset3.x, normalized3) * customForward;
				customUp = Quaternion.AngleAxis(nextOffset3.x, normalized3) * Quaternion.AngleAxis(nextOffset3.z, customForward) * customUp;
				gameObject.transform.rotation = Unity.Mathematics.quaternion.LookRotationSafe(customForward, customUp) * quaternion;
			}
			num2 = num;
		}
		m_SplineDirty = false;
		Random.state = state;
	}

	private bool SpawnPrefab(int index)
	{
		int index2 = ((m_ItemsToInstantiate.Count != 1) ? GetPrefabIndex() : 0);
		m_CurrentItem = m_ItemsToInstantiate[index2];
		if (m_CurrentItem.Prefab == null)
		{
			return false;
		}
		if (index >= m_Instances.Count)
		{
			m_Instances.Add(Object.Instantiate(m_CurrentItem.Prefab, instancesRootTransform));
		}
		m_Instances[index].transform.localPosition = m_CurrentItem.Prefab.transform.localPosition;
		m_Instances[index].transform.localRotation = m_CurrentItem.Prefab.transform.localRotation;
		m_Instances[index].transform.localScale = m_CurrentItem.Prefab.transform.localScale;
		return true;
	}

	private void GetCustomSpaceAxis(OffsetSpace space, float3 splineUp, float3 direction, Transform instanceTransform, out float3 customUp, out float3 customForward)
	{
		customUp = Vector3.up;
		customForward = Vector3.forward;
		switch (space)
		{
		case OffsetSpace.Local:
			customUp = base.transform.TransformDirection(Vector3.up);
			customForward = base.transform.TransformDirection(Vector3.forward);
			break;
		case OffsetSpace.Spline:
			customUp = splineUp;
			customForward = direction;
			break;
		case OffsetSpace.Object:
			customUp = instanceTransform.TransformDirection(Vector3.up);
			customForward = instanceTransform.TransformDirection(Vector3.forward);
			break;
		}
	}

	private int GetPrefabIndex()
	{
		float num = Random.Range(0f, m_MaxProbability);
		float num2 = 0f;
		for (int i = 0; i < m_ItemsToInstantiate.Count; i++)
		{
			if (!(m_ItemsToInstantiate[i].Prefab == null))
			{
				float probability = m_ItemsToInstantiate[i].Probability;
				if (num < num2 + probability)
				{
					return i;
				}
				num2 += probability;
			}
		}
		return 0;
	}

	private void OnSplineChanged(Spline spline, int knotIndex, SplineModification modificationType)
	{
		if (m_Container != null && m_Container.Spline == spline)
		{
			m_SplineDirty = m_AutoRefresh;
		}
	}
}
