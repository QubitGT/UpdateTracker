using UnityEngine;

[AddComponentMenu("GorillaTag/Ghost Reactor/GR Debug Godmode Button")]
public class GRDebugGodmodeButton : GorillaPressableReleaseButton
{
	private void Awake()
	{
		base.gameObject.SetActive(value: false);
	}

	public void OnPressedButton()
	{
	}

	public override void ButtonActivation()
	{
		base.ButtonActivation();
		UpdateColor();
	}

	public override void ButtonDeactivation()
	{
		base.ButtonDeactivation();
		UpdateColor();
	}
}
