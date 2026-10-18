using System;
using PlayFab.SharedModels;

namespace PlayFab.CloudScriptModels;

[Serializable]
public class PostFunctionResultForPlayerTriggeredActionRequest : PlayFabRequestCommon
{
	public EntityKey Entity;

	public ExecuteFunctionResult FunctionResult;

	public PlayerProfileModel PlayerProfile;

	public PlayStreamEventEnvelopeModel PlayStreamEventEnvelope;
}
