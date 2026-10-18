using UnityEngine.XR.Interaction.Toolkit.Filtering;
using UnityEngine.XR.Interaction.Toolkit.Utilities.Internal;

namespace UnityEngine.XR.Interaction.Toolkit.Inputs;

[AddComponentMenu("XR/XR Hand Skeleton Poke Displacer", 11)]
[HelpURL("https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.2/api/UnityEngine.XR.Interaction.Toolkit.Inputs.XRHandSkeletonPokeDisplacer.html")]
public class XRHandSkeletonPokeDisplacer : MonoBehaviour
{
	private const float k_MinSmoothingAmount = 0f;

	private const float k_MaxSmoothingAmount = 30f;

	[SerializeField]
	[RequireInterface(typeof(IPokeStateDataProvider))]
	[Tooltip("Poke interactor reference used to get poke data.")]
	private Object m_PokeInteractorObject;

	private IPokeStateDataProvider m_PokeInteractor;

	[SerializeField]
	[Range(0f, 1f)]
	[Tooltip("Threshold poke interaction strength must be above to snap the poke pose to the current pose.")]
	private float m_PokeStrengthSnapThreshold = 0.01f;

	[SerializeField]
	[Range(0f, 30f)]
	[Tooltip("Smoothing to apply to the offset root. If smoothing amount is 0, no smoothing will be applied.")]
	private float m_SmoothingAmount = 16f;

	[SerializeField]
	[Tooltip("Additional offset subtracted along the poke interaction axis to apply to the root pose when poking. Default value accounts for the width of the finger mesh.")]
	private float m_FixedOffset = 0.005f;

	public IPokeStateDataProvider pokeInteractor
	{
		get
		{
			return m_PokeInteractor;
		}
		set
		{
			m_PokeInteractorObject = value as Object;
			m_PokeInteractor = value;
		}
	}

	public float pokeStrengthSnapThreshold
	{
		get
		{
			return m_PokeStrengthSnapThreshold;
		}
		set
		{
			m_PokeStrengthSnapThreshold = Mathf.Clamp01(value);
		}
	}

	public float smoothingAmount
	{
		get
		{
			return m_SmoothingAmount;
		}
		set
		{
			m_SmoothingAmount = Mathf.Clamp(value, 0f, 30f);
		}
	}

	public float fixedOffset
	{
		get
		{
			return m_FixedOffset;
		}
		set
		{
			m_FixedOffset = value;
		}
	}

	protected void Awake()
	{
		if (m_PokeInteractor == null)
		{
			m_PokeInteractor = m_PokeInteractorObject as IPokeStateDataProvider;
		}
		Debug.LogWarning("XRHandSkeletonPokeDisplacer requires XR Hands (com.unity.xr.hands) 1.3.0 or newer. Disabling component.", this);
		base.enabled = false;
	}

	protected void OnEnable()
	{
		if (m_PokeInteractor == null)
		{
			m_PokeInteractor = m_PokeInteractorObject as IPokeStateDataProvider;
		}
	}

	protected void OnDisable()
	{
	}

	protected void Update()
	{
	}
}
