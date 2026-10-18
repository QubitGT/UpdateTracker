using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modio.Unity.UI.Input;

public class ModioUIPromptIconResolver : MonoBehaviour
{
	[Serializable]
	private class KeyboardMapping
	{
		public string controlPath;

		public Sprite icon;

		public string displayAsText;
	}

	[Serializable]
	private class PlatformSprites
	{
		[SerializeField]
		public List<RuntimePlatform> forControllerTypes;

		public Sprite buttonSouth;

		public Sprite buttonNorth;

		public Sprite buttonEast;

		public Sprite buttonWest;

		public Sprite startButton;

		public Sprite selectButton;

		public Sprite leftTrigger;

		public Sprite rightTrigger;

		public Sprite leftShoulder;

		public Sprite rightShoulder;

		public Sprite dpad;

		public Sprite dpadUp;

		public Sprite dpadDown;

		public Sprite dpadLeft;

		public Sprite dpadRight;

		public Sprite leftStick;

		public Sprite rightStick;

		public Sprite leftStickPress;

		public Sprite rightStickPress;

		public Sprite GetSprite(string controlPath)
		{
			return controlPath switch
			{
				"buttonSouth" => buttonSouth, 
				"buttonNorth" => buttonNorth, 
				"buttonEast" => buttonEast, 
				"buttonWest" => buttonWest, 
				"start" => startButton, 
				"select" => selectButton, 
				"leftTrigger" => leftTrigger, 
				"rightTrigger" => rightTrigger, 
				"leftShoulder" => leftShoulder, 
				"rightShoulder" => rightShoulder, 
				"dpad" => dpad, 
				"dpad/up" => dpadUp, 
				"dpad/down" => dpadDown, 
				"dpad/left" => dpadLeft, 
				"dpad/right" => dpadRight, 
				"leftStick" => leftStick, 
				"rightStick" => rightStick, 
				"leftStickPress" => leftStickPress, 
				"rightStickPress" => rightStickPress, 
				_ => null, 
			};
		}
	}

	[SerializeField]
	private PlatformSprites[] _platforms;

	[SerializeField]
	private KeyboardMapping[] _keyboardMappings;

	public (Sprite icon, string displayAsText) TryGetKeyboardIcon(string controlPath)
	{
		KeyboardMapping[] keyboardMappings = _keyboardMappings;
		foreach (KeyboardMapping keyboardMapping in keyboardMappings)
		{
			if (keyboardMapping.controlPath == controlPath)
			{
				return (icon: keyboardMapping.icon, displayAsText: keyboardMapping.displayAsText);
			}
		}
		return default((Sprite, string));
	}

	public Sprite ResolveIcon(string controlPath, RuntimePlatform forControllerType)
	{
		PlatformSprites[] platforms = _platforms;
		foreach (PlatformSprites platformSprites in platforms)
		{
			if (platformSprites.forControllerTypes.Contains(forControllerType))
			{
				return platformSprites.GetSprite(controlPath);
			}
		}
		return null;
	}
}
