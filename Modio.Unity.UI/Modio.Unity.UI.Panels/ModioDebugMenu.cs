using System;
using System.Reflection;
using System.Text.RegularExpressions;
using Modio.Unity.UI.Components.Selectables;
using Modio.Unity.UI.Navigation;
using TMPro;
using UnityEngine;

namespace Modio.Unity.UI.Panels;

public class ModioDebugMenu : MonoBehaviour
{
	[SerializeField]
	private ModioUIButton _buttonPrefab;

	[SerializeField]
	private ModioUIToggle _togglePrefab;

	[SerializeField]
	private ModioInputFieldSelectionWrapper _textPrefab;

	[SerializeField]
	private TMP_Text _labelPrefab;

	private Action _onSetToDefaults;

	public void Awake()
	{
		_buttonPrefab.gameObject.SetActive(value: false);
		_togglePrefab.gameObject.SetActive(value: false);
		_textPrefab.gameObject.SetActive(value: false);
		if (_labelPrefab != null)
		{
			_labelPrefab.gameObject.SetActive(value: false);
		}
	}

	public void SetToDefaults()
	{
		_onSetToDefaults?.Invoke();
	}

	public void AddButton(string text, Action onClick)
	{
		ModioUIButton modioUIButton = UnityEngine.Object.Instantiate(_buttonPrefab, _buttonPrefab.transform.parent, worldPositionStays: false);
		modioUIButton.gameObject.SetActive(value: true);
		modioUIButton.GetComponentInChildren<TMP_Text>().text = text;
		modioUIButton.onClick.AddListener(delegate
		{
			onClick();
		});
	}

	public void AddToggle(string text, Func<bool> initialValueGetter, Action<bool> onToggle)
	{
		ModioUIToggle toggle = UnityEngine.Object.Instantiate(_togglePrefab, _buttonPrefab.transform.parent, worldPositionStays: false);
		toggle.gameObject.SetActive(value: true);
		toggle.GetComponentInChildren<TMP_Text>().text = text;
		_onSetToDefaults = (Action)Delegate.Combine(_onSetToDefaults, (Action)delegate
		{
			toggle.isOn = initialValueGetter();
		});
		toggle.onValueChanged.AddListener(delegate(bool b)
		{
			onToggle(b);
		});
	}

	public void AddLabel(string text)
	{
		TMP_Text tMP_Text = UnityEngine.Object.Instantiate(_labelPrefab, _labelPrefab.transform.parent, worldPositionStays: false);
		tMP_Text.gameObject.SetActive(value: true);
		tMP_Text.text = text;
	}

	public void AddTextField(string text, Func<string> initialValueGetter, Action<string> onSubmitted)
	{
		ModioInputFieldSelectionWrapper modioInputFieldSelectionWrapper = UnityEngine.Object.Instantiate(_textPrefab, _buttonPrefab.transform.parent, worldPositionStays: false);
		modioInputFieldSelectionWrapper.gameObject.SetActive(value: true);
		modioInputFieldSelectionWrapper.GetComponentInChildren<TMP_Text>().text = text;
		TMP_InputField inputField = modioInputFieldSelectionWrapper.GetComponentInChildren<TMP_InputField>();
		_onSetToDefaults = (Action)Delegate.Combine(_onSetToDefaults, (Action)delegate
		{
			inputField.text = initialValueGetter();
		});
		inputField.onDeselect.AddListener(OnTextFieldSubmit);
		inputField.onSubmit.AddListener(OnTextFieldSubmit);
		void OnTextFieldSubmit(string s)
		{
			try
			{
				onSubmitted(s);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
				inputField.text = initialValueGetter();
			}
		}
	}

	public void AddTextField(string text, Func<int> initialValueGetter, Action<int> onSubmitted)
	{
		AddTextField(text, () => initialValueGetter().ToString(), delegate(string s)
		{
			onSubmitted(int.Parse(s));
		});
	}

	public void AddTextField(string text, Func<long> initialValueGetter, Action<long> onSubmitted)
	{
		AddTextField(text, () => initialValueGetter().ToString(), delegate(string s)
		{
			onSubmitted(int.Parse(s));
		});
	}

	public void AddAllMethodsOrPropertiesWithAttribute<T>(Func<T, bool> predicate = null) where T : Attribute
	{
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		for (int i = 0; i < assemblies.Length; i++)
		{
			Type[] types = assemblies[i].GetTypes();
			foreach (Type type in types)
			{
				MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				PropertyInfo[] properties = type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				MethodInfo[] array = methods;
				foreach (MethodInfo methodInfo in array)
				{
					T customAttribute = methodInfo.GetCustomAttribute<T>();
					if (customAttribute == null || (predicate != null && !predicate(customAttribute)))
					{
						continue;
					}
					if (methodInfo.GetParameters().Length > 0)
					{
						Debug.LogError("Can't handle method " + methodInfo.Name + " on type " + type.Name + " because it has more than one parameter");
					}
					else
					{
						AddButton(Nicify(type.Name + ": " + methodInfo.Name), delegate
						{
							methodInfo.Invoke(null, null);
						});
					}
				}
				PropertyInfo[] array2 = properties;
				foreach (PropertyInfo propertyInfo in array2)
				{
					T customAttribute2 = propertyInfo.GetCustomAttribute<T>();
					if (customAttribute2 == null || (predicate != null && !predicate(customAttribute2)))
					{
						continue;
					}
					string propertyName = Nicify(type.Name + ": " + propertyInfo.Name);
					if (propertyInfo.PropertyType == typeof(bool))
					{
						AddToggle(propertyName, () => (bool)propertyInfo.GetValue(null), delegate(bool b)
						{
							propertyInfo.SetValue(null, b);
						});
					}
					if (propertyInfo.PropertyType == typeof(string))
					{
						HookUpField((object o) => (string)o, (string s) => s);
					}
					if (propertyInfo.PropertyType == typeof(int))
					{
						HookUpField((object o) => o.ToString(), (string s) => int.Parse(s));
					}
					if (propertyInfo.PropertyType == typeof(long))
					{
						HookUpField((object o) => o.ToString(), (string s) => long.Parse(s));
					}
					else
					{
						Debug.LogWarning(string.Format("{0} hit property of unhandled type {1}", "ModioDebugMenu", propertyInfo.PropertyType));
					}
					void HookUpField(Func<object, string> func1, Func<string, object> func2)
					{
						AddTextField(propertyName, () => func1(propertyInfo.GetValue(null)), delegate(string s)
						{
							propertyInfo.SetValue(null, func2(s));
						});
					}
				}
			}
		}
	}

	public static string Nicify(string name)
	{
		return Regex.Replace(name, "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])", " $1");
	}
}
