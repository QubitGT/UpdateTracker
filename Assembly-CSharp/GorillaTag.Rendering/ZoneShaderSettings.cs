using GT_CustomMapSupportRuntime;
using GorillaExtensions;
using UnityEngine;

namespace GorillaTag.Rendering;

public class ZoneShaderSettings : MonoBehaviour, ITickSystemPost
{
	public enum EOverrideMode
	{
		LeaveUnchanged,
		ApplyNewValue,
		ApplyDefaultValue
	}

	public enum EZoneLiquidType
	{
		None,
		Water,
		Lava
	}

	public enum ELiquidShape
	{
		Plane,
		Cylinder
	}

	[OnEnterPlay_Set(false)]
	private static bool isInitialized;

	[Tooltip("Set this to true for cases like it is the first ZoneShaderSettings that should be activated when entering a scene.")]
	[SerializeField]
	private bool _activateOnAwake;

	[Tooltip("These values will be used as the default global values that will be fallen back to when not in a zone and that the other scripts will reference.")]
	public bool isDefaultValues;

	private static readonly int groundFogColor_shaderProp = Shader.PropertyToID("_ZoneGroundFogColor");

	[SerializeField]
	private EOverrideMode groundFogColor_overrideMode;

	[SerializeField]
	private Color groundFogColor = new Color(0.7f, 0.9f, 1f, 1f);

	private static readonly int groundFogDepthFadeSq_shaderProp = Shader.PropertyToID("_ZoneGroundFogDepthFadeSq");

	[SerializeField]
	private EOverrideMode groundFogDepthFade_overrideMode;

	[SerializeField]
	private float _groundFogDepthFadeSize = 20f;

	private static readonly int groundFogHeight_shaderProp = Shader.PropertyToID("_ZoneGroundFogHeight");

	[SerializeField]
	private EOverrideMode groundFogHeight_overrideMode;

	[SerializeField]
	private float groundFogHeight = 7.45f;

	private static readonly int groundFogHeightFade_shaderProp = Shader.PropertyToID("_ZoneGroundFogHeightFade");

	[SerializeField]
	private EOverrideMode groundFogHeightFade_overrideMode;

	[SerializeField]
	private float _groundFogHeightFadeSize = 20f;

	[SerializeField]
	private EOverrideMode zoneLiquidType_overrideMode;

	[SerializeField]
	private EZoneLiquidType zoneLiquidType = EZoneLiquidType.Water;

	[OnEnterPlay_Set(EZoneLiquidType.None)]
	private static EZoneLiquidType liquidType_previousValue = EZoneLiquidType.None;

	[SerializeField]
	private EOverrideMode liquidShape_overrideMode;

	[SerializeField]
	private ELiquidShape liquidShape;

	[OnEnterPlay_Set(ELiquidShape.Plane)]
	private static ELiquidShape liquidShape_previousValue = ELiquidShape.Plane;

	[SerializeField]
	private EOverrideMode liquidShapeRadius_overrideMode;

	[Tooltip("Radius of the liquid when the shape is a cylinder.")]
	[SerializeField]
	private float liquidShapeRadius = 1f;

	[OnEnterPlay_Set(1f)]
	private static float liquidShapeRadius_previousValue = 1f;

	private bool hasLiquidBottomTransform;

	[SerializeField]
	private EOverrideMode liquidBottomTransform_overrideMode;

	[Tooltip("Only used when the liquid shape is Cylinder: this transform's Y is the bottom of the liquid cylinder. Surfaces below it are not treated as underwater.")]
	[SerializeField]
	private Transform liquidBottomTransform;

	[OnEnterPlay_Set(0f)]
	private static float liquidBottomPosY_previousValue;

	private static readonly int shaderParam_GlobalZoneLiquidUVScale = Shader.PropertyToID("_GlobalZoneLiquidUVScale");

	[SerializeField]
	private EOverrideMode zoneLiquidUVScale_overrideMode;

	[Tooltip("UV scale applied to the zone liquid surface textures.")]
	[SerializeField]
	private float zoneLiquidUVScale = 1f;

	private static readonly int shaderParam_GlobalWaterTintColor = Shader.PropertyToID("_GlobalWaterTintColor");

	[SerializeField]
	private EOverrideMode underwaterTintColor_overrideMode;

	[SerializeField]
	private Color underwaterTintColor = new Color(0.3f, 0.65f, 1f, 0.2f);

	private static readonly int shaderParam_GlobalUnderwaterFogColor = Shader.PropertyToID("_GlobalUnderwaterFogColor");

	[SerializeField]
	private EOverrideMode underwaterFogColor_overrideMode;

	[SerializeField]
	private Color underwaterFogColor = new Color(0.12f, 0.41f, 0.77f);

