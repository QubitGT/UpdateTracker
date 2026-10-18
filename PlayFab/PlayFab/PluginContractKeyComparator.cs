using System.Collections.Generic;

namespace PlayFab;

public class PluginContractKeyComparator : EqualityComparer<PluginContractKey>
{
	public override bool Equals(PluginContractKey x, PluginContractKey y)
	{
		if (x._pluginContract == y._pluginContract)
		{
			return x._pluginName.Equals(y._pluginName);
		}
		return false;
	}

	public override int GetHashCode(PluginContractKey obj)
	{
		return (int)(obj._pluginContract + obj._pluginName.GetHashCode());
	}
}
