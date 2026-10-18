using System;

namespace Liv.Lck;

internal interface ILckPreviewer : IDisposable
{
	bool IsPreviewActive { get; set; }
}
