using System;

namespace Modio.Customizations;

[Serializable]
internal struct WssErrorObject
{
	public WssError error;

	public string operation;
}
