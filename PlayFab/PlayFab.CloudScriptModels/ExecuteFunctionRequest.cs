using System;
using PlayFab.SharedModels;

namespace PlayFab.CloudScriptModels;

[Serializable]
public class ExecuteFunctionRequest : PlayFabRequestCommon
{
	public EntityKey Entity;

	public string FunctionName;

	public object FunctionParameter;

	public bool? GeneratePlayStreamEvent;
}
