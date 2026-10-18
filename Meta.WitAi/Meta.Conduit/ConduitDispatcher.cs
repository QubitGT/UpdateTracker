using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Meta.WitAi;

namespace Meta.Conduit;

internal class ConduitDispatcher : IConduitDispatcher
{
	internal class InvocationContextFilter
	{
		private readonly List<InvocationContext> _actionContexts;

		private readonly IParameterProvider _parameterProvider;

		private readonly bool _relaxed;

		public InvocationContextFilter(IParameterProvider parameterProvider, List<InvocationContext> actionContexts, bool relaxed = false)
		{
			_parameterProvider = parameterProvider;
			_actionContexts = actionContexts;
			_relaxed = relaxed;
		}

		public List<InvocationContext> ResolveInvocationContexts(string actionId, float confidence, bool partial)
		{
			if (_actionContexts == null)
			{
				return new List<InvocationContext>();
			}
			return _actionContexts.Where((InvocationContext context) => CompatibleInvocationContext(context, confidence, partial)).ToList();
		}

		private bool CompatibleInvocationContext(InvocationContext invocationContext, float confidence, bool partial)
		{
			Dictionary<string, string> parameterMap = new Dictionary<string, string>();
			ParameterInfo[] parameters = invocationContext.MethodInfo.GetParameters();
			if (invocationContext.ValidatePartial != partial)
			{
				return false;
			}
			if (invocationContext.MinConfidence > confidence || confidence > invocationContext.MaxConfidence)
			{
				return false;
			}
			HashSet<string> hashSet = new HashSet<string>();
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = true;
			ParameterInfo[] array = parameters;
			foreach (ParameterInfo parameterInfo in array)
			{
				if (_parameterProvider.ContainsParameter(parameterInfo, stringBuilder))
				{
					hashSet.Add(parameterInfo.Name);
				}
				else if (!parameterInfo.ParameterType.IsNullableType())
				{
					VLog.D((!_relaxed) ? ("Could not find value for parameter: " + parameterInfo.Name) : ("Could not find exact value for parameter: " + parameterInfo.Name + ". Will attempt resolving by type."));
					flag = false;
				}
			}
			if (flag)
			{
				return true;
			}
			if (!_relaxed)
			{
				VLog.D($"Failed to resolve parameters. \nType: {invocationContext.Type.FullName}\nMethod: {invocationContext.MethodInfo.Name}\n{stringBuilder}");
				return false;
			}
			HashSet<Type> actualTypes = new HashSet<Type>();
			return ResolveByType(invocationContext, parameters, hashSet, actualTypes, parameterMap);
		}

		private bool ResolveByType(InvocationContext invocationContext, ParameterInfo[] parameters, ICollection<string> exactMatches, ISet<Type> actualTypes, Dictionary<string, string> parameterMap)
		{
			foreach (ParameterInfo parameterInfo in parameters)
			{
				if (!exactMatches.Contains(parameterInfo.Name))
				{
					if (actualTypes.Contains(parameterInfo.ParameterType))
					{
						VLog.D($"Failed to resolve parameters by type. More than one value of type {parameterInfo.ParameterType} were provided.");
						return false;
					}
					actualTypes.Add(parameterInfo.ParameterType);
					List<string> list = (from parameterName in _parameterProvider.GetParameterNamesOfType(parameterInfo.ParameterType)
						where !exactMatches.Contains(parameterName)
						select parameterName).ToList();
					if (list.Count != 1)
					{
						VLog.D("Failed to find compatible value for " + parameterInfo.Name);
						return false;
					}
					parameterMap[parameterInfo.Name] = list[0];
				}
			}
			invocationContext.ParameterMap = parameterMap;
			return true;
		}
	}

	private readonly IManifestLoader _manifestLoader;

	private readonly IInstanceResolver _instanceResolver;

	private bool _isInitializing;

	private bool _isInitialized;

	private readonly Dictionary<string, string> _parameterToRoleMap = new Dictionary<string, string>();

	private readonly HashSet<string> _ignoredActionIds = new HashSet<string>();

	public Manifest Manifest { get; private set; }

	public ConduitDispatcher(IManifestLoader manifestLoader, IInstanceResolver instanceResolver)
	{
		_manifestLoader = manifestLoader;
		_instanceResolver = instanceResolver;
	}

	public async Task Initialize(string manifestFilePath)
	{
		if (Manifest != null || _isInitializing)
		{
			return;
		}
		_isInitializing = true;
		Manifest = await _manifestLoader.LoadManifestAsync(manifestFilePath);
		if (Manifest == null)
		{
			_isInitializing = false;
			return;
		}
		foreach (ManifestAction action in Manifest.Actions)
		{
			foreach (ManifestParameter parameter in action.Parameters)
			{
				if (!_parameterToRoleMap.ContainsKey(parameter.InternalName))
				{
					_parameterToRoleMap.Add(parameter.InternalName, parameter.QualifiedName);
				}
			}
		}
		_isInitializing = false;
		_isInitialized = true;
	}

