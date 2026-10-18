using System;
using System.Collections.Generic;
using UnityEngine;

namespace Meta.XR.ImmersiveDebugger.Gizmo;

[ExecuteAlways]
public class DebugGizmos : MonoBehaviour
{
	internal struct ColorScope : IDisposable
	{
		private readonly Color _savedColor;

		public ColorScope(Color color)
		{
			_savedColor = Color;
			Color = color;
		}

		public void Dispose()
		{
			Color = _savedColor;
		}
	}

	private List<Vector4> _points = new List<Vector4>();

	private List<Color> _colors = new List<Color>();

	private int _index;

	private bool _addedSegmentSinceLastUpdate;

	protected static DebugGizmos _root;

	private PolylineRenderer _polylineRenderer;

	private static bool _renderSinglePass = true;

	public static Color Color = Color.black;

	public static float LineWidth = 0.1f;

	private static readonly IReadOnlyList<Vector2> PLANE_POINTS = new List<Vector2>
	{
		new Vector2(-0.5f, -0.5f),
		new Vector2(-0.5f, 0.5f),
		new Vector2(0.5f, -0.5f),
		new Vector2(0.5f, 0.5f)
	};

	private static readonly IReadOnlyList<int> PLANE_SEGMENTS = new List<int> { 0, 1, 1, 3, 3, 2, 2, 0 };

	private static readonly IReadOnlyList<Vector3> CUBE_POINTS = new List<Vector3>
	{
		new Vector3(-0.5f, -0.5f, -0.5f),
		new Vector3(0.5f, -0.5f, -0.5f),
		new Vector3(-0.5f, 0.5f, -0.5f),
		new Vector3(-0.5f, -0.5f, 0.5f),
		new Vector3(0.5f, 0.5f, -0.5f),
		new Vector3(0.5f, -0.5f, 0.5f),
		new Vector3(-0.5f, 0.5f, 0.5f),
		new Vector3(0.5f, 0.5f, 0.5f)
	};

	private static readonly IReadOnlyList<int> CUBE_SEGMENTS = new List<int>
	{
		0, 1, 1, 5, 3, 5, 0, 3, 0, 2,
		1, 4, 3, 6, 5, 7, 2, 4, 4, 7,
		7, 6, 6, 2
	};

	protected static DebugGizmos Root
	{
		get
		{
			if (_root == null)
			{
				GameObject gameObject = GameObject.Find("Polyline Gizmos");
				if (gameObject != null)
				{
					DebugGizmos component = gameObject.GetComponent<DebugGizmos>();
					if (component != null)
					{
						_root = component;
					}
				}
			}
			if (_root == null)
			{
				GameObject gameObject2 = new GameObject("Polyline Gizmos");
				_root = gameObject2.AddComponent<DebugGizmos>();
				if (Application.isPlaying)
				{
					UnityEngine.Object.DontDestroyOnLoad(gameObject2);
				}
			}
			return _root;
		}
	}

	private PolylineRenderer Renderer
	{
		get
		{
			if (_polylineRenderer == null)
			{
				_polylineRenderer = new PolylineRenderer(null, _renderSinglePass);
			}
			return _polylineRenderer;
		}
	}

