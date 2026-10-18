using UnityEngine;

namespace Oculus.Interaction;

public class ArcTubeVisual : MonoBehaviour
{
	[Header("Visual renderers")]
	[SerializeField]
	private TubeRenderer _tubeRenderer;

	[Header("Visual parameters")]
	[SerializeField]
	private float _radius = 0.07f;

	[SerializeField]
	private float _minAngle;

	[SerializeField]
	private float _maxAngle = 45f;

	private const float _degreesPerSegment = 1f;

	private static readonly Quaternion _rotationCorrectionLeft = Quaternion.Euler(0f, -90f, 0f);

	protected bool _started;

	protected virtual void Start()
	{
		this.BeginStart(ref _started);
		InitializeVisuals();
		this.EndStart(ref _started);
	}

	private void InitializeVisuals()
	{
		TubePoint[] points = InitializeSegment(new Vector2(_minAngle, _maxAngle));
		_tubeRenderer.RenderTube(points);
	}

	private TubePoint[] InitializeSegment(Vector2 minMaxAngle)
	{
		float x = minMaxAngle.x;
		int num = Mathf.RoundToInt(Mathf.Repeat(minMaxAngle.y - x, 360f) / 1f);
		TubePoint[] array = new TubePoint[num];
		float num2 = 1f / (float)num;
		for (int i = 0; i < num; i++)
		{
			Quaternion quaternion = Quaternion.AngleAxis((float)(-i) * 1f - x, Vector3.up);
			array[i] = new TubePoint
			{
				position = quaternion * Vector3.forward * _radius,
				rotation = quaternion * _rotationCorrectionLeft,
				relativeLength = (float)i * num2
			};
		}
		return array;
	}

	public void InjectAllArcTubeVisual(TubeRenderer tubeRenderer, float radius, float minAngle, float maxAngle)
	{
		InjectTubeRenderer(tubeRenderer);
		InjectRadius(radius);
		InjectMinAngle(minAngle);
		InjectMaxAngle(maxAngle);
	}

	public void InjectTubeRenderer(TubeRenderer tubeRenderer)
	{
		_tubeRenderer = tubeRenderer;
	}

	public void InjectRadius(float radius)
	{
		_radius = radius;
	}

	public void InjectMinAngle(float minAngle)
	{
		_minAngle = minAngle;
	}

	public void InjectMaxAngle(float maxAngle)
	{
		_maxAngle = maxAngle;
	}
}
