using System;
using System.Collections.Generic;
using Oculus.Interaction.Surfaces;
using UnityEngine;

namespace Oculus.Interaction.UnityCanvas;

public class CanvasCylinder : CanvasMesh, ICurvedPlane, ICylinderClipper
{
	[Serializable]
	public struct MeshGenerationSettings
	{
		[Delayed]
		public float VerticesPerDegree;

		[Delayed]
		public int MaxHorizontalResolution;

		[Delayed]
		public int MaxVerticalResolution;
	}

	public const int MIN_RESOLUTION = 2;

	[SerializeField]
	[Tooltip("The cylinder used to dictate the position and radius of the mesh.")]
	private Cylinder _cylinder;

	[SerializeField]
	[Tooltip("Determines how the mesh is projected on the cylinder wall. Vertical results in a left-to-right curvature, Horizontal results in a top-to-bottom curvature.")]
	private CylinderOrientation _orientation;

	[SerializeField]
	private MeshGenerationSettings _meshGeneration = new MeshGenerationSettings
	{
		VerticesPerDegree = 1.4f,
		MaxHorizontalResolution = 128,
		MaxVerticalResolution = 32
	};

	public float Radius => _cylinder.Radius;

	public Cylinder Cylinder => _cylinder;

	public float ArcDegrees { get; private set; }

	public float Rotation { get; private set; }

	public float Bottom { get; private set; }

	public float Top { get; private set; }

	private float CylinderRelativeScale => _cylinder.transform.lossyScale.x / base.transform.lossyScale.x;

	public bool GetCylinderSegment(out CylinderSegment segment)
	{
		segment = new CylinderSegment(Rotation, ArcDegrees, Bottom, Top);
		if (_started)
		{
			return base.isActiveAndEnabled;
		}
		return false;
	}

	protected override void Start()
	{
		this.BeginStart(ref _started, delegate
		{
			base.Start();
		});
		this.EndStart(ref _started);
	}

	protected override void UpdateImposter()
	{
		base.UpdateImposter();
		UpdateMeshPosition();
		UpdateCurvedPlane();
	}

	protected override Vector3 MeshInverseTransform(Vector3 localPosition)
	{
		float x = Mathf.Atan2(localPosition.x, localPosition.z + Radius) * Radius;
		float y = localPosition.y;
		return new Vector3(x, y);
	}

	protected override void GenerateMesh(out List<Vector3> verts, out List<int> tris, out List<Vector2> uvs)
	{
		verts = new List<Vector3>();
		tris = new List<int>();
		uvs = new List<Vector2>();
		Vector2 worldSize = GetWorldSize();
		float scaledRadius = Radius * CylinderRelativeScale;
		float xPos = worldSize.x * 0.5f;
		float xNeg = 0f - xPos;
		float yPos = worldSize.y * 0.5f;
		float yNeg = 0f - yPos;
		CylinderOrientation orientation = _orientation;
		Vector2Int vector2Int = ((orientation != 0 && orientation == CylinderOrientation.Horizontal) ? GetClampedResolution(yPos, xPos) : GetClampedResolution(xPos, yPos));
		for (int i = 0; i < vector2Int.y; i++)
		{
			for (int j = 0; j < vector2Int.x; j++)
			{
				float num = (float)j / ((float)vector2Int.x - 1f);
				float num2 = (float)i / ((float)vector2Int.y - 1f);
				verts.Add(GetCurvedPoint(num, num2));
				uvs.Add(new Vector2(num, num2));
			}
		}
		for (int k = 0; k < vector2Int.y - 1; k++)
		{
			for (int l = 0; l < vector2Int.x - 1; l++)
			{
				int num3 = l + k * vector2Int.x;
				int item = num3 + 1;
				int item2 = num3 + vector2Int.x;
				int item3 = num3 + 1 + vector2Int.x;
				tris.Add(num3);
				tris.Add(item3);
				tris.Add(item);
				tris.Add(num3);
				tris.Add(item2);
				tris.Add(item3);
			}
		}
		Vector2Int GetClampedResolution(float arcMax, float axisMax)
		{
			int num6 = Mathf.Max(2, Mathf.RoundToInt(_meshGeneration.VerticesPerDegree * 57.29578f * arcMax / scaledRadius));
			int value = Mathf.Max(2, Mathf.RoundToInt((float)num6 * axisMax / arcMax));
			num6 = Mathf.Clamp(num6, 2, _meshGeneration.MaxHorizontalResolution);
			value = Mathf.Clamp(value, 2, _meshGeneration.MaxVerticalResolution);
			return new Vector2Int(num6, value);
		}
		Vector3 GetCurvedPoint(float u, float v)
		{
			float num4 = Mathf.Lerp(xNeg, xPos, u);
			float num5 = Mathf.Lerp(yNeg, yPos, v);
			CylinderOrientation orientation2 = _orientation;
			Vector3 result = default(Vector3);
			if (orientation2 == CylinderOrientation.Vertical || orientation2 != CylinderOrientation.Horizontal)
			{
				float f = num4 / scaledRadius;
				result.x = Mathf.Sin(f) * scaledRadius;
				result.y = num5;
				result.z = Mathf.Cos(f) * scaledRadius - scaledRadius;
			}
			else
			{
				float f = num5 / scaledRadius;
				result.x = num4;
				result.y = Mathf.Sin(f) * scaledRadius;
				result.z = Mathf.Cos(f) * scaledRadius - scaledRadius;
			}
			return result;
		}
	}

