using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

[AddComponentMenu("Gorilla Tag/Basketball Scoreboard")]
public class BasketballScoreboard : MonoBehaviour
{
	[SerializeField]
	private BasketballHoop hoop;

	[SerializeField]
	private TMP_Text text;

	[SerializeField]
	private int maxLines = 10;

	[SerializeField]
	private int maxNameLength = 12;

	[Tooltip("Horizontal position of the score column, as a percentage of the text width")]
	[SerializeField]
	private int scoreColumnPercent = 78;

	[TextArea]
	[SerializeField]
	private string emptyText = "Grab a ball from the bin\nand shoot some hoops!";

	private readonly List<BasketballHoop.ScoreEntry> _sorted = new List<BasketballHoop.ScoreEntry>();

	private readonly StringBuilder _sb = new StringBuilder(512);

	private void OnEnable()
	{
		if ((bool)hoop)
		{
			hoop.OnScoresChanged += UpdateDisplay;
			UpdateDisplay();
		}
	}

	private void OnDisable()
	{
		if ((bool)hoop)
		{
			hoop.OnScoresChanged -= UpdateDisplay;
		}
	}

	private void UpdateDisplay()
	{
		_sorted.Clear();
		foreach (BasketballHoop.ScoreEntry score in hoop.Scores)
		{
			if (Utils.PlayerInRoom(score.actorNumber))
			{
				_sorted.Add(score);
			}
		}
		if (_sorted.Count == 0)
		{
			this.text.text = "<align=center>" + emptyText + "</align>";
			return;
		}
		for (int i = 1; i < _sorted.Count; i++)
		{
			BasketballHoop.ScoreEntry value = _sorted[i];
			int num = i - 1;
			while (num >= 0 && _sorted[num].score < value.score)
			{
				_sorted[num + 1] = _sorted[num];
				num--;
			}
			_sorted[num + 1] = value;
		}
		_sb.Clear();
		_sb.Append("<align=left>PLAYER<pos=").Append(scoreColumnPercent).Append("%>PTS\n");
		int num2 = Mathf.Min(_sorted.Count, maxLines);
		for (int j = 0; j < num2; j++)
		{
			NetPlayer netPlayer = NetPlayer.Get(_sorted[j].actorNumber);
			string text = netPlayer?.SanitizedNickName;
			if (string.IsNullOrEmpty(text))
			{
				text = netPlayer?.DefaultName ?? "UNKNOWN";
			}
			if (text.Length > maxNameLength)
			{
				text = text.Substring(0, maxNameLength);
			}
			_sb.Append("<noparse>").Append(text).Append("</noparse><pos=")
				.Append(scoreColumnPercent)
				.Append("%>")
				.Append(_sorted[j].score)
				.Append('\n');
		}
		this.text.text = _sb.ToString();
	}
}
