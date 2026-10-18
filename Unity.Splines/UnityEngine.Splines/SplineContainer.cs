using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Unity.Collections;
using Unity.Mathematics;

namespace UnityEngine.Splines;

[AddComponentMenu("Splines/Spline Container")]
[ExecuteAlways]
public sealed class SplineContainer : MonoBehaviour, ISplineContainer, ISerializationCallbackReceiver
{
	private struct SplineToNative
	{
		public ISpline spline;

		public NativeSpline nativeSpline;
	}

	private const string k_IconPath = "Packages/com.unity.splines/Editor/Editor Resources/Icons/SplineComponent.png";

	[SerializeField]
	[Obsolete]
	[HideInInspector]
	private Spline m_Spline;

	[SerializeField]
	private Spline[] m_Splines = new Spline[1]
	{
		new Spline()
	};

	[SerializeField]
	private KnotLinkCollection m_Knots = new KnotLinkCollection();

	private List<(int previousIndex, int newIndex)> m_ReorderedSplinesIndices = new List<(int, int)>();

	private List<int> m_RemovedSplinesIndices = new List<int>();

	private List<int> m_AddedSplinesIndices = new List<int>();

	private ReadOnlyCollection<Spline> m_ReadOnlySplines;

	private Dictionary<ISpline, NativeSpline> m_NativeSplinesCache = new Dictionary<ISpline, NativeSpline>();

	private float4x4 m_NativeSplinesCacheTransform = float4x4.identity;

	private static List<SplineToNative> s_AllocPreventionHelperBuffer = new List<SplineToNative>(32);

	public IReadOnlyList<Spline> Splines
	{
		get
		{
			return m_ReadOnlySplines ?? (m_ReadOnlySplines = new ReadOnlyCollection<Spline>(m_Splines));
		}
		set
		{
			if (value == null)
			{
				m_Splines = Array.Empty<Spline>();
				return;
			}
			ClearCaches();
			DisposeNativeSplinesCache();
			for (int j = 0; j < m_Splines.Length; j++)
			{
				int num = IndexOf(value, m_Splines[j]);
				if (num == -1)
				{
					m_RemovedSplinesIndices.Add(j);
				}
				else if (num != j)
				{
					m_ReorderedSplinesIndices.Add((j, num));
				}
			}
			int i;
			for (i = 0; i < value.Count; i++)
			{
				if (Array.FindIndex(m_Splines, (Spline spline) => spline == value[i]) == -1)
				{
					m_AddedSplinesIndices.Add(i);
				}
			}
			m_Splines = new Spline[value.Count];
			for (int k = 0; k < m_Splines.Length; k++)
			{
				m_Splines[k] = value[k];
				if (IsNonUniformlyScaled)
				{
					GetOrBakeNativeSpline(m_Splines[k]);
				}
			}
			m_ReadOnlySplines = new ReadOnlyCollection<Spline>(m_Splines);
			foreach (int removedSplinesIndex in m_RemovedSplinesIndices)
			{
				SplineContainer.SplineRemoved?.Invoke(this, removedSplinesIndex);
			}
			foreach (int addedSplinesIndex in m_AddedSplinesIndices)
			{
				SplineContainer.SplineAdded?.Invoke(this, addedSplinesIndex);
			}
			foreach (var reorderedSplinesIndex in m_ReorderedSplinesIndices)
			{
				SplineContainer.SplineReordered?.Invoke(this, reorderedSplinesIndex.previousIndex, reorderedSplinesIndex.newIndex);
			}
		}
	}

	public KnotLinkCollection KnotLinkCollection => m_Knots;

	public Spline this[int index] => m_Splines[index];

	private bool IsNonUniformlyScaled
	{
		get
		{
			float3 @float = base.transform.lossyScale;
			return !math.all(@float == @float.x);
		}
	}

	public Spline Spline
	{
		get
		{
			if (m_Splines.Length == 0)
			{
				return null;
			}
			return m_Splines[0];
		}
		set
		{
			if (m_Splines.Length != 0)
			{
				m_Splines[0] = value;
			}
		}
	}

	public static event Action<SplineContainer, int> SplineAdded;

	public static event Action<SplineContainer, int> SplineRemoved;

	public static event Action<SplineContainer, int, int> SplineReordered;