	private static readonly int shaderParam_GlobalUnderwaterFogParams = Shader.PropertyToID("_GlobalUnderwaterFogParams");

	[SerializeField]
	private EOverrideMode underwaterFogParams_overrideMode;

	[Tooltip("Fog params are: start, distance (end - start), unused, unused")]
	[SerializeField]
	private Vector4 underwaterFogParams = new Vector4(-5f, 40f, 0f, 0f);

	private static readonly int shaderParam_GlobalUnderwaterCausticsParams = Shader.PropertyToID("_GlobalUnderwaterCausticsParams");

	[SerializeField]
	private EOverrideMode underwaterCausticsParams_overrideMode;

	[Tooltip("Caustics params are: speed1, scale, alpha, unused")]
	[SerializeField]
	private Vector4 underwaterCausticsParams = new Vector4(0.075f, 0.075f, 1f, 0f);

	private static readonly int shaderParam_GlobalUnderwaterCausticsTex = Shader.PropertyToID("_GlobalUnderwaterCausticsTex");

	[SerializeField]
	private EOverrideMode underwaterCausticsTexture_overrideMode;

	[SerializeField]
	private Texture2D underwaterCausticsTexture;

	private static readonly int shaderParam_GlobalUnderwaterEffectsDistanceToSurfaceFade = Shader.PropertyToID("_GlobalUnderwaterEffectsDistanceToSurfaceFade");

	[SerializeField]
	private EOverrideMode underwaterEffectsDistanceToSurfaceFade_overrideMode;

	[SerializeField]
	private Vector2 underwaterEffectsDistanceToSurfaceFade = new Vector2(0.0001f, 50f);

	private const string kEdTooltip_liquidResidueTex = "This is used for things like the charred surface effect when lava burns static geo.";

	private static readonly int shaderParam_GlobalLiquidResidueTex = Shader.PropertyToID("_GlobalLiquidResidueTex");

	[SerializeField]
	[Tooltip("This is used for things like the charred surface effect when lava burns static geo.")]
	private EOverrideMode liquidResidueTex_overrideMode;

	[SerializeField]
	[Tooltip("This is used for things like the charred surface effect when lava burns static geo.")]
	private Texture2D liquidResidueTex;

	private static readonly int shaderParam_GlobalMainWaterSurfacePlane = Shader.PropertyToID("_GlobalMainWaterSurfacePlane");

	private bool hasMainWaterSurfacePlane;

	private bool hasDynamicWaterSurfacePlane;

	private Transform resolvedWaterPlane;

	private Transform resolvedLiquidBottom;

	[SerializeField]
	private EOverrideMode mainWaterSurfacePlane_overrideMode;

	[Tooltip("Transform whose position and up axis define the water surface. Surfaces below this plane get the underwater fog, tint and caustics. For a cylinder liquid, its X/Z is also the cylinder's center.")]
	[SerializeField]
	private Transform mainWaterSurfacePlane;

	private static readonly int shaderParam_ZoneWeatherMapDissolveProgress = Shader.PropertyToID("_ZoneWeatherMapDissolveProgress");

	[SerializeField]
	private EOverrideMode zoneWeatherMapDissolveProgress_overrideMode;

	[Tooltip("Dissolve progress of the zone weather map (0 to 1).")]
	[Range(0f, 1f)]
	[SerializeField]
	private float zoneWeatherMapDissolveProgress = 1f;

	[DebugReadout]
	[field: OnEnterPlay_SetNull]
	public static ZoneShaderSettings defaultsInstance { get; private set; }

	[field: OnEnterPlay_Set(false)]
	public static bool hasDefaultsInstance { get; private set; }

	[DebugReadout]
	[field: OnEnterPlay_SetNull]
	public static ZoneShaderSettings activeInstance { get; private set; }

	[field: OnEnterPlay_Set(false)]
	public static bool hasActiveInstance { get; private set; }

	public bool isActiveInstance => activeInstance == this;

	[DebugReadout]
	private float GroundFogDepthFadeSq => 1f / Mathf.Max(1E-05f, _groundFogDepthFadeSize * _groundFogDepthFadeSize);

	[DebugReadout]
	private float GroundFogHeightFade => 1f / Mathf.Max(1E-05f, _groundFogHeightFadeSize);

	public static int shaderParam_ZoneLiquidPosRadiusSq { get; } = Shader.PropertyToID("_ZoneLiquidPosRadiusSq");


	bool ITickSystemPost.PostTickRunning { get; set; }

	private static void SetGlobalKeyword(string keyword, bool enabled)
	{
		if (enabled)
		{
			Shader.EnableKeyword(keyword);
		}
		else
		{
			Shader.DisableKeyword(keyword);
		}
	}

