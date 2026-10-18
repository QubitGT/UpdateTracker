using System;
using UnityEngine;

namespace GT_CustomMapSupportRuntime;

public class CMSZoneShaderSettings : MonoBehaviour
{
	public enum EOverrideMode
	{
		LeaveUnchanged,
		ApplyNewValue,
		ApplyDefaultValue
	}

	public enum ETextureOverrideType
	{
		Default,
		Custom
	}

	public struct CMSZoneShaderProperties
	{
		public bool isInitialized;

		public Color groundFogColor;

		public float groundFogDepthFadeSize;

		public Transform? groundFogHeightPlane;

		public float groundFogHeight;

		public float groundFogHeightFadeSize;

		public int zoneLiquidType;

		public int liquidShape;

		public float liquidShapeRadius;

		public Transform? liquidBottomTransform;

		public float zoneLiquidUVScale;

		public Color underwaterTintColor;

		public Color underwaterFogColor;

		public Vector4 underwaterFogParams;

		public Vector4 underwaterCausticsParams;

		public Texture2D? underwaterCausticsTexture;

		public Vector2 underwaterEffectsDistanceToSurfaceFade;

		public Texture2D? liquidResidueTex;

		public Transform? mainWaterSurfacePlane;

		public float zoneWeatherMapDissolveProgress;

		public float GroundFogHeightFade => 1f / Mathf.Max(1E-05f, groundFogHeightFadeSize);
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

	[NonSerialized]
	public Collider[]? edZoneColliders;

	[NonSerialized]
	public bool edWasInitialized;

	public static bool isInitialized;

	public static CMSZoneShaderSettings? defaultsInstance;

	public static bool hasDefaultsInstance;

	public static CMSZoneShaderSettings? activeInstance;

	public static bool hasActiveInstance;

	public bool isExported;

	[Tooltip("Set this to true for cases like it is the first CMSZoneShaderSettings that should be activated when a scene is loaded.")]
	public bool activateOnLoad;

	[Tooltip("These values will be used as the default global values that will be fallen back to when not in a zone and that the other scripts will reference.")]
	public bool isDefaultValues;

	public bool applyGroundFog;

	private static readonly int groundFogColor_shaderProp = Shader.PropertyToID("_ZoneGroundFogColor");

	public EOverrideMode groundFogColor_overrideMode;

	public Color groundFogColor = new Color(0.7f, 0.9f, 1f, 1f);

	private static readonly int groundFogDepthFadeSq_shaderProp = Shader.PropertyToID("_ZoneGroundFogDepthFadeSq");

	public EOverrideMode groundFogDepthFade_overrideMode;

	public float groundFogDepthFadeSize = 20f;

	private static readonly int groundFogHeight_shaderProp = Shader.PropertyToID("_ZoneGroundFogHeight");

	public EOverrideMode groundFogHeight_overrideMode;

	public Transform? groundFogHeightPlane;

	public float groundFogHeight = 7.45f;

	private static readonly int groundFogHeightFade_shaderProp = Shader.PropertyToID("_ZoneGroundFogHeightFade");

	public EOverrideMode groundFogHeightFade_overrideMode;

	public float groundFogHeightFadeSize = 20f;

	public bool applyLiquidEffects;

	public EOverrideMode zoneLiquidType_overrideMode;

	public EZoneLiquidType zoneLiquidType = EZoneLiquidType.Water;

	private static EZoneLiquidType liquidType_previousValue = EZoneLiquidType.None;

	public EOverrideMode liquidShape_overrideMode;

	public ELiquidShape liquidShape;

	private static ELiquidShape liquidShape_previousValue = ELiquidShape.Plane;

	public EOverrideMode liquidShapeRadius_overrideMode;

	public float liquidShapeRadius = 1f;

	private static float liquidShapeRadius_previousValue;

	private bool hasLiquidBottomTransform;

	public EOverrideMode liquidBottomTransform_overrideMode;

	public Transform? liquidBottomTransform;

	private float liquidBottomPosY_previousValue;

	private static readonly int shaderParam_GlobalZoneLiquidUVScale = Shader.PropertyToID("_GlobalZoneLiquidUVScale");

	public EOverrideMode zoneLiquidUVScale_overrideMode;

	public float zoneLiquidUVScale = 0.01f;

	private static readonly int shaderParam_GlobalWaterTintColor = Shader.PropertyToID("_GlobalWaterTintColor");

