using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction;

[ExecuteAlways]
public class DebugGizmos : MonoBehaviour
{
	private List<Vector4> _points = new List<Vector4>();

	private List<Color> _colors = new List<Color>();

	private int _index;

	private bool _addedSegmentSinceLastUpdate;

	protected static DebugGizmos _root = null;

	private PolylineRenderer _polylineRenderer;

	private static bool _renderSinglePass = true;

	public static Color Color = Color.black;

	public static float LineWidth = 0.1f;

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
				_root = new GameObject("Polyline Gizmos").AddComponent<DebugGizmos>();
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
					Object.Destroy(Root);
				}
			}
		}
	}

	protected virtual void OnEnable()
	{
		if (!(_root == null) && _root != this)
		{
			Object.Destroy(this);
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

	public static void DrawQuad(Vector3 center, float width, float height, Transform t = null)
	{
		Vector3 vector = new Vector3((0f - width) / 2f, height / 2f) + center;
		Vector3 vector2 = new Vector3(width / 2f, height / 2f) + center;
		Vector3 vector3 = new Vector3(width / 2f, (0f - height) / 2f) + center;
		Vector3 vector4 = new Vector3((0f - width) / 2f, (0f - height) / 2f) + center;
		DrawLine(vector, vector2, t);
		DrawLine(vector2, vector3, t);
		DrawLine(vector3, vector4, t);
		DrawLine(vector4, vector, t);
	}

	public static void DrawCurvedQuad(Vector3 center, float width, float height, float radius, Transform t = null, int divisions = 20)
	{
		Vector3[] array = new Vector3[divisions + 1];
		float num = width / radius;
		float num2 = num / (float)divisions;
		for (int i = 0; i <= divisions; i++)
		{
			float f = (float)i * num2 - num / 2f;
			Vector3 vector = new Vector3(Mathf.Sin(f), 0f, Mathf.Cos(f) - 1f) * radius;
			array[i] = vector + center;
		}
		Vector3 vector2 = new Vector3(0f, height / 2f, 0f);
		for (int j = 0; j < divisions; j++)
		{
			Vector3 vector3 = array[j];
			Vector3 vector4 = array[j + 1];
			DrawLine(vector3 + vector2, vector4 + vector2, t);
			DrawLine(vector3 - vector2, vector4 - vector2, t);
		}
		DrawLine(array[0] + vector2, array[0] - vector2, t);
		DrawLine(array[divisions] + vector2, array[divisions] - vector2, t);
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

	public static void DrawAxis(Vector3 position, Quaternion rotation, float size = 1f)
	{
		Color color = Color;
		Color = Color.red;
		DrawLine(position, position + rotation * Vector3.right * size);
		Color = Color.green;
		DrawLine(position, position + rotation * Vector3.up * size);
		Color = Color.blue;
		DrawLine(position, position + rotation * Vector3.forward * size);
		Color = color;
	}

	public static void DrawAxis(Pose pose, float size = 1f)
	{
		DrawAxis(pose.position, pose.rotation, size);
	}

	public static void DrawAxis(Transform t, float size = 1f)
	{
		DrawAxis(t.GetPose(), size);
	}
}