	public static bool RenderSinglePass
	{
		get
		{
			return _renderSinglePass;
		}
		set
		{
			if (_renderSinglePass != value)
			{
				_renderSinglePass = value;
				if (Root != null)
				{
					UnityEngine.Object.Destroy(Root);
				}
			}
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Init()
	{
		_root = null;
		_renderSinglePass = true;
		Color = Color.black;
		LineWidth = 0.1f;
	}

	protected virtual void OnEnable()
	{
		if (!(_root == null) && _root != this)
		{
			UnityEngine.Object.Destroy(this);
		}
	}

	protected virtual void OnDisable()
	{
		if (_polylineRenderer != null)
		{
			_polylineRenderer.Cleanup();
			_polylineRenderer = null;
		}
		_ = Application.isPlaying;
	}

	protected void ClearSegments()
	{
		_index = 0;
	}

	protected void RenderSegments()
	{
		Renderer.SetLines(_points, _colors, _index);
		Renderer.RenderLines();
	}

	protected virtual void LateUpdate()
	{
		if (Application.isPlaying)
		{
			RenderSegments();
			ClearSegments();
		}
	}

	protected void AddSegment(Vector3 p0, Vector3 p1, float width, Color color0, Color color1)
	{
		if (!_addedSegmentSinceLastUpdate)
		{
			ClearSegments();
			_addedSegmentSinceLastUpdate = true;
		}
		while (_index + 2 > _points.Count)
		{
			_points.Add(default(Vector4));
			_colors.Add(default(Color));
		}
		_points[_index] = new Vector4(p0.x, p0.y, p0.z, width);
		_points[_index + 1] = new Vector4(p1.x, p1.y, p1.z, width);
		_colors[_index] = color0;
		_colors[_index + 1] = color1;
		_index += 2;
	}

	public static void DrawPoint(Vector3 p0, Transform t = null)
	{
		if (t != null)
		{
			p0 = t.TransformPoint(p0);
		}
		Root.AddSegment(p0, p0, LineWidth, Color, Color);
	}

	public static void DrawLine(Vector3 p0, Vector3 p1, Transform t = null)
	{
		if (t != null)
		{
			p0 = t.TransformPoint(p0);
			p1 = t.TransformPoint(p1);
		}
		Root.AddSegment(p0, p1, LineWidth, Color, Color);
	}

	public static void DrawWireCube(Vector3 center, float size, Transform t = null)
	{
		for (int i = 0; i < CUBE_SEGMENTS.Count; i += 2)
		{
			Vector3 p = CUBE_POINTS[CUBE_SEGMENTS[i]] * size + center;
			Vector3 p2 = CUBE_POINTS[CUBE_SEGMENTS[i + 1]] * size + center;
			DrawLine(p, p2, t);
		}
	}

	private static void DrawAxis(Vector3 position, Quaternion rotation, float size = 0.1f)
	{
		using (new ColorScope(Color.black))
		{
			Color = Color.red;
			DrawLine(position, position + rotation * Vector3.right * size);
			Color = Color.green;
			DrawLine(position, position + rotation * Vector3.up * size);
			Color = Color.blue;
			DrawLine(position, position + rotation * Vector3.forward * size);
		}
	}

	public static void DrawAxis(Pose pose, float size = 0.1f)
	{
		DrawAxis(pose.position, pose.rotation, size);
	}

	public static void DrawAxis(Transform t, float size = 0.1f)
	{
		DrawAxis(new Pose(t.position, t.rotation), size);
	}

	private static void DrawPlane(Vector3 position, Quaternion rotation, float width, float height)
	{
		DrawAxis(position, rotation);
		Matrix4x4 matrix4x = Matrix4x4.TRS(position, rotation, Vector3.one);
		for (int i = 0; i < PLANE_SEGMENTS.Count; i += 2)
		{
			Vector3 point = new Vector3(PLANE_POINTS[PLANE_SEGMENTS[i]].x * width, PLANE_POINTS[PLANE_SEGMENTS[i]].y * height, 0f);
			DrawLine(p1: matrix4x.MultiplyPoint3x4(new Vector3(PLANE_POINTS[PLANE_SEGMENTS[i + 1]].x * width, PLANE_POINTS[PLANE_SEGMENTS[i + 1]].y * height, 0f)), p0: matrix4x.MultiplyPoint3x4(point));
		}
	}

	public static void DrawPlane(Pose pose, float width, float height)
	{
		DrawPlane(pose.position, pose.rotation, width, height);
	}

	private static void DrawBox(Vector3 position, Quaternion rotation, float width, float height, float depth, bool isPivotTopSurface)
	{
		DrawAxis(position, rotation);
		if (isPivotTopSurface)
		{
			Vector3 vector = new Vector3(0f, depth / 2f, 0f);
			position -= vector;
		}
		Matrix4x4 matrix4x = Matrix4x4.TRS(position, rotation, Vector3.one);
		for (int i = 0; i < CUBE_SEGMENTS.Count; i += 2)
		{
			Vector3 point = new Vector3(CUBE_POINTS[CUBE_SEGMENTS[i]].x * width, CUBE_POINTS[CUBE_SEGMENTS[i]].y * height, CUBE_POINTS[CUBE_SEGMENTS[i]].z * depth);
			DrawLine(p1: matrix4x.MultiplyPoint3x4(new Vector3(CUBE_POINTS[CUBE_SEGMENTS[i + 1]].x * width, CUBE_POINTS[CUBE_SEGMENTS[i + 1]].y * height, CUBE_POINTS[CUBE_SEGMENTS[i + 1]].z * depth)), p0: matrix4x.MultiplyPoint3x4(point));
		}
	}

	public static void DrawBox(Pose pose, float width, float height, float depth, bool isPivotTopSurface = false)
	{
		DrawBox(pose.position, pose.rotation, width, height, depth, isPivotTopSurface);
	}
}