	public EOverrideMode underwaterTintColor_overrideMode;

	public Color underwaterTintColor = new Color(0.3f, 0.65f, 1f, 0.2f);

	private static readonly int shaderParam_GlobalUnderwaterFogColor = Shader.PropertyToID("_GlobalUnderwaterFogColor");

	public EOverrideMode underwaterFogColor_overrideMode;

	public Color underwaterFogColor = new Color(0.12f, 0.41f, 0.77f);

	private static readonly int shaderParam_GlobalUnderwaterFogParams = Shader.PropertyToID("_GlobalUnderwaterFogParams");

	public EOverrideMode underwaterFogParams_overrideMode;

	public float underwaterFogStart = -5f;

	public float underwaterFogDistance = 40f;

	[Tooltip("Fog params are: start, distance (end - start), unused, unused")]
	public Vector4 underwaterFogParams = new Vector4(-5f, 40f, 0f, 0f);

	private static readonly int shaderParam_GlobalUnderwaterCausticsParams = Shader.PropertyToID("_GlobalUnderwaterCausticsParams");

	public EOverrideMode underwaterCausticsParams_overrideMode;

	public float underwaterCausticsSpeed = 0.075f;

	public float underwaterCausticsScale = 0.075f;

	[Tooltip("Caustics params are: Speed, Scale, Alpha, unused")]
	public Vector4 underwaterCausticsParams = new Vector4(0.075f, 0.075f, 1f, 0f);

	public static readonly int shaderParam_GlobalUnderwaterCausticsTex = Shader.PropertyToID("_GlobalUnderwaterCausticsTex");

	public EOverrideMode underwaterCausticsTexture_overrideMode;

	public ETextureOverrideType underwaterCausticsTextureOverrideType;

	public Texture2D? underwaterCausticsTexture;

	private static readonly int shaderParam_GlobalUnderwaterEffectsDistanceToSurfaceFade = Shader.PropertyToID("_GlobalUnderwaterEffectsDistanceToSurfaceFade");

	public EOverrideMode underwaterEffectsDistanceToSurfaceFade_overrideMode;

	[Range(0.0001f, 50f)]
	public float underwaterFogDistanceToSurfaceFadeMinimum = 0.0001f;

	[Range(0.0001f, 50f)]
	public float underwaterFogDistanceToSurfaceFadeMaximum = 50f;

	public Vector2 underwaterEffectsDistanceToSurfaceFade = new Vector2(0.0001f, 50f);

	private const string kEdTooltip_liquidResidueTex = "This is used for things like the charred surface effect when lava burns static geo.";

	public static readonly int shaderParam_GlobalLiquidResidueTex = Shader.PropertyToID("_GlobalLiquidResidueTex");

	[Tooltip("This is used for things like the charred surface effect when lava burns static geo.")]
	public EOverrideMode liquidResidueTex_overrideMode;

	public ETextureOverrideType liquidResidueTextureOverrideType;

	[Tooltip("This is used for things like the charred surface effect when lava burns static geo.")]
	public Texture2D? liquidResidueTex;

	private readonly int shaderParam_GlobalMainWaterSurfacePlane = Shader.PropertyToID("_GlobalMainWaterSurfacePlane");

	public bool hasMainWaterSurfacePlane;

	public bool hasDynamicWaterSurfacePlane;

	public EOverrideMode mainWaterSurfacePlane_overrideMode;

	public Transform? mainWaterSurfacePlane;

	private static readonly int shaderParam_ZoneWeatherMapDissolveProgress = Shader.PropertyToID("_ZoneWeatherMapDissolveProgress");

	public EOverrideMode zoneWeatherMapDissolveProgress_overrideMode;

	[Range(0f, 1f)]
	public float zoneWeatherMapDissolveProgress = 1f;

	public bool isActiveInstance => activeInstance == this;

	public float GroundFogDepthFadeSq => 1f / Mathf.Max(1E-05f, groundFogDepthFadeSize * groundFogDepthFadeSize);

	public float GroundFogHeightFade => 1f / Mathf.Max(1E-05f, groundFogHeightFadeSize);

	private static int shaderParam_ZoneLiquidPosRadiusSq { get; set; } = Shader.PropertyToID("_ZoneLiquidPosRadiusSq");


