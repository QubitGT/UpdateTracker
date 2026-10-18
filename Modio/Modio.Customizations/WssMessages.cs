using System;

namespace Modio.Customizations;

[Serializable]
internal struct WssMessages
{
	public WssMessage[] messages;

	public WssMessages(params WssMessage[] messages)
	{
		this.messages = messages;
	}
}
