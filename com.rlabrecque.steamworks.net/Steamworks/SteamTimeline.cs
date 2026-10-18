namespace Steamworks;

public static class SteamTimeline
{
	public static void SetTimelineTooltip(string pchDescription, float flTimeDelta)
	{
		InteropHelp.TestIfAvailableClient();
		using InteropHelp.UTF8StringHandle pchDescription2 = new InteropHelp.UTF8StringHandle(pchDescription);
		NativeMethods.ISteamTimeline_SetTimelineTooltip(CSteamAPIContext.GetSteamTimeline(), pchDescription2, flTimeDelta);
	}

	public static void ClearTimelineTooltip(float flTimeDelta)
	{
		InteropHelp.TestIfAvailableClient();
		NativeMethods.ISteamTimeline_ClearTimelineTooltip(CSteamAPIContext.GetSteamTimeline(), flTimeDelta);
	}

	public static void SetTimelineGameMode(ETimelineGameMode eMode)
	{
		InteropHelp.TestIfAvailableClient();
		NativeMethods.ISteamTimeline_SetTimelineGameMode(CSteamAPIContext.GetSteamTimeline(), eMode);
	}

	public static TimelineEventHandle_t AddInstantaneousTimelineEvent(string pchTitle, string pchDescription, string pchIcon, uint unIconPriority, float flStartOffsetSeconds = 0f, ETimelineEventClipPriority ePossibleClip = ETimelineEventClipPriority.k_ETimelineEventClipPriority_None)
	{
		InteropHelp.TestIfAvailableClient();
		using InteropHelp.UTF8StringHandle pchTitle2 = new InteropHelp.UTF8StringHandle(pchTitle);
		using InteropHelp.UTF8StringHandle pchDescription2 = new InteropHelp.UTF8StringHandle(pchDescription);
		using InteropHelp.UTF8StringHandle pchIcon2 = new InteropHelp.UTF8StringHandle(pchIcon);
		return (TimelineEventHandle_t)NativeMethods.ISteamTimeline_AddInstantaneousTimelineEvent(CSteamAPIContext.GetSteamTimeline(), pchTitle2, pchDescription2, pchIcon2, unIconPriority, flStartOffsetSeconds, ePossibleClip);
	}

	public static TimelineEventHandle_t AddRangeTimelineEvent(string pchTitle, string pchDescription, string pchIcon, uint unIconPriority, float flStartOffsetSeconds = 0f, float flDuration = 0f, ETimelineEventClipPriority ePossibleClip = ETimelineEventClipPriority.k_ETimelineEventClipPriority_None)
	{
		InteropHelp.TestIfAvailableClient();
		using InteropHelp.UTF8StringHandle pchTitle2 = new InteropHelp.UTF8StringHandle(pchTitle);
		using InteropHelp.UTF8StringHandle pchDescription2 = new InteropHelp.UTF8StringHandle(pchDescription);
		using InteropHelp.UTF8StringHandle pchIcon2 = new InteropHelp.UTF8StringHandle(pchIcon);
		return (TimelineEventHandle_t)NativeMethods.ISteamTimeline_AddRangeTimelineEvent(CSteamAPIContext.GetSteamTimeline(), pchTitle2, pchDescription2, pchIcon2, unIconPriority, flStartOffsetSeconds, flDuration, ePossibleClip);
	}

	public static TimelineEventHandle_t StartRangeTimelineEvent(string pchTitle, string pchDescription, string pchIcon, uint unPriority, float flStartOffsetSeconds, ETimelineEventClipPriority ePossibleClip)
	{
		InteropHelp.TestIfAvailableClient();
		using InteropHelp.UTF8StringHandle pchTitle2 = new InteropHelp.UTF8StringHandle(pchTitle);
		using InteropHelp.UTF8StringHandle pchDescription2 = new InteropHelp.UTF8StringHandle(pchDescription);
		using InteropHelp.UTF8StringHandle pchIcon2 = new InteropHelp.UTF8StringHandle(pchIcon);
		return (TimelineEventHandle_t)NativeMethods.ISteamTimeline_StartRangeTimelineEvent(CSteamAPIContext.GetSteamTimeline(), pchTitle2, pchDescription2, pchIcon2, unPriority, flStartOffsetSeconds, ePossibleClip);
	}

	public static void UpdateRangeTimelineEvent(TimelineEventHandle_t ulEvent, string pchTitle, string pchDescription, string pchIcon, uint unPriority, ETimelineEventClipPriority ePossibleClip)
	{
		InteropHelp.TestIfAvailableClient();
		using InteropHelp.UTF8StringHandle pchTitle2 = new InteropHelp.UTF8StringHandle(pchTitle);
		using InteropHelp.UTF8StringHandle pchDescription2 = new InteropHelp.UTF8StringHandle(pchDescription);
		using InteropHelp.UTF8StringHandle pchIcon2 = new InteropHelp.UTF8StringHandle(pchIcon);
		NativeMethods.ISteamTimeline_UpdateRangeTimelineEvent(CSteamAPIContext.GetSteamTimeline(), ulEvent, pchTitle2, pchDescription2, pchIcon2, unPriority, ePossibleClip);
	}

