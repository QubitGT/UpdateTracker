using System;
using System.Collections.Generic;
using Backtrace.Unity.Model.Attributes;

namespace Backtrace.Unity.Model.JsonData;

public sealed class AttributeProvider
{
	private readonly IDictionary<string, string> _attributes = new Dictionary<string, string>();

	private readonly IList<IDynamicAttributeProvider> _dynamicAttributeProvider;

	public string ApplicationVersion => this["application.version"];

	public string ApplicationSessionKey
	{
		get
		{
			if (_attributes.TryGetValue("application.session", out var value))
			{
				return value;
			}
			return null;
		}
	}

	public string ApplicationGuid => this["guid"];

	public string this[string index]
	{
		get
		{
			return _attributes[index];
		}
		set
		{
			_attributes[index] = value;
		}
	}

	internal AttributeProvider()
		: this(new List<IScopeAttributeProvider>
		{
			new MachineAttributeProvider(),
			new RuntimeAttributeProvider(),
			new PiiAttributeProvider()
		}, new List<IDynamicAttributeProvider>
		{
			new MachineStateAttributeProvider(),
			new ProcessAttributeProvider(),
			new SceneAttributeProvider()
		})
	{
	}

	internal AttributeProvider(IEnumerable<IScopeAttributeProvider> scopeAttributeProvider, IList<IDynamicAttributeProvider> dynamicAttributeProvider)
	{
		if (scopeAttributeProvider == null)
		{
			throw new ArgumentException("Scoped attributes provider collection is not defined");
		}
		if (dynamicAttributeProvider == null)
		{
			throw new ArgumentException("dynamic attributes provider colleciton is not defined");
		}
		foreach (IScopeAttributeProvider item in scopeAttributeProvider)
		{
			item.GetAttributes(_attributes);
		}
		_dynamicAttributeProvider = dynamicAttributeProvider;
	}

	public int Count()
	{
		return _attributes.Count;
	}

	public void AddDynamicAttributeProvider(IDynamicAttributeProvider attributeProvider)
	{
		if (attributeProvider != null)
		{
			_dynamicAttributeProvider.Add(attributeProvider);
		}
	}

	internal void AddScopedAttributeProvider(IScopeAttributeProvider attributeProvider)
	{
		attributeProvider?.GetAttributes(_attributes);
	}

	internal void AddAttributes(IDictionary<string, string> source, bool includeDynamic = true)
	{
		if (includeDynamic)
		{
			foreach (IDynamicAttributeProvider item in _dynamicAttributeProvider)
			{
				item.GetAttributes(source);
			}
		}
		foreach (KeyValuePair<string, string> attribute in _attributes)
		{
			source[attribute.Key] = attribute.Value;
		}
	}

	internal IDictionary<string, string> GenerateAttributes(bool includeDynamic = true)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		AddAttributes(dictionary, includeDynamic);
		return dictionary;
	}
}
