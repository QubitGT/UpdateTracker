using System;
using PlayFab.SharedModels;

namespace PlayFab.CloudScriptModels;

[Serializable]
public class PostFunctionResultForEntityTriggeredActionRequest : PlayFabRequestCommon
{
	public EntityKey Entity;

	public ExecuteFunctionResult FunctionResult;
}
