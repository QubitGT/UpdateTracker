using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Meta.WitAi;
using Meta.WitAi.Json;
using UnityEngine.Scripting;

namespace Meta.Conduit;

internal class Manifest
{
	[Preserve]
	public List<ManifestErrorHandler> ErrorHandlers = new List<ManifestErrorHandler>();

	private readonly Dictionary<string, List<InvocationContext>> _methodLookup = new Dictionary<string, List<InvocationContext>>(StringComparer.OrdinalIgnoreCase);

	[JsonIgnore]
	public static List<string> WitResponseMatcherIntents = new List<string>();

	[Preserve]
	public string ID { get; set; }

	[Preserve]
	public string Version { get; set; }

	[Preserve]
	public string Domain { get; set; }

	[Preserve]
	public List<ManifestEntity> Entities { get; set; } = new List<ManifestEntity>();


	[Preserve]
	public List<ManifestAction> Actions { get; set; } = new List<ManifestAction>();


	[JsonIgnore]
	public Dictionary<string, Type> CustomEntityTypes { get; } = new Dictionary<string, Type>();


	[Preserve]
	public Manifest()
	{
	}

	public bool ResolveEntities()
	{
		bool result = true;
		foreach (ManifestEntity entity in Entities)
		{
			string text = (string.IsNullOrEmpty(entity.Namespace) ? entity.ID : (entity.Namespace + "." + entity.ID)) + "," + entity.Assembly;
			Type type = Type.GetType(text);
			if (type == null)
			{
				VLog.E(GetType().Name, "Failed to resolve type: " + text);
				result = false;
			}
			CustomEntityTypes[entity.Name] = type;
		}
		return result;
	}

	public Tuple<MethodInfo, Type> GetMethodInfo(IManifestMethod action)
	{
		if (action == null)
		{
			VLog.E(GetType().Name, "Cannot get MethodInfo without provided action");
			return null;
		}
		string iD = action.ID;
		int num = (string.IsNullOrEmpty(iD) ? (-1) : iD.LastIndexOf('.'));
		if (num <= 0)
		{
			VLog.E(GetType().Name, "Invalid Action ID: " + iD);
			return null;
		}
		string text = iD.Substring(0, num);
		string text2 = text;
		text2 += ((action.Assembly != null) ? ("," + action.Assembly) : "");
		string text3 = iD.Substring(num + 1);
		Type type = Type.GetType(text2);
		if (type == null)
		{
			VLog.E(GetType().Name, "Failed to resolve type: " + text2);
			return null;
		}
		int num2 = ((action.Parameters != null) ? action.Parameters.Count : 0);
		Type[] array = new Type[num2];
		for (int i = 0; i < num2; i++)
		{
			ManifestParameter manifestParameter = action.Parameters[i];
			string text4 = manifestParameter.QualifiedTypeName + "," + manifestParameter.TypeAssembly;
			array[i] = Type.GetType(text4);
			if (array[i] == null)
			{
				VLog.E(GetType().Name, "Failed to resolve type: " + text4);
			}
		}
		MethodInfo bestMethodMatch = GetBestMethodMatch(type, text3, array);
		if (bestMethodMatch == null)
		{
			VLog.E(GetType().Name, "Failed to resolve method " + text + "." + text3 + ".");
			return null;
		}
		return Tuple.Create(bestMethodMatch, type);
	}

