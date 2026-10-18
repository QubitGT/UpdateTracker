using System;
using UnityEngine;

namespace GT_CustomMapSupportRuntime;

[DisallowMultipleComponent]
public class MapDescriptor : MonoBehaviour
{
	[Obsolete("Moved to Map Export Settings")]
	public bool IsInitialScene;

	[Obsolete("Moved to Map Export Settings")]
	public bool DisableHoldingHandsAllGameModes;

	[Obsolete("Moved to Map Export Settings")]
	public bool DisableHoldingHandsCustomOnly;

	[Obsolete("Moved to Map Export Settings")]
	public float watchHoldDuration = 0.5f;

	[Obsolete("Moved to Map Export Settings")]
	public bool watchShouldTagPlayer;

	[Obsolete("Moved to Map Export Settings")]
	public bool watchShouldKickPlayer;

	[Obsolete("Moved to Map Export Settings")]
	public bool watchInfectionOverride;

	[Obsolete("Moved to Map Export Settings")]
	public float watchHoldDuration_Infection = 0.5f;

	[Obsolete("Moved to Map Export Settings")]
	public bool watchShouldTagPlayer_Infection;

	[Obsolete("Moved to Map Export Settings")]
	public bool watchShouldKickPlayer_Infection;

	[Obsolete("Moved to Map Export Settings")]
	public bool watchCustomModeOverride;

	[Obsolete("Moved to Map Export Settings")]
	public float watchHoldDuration_CustomMode = 0.5f;

	[Obsolete("Moved to Map Export Settings")]
	public bool watchShouldTagPlayer_CustomMode;

	[Obsolete("Moved to Map Export Settings")]
	public bool watchShouldKickPlayer_CustomMode;

	[Obsolete("Moved to Map Export Settings")]
	public bool UseUberShaderDynamicLighting;

	[Obsolete("Moved to Map Export Settings")]
	public Color UberShaderAmbientDynamicLight = Color.black;

	[Obsolete("Moved to Map Export Settings")]
	public TextAsset? CustomGamemode;

	[Obsolete("Moved to Map Export Settings")]
	public bool DevMode;

	[Obsolete("Moved to Map Export Settings")]
	public int MaxPlayers = 10;

	public const float MIN_DIAMETER = 1000f;

	public const float MAX_DIAMETER = 50000f;

	[Tooltip("If \"AddSkybox\" is enabled, a skybox will automatically be added to your scene prior to export.")]
	public bool AddSkybox = true;

	[Range(1000f, 50000f)]
	[Tooltip("Set the size of the skybox.")]
	public float SkyboxDiameter = 1000f;

	[Tooltip("If \"CustomSkybox\" texture is valid, it will be used on the added skybox, otherwise the skybox will use the \"Bobbie\\Outer\" shader along with \"CustomSkyboxTint\".")]
	public Cubemap? CustomSkybox;

	[Tooltip("If \"CustomSkybox\" texture is set to None, this color will be used to Tint the skybox")]
	public Color CustomSkyboxTint = new Color(0.224f, 0.424f, 0.839f, 1f);

	[Tooltip("How should lighting be baked/exported?\n 1. Default_Unity - this will bake lighting using Unity's built-in system\n 2. Alternative - this will not trigger a light bake and will NOT delete Lightmapping data before exporting (use this option if you use a 3rd party baker like Bakery)\n 3. Off - this will not bake lighting and will delete Lightmapping data before exporting")]
	public ExportLightingType LightingExportType;

	[Tooltip("If \"ExportAllObjects\" is enabled, any objects that aren't a child object of your MapDescriptor will be automatically re-parented to the MapDescriptor GameObject prior to export.")]
	public bool ExportAllObjects = true;

	[Obsolete("Moved to Map Export Settings")]
	public VirtualStumpReturnWatchProps GetReturnToVStumpWatchProps()
	{
		VirtualStumpReturnWatchProps result = default(VirtualStumpReturnWatchProps);
		result.holdDuration = watchHoldDuration;
		result.shouldTagPlayer = watchShouldTagPlayer;
		result.shouldKickPlayer = watchShouldKickPlayer;
		result.infectionOverride = watchInfectionOverride;
		result.holdDuration_Infection = watchHoldDuration_Infection;
		result.shouldTagPlayer_Infection = watchShouldTagPlayer_Infection;
		result.shouldKickPlayer_Infection = watchShouldKickPlayer_Infection;
		result.customModeOverride = watchCustomModeOverride;
		result.holdDuration_Custom = watchHoldDuration_CustomMode;
		result.shouldTagPlayer_CustomMode = watchShouldTagPlayer_CustomMode;
		result.shouldKickPlayer_CustomMode = watchShouldKickPlayer_CustomMode;
		return result;
	}
}
