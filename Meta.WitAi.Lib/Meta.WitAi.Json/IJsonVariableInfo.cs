using System;

namespace Meta.WitAi.Json;

internal interface IJsonVariableInfo
{
	string[] GetSerializeNames();

	bool GetShouldSerialize();

	bool GetShouldDeserialize();

	Type GetVariableType();

	object GetValue(object obj);

	void SetValue(object obj, object newValue);
}
