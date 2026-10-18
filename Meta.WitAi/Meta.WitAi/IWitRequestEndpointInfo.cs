namespace Meta.WitAi;

public interface IWitRequestEndpointInfo
{
	string UriScheme { get; }

	string Authority { get; }

	int Port { get; }

	string WitApiVersion { get; }

	string Message { get; }

	string Speech { get; }

	string Dictation { get; }

	string Synthesize { get; }

	string Event { get; }

	string Converse { get; }
}
