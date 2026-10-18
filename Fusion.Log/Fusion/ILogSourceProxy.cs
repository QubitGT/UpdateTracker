using System;

namespace Fusion;

[Obsolete]
public interface ILogSourceProxy
{
	ILogSource LogSource { get; }
}
