using System;
using System.Reflection;
using System.Text;
using Meta.Conduit;
using Meta.WitAi.Data;
using Meta.WitAi.Json;

namespace Meta.WitAi;

[Obsolete("Use ParameterProvider.SetSpecializedParameter() instead of this class")]
internal class WitConduitParameterProvider : ParameterProvider
{
	protected override object GetSpecializedParameter(ParameterInfo formalParameter)
	{
		if (formalParameter.ParameterType == typeof(WitResponseNode) && ActualParameters.ContainsKey("@WitResponseNode".ToLower()))
		{
			return ActualParameters["@WitResponseNode".ToLower()];
		}
		if (formalParameter.ParameterType == typeof(VoiceSession) && ActualParameters.ContainsKey("@VoiceSession".ToLower()))
		{
			return ActualParameters["@VoiceSession".ToLower()];
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Specialized parameter not found");
		stringBuilder.AppendLine($"Parameter Type: {formalParameter.ParameterType}");
		stringBuilder.AppendLine("Parameter Name: " + formalParameter.Name);
		stringBuilder.AppendLine($"Actual Parameters: {ActualParameters.Keys.Count}");
		foreach (string key in ActualParameters.Keys)
		{
			string text = ((ActualParameters[key] == null) ? "NULL" : ActualParameters[key].GetType().ToString());
			stringBuilder.AppendLine("\t" + key + ": " + text);
		}
		VLog.W(stringBuilder.ToString());
		return null;
	}

	protected override bool SupportedSpecializedParameter(ParameterInfo formalParameter)
	{
		if (!(formalParameter.ParameterType == typeof(WitResponseNode)))
		{
			return formalParameter.ParameterType == typeof(VoiceSession);
		}
		return true;
	}
}
