namespace Liv.Lck;

public class AnotherExamplePlugin : LCKPluginBase
{
	public override string PluginName => "AnotherExamplePlugin";

	public override string PluginVersion => "1.0.0";

	protected override void OnInitialize()
	{
		LckLog.Log("AnotherExamplePlugin initialized", "OnInitialize", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\ExamplePlugin.cs", 97);
	}

	protected override void OnShutdown()
	{
		LckLog.Log("AnotherExamplePlugin shutdown", "OnShutdown", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\ExamplePlugin.cs", 102);
	}

	public void DoSomethingElse()
	{
		LckLog.Log("AnotherExamplePlugin is doing something else!", "DoSomethingElse", ".\\Packages\\tv.liv.lck\\Runtime\\Scripts\\ExamplePlugin.cs", 107);
	}
}
