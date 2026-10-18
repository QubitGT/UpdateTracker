using System;
using UnityEngine;

namespace Meta.XR.MRUtilityKit;

[HelpURL("https://developers.meta.com/horizon/reference/mruk/latest/class_meta_x_r_m_r_utility_kit_grid_slice_resizer")]
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class GridSliceResizer : MonoBehaviour
{
	public enum Method
	{
		SLICE,
		SLICE_WITH_ASYMMETRICAL_BORDER,
		SCALE
	}

	[Flags]
	public enum StretchCenterAxis
	{
		X = 1,
		Y = 2,
		Z = 4
	}

	[Tooltip("Represents the offset from the pivot point of the mesh. This offset is used to adjust the origin of scaling operations.")]
	public Vector3 PivotOffset;

	[Tooltip("Specifies the proportion of the mesh along the positive X-axis that is protected from scaling.")]
	[Space(15f)]
	public Method ScalingX;

	[Tooltip("Specifies the proportion of the mesh along the negative X-axis that is protected from scaling.")]
	[Range(0f, 1f)]
	public float BorderXNegative;

	[Tooltip("Specifies the proportion of the mesh along the positive X-axis that is protected from scaling.")]
	[Range(0f, 1f)]
	public float BorderXPositive;

	[Tooltip(" Defines the scaling method to be applied along the Y-axis of the mesh.")]
	[Space(15f)]
	public Method ScalingY;

	[Tooltip("Specifies the proportion of the mesh along the negative Y-axis that is protected from scaling.")]
	[Range(0f, 1f)]
	public float BorderYNegative;

	[Tooltip("Specifies the proportion of the mesh along the positive Y-axis that is protected from scaling.")]
	[Range(0f, 1f)]
	public float BorderYPositive;

	[Tooltip("Defines the scaling method to be applied along the Z-axis of the mesh.")]
	[Space(15f)]
	public Method ScalingZ;

	[Tooltip("Specifies the proportion of the mesh along the negative Z-axis that is protected from scaling.")]
	[Range(0f, 1f)]
	public float BorderZNegative;

	[Tooltip("Specifies the proportion of the mesh along the positive Z-axis that is protected from scaling.")]
	[Range(0f, 1f)]
	public float BorderZPositive;

	[Tooltip("Specifies which axes should allow the center part of the object to stretch.This setting is used to control the stretching behavior of the central section of the mesh allowing for selective stretching along specified axes.")]
	public StretchCenterAxis StretchCenter;

	[Tooltip("Indicates whether the resizer should update the mesh in play mode.When set to true, the mesh will continue to be updated based on the scaling settings during runtime.This can be useful for dynamic scaling effects but may impact performance if used excessively.")]
	public bool UpdateInPlayMode = true;

	[Tooltip("The original mesh before any modifications. This mesh is used as the baseline for all scaling operations")]
	public Mesh OriginalMesh;

	private readonly Color[] _axisGizmosColors = new Color[3]
	{
		new Color(1f, 0f, 0f, 0.5f),
		new Color(0f, 1f, 0f, 0.5f),
		new Color(0f, 0f, 1f, 0.5f)
	};

	private float _cachedBorderXNegative;

	private float _cachedBorderXPositive;

	private float _cachedBorderYNegative;

	private float _cachedBorderYPositive;

	private float _cachedBorderZNegative;

	private float _cachedBorderZPositive;

	private const float _minBorderSize = 0.01f;

	private MeshFilter _meshFilter;

	private Vector3 _currentSize;

	private Bounds _boundingBox;

	private Bounds _scaledBoundingBox;

	private Matrix4x4 _pivotTransform = Matrix4x4.identity;

	private Matrix4x4 _scaledInvPivotTransform = Matrix4x4.identity;

	private Mesh _currentMesh;

	private MeshCollider _meshCollider;

	private void Awake()
	{
		_meshFilter = GetComponent<MeshFilter>();
		if (OriginalMesh != null)
		{
			_currentMesh = OriginalMesh;
			_meshFilter.sharedMesh = OriginalMesh;
		}
		else
		{
			_currentMesh = (OriginalMesh = _meshFilter.sharedMesh);
		}
		_currentSize = OriginalMesh.bounds.size;
		_cachedBorderXNegative = BorderXNegative;
		_cachedBorderYNegative = BorderYNegative;
		_cachedBorderZNegative = BorderZNegative;
		_cachedBorderXPositive = BorderXPositive;
		_cachedBorderYPositive = BorderYPositive;
		_cachedBorderZPositive = BorderZPositive;
		_meshCollider = GetComponent<MeshCollider>();
	}

	private void Start()
	{
		OVRTelemetry.Start(651896136, 0, -1L).Send();
	}

	public void Update()
	{
		if ((Application.isPlaying && !UpdateInPlayMode) || !_meshFilter || !ShouldResize())
		{
			return;
		}
		Mesh mesh = ProcessVertices();
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
		mesh.RecalculateTangents();
		mesh.Optimize();
		_meshFilter.sharedMesh = mesh;
		_currentSize = base.transform.lossyScale;
		UpdateCachedValues();
		if (!_meshCollider)
		{
			TryGetComponent<MeshCollider>(out _meshCollider);
			if (!_meshCollider)
			{
				return;
			}
		}
		_meshCollider.sharedMesh = null;
		_meshCollider.sharedMesh = mesh;
	}

	private void OnDestroy()
	{
		if ((bool)OriginalMesh)
		{
			_meshFilter.mesh = OriginalMesh;
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		float num = OriginalMesh.bounds.max.x / 10f;
		Vector3 vector = base.transform.TransformPoint(PivotOffset);
		Vector3 from = vector + Vector3.left * num * 0.5f;
		Vector3 from2 = vector + Vector3.down * num * 0.5f;
		Vector3 from3 = vector + Vector3.back * num * 0.5f;
		Gizmos.DrawRay(from, Vector3.right * num);
		Gizmos.DrawRay(from2, Vector3.up * num);
		Gizmos.DrawRay(from3, Vector3.forward * num);
	}

	private void OnDrawGizmosSelected()
	{
		if (!(_meshFilter == null))
		{
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Method[] array = new Method[3] { ScalingX, ScalingY, ScalingZ };
			float[] array2 = new float[3] { BorderXNegative, BorderYNegative, BorderZNegative };
			float[] array3 = new float[3] { BorderXPositive, BorderYPositive, BorderZPositive };
			Vector3 lossyScale = base.transform.lossyScale;
			Vector3 b = new Vector3(1f / lossyScale.x, 1f / lossyScale.y, 1f / lossyScale.z);
			Vector3 vector = Vector3.Scale(OriginalMesh.bounds.min, b);
			Vector3 vector2 = Vector3.Scale(OriginalMesh.bounds.max, b);
			new Bounds((vector + vector2) * 0.5f, vector2 - vector);
			for (int i = 0; i <= 2; i++)
			{
				Gizmos.color = _axisGizmosColors[i];
				DrawBorderCubeGizmo(array[i], array2[i], array3[i], i);
			}
			Gizmos.color = new Color(0f, 1f, 1f, 0.5f);
			Gizmos.DrawWireCube(_boundingBox.center, _boundingBox.size);
		}
	}

	public Mesh ProcessVertices()
	{
		Mesh currentMesh = _currentMesh;
		Vector3 lossyScale = base.transform.lossyScale;
		BorderXNegative = Mathf.Max(BorderXNegative, 0.01f);
		BorderYNegative = Mathf.Max(BorderYNegative, 0.01f);
		BorderZNegative = Mathf.Max(BorderZNegative, 0.01f);
		BorderXPositive = Mathf.Max(BorderXPositive, 0.01f);
		BorderYPositive = Mathf.Max(BorderYPositive, 0.01f);
		BorderZPositive = Mathf.Max(BorderZPositive, 0.01f);
		_pivotTransform.SetColumn(3, -PivotOffset);
		_scaledInvPivotTransform.SetColumn(3, Vector3.Scale(lossyScale, PivotOffset));
		Vector3 vector = _pivotTransform.MultiplyPoint3x4(Vector3.Min(currentMesh.bounds.min, PivotOffset));
		Vector3 vector2 = _pivotTransform.MultiplyPoint3x4(Vector3.Max(currentMesh.bounds.max, PivotOffset));
		_boundingBox = new Bounds((vector + vector2) * 0.5f, vector2 - vector);
		_scaledBoundingBox = ScaleBounds(_boundingBox, base.transform.lossyScale);
		Vector3 a = new Vector3(1f / lossyScale.x, 1f / lossyScale.y, 1f / lossyScale.z);
		Vector3 vector3 = new Vector3(BorderXPositive, BorderYPositive, BorderZPositive);
		Vector3 vector4 = new Vector3(BorderXNegative, BorderYNegative, BorderZNegative);
		Method[] array = new Method[3] { ScalingX, ScalingY, ScalingZ };
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		Vector3 zero3 = Vector3.zero;
		Vector3 zero4 = Vector3.zero;
		Vector3 zero5 = Vector3.zero;
		Vector3 zero6 = Vector3.zero;
		Vector3 zero7 = Vector3.zero;
		Vector3 zero8 = Vector3.zero;
		Vector3 zero9 = Vector3.zero;
		Vector3 zero10 = Vector3.zero;
		Vector3 zero11 = Vector3.zero;
		Vector3 zero12 = Vector3.zero;
		bool[] array2 = new bool[3]
		{
			(StretchCenter & StretchCenterAxis.X) != 0,
			(StretchCenter & StretchCenterAxis.Y) != 0,
			(StretchCenter & StretchCenterAxis.Z) != 0
		};
		for (int i = 0; i < 3; i++)
		{
			switch (array[i])
			{
			case Method.SCALE:
			{
				array2[i] = true;
				int index = i;
				float value = (vector3[i] = 1f);
				vector4[index] = value;
				break;
			}
			case Method.SLICE:
				vector3[i] = vector4[i];
				break;
			}
			float num2 = _boundingBox.max[i];
			float num3 = _boundingBox.min[i];
			zero[i] = num2 - (1f - vector3[i]) * Mathf.Abs(num2);
			zero2[i] = num3 + (1f - vector4[i]) * Mathf.Abs(num3);
			zero3[i] = Mathf.Abs(num2 - zero[i]);
			zero4[i] = Mathf.Abs(num3 - zero2[i]);
			zero5[i] = num2 - zero3[i];
			zero6[i] = num3 + zero4[i];
			zero7[i] = _scaledBoundingBox.max[i] - zero3[i];
			zero8[i] = _scaledBoundingBox.min[i] + zero4[i];
			zero9[i] = Mathf.Max(0f, zero7[i] / zero5[i]);
			zero10[i] = Mathf.Max(0f, zero8[i] / zero6[i]);
			zero11[i] = _scaledBoundingBox.max[i] / zero3[i];
			zero12[i] = _scaledBoundingBox.min[i] / zero4[i];
		}
		Vector3[] vertices = currentMesh.vertices;
		Vector3[] array3 = new Vector3[vertices.Length];
		bool[] array4 = new bool[3];
		for (int j = 0; j < vertices.Length; j++)
		{
			Vector3 vector5 = (array3[j] = _pivotTransform.MultiplyPoint3x4(vertices[j]));
			for (int k = 0; k < 3; k++)
			{
				if (0f <= vector5[k] && vector5[k] <= zero[k] && vector5[k] > zero7[k])
				{
					array4[k] = true;
				}
				else if (zero2[k] <= vector5[k] && vector5[k] <= 0f && vector5[k] < zero8[k])
				{
					array4[k] = true;
				}
			}
		}
		for (int l = 0; l < array3.Length; l++)
		{
			Vector3 b = array3[l];
			for (int m = 0; m < 3; m++)
			{
				if (vector4[m] == 0f || vector3[m] == 0f)
				{
					continue;
				}
				if (0f <= b[m] && b[m] <= zero[m] && (array2[m] || array4[m]))
				{
					b[m] *= zero9[m];
				}
				else if (zero2[m] <= b[m] && b[m] <= 0f && (array2[m] || array4[m]))
				{
					b[m] *= zero10[m];
				}
				else if (zero[m] < b[m])
				{
					b[m] = zero[m] * zero9[m] + (b[m] - zero[m]);
					if (zero7[m] < 0f)
					{
						b[m] *= zero11[m];
					}
				}
				else if (b[m] < zero2[m])
				{
					b[m] = zero2[m] * zero10[m] - (zero2[m] - b[m]);
					if (zero8[m] > 0f)
					{
						b[m] *= 0f - zero12[m];
					}
				}
				vertices[l] = Vector3.Scale(a, b);
			}
		}
		Mesh mesh = UnityEngine.Object.Instantiate(currentMesh);
		mesh.vertices = vertices;
		return mesh;
	}

	private bool ShouldResize()
	{
		if (!(_currentSize != base.transform.lossyScale) && !(Math.Abs(_cachedBorderXNegative - BorderXNegative) > Mathf.Epsilon) && !(Math.Abs(_cachedBorderYNegative - BorderYNegative) > Mathf.Epsilon) && !(Math.Abs(_cachedBorderZNegative - BorderZNegative) > Mathf.Epsilon) && !(Math.Abs(_cachedBorderXPositive - BorderXPositive) > Mathf.Epsilon) && !(Math.Abs(_cachedBorderYPositive - BorderYPositive) > Mathf.Epsilon))
		{
			return Math.Abs(_cachedBorderZPositive - BorderZPositive) > Mathf.Epsilon;
		}
		return true;
	}

	private void UpdateCachedValues()
	{
		_cachedBorderXNegative = BorderXNegative;
		_cachedBorderXPositive = BorderXPositive;
		_cachedBorderYNegative = BorderYNegative;
		_cachedBorderYPositive = BorderYPositive;
		_cachedBorderZNegative = BorderZNegative;
		_cachedBorderZPositive = BorderZPositive;
	}

	private void DrawBorderCubeGizmo(Method scalingMethod, float borderNegative, float borderPositive, int axis)
	{
		Vector3 lossyScale = base.transform.lossyScale;
		Bounds originalScaledBounds = ScaleBounds(scale: new Vector3(1f / lossyScale.x, 1f / lossyScale.y, 1f / lossyScale.z), originalBounds: _boundingBox);
		Vector3 size = _boundingBox.size;
		switch (scalingMethod)
		{
		case Method.SLICE:
			DrawPositiveDrawBorderForAxis(borderNegative, axis, originalScaledBounds, size);
			DrawNegativeBorderForAxis(borderNegative, axis, originalScaledBounds, size);
			break;
		case Method.SLICE_WITH_ASYMMETRICAL_BORDER:
			DrawPositiveDrawBorderForAxis(borderPositive, axis, originalScaledBounds, size);
			DrawNegativeBorderForAxis(borderNegative, axis, originalScaledBounds, size);
			break;
		case Method.SCALE:
			break;
		}
	}

	private void DrawNegativeBorderForAxis(float borderNegative, int axis, Bounds originalScaledBounds, Vector3 boundingBoxSize)
	{
		boundingBoxSize[axis] = 0f;
		Vector3 center = _boundingBox.center;
		center[axis] = _boundingBox.min[axis] - (originalScaledBounds.min[axis] - ((0f - Mathf.Abs(originalScaledBounds.min[axis] - PivotOffset[axis])) * borderNegative + PivotOffset[axis]));
		if (center[axis] - PivotOffset[axis] > 0f)
		{
			center[axis] = _boundingBox.min[axis];
		}
		if (PivotOffset[axis] < _boundingBox.min[axis])
		{
			center[axis] = Mathf.Max(PivotOffset[axis] * base.transform.lossyScale[axis], center[axis]);
		}
		Gizmos.DrawWireCube(center, boundingBoxSize);
	}

	private void DrawPositiveDrawBorderForAxis(float borderNegative, int axis, Bounds originalScaledBounds, Vector3 boundingBoxSize)
	{
		boundingBoxSize[axis] = 0f;
		Vector3 center = _boundingBox.center;
		center[axis] = _boundingBox.max[axis] - (originalScaledBounds.max[axis] - (Mathf.Abs(originalScaledBounds.max[axis] - PivotOffset[axis]) * borderNegative + PivotOffset[axis]));
		if (center[axis] + PivotOffset[axis] < 0f)
		{
			center[axis] = _boundingBox.max[axis];
		}
		if (PivotOffset[axis] > _boundingBox.max[axis])
		{
			center[axis] = Mathf.Min(PivotOffset[axis] * base.transform.lossyScale[axis], center[axis]);
		}
		Gizmos.DrawWireCube(center, boundingBoxSize);
	}

	private Bounds ScaleBounds(Bounds originalBounds, Vector3 scale)
	{
		Vector3 vector = Vector3.Scale(originalBounds.min, scale);
		Vector3 vector2 = Vector3.Scale(originalBounds.max, scale);
		return new Bounds((vector + vector2) * 0.5f, vector2 - vector);
	}
}
