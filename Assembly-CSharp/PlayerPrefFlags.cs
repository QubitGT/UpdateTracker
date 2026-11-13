using System;
using UnityEngine;

public class PlayerPrefFlags
{
	[Flags]
	public enum Flag
	{
		SHOW_1P_COSMETICS = 1,
		SWAP_HELD_COSMETICS = 2,
		GAME_MODE_SELECTOR_IS_SUPER = 4,
		GTV_MUTED = 8,
		ANTI_NAUSEA_ON = 0x10,
		GRAVDASH_FLIP_X = 0x20,
		GRAVDASH_FLIP_Y = 0x40
	}

	public static Action<Flag, bool> OnFlagChange;

	private const int defaultValue = 1;

	internal static bool Check(Flag flag)
	{
		return ((uint)PlayerPrefs.GetInt("PlayerPrefFlags0", 1) & (uint)flag) == (uint)flag;
	}

	internal static void Touch(Flag flag)
	{
		bool arg = ((uint)PlayerPrefs.GetInt("PlayerPrefFlags0", 1) & (uint)flag) == (uint)flag;
		OnFlagChange?.Invoke(flag, arg);
	}

	internal static void TouchIf(Flag flag, bool value)
	{
		int @int = PlayerPrefs.GetInt("PlayerPrefFlags0", 1);
		if (value == (((uint)@int & (uint)flag) == (uint)flag))
		{
			OnFlagChange?.Invoke(flag, value);
		}
	}

	internal static void Set(Flag flag, bool value)
	{
		int @int = PlayerPrefs.GetInt("PlayerPrefFlags0", 1);
		@int = ((!value) ? (@int & (int)(~flag)) : (@int | (int)flag));
		PlayerPrefs.SetInt("PlayerPrefFlags0", @int);
		OnFlagChange?.Invoke(flag, value);
	}

	internal static bool Flip(Flag flag)
	{
		int @int = PlayerPrefs.GetInt("PlayerPrefFlags0", 1);
		bool flag2 = ((uint)@int & (uint)flag) != (uint)flag;
		@int = ((!flag2) ? (@int & (int)(~flag)) : (@int | (int)flag));
		PlayerPrefs.SetInt("PlayerPrefFlags0", @int);
		OnFlagChange?.Invoke(flag, flag2);
		return flag2;
	}
}