	public int GetGroundFogColorOverrideMode()
	{
		if (isDefaultValues)
		{
			return 1;
		}
		return (int)groundFogColor_overrideMode;
	}

	public int GetGroundFogDepthFadeOverrideMode()
	{
		if (isDefaultValues)
		{
			return 1;
		}
		return (int)groundFogDepthFade_overrideMode;
	}

	public int GetGroundFogHeightOverrideMode()
	{
		if (isDefaultValues)
		{
			return 1;
		}
		return (int)groundFogHeight_overrideMode;
	}

	public int GetGroundFogHeightFadeOverrideMode()
	{
		if (isDefaultValues)
		{
			return 1;
		}
		return (int)groundFogHeightFade_overrideMode;
	}

	public void SetZoneLiquidTypeKeywordEnum(EZoneLiquidType liquidType)
	{
		if (!isExported)
		{
			if (liquidType == EZoneLiquidType.None)
			{
				Shader.EnableKeyword("_GLOBAL_ZONE_LIQUID_TYPE__NONE");
			}
			else
			{
				Shader.DisableKeyword("_GLOBAL_ZONE_LIQUID_TYPE__NONE");
			}
			if (liquidType == EZoneLiquidType.Water)
			{
				Shader.EnableKeyword("_GLOBAL_ZONE_LIQUID_TYPE__WATER");
			}
			else
			{
				Shader.DisableKeyword("_GLOBAL_ZONE_LIQUID_TYPE__WATER");
			}
			if (liquidType == EZoneLiquidType.Lava)
			{
				Shader.EnableKeyword("_GLOBAL_ZONE_LIQUID_TYPE__LAVA");
			}
			else
			{
				Shader.DisableKeyword("_GLOBAL_ZONE_LIQUID_TYPE__LAVA");
			}
		}
	}

	public int GetZoneLiquidTypeOverrideMode()
	{
		if (isDefaultValues)
		{
			return 1;
		}
		return (int)zoneLiquidType_overrideMode;
	}

	public int GetZoneLiquidType()
	{
		return (int)zoneLiquidType;
	}

	public void SetZoneLiquidShapeKeywordEnum(ELiquidShape shape)
	{
		if (!isExported)
		{
			if (shape == ELiquidShape.Plane)
			{
				Shader.EnableKeyword("_ZONE_LIQUID_SHAPE__PLANE");
			}
			else
			{
				Shader.DisableKeyword("_ZONE_LIQUID_SHAPE__PLANE");
			}
			if (shape == ELiquidShape.Cylinder)
			{
				Debug.Log("Enable CYLINDER liquid...");
				Shader.EnableKeyword("_ZONE_LIQUID_SHAPE__CYLINDER");
			}
			else
			{
				Shader.DisableKeyword("_ZONE_LIQUID_SHAPE__CYLINDER");
			}
		}
	}

	public int GetLiquidShapeOverrideMode()
	{
		if (isDefaultValues)
		{
			return 1;
		}
		return (int)liquidShape_overrideMode;
	}

	public int GetZoneLiquidShape()
	{
		return (int)liquidShape;
	}

	public int GetLiquidShapeRadiusOverrideMode()
	{
		if (isDefaultValues)
		{
			return 1;
		}
		return (int)liquidShapeRadius_overrideMode;
	}

	public int GetLiquidBottomTransformOverrideMode()
	{
		if (isDefaultValues)
		{
			return 1;
		}
		return (int)liquidBottomTransform_overrideMode;
	}

	public static float GetWaterY()
	{
		if (!(activeInstance == null) && !(activeInstance.mainWaterSurfacePlane == null))
		{
			return activeInstance.mainWaterSurfacePlane.position.y;
		}
		return -1f;
	}

	public int GetZoneLiquidUVScaleOverrideMode()
	{
		if (isDefaultValues)
		{
			return 1;
		}
		return (int)zoneLiquidUVScale_overrideMode;
	}

	public int GetUnderwaterTintColorOverrideMode()
	{
		if (isDefaultValues)
		{
			return 1;
		}
		return (int)underwaterTintColor_overrideMode;
	}

	public int GetUnderwaterFogColorOverrideMode()
	{
		if (isDefaultValues)
		{
			return 1;
		}
		return (int)underwaterFogColor_overrideMode;
	}