	public static void EndRangeTimelineEvent(TimelineEventHandle_t ulEvent, float flEndOffsetSeconds)
	{
		InteropHelp.TestIfAvailableClient();
		NativeMethods.ISteamTimeline_EndRangeTimelineEvent(CSteamAPIContext.GetSteamTimeline(), ulEvent, flEndOffsetSeconds);
	}

	public static void RemoveTimelineEvent(TimelineEventHandle_t ulEvent)
	{
		InteropHelp.TestIfAvailableClient();
		NativeMethods.ISteamTimeline_RemoveTimelineEvent(CSteamAPIContext.GetSteamTimeline(), ulEvent);
	}

	public static SteamAPICall_t DoesEventRecordingExist(TimelineEventHandle_t ulEvent)
	{
		InteropHelp.TestIfAvailableClient();
		return (SteamAPICall_t)NativeMethods.ISteamTimeline_DoesEventRecordingExist(CSteamAPIContext.GetSteamTimeline(), ulEvent);
	}

	public static void StartGamePhase()
	{
		InteropHelp.TestIfAvailableClient();
		NativeMethods.ISteamTimeline_StartGamePhase(CSteamAPIContext.GetSteamTimeline());
	}

	public static void EndGamePhase()
	{
		InteropHelp.TestIfAvailableClient();
		NativeMethods.ISteamTimeline_EndGamePhase(CSteamAPIContext.GetSteamTimeline());
	}

	public static void SetGamePhaseID(string pchPhaseID)
	{
		InteropHelp.TestIfAvailableClient();
		using InteropHelp.UTF8StringHandle pchPhaseID2 = new InteropHelp.UTF8StringHandle(pchPhaseID);
		NativeMethods.ISteamTimeline_SetGamePhaseID(CSteamAPIContext.GetSteamTimeline(), pchPhaseID2);
	}

	public static SteamAPICall_t DoesGamePhaseRecordingExist(string pchPhaseID)
	{
		InteropHelp.TestIfAvailableClient();
		using InteropHelp.UTF8StringHandle pchPhaseID2 = new InteropHelp.UTF8StringHandle(pchPhaseID);
		return (SteamAPICall_t)NativeMethods.ISteamTimeline_DoesGamePhaseRecordingExist(CSteamAPIContext.GetSteamTimeline(), pchPhaseID2);
	}

	public static void AddGamePhaseTag(string pchTagName, string pchTagIcon, string pchTagGroup, uint unPriority)
	{
		InteropHelp.TestIfAvailableClient();
		using InteropHelp.UTF8StringHandle pchTagName2 = new InteropHelp.UTF8StringHandle(pchTagName);
		using InteropHelp.UTF8StringHandle pchTagIcon2 = new InteropHelp.UTF8StringHandle(pchTagIcon);
		using InteropHelp.UTF8StringHandle pchTagGroup2 = new InteropHelp.UTF8StringHandle(pchTagGroup);
		NativeMethods.ISteamTimeline_AddGamePhaseTag(CSteamAPIContext.GetSteamTimeline(), pchTagName2, pchTagIcon2, pchTagGroup2, unPriority);
	}

	public static void SetGamePhaseAttribute(string pchAttributeGroup, string pchAttributeValue, uint unPriority)
	{
		InteropHelp.TestIfAvailableClient();
		using InteropHelp.UTF8StringHandle pchAttributeGroup2 = new InteropHelp.UTF8StringHandle(pchAttributeGroup);
		using InteropHelp.UTF8StringHandle pchAttributeValue2 = new InteropHelp.UTF8StringHandle(pchAttributeValue);
		NativeMethods.ISteamTimeline_SetGamePhaseAttribute(CSteamAPIContext.GetSteamTimeline(), pchAttributeGroup2, pchAttributeValue2, unPriority);
	}

	public static void OpenOverlayToGamePhase(string pchPhaseID)
	{
		InteropHelp.TestIfAvailableClient();
		using InteropHelp.UTF8StringHandle pchPhaseID2 = new InteropHelp.UTF8StringHandle(pchPhaseID);
		NativeMethods.ISteamTimeline_OpenOverlayToGamePhase(CSteamAPIContext.GetSteamTimeline(), pchPhaseID2);
	}

	public static void OpenOverlayToTimelineEvent(TimelineEventHandle_t ulEvent)
	{
		InteropHelp.TestIfAvailableClient();
		NativeMethods.ISteamTimeline_OpenOverlayToTimelineEvent(CSteamAPIContext.GetSteamTimeline(), ulEvent);
	}
}
