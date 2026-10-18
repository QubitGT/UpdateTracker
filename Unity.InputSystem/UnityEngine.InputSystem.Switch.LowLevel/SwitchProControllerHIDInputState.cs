using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

namespace UnityEngine.InputSystem.Switch.LowLevel;

[StructLayout(LayoutKind.Explicit, Size = 7)]
internal struct SwitchProControllerHIDInputState : IInputStateTypeInfo
{
	public enum Button
	{
		Up = 0,
		Right = 1,
		Down = 2,
		Left = 3,
		West = 4,
		North = 5,
		South = 6,
		East = 7,
		L = 8,
		R = 9,
		StickL = 10,
		StickR = 11,
		ZL = 12,
		ZR = 13,
		Plus = 14,
		Minus = 15,
		Capture = 16,
		Home = 17,
		X = 5,
		B = 6,
		Y = 4,
		A = 7
	}

	public static FourCC Format = new FourCC('S', 'P', 'V', 'S');

	[FieldOffset(0)]
	[InputControl(name = "leftStick", layout = "Stick", format = "VC2B")]
	[InputControl(name = "leftStick/x", offset = 0u, format = "BYTE", parameters = "normalize,normalizeMin=0.15,normalizeMax=0.85,normalizeZero=0.5")]
	[InputControl(name = "leftStick/left", offset = 0u, format = "BYTE", parameters = "normalize,normalizeMin=0.15,normalizeMax=0.85,normalizeZero=0.5,clamp=1,clampMin=0.15,clampMax=0.5,invert")]
	[InputControl(name = "leftStick/right", offset = 0u, format = "BYTE", parameters = "normalize,normalizeMin=0.15,normalizeMax=0.85,normalizeZero=0.5,clamp=1,clampMin=0.5,clampMax=0.85")]
	[InputControl(name = "leftStick/y", offset = 1u, format = "BYTE", parameters = "invert,normalize,normalizeMin=0.15,normalizeMax=0.85,normalizeZero=0.5")]
	[InputControl(name = "leftStick/up", offset = 1u, format = "BYTE", parameters = "normalize,normalizeMin=0.15,normalizeMax=0.85,normalizeZero=0.5,clamp=1,clampMin=0.15,clampMax=0.5,invert")]
	[InputControl(name = "leftStick/down", offset = 1u, format = "BYTE", parameters = "normalize,normalizeMin=0.15,normalizeMax=0.85,normalizeZero=0.5,clamp=1,clampMin=0.5,clampMax=0.85,invert=false")]
	public byte leftStickX;

	[FieldOffset(1)]
	public byte leftStickY;

	[FieldOffset(2)]
	[InputControl(name = "rightStick", layout = "Stick", format = "VC2B")]
	[InputControl(name = "rightStick/x", offset = 0u, format = "BYTE", parameters = "normalize,normalizeMin=0.15,normalizeMax=0.85,normalizeZero=0.5")]
	[InputControl(name = "rightStick/left", offset = 0u, format = "BYTE", parameters = "normalize,normalizeMin=0.15,normalizeMax=0.85,normalizeZero=0.5,clamp=1,clampMin=0,clampMax=0.5,invert")]
	[InputControl(name = "rightStick/right", offset = 0u, format = "BYTE", parameters = "normalize,normalizeMin=0.15,normalizeMax=0.85,normalizeZero=0.5,clamp=1,clampMin=0.5,clampMax=1")]
	[InputControl(name = "rightStick/y", offset = 1u, format = "BYTE", parameters = "invert,normalize,normalizeMin=0.15,normalizeMax=0.85,normalizeZero=0.5")]
	[InputControl(name = "rightStick/up", offset = 1u, format = "BYTE", parameters = "normalize,normalizeMin=0.15,normalizeMax=0.85,normalizeZero=0.5,clamp=1,clampMin=0.15,clampMax=0.5,invert")]
	[InputControl(name = "rightStick/down", offset = 1u, format = "BYTE", parameters = "normalize,normalizeMin=0.15,normalizeMax=0.85,normalizeZero=0.5,clamp=1,clampMin=0.5,clampMax=0.85,invert=false")]
	public byte rightStickX;

	[FieldOffset(3)]
	public byte rightStickY;

	[FieldOffset(4)]
	[InputControl(name = "dpad", format = "BIT", bit = 0u, sizeInBits = 4u)]
	[InputControl(name = "dpad/up", bit = 0u)]
	[InputControl(name = "dpad/right", bit = 1u)]
	[InputControl(name = "dpad/down", bit = 2u)]
	[InputControl(name = "dpad/left", bit = 3u)]
	[InputControl(name = "buttonWest", displayName = "Y", shortDisplayName = "Y", bit = 4u, usage = "SecondaryAction")]
	[InputControl(name = "buttonNorth", displayName = "X", shortDisplayName = "X", bit = 5u)]
	[InputControl(name = "buttonSouth", displayName = "B", shortDisplayName = "B", bit = 6u, usages = new string[] { "Back", "Cancel" })]
	[InputControl(name = "buttonEast", displayName = "A", shortDisplayName = "A", bit = 7u, usages = new string[] { "PrimaryAction", "Submit" })]
	[InputControl(name = "leftShoulder", displayName = "L", shortDisplayName = "L", bit = 8u)]
	[InputControl(name = "rightShoulder", displayName = "R", shortDisplayName = "R", bit = 9u)]
	[InputControl(name = "leftStickPress", displayName = "Left Stick", bit = 10u)]
	[InputControl(name = "rightStickPress", displayName = "Right Stick", bit = 11u)]
	[InputControl(name = "leftTrigger", displayName = "ZL", shortDisplayName = "ZL", format = "BIT", bit = 12u)]
	[InputControl(name = "rightTrigger", displayName = "ZR", shortDisplayName = "ZR", format = "BIT", bit = 13u)]
	[InputControl(name = "start", displayName = "Plus", bit = 14u, usage = "Menu")]
	[InputControl(name = "select", displayName = "Minus", bit = 15u)]
	public ushort buttons1;

	[FieldOffset(6)]
	[InputControl(name = "capture", layout = "Button", displayName = "Capture", bit = 0u)]
	[InputControl(name = "home", layout = "Button", displayName = "Home", bit = 1u)]
	public byte buttons2;

	public FourCC format => Format;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SwitchProControllerHIDInputState WithButton(Button button, bool value = true)
	{
		Set(button, value);
		return this;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Set(Button button, bool state)
	{
		if (button < Button.Capture)
		{
			ushort num = (ushort)(1 << (int)button);
			if (state)
			{
				buttons1 |= num;
			}
			else
			{
				buttons1 &= (ushort)(~num);
			}
		}
		else if (button < (Button)18)
		{
			byte b = (byte)(1 << (int)(button - 16));
			if (state)
			{
				buttons2 |= b;
			}
			else
			{
				buttons2 &= (byte)(~b);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Press(Button button)
	{
		Set(button, state: true);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Release(Button button)
	{
		Set(button, state: false);
	}
}
