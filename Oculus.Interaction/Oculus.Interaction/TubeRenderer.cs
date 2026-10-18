using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Oculus.Interaction;

public class TubeRenderer : MonoBehaviour
{
	private struct VertexLayout
	{
		public Vector3 pos;

		public Color32 color;

		public Vector2 uv;
	}

	[Tooltip("The Mesh Filter that's included in the ReticleLine prefab.")]
	[SerializeField]
	private MeshFilter _filter;

	[Tooltip("The Mesh Renderer that's included in the ReticleLine prefab.")]
	[SerializeField]
	private MeshRenderer _renderer;

	[Tooltip("The number of divisions to use when calculating the tube mesh's vertices.")]
	[SerializeField]
	private int _divisions = 6;

	[Tooltip("The number of bevels to use when calculating the tube mesh's vertices.")]
	[SerializeField]
	private int _bevel = 4;

	[Tooltip("Unity shader queue that determines when the tube is rendered. Defaults to -1, which uses the render queue of the shader.")]
	[SerializeField]
	private int _renderQueue = -1;

	[SerializeField]
	private Vector2 _renderOffset = Vector2.zero;

	[Tooltip("The thickness of the tube.")]
	[SerializeField]
	private float _radius = 0.005f;

	[Tooltip("The gradient of the tube.")]
	[SerializeField]
	private Gradient _gradient;

	[Tooltip("The color of the tube.")]
	[SerializeField]
	private Color _tint = Color.white;

	[SerializeField]
	[Range(0f, 1f)]
	private float _progressFade = 0.2f;

	[Tooltip("Defines the length of the transparent portion at the beginning of the tube. The higher the value, the longer the transparent portion.")]
	[SerializeField]
	private float _startFadeThresold = 0.2f;

	[Tooltip("Defines the length of the transparent portion at the end of the tube. The higher the value, the longer the transparent portion.")]
	[SerializeField]
	private float _endFadeThresold = 0.2f;

	[Tooltip("Should the transparent portion of the tube be in the middle instead of at the beginning and end?")]
	[SerializeField]
	private bool _invertThreshold;

	[SerializeField]
	private float _feather = 0.2f;

	[SerializeField]
	private bool _mirrorTexture;

	private VertexAttributeDescriptor[] _dataLayout;

	private NativeArray<VertexLayout> _vertsData;

	private VertexLayout _layout;

	private Mesh _mesh;

	private int[] _tris;

	private int _initializedSteps = -1;

	private int _vertsCount;

	private float _totalLength;

	private bool _hidden;

	private static readonly int _fadeLimitsShaderID = Shader.PropertyToID("_FadeLimit");

	private static readonly int _fadeSignShaderID = Shader.PropertyToID("_FadeSign");

	private static readonly int _offsetFactorShaderPropertyID = Shader.PropertyToID("_OffsetFactor");

	private static readonly int _offsetUnitsShaderPropertyID = Shader.PropertyToID("_OffsetUnits");

	public int RenderQueue
	{
		get
		{
			return _renderQueue;
		}
		set
		{
			_renderQueue = value;
		}
	}

	public Vector2 RenderOffset
	{
		get
		{
			return _renderOffset;
		}
		set
		{
			_renderOffset = value;
		}
	}

	public float Radius
	{
		get
		{
			return _radius;
		}
		set
		{
			_radius = value;
		}
	}

	public Gradient Gradient
	{
		get
		{
			return _gradient;
		}
		set
		{
			_gradient = value;
		}
	}

	public Color Tint
	{
		get
		{
			return _tint;
		}
		set
		{
			_tint = value;
		}
	}

	public float ProgressFade
	{
		get
		{
			return _progressFade;
		}
		set
		{
			_progressFade = value;
		}
	}

	public float StartFadeThresold
	{
		get
		{
			return _startFadeThresold;
		}
		set
		{
			_startFadeThresold = value;
		}
	}

	public float EndFadeThresold
	{
		get
		{
			return _endFadeThresold;
		}
		set
		{
			_endFadeThresold = value;
		}
	}

	public bool InvertThreshold
	{
		get
		{
			return _invertThreshold;
		}
		set
		{
			_invertThreshold = value;
		}
	}

	public float Feather
	{
		get
		{
			return _feather;
		}
		set
		{
			_feather = value;
		}
	}

	public bool MirrorTexture
	{
		get
		{
			return _mirrorTexture;
		}
		set
		{
			_mirrorTexture = value;
		}
	}

