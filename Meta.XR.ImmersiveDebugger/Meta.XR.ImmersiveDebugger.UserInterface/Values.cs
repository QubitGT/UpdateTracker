using System.Collections.Generic;
using Meta.XR.ImmersiveDebugger.Manager;
using Meta.XR.ImmersiveDebugger.UserInterface.Generic;

namespace Meta.XR.ImmersiveDebugger.UserInterface;

public class Values : Controller
{
	protected readonly List<Value> _values = new List<Value>();

	internal List<Value> GetValues => _values;

	internal Watch Watch { get; private set; }

	internal ImageStyle BackgroundStyle
	{
		set
		{
			foreach (Value value2 in _values)
			{
				value2.BackgroundStyle = value;
			}
		}
	}

	internal TextStyle TextStyle
	{
		set
		{
			foreach (Value value2 in _values)
			{
				value2.TextStyle = value;
			}
		}
	}

	internal void Setup(Watch watch)
	{
		if (watch == Watch)
		{
			return;
		}
		Watch = watch;
		foreach (Value value2 in _values)
		{
			base.Owner.Remove(value2, destroy: true);
		}
		_values.Clear();
		for (int i = 0; i < Watch?.NumberOfValues; i++)
		{
			Value value = base.Owner.Append<Value>($"value {i}");
			value.LayoutStyle = Style.Instantiate<LayoutStyle>("MemberValueDynamic");
			value.TextStyle = Style.Load<TextStyle>("MemberValue");
			value.BackgroundStyle = Style.Load<ImageStyle>("MemberValueBackground");
			_values.Add(value);
		}
	}

	private void Update()
	{
		Watch watch = Watch;
		if (watch == null || !watch.Valid)
		{
			return;
		}
		string[] values = Watch.Values;
		int num = Watch.NumberOfValues;
		foreach (Value value in _values)
		{
			value.Content = values[--num];
		}
	}
}
