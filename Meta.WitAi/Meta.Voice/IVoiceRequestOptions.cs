namespace Meta.Voice;

public interface IVoiceRequestOptions
{
	string RequestId { get; }

	string ClientUserId { get; }

	string OperationId { get; }

	int TimeoutMs { get; set; }
}