	public float Progress { get; set; }

	public float TotalLength => _totalLength;

	protected virtual void Reset()
	{
		_filter = GetComponent<MeshFilter>();
		_renderer = GetComponent<MeshRenderer>();
	}

	protected virtual void Awake()
	{
		_hidden = base.enabled;
	}

	protected virtual void OnEnable()
	{
		_renderer.enabled = !_hidden;
	}

	protected virtual void OnDisable()
	{
		_renderer.enabled = false;
	}

	public void RenderTube(TubePoint[] points, Space space = Space.Self)
	{
		int num = points.Length;
		if (num != _initializedSteps)
		{
			InitializeMeshData(num);
			_initializedSteps = num;
		}
		_vertsData = new NativeArray<VertexLayout>(_vertsCount, Allocator.Temp);
		UpdateMeshData(points, space);
		_renderer.enabled = base.enabled;
		_hidden = false;
	}

	public void Hide()
	{
		_renderer.enabled = false;
		_hidden = true;
	}

	public void Show()
	{
		_renderer.enabled = true;
		_hidden = false;
	}

	private void InitializeMeshData(int steps)
	{
		_dataLayout = new VertexAttributeDescriptor[3]
		{
			new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0),
			new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4),
			new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2)
		};
		_vertsCount = SetVertexCount(steps, _divisions, _bevel);
		SubMeshDescriptor desc = new SubMeshDescriptor(0, _tris.Length);
		_mesh = new Mesh();
		_mesh.SetVertexBufferParams(_vertsCount, _dataLayout);
		_mesh.SetIndexBufferParams(_tris.Length, IndexFormat.UInt32);
		_mesh.SetIndexBufferData(_tris, 0, 0, _tris.Length);
		_mesh.subMeshCount = 1;
		_mesh.SetSubMesh(0, desc);
		_filter.mesh = _mesh;
	}

	private void UpdateMeshData(TubePoint[] points, Space space)
	{
		int num = points.Length;
		float num2 = 0f;
		Vector3 b = Vector3.zero;
		Pose pose2 = Pose.identity;
		Pose pose3 = Pose.identity;
		Pose pose4 = Pose.identity;
		Pose pose5 = base.transform.GetPose();
		Quaternion inverseRootRotation = Quaternion.Inverse(pose5.rotation);
		Vector3 rootPositionScaled = new Vector3(pose5.position.x / base.transform.lossyScale.x, pose5.position.y / base.transform.lossyScale.y, pose5.position.z / base.transform.lossyScale.z);
		float num3 = ((space == Space.World) ? base.transform.lossyScale.x : 1f);
		TransformPose(in points[0], ref pose3);
		TransformPose(in points[^1], ref pose4);
		BevelCap(in pose3, end: false, 0);
		for (int i = 0; i < num; i++)
		{
			TransformPose(in points[i], ref pose2);
			Vector3 position = pose2.position;
			Quaternion rotation = pose2.rotation;
			float relativeLength = points[i].relativeLength;
			Color color = Gradient.Evaluate(relativeLength) * _tint;
			if (i > 0)
			{
				num2 += Vector3.Distance(position, b);
			}
			b = position;
			if ((float)i / ((float)num - 1f) < Progress)
			{
				color.a *= ProgressFade;
			}
			_layout.color = color;
			WriteCircle(position, rotation, _radius, i + _bevel, relativeLength);
		}
		BevelCap(in pose4, end: true, _bevel + num);
		_mesh.bounds = new Bounds((pose3.position + pose4.position) * 0.5f, pose4.position - pose3.position);
		_mesh.SetVertexBufferData(_vertsData, 0, 0, _vertsData.Length, 0, MeshUpdateFlags.DontRecalculateBounds);
		_totalLength = num2 * num3;
		RedrawFadeThresholds();
		void TransformPose(in TubePoint tubePoint, ref Pose pose)
		{
			if (space == Space.Self)
			{
				pose.position = tubePoint.position;
				pose.rotation = tubePoint.rotation;
			}
			else
			{
				pose.position = inverseRootRotation * (tubePoint.position - rootPositionScaled);
				pose.rotation = inverseRootRotation * tubePoint.rotation;
			}
		}
	}

	public void RedrawFadeThresholds()
	{
		float num = StartFadeThresold / _totalLength;
		float num2 = (StartFadeThresold + Feather) / _totalLength;
		float w = (_totalLength - EndFadeThresold) / _totalLength;
		float z = (_totalLength - EndFadeThresold - Feather) / _totalLength;
		_renderer.material.SetVector(_fadeLimitsShaderID, new Vector4(_invertThreshold ? num2 : num, _invertThreshold ? num : num2, z, w));
		_renderer.material.SetFloat(_fadeSignShaderID, (!_invertThreshold) ? 1 : (-1));
		_renderer.material.renderQueue = _renderQueue;
		_renderer.material.SetFloat(_offsetFactorShaderPropertyID, _renderOffset.x);
		_renderer.material.SetFloat(_offsetUnitsShaderPropertyID, _renderOffset.y);
	}

	private void BevelCap(in Pose pose, bool end, int indexOffset)
	{
		Vector3 position = pose.position;
		Quaternion rotation = pose.rotation;
		for (int i = 0; i < _bevel; i++)
		{
			float num = Mathf.InverseLerp(-1f, _bevel + 1, i);
			if (end)
			{
				num = 1f - num;
			}
			float num2 = Mathf.Sqrt(1f - num * num);
			Vector3 point = position + (end ? 1 : (-1)) * (rotation * Vector3.forward) * _radius * num2;
			WriteCircle(point, rotation, _radius * num, i + indexOffset, end ? 1 : 0);
		}
	}

	private void WriteCircle(Vector3 point, Quaternion rotation, float width, int index, float progress)
	{
		Color color = Gradient.Evaluate(progress) * _tint;
		if (progress < Progress)
		{
			color.a *= ProgressFade;
		}
		_layout.color = color;
		for (int i = 0; i <= _divisions; i++)
		{
			float f = MathF.PI * 2f * (float)i / (float)_divisions;
			Vector3 vector = new Vector3(Mathf.Sin(f), Mathf.Cos(f), 0f);
			Vector3 vector2 = rotation * vector;
			_layout.pos = point + vector2 * width;
			if (_mirrorTexture)
			{
				float num = (float)i / (float)_divisions * 2f;
				if ((float)i >= (float)_divisions * 0.5f)
				{
					num = 2f - num;
				}
				_layout.uv = new Vector2(num, progress);
			}
			else
			{
				_layout.uv = new Vector2((float)i / (float)_divisions, progress);
			}
			int index2 = index * (_divisions + 1) + i;
			_vertsData[index2] = _layout;
		}
	}

	private int SetVertexCount(int positionCount, int divisions, int bevelCap)
	{
		bevelCap *= 2;
		int num = divisions + 1;
		int num2 = (positionCount + bevelCap) * num;
		int num3 = (positionCount - 1 + bevelCap) * divisions * 6;
		int num4 = (divisions - 2) * 3;
		int num5 = num3 + num4 * 2;
		_tris = new int[num5];
		for (int i = 0; i < positionCount - 1 + bevelCap; i++)
		{
			for (int j = 0; j < divisions; j++)
			{
				int num6 = i * num + j;
				int num7 = (i + 1) * num + j;
				int num8 = (i * divisions + j) * 6;
				_tris[num8] = num6;
				_tris[num8 + 1] = (_tris[num8 + 4] = num7);
				_tris[num8 + 2] = (_tris[num8 + 3] = num6 + 1);
				_tris[num8 + 5] = num7 + 1;
			}
		}
		Cap(num3, 0, divisions - 1, clockwise: true);
		Cap(num3 + num4, num2 - divisions, num2 - 1);
		return num2;
		void Cap(int t, int firstVert, int lastVert, bool clockwise = false)
		{
			for (int k = firstVert + 1; k < lastVert; k++)
			{
				_tris[t++] = firstVert;
				_tris[t++] = (clockwise ? k : (k + 1));
				_tris[t++] = (clockwise ? (k + 1) : k);
			}
		}
	}

	public void InjectAllTubeRenderer(MeshFilter filter, MeshRenderer renderer, int divisions, int bevel)
	{
		InjectFilter(filter);
		InjectRenderer(renderer);
		InjectDivisions(divisions);
		InjectBevel(bevel);
	}

	public void InjectFilter(MeshFilter filter)
	{
		_filter = filter;
	}

	public void InjectRenderer(MeshRenderer renderer)
	{
		_renderer = renderer;
	}

	public void InjectDivisions(int divisions)
	{
		_divisions = divisions;
	}

	public void InjectBevel(int bevel)
	{
		_bevel = bevel;
	}
}
