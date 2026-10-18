using System;
using PlayFab.SharedModels;

namespace PlayFab.CloudScriptModels;

[Serializable]
public class UnregisterFunctionRequest : PlayFabRequestCommon
{
	public string FunctionName;
}
