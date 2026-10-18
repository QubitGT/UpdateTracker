using System.Reflection;
using Meta.XR.ImmersiveDebugger.Utils;

namespace Meta.XR.ImmersiveDebugger.Manager;

internal abstract class Hook
{
	private readonly InstanceHandle _instanceHandle;

	private readonly DebugMember _attribute;

	protected readonly MemberInfo _memberInfo;

	protected readonly object _instance;

	public DebugMember Attribute => _attribute;

	public MemberInfo MemberInfo => _memberInfo;

	public bool Valid => _instanceHandle.Valid;

	protected Hook(MemberInfo memberInfo, InstanceHandle instanceHandle, DebugMember attribute)
	{
		_memberInfo = memberInfo;
		_instanceHandle = instanceHandle;
		_instance = _instanceHandle.Instance;
		_attribute = attribute;
	}

	public bool Matches(MemberInfo memberInfo, InstanceHandle instance)
	{
		if (_memberInfo == memberInfo)
		{
			return _instanceHandle.Equals(instance);
		}
		return false;
	}
}