	private bool ResolveAllActions()
	{
		bool result = true;
		foreach (ManifestAction action in Actions)
		{
			Tuple<MethodInfo, Type> methodInfo = GetMethodInfo(action);
			if (methodInfo == null)
			{
				return false;
			}
			MethodInfo item = methodInfo.Item1;
			Type item2 = methodInfo.Item2;
			if (item == null)
			{
				VLog.E(GetType().Name, "Invalid Action ID: " + action.ID);
				result = false;
				continue;
			}
			object[] customAttributes = item.GetCustomAttributes(typeof(ConduitActionAttribute), inherit: false);
			if (customAttributes.Length == 0)
			{
				VLog.E(GetType().Name, $"{item} - Did not have expected Conduit attribute");
				result = false;
				continue;
			}
			ConduitActionAttribute conduitActionAttribute = customAttributes.First() as ConduitActionAttribute;
			InvocationContext item3 = new InvocationContext
			{
				Type = item2,
				MethodInfo = item,
				MinConfidence = conduitActionAttribute.MinConfidence,
				MaxConfidence = conduitActionAttribute.MaxConfidence,
				ValidatePartial = conduitActionAttribute.ValidatePartial
			};
			if (!_methodLookup.ContainsKey(action.Name))
			{
				_methodLookup.Add(action.Name, new List<InvocationContext>());
			}
			_methodLookup[action.Name].Add(item3);
		}
		foreach (List<InvocationContext> item4 in _methodLookup.Values.Where((List<InvocationContext> invocationContext) => invocationContext.Count > 1))
		{
			item4.Sort((InvocationContext one, InvocationContext two) => two.MethodInfo.GetParameters().Length - one.MethodInfo.GetParameters().Length);
		}
		return result;
	}

	private bool ResolveErrorHandlers()
	{
		if (ErrorHandlers == null)
		{
			return true;
		}
		bool result = true;
		foreach (ManifestErrorHandler errorHandler in ErrorHandlers)
		{
			Tuple<MethodInfo, Type> methodInfo = GetMethodInfo(errorHandler);
			MethodInfo item = methodInfo.Item1;
			Type item2 = methodInfo.Item2;
			if (item == null)
			{
				VLog.E(GetType().Name, "Invalid Action ID: " + errorHandler.ID);
				result = false;
				continue;
			}
			object[] customAttributes = item.GetCustomAttributes(typeof(HandleEntityResolutionFailure), inherit: false);
			if (customAttributes.Length == 0)
			{
				VLog.E(GetType().Name, $"{item} - Did not have expected Conduit attribute");
				result = false;
				continue;
			}
			if (!(customAttributes.First() is HandleEntityResolutionFailure))
			{
				VLog.E(GetType().Name, "Found null attribute when one was expected");
				continue;
			}
			InvocationContext item3 = new InvocationContext
			{
				Type = item2,
				MethodInfo = item,
				CustomAttributeType = typeof(HandleEntityResolutionFailure)
			};
			if (!_methodLookup.ContainsKey(errorHandler.Name))
			{
				_methodLookup.Add(errorHandler.Name, new List<InvocationContext>());
			}
			_methodLookup[errorHandler.Name].Add(item3);
		}
		foreach (List<InvocationContext> item4 in _methodLookup.Values.Where((List<InvocationContext> invocationContext) => invocationContext.Count > 1))
		{
			item4.Sort((InvocationContext one, InvocationContext two) => two.MethodInfo.GetParameters().Length - one.MethodInfo.GetParameters().Length);
		}
		return result;
	}

	public bool ResolveActions()
	{
		if (ResolveAllActions())
		{
			return ResolveErrorHandlers();
		}
		return false;
	}

	private MethodInfo GetBestMethodMatch(Type targetType, string method, Type[] parameterTypes)
	{
		return targetType.GetMethod(method, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, CallingConventions.Any, parameterTypes, null);
	}

	public bool ContainsAction(string actionId)
	{
		if (string.IsNullOrEmpty(actionId))
		{
			VLog.E(GetType().Name, "Null or empty action ID supplied");
			return false;
		}
		return _methodLookup.ContainsKey(actionId);
	}

	public List<InvocationContext> GetInvocationContexts(string actionId)
	{
		if (!_methodLookup.ContainsKey(actionId))
		{
			return null;
		}
		return _methodLookup[actionId];
	}

	public override string ToString()
	{
		return JsonConvert.SerializeObject(this);
	}

	public List<InvocationContext> GetErrorHandlerContexts()
	{
		List<InvocationContext> list = new List<InvocationContext>();
		foreach (List<InvocationContext> value in _methodLookup.Values)
		{
			foreach (InvocationContext item in value)
			{
				if (item.CustomAttributeType == typeof(HandleEntityResolutionFailure))
				{
					list.Add(item);
				}
			}
		}
		return list;
	}
}
