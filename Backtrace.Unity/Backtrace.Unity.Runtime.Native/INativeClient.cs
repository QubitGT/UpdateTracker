using Backtrace.Unity.Model.Attributes;

namespace Backtrace.Unity.Runtime.Native;

internal interface INativeClient : IDynamicAttributeProvider
{
	void HandleAnr();

	void SetAttribute(string key, string value);

	bool OnOOM();

	void Update(float time);

	void Disable();

	void PauseAnrThread(bool state);
}