	public void SetZoneLiquidTypeKeywordEnum(EZoneLiquidType liquidType)
	{
		SetGlobalKeyword("_GLOBAL_ZONE_LIQUID_TYPE__NONE", liquidType == EZoneLiquidType.None);
		SetGlobalKeyword("_GLOBAL_ZONE_LIQUID_TYPE__WATER", liquidType == EZoneLiquidType.Water);
		SetGlobalKeyword("_GLOBAL_ZONE_LIQUID_TYPE__LAVA", liquidType == EZoneLiquidType.Lava);
	}

	public void SetZoneLiquidShapeKeywordEnum(ELiquidShape shape)
	{
		SetGlobalKeyword("_ZONE_LIQUID_SHAPE__PLANE", shape == ELiquidShape.Plane);
		SetGlobalKeyword("_ZONE_LIQUID_SHAPE__CYLINDER", shape == ELiquidShape.Cylinder);
	}

	private static void UpdateZoneLiquidCustomKeyword()
	{
		SetGlobalKeyword("_ZONE_LIQUID_CUSTOM", liquidType_previousValue == EZoneLiquidType.Lava || liquidShape_previousValue == ELiquidShape.Cylinder);
	}

	public static float GetWaterY()
	{
		if (!hasActiveInstance || !(activeInstance.resolvedWaterPlane != null))
		{
			return float.NegativeInfinity;
		}
		return activeInstance.resolvedWaterPlane.position.y;
	}

	protected void Awake()
	{
		CacheTransformFlags();
		if (CheckDefaultsInstance() && (_activateOnAwake || activeInstance == this))
		{
			BecomeActiveInstance(activeInstance == this);
		}
	}

	protected void OnEnable()
	{
		if (activeInstance == this && hasDynamicWaterSurfacePlane)
		{
			TickSystem<object>.AddPostTickCallback(this);
		}
	}

	protected void OnDisable()
	{
		TickSystem<object>.RemovePostTickCallback(this);
	}

	protected void OnDestroy()
	{
		if (defaultsInstance == this)
		{
			hasDefaultsInstance = false;
		}
		if (activeInstance == this)
		{
			hasActiveInstance = false;
		}
		TickSystem<object>.RemovePostTickCallback(this);
	}

	private void CacheTransformFlags()
	{
		hasMainWaterSurfacePlane = UsesOwnValue(mainWaterSurfacePlane_overrideMode) && mainWaterSurfacePlane != null;
		hasLiquidBottomTransform = UsesOwnValue(liquidBottomTransform_overrideMode) && liquidBottomTransform != null;
	}

	private bool UsesOwnValue(EOverrideMode mode)
	{
		if (!isDefaultValues)
		{
			return mode == EOverrideMode.ApplyNewValue;
		}
		return true;
	}

	private bool TryResolve<T>(EOverrideMode mode, T value, T defaultValue, out T result)
	{
		if (UsesOwnValue(mode))
		{
			result = value;
			return true;
		}
		if (mode == EOverrideMode.ApplyDefaultValue && hasDefaultsInstance)
		{
			result = defaultValue;
			return true;
		}
		result = default(T);
		return false;
	}

	void ITickSystemPost.PostTick()
	{
		if (activeInstance == this && Application.isPlaying && !ApplicationQuittingState.IsQuitting)
		{
			UpdateMainPlaneShaderProperty();
		}
	}

	private void ResolveLiquidTransforms()
	{
		bool flag = hasDefaultsInstance && defaultsInstance != null;
		TryResolve(mainWaterSurfacePlane_overrideMode, hasMainWaterSurfacePlane ? mainWaterSurfacePlane : null, (flag && defaultsInstance.hasMainWaterSurfacePlane) ? defaultsInstance.mainWaterSurfacePlane : null, out resolvedWaterPlane);
		hasDynamicWaterSurfacePlane = resolvedWaterPlane != null && !resolvedWaterPlane.gameObject.isStatic;
		TryResolve(liquidBottomTransform_overrideMode, hasLiquidBottomTransform ? liquidBottomTransform : null, (flag && defaultsInstance.hasLiquidBottomTransform) ? defaultsInstance.liquidBottomTransform : null, out resolvedLiquidBottom);
		if (TryResolve(liquidShapeRadius_overrideMode, liquidShapeRadius, flag ? defaultsInstance.liquidShapeRadius : liquidShapeRadius, out var result))
		{
			liquidShapeRadius_previousValue = result;
		}
	}

