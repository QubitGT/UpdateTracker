using UnityEngine;

namespace Assets.OVR.Scripts;

internal class FixRecord : Record
{
	public FixMethodDelegate fixMethod;

	public Object targetObject;

	public string[] buttonNames;

	public bool editModeRequired;

	public bool complete;

	public FixRecord(int order, string cat, string msg, FixMethodDelegate fix, Object target, bool editRequired, string[] buttons)
		: base(order, cat, msg)
	{
		buttonNames = buttons;
		fixMethod = fix;
		targetObject = target;
		editModeRequired = editRequired;
		complete = false;
	}
}