	private void UpdateMeshPosition()
	{
		Vector3 vector = _cylinder.transform.InverseTransformPoint(base.transform.position);
		Vector3 vector2 = new Vector3(0f, vector.y, 0f);
		Vector3 vector3 = vector - vector2;
		Vector3 vector4 = (Mathf.Approximately(vector3.sqrMagnitude, 0f) ? Vector3.forward : vector3.normalized);
		CylinderOrientation orientation = _orientation;
		Vector3 upwards = ((orientation != 0 && orientation == CylinderOrientation.Horizontal) ? Vector3.right : Vector3.up);
		base.transform.position = _cylinder.transform.TransformPoint(vector4 * _cylinder.Radius + vector2);
		base.transform.rotation = _cylinder.transform.rotation * Quaternion.LookRotation(vector4, upwards);
		if (_meshCollider != null && _meshCollider.transform != base.transform && !base.transform.IsChildOf(_meshCollider.transform))
		{
			_meshCollider.transform.position = base.transform.position;
			_meshCollider.transform.rotation = base.transform.rotation;
			_meshCollider.transform.localScale *= base.transform.lossyScale.x / _meshCollider.transform.lossyScale.x;
		}
	}

	private Vector2 GetWorldSize()
	{
		Vector2Int baseResolutionToUse = _canvasRenderTexture.GetBaseResolutionToUse();
		float x = _canvasRenderTexture.PixelsToUnits(Mathf.RoundToInt(baseResolutionToUse.x));
		float y = _canvasRenderTexture.PixelsToUnits(Mathf.RoundToInt(baseResolutionToUse.y));
		return new Vector2(x, y) / base.transform.lossyScale;
	}

	private void UpdateCurvedPlane()
	{
		Vector2 vector = GetWorldSize() / CylinderRelativeScale;
		CylinderOrientation orientation = _orientation;
		float num;
		float num2;
		if (orientation == CylinderOrientation.Vertical || orientation != CylinderOrientation.Horizontal)
		{
			num = vector.x;
			num2 = vector.y;
		}
		else
		{
			num = vector.y;
			num2 = vector.x;
		}
		Vector3 vector2 = Cylinder.transform.InverseTransformPoint(base.transform.position);
		Rotation = Mathf.Atan2(vector2.x, vector2.z) * 57.29578f;
		ArcDegrees = num * 0.5f / Radius * 2f * 57.29578f;
		Top = vector2.y + num2 * 0.5f;
		Bottom = vector2.y - num2 * 0.5f;
	}

	public void InjectAllCanvasCylinder(CanvasRenderTexture canvasRenderTexture, MeshFilter meshFilter, Cylinder cylinder, CylinderOrientation orientation)
	{
		InjectAllCanvasMesh(canvasRenderTexture, meshFilter);
		InjectCylinder(cylinder);
		InjectOrientation(orientation);
	}

	public void InjectCylinder(Cylinder cylinder)
	{
		_cylinder = cylinder;
	}

	public void InjectOrientation(CylinderOrientation orientation)
	{
		_orientation = orientation;
	}
}
