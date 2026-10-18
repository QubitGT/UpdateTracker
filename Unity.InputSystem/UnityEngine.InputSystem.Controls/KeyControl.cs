using System.Globalization;
using UnityEngine.InputSystem.LowLevel;

namespace UnityEngine.InputSystem.Controls;

public class KeyControl : ButtonControl
{
	private int m_ScanCode;

	public Key keyCode { get; set; }

	public int scanCode
	{
		get
		{
			RefreshConfigurationIfNeeded();
			return m_ScanCode;
		}
	}

	protected override void RefreshConfiguration()
	{
		base.displayName = null;
		m_ScanCode = 0;
		QueryKeyNameCommand command = QueryKeyNameCommand.Create(keyCode);
		if (base.device.ExecuteCommand(ref command) <= 0)
		{
			return;
		}
		m_ScanCode = command.scanOrKeyCode;
		string text = command.ReadKeyName();
		if (string.IsNullOrEmpty(text))
		{
			base.displayName = text;
			return;
		}
		string str = text.ToLowerInvariant();
		if (string.IsNullOrEmpty(str))
		{
			base.displayName = text;
			return;
		}
		TextInfo textInfo = CultureInfo.InvariantCulture.TextInfo;
		base.displayName = textInfo.ToTitleCase(str);
	}
}
