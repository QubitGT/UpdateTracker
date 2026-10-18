using System;
using System.Reflection;

namespace Meta.WitAi;

internal class RegisteredMatchIntent
{
	public Type type;

	public MethodInfo method;

	public MatchIntent matchIntent;
}
