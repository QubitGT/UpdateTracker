using System.Collections.Generic;
using UnityEngine;

namespace Oculus.Interaction.DistanceReticles;

public class DistantInteractionPolylineVisual : DistantInteractionLineVisual
{
	[SerializeField]
	private Color _color = Color.white;

	[SerializeField]
	private float _lineWidth = 0.02f;

	private List<Vector4> _linePointsVec4;

	[SerializeField]
	private Material _lineMaterial;

	private PolylineRenderer _polylineRenderer;

	public Color Color
	{
		get
		{
			return _color;
		}
		set
		{
			_color = value;
		}
	}

	public float LineWidth
	{
		get
		{
			return _lineWidth;
		}
		set
		{
			_lineWidth = value;
		}
	}

	protected override void Start()
	{
		base.Start();
		_polylineRenderer = new PolylineRenderer(_lineMaterial);
		_linePointsVec4 = new List<Vector4>(new Vector4[base.NumLinePoints]);
	}

	private void OnDestroy()
	{
		_polylineRenderer.Cleanup();
	}

	protected override void RenderLine(Vector3[] linePoints)
	{
		for (int i = 0; i < linePoints.Length; i++)
		{
			Vector3 vector = linePoints[i];
			_linePointsVec4[i] = new Vector4(vector.x, vector.y, vector.z, _lineWidth);
		}
		_polylineRenderer.SetLines(_linePointsVec4, _color);
		_polylineRenderer.RenderLines();
	}

	protected override void HideLine()
	{
	}

	public void InjectAllDistantInteractionPolylineVisual(IDistanceInteractor interactor, Color color, Material material)
	{
		InjectDistanceInteractor(interactor);
		InjectLineColor(color);
		InjectLineMaterial(material);
	}

	public void InjectLineColor(Color color)
	{
		_color = color;
	}

	public void InjectLineMaterial(Material material)
	{
		_lineMaterial = material;
	}
}