	private void UpdateMainPlaneShaderProperty()
	{
		if (resolvedWaterPlane == null)
		{
			return;
		}
		Vector3 position = resolvedWaterPlane.position;
		Vector3 up = resolvedWaterPlane.up;
		Shader.SetGlobalVector(shaderParam_GlobalMainWaterSurfacePlane, new Vector4(up.x, up.y, up.z, 0f - Vector3.Dot(up, position)));
		if (liquidShape_previousValue == ELiquidShape.Cylinder)
		{
			if (resolvedLiquidBottom != null)
			{
				liquidBottomPosY_previousValue = resolvedLiquidBottom.position.y;
			}
			Shader.SetGlobalVector(shaderParam_ZoneLiquidPosRadiusSq, new Vector4(position.x, liquidBottomPosY_previousValue, position.z, liquidShapeRadius_previousValue * liquidShapeRadius_previousValue));
		}
	}

	private bool CheckDefaultsInstance()
	{
		if (!isDefaultValues)
		{
			return true;
		}
		if (hasDefaultsInstance && defaultsInstance != null && defaultsInstance != this)
		{
			string path = defaultsInstance.transform.GetPath();
			Debug.LogError("ZoneShaderSettings: Destroying conflicting defaults instance.\n- keeping: \"" + path + "\"\n- destroying (this): \"" + base.transform.GetPath() + "\"", this);
			Object.Destroy(base.gameObject);
			return false;
		}
		defaultsInstance = this;
		hasDefaultsInstance = true;
		BecomeActiveInstance();
		return true;
	}

	public void BecomeActiveInstance(bool force = false)
	{
		if (!(activeInstance == this) || force)
		{
			if (activeInstance.IsNotNull())
			{
				TickSystem<object>.RemovePostTickCallback(activeInstance);
			}
			ApplyValues();
			if (hasDynamicWaterSurfacePlane)
			{
				TickSystem<object>.AddPostTickCallback(this);
			}
			activeInstance = this;
			hasActiveInstance = true;
		}
	}

	public static void ActivateDefaultSettings()
	{
		if (hasDefaultsInstance)
		{
			defaultsInstance.BecomeActiveInstance();
		}
	}

	public void SetGroundFogValue(Color fogColor, float fogDepthFade, float fogHeight, float fogHeightFade)
	{
		groundFogColor_overrideMode = EOverrideMode.ApplyNewValue;
		groundFogColor = fogColor;
		groundFogDepthFade_overrideMode = EOverrideMode.ApplyNewValue;
		_groundFogDepthFadeSize = fogDepthFade;
		groundFogHeight_overrideMode = EOverrideMode.ApplyNewValue;
		groundFogHeight = fogHeight;
		groundFogHeightFade_overrideMode = EOverrideMode.ApplyNewValue;
		_groundFogHeightFadeSize = fogHeightFade;
		BecomeActiveInstance(force: true);
	}

	private void ApplyValues()
	{
		if (hasDefaultsInstance && !(defaultsInstance == null) && !ApplicationQuittingState.IsQuitting)
		{
			ZoneShaderSettings zoneShaderSettings = defaultsInstance;
			ApplyColor(groundFogColor_shaderProp, groundFogColor_overrideMode, groundFogColor, zoneShaderSettings.groundFogColor);
			ApplyFloat(groundFogDepthFadeSq_shaderProp, groundFogDepthFade_overrideMode, GroundFogDepthFadeSq, zoneShaderSettings.GroundFogDepthFadeSq);
			ApplyFloat(groundFogHeight_shaderProp, groundFogHeight_overrideMode, groundFogHeight, zoneShaderSettings.groundFogHeight);
			ApplyFloat(groundFogHeightFade_shaderProp, groundFogHeightFade_overrideMode, GroundFogHeightFade, zoneShaderSettings.GroundFogHeightFade);
			if (TryResolve(zoneLiquidType_overrideMode, zoneLiquidType, zoneShaderSettings.zoneLiquidType, out var result) && (result != liquidType_previousValue || !isInitialized))
			{
				SetZoneLiquidTypeKeywordEnum(result);
				liquidType_previousValue = result;
			}
			if (TryResolve(liquidShape_overrideMode, liquidShape, zoneShaderSettings.liquidShape, out var result2) && (result2 != liquidShape_previousValue || !isInitialized))
			{
				SetZoneLiquidShapeKeywordEnum(result2);
				liquidShape_previousValue = result2;
			}
			UpdateZoneLiquidCustomKeyword();
			ApplyFloat(shaderParam_GlobalZoneLiquidUVScale, zoneLiquidUVScale_overrideMode, zoneLiquidUVScale, zoneShaderSettings.zoneLiquidUVScale);
			ApplyColor(shaderParam_GlobalWaterTintColor, underwaterTintColor_overrideMode, underwaterTintColor, zoneShaderSettings.underwaterTintColor);
			ApplyColor(shaderParam_GlobalUnderwaterFogColor, underwaterFogColor_overrideMode, underwaterFogColor, zoneShaderSettings.underwaterFogColor);
			ApplyVector(shaderParam_GlobalUnderwaterFogParams, underwaterFogParams_overrideMode, underwaterFogParams, zoneShaderSettings.underwaterFogParams);
			ApplyVector(shaderParam_GlobalUnderwaterCausticsParams, underwaterCausticsParams_overrideMode, underwaterCausticsParams, zoneShaderSettings.underwaterCausticsParams);
			ApplyTexture(shaderParam_GlobalUnderwaterCausticsTex, underwaterCausticsTexture_overrideMode, underwaterCausticsTexture, zoneShaderSettings.underwaterCausticsTexture);
			ApplyVector(shaderParam_GlobalUnderwaterEffectsDistanceToSurfaceFade, underwaterEffectsDistanceToSurfaceFade_overrideMode, underwaterEffectsDistanceToSurfaceFade, zoneShaderSettings.underwaterEffectsDistanceToSurfaceFade);
			ApplyTexture(shaderParam_GlobalLiquidResidueTex, liquidResidueTex_overrideMode, liquidResidueTex, zoneShaderSettings.liquidResidueTex);
			ApplyFloat(shaderParam_ZoneWeatherMapDissolveProgress, zoneWeatherMapDissolveProgress_overrideMode, zoneWeatherMapDissolveProgress, zoneShaderSettings.zoneWeatherMapDissolveProgress);
			ResolveLiquidTransforms();
			UpdateMainPlaneShaderProperty();
			isInitialized = true;
		}
	}

