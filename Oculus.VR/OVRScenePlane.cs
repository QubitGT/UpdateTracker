using System;
using System.Collections.Generic;
using Meta.XR.Util;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(OVRSceneAnchor))]
[HelpURL("https://developer.oculus.com/documentation/unity/unity-scene-use-scene-anchors/#further-scene-model-unity-components")]
[Obsolete("OVRSceneManager and associated classes are deprecated (v65), please use MR Utility Kit instead (https://developer.oculus.com/documentation/unity/unity-mr-utility-kit-overview)")]
[Feature(Feature.Scene)]
public class OVRScenePlane : MonoBehaviour, IOVRSceneComponent
{
	private struct GetBoundaryLengthJob : IJob
	{
		public OVRSpace Space;

		[WriteOnly]
		public NativeArray<int> Length;

		public void Execute()
		{
			Length[0] = (OVRPlugin.GetSpaceBoundary2DCount(Space, out var count) ? count : 0);
		}
	}

	private struct GetBoundaryJob : IJob
	{
		public OVRSpace Space;

		public NativeArray<Vector2> Boundary;

		public NativeArray<Vector2> PreviousBoundary;

		private bool HasBoundaryChanged()
		{
			if (!PreviousBoundary.IsCreated)
			{
				return true;
			}
			if (Boundary.Length != PreviousBoundary.Length)
			{
				return true;
			}
			int length = Boundary.Length;
			for (int i = 0; i < length; i++)
			{
				if (Vector2.SqrMagnitude(Boundary[i] - PreviousBoundary[i]) > 1E-06f)
				{
					return true;
				}
			}
			return false;
		}

		private static void SetNaN(NativeArray<Vector2> array)
		{
			if (array.Length > 0)
			{
				array[0] = new Vector2(float.NaN, float.NaN);
			}
		}

		public void Execute()
		{
			if (OVRPlugin.GetSpaceBoundary2D(Space, Boundary) && HasBoundaryChanged())
			{
				SetNaN(PreviousBoundary);
			}
			else
			{
				SetNaN(Boundary);
			}
		}
	}

	[Tooltip("When enabled, scales the child transforms according to the dimensions of this plane. If both Volume and Plane components exist on the game object, the volume takes precedence.")]
	[SerializeField]
	internal bool _scaleChildren = true;

	[Tooltip("When enabled, offsets the child transforms according to the offset of this plane. If both Volume and Plane components exist on the game object, the volume takes precedence.")]
	[SerializeField]
	internal bool _offsetChildren = true;

	internal JobHandle? _jobHandle;

	private NativeArray<Vector2> _previousBoundary;

	private NativeArray<int> _boundaryLength;

	private NativeArray<Vector2> _boundaryBuffer;

	private bool _boundaryRequested;

	private OVRSceneAnchor _sceneAnchor;

	private readonly List<Vector2> _boundary = new List<Vector2>();

	public float Width { get; private set; }

	public float Height { get; private set; }

	public Vector2 Offset { get; private set; }

	public Vector2 Dimensions => new Vector2(Width, Height);

	public IReadOnlyList<Vector2> Boundary => _boundary;

	public bool ScaleChildren
	{
		get
		{
			return _scaleChildren;
		}
		set
		{
			_scaleChildren = value;
			if (_scaleChildren && _sceneAnchor.Space.Valid)
			{
				SetChildScale();
			}
		}
	}

	public bool OffsetChildren
	{
		get
		{
			return _offsetChildren;
		}
		set
		{
			_offsetChildren = value;
			if (_offsetChildren && _sceneAnchor.Space.Valid)
			{
				SetChildOffset();
			}
		}
	}

