using UnityEngine;

namespace Photon.Voice.Unity.UtilityScripts;

[RequireComponent(typeof(Recorder))]
public class TestTone : MonoBehaviour
{
	private void Start()
	{
		Recorder component = base.gameObject.GetComponent<Recorder>();
		component.SourceType = Recorder.InputSourceType.Factory;
		component.InputFactory = () => new ToneAudioReader();
	}
}
