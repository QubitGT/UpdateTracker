using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Meta.WitAi.Requests;

internal interface IVRequest
{
	void Cancel();

	Task<VRequestResponse<TValue>> Request<TValue>(VRequestDecodeDelegate<TValue> decoder);

	Task<VRequestResponse<Dictionary<string, string>>> RequestFileHeaders(string url);

	Task<VRequestResponse<byte[]>> RequestFile(string url);

	Task<VRequestResponse<bool>> RequestFileDownload(string url, string downloadPath);

	Task<VRequestResponse<bool>> RequestFileExists(string url);

	Task<VRequestResponse<string>> RequestText(Action<string> onPartial = null);

	Task<VRequestResponse<TData>> RequestJson<TData>(Action<TData> onPartial = null);

	Task<VRequestResponse<TData>> RequestJsonGet<TData>(Action<TData> onPartial = null);

	Task<VRequestResponse<TData>> RequestJsonPost<TData>(Action<TData> onPartial = null);

	Task<VRequestResponse<TData>> RequestJsonPost<TData>(byte[] postData, Action<TData> onPartial = null);

	Task<VRequestResponse<TData>> RequestJsonPost<TData>(string postText, Action<TData> onPartial = null);

	Task<VRequestResponse<TData>> RequestJsonPut<TData>(Action<TData> onPartial = null);

	Task<VRequestResponse<TData>> RequestJsonPut<TData>(byte[] postData, Action<TData> onPartial = null);

	Task<VRequestResponse<TData>> RequestJsonPut<TData>(string postText, Action<TData> onPartial = null);
}
