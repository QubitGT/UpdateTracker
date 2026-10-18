using UnityEngine;

namespace Oculus.Interaction.Demo;

public class BasicPBRGlobals : MonoBehaviour
{
	[SerializeField]
	private Light _mainlight;

	private void Update()
	{
		UpateShaderGlobals();
	}

	private void UpateShaderGlobals()
	{
		Light mainlight = _mainlight;
		bool flag = (bool)mainlight && mainlight.isActiveAndEnabled;
		Shader.SetGlobalVector("_BasicPBRLightDir", flag ? mainlight.transform.forward : Vector3.down);
		Shader.SetGlobalColor("_BasicPBRLightColor", flag ? mainlight.color : Color.black);
	}
}
