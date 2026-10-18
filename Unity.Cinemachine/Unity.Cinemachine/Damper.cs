using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.Cinemachine;

public static class Damper
{
	internal static class AverageFrameRateTracker
	{
		private const int kBufferSize = 100;

		private static float[] s_Buffer = new float[100];

		private static int s_NumItems = 0;

		private static int s_Head = 0;

		private static float s_Sum = 0f;

		public const float kSubframeTime = 0.0009765625f;

		public static float FPS { get; private set; }

		public static float DampTimeScale { get; private set; }

		[RuntimeInitializeOnLoadMethod]
		private static void Initialize()
		{
			Reset();
		}

		private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			Reset();
		}

		internal static void Reset()
		{
			s_NumItems = 0;
			s_Head = 0;
			s_Sum = 0f;
			FPS = 60f;
			SetDampTimeScale(FPS);
		}

		private static void Append()
		{
			float unscaledDeltaTime = Time.unscaledDeltaTime;
			if (++s_Head == 100)
			{
				s_Head = 0;
			}
			if (s_NumItems == 100)
			{
				s_Sum -= s_Buffer[s_Head];
			}
			else
			{
				s_NumItems++;
			}
			s_Sum += unscaledDeltaTime;
			s_Buffer[s_Head] = unscaledDeltaTime;
			FPS = (float)s_NumItems / s_Sum;
			SetDampTimeScale(FPS);
		}

		internal static void SetDampTimeScale(float fps)
		{
			DampTimeScale = 2f - 0.00181f * fps + 7.9E-07f * fps * fps;
		}
	}

	private const float Epsilon = 0.0001f;

	public const float kNegligibleResidual = 0.01f;

	private const float kLogNegligibleResidual = -4.6051702f;

	private static float DecayConstant(float time, float residual)
	{
		return Mathf.Log(1f / residual) / time;
	}

	private static float DecayedRemainder(float initial, float decayConstant, float deltaTime)
	{
		return initial / Mathf.Exp(decayConstant * deltaTime);
	}

	public static float Damp(float initial, float dampTime, float deltaTime)
	{
		return StandardDamp(initial, dampTime, deltaTime);
	}

	public static Vector3 Damp(Vector3 initial, Vector3 dampTime, float deltaTime)
	{
		for (int i = 0; i < 3; i++)
		{
			initial[i] = Damp(initial[i], dampTime[i], deltaTime);
		}
		return initial;
	}

	public static Vector3 Damp(Vector3 initial, float dampTime, float deltaTime)
	{
		for (int i = 0; i < 3; i++)
		{
			initial[i] = Damp(initial[i], dampTime, deltaTime);
		}
		return initial;
	}

	internal static float StandardDamp(float initial, float dampTime, float deltaTime)
	{
		if (dampTime < 0.0001f || Mathf.Abs(initial) < 0.0001f)
		{
			return initial;
		}
		if (deltaTime < 0.0001f)
		{
			return 0f;
		}
		return initial * (1f - Mathf.Exp(-4.6051702f * deltaTime / dampTime));
	}

	internal static float StableDamp(float initial, float dampTime, float deltaTime)
	{
		if (dampTime < 0.0001f || Mathf.Abs(initial) < 0.0001f)
		{
			return initial;
		}
		if (deltaTime < 0.0001f)
		{
			return 0f;
		}
		float num = Mathf.Min(deltaTime, 0.0009765625f);
		int num2 = Mathf.FloorToInt(deltaTime / num);
		float num3 = initial * num / deltaTime;
		float num4 = Mathf.Exp(-4.6051702f * AverageFrameRateTracker.DampTimeScale * num / dampTime);
		float num5 = num3;
		if (Mathf.Abs(num4 - 1f) < 0.0001f)
		{
			num5 *= num4 * (float)num2;
		}
		else
		{
			num5 *= num4 - Mathf.Pow(num4, num2 + 1);
			num5 /= 1f - num4;
		}
		num5 = Mathf.Lerp(num5, (num5 + num3) * num4, (deltaTime - num * (float)num2) / num);
		return initial - num5;
	}
}
