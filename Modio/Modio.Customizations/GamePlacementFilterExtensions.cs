using Modio.API;

namespace Modio.Customizations;

public static class GamePlacementFilterExtensions
{
	public static ModioAPI.Mods.GetModsFilter CustomParameter(this ModioAPI.Mods.GetModsFilter filter, string key, object value)
	{
		if (filter == null || string.IsNullOrEmpty(key))
		{
			return filter;
		}
		if (value == null)
		{
			filter.Parameters.Remove(key);
		}
		else
		{
			filter.Parameters[key] = value;
		}
		return filter;
	}

	public static ModioAPI.Mods.GetModsFilter Timespan(this ModioAPI.Mods.GetModsFilter filter, string timespan)
	{
		return filter.CustomParameter("timespan", string.IsNullOrEmpty(timespan) ? null : timespan);
	}
}
