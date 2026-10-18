using System;
using UnityEngine;
using UnityEngine.Rendering;

public class OVROverlayCanvasSettings : OVRRuntimeAssetsBase
{
	private const string kAssetName = "OVROverlayCanvasSettings";

	private const string kBuiltInOpaqueShaderName = "UI/Prerendered Opaque";

	private const string kUrpOpaqueShaderName = "URP/UI/Prerendered Opaque";

	private const string kBuiltInTransparentShaderName = "UI/Prerendered";

	private const string kUrpTransparentShaderName = "URP/UI/Prerendered";

	private static OVROverlayCanvasSettings _instance;

	[SerializeField]
	private Shader _transparentImposterShader;

	[SerializeField]
	private Shader _opaqueImposterShader;

	public int MaxSimultaneousCanvases = 1;

	public int CanvasRenderLayer = 31;

	public int CanvasLayer = -1;

	public static OVROverlayCanvasSettings Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = GetOverlayCanvasSettings();
			}
			return _instance;
		}
	}

	private static OVROverlayCanvasSettings GetOverlayCanvasSettings()
	{
		OVRRuntimeAssetsBase.LoadAsset(out OVROverlayCanvasSettings assetInstance, "OVROverlayCanvasSettings", (Action<OVROverlayCanvasSettings>)null);
		if (assetInstance == null)
		{
			Debug.LogWarning("Failed to load runtime settings. Using default runtime settings instead.");
			assetInstance = ScriptableObject.CreateInstance<OVROverlayCanvasSettings>();
		}
		assetInstance.EnsureInitialized();
		return assetInstance;
	}

	public void ApplyGlobalSettings()
	{
	}

	public Shader GetShader(OVROverlayCanvas.DrawMode drawMode)
	{
		switch (drawMode)
		{
		case OVROverlayCanvas.DrawMode.Opaque:
		case OVROverlayCanvas.DrawMode.OpaqueWithClip:
		case OVROverlayCanvas.DrawMode.AlphaToMask:
			return _opaqueImposterShader;
		default:
			return _transparentImposterShader;
		}
	}

	private static bool UsingBuiltInRenderPipeline()
	{
		return GraphicsSettings.currentRenderPipeline == null;
	}

	private static void EnsureShaderInitialized(ref Shader shader, string shaderName, string replaceShaderName)
	{
		if (!(shader != null) || !(shader.name != replaceShaderName))
		{
			Shader shader2 = Shader.Find(shaderName);
			if (shader2 == null)
			{
				Debug.LogError("Failed to find shader \"" + shaderName + "\"");
			}
			else
			{
				shader = shader2;
			}
		}
	}

	private void EnsureInitialized()
	{
		bool flag = UsingBuiltInRenderPipeline();
		EnsureShaderInitialized(ref _opaqueImposterShader, flag ? "UI/Prerendered Opaque" : "URP/UI/Prerendered Opaque", flag ? "URP/UI/Prerendered Opaque" : "UI/Prerendered Opaque");
		EnsureShaderInitialized(ref _transparentImposterShader, flag ? "UI/Prerendered" : "URP/UI/Prerendered", flag ? "URP/UI/Prerendered" : "UI/Prerendered");
	}

	private void OnValidate()
	{
		EnsureInitialized();
	}
}
