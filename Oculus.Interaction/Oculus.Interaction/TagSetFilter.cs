using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Oculus.Interaction;

public class TagSetFilter : MonoBehaviour, IGameObjectFilter
{
	[Tooltip("A GameObject must meet all required tags.")]
	[SerializeField]
	[Optional]
	private string[] _requireTags;

	[Tooltip("A GameObject must not meet any exclude tags.")]
	[SerializeField]
	[Optional]
	[FormerlySerializedAs("_avoidTags")]
	private string[] _excludeTags;

	private readonly HashSet<string> _requireTagSet = new HashSet<string>();

	private readonly HashSet<string> _excludeTagSet = new HashSet<string>();

	protected virtual void Start()
	{
		string[] requireTags = _requireTags;
		foreach (string item in requireTags)
		{
			_requireTagSet.Add(item);
		}
		requireTags = _excludeTags;
		foreach (string item2 in requireTags)
		{
			_excludeTagSet.Add(item2);
		}
	}

	public bool Filter(GameObject gameObject)
	{
		TagSet component;
		bool flag = gameObject.TryGetComponent<TagSet>(out component);
		if (!flag && _requireTagSet.Count > 0)
		{
			return false;
		}
		foreach (string item in _requireTagSet)
		{
			if (!component.ContainsTag(item))
			{
				return false;
			}
		}
		if (!flag)
		{
			return true;
		}
		foreach (string item2 in _excludeTagSet)
		{
			if (component.ContainsTag(item2))
			{
				return false;
			}
		}
		return true;
	}

	public bool ContainsRequireTag(string tag)
	{
		return _requireTagSet.Contains(tag);
	}

	public void AddRequireTag(string tag)
	{
		_requireTagSet.Add(tag);
	}

	public void RemoveRequireTag(string tag)
	{
		_requireTagSet.Remove(tag);
	}

	public bool ContainsExcludeTag(string tag)
	{
		return _excludeTagSet.Contains(tag);
	}

	public void AddExcludeTag(string tag)
	{
		_excludeTagSet.Add(tag);
	}

	public void RemoveExcludeTag(string tag)
	{
		_excludeTagSet.Remove(tag);
	}

	public void InjectOptionalRequireTags(string[] requireTags)
	{
		_requireTags = requireTags;
	}

	public void InjectOptionalExcludeTags(string[] excludeTags)
	{
		_excludeTags = excludeTags;
	}
}