	public int GetUnderwaterFogParamsOverrideMode()
	{
		if (isDefaultValues)
		{
			return 1;
		}
		return (int)underwaterFogParams_overrideMode;
	}

	public int GetUnderwaterCausticsParamsOverrideMode()
	{
		if (isDefaultValues)
		{
			return 1;
		}
		return (int)underwaterCausticsParams_overrideMode;
	}

	public int GetUnderwaterCausticsTextureOverrideMode()
	{
		if (isDefaultValues)
		{
			return 1;
		}
		return (int)underwaterCausticsTexture_overrideMode;
	}

	public int GetUnderwaterEffectsDistanceToSurfaceFadeOverrideMode()
	{
		if (isDefaultValues)
		{
			return 1;
		}
		return (int)underwaterEffectsDistanceToSurfaceFade_overrideMode;
	}

	public int GetLiquidResidueTextureOverrideMode()
	{
		if (isDefaultValues)
		{
			return 1;
		}
		return (int)liquidResidueTex_overrideMode;
	}

	public int GetMainWaterSurfacePlaneOverrideMode()
	{
		if (isDefaultValues)
		{
			return 1;
		}
		return (int)mainWaterSurfacePlane_overrideMode;
	}

	public void Initialize()
	{
		if (!isExported)
		{
			if (mainWaterSurfacePlane == null)
			{
				hasMainWaterSurfacePlane = false;
				hasDynamicWaterSurfacePlane = false;
			}
			else
			{
				hasMainWaterSurfacePlane = mainWaterSurfacePlane_overrideMode == EOverrideMode.ApplyNewValue || isDefaultValues;
				hasDynamicWaterSurfacePlane = hasMainWaterSurfacePlane && !mainWaterSurfacePlane.gameObject.isStatic;
			}
			hasLiquidBottomTransform = liquidBottomTransform != null && (liquidBottomTransform_overrideMode == EOverrideMode.ApplyNewValue || isDefaultValues);
			CheckDefaultsInstance();
			if (activateOnLoad)
			{
				BecomeActiveInstance();
			}
		}
	}

	protected void OnDestroy()
	{
		if (!isExported)
		{
			if (defaultsInstance == this)
			{
				hasDefaultsInstance = false;
			}
			if (activeInstance == this)
			{
				hasActiveInstance = false;
			}
		}
	}

	private void UpdateMainPlaneShaderProperty()
	{
		if (isExported)
		{
			return;
		}
		Transform transform = null;
		if (hasMainWaterSurfacePlane && (mainWaterSurfacePlane_overrideMode == EOverrideMode.ApplyNewValue || isDefaultValues))
		{
			transform = mainWaterSurfacePlane;
		}
		else if (mainWaterSurfacePlane_overrideMode == EOverrideMode.ApplyDefaultValue && defaultsInstance != null && defaultsInstance.hasMainWaterSurfacePlane)
		{
			transform = defaultsInstance.mainWaterSurfacePlane;
		}
		if (!(transform == null))
		{
			Vector3 position = transform.position;
			Vector3 up = transform.up;
			float w = 0f - Vector3.Dot(up, position);
			Shader.SetGlobalVector(shaderParam_GlobalMainWaterSurfacePlane, new Vector4(up.x, up.y, up.z, w));
			ELiquidShape eLiquidShape = (liquidShape_previousValue = ((liquidShape_overrideMode == EOverrideMode.ApplyNewValue || isDefaultValues) ? liquidShape : ((liquidShape_overrideMode != EOverrideMode.ApplyDefaultValue || !(defaultsInstance != null)) ? liquidShape_previousValue : defaultsInstance.liquidShape)));
			float y = (liquidBottomPosY_previousValue = (((liquidBottomTransform_overrideMode == EOverrideMode.ApplyNewValue || isDefaultValues) && liquidBottomTransform != null) ? liquidBottomTransform.position.y : ((liquidBottomTransform_overrideMode != EOverrideMode.ApplyDefaultValue || !(defaultsInstance != null) || !(defaultsInstance.liquidBottomTransform != null)) ? liquidBottomPosY_previousValue : defaultsInstance.liquidBottomTransform.position.y)));
			float num = ((liquidShapeRadius_overrideMode == EOverrideMode.ApplyNewValue || isDefaultValues) ? liquidShapeRadius : ((liquidShape_overrideMode != EOverrideMode.ApplyDefaultValue || !(defaultsInstance != null)) ? liquidShapeRadius_previousValue : defaultsInstance.liquidShapeRadius));
			if (eLiquidShape == ELiquidShape.Cylinder)
			{
				Debug.Log("Setting Cylinder Liquid Radius...");
				Shader.SetGlobalVector(shaderParam_ZoneLiquidPosRadiusSq, new Vector4(position.x, y, position.z, num * num));
				liquidShapeRadius_previousValue = num;
			}
		}
	}

