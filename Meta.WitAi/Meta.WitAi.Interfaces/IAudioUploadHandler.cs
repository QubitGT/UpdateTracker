using System;
using Meta.WitAi.Data;

namespace Meta.WitAi.Interfaces;

public interface IAudioUploadHandler : IDataUploadHandler
{
	bool IsInputStreamReady { get; }

	Action OnInputStreamReady { get; set; }

	AudioEncoding AudioEncoding { get; set; }
}
