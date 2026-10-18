namespace UnityEngine.XR.OpenXR.Features.Extensions.PerformanceSettings;

public struct PerformanceChangeNotification
{
	public PerformanceDomain domain;

	public PerformanceSubDomain subDomain;

	public PerformanceNotificationLevel fromLevel;

	public PerformanceNotificationLevel toLevel;
}
