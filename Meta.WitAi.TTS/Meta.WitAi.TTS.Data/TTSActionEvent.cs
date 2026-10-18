using System;
using Meta.WitAi.Json;

namespace Meta.WitAi.TTS.Data;

[Serializable]
public class TTSActionEvent : TTSStringEvent
{
	private WitResponseNode response;

	public static readonly WitResponseNode EMPTY_RESPONSE = new WitResponseNode();

	public WitResponseNode Response
	{
		get
		{
			if (string.IsNullOrEmpty(base.Data))
			{
				return EMPTY_RESPONSE;
			}
			if (null == response)
			{
				response = WitResponseNode.Parse(base.Data);
			}
			return response;
		}
	}
}