	private static int IndexOf(IReadOnlyList<Spline> self, Spline elementToFind)
	{
		for (int i = 0; i < self.Count; i++)
		{
			if (self[i] == elementToFind)
			{
				return i;
			}
		}
		return -1;
	}

	~SplineContainer()
	{
	}

	private void OnEnable()
	{
		Spline.Changed += OnSplineChanged;
	}

	private void OnDisable()
	{
		Spline.Changed -= OnSplineChanged;
	}

	private void OnDestroy()
	{
		DisposeNativeSplinesCache();
	}

	public void Warmup()
	{
		for (int i = 0; i < Splines.Count; i++)
		{
			Spline spline = Splines[i];
			spline.Warmup();
			GetOrBakeNativeSpline(spline);
		}
	}

	internal void ClearCaches()
	{
		m_ReorderedSplinesIndices.Clear();
		m_RemovedSplinesIndices.Clear();
		m_AddedSplinesIndices.Clear();
		m_ReadOnlySplines = null;
	}

	private void DisposeNativeSplinesCache()
	{
		foreach (KeyValuePair<ISpline, NativeSpline> item in m_NativeSplinesCache)
		{
			item.Value.Dispose();
		}
		m_NativeSplinesCache.Clear();
	}

	private void OnSplineChanged(Spline spline, int index, SplineModification modificationType)
	{
		int num = Array.IndexOf(m_Splines, spline);
		if (num >= 0)
		{
			switch (modificationType)
			{
			case SplineModification.KnotModified:
				this.SetLinkedKnotPosition(new SplineKnotIndex(num, index));
				break;
			case SplineModification.KnotInserted:
			case SplineModification.KnotReordered:
				m_Knots.KnotInserted(num, index);
				break;
			case SplineModification.KnotRemoved:
				m_Knots.KnotRemoved(num, index);
				break;
			}
			if (m_NativeSplinesCache.TryGetValue(spline, out var value))
			{
				value.Dispose();
			}
			m_NativeSplinesCache.Remove(spline);
		}
	}

	private void OnKnotModified(Spline spline, int index)
	{
		int num = Array.IndexOf(m_Splines, spline);
		if (num >= 0)
		{
			this.SetLinkedKnotPosition(new SplineKnotIndex(num, index));
		}
	}

	public bool Evaluate(float t, out float3 position, out float3 tangent, out float3 upVector)
	{
		return Evaluate(0, t, out position, out tangent, out upVector);
	}

	public bool Evaluate(int splineIndex, float t, out float3 position, out float3 tangent, out float3 upVector)
	{
		return Evaluate(m_Splines[splineIndex], t, out position, out tangent, out upVector);
	}

	public bool Evaluate<T>(T spline, float t, out float3 position, out float3 tangent, out float3 upVector) where T : ISpline
	{
		if (spline == null)
		{
			position = float3.zero;
			tangent = new float3(0f, 0f, 1f);
			upVector = new float3(0f, 1f, 0f);
			return false;
		}
		if (IsNonUniformlyScaled)
		{
			return GetOrBakeNativeSpline(spline).Evaluate(t, out position, out tangent, out upVector);
		}
		bool num = spline.Evaluate(t, out position, out tangent, out upVector);
		if (num)
		{
			position = base.transform.TransformPoint(position);
			tangent = base.transform.TransformVector(tangent);
			upVector = base.transform.TransformDirection(upVector);
		}
		return num;
	}

	public float3 EvaluatePosition(float t)
	{
		return EvaluatePosition(0, t);
	}

	public float3 EvaluatePosition(int splineIndex, float t)
	{
		return EvaluatePosition(m_Splines[splineIndex], t);
	}

	public float3 EvaluatePosition<T>(T spline, float t) where T : ISpline
	{
		if (spline == null)
		{
			return float.PositiveInfinity;
		}
		if (IsNonUniformlyScaled)
		{
			return GetOrBakeNativeSpline(spline).EvaluatePosition(t);
		}
		return base.transform.TransformPoint(spline.EvaluatePosition(t));
	}

	public float3 EvaluateTangent(float t)
	{
		return EvaluateTangent(0, t);
	}

	public float3 EvaluateTangent(int splineIndex, float t)
	{
		return EvaluateTangent(m_Splines[splineIndex], t);
	}

