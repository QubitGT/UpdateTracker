using System.Collections.Generic;

namespace SynthesisvrArcade;

public interface ArcadeInterface
{
	bool timeTracking(string mode);

	void excludeFromBilling(bool excludeFromBilling);

	void setKeyValue(string _key, string _value);

	int CheckInMode();

	int CheckInMode(List<string> ipAddresses);

	bool IsSynthesisVRStation();

	string DebugString();

	void SendAIOStartCountdown(string game_id, int seconds);

	bool AddToLeaderboard(string json_in);

	bool ResetBillingSession(string game_id);

	void Init(string game_id);

	void Init(string game_id, int drmmode);

	string WebsocketURL();

	int getSessionSecondsLeft();

	void setSynthesisOptions(string options);
}