	private void CheckDefaultsInstance()
	{
		if (isExported || !isDefaultValues)
		{
			return;
		}
		if (hasDefaultsInstance && defaultsInstance != null && defaultsInstance != this)
		{
			if (!Application.isPlaying)
			{
				Debug.LogWarning("CMSZoneShaderSettings: (Edit time warning) Deactivating instance with `isDefaultValues` set to true. CMSZoneShaderSettings Object: " + base.name, this);
				base.gameObject.SetActive(value: false);
				return;
			}
			string hierarchyPath = defaultsInstance.transform.GetHierarchyPath();
			Debug.LogError("CMSZoneShaderSettings: Destroying conflicting defaults instance.\n- keeping: \"" + hierarchyPath + "\"\n- destroying (this): \"" + base.transform.GetHierarchyPath() + "\"", this);
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			defaultsInstance = this;
			hasDefaultsInstance = true;
			BecomeActiveInstance();
		}
	}

	public void BecomeActiveInstance(bool force = false)
	{
		if (!isExported && (!(activeInstance == this) || force))
		{
			ApplyValues();
			activeInstance = this;
			hasActiveInstance = true;
		}
	}

	public static void ActivateDefaultSettings()
	{
		if (defaultsInstance != null)
		{
			defaultsInstance.BecomeActiveInstance();
		}
	}

	public void SetGroundFogValue(Color fogColor, float fogDepthFade, float fogHeight, float fogHeightFade)
	{
		if (!isExported)
		{
			groundFogColor_overrideMode = EOverrideMode.ApplyNewValue;
			groundFogColor = fogColor;
			groundFogDepthFade_overrideMode = EOverrideMode.ApplyNewValue;
			groundFogDepthFadeSize = fogDepthFade;
			groundFogHeight_overrideMode = EOverrideMode.ApplyNewValue;
			groundFogHeight = fogHeight;
			groundFogHeightFade_overrideMode = EOverrideMode.ApplyNewValue;
			groundFogHeightFadeSize = fogHeightFade;
			BecomeActiveInstance(force: true);
		}
	}

