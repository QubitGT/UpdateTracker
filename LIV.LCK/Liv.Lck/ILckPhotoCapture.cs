using System;

namespace Liv.Lck;

internal interface ILckPhotoCapture : IDisposable
{
	LckResult Capture();
}
