using System;

namespace Modio.Settings;

[Serializable]
public class TempModInstallationSettings : IModioServiceSettings
{
	public int LifeTimeDays;
}
