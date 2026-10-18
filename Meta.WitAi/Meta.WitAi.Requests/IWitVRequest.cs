using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Meta.WitAi.Requests;

internal interface IWitVRequest : IVRequest
{
	Task<VRequestResponse<TValue>> RequestWitGet<TValue>(string endpoint, Dictionary<string, string> urlParameters, Action<TValue> onPartial = null);

	Task<VRequestResponse<TValue>> RequestWitPost<TValue>(string endpoint, Dictionary<string, string> urlParameters, string payload, Action<TValue> onPartial = null);

	Task<VRequestResponse<TValue>> RequestWitPut<TValue>(string endpoint, Dictionary<string, string> urlParameters, string payload, Action<TValue> onPartial = null);
}
