using System;
using UnityEngine.Serialization;

namespace UnityEngine.Rendering;

[Serializable]
public sealed class LensFlareDataElementSRP
{
	public LensFlareDataSRP lensFlareDataSRP;

	public bool visible;

	public float position;

	public Vector2 positionOffset;

	public float angularOffset;

	public Vector2 translationScale;

	[Range(0f, 1f)]
	public float ringThickness;

	[Range(-1f, 1f)]
	public float hoopFactor;

	public float noiseAmplitude;

	public int noiseFrequency;

	public float noiseSpeed;

	public float shapeCutOffSpeed;

	public float shapeCutOffRadius;

	[Min(0f)]
	[SerializeField]
	[FormerlySerializedAs("localIntensity")]
	private float m_LocalIntensity;

	public Texture lensFlareTexture;

	public float uniformScale;

	public Vector2 sizeXY;

	public bool allowMultipleElement;

	[Min(1f)]
	[SerializeField]
	[FormerlySerializedAs("count")]
	private int m_Count;

	public bool preserveAspectRatio;

	public float rotation;

	public SRPLensFlareColorType tintColorType;

	public Color tint;

	public TextureGradient tintGradient;

	public SRPLensFlareBlendMode blendMode;

	public bool autoRotate;

	public SRPLensFlareType flareType;

	public bool modulateByLightColor;

	[SerializeField]
	private bool isFoldOpened;

	public SRPLensFlareDistribution distribution;

	public float lengthSpread;

	public AnimationCurve positionCurve;

	public AnimationCurve scaleCurve;

	public int seed;

	public Gradient colorGradient;

	[Range(0f, 1f)]
	[SerializeField]
	[FormerlySerializedAs("intensityVariation")]
	private float m_IntensityVariation;

	public Vector2 positionVariation;

	public float scaleVariation;

	public float rotationVariation;

	public bool enableRadialDistortion;

	public Vector2 targetSizeDistortion;

	public AnimationCurve distortionCurve;

	public bool distortionRelativeToCenter;

	[Range(0f, 1f)]
	[SerializeField]
	[FormerlySerializedAs("fallOff")]
	private float m_FallOff;

	[Range(0f, 1f)]
	[SerializeField]
	[FormerlySerializedAs("edgeOffset")]
	private float m_EdgeOffset;

	[Min(3f)]
	[SerializeField]
	[FormerlySerializedAs("sideCount")]
	private int m_SideCount;

	[Range(0f, 1f)]
	[SerializeField]
	[FormerlySerializedAs("sdfRoundness")]
	private float m_SdfRoundness;

	public bool inverseSDF;

	public float uniformAngle;

	public AnimationCurve uniformAngleCurve;

	public float localIntensity
	{
		get
		{
			return m_LocalIntensity;
		}
		set
		{
			m_LocalIntensity = Mathf.Max(0f, value);
		}
	}

	public int count
	{
		get
		{
			return m_Count;
		}
		set
		{
			m_Count = Mathf.Max(1, value);
		}
	}

	public float intensityVariation
	{
		get
		{
			return m_IntensityVariation;
		}
		set
		{
			m_IntensityVariation = Mathf.Max(0f, value);
		}
	}

	public float fallOff
	{
		get
		{
			return m_FallOff;
		}
		set
		{
			m_FallOff = Mathf.Clamp01(value);
		}
	}

	public float edgeOffset
	{
		get
		{
			return m_EdgeOffset;
		}
		set
		{
			m_EdgeOffset = Mathf.Clamp01(value);
		}
	}

	public int sideCount
	{
		get
		{
			return m_SideCount;
		}
		set
		{
			m_SideCount = Mathf.Max(3, value);
		}
	}

	public float sdfRoundness
	{
		get
		{
			return m_SdfRoundness;
		}
		set
		{
			m_SdfRoundness = Mathf.Clamp01(value);
		}
	}