	private void ApplyColor(int shaderProp, EOverrideMode overrideMode, Color value, Color defaultValue)
	{
		if (TryResolve(overrideMode, value, defaultValue, out var result))
		{
			Shader.SetGlobalColor(shaderProp, result.linear);
		}
	}

	private void ApplyFloat(int shaderProp, EOverrideMode overrideMode, float value, float defaultValue)
	{
		if (TryResolve(overrideMode, value, defaultValue, out var result))
		{
			Shader.SetGlobalFloat(shaderProp, result);
		}
	}

	private void ApplyVector(int shaderProp, EOverrideMode overrideMode, Vector4 value, Vector4 defaultValue)
	{
		if (TryResolve(overrideMode, value, defaultValue, out var result))
		{
			Shader.SetGlobalVector(shaderProp, result);
		}
	}

	private void ApplyTexture(int shaderProp, EOverrideMode overrideMode, Texture2D value, Texture2D defaultValue)
	{
		if (TryResolve(overrideMode, value, defaultValue, out var result))
		{
			Shader.SetGlobalTexture(shaderProp, result);
		}
	}

	public void CopySettings(CMSZoneShaderSettings cmsZoneShaderSettings, bool rerunAwake = false)
	{
		_activateOnAwake = cmsZoneShaderSettings.activateOnLoad;
		if (cmsZoneShaderSettings.applyGroundFog)
		{
			groundFogColor_overrideMode = (EOverrideMode)cmsZoneShaderSettings.GetGroundFogColorOverrideMode();
			groundFogColor = cmsZoneShaderSettings.groundFogColor;
			groundFogHeight_overrideMode = (EOverrideMode)cmsZoneShaderSettings.GetGroundFogHeightOverrideMode();
			if (cmsZoneShaderSettings.groundFogHeightPlane.IsNotNull())
			{
				groundFogHeight = cmsZoneShaderSettings.groundFogHeightPlane.position.y;
			}
			else
			{
				groundFogHeight = cmsZoneShaderSettings.groundFogHeight;
			}
			groundFogHeightFade_overrideMode = (EOverrideMode)cmsZoneShaderSettings.GetGroundFogHeightFadeOverrideMode();
			_groundFogHeightFadeSize = cmsZoneShaderSettings.groundFogHeightFadeSize;
			groundFogDepthFade_overrideMode = (EOverrideMode)cmsZoneShaderSettings.GetGroundFogDepthFadeOverrideMode();
			_groundFogDepthFadeSize = cmsZoneShaderSettings.groundFogDepthFadeSize;
		}
		else
		{
			groundFogColor_overrideMode = EOverrideMode.ApplyNewValue;
			groundFogColor = new Color(0f, 0f, 0f, 0f);
			groundFogHeight_overrideMode = EOverrideMode.ApplyNewValue;
			groundFogHeight = -9999f;
		}
		if (cmsZoneShaderSettings.applyLiquidEffects)
		{
			zoneLiquidType_overrideMode = (EOverrideMode)cmsZoneShaderSettings.GetZoneLiquidTypeOverrideMode();
			zoneLiquidType = (EZoneLiquidType)cmsZoneShaderSettings.GetZoneLiquidType();
			liquidShape_overrideMode = (EOverrideMode)cmsZoneShaderSettings.GetLiquidShapeOverrideMode();
			liquidShape = (ELiquidShape)cmsZoneShaderSettings.GetZoneLiquidShape();
			liquidShapeRadius_overrideMode = (EOverrideMode)cmsZoneShaderSettings.GetLiquidShapeRadiusOverrideMode();
			liquidShapeRadius = cmsZoneShaderSettings.liquidShapeRadius;
			liquidBottomTransform_overrideMode = (EOverrideMode)cmsZoneShaderSettings.GetLiquidBottomTransformOverrideMode();
			liquidBottomTransform = cmsZoneShaderSettings.liquidBottomTransform;
			zoneLiquidUVScale_overrideMode = (EOverrideMode)cmsZoneShaderSettings.GetZoneLiquidUVScaleOverrideMode();
			zoneLiquidUVScale = cmsZoneShaderSettings.zoneLiquidUVScale;
			underwaterTintColor_overrideMode = (EOverrideMode)cmsZoneShaderSettings.GetUnderwaterTintColorOverrideMode();
			underwaterTintColor = cmsZoneShaderSettings.underwaterTintColor;
			underwaterFogColor_overrideMode = (EOverrideMode)cmsZoneShaderSettings.GetUnderwaterFogColorOverrideMode();
			underwaterFogColor = cmsZoneShaderSettings.underwaterFogColor;
			underwaterFogParams_overrideMode = (EOverrideMode)cmsZoneShaderSettings.GetUnderwaterFogParamsOverrideMode();
			underwaterFogParams = cmsZoneShaderSettings.underwaterFogParams;
			underwaterCausticsParams_overrideMode = (EOverrideMode)cmsZoneShaderSettings.GetUnderwaterCausticsParamsOverrideMode();
			underwaterCausticsParams = cmsZoneShaderSettings.underwaterCausticsParams;
			underwaterCausticsTexture_overrideMode = (EOverrideMode)cmsZoneShaderSettings.GetUnderwaterCausticsTextureOverrideMode();
			underwaterCausticsTexture = cmsZoneShaderSettings.underwaterCausticsTexture;
			underwaterEffectsDistanceToSurfaceFade_overrideMode = (EOverrideMode)cmsZoneShaderSettings.GetUnderwaterEffectsDistanceToSurfaceFadeOverrideMode();
			underwaterEffectsDistanceToSurfaceFade = cmsZoneShaderSettings.underwaterEffectsDistanceToSurfaceFade;
			liquidResidueTex_overrideMode = (EOverrideMode)cmsZoneShaderSettings.GetLiquidResidueTextureOverrideMode();
			liquidResidueTex = cmsZoneShaderSettings.liquidResidueTex;
			mainWaterSurfacePlane_overrideMode = (EOverrideMode)cmsZoneShaderSettings.GetMainWaterSurfacePlaneOverrideMode();
			mainWaterSurfacePlane = cmsZoneShaderSettings.mainWaterSurfacePlane;
		}
		else
		{
			underwaterTintColor_overrideMode = EOverrideMode.ApplyNewValue;
			underwaterTintColor = new Color(0f, 0f, 0f, 0f);
			underwaterFogColor_overrideMode = EOverrideMode.ApplyNewValue;
			underwaterFogColor = new Color(0f, 0f, 0f, 0f);
			mainWaterSurfacePlane_overrideMode = EOverrideMode.ApplyNewValue;
			Transform transform = base.gameObject.transform.Find("DummyWaterPlane");
			GameObject gameObject = null;
			if (transform != null)
			{
				gameObject = transform.gameObject;
			}
			else
			{
				gameObject = new GameObject("DummyWaterPlane");
				gameObject.transform.SetParent(base.gameObject.transform);
				gameObject.transform.rotation = Quaternion.identity;
				gameObject.transform.position = new Vector3(0f, -9999f, 0f);
			}
			mainWaterSurfacePlane = gameObject.transform;
		}
		zoneWeatherMapDissolveProgress_overrideMode = EOverrideMode.LeaveUnchanged;
		if (rerunAwake)
		{
			Awake();
		}
	}

