using System;
using UnityEngine;

namespace Meta.WitAi.Events;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class EventCategoryAttribute : PropertyAttribute
{
	public readonly string Category;

	public EventCategoryAttribute(string category = "")
	{
		Category = category;
	}
}
