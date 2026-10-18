using System.Collections.Generic;
using Liv.Lck.Settings;
using UnityEngine;
using UnityEngine.Rendering;

namespace Liv.Lck.Core;

public static class LckCoreHandler
{
	internal static Result<bool> LckCoreInitializationResult { get; private set; }

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
	private static void Initialize()
	{
		InitializeInternal();
	}

	private static void InitializeInternal()
	{
		LckSettings instance = LckSettings.Instance;
		LckCore.SetMaxLogLevel(instance.CoreLogLevel);
		if (instance.CoreLogLevel == LevelFilter.Info)
		{
			Debug.Log("LCK Core Handler initializing...");
		}
		IReadOnlyCollection<InteractionSystemDetector.InteractionSystem> availableInteractionSystems = InteractionSystemDetector.GetAvailableInteractionSystems();
		string interactionSystems = ((availableInteractionSystems.Count == 0) ? "Unknown" : string.Join(";", availableInteractionSystems));
		GameInfo gameInfo = default(GameInfo);
		gameInfo.GameName = instance.GameName;
		gameInfo.GameVersion = Application.version;
		gameInfo.ProjectName = Application.productName;
		gameInfo.CompanyName = Application.companyName;
		gameInfo.EngineVersion = Application.unityVersion;
		gameInfo.RenderPipeline = GetRenderPipelineType();
		gameInfo.GraphicsAPI = SystemInfo.graphicsDeviceType.ToString();
		gameInfo.Platform = Application.platform.ToString();
		gameInfo.PersistentDataPath = Application.persistentDataPath;
		gameInfo.InteractionSystems = interactionSystems;
		GameInfo gameInfo2 = gameInfo;
		LckInfo lckInfo = default(LckInfo);
		lckInfo.Version = "1.4.6";
		lckInfo.BuildNumber = -1;
		LckInfo lckInfo2 = lckInfo;
		LckCoreInitializationResult = LckCore.Initialize(instance.TrackingId, gameInfo2, lckInfo2);
		if (!LckCoreInitializationResult.IsOk)
		{
			if (LckCoreInitializationResult.Err == CoreError.MissingTrackingId || LckCoreInitializationResult.Err == CoreError.InvalidTrackingId)
			{
				Debug.LogError("LCK: Missing or bad Tracking ID supplied. Recording and streaming will not be available.");
			}
			else
			{
				Debug.LogError($"LCK: LCK Core initialization failed: {LckCoreInitializationResult.Err} - {LckCoreInitializationResult.Message}");
			}
		}
		else
		{
			LckLog.OnLckCoreInitialized();
		}
	}

	private static string GetRenderPipelineType()
	{
		if ((bool)GraphicsSettings.defaultRenderPipeline)
		{
			if (GraphicsSettings.defaultRenderPipeline.GetType().ToString().Contains("HDRenderPipelineAsset"))
			{
				return "High Definition render pipeline";
			}
			if (GraphicsSettings.defaultRenderPipeline.GetType().ToString().Contains("UniversalRenderPipelineAsset"))
			{
				return "Universal render pipeline";
			}
			return "Custom render pipeline";
		}
		return "Built-in render pipeline";
	}
}
