using System;

namespace Fusion.Photon.Realtime;

[Flags]
internal enum PropertyTypeFlag : byte
{
	None = 0,
	Game = 1,
	Actor = 2,
	GameAndActor = 3
}
