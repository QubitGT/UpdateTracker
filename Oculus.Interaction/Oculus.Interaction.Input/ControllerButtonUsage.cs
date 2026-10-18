using System;

namespace Oculus.Interaction.Input;

[Flags]
public enum ControllerButtonUsage
{
	None = 0,
	PrimaryButton = 1,
	PrimaryTouch = 2,
	SecondaryButton = 4,
	SecondaryTouch = 8,
	GripButton = 0x10,
	TriggerButton = 0x20,
	MenuButton = 0x40,
	Primary2DAxisClick = 0x80,
	Primary2DAxisTouch = 0x100,
	Thumbrest = 0x200
}
