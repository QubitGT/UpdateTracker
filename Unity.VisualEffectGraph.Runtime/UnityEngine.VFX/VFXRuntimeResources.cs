namespace UnityEngine.VFX;

internal class VFXRuntimeResources : ScriptableObject
{
	[SerializeField]
	private ComputeShader m_SDFRayMapCS;

	[SerializeField]
	private ComputeShader m_SDFNormalsCS;

	[SerializeField]
	private Shader m_SDFRayMapShader;

	internal ComputeShader sdfRayMapCS
	{
		get
		{
			return m_SDFRayMapCS;
		}
		set
		{
			m_SDFRayMapCS = value;
		}
	}

	internal ComputeShader sdfNormalsCS
	{
		get
		{
			return m_SDFNormalsCS;
		}
		set
		{
			m_SDFNormalsCS = value;
		}
	}

	internal Shader sdfRayMapShader
	{
		get
		{
			return m_SDFRayMapShader;
		}
		set
		{
			m_SDFRayMapShader = value;
		}
	}

	public static VFXRuntimeResources runtimeResources => VFXManager.runtimeResources as VFXRuntimeResources;
}
