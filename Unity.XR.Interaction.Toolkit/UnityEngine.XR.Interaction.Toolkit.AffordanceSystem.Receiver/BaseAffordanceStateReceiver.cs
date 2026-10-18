using System;
using System.Text;
using Unity.XR.CoreUtils;
using Unity.XR.CoreUtils.Bindings.Variables;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver;

[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public abstract class BaseAffordanceStateReceiver<T> : MonoBehaviour, IAffordanceStateReceiver<T>, IAffordanceStateReceiver where T : struct, IEquatable<T>
{
	[SerializeField]
	[Tooltip("Affordance state provider component to subscribe to.")]
	private BaseAffordanceStateProvider m_AffordanceStateProvider;

	[SerializeField]
	[Tooltip("If true, the initial captured state value for the receiver will replace the idle value in the affordance theme.")]
	private bool m_ReplaceIdleStateValueWithInitialValue;

	private BaseAffordanceTheme<T> m_AffordanceTheme;

	private readonly BindableVariable<AffordanceStateData> m_AffordanceStateData = new BindableVariable<AffordanceStateData>();

	private bool m_Initialized;

	public BaseAffordanceStateProvider affordanceStateProvider
	{
		get
		{
			return m_AffordanceStateProvider;
		}
		set
		{
			m_AffordanceStateProvider = value;
		}
	}

	public bool replaceIdleStateValueWithInitialValue
	{
		get
		{
			return m_ReplaceIdleStateValueWithInitialValue;
		}
		set
		{
			m_ReplaceIdleStateValueWithInitialValue = value;
		}
	}

	public BaseAffordanceTheme<T> affordanceTheme
	{
		get
		{
			return m_AffordanceTheme;
		}
		set
		{
			m_AffordanceTheme = value;
			OnAffordanceThemeChanged(value);
		}
	}

	protected abstract BaseAffordanceTheme<T> defaultAffordanceTheme { get; }

	protected abstract BindableVariable<T> affordanceValue { get; }

	public IReadOnlyBindableVariable<T> currentAffordanceValue => affordanceValue;

	public IReadOnlyBindableVariable<AffordanceStateData> currentAffordanceStateData => m_AffordanceStateData;

	protected T initialValue { get; set; }

	protected bool initialValueCaptured { get; set; }

	protected virtual void Awake()
	{
		if (m_AffordanceStateProvider == null)
		{
			m_AffordanceStateProvider = GetComponentInParent<BaseAffordanceStateProvider>();
		}
	}

	protected virtual void OnEnable()
	{
		Initialize();
	}

	protected virtual void OnDisable()
	{
		if (m_AffordanceStateProvider != null)
		{
			m_AffordanceStateProvider.UnregisterAffordanceReceiver(this);
		}
	}

	protected virtual void Start()
	{
		Initialize();
		if (m_AffordanceStateProvider == null)
		{
			XRLoggingUtils.LogError($"Missing Affordance State Provider reference. Please set one on {this}.", this);
		}
	}

	private void Initialize()
	{
		if (!m_Initialized)
		{
			if (m_AffordanceStateProvider == null)
			{
				return;
			}
			if (affordanceTheme == null)
			{
				if (defaultAffordanceTheme == null)
				{
					return;
				}
				defaultAffordanceTheme.ValidateTheme();
				BaseAffordanceTheme<T> baseAffordanceTheme = GenerateNewAffordanceThemeInstance();
				baseAffordanceTheme.CopyFrom(defaultAffordanceTheme);
				affordanceTheme = baseAffordanceTheme;
			}
			m_Initialized = true;
		}
		m_AffordanceStateProvider.RegisterAffordanceReceiver(this);
	}

	protected abstract BaseAffordanceTheme<T> GenerateNewAffordanceThemeInstance();

	protected virtual void OnAffordanceThemeChanged(BaseAffordanceTheme<T> newValue)
	{
		LogIfMissingAffordanceStates(newValue);
	}

	private void LogIfMissingAffordanceStates(BaseAffordanceTheme<T> theme)
	{
		if (theme.GetAffordanceThemeDataForIndex((byte)(AffordanceStateShortcuts.stateCount - 1)) != null)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		for (byte b = 0; b < AffordanceStateShortcuts.stateCount; b++)
		{
			AffordanceThemeData<T> affordanceThemeDataForIndex = theme.GetAffordanceThemeDataForIndex(b);
			stringBuilder.Append($"Expected: {b} \"{AffordanceStateShortcuts.GetNameForIndex(b)}\",\tActual: ");
			stringBuilder.AppendLine((affordanceThemeDataForIndex != null) ? $"{b} \"{affordanceThemeDataForIndex.stateName}\"" : "<b>(None)</b>");
			if (affordanceThemeDataForIndex != null)
			{
				num++;
			}
		}
		Debug.LogWarning("Affordance Theme on affordance receiver is missing a potential affordance state. Expected states:" + $"\nExpected Count: {AffordanceStateShortcuts.stateCount}, Actual Count: {num}" + $"\n{stringBuilder}", this);
	}

	public virtual void OnAffordanceStateUpdated(AffordanceStateData previousState, AffordanceStateData newState)
	{
		m_AffordanceStateData.Value = newState;
	}

	protected virtual void ConsumeAffordance(T newValue)
	{
		affordanceValue.Value = newValue;
		OnAffordanceValueUpdated(newValue);
	}

	protected abstract void OnAffordanceValueUpdated(T newValue);

	protected virtual void CaptureInitialValue()
	{
		if (!initialValueCaptured)
		{
			initialValue = GetCurrentValueForCapture();
			affordanceValue.Value = initialValue;
			initialValueCaptured = true;
		}
	}

	protected virtual T GetCurrentValueForCapture()
	{
		return affordanceValue.Value;
	}

	protected virtual T ProcessTargetAffordanceValue(T newValue)
	{
		return newValue;
	}
}
