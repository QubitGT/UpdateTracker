namespace Sirenix.OdinInspector;

public struct ValueDropdownItem : IValueDropdownItem
{
	public string Text;

	public object Value;

	public ValueDropdownItem(string text, object value)
	{
		Text = text;
		Value = value;
	}

	public override string ToString()
	{
		return Text ?? Value?.ToString() ?? "";
	}

	string IValueDropdownItem.GetText()
	{
		return Text;
	}

	object IValueDropdownItem.GetValue()
	{
		return Value;
	}
}
public struct ValueDropdownItem<T> : IValueDropdownItem
{
	public string Text;

	public T Value;

	public ValueDropdownItem(string text, T value)
	{
		Text = text;
		Value = value;
	}

	string IValueDropdownItem.GetText()
	{
		return Text;
	}

	object IValueDropdownItem.GetValue()
	{
		return Value;
	}

	public override string ToString()
	{
		object obj = Text;
		if (obj == null)
		{
			T value = Value;
			obj = value?.ToString() ?? "";
		}
		return (string)obj;
	}
}
