namespace Meta.Voice;

public interface IVoiceRequestResults
{
	int StatusCode { get; }

	string Message { get; }

	void SetCancel(string reason);

	void SetError(int errorStatusCode, string error);
}
