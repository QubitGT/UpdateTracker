using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Meta.WitAi.Composer.Data;

public abstract class ReservedContextPathDictionary : BaseReservedContextPath
{
	private readonly Dictionary<string, string> _runtimeDynamicContext = new Dictionary<string, string>();

	private ComposerService _composer;

	protected readonly string Separator = "\n";

	public string this[string key]
	{
		get
		{
			if (!_runtimeDynamicContext.ContainsKey(key))
			{
				return string.Empty;
			}
			return _runtimeDynamicContext[key];
		}
		set
		{
			Set(key, value);
		}
	}

	public Dictionary<string, string> GetDictionary()
	{
		return _runtimeDynamicContext;
	}

	public bool Add(string key, string context = null)
	{
		if (_runtimeDynamicContext.ContainsKey(key))
		{
			return false;
		}
		Set(key, context);
		return true;
	}

	public void Set(string key, string context = null)
	{
		if (context == null)
		{
			context = key;
		}
		_runtimeDynamicContext[key] = context;
		UpdateContextMap();
	}

	public void Remove(string key)
	{
		_runtimeDynamicContext.Remove(key);
		string.Join(Separator, _runtimeDynamicContext.Values);
	}

	protected internal override void UpdateContextMap()
	{
		string newValue = string.Join(Separator, _runtimeDynamicContext.Values);
		base.Map.SetData(ReservedPath, newValue);
		string.Join(Separator, _runtimeDynamicContext.Values);
	}

	public override void Clear()
	{
		_runtimeDynamicContext.Clear();
		base.Clear();
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("Reserved Path: " + ReservedPath);
		for (int i = 0; i < _runtimeDynamicContext.Keys.Count; i++)
		{
			string text = _runtimeDynamicContext.Keys.ElementAt(i);
			stringBuilder.Append($"\n\t[{i}] {text} : {_runtimeDynamicContext[text]}");
		}
		return stringBuilder.ToString();
	}
}
