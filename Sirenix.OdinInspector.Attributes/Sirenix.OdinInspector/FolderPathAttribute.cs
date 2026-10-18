using System;
using System.ComponentModel;
using System.Diagnostics;
using UnityEngine;

namespace Sirenix.OdinInspector;

[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
[Conditional("UNITY_EDITOR")]
public sealed class FolderPathAttribute : Attribute
{
	public bool AbsolutePath;

	public string ParentFolder;

	[HideInInspector]
	[Obsolete("Use RequireExistingPath instead.", true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool RequireValidPath;

	public bool RequireExistingPath;

	public bool UseBackslashes;
}
