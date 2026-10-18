using System;
using PlayFab.SharedModels;

namespace PlayFab.CloudScriptModels;

[Serializable]
public class RegisterHttpFunctionRequest : PlayFabRequestCommon
{
	public string FunctionName;

	public string FunctionUrl;
}
