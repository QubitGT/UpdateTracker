using ExitGames.Client.Photon;
using Photon.Realtime;

namespace Photon.Pun.UtilityScripts;

public static class ScoreExtensions
{
	public static void SetScore(this Player player, int newScore)
	{
		Hashtable hashtable = new Hashtable();
		hashtable["score"] = newScore;
		player.SetCustomProperties(hashtable);
	}

	public static void AddScore(this Player player, int scoreToAddToCurrent)
	{
		int score = player.GetScore();
		score += scoreToAddToCurrent;
		Hashtable hashtable = new Hashtable();
		hashtable["score"] = score;
		player.SetCustomProperties(hashtable);
	}

	public static int GetScore(this Player player)
	{
		if (player.CustomProperties.TryGetValue("score", out var value))
		{
			return (int)value;
		}
		return 0;
	}
}
