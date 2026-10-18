using System;
using PlayFab.SharedModels;

namespace PlayFab.CloudScriptModels;

[Serializable]
public class PostFunctionResultForFunctionExecutionRequest : PlayFabRequestCommon
{
	public EntityKey Entity;

	public ExecuteFunctionResult FunctionResult;
}
