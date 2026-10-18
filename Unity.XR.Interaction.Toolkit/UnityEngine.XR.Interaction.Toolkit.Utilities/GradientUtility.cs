using System.Collections.Generic;

namespace UnityEngine.XR.Interaction.Toolkit.Utilities;

public class GradientUtility
{
	private const int k_MaxGradientColorKeys = 8;

	private const int k_Precision = 100;

	private static readonly List<float> s_ColorKeyTimes;

	private static readonly HashSet<int> s_TruncatedColorKeyTimes;

	private static readonly List<float> s_AlphaKeyTimes;

	private static readonly HashSet<int> s_TruncatedAlphaKeyTimes;

	private static readonly GradientColorKey[][] s_ColorKeyArrays;

	private static readonly GradientAlphaKey[][] s_AlphaKeyArrays;

	static GradientUtility()
	{
		s_ColorKeyTimes = new List<float>();
		s_TruncatedColorKeyTimes = new HashSet<int>();
		s_AlphaKeyTimes = new List<float>();
		s_TruncatedAlphaKeyTimes = new HashSet<int>();
		s_ColorKeyArrays = new GradientColorKey[7][];
		s_AlphaKeyArrays = new GradientAlphaKey[7][];
		for (int i = 2; i <= 8; i++)
		{
			s_ColorKeyArrays[i - 2] = new GradientColorKey[i];
			s_AlphaKeyArrays[i - 2] = new GradientAlphaKey[i];
		}
	}

	public static Gradient Lerp(Gradient a, Gradient b, float t, bool lerpColors = true, bool lerpAlphas = true)
	{
		Gradient gradient = new Gradient();
		Lerp(a, b, gradient, t, lerpColors, lerpAlphas);
		return gradient;
	}

	public static void Lerp(Gradient a, Gradient b, Gradient output, float t, bool lerpColors = true, bool lerpAlphas = true)
	{
		s_ColorKeyTimes.Clear();
		s_TruncatedColorKeyTimes.Clear();
		s_AlphaKeyTimes.Clear();
		s_TruncatedAlphaKeyTimes.Clear();
		if (lerpColors)
		{
			AddUniqueColorKeys(a.colorKeys);
			AddUniqueColorKeys(b.colorKeys);
		}
		if (lerpAlphas)
		{
			AddUniqueAlphaKeys(a.alphaKeys);
			AddUniqueAlphaKeys(b.alphaKeys);
		}
		ReduceKeysIfNeeded(s_ColorKeyTimes, 8);
		ReduceKeysIfNeeded(s_AlphaKeyTimes, 8);
		GradientColorKey[] colorKeys = (lerpColors ? PrepareColorKeys(s_ColorKeyTimes, a, b, t) : b.colorKeys);
		GradientAlphaKey[] alphaKeys = (lerpAlphas ? PrepareAlphaKeys(s_AlphaKeyTimes, a, b, t) : b.alphaKeys);
		output.SetKeys(colorKeys, alphaKeys);
	}

	public static void CopyGradient(Gradient source, Gradient destination)
	{
		destination.SetKeys(source.colorKeys, source.alphaKeys);
	}

	private static void AddUniqueColorKeys(GradientColorKey[] keys)
	{
		for (int i = 0; i < keys.Length; i++)
		{
			AddColorKeyIfUnique(keys[i].time);
		}
	}

	private static void AddUniqueAlphaKeys(GradientAlphaKey[] keys)
	{
		for (int i = 0; i < keys.Length; i++)
		{
			AddAlphaKeyIfUnique(keys[i].time);
		}
	}

	private static void AddColorKeyIfUnique(float keyTime)
	{
		int item = TruncatePrecision(keyTime);
		if (s_TruncatedColorKeyTimes.Add(item))
		{
			s_ColorKeyTimes.Add(keyTime);
		}
	}

	private static void AddAlphaKeyIfUnique(float keyTime)
	{
		int item = TruncatePrecision(keyTime);
		if (s_TruncatedAlphaKeyTimes.Add(item))
		{
			s_AlphaKeyTimes.Add(keyTime);
		}
	}

	private static GradientColorKey[] PrepareColorKeys(List<float> keyTimes, Gradient a, Gradient b, float t)
	{
		int count = keyTimes.Count;
		GradientColorKey[] colorKeyArray = GetColorKeyArray(count);
		for (int i = 0; i < count; i++)
		{
			float time = keyTimes[i];
			Color col = Color.Lerp(a.Evaluate(time), b.Evaluate(time), t);
			colorKeyArray[i] = new GradientColorKey(col, time);
		}
		return colorKeyArray;
	}

	private static GradientAlphaKey[] PrepareAlphaKeys(List<float> keyTimes, Gradient a, Gradient b, float t)
	{
		int count = keyTimes.Count;
		GradientAlphaKey[] alphaKeyArray = GetAlphaKeyArray(count);
		for (int i = 0; i < count; i++)
		{
			float time = keyTimes[i];
			float alpha = Mathf.Lerp(a.Evaluate(time).a, b.Evaluate(time).a, t);
			alphaKeyArray[i] = new GradientAlphaKey(alpha, time);
		}
		return alphaKeyArray;
	}

	private static int TruncatePrecision(float value)
	{
		return (int)(value * 100f);
	}

	private static void ReduceKeysIfNeeded(List<float> keyTimes, int maxKeys)
	{
		while (keyTimes.Count > maxKeys)
		{
			int index = keyTimes.Count / 2;
			keyTimes.RemoveAt(index);
		}
	}

	private static GradientColorKey[] GetColorKeyArray(int size)
	{
		if (size < 2 || size > 8)
		{
			return new GradientColorKey[size];
		}
		return s_ColorKeyArrays[size - 2];
	}

	private static GradientAlphaKey[] GetAlphaKeyArray(int size)
	{
		if (size < 2 || size > 8)
		{
			return new GradientAlphaKey[size];
		}
		return s_AlphaKeyArrays[size - 2];
	}
}