	public void CopySettings(ZoneShaderSettings zoneShaderSettings, bool rerunAwake = false)
	{
		_activateOnAwake = zoneShaderSettings._activateOnAwake;
		groundFogColor_overrideMode = zoneShaderSettings.groundFogColor_overrideMode;
		groundFogColor = zoneShaderSettings.groundFogColor;
		groundFogHeight_overrideMode = zoneShaderSettings.groundFogHeight_overrideMode;
		groundFogHeight = zoneShaderSettings.groundFogHeight;
		groundFogHeightFade_overrideMode = zoneShaderSettings.groundFogHeightFade_overrideMode;
		_groundFogHeightFadeSize = zoneShaderSettings._groundFogHeightFadeSize;
		groundFogDepthFade_overrideMode = zoneShaderSettings.groundFogDepthFade_overrideMode;
		_groundFogDepthFadeSize = zoneShaderSettings._groundFogDepthFadeSize;
		zoneLiquidType_overrideMode = zoneShaderSettings.zoneLiquidType_overrideMode;
		zoneLiquidType = zoneShaderSettings.zoneLiquidType;
		liquidShape_overrideMode = zoneShaderSettings.liquidShape_overrideMode;
		liquidShape = zoneShaderSettings.liquidShape;
		liquidShapeRadius_overrideMode = zoneShaderSettings.liquidShapeRadius_overrideMode;
		liquidShapeRadius = zoneShaderSettings.liquidShapeRadius;
		liquidBottomTransform_overrideMode = zoneShaderSettings.liquidBottomTransform_overrideMode;
		liquidBottomTransform = zoneShaderSettings.liquidBottomTransform;
		zoneLiquidUVScale_overrideMode = zoneShaderSettings.zoneLiquidUVScale_overrideMode;
		zoneLiquidUVScale = zoneShaderSettings.zoneLiquidUVScale;
		underwaterTintColor_overrideMode = zoneShaderSettings.underwaterTintColor_overrideMode;
		underwaterTintColor = zoneShaderSettings.underwaterTintColor;
		underwaterFogColor_overrideMode = zoneShaderSettings.underwaterFogColor_overrideMode;
		underwaterFogColor = zoneShaderSettings.underwaterFogColor;
		underwaterFogParams_overrideMode = zoneShaderSettings.underwaterFogParams_overrideMode;
		underwaterFogParams = zoneShaderSettings.underwaterFogParams;
		underwaterCausticsParams_overrideMode = zoneShaderSettings.underwaterCausticsParams_overrideMode;
		underwaterCausticsParams = zoneShaderSettings.underwaterCausticsParams;
		underwaterCausticsTexture_overrideMode = zoneShaderSettings.underwaterCausticsTexture_overrideMode;
		underwaterCausticsTexture = zoneShaderSettings.underwaterCausticsTexture;
		underwaterEffectsDistanceToSurfaceFade_overrideMode = zoneShaderSettings.underwaterEffectsDistanceToSurfaceFade_overrideMode;
		underwaterEffectsDistanceToSurfaceFade = zoneShaderSettings.underwaterEffectsDistanceToSurfaceFade;
		liquidResidueTex_overrideMode = zoneShaderSettings.liquidResidueTex_overrideMode;
		liquidResidueTex = zoneShaderSettings.liquidResidueTex;
		mainWaterSurfacePlane_overrideMode = zoneShaderSettings.mainWaterSurfacePlane_overrideMode;
		mainWaterSurfacePlane = zoneShaderSettings.mainWaterSurfacePlane;
		zoneWeatherMapDissolveProgress_overrideMode = zoneShaderSettings.zoneWeatherMapDissolveProgress_overrideMode;
		zoneWeatherMapDissolveProgress = zoneShaderSettings.zoneWeatherMapDissolveProgress;
		if (rerunAwake)
		{
			Awake();
		}
	}

