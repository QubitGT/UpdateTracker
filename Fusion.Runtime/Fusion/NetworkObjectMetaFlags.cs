using System;

namespace Fusion;

[Flags]
internal enum NetworkObjectMetaFlags
{
	None = 0,
	InstanceWillNotBeCreated = 1
}
