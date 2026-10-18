namespace Valve.Newtonsoft.Json.Serialization;

public abstract class NamingStrategy
{
	public bool ProcessDictionaryKeys { get; set; }

	public bool OverrideSpecifiedNames { get; set; }

	public virtual string GetPropertyName(string name, bool hasSpecifiedName)
	{
		if (hasSpecifiedName && !OverrideSpecifiedNames)
		{
			return name;
		}
		return ResolvePropertyName(name);
	}

	public virtual string GetDictionaryKey(string key)
	{
		if (!ProcessDictionaryKeys)
		{
			return key;
		}
		return ResolvePropertyName(key);
	}

	protected abstract string ResolvePropertyName(string name);
}
