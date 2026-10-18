namespace Oculus.Voice.Core.Bindings.Interfaces;

public interface IConnection
{
	bool IsConnected { get; }

	void Connect(string version);

	void Disconnect();
}