	public bool InvokeAction(IParameterProvider parameterProvider, string actionId, bool relaxed, float confidence = 1f, bool partial = false)
	{
		if (!_isInitialized)
		{
			VLog.W("Conduit Manifest is not yet initialized");
			return false;
		}
		if (!Manifest.ContainsAction(actionId))
		{
			bool flag = Manifest.WitResponseMatcherIntents.Contains(actionId);
			if (!_ignoredActionIds.Contains(actionId) && !flag)
			{
				_ignoredActionIds.Add(actionId);
				InvokeError(actionId, new Exception("Conduit did not find intent '" + actionId + "' in manifest."));
				VLog.W("Conduit did not find intent '" + actionId + "' in manifest.");
			}
			return false;
		}
		parameterProvider.PopulateRoles(_parameterToRoleMap);
		InvocationContextFilter invocationContextFilter = new InvocationContextFilter(parameterProvider, Manifest.GetInvocationContexts(actionId), relaxed);
		List<InvocationContext> list = invocationContextFilter.ResolveInvocationContexts(actionId, confidence, partial);
		if (list.Count < 1)
		{
			if (!partial && invocationContextFilter.ResolveInvocationContexts(actionId, confidence, partial: true).Count < 1)
			{
				VLog.W("Failed to resolve " + (partial ? "partial" : "final") + " method for " + actionId + " with supplied context");
				InvokeError(actionId, new Exception("Failed to resolve " + (partial ? "partial" : "final") + " method for " + actionId + " with supplied context"));
			}
			return false;
		}
		bool result = true;
		foreach (InvocationContext item in list)
		{
			try
			{
				if (!InvokeMethod(item, parameterProvider, relaxed))
				{
					result = false;
				}
			}
			catch (Exception ex)
			{
				VLog.W($"Failed to invoke {item.MethodInfo.Name}. {ex}");
				result = false;
				InvokeError(item.MethodInfo.Name, ex);
			}
		}
		return result;
	}

	public bool InvokeError(string actionId, Exception exception)
	{
		if (!_isInitialized)
		{
			VLog.E($"Attempting to invoke error {actionId} ({exception}) with no initialized manifest.");
			return false;
		}
		foreach (InvocationContext errorHandlerContext in Manifest.GetErrorHandlerContexts())
		{
			ParameterProvider parameterProvider = new ParameterProvider();
			parameterProvider.AddParameter("intent", actionId);
			parameterProvider.AddParameter("error", exception);
			InvokeMethod(errorHandlerContext, parameterProvider, relaxed: true);
		}
		return true;
	}

	private bool InvokeMethod(InvocationContext invocationContext, IParameterProvider parameterProvider, bool relaxed)
	{
		MethodInfo methodInfo = invocationContext.MethodInfo;
		ParameterInfo[] parameters = methodInfo.GetParameters();
		object[] array = new object[parameters.Length];
		for (int i = 0; i < parameters.Length; i++)
		{
			StringBuilder arg = new StringBuilder();
			array[i] = parameterProvider.GetParameterValue(parameters[i], invocationContext.ParameterMap, relaxed);
			if (array[i] == null && !parameters[i].ParameterType.IsNullableType())
			{
				InvokeError(invocationContext.MethodInfo.Name, new Exception($"Failed to find method param while invoking\nType: {invocationContext.Type.FullName}\nMethod: {invocationContext.MethodInfo.Name}\nParameter Issues\n{arg}"));
				VLog.W($"Failed to find method param while invoking\nType: {invocationContext.Type.FullName}\nMethod: {invocationContext.MethodInfo.Name}\nParameter Issues\n{arg}");
				return false;
			}
		}
		if (methodInfo.IsStatic)
		{
			try
			{
				methodInfo.Invoke(null, array.ToArray());
			}
			catch (Exception ex)
			{
				VLog.W($"Failed to invoke static method {methodInfo.Name}. {ex}");
				InvokeError(invocationContext.MethodInfo.Name, ex);
				return false;
			}
			return true;
		}
		bool result = true;
		foreach (object item in _instanceResolver.GetObjectsOfType(invocationContext.Type))
		{
			try
			{
				methodInfo.Invoke(item, array.ToArray());
			}
			catch (Exception ex2)
			{
				VLog.W($"Failed to invoke method {methodInfo.Name}. {ex2} on {item}");
				result = false;
				InvokeError(invocationContext.MethodInfo.Name, ex2);
			}
		}
		return result;
	}
}
