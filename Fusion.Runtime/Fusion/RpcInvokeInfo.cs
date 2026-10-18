using System;

namespace Fusion;

[Serializable]
public struct RpcInvokeInfo
{
	public RpcLocalInvokeResult LocalInvokeResult;

	public RpcSendCullResult SendCullResult;

	public RpcSendResult SendResult;

	public override string ToString()
	{
		return $"[Local: {LocalInvokeResult}, SendCull: {SendCullResult}, Send: {SendResult}]";
	}
}
