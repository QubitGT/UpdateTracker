using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Backtrace.Unity.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Backtrace.Unity.Model.JsonData;

public class Annotations
{
	internal static Dictionary<string, string> _environmentVariablesCache;

	internal static bool VariablesLoaded;

	private readonly int _gameObjectDepth;

	public static Dictionary<string, string> EnvironmentVariablesCache
	{
		get
		{
			if (!VariablesLoaded)
			{
				_environmentVariablesCache = SetEnvironmentVariables();
				VariablesLoaded = true;
			}
			return _environmentVariablesCache;
		}
		set
		{
			_environmentVariablesCache = value;
		}
	}

	public Dictionary<string, string> EnvironmentVariables
	{
		get
		{
			return EnvironmentVariablesCache;
		}
		set
		{
			EnvironmentVariablesCache = value;
		}
	}

	public Exception Exception { get; set; }

	public Annotations(Exception exception, int gameObjectDepth)
	{
		_gameObjectDepth = gameObjectDepth;
		Exception = exception;
	}

	private static Dictionary<string, string> SetEnvironmentVariables()
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		IDictionary environmentVariables = Environment.GetEnvironmentVariables();
		if (environmentVariables == null)
		{
			return dictionary;
		}
		foreach (DictionaryEntry item in environmentVariables)
		{
			string text = item.Key as string;
			if (!string.IsNullOrEmpty(text))
			{
				string text2 = item.Value as string;
				dictionary.Add(text, string.IsNullOrEmpty(text2) ? "NULL" : text2);
			}
		}
		return dictionary;
	}

	public BacktraceJObject ToJson()
	{
		BacktraceJObject backtraceJObject = new BacktraceJObject();
		backtraceJObject.Add("Environment Variables", new BacktraceJObject(EnvironmentVariables));
		if (Exception != null)
		{
			backtraceJObject.Add("Exception properties", new BacktraceJObject(new Dictionary<string, string>
			{
				{ "message", Exception.Message },
				{ "stackTrace", Exception.StackTrace },
				{
					"type",
					Exception.GetType().FullName
				},
				{ "source", Exception.Source }
			}));
		}
		if (_gameObjectDepth > -1)
		{
			Scene activeScene = SceneManager.GetActiveScene();
			List<BacktraceJObject> list = new List<BacktraceJObject>();
			List<GameObject> list2 = new List<GameObject>();
			activeScene.GetRootGameObjects(list2);
			foreach (GameObject item in list2)
			{
				list.Add(ConvertGameObject(item));
			}
			backtraceJObject.Add("Game objects", list);
		}
		return backtraceJObject;
	}

	private BacktraceJObject ConvertGameObject(GameObject gameObject, int depth = 0)
	{
		if (gameObject == null)
		{
			return new BacktraceJObject();
		}
		BacktraceJObject jObject = GetJObject(gameObject);
		List<BacktraceJObject> list = new List<BacktraceJObject>();
		foreach (object item in gameObject.transform)
		{
			Component component = item as Component;
			if (!(component == null))
			{
				list.Add(ConvertGameObject(component, gameObject.name, depth + 1));
			}
		}
		jObject.Add("children", list);
		return jObject;
	}

	private BacktraceJObject ConvertGameObject(Component gameObject, string parentName, int depth)
	{
		if (_gameObjectDepth > 0 && depth > _gameObjectDepth)
		{
			return new BacktraceJObject();
		}
		BacktraceJObject jObject = GetJObject(gameObject, parentName);
		if (_gameObjectDepth > 0 && depth + 1 >= _gameObjectDepth)
		{
			return jObject;
		}
		List<BacktraceJObject> list = new List<BacktraceJObject>();
		foreach (object item in gameObject.transform)
		{
			Component component = item as Component;
			if (!(component == null))
			{
				list.Add(ConvertGameObject(component, gameObject.name, depth + 1));
			}
		}
		jObject.Add("children", list);
		return jObject;
	}

	private BacktraceJObject GetJObject(GameObject gameObject, string parentName = "")
	{
		return new BacktraceJObject(new Dictionary<string, string>
		{
			{ "name", gameObject.name },
			{
				"isStatic",
				gameObject.isStatic.ToString(CultureInfo.InvariantCulture).ToLower()
			},
			{
				"layer",
				gameObject.layer.ToString(CultureInfo.InvariantCulture)
			},
			{
				"transform.position",
				gameObject.transform.position.ToString()
			},
			{
				"transform.rotation",
				gameObject.transform.rotation.ToString()
			},
			{ "tag", gameObject.tag },
			{
				"activeInHierarchy",
				gameObject.activeInHierarchy.ToString(CultureInfo.InvariantCulture).ToLower()
			},
			{
				"activeSelf",
				gameObject.activeSelf.ToString(CultureInfo.InvariantCulture).ToLower()
			},
			{
				"instanceId",
				gameObject.GetInstanceID().ToString(CultureInfo.InvariantCulture)
			},
			{
				"parentName",
				string.IsNullOrEmpty(parentName) ? "root object" : parentName
			}
		});
	}

	private BacktraceJObject GetJObject(Component gameObject, string parentName = "")
	{
		return new BacktraceJObject(new Dictionary<string, string>
		{
			{ "name", gameObject.name },
			{
				"transform.position",
				gameObject.transform.position.ToString()
			},
			{
				"transform.rotation",
				gameObject.transform.rotation.ToString()
			},
			{ "tag", gameObject.tag },
			{
				"instanceId",
				gameObject.GetInstanceID().ToString(CultureInfo.InvariantCulture)
			},
			{
				"parentName",
				string.IsNullOrEmpty(parentName) ? "root object" : parentName
			}
		});
	}
}
