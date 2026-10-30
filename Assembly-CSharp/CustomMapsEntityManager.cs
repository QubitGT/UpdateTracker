using GorillaNetworking;
using UnityEngine;

[AddComponentMenu("GorillaTag/Virtual Stump/Custom Maps Entity Manager")]
public class CustomMapsEntityManager : GameEntityManager
{
	private static bool IsOverrideEnabled()
	{
		GorillaServer instance = GorillaServer.Instance;
		if (instance != null)
		{
			return instance.CheckIsVStumpGrabbablesFixEnabled();
		}
		return false;
	}

	public override bool IsPositionInManagerBounds(Vector3 pos)
	{
		if (CustomMapLoader.CanLoadEntities && IsOverrideEnabled())
		{
			return true;
		}
		return base.IsPositionInManagerBounds(pos);
	}

	protected override bool IsInZone()
	{
		if (!CustomMapLoader.CanLoadEntities || !IsOverrideEnabled())
		{
			return base.IsInZone();
		}
		bool flag = true;
		for (int i = 0; i < zoneComponents.Count; i++)
		{
			flag &= zoneComponents[i].IsZoneReady();
		}
		return flag;
	}
}