	private void ApplyValues()
	{
		if (isExported || defaultsInstance == null)
		{
			return;
		}
		if (!applyGroundFog)
		{
			ApplyColor(groundFogColor_shaderProp, groundFogColor_overrideMode, new Color(0f, 0f, 0f, 0f), defaultsInstance.groundFogColor);
		}
		else
		{
			ApplyColor(groundFogColor_shaderProp, groundFogColor_overrideMode, groundFogColor, defaultsInstance.groundFogColor);
			ApplyFloat(groundFogDepthFadeSq_shaderProp, groundFogDepthFade_overrideMode, GroundFogDepthFadeSq, defaultsInstance.GroundFogDepthFadeSq);
			if (groundFogHeightPlane != null)
			{
				groundFogHeight = groundFogHeightPlane.position.y;
			}
			ApplyFloat(groundFogHeight_shaderProp, groundFogHeight_overrideMode, groundFogHeight, defaultsInstance.groundFogHeight);
			ApplyFloat(groundFogHeightFade_shaderProp, groundFogHeightFade_overrideMode, GroundFogHeightFade, defaultsInstance.GroundFogHeightFade);
		}
		if (!applyLiquidEffects)
		{
			SetZoneLiquidTypeKeywordEnum(EZoneLiquidType.None);
			SetZoneLiquidShapeKeywordEnum(ELiquidShape.Plane);
			ApplyColor(shaderParam_GlobalWaterTintColor, underwaterTintColor_overrideMode, new Color(0f, 0f, 0f, 0f), defaultsInstance.underwaterTintColor);
			ApplyColor(shaderParam_GlobalUnderwaterFogColor, underwaterFogColor_overrideMode, new Color(0f, 0f, 0f, 0f), defaultsInstance.underwaterFogColor);
			ApplyTexture(shaderParam_GlobalLiquidResidueTex, liquidResidueTex_overrideMode, null, defaultsInstance.liquidResidueTex);
			Shader.SetGlobalVector(shaderParam_GlobalMainWaterSurfacePlane, new Vector4(0f, 1f, 0f, 10000f));
		}
		else
		{
			if (zoneLiquidType_overrideMode != 0 || isDefaultValues)
			{
				EZoneLiquidType eZoneLiquidType = ((zoneLiquidType_overrideMode == EOverrideMode.ApplyNewValue) ? zoneLiquidType : defaultsInstance.zoneLiquidType);
				if (eZoneLiquidType != liquidType_previousValue || !isInitialized)
				{
					SetZoneLiquidTypeKeywordEnum(eZoneLiquidType);
					liquidType_previousValue = eZoneLiquidType;
				}
			}
			Debug.Log("Applying Liquid Shape...");
			if (liquidShape_overrideMode != 0 || isDefaultValues)
			{
				Debug.Log("Override Mode != LeaveUnchanged");
				ELiquidShape eLiquidShape = ((liquidShape_overrideMode == EOverrideMode.ApplyNewValue) ? liquidShape : defaultsInstance.liquidShape);
				if (eLiquidShape != liquidShape_previousValue || !isInitialized)
				{
					Debug.Log("Set Liquid Shape...");
					SetZoneLiquidShapeKeywordEnum(eLiquidShape);
					liquidShape_previousValue = eLiquidShape;
				}
				else
				{
					Debug.Log("Same liquid shape AND already initialized");
				}
			}
			ApplyFloat(shaderParam_GlobalZoneLiquidUVScale, zoneLiquidUVScale_overrideMode, zoneLiquidUVScale, defaultsInstance.zoneLiquidUVScale);
			ApplyColor(shaderParam_GlobalWaterTintColor, underwaterTintColor_overrideMode, underwaterTintColor, defaultsInstance.underwaterTintColor);
			ApplyColor(shaderParam_GlobalUnderwaterFogColor, underwaterFogColor_overrideMode, underwaterFogColor, defaultsInstance.underwaterFogColor);
			ApplyVector(shaderParam_GlobalUnderwaterFogParams, underwaterFogParams_overrideMode, underwaterFogParams, defaultsInstance.underwaterFogParams);
			ApplyVector(shaderParam_GlobalUnderwaterCausticsParams, underwaterCausticsParams_overrideMode, underwaterCausticsParams, defaultsInstance.underwaterCausticsParams);
			ApplyTexture(shaderParam_GlobalUnderwaterCausticsTex, underwaterCausticsTexture_overrideMode, underwaterCausticsTexture, defaultsInstance.underwaterCausticsTexture);
			ApplyVector(shaderParam_GlobalUnderwaterEffectsDistanceToSurfaceFade, underwaterEffectsDistanceToSurfaceFade_overrideMode, underwaterEffectsDistanceToSurfaceFade, defaultsInstance.underwaterEffectsDistanceToSurfaceFade);
			ApplyTexture(shaderParam_GlobalLiquidResidueTex, liquidResidueTex_overrideMode, liquidResidueTex, defaultsInstance.liquidResidueTex);
			ApplyFloat(shaderParam_ZoneWeatherMapDissolveProgress, zoneWeatherMapDissolveProgress_overrideMode, zoneWeatherMapDissolveProgress, defaultsInstance.zoneWeatherMapDissolveProgress);
			UpdateMainPlaneShaderProperty();
		}
		isInitialized = true;
	}

	public void RefreshValues()
	{
		if (!isExported)
		{
			if (mainWaterSurfacePlane == null)
			{
				hasMainWaterSurfacePlane = false;
				hasDynamicWaterSurfacePlane = false;
			}
			else
			{
				hasMainWaterSurfacePlane = mainWaterSurfacePlane_overrideMode == EOverrideMode.ApplyNewValue || isDefaultValues;
				hasDynamicWaterSurfacePlane = hasMainWaterSurfacePlane && !mainWaterSurfacePlane.gameObject.isStatic;
			}
			hasLiquidBottomTransform = liquidBottomTransform != null && (liquidBottomTransform_overrideMode == EOverrideMode.ApplyNewValue || isDefaultValues);
		}
	}