	private static void BakeDefault<T>(ref EOverrideMode mode, ref T value, T defaultValue)
	{
		if (mode == EOverrideMode.ApplyDefaultValue)
		{
			mode = EOverrideMode.ApplyNewValue;
			value = defaultValue;
		}
	}

	public void ReplaceDefaultValues(ZoneShaderSettings defaultZoneShaderSettings, bool rerunAwake = false)
	{
		BakeDefault(ref groundFogColor_overrideMode, ref groundFogColor, defaultZoneShaderSettings.groundFogColor);
		BakeDefault(ref groundFogHeight_overrideMode, ref groundFogHeight, defaultZoneShaderSettings.groundFogHeight);
		BakeDefault(ref groundFogHeightFade_overrideMode, ref _groundFogHeightFadeSize, defaultZoneShaderSettings._groundFogHeightFadeSize);
		BakeDefault(ref groundFogDepthFade_overrideMode, ref _groundFogDepthFadeSize, defaultZoneShaderSettings._groundFogDepthFadeSize);
		BakeDefault(ref zoneLiquidType_overrideMode, ref zoneLiquidType, defaultZoneShaderSettings.zoneLiquidType);
		BakeDefault(ref liquidShape_overrideMode, ref liquidShape, defaultZoneShaderSettings.liquidShape);
		BakeDefault(ref liquidShapeRadius_overrideMode, ref liquidShapeRadius, defaultZoneShaderSettings.liquidShapeRadius);
		BakeDefault(ref liquidBottomTransform_overrideMode, ref liquidBottomTransform, defaultZoneShaderSettings.liquidBottomTransform);
		BakeDefault(ref zoneLiquidUVScale_overrideMode, ref zoneLiquidUVScale, defaultZoneShaderSettings.zoneLiquidUVScale);
		BakeDefault(ref underwaterTintColor_overrideMode, ref underwaterTintColor, defaultZoneShaderSettings.underwaterTintColor);
		BakeDefault(ref underwaterFogColor_overrideMode, ref underwaterFogColor, defaultZoneShaderSettings.underwaterFogColor);
		BakeDefault(ref underwaterFogParams_overrideMode, ref underwaterFogParams, defaultZoneShaderSettings.underwaterFogParams);
		BakeDefault(ref underwaterCausticsParams_overrideMode, ref underwaterCausticsParams, defaultZoneShaderSettings.underwaterCausticsParams);
		BakeDefault(ref underwaterCausticsTexture_overrideMode, ref underwaterCausticsTexture, defaultZoneShaderSettings.underwaterCausticsTexture);
		BakeDefault(ref underwaterEffectsDistanceToSurfaceFade_overrideMode, ref underwaterEffectsDistanceToSurfaceFade, defaultZoneShaderSettings.underwaterEffectsDistanceToSurfaceFade);
		BakeDefault(ref liquidResidueTex_overrideMode, ref liquidResidueTex, defaultZoneShaderSettings.liquidResidueTex);
		BakeDefault(ref mainWaterSurfacePlane_overrideMode, ref mainWaterSurfacePlane, defaultZoneShaderSettings.mainWaterSurfacePlane);
		if (rerunAwake)
		{
			Awake();
		}
	}

