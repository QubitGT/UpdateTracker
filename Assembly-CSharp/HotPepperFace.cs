using UnityEngine;

[AddComponentMenu("GorillaTag/Cosmetics/Hot Pepper Face")]
public class HotPepperFace : MonoBehaviour
{
	[SerializeField]
	private GameObject _faceMesh;

	[SerializeField]
	private ParticleSystem _fireFX;

	[SerializeField]
	private AudioSource _flameSpeaker;

	[SerializeField]
	private AudioSource _breathSpeaker;

	[SerializeField]
	private float _effectLength = 1.5f;

	[SerializeField]
	private GameObject _thermalSourceVolume;

	private bool _isPlaying;

	public void PlayFX(float delay)
	{
		if (delay < 0f)
		{
			PlayFX();
			return;
		}
		CancelInvoke("PlayFXDelayed");
		Invoke("PlayFXDelayed", delay);
	}

	private void PlayFXDelayed()
	{
		PlayFX();
	}

	public void PlayFX()
	{
		_faceMesh.SetActive(value: true);
		_thermalSourceVolume.SetActive(value: true);
		_fireFX.Play();
		_flameSpeaker.GTPlay();
		_breathSpeaker.GTPlay();
		_isPlaying = true;
		CancelInvoke("StopFX");
		Invoke("StopFX", _effectLength);
	}

	public void StopFX()
	{
		CancelInvoke("StopFX");
		_isPlaying = false;
		if (_faceMesh != null)
		{
			_faceMesh.SetActive(value: false);
		}
		if (_thermalSourceVolume != null)
		{
			_thermalSourceVolume.SetActive(value: false);
		}
		if (_fireFX != null)
		{
			_fireFX.Stop();
		}
		if (_flameSpeaker != null)
		{
			_flameSpeaker.GTStop();
		}
		if (_breathSpeaker != null)
		{
			_breathSpeaker.GTStop();
		}
	}

	protected void OnDisable()
	{
		CancelInvoke();
		if (_isPlaying)
		{
			StopFX();
		}
	}
}
