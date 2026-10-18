using System;

namespace Fusion;

[Flags]
public enum NetworkSpawnFlags : short
{
	DontDestroyOnLoad = 1,
	SharedModeStateAuthMasterClient = 2,
	SharedModeStateAuthLocalPlayer = 4
}
