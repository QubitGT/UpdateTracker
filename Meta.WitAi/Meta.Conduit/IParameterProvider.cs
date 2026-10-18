using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Meta.WitAi.Json;

namespace Meta.Conduit;

public interface IParameterProvider
{
	List<string> AllParameterNames { get; }

	void PopulateParametersFromNode(WitResponseNode responseNode);

	void PopulateRoles(Dictionary<string, string> parameterToRoleMap);

	void AddParameter(string parameterName, object value);

	bool ContainsParameter(ParameterInfo parameter, StringBuilder log);

	void AddCustomType(string name, Type type);

	object GetParameterValue(ParameterInfo formalParameter, Dictionary<string, string> parameterMap = null, bool relaxed = false);

	T GetParameterValue<T>(string parameterName, Dictionary<string, string> parameterMap = null, bool relaxed = false);

	List<string> GetParameterNamesOfType(Type targetType);

	void SetSpecializedParameter(string reservedParameterName, Type parameterType);
}
