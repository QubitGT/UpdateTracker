using UnityEngine;

namespace Meta.WitAi.Data;

public class AudioBufferPrefabProvider : MonoBehaviour, IAudioBufferProvider
{
	[SerializeField]
	private AudioBuffer _audioBufferPrefab;

	private void Awake()
	{
		AudioBuffer.AudioBufferProvider = this;
	}

	public AudioBuffer InstantiateAudioBuffer()
	{
		if (_audioBufferPrefab == null)
		{
			return null;
		}
		GameObject obj = Object.Instantiate(_audioBufferPrefab.gameObject, null, worldPositionStays: true);
		obj.name = _audioBufferPrefab.gameObject.name;
		return obj.GetComponent<AudioBuffer>();
	}
}
