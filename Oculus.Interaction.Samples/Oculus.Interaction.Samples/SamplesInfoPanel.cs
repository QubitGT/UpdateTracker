using UnityEngine;

namespace Oculus.Interaction.Samples;

public class SamplesInfoPanel : MonoBehaviour
{
	public void HandleUrlButton(string url)
	{
		Application.OpenURL(url);
	}
}
