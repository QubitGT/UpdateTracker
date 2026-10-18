using System;
using UnityEngine;

namespace Backtrace.Unity.Types;

[Flags]
public enum ReportFilterType
{
	[Tooltip("Disable report filtering.")]
	[InspectorName("Disable")]
	None = 0,
	[Tooltip("String message report.")]
	Message = 1,
	[Tooltip("Handled exception.")]
	[InspectorName("Handled exception")]
	Exception = 2,
	[Tooltip("Game unhandled exception.")]
	[InspectorName("Unhandled exception")]
	UnhandledException = 4,
	[Tooltip("Game hang.")]
	Hang = 8,
	[Tooltip("Game error.")]
	[InspectorName("Game error")]
	Error = 0x10
}
