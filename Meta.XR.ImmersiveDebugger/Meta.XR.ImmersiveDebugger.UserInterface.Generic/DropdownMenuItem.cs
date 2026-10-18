namespace Meta.XR.ImmersiveDebugger.UserInterface.Generic;

internal class DropdownMenuItem : ButtonWithLabel
{
	private Dropdown _dropdown;

	protected override void Setup(Controller owner)
	{
		base.Setup(owner);
		base.LayoutStyle = Style.Load<LayoutStyle>("DropdownValueItem");
		base.TextStyle = Style.Instantiate<TextStyle>("MemberValue");
		base.BackgroundStyle = Style.Instantiate<ImageStyle>("DropdownValueBackground");
	}

	internal void RegisterDropdownSourceMenu(Dropdown dropdown)
	{
		_dropdown = dropdown;
	}

	public override void OnPointerClick()
	{
		base.OnPointerClick();
		_dropdown?.OnMenuItemClick(this);
	}
}
