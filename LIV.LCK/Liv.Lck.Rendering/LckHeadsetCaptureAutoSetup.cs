using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Liv.Lck.Rendering;

internal static class LckHeadsetCaptureAutoSetup
{
	internal static bool EnsureFeaturePresent()
	{
		if (LckHeadsetCaptureRenderFeature.IsConfigured)
		{
			return true;
		}
		UniversalRenderPipelineAsset asset = UniversalRenderPipeline.asset;
		if (asset == null)
		{
			LckLog.LogWarning("Cannot auto-add LckHeadsetCaptureRenderFeature: URP pipeline asset is null.", "EnsureFeaturePresent", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Rendering\\LckHeadsetCaptureAutoSetup.cs", 46);
			return false;
		}
		ScriptableRendererData[] rendererDataList = GetRendererDataList(asset);
		if (rendererDataList == null)
		{
			LckLog.LogWarning("LCK could not automatically add LckHeadsetCaptureRenderFeature to your URP renderer. Please add it manually: select your URP Renderer asset, click 'Add Renderer Feature', and choose 'LckHeadsetCaptureRenderFeature'.", "EnsureFeaturePresent", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Rendering\\LckHeadsetCaptureAutoSetup.cs", 53);
			return false;
		}
		bool flag = false;
		ScriptableRendererData[] array = rendererDataList;
		foreach (ScriptableRendererData scriptableRendererData in array)
		{
			if (scriptableRendererData == null)
			{
				LckLog.LogWarning("Cannot auto-add LckHeadsetCaptureRenderFeature: a URP renderer data entry is null.", "EnsureFeaturePresent", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Rendering\\LckHeadsetCaptureAutoSetup.cs", 65);
			}
			else if (!HasFeature<LckHeadsetCaptureRenderFeature>(scriptableRendererData))
			{
				LckHeadsetCaptureRenderFeature item = CreateFeature();
				scriptableRendererData.rendererFeatures.Add(item);
				InvalidateRendererData(scriptableRendererData);
				flag = true;
			}
		}
		if (flag)
		{
			LckLog.Log("LckHeadsetCaptureRenderFeature automatically added to URP renderer at runtime.", "EnsureFeaturePresent", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\Rendering\\LckHeadsetCaptureAutoSetup.cs", 80);
		}
		return LckHeadsetCaptureRenderFeature.IsConfigured;
	}

	private static ScriptableRendererData[] GetRendererDataList(UniversalRenderPipelineAsset asset)
	{
		return asset.rendererDataList.ToArray();
	}

	private static bool HasFeature<T>(ScriptableRendererData rendererData) where T : ScriptableRendererFeature
	{
		foreach (ScriptableRendererFeature rendererFeature in rendererData.rendererFeatures)
		{
			if (rendererFeature is T)
			{
				return true;
			}
		}
		return false;
	}

	private static void InvalidateRendererData(ScriptableRendererData data)
	{
		data.SetDirty();
	}

	private static LckHeadsetCaptureRenderFeature CreateFeature()
	{
		LckHeadsetCaptureRenderFeature lckHeadsetCaptureRenderFeature = ScriptableObject.CreateInstance<LckHeadsetCaptureRenderFeature>();
		lckHeadsetCaptureRenderFeature.name = "LckHeadsetCaptureRenderFeature";
		lckHeadsetCaptureRenderFeature.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
		lckHeadsetCaptureRenderFeature.SetActive(active: true);
		lckHeadsetCaptureRenderFeature.Create();
		return lckHeadsetCaptureRenderFeature;
	}
}
