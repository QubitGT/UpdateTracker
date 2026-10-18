using System;
using System.Text;

namespace Fusion;

[Serializable]
public struct RpcSendResult
{
	public RpcSendMessageResult Result;

	public int MessageSize;

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("[");
		stringBuilder.Append(Result.ToString());
		stringBuilder.Append(", Size: ");
		stringBuilder.Append(MessageSize);
		stringBuilder.Append("}");
		stringBuilder.Append("]");
		return stringBuilder.ToString();
	}
}
