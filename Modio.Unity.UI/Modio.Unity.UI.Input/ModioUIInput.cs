using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modio.Unity.UI.Input;

public static class ModioUIInput
{
	public class InputPromptDisplayInfo
	{
		public List<Sprite> Icons { get; private set; }

		public List<string> TextPrompts { get; private set; }

		public bool InputHasListeners { get; private set; }

		public event Action<InputPromptDisplayInfo> OnUpdated;

		public virtual void UpdateInfo(List<string> textPrompts, List<Sprite> icons, bool hasListeners)
		{
			if (textPrompts != null && textPrompts.Count > 0)
			{
				if (TextPrompts == null)
				{
					List<string> list2 = (TextPrompts = new List<string>());
				}
				TextPrompts.Clear();
				TextPrompts.AddRange(textPrompts);
				AnyBindingsExist = true;
			}
			else
			{
				TextPrompts?.Clear();
			}
			if (icons != null && icons.Count > 0)
			{
				if (Icons == null)
				{
					List<Sprite> list4 = (Icons = new List<Sprite>());
				}
				Icons.Clear();
				Icons.AddRange(icons);
				AnyBindingsExist = true;
			}
			else
			{
				Icons?.Clear();
			}
			InputHasListeners = hasListeners;
			this.OnUpdated?.Invoke(this);
		}

		public void UpdateListenerInfo(bool hasListeners)
		{
			if (InputHasListeners != hasListeners)
			{
				InputHasListeners = hasListeners;
				this.OnUpdated?.Invoke(this);
			}
		}
	}

	public enum ModioAction
	{
		Cancel,
		Subscribe,
		Report,
		Filter,
		Sort,
		Search,
		TabLeft,
		TabRight,
		BuyTokens,
		FilterLeft,
		FilterRight,
		FilterClear,
		MoreOptions,
		SearchClear,
		SearchPageLeft,
		SearchPageRight,
		MoreFromThisCreator,
		DeveloperMenu
	}

	private static readonly Dictionary<ModioAction, List<(Action action, int frameAdded)>> Handlers = new Dictionary<ModioAction, List<(Action, int)>>();

	private static readonly Dictionary<ModioAction, InputPromptDisplayInfo> Prompts = new Dictionary<ModioAction, InputPromptDisplayInfo>();

	private static readonly List<Action> CachedHandlersForCurrentCall = new List<Action>();

	public static Func<Vector2> RawCursorProvider;

	public static bool IsUsingGamepad { get; private set; }

	public static bool SuppressNoInputListenerWarning { get; set; }

	public static bool AnyBindingsExist { get; private set; }

	public static event Action<bool> SwappedControlScheme;

	public static void PressedAction(ModioAction action)
	{
		if (!Handlers.TryGetValue(action, out List<(Action, int)> value))
		{
			return;
		}
		CachedHandlersForCurrentCall.Clear();
		foreach (var item in value)
		{
			if (item.Item2 != Time.frameCount)
			{
				CachedHandlersForCurrentCall.Add(item.Item1);
			}
		}
		foreach (Action item2 in CachedHandlersForCurrentCall)
		{
			item2?.Invoke();
		}
		CachedHandlersForCurrentCall.Clear();
	}

	public static void AddHandler(ModioAction action, Action onPressed)
	{
		if (!SuppressNoInputListenerWarning)
		{
			Debug.LogWarning("Modio's input system appears to be running without an input listener. You might not have controller and full keyboard support. Ensure you have ModioUI_InputCapture added to your scene\nIf you are using Unity's InputSystem, you can extract the following file: \"Assets\\Plugins\\ModioUI\\InputPackages\\InputSystem\\ModioInputListener_InputSystem.zip\"");
			SuppressNoInputListenerWarning = true;
		}
		if (!Handlers.TryGetValue(action, out List<(Action, int)> value))
		{
			value = new List<(Action, int)>();
			Handlers[action] = value;
		}
		value.Add((onPressed, Time.frameCount));
		if (value.Count == 1)
		{
			GetInputPromptDisplayInfo(action).UpdateListenerInfo(hasListeners: true);
		}
	}

	public static void RemoveHandler(ModioAction action, Action onPressed)
	{
		if (!Handlers.TryGetValue(action, out List<(Action, int)> value))
		{
			return;
		}
		bool flag = false;
		for (int num = value.Count - 1; num >= 0; num--)
		{
			if (value[num].Item1 == onPressed)
			{
				value.RemoveAt(num);
				flag = true;
			}
		}
		if (flag && value.Count == 0)
		{
			GetInputPromptDisplayInfo(action).UpdateListenerInfo(hasListeners: false);
		}
	}

	public static void ControlSchemeChanged(bool isController)
	{
		IsUsingGamepad = isController;
		ModioUIInput.SwappedControlScheme?.Invoke(isController);
	}

	public static void SetButtonPrompts(ModioAction action, List<string> textPrompts, List<Sprite> icons)
	{
		InputPromptDisplayInfo inputPromptDisplayInfo = GetInputPromptDisplayInfo(action);
		List<(Action, int)> value;
		bool hasListeners = Handlers.TryGetValue(action, out value) && value.Count > 0;
		inputPromptDisplayInfo.UpdateInfo(textPrompts, icons, hasListeners);
	}

	public static InputPromptDisplayInfo GetInputPromptDisplayInfo(ModioAction action)
	{
		if (!Prompts.TryGetValue(action, out var value))
		{
			value = new InputPromptDisplayInfo();
			Prompts[action] = value;
		}
		return value;
	}

	public static Vector2 GetRawCursor()
	{
		return RawCursorProvider?.Invoke() ?? Vector2.zero;
	}
}
