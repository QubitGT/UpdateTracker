using System;
using PlayFab.SharedModels;

namespace PlayFab.AuthenticationModels;

[Serializable]
public class ValidateEntityTokenResponse : PlayFabResultCommon
{
	public EntityKey Entity;

	public LoginIdentityProvider? IdentityProvider;

	public EntityLineage Lineage;
}
