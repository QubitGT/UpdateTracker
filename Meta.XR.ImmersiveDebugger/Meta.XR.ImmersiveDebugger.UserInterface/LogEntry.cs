using System;

namespace Meta.XR.ImmersiveDebugger.UserInterface;

internal class LogEntry
{
	public static Action<LogEntry> OnDisplayDetails { get; set; }

	public string Label { get; private set; }

	public string Callstack { get; private set; }

	public SeverityEntry Severity { get; private set; }

	public int Count { get; set; }

	public ProxyConsoleLine Line { get; set; }

	public bool Shown => Line != null;

	public void Setup(string label, string callstack, SeverityEntry severity)
	{
		Label = label;
		Callstack = callstack;
		Severity = severity;
		Line = null;
		Count = 1;
	}

	public void DisplayDetails()
	{
		OnDisplayDetails?.Invoke(this);
	}
}
