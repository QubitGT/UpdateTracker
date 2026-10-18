using System.Collections.Generic;
using UnityEngine;

namespace Meta.XR.ImmersiveDebugger.Gizmo;

internal class PolylineRenderer
{
	private Vector4[] _positions;

	private bool _positionsNeedUpdate;

	private Color[] _colors;

	private bool _colorsNeedUpdate;

	private Bounds _bounds;

	private Mesh _baseMesh;

	private Material _material;

	private bool _renderSinglePass;

	private ComputeBuffer _positionBuffer;

	private ComputeBuffer _colorBuffer;

	private ComputeBuffer _argsBuffer;

	private uint[] _argsData;

	private int _positionBufferShaderID = Shader.PropertyToID("_PositionBuffer");

	private int _colorBufferShaderID = Shader.PropertyToID("_ColorBuffer");

	private int _localToWorldShaderID = Shader.PropertyToID("_LocalToWorld");

	private int _scaleShaderID = Shader.PropertyToID("_Scale");

	private int _maxLineCount = 1;

	private Matrix4x4 _matrix = Matrix4x4.identity;

	private float _lineScaleFactor = 1f;

	private int Copies
	{
		get
		{
			if (!_renderSinglePass)
			{
				return 1;
			}
			return 2;
		}
	}

	private int BufferSize => _maxLineCount * 2 * Copies;

	public float LineScaleFactor
	{
		get
		{
			return _lineScaleFactor;
		}
		set
		{
			_lineScaleFactor = value;
		}
	}

	public PolylineRenderer(Material material = null, bool renderSinglePass = true)
	{
		_renderSinglePass = renderSinglePass;
		if (material == null)
		{
			material = new Material(Shader.Find("Custom/PolylineUnlit"));
		}
		_material = new Material(material);
		GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
		_baseMesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
		Object.DestroyImmediate(gameObject);
		_positions = new Vector4[BufferSize];
		_colors = new Color[BufferSize];
		_positionBuffer = new ComputeBuffer(BufferSize, 16);
		_positionBuffer.SetData(_positions);
		_colorBuffer = new ComputeBuffer(BufferSize, 16);
		_colorBuffer.SetData(_colors);
		_material.SetBuffer(_positionBufferShaderID, _positionBuffer);
		_material.SetBuffer(_colorBufferShaderID, _colorBuffer);
		_argsData = new uint[5];
		_argsData[0] = _baseMesh.GetIndexCount(0);
		_argsData[1] = (uint)(_maxLineCount * Copies);
		_argsBuffer = new ComputeBuffer(1, _argsData.Length * 4, ComputeBufferType.DrawIndirect);
		_argsBuffer.SetData(_argsData);
		_positionsNeedUpdate = true;
		_colorsNeedUpdate = true;
	}

	public void Cleanup()
	{
		_positionBuffer.Release();
		_colorBuffer.Release();
		_argsBuffer.Release();
		if (Application.isPlaying)
		{
			Object.Destroy(_material);
		}
		else
		{
			Object.DestroyImmediate(_material);
		}
	}

	public void SetLines(List<Vector4> positions, Color color)
	{
		SetPositions(positions.Count, positions);
		SetDrawCount(positions.Count / 2);
		SetColor(positions.Count, color);
	}

	public void SetLines(List<Vector4> positions, List<Color> colors, int maxCount = -1)
	{
		int num = ((maxCount < 0) ? positions.Count : maxCount);
		SetPositions(num, positions);
		SetDrawCount(num / 2);
		SetColors(num, colors);
	}

	private void SetPositions(int count, List<Vector4> positions)
	{
		if (count * Copies > _positions.Length)
		{
			_maxLineCount = count / 2;
			_positions = new Vector4[BufferSize];
			_positionBuffer.Release();
			_positionBuffer = new ComputeBuffer(BufferSize, 16);
			_positionBuffer.SetData(_positions);
		}
		_bounds = default(Bounds);
		Vector3 min = Vector3.zero;
		Vector3 max = Vector3.zero;
		for (int i = 0; i < count; i += 2)
		{
			for (int j = 0; j < 2; j++)
			{
				Vector4 vector = positions[i + j];
				for (int k = 0; k < Copies; k++)
				{
					_positions[(i + k) * Copies + j] = vector;
				}
				Vector3 vector2 = vector.w * Vector3.one;
				Vector3 vector3 = vector;
				Vector3 vector4 = vector3 - vector2 / 2f;
				Vector3 vector5 = vector3 + vector2 / 2f;
				if (i == 0)
				{
					min = vector4;
					max = vector5;
					continue;
				}
				min.x = Mathf.Min(vector4.x, min.x);
				min.y = Mathf.Min(vector4.y, min.y);
				min.z = Mathf.Min(vector4.z, min.z);
				max.x = Mathf.Max(vector5.x, max.x);
				max.y = Mathf.Max(vector5.y, max.y);
				max.z = Mathf.Max(vector5.z, max.z);
			}
		}
		_bounds.SetMinMax(min, max);
		_positionsNeedUpdate = true;
	}

	private void SetColors(int count, List<Color> colors)
	{
		PrepareColorBuffer(count);
		for (int i = 0; i < count; i += 2)
		{
			for (int j = 0; j < 2; j++)
			{
				for (int k = 0; k < Copies; k++)
				{
					_colors[(i + k) * Copies + j] = colors[i + j];
				}
			}
		}
		_colorsNeedUpdate = true;
	}

	private void SetColor(int count, Color color)
	{
		PrepareColorBuffer(count);
		for (int i = 0; i < count; i += 2)
		{
			for (int j = 0; j < 2; j++)
			{
				for (int k = 0; k < Copies; k++)
				{
					_colors[(i + k) * Copies + j] = color;
				}
			}
		}
		_colorsNeedUpdate = true;
	}

	private void SetDrawCount(int c)
	{
		_argsData[1] = (uint)(c * Copies);
		_argsBuffer.SetData(_argsData);
	}

	private void PrepareColorBuffer(int count)
	{
		if (count * Copies > _colors.Length)
		{
			_maxLineCount = count / 2;
			_colors = new Color[BufferSize];
			_colorBuffer.Release();
			_colorBuffer = new ComputeBuffer(BufferSize, 16);
			_colorBuffer.SetData(_colors);
		}
	}

	public void RenderLines()
	{
		if (_positionsNeedUpdate)
		{
			_positionBuffer.SetData(_positions);
			_material.SetBuffer(_positionBufferShaderID, _positionBuffer);
			_positionsNeedUpdate = false;
		}
		if (_colorsNeedUpdate)
		{
			_colorBuffer.SetData(_colors);
			_material.SetBuffer(_colorBufferShaderID, _colorBuffer);
			_colorsNeedUpdate = false;
		}
		_material.SetFloat(_scaleShaderID, _lineScaleFactor);
		_material.SetMatrix(_localToWorldShaderID, _matrix);
		Graphics.DrawMeshInstancedIndirect(bounds: new Bounds(_matrix.MultiplyPoint(_bounds.center), _matrix.MultiplyVector(_bounds.size)), mesh: _baseMesh, submeshIndex: 0, material: _material, bufferWithArgs: _argsBuffer);
	}

	public void SetTransform(Transform transform)
	{
		_matrix = transform.localToWorldMatrix;
	}
}