	private void ApplyColor(int shaderProp, EOverrideMode overrideMode, Color value, Color defaultValue)
	{
		if (!isExported)
		{
			if (overrideMode == EOverrideMode.ApplyNewValue || isDefaultValues)
			{
				Shader.SetGlobalColor(shaderProp, value.linear);
			}
			else if (overrideMode == EOverrideMode.ApplyDefaultValue)
			{
				Shader.SetGlobalColor(shaderProp, defaultValue.linear);
			}
		}
	}

	private void ApplyFloat(int shaderProp, EOverrideMode overrideMode, float value, float defaultValue)
	{
		if (!isExported)
		{
			if (overrideMode == EOverrideMode.ApplyNewValue || isDefaultValues)
			{
				Shader.SetGlobalFloat(shaderProp, value);
			}
			else if (overrideMode == EOverrideMode.ApplyDefaultValue)
			{
				Shader.SetGlobalFloat(shaderProp, defaultValue);
			}
		}
	}

	private void ApplyVector(int shaderProp, EOverrideMode overrideMode, Vector2 value, Vector2 defaultValue)
	{
		if (!isExported)
		{
			if (overrideMode == EOverrideMode.ApplyNewValue || isDefaultValues)
			{
				Shader.SetGlobalVector(shaderProp, value);
			}
			else if (overrideMode == EOverrideMode.ApplyDefaultValue)
			{
				Shader.SetGlobalVector(shaderProp, defaultValue);
			}
		}
	}

	private void ApplyVector(int shaderProp, EOverrideMode overrideMode, Vector3 value, Vector3 defaultValue)
	{
		if (!isExported)
		{
			if (overrideMode == EOverrideMode.ApplyNewValue || isDefaultValues)
			{
				Shader.SetGlobalVector(shaderProp, value);
			}
			else if (overrideMode == EOverrideMode.ApplyDefaultValue)
			{
				Shader.SetGlobalVector(shaderProp, defaultValue);
			}
		}
	}

	private void ApplyVector(int shaderProp, EOverrideMode overrideMode, Vector4 value, Vector4 defaultValue)
	{
		if (!isExported)
		{
			if (overrideMode == EOverrideMode.ApplyNewValue || isDefaultValues)
			{
				Shader.SetGlobalVector(shaderProp, value);
			}
			else if (overrideMode == EOverrideMode.ApplyDefaultValue)
			{
				Shader.SetGlobalVector(shaderProp, defaultValue);
			}
		}
	}

	public void ApplyTexture(int shaderProp, EOverrideMode overrideMode, Texture2D? value, Texture2D? defaultValue)
	{
		if (!isExported)
		{
			if (overrideMode == EOverrideMode.ApplyNewValue || isDefaultValues)
			{
				Shader.SetGlobalTexture(shaderProp, value);
			}
			else if (overrideMode == EOverrideMode.ApplyDefaultValue)
			{
				Shader.SetGlobalTexture(shaderProp, defaultValue);
			}
		}
	}

	public CMSZoneShaderProperties GetProperties()
	{
		CMSZoneShaderProperties result = default(CMSZoneShaderProperties);
		result.groundFogColor = groundFogColor;
		result.groundFogDepthFadeSize = groundFogDepthFadeSize;
		result.groundFogHeightPlane = groundFogHeightPlane;
		result.groundFogHeight = groundFogHeight;
		result.groundFogHeightFadeSize = groundFogDepthFadeSize;
		result.zoneLiquidType = GetZoneLiquidType();
		result.liquidShape = GetZoneLiquidShape();
		result.liquidShapeRadius = liquidShapeRadius;
		result.liquidBottomTransform = liquidBottomTransform;
		result.zoneLiquidUVScale = zoneLiquidUVScale;
		result.underwaterTintColor = underwaterTintColor;
		result.underwaterFogColor = underwaterFogColor;
		result.underwaterFogParams = underwaterFogParams;
		result.underwaterCausticsParams = underwaterCausticsParams;
		result.underwaterCausticsTexture = underwaterCausticsTexture;
		result.underwaterEffectsDistanceToSurfaceFade = underwaterEffectsDistanceToSurfaceFade;
		result.liquidResidueTex = liquidResidueTex;
		result.mainWaterSurfacePlane = mainWaterSurfacePlane;
		result.zoneWeatherMapDissolveProgress = zoneWeatherMapDissolveProgress;
		result.isInitialized = true;
		return result;
	}
}
