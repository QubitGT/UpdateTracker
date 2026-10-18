using System;
using PlayFab.SharedModels;

namespace PlayFab.CloudScriptModels;

[Serializable]
public class PostFunctionResultForScheduledTaskRequest : PlayFabRequestCommon
{
	public EntityKey Entity;

	public ExecuteFunctionResult FunctionResult;

	public NameIdentifier ScheduledTaskId;
}