	public void ReplaceDefaultValues(CMSZoneShaderSettings.CMSZoneShaderProperties defaultZoneShaderProperties, bool rerunAwake = false)
	{
		CMSZoneShaderSettings.CMSZoneShaderProperties cMSZoneShaderProperties = defaultZoneShaderProperties;
		BakeDefault(ref groundFogColor_overrideMode, ref groundFogColor, cMSZoneShaderProperties.groundFogColor);
		BakeDefault(ref groundFogHeight_overrideMode, ref groundFogHeight, cMSZoneShaderProperties.groundFogHeight);
		BakeDefault(ref groundFogHeightFade_overrideMode, ref _groundFogHeightFadeSize, cMSZoneShaderProperties.groundFogHeightFadeSize);
		BakeDefault(ref groundFogDepthFade_overrideMode, ref _groundFogDepthFadeSize, cMSZoneShaderProperties.groundFogDepthFadeSize);
		BakeDefault(ref zoneLiquidType_overrideMode, ref zoneLiquidType, (EZoneLiquidType)cMSZoneShaderProperties.zoneLiquidType);
		BakeDefault(ref liquidShape_overrideMode, ref liquidShape, (ELiquidShape)cMSZoneShaderProperties.liquidShape);
		BakeDefault(ref liquidShapeRadius_overrideMode, ref liquidShapeRadius, cMSZoneShaderProperties.liquidShapeRadius);
		BakeDefault(ref liquidBottomTransform_overrideMode, ref liquidBottomTransform, cMSZoneShaderProperties.liquidBottomTransform);
		BakeDefault(ref zoneLiquidUVScale_overrideMode, ref zoneLiquidUVScale, cMSZoneShaderProperties.zoneLiquidUVScale);
		BakeDefault(ref underwaterTintColor_overrideMode, ref underwaterTintColor, cMSZoneShaderProperties.underwaterTintColor);
		BakeDefault(ref underwaterFogColor_overrideMode, ref underwaterFogColor, cMSZoneShaderProperties.underwaterFogColor);
		BakeDefault(ref underwaterFogParams_overrideMode, ref underwaterFogParams, cMSZoneShaderProperties.underwaterFogParams);
		BakeDefault(ref underwaterCausticsParams_overrideMode, ref underwaterCausticsParams, cMSZoneShaderProperties.underwaterCausticsParams);
		BakeDefault(ref underwaterCausticsTexture_overrideMode, ref underwaterCausticsTexture, cMSZoneShaderProperties.underwaterCausticsTexture);
		BakeDefault(ref underwaterEffectsDistanceToSurfaceFade_overrideMode, ref underwaterEffectsDistanceToSurfaceFade, cMSZoneShaderProperties.underwaterEffectsDistanceToSurfaceFade);
		BakeDefault(ref liquidResidueTex_overrideMode, ref liquidResidueTex, cMSZoneShaderProperties.liquidResidueTex);
		BakeDefault(ref mainWaterSurfacePlane_overrideMode, ref mainWaterSurfacePlane, cMSZoneShaderProperties.mainWaterSurfacePlane);
		if (rerunAwake)
		{
			Awake();
		}
	}
}
