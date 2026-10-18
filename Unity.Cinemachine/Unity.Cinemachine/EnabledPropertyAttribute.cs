namespace Unity.Cinemachine;

public sealed class EnabledPropertyAttribute : FoldoutWithEnabledButtonAttribute
{
	public string ToggleDisabledText;

	public EnabledPropertyAttribute(string enabledProperty = "Enabled", string toggleText = "")
		: base(enabledProperty)
	{
		ToggleDisabledText = toggleText;
	}
}
