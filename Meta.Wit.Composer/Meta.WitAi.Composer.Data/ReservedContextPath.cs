namespace Meta.WitAi.Composer.Data;

public abstract class ReservedContextPath : BaseReservedContextPath
{
	private string _value;

	public string GetValue()
	{
		return _value;
	}

	public void Set(string value)
	{
		_value = value;
		UpdateContextMap();
	}

	protected internal override void UpdateContextMap()
	{
		if (base.Map == null)
		{
			VLog.W($"Missing Composer map for {this}");
		}
		base.Map?.SetData(ReservedPath, _value);
	}

	public override void Clear()
	{
		_value = string.Empty;
		base.Clear();
	}

	public override string ToString()
	{
		return ReservedPath + " : " + _value;
	}
}
