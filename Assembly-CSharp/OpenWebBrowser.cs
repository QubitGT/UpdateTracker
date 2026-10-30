using UnityEngine;

public class OpenWebBrowser : MonoBehaviour
{
	[SerializeField]
	private string url;

	[SerializeField]
	private bool PC;

	[SerializeField]
	private bool Quest;

	[SerializeField]
	private bool Unknown;

	public void Navigate()
	{
		Application.OpenURL(url);
	}

	private void Start()
	{
		if (selfDisable())
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private bool selfDisable()
	{
		return !PC;
	}
}
