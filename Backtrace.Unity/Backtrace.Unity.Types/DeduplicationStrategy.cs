using System;
using UnityEngine;

namespace Backtrace.Unity.Types;

[Flags]
public enum DeduplicationStrategy
{
	[Tooltip("Deduplication rules are disabled.")]
	[InspectorName("Disable")]
	None = 0,
	[Tooltip("Faulting callstack - use the faulting callstack as a factor in client-side rate limiting.")]
	[InspectorName("Faulting callstack")]
	Default = 1,
	[Tooltip("Unity by default will validate ssl certificates. By using this option you can avoid ssl certificates validation. However, if you don't need to ignore ssl validation, please set this option to false.", order = 0)]
	[InspectorName("Exception type")]
	Classifier = 2,
	[Tooltip("Unity by default will validate ssl certificates. By using this option you can avoid ssl certificates validation. However, if you don't need to ignore ssl validation, please set this option to false.", order = 0)]
	[InspectorName("Exception message")]
	Message = 4
}
