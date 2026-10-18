using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Meta.WitAi.Configuration;

namespace Meta.WitAi.Requests;

internal class WitVRequest : VRequest, IWitVRequest, IVRequest
{
	private bool _useServerToken;

	[Obsolete("Use WitRequestSettings.OnProvideCustomUri instead.")]
	public static Func<UriBuilder, UriBuilder> OnProvideCustomUri => WitRequestSettings.OnProvideCustomUri;

	[Obsolete("Use WitRequestSettings.OnProvideCustomHeaders instead.")]
	public static Action<Dictionary<string, string>> OnProvideCustomHeaders => WitRequestSettings.OnProvideCustomHeaders;

	[Obsolete("Use WitRequestSettings.OnProvideCustomUserAgent instead.")]
	public static Action<StringBuilder> OnProvideCustomUserAgent => WitRequestSettings.OnProvideCustomUserAgent;

	public WitRequestOptions RequestOptions { get; private set; }

	public string RequestId => RequestOptions.RequestId;

	public IWitRequestConfiguration Configuration { get; private set; }

	public WitVRequest(IWitRequestConfiguration configuration, string requestId, string operationId = null, bool useServerToken = false)
	{
		Configuration = configuration;
		RequestOptions = new WitRequestOptions(requestId, WitRequestSettings.LocalClientUserId, operationId);
		base.TimeoutMs = configuration.RequestTimeoutMs;
		_useServerToken = useServerToken;
	}

	protected bool IsLocalFile()
	{
		if (!string.IsNullOrEmpty(base.Url))
		{
			return base.Url.StartsWith("file://");
		}
		return false;
	}

	protected override Uri GetUri()
	{
		if (IsLocalFile())
		{
			return base.GetUri();
		}
		return WitRequestSettings.GetUri(Configuration, base.Url, base.UrlParameters);
	}

	protected override Dictionary<string, string> GetHeaders()
	{
		if (IsLocalFile())
		{
			return base.GetHeaders();
		}
		return WitRequestSettings.GetHeaders(Configuration, RequestOptions, _useServerToken);
	}

	public override async Task<VRequestResponse<TValue>> Request<TValue>(VRequestDecodeDelegate<TValue> decoder)
	{
		if (Configuration == null)
		{
			return new VRequestResponse<TValue>(-1, "No wit configuration set");
		}
		return await base.Request(decoder);
	}

	public async Task<VRequestResponse<TValue>> RequestWitGet<TValue>(string endpoint, Dictionary<string, string> urlParameters = null, Action<TValue> onPartial = null)
	{
		base.Url = endpoint;
		base.UrlParameters = urlParameters;
		return await RequestJsonGet(onPartial);
	}

	public async Task<VRequestResponse<TValue>> RequestWitPost<TValue>(string endpoint, Dictionary<string, string> urlParameters, string payload, Action<TValue> onPartial = null)
	{
		base.Url = endpoint;
		base.UrlParameters = urlParameters;
		return await RequestJsonPost(payload, onPartial);
	}

	public async Task<VRequestResponse<TValue>> RequestWitPut<TValue>(string endpoint, Dictionary<string, string> urlParameters, string payload, Action<TValue> onPartial = null)
	{
		base.Url = endpoint;
		base.UrlParameters = urlParameters;
		return await RequestJsonPut(payload, onPartial);
	}
}
