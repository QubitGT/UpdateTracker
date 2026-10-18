using System;

namespace UnityEngine.Rendering;

public static class LightUnitUtils
{
	public const float SphereSolidAngle = MathF.PI * 4f;

	private static float k_LuminanceToEvFactor => Mathf.Log(100f / ColorUtils.s_LightMeterCalibrationConstant, 2f);

	private static float k_EvToLuminanceFactor => 0f - k_LuminanceToEvFactor;

	public static LightUnit GetNativeLightUnit(LightType lightType)
	{
		switch (lightType)
		{
		case LightType.Spot:
		case LightType.Point:
		case LightType.Pyramid:
			return LightUnit.Candela;
		case LightType.Directional:
		case LightType.Box:
			return LightUnit.Lux;
		case LightType.Area:
		case LightType.Disc:
		case LightType.Tube:
			return LightUnit.Nits;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public static bool IsLightUnitSupported(LightType lightType, LightUnit lightUnit)
	{
		int num = 1 << (int)lightUnit;
		switch (lightType)
		{
		case LightType.Spot:
		case LightType.Point:
		case LightType.Pyramid:
			return (num & 0x17) > 0;
		case LightType.Directional:
		case LightType.Box:
			return (num & 4) > 0;
		case LightType.Area:
		case LightType.Disc:
		case LightType.Tube:
			return (num & 0x19) > 0;
		default:
			return false;
		}
	}

	public static float GetSolidAngleFromPointLight()
	{
		return MathF.PI * 4f;
	}

	public static float GetSolidAngleFromSpotLight(float spotAngle)
	{
		double num = Math.PI * (double)spotAngle / 180.0;
		return (float)(Math.PI * 2.0 * (1.0 - Math.Cos(num * 0.5)));
	}

	public static float GetSolidAngleFromPyramidLight(float spotAngle, float aspectRatio)
	{
		if (aspectRatio < 1f)
		{
			aspectRatio = (float)(1.0 / (double)aspectRatio);
		}
		double num = Math.PI * (double)spotAngle / 180.0;
		double num2 = Math.Atan(Math.Tan(0.5 * num) * (double)aspectRatio) * 2.0;
		return (float)(4.0 * Math.Asin(Math.Sin(num * 0.5) * Math.Sin(num2 * 0.5)));
	}

	internal static float GetSolidAngle(LightType lightType, bool spotReflector, float spotAngle, float aspectRatio)
	{
		return lightType switch
		{
			LightType.Spot => spotReflector ? GetSolidAngleFromSpotLight(spotAngle) : (MathF.PI * 4f), 
			LightType.Pyramid => spotReflector ? GetSolidAngleFromPyramidLight(spotAngle, aspectRatio) : (MathF.PI * 4f), 
			LightType.Point => GetSolidAngleFromPointLight(), 
			_ => throw new ArgumentException("Solid angle is undefined for lights of type " + lightType), 
		};
	}

	public static float GetAreaFromRectangleLight(float rectSizeX, float rectSizeY)
	{
		return Mathf.Abs(rectSizeX * rectSizeY) * MathF.PI;
	}

	public static float GetAreaFromRectangleLight(Vector2 rectSize)
	{
		return GetAreaFromRectangleLight(rectSize.x, rectSize.y);
	}

	public static float GetAreaFromDiscLight(float discRadius)
	{
		return discRadius * discRadius * MathF.PI * MathF.PI;
	}

	public static float GetAreaFromTubeLight(float tubeLength)
	{
		return Mathf.Abs(tubeLength) * 4f * MathF.PI;
	}

	public static float LumenToCandela(float lumen, float solidAngle)
	{
		return lumen / solidAngle;
	}

	public static float CandelaToLumen(float candela, float solidAngle)
	{
		return candela * solidAngle;
	}

	public static float LumenToNits(float lumen, float area)
	{
		return lumen / area;
	}

	public static float NitsToLumen(float nits, float area)
	{
		return nits * area;
	}

	public static float LuxToCandela(float lux, float distance)
	{
		return lux * (distance * distance);
	}

	public static float CandelaToLux(float candela, float distance)
	{
		return candela / (distance * distance);
	}

	public static float Ev100ToNits(float ev100)
	{
		return Mathf.Pow(2f, ev100 + k_EvToLuminanceFactor);
	}

	public static float NitsToEv100(float nits)
	{
		return Mathf.Log(nits, 2f) + k_LuminanceToEvFactor;
	}

	public static float Ev100ToCandela(float ev100)
	{
		return Ev100ToNits(ev100);
	}

	public static float CandelaToEv100(float candela)
	{
		return NitsToEv100(candela);
	}

	internal static float ConvertIntensityInternal(float intensity, LightUnit fromUnit, LightUnit toUnit, LightType lightType, float area, float luxAtDistance, float solidAngle)
	{
		if (!IsLightUnitSupported(lightType, fromUnit) || !IsLightUnitSupported(lightType, toUnit))
		{
			throw new ArgumentException("Converting " + fromUnit.ToString() + " to " + toUnit.ToString() + " is undefined for lights of type " + lightType);
		}
		if (fromUnit == toUnit)
		{
			return intensity;
		}
		switch (fromUnit)
		{
		case LightUnit.Lumen:
			switch (toUnit)
			{
			case LightUnit.Candela:
				return LumenToCandela(intensity, solidAngle);
			case LightUnit.Lux:
				return CandelaToLux(LumenToCandela(intensity, solidAngle), luxAtDistance);
			case LightUnit.Nits:
				return LumenToNits(intensity, area);
			case LightUnit.Ev100:
			{
				float nits;
				switch (lightType)
				{
				case LightType.Spot:
				case LightType.Point:
				case LightType.Pyramid:
					nits = LumenToCandela(intensity, solidAngle);
					break;
				case LightType.Area:
				case LightType.Disc:
				case LightType.Tube:
					nits = LumenToNits(intensity, area);
					break;
				default:
					throw new ArgumentException("Converting from Lumen to Ev100 is undefined for light type " + lightType);
				}
				return NitsToEv100(nits);
			}
			default:
				throw new ArgumentOutOfRangeException("toUnit", toUnit, null);
			}
		case LightUnit.Candela:
			return toUnit switch
			{
				LightUnit.Lumen => CandelaToLumen(intensity, solidAngle), 
				LightUnit.Lux => CandelaToLux(intensity, luxAtDistance), 
				LightUnit.Ev100 => NitsToEv100(intensity), 
				_ => throw new ArgumentOutOfRangeException("toUnit", toUnit, null), 
			};
		case LightUnit.Lux:
			return toUnit switch
			{
				LightUnit.Lumen => CandelaToLumen(LuxToCandela(intensity, luxAtDistance), solidAngle), 
				LightUnit.Candela => LuxToCandela(intensity, luxAtDistance), 
				LightUnit.Ev100 => NitsToEv100(LuxToCandela(intensity, luxAtDistance)), 
				_ => throw new ArgumentOutOfRangeException("toUnit", toUnit, null), 
			};
		case LightUnit.Nits:
			return toUnit switch
			{
				LightUnit.Lumen => NitsToLumen(intensity, area), 
				LightUnit.Ev100 => NitsToEv100(intensity), 
				_ => throw new ArgumentOutOfRangeException("toUnit", toUnit, null), 
			};
		case LightUnit.Ev100:
			switch (toUnit)
			{
			case LightUnit.Lumen:
			{
				float num = Ev100ToNits(intensity);
				switch (lightType)
				{
				case LightType.Spot:
				case LightType.Point:
				case LightType.Pyramid:
					return CandelaToLumen(num, solidAngle);
				case LightType.Area:
				case LightType.Disc:
				case LightType.Tube:
					return NitsToLumen(num, area);
				default:
					throw new ArgumentException("Converting from Lumen to Ev100 is undefined for light type " + lightType);
				}
			}
			case LightUnit.Candela:
			case LightUnit.Nits:
				return Ev100ToNits(intensity);
			case LightUnit.Lux:
				return CandelaToLux(Ev100ToNits(intensity), luxAtDistance);
			default:
				throw new ArgumentOutOfRangeException("toUnit", toUnit, null);
			}
		default:
			throw new ArgumentOutOfRangeException("fromUnit", fromUnit, null);
		}
	}

	public static float ConvertIntensity(Light light, float intensity, LightUnit fromUnit, LightUnit toUnit)
	{
		LightType type = light.type;
		float area = type switch
		{
			LightType.Area => GetAreaFromRectangleLight(light.areaSize), 
			LightType.Disc => GetAreaFromDiscLight(light.areaSize.x), 
			LightType.Tube => GetAreaFromTubeLight(light.areaSize.x), 
			_ => 0f, 
		};
		float luxAtDistance = light.luxAtDistance;
		float num = ((type != 0 && type != LightType.Point && type != LightType.Pyramid) ? 0f : GetSolidAngle(type, light.enableSpotReflector, light.spotAngle, light.areaSize.x));
		float solidAngle = num;
		return ConvertIntensityInternal(intensity, fromUnit, toUnit, type, area, luxAtDistance, solidAngle);
	}
}
