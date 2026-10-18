using System;
using System.Reflection;
using Meta.XR.ImmersiveDebugger.Utils;
using UnityEngine;

namespace Meta.XR.ImmersiveDebugger.Manager;

internal class WatchTexture : Watch
{
	private readonly Func<Texture2D> _getter;

	public Texture2D Texture => _getter();

	public override string Value => string.Empty;

	public override string[] Values => Array.Empty<string>();

	public override int NumberOfValues => 0;

	public WatchTexture(MemberInfo memberInfo, InstanceHandle instanceHandle, DebugMember attribute)
		: base(memberInfo, instanceHandle, attribute)
	{
		WatchTexture watchTexture = this;
		_getter = () => (Texture2D)memberInfo.GetValue(watchTexture._instance);
	}
}