	private void SetChildScale()
	{
		OVRSceneVolume component;
		bool flag = TryGetComponent<OVRSceneVolume>(out component);
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Transform child = base.transform.GetChild(i);
			if (child.TryGetComponent<OVRSceneObjectTransformType>(out var component2))
			{
				if (component2.TransformType != OVRSceneObjectTransformType.Transformation.Plane)
				{
					continue;
				}
			}
			else if (flag && component.ScaleChildren)
			{
				continue;
			}
			child.localScale = new Vector3(Width, Height, child.localScale.z);
		}
	}

	private void SetChildOffset()
	{
		OVRSceneVolume component;
		bool flag = TryGetComponent<OVRSceneVolume>(out component);
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Transform child = base.transform.GetChild(i);
			if (child.TryGetComponent<OVRSceneObjectTransformType>(out var component2))
			{
				if (component2.TransformType != OVRSceneObjectTransformType.Transformation.Plane)
				{
					continue;
				}
			}
			else if (flag && component.OffsetChildren)
			{
				continue;
			}
			child.localPosition = new Vector3(Offset.x, Offset.y, 0f);
		}
	}

	internal void UpdateTransform()
	{
		if (OVRPlugin.GetSpaceBoundingBox2D(GetComponent<OVRSceneAnchor>().Space, out var rect))
		{
			Width = rect.Size.w;
			Height = rect.Size.h;
			Vector2 vector = base.transform.TransformPoint(rect.Pos.FromVector2f() + rect.Size.FromSizef() / 2f);
			Vector2 vector2 = new Vector2(base.transform.position.x, base.transform.position.y);
			Offset = vector - vector2;
			if (ScaleChildren)
			{
				SetChildScale();
			}
			if (OffsetChildren)
			{
				SetChildOffset();
			}
		}
	}

	private void Awake()
	{
		_sceneAnchor = GetComponent<OVRSceneAnchor>();
		if (_sceneAnchor.Space.Valid)
		{
			((IOVRSceneComponent)this).Initialize();
		}
	}

	private void Start()
	{
		RequestBoundary();
	}

	void IOVRSceneComponent.Initialize()
	{
		UpdateTransform();
	}

	internal void ScheduleGetLengthJob()
	{
		if (!_jobHandle.HasValue && OVRPlugin.GetSpaceComponentStatus(_sceneAnchor.Space, OVRPlugin.SpaceComponentType.Bounded2D, out var flag, out var changePending) && !(!flag || changePending))
		{
			_boundaryLength = new NativeArray<int>(1, Allocator.TempJob);
			_jobHandle = new GetBoundaryLengthJob
			{
				Length = _boundaryLength,
				Space = _sceneAnchor.Space
			}.Schedule();
			_boundaryRequested = false;
		}
	}

	internal void RequestBoundary()
	{
		_boundaryRequested = true;
		if (base.enabled)
		{
			ScheduleGetLengthJob();
		}
	}

	private void Update()
	{
		ref JobHandle? jobHandle = ref _jobHandle;
		if (!jobHandle.HasValue || !jobHandle.GetValueOrDefault().IsCompleted)
		{
			return;
		}
		_jobHandle.Value.Complete();
		_jobHandle = null;
		if (_boundaryLength.IsCreated)
		{
			int num = _boundaryLength[0];
			_boundaryLength.Dispose();
			if (num >= 3)
			{
				using (new OVRProfilerScope("Schedule GetBoundaryJob"))
				{
					_boundaryBuffer = new NativeArray<Vector2>(num, Allocator.TempJob);
					if (!_previousBoundary.IsCreated)
					{
						_previousBoundary = new NativeArray<Vector2>(num, Allocator.Persistent);
					}
					_jobHandle = new GetBoundaryJob
					{
						Space = _sceneAnchor.Space,
						Boundary = _boundaryBuffer,
						PreviousBoundary = _previousBoundary
					}.Schedule();
					return;
				}
			}
			ScheduleGetLengthJob();
		}
		else if (_boundaryBuffer.IsCreated)
		{
			using (new OVRProfilerScope("Copy boundary"))
			{
				if (_previousBoundary.Length == 0 || float.IsNaN(_previousBoundary[0].x))
				{
					if (_previousBoundary.IsCreated)
					{
						_previousBoundary.Dispose();
					}
					_previousBoundary = new NativeArray<Vector2>(_boundaryBuffer.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
					_previousBoundary.CopyFrom(_boundaryBuffer);
					_boundary.Clear();
					foreach (Vector2 item in _previousBoundary)
					{
						_boundary.Add(new Vector2(0f - item.x, item.y));
					}
				}
			}
			_boundaryBuffer.Dispose();
			if (TryGetComponent<OVRScenePlaneMeshFilter>(out var component))
			{
				component.RequestMeshGeneration();
			}
		}
		else if (_boundaryRequested)
		{
			ScheduleGetLengthJob();
		}
	}

	private void OnDisable()
	{
		if (_boundaryLength.IsCreated)
		{
			_boundaryLength.Dispose(_jobHandle.GetValueOrDefault());
		}
		if (_boundaryBuffer.IsCreated)
		{
			_boundaryBuffer.Dispose(_jobHandle.GetValueOrDefault());
		}
		if (_previousBoundary.IsCreated)
		{
			_previousBoundary.Dispose(_jobHandle.GetValueOrDefault());
		}
		_previousBoundary = default(NativeArray<Vector2>);
		_boundaryBuffer = default(NativeArray<Vector2>);
		_boundaryLength = default(NativeArray<int>);
		_jobHandle = null;
	}
}