	public float3 EvaluateTangent<T>(T spline, float t) where T : ISpline
	{
		if (spline == null)
		{
			return float.PositiveInfinity;
		}
		if (IsNonUniformlyScaled)
		{
			return GetOrBakeNativeSpline(spline).EvaluateTangent(t);
		}
		return base.transform.TransformVector(spline.EvaluateTangent(t));
	}

	public float3 EvaluateUpVector(float t)
	{
		return EvaluateUpVector(0, t);
	}

	public float3 EvaluateUpVector(int splineIndex, float t)
	{
		return EvaluateUpVector(m_Splines[splineIndex], t);
	}

	public float3 EvaluateUpVector<T>(T spline, float t) where T : ISpline
	{
		if (spline == null)
		{
			return float3.zero;
		}
		if (IsNonUniformlyScaled)
		{
			return GetOrBakeNativeSpline(spline).EvaluateUpVector(t);
		}
		return base.transform.TransformDirection(spline.EvaluateUpVector(t));
	}

	public float3 EvaluateAcceleration(float t)
	{
		return EvaluateAcceleration(0, t);
	}

	public float3 EvaluateAcceleration(int splineIndex, float t)
	{
		return EvaluateAcceleration(m_Splines[splineIndex], t);
	}

	public float3 EvaluateAcceleration<T>(T spline, float t) where T : ISpline
	{
		if (spline == null)
		{
			return float3.zero;
		}
		if (IsNonUniformlyScaled)
		{
			return GetOrBakeNativeSpline(spline).EvaluateAcceleration(t);
		}
		return base.transform.TransformVector(spline.EvaluateAcceleration(t));
	}

	public float CalculateLength()
	{
		return CalculateLength(0);
	}

	public float CalculateLength(int splineIndex)
	{
		return m_Splines[splineIndex].CalculateLength(base.transform.localToWorldMatrix);
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
		if (m_Spline != null && m_Spline.Count > 0)
		{
			if (m_Splines == null || m_Splines.Length == 0 || (m_Splines.Length == 1 && m_Splines[0].Count == 0))
			{
				m_Splines = new Spline[1] { m_Spline };
			}
			m_Spline = new Spline();
		}
		bool flag = m_ReadOnlySplines == null || m_ReadOnlySplines.Count != m_Splines.Length;
		if (!flag)
		{
			for (int i = 0; i < m_Splines.Length; i++)
			{
				if (m_ReadOnlySplines[i] != m_Splines[i])
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			m_ReadOnlySplines = new ReadOnlyCollection<Spline>(m_Splines);
		}
	}

	private NativeSpline GetOrBakeNativeSpline<T>(T spline) where T : ISpline
	{
		if (!m_NativeSplinesCache.TryGetValue(spline, out var value))
		{
			m_NativeSplinesCacheTransform = base.transform.localToWorldMatrix;
			value = new NativeSpline(spline, m_NativeSplinesCacheTransform, cacheUpVectors: true, Allocator.Persistent);
			m_NativeSplinesCache.Add(spline, value);
		}
		else if (!MathUtility.All(m_NativeSplinesCacheTransform, base.transform.localToWorldMatrix))
		{
			m_NativeSplinesCacheTransform = base.transform.localToWorldMatrix;
			s_AllocPreventionHelperBuffer.Clear();
			foreach (ISpline key in m_NativeSplinesCache.Keys)
			{
				NativeSpline nativeSpline = m_NativeSplinesCache[key];
				NativeSpline nativeSpline2 = new NativeSpline(spline, m_NativeSplinesCacheTransform, cacheUpVectors: true, Allocator.Persistent);
				if (key == (object)spline)
				{
					value = nativeSpline2;
				}
				nativeSpline.Dispose();
				s_AllocPreventionHelperBuffer.Add(new SplineToNative
				{
					spline = key,
					nativeSpline = nativeSpline2
				});
			}
			for (int i = 0; i < s_AllocPreventionHelperBuffer.Count; i++)
			{
				SplineToNative splineToNative = s_AllocPreventionHelperBuffer[i];
				m_NativeSplinesCache[splineToNative.spline] = splineToNative.nativeSpline;
			}
		}
		return value;
	}
}
