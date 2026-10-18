using System;
using UnityEngine;

namespace Oculus.Interaction.Input;

public class OneEuroFilter : IOneEuroFilter<float>
{
	private class LowPassFilter
	{
		private bool _isFirstUpdate;

		private float _hatx;

		private float _hatxprev;

		public float PrevValue => _hatxprev;

		public LowPassFilter()
		{
			_isFirstUpdate = true;
		}

		public void Reset()
		{
			_isFirstUpdate = true;
			_hatx = (_hatxprev = 0f);
		}

		public float Filter(float x, float alpha)
		{
			if (_isFirstUpdate)
			{
				_isFirstUpdate = false;
				_hatxprev = x;
			}
			_hatx = alpha * x + (1f - alpha) * _hatxprev;
			_hatxprev = _hatx;
			return _hatx;
		}
	}

	private class OneEuroFilterMulti<TData> : IOneEuroFilter<TData>
	{
		private readonly Func<float[], TData> _arrayToType;

		private readonly Func<TData, int, float> _getValAtIndex;

		private readonly IOneEuroFilter<float>[] _filters;

		private readonly float[] _componentValues;

		public TData Value { get; private set; }

		public OneEuroFilterMulti(int numComponents, Func<float[], TData> arrayToType, Func<TData, int, float> getValAtIndex)
		{
			IOneEuroFilter<float>[] filters = new OneEuroFilter[numComponents];
			_filters = filters;
			_componentValues = new float[numComponents];
			_arrayToType = arrayToType;
			_getValAtIndex = getValAtIndex;
			for (int i = 0; i < _filters.Length; i++)
			{
				_filters[i] = new OneEuroFilter();
			}
		}

		public void SetProperties(in OneEuroFilterPropertyBlock properties)
		{
			IOneEuroFilter<float>[] filters = _filters;
			for (int i = 0; i < filters.Length; i++)
			{
				filters[i].SetProperties(in properties);
			}
		}

		public TData Step(TData newValue, float deltaTime)
		{
			for (int i = 0; i < _filters.Length; i++)
			{
				float rawValue = _getValAtIndex(newValue, i);
				_componentValues[i] = _filters[i].Step(rawValue, deltaTime);
			}
			Value = _arrayToType(_componentValues);
			return Value;
		}

		public void Reset()
		{
			IOneEuroFilter<float>[] filters = _filters;
			for (int i = 0; i < filters.Length; i++)
			{
				filters[i].Reset();
			}
		}

		void IOneEuroFilter<TData>.SetProperties(in OneEuroFilterPropertyBlock properties)
		{
			SetProperties(in properties);
		}
	}

	public const float _DEFAULT_FREQUENCY_HZ = 60f;

	private OneEuroFilterPropertyBlock _properties;

	private bool _isFirstUpdate;

	private LowPassFilter _xfilt;

	private LowPassFilter _dxfilt;

	public float Value { get; private set; }

	private OneEuroFilter()
	{
		_xfilt = new LowPassFilter();
		_dxfilt = new LowPassFilter();
		_isFirstUpdate = true;
		OneEuroFilterPropertyBlock properties = OneEuroFilterPropertyBlock.Default;
		SetProperties(in properties);
	}

	public void SetProperties(in OneEuroFilterPropertyBlock properties)
	{
		_properties = properties;
	}

	public float Step(float newValue, float deltaTime)
	{
		if (deltaTime > 0f)
		{
			float num = 1f / deltaTime;
			float x = (_isFirstUpdate ? 0f : ((newValue - _xfilt.PrevValue) * num));
			_isFirstUpdate = false;
			float f = _dxfilt.Filter(x, GetAlpha(num, _properties.DCutoff));
			float cutoff = _properties.MinCutoff + _properties.Beta * Mathf.Abs(f);
			Value = _xfilt.Filter(newValue, GetAlpha(num, cutoff));
		}
		return Value;
	}

	public void Reset()
	{
		Value = 0f;
		_xfilt.Reset();
		_dxfilt.Reset();
		_isFirstUpdate = true;
	}

	private float GetAlpha(float rate, float cutoff)
	{
		float num = 1f / (MathF.PI * 2f * cutoff);
		float num2 = 1f / rate;
		return 1f / (1f + num / num2);
	}

	public static IOneEuroFilter<float> CreateFloat()
	{
		return new OneEuroFilter();
	}

	public static IOneEuroFilter<Vector2> CreateVector2()
	{
		return new OneEuroFilterMulti<Vector2>(2, (float[] values) => new Vector2(values[0], values[1]), (Vector2 value, int index) => value[index]);
	}

	public static IOneEuroFilter<Vector3> CreateVector3()
	{
		return new OneEuroFilterMulti<Vector3>(3, (float[] values) => new Vector3(values[0], values[1], values[2]), (Vector3 value, int index) => value[index]);
	}

	public static IOneEuroFilter<Vector4> CreateVector4()
	{
		return new OneEuroFilterMulti<Vector4>(4, (float[] values) => new Vector4(values[0], values[1], values[2], values[3]), (Vector4 value, int index) => value[index]);
	}

	public static IOneEuroFilter<Quaternion> CreateQuaternion()
	{
		return new OneEuroFilterMulti<Quaternion>(4, (float[] values) => new Quaternion(values[0], values[1], values[2], values[3]).normalized, (Quaternion value, int index) => value[index]);
	}

	public static IOneEuroFilter<Pose> CreatePose()
	{
		return new OneEuroFilterMulti<Pose>(7, (float[] values) => new Pose(new Vector3(values[0], values[1], values[2]), new Quaternion(values[3], values[4], values[5], values[6]).normalized), (Pose value, int index) => (index <= 2) ? value.position[index] : value.rotation[index - 3]);
	}

	void IOneEuroFilter<float>.SetProperties(in OneEuroFilterPropertyBlock properties)
	{
		SetProperties(in properties);
	}
}
