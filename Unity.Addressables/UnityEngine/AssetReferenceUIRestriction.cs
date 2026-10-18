using System;

namespace UnityEngine;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class AssetReferenceUIRestriction : Attribute
{
	public virtual bool ValidateAsset(Object obj)
	{
		return true;
	}

	public virtual bool ValidateAsset(string path)
	{
		return true;
	}
}
