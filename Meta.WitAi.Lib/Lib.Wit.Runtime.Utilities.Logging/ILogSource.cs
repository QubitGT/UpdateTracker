using Meta.Voice.Logging;

namespace Lib.Wit.Runtime.Utilities.Logging;

public interface ILogSource
{
	IVLogger Logger { get; }
}
