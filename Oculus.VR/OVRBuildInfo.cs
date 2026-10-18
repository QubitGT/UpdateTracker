using UnityEngine;
using UnityEngine.UI;

public class OVRBuildInfo : MonoBehaviour
{
	public Text BuildInfo;

	public void OnEnable()
	{
		LoadBuildInfo();
	}

	private void OnValidate()
	{
		LoadBuildInfo();
	}

	public void LoadBuildInfo()
	{
		TextAsset textAsset = Resources.Load<TextAsset>("BuildInfo");
		BuildInfo.text = textAsset?.text ?? "";
	}
}
