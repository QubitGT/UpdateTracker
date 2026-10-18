using System;
using UnityEngine;

namespace Meta.WitAi.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class HideWhenSetAttribute : PropertyAttribute
{
}