	public LensFlareDataElementSRP()
	{
		lensFlareDataSRP = null;
		visible = true;
		localIntensity = 1f;
		position = 0f;
		positionOffset = new Vector2(0f, 0f);
		angularOffset = 0f;
		translationScale = new Vector2(1f, 1f);
		lensFlareTexture = null;
		uniformScale = 1f;
		sizeXY = Vector2.one;
		allowMultipleElement = false;
		count = 5;
		rotation = 0f;
		preserveAspectRatio = false;
		ringThickness = 0.25f;
		hoopFactor = 1f;
		noiseAmplitude = 1f;
		noiseFrequency = 1;
		noiseSpeed = 0f;
		shapeCutOffSpeed = 0f;
		shapeCutOffRadius = 10f;
		tintColorType = SRPLensFlareColorType.Constant;
		tint = new Color(1f, 1f, 1f, 0.5f);
		tintGradient = new TextureGradient(new GradientColorKey[2]
		{
			new GradientColorKey(Color.black, 0f),
			new GradientColorKey(Color.white, 1f)
		}, new GradientAlphaKey[2]
		{
			new GradientAlphaKey(0f, 0f),
			new GradientAlphaKey(1f, 1f)
		});
		blendMode = SRPLensFlareBlendMode.Additive;
		autoRotate = false;
		isFoldOpened = true;
		flareType = SRPLensFlareType.Circle;
		distribution = SRPLensFlareDistribution.Uniform;
		lengthSpread = 1f;
		colorGradient = new Gradient();
		colorGradient.SetKeys(new GradientColorKey[2]
		{
			new GradientColorKey(Color.white, 0f),
			new GradientColorKey(Color.white, 1f)
		}, new GradientAlphaKey[2]
		{
			new GradientAlphaKey(1f, 0f),
			new GradientAlphaKey(1f, 1f)
		});
		positionCurve = new AnimationCurve(new Keyframe(0f, 0f, 1f, 1f), new Keyframe(1f, 1f, 1f, -1f));
		scaleCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 1f));
		uniformAngle = 0f;
		uniformAngleCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 0f));
		seed = 0;
		intensityVariation = 0.75f;
		positionVariation = new Vector2(1f, 0f);
		scaleVariation = 1f;
		rotationVariation = 180f;
		enableRadialDistortion = false;
		targetSizeDistortion = Vector2.one;
		distortionCurve = new AnimationCurve(new Keyframe(0f, 0f, 1f, 1f), new Keyframe(1f, 1f, 1f, -1f));
		distortionRelativeToCenter = false;
		fallOff = 1f;
		edgeOffset = 0.1f;
		sdfRoundness = 0f;
		sideCount = 6;
		inverseSDF = false;
	}

	public LensFlareDataElementSRP Clone()
	{
		LensFlareDataElementSRP lensFlareDataElementSRP = new LensFlareDataElementSRP();
		lensFlareDataElementSRP.lensFlareDataSRP = lensFlareDataSRP;
		lensFlareDataElementSRP.visible = visible;
		lensFlareDataElementSRP.localIntensity = localIntensity;
		lensFlareDataElementSRP.position = position;
		lensFlareDataElementSRP.positionOffset = positionOffset;
		lensFlareDataElementSRP.angularOffset = angularOffset;
		lensFlareDataElementSRP.translationScale = translationScale;
		lensFlareDataElementSRP.lensFlareTexture = lensFlareTexture;
		lensFlareDataElementSRP.uniformScale = uniformScale;
		lensFlareDataElementSRP.sizeXY = sizeXY;
		lensFlareDataElementSRP.allowMultipleElement = allowMultipleElement;
		lensFlareDataElementSRP.count = count;
		lensFlareDataElementSRP.rotation = rotation;
		lensFlareDataElementSRP.preserveAspectRatio = preserveAspectRatio;
		lensFlareDataElementSRP.ringThickness = ringThickness;
		lensFlareDataElementSRP.hoopFactor = hoopFactor;
		lensFlareDataElementSRP.noiseAmplitude = noiseAmplitude;
		lensFlareDataElementSRP.noiseFrequency = noiseFrequency;
		lensFlareDataElementSRP.noiseSpeed = noiseSpeed;
		lensFlareDataElementSRP.shapeCutOffSpeed = shapeCutOffSpeed;
		lensFlareDataElementSRP.shapeCutOffRadius = shapeCutOffRadius;
		lensFlareDataElementSRP.tintColorType = tintColorType;
		lensFlareDataElementSRP.tint = tint;
		lensFlareDataElementSRP.tintGradient = new TextureGradient(tintGradient.colorKeys, tintGradient.alphaKeys, tintGradient.mode, tintGradient.colorSpace, tintGradient.textureSize);
		lensFlareDataElementSRP.tintGradient = new TextureGradient(tintGradient.colorKeys, tintGradient.alphaKeys);
		lensFlareDataElementSRP.blendMode = blendMode;
		lensFlareDataElementSRP.autoRotate = autoRotate;
		lensFlareDataElementSRP.isFoldOpened = isFoldOpened;
		lensFlareDataElementSRP.flareType = flareType;
		lensFlareDataElementSRP.distribution = distribution;
		lensFlareDataElementSRP.lengthSpread = lengthSpread;
		lensFlareDataElementSRP.colorGradient = new Gradient();
		lensFlareDataElementSRP.colorGradient.SetKeys(colorGradient.colorKeys, colorGradient.alphaKeys);
		lensFlareDataElementSRP.colorGradient.mode = colorGradient.mode;
		lensFlareDataElementSRP.colorGradient.colorSpace = colorGradient.colorSpace;
		lensFlareDataElementSRP.positionCurve = new AnimationCurve(positionCurve.keys);
		lensFlareDataElementSRP.scaleCurve = new AnimationCurve(scaleCurve.keys);
		lensFlareDataElementSRP.uniformAngle = uniformAngle;
		lensFlareDataElementSRP.uniformAngleCurve = new AnimationCurve(uniformAngleCurve.keys);
		lensFlareDataElementSRP.seed = seed;
		lensFlareDataElementSRP.intensityVariation = intensityVariation;
		lensFlareDataElementSRP.positionVariation = positionVariation;
		lensFlareDataElementSRP.scaleVariation = scaleVariation;
		lensFlareDataElementSRP.rotationVariation = rotationVariation;
		lensFlareDataElementSRP.enableRadialDistortion = enableRadialDistortion;
		lensFlareDataElementSRP.targetSizeDistortion = targetSizeDistortion;
		lensFlareDataElementSRP.distortionCurve = new AnimationCurve(distortionCurve.keys);
		lensFlareDataElementSRP.distortionRelativeToCenter = distortionRelativeToCenter;
		lensFlareDataElementSRP.fallOff = fallOff;
		lensFlareDataElementSRP.edgeOffset = edgeOffset;
		lensFlareDataElementSRP.sdfRoundness = sdfRoundness;
		lensFlareDataElementSRP.sideCount = sideCount;
		lensFlareDataElementSRP.inverseSDF = inverseSDF;
		return lensFlareDataElementSRP;
	}
}
