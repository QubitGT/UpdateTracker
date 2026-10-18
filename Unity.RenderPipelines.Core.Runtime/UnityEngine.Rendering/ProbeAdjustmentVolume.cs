using System;

namespace UnityEngine.Rendering;

[ExecuteAlways]
[AddComponentMenu("Rendering/Probe Adjustment Volume")]
public class ProbeAdjustmentVolume : MonoBehaviour, ISerializationCallbackReceiver
{
	public enum Shape
	{
		Box,
		Sphere
	}

	public enum Mode
	{
		InvalidateProbes = 0,
		OverrideValidityThreshold = 1,
		ApplyVirtualOffset = 2,
		OverrideVirtualOffsetSettings = 3,
		OverrideSkyDirection = 4,
		OverrideSampleCount = 5,
		OverrideRenderingLayerMask = 6,
		IntensityScale = 99
	}

	public enum RenderingLayerMaskOperation
	{
		Override,
		Add,
		Remove
	}

	private enum Version
	{
		Initial,
		Mode,
		Count
	}

	[Tooltip("Select the shape used for this Probe Adjustment Volume.")]
	public Shape shape;

	[Min(0f)]
	[Tooltip("Modify the size of this Probe Adjustment Volume. This is unaffected by the GameObject's Transform's Scale property.")]
	public Vector3 size = new Vector3(1f, 1f, 1f);

	[Min(0f)]
	[Tooltip("Modify the radius of this Probe Adjustment Volume. This is unaffected by the GameObject's Transform's Scale property.")]
	public float radius = 1f;

	public Mode mode;

	[Range(0.0001f, 2f)]
	[Tooltip("A multiplier applied to the intensity of probes covered by this Probe Adjustment Volume.")]
	public float intensityScale = 1f;

	[Range(0f, 0.95f)]
	public float overriddenDilationThreshold = 0.75f;

	public Vector3 virtualOffsetRotation = Vector3.zero;

	[Min(0f)]
	public float virtualOffsetDistance = 1f;

	[Range(0f, 1f)]
	[Tooltip("Determines how far Unity pushes a probe out of geometry after a ray hit.")]
	public float geometryBias = 0.01f;

	[Range(0f, 0.95f)]
	public float virtualOffsetThreshold = 0.75f;

	[Range(-0.05f, 0f)]
	[Tooltip("Distance from the probe position used to determine the origin of the sampling ray.")]
	public float rayOriginBias = -0.001f;

	[Tooltip("The direction for sampling the ambient probe in worldspace when using the Sky Visibility feature.")]
	public Vector3 skyDirection = Vector3.zero;

	internal Vector3 skyShadingDirectionRotation = Vector3.zero;

	[Logarithmic(1, 1024)]
	[Tooltip("Number of samples for direct lighting computations.")]
	public int directSampleCount = 32;

	[Logarithmic(1, 8192)]
	[Tooltip("Number of samples for indirect lighting computations. This includes environment samples.")]
	public int indirectSampleCount = 512;

	[Min(0f)]
	[Tooltip("Multiplier for the number of samples specified above.")]
	public int sampleCountMultiplier = 4;

	[Min(0f)]
	[Tooltip("Maximum number of bounces for indirect lighting.")]
	public int maxBounces = 2;

	[Logarithmic(1, 8192)]
	public int skyOcclusionSampleCount = 2048;

	[Range(0f, 5f)]
	public int skyOcclusionMaxBounces = 2;

	public RenderingLayerMaskOperation renderingLayerMaskOperation;

	public byte renderingLayerMask;

	[SerializeField]
	private Version version = Version.Count;

	[Obsolete("This field is only kept for migration purpose. Use mode instead. #from(2023.1)")]
	public bool invalidateProbes;

	[Obsolete("This field is only kept for migration purpose. Use mode instead. #from(2023.1)")]
	public bool overrideDilationThreshold;

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
		if (version == Version.Count)
		{
			version = Version.Mode;
		}
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		if (version == Version.Count)
		{
			version = Version.Initial;
		}
		if (version < Version.Mode)
		{
			if (invalidateProbes)
			{
				mode = Mode.InvalidateProbes;
			}
			else if (overrideDilationThreshold)
			{
				mode = Mode.OverrideValidityThreshold;
			}
			version = Version.Mode;
		}
	}
}
