using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Meta.WitAi.TTS.Interfaces;
using UnityEngine;
using UnityEngine.Serialization;

namespace Meta.WitAi.TTS.Utilities;

public class TTSSpeechSplitter : MonoBehaviour, ISpeakerTextPreprocessor
{
	[Tooltip("If text-to-speech phrase is greater than this length, it will be split.")]
	[Range(10f, 250f)]
	[FormerlySerializedAs("maxTextLength")]
	public int MaxTextLength = 250;

	private Regex _cleaner = new Regex("\\s\\s+|</?s>|</?p>", RegexOptions.Multiline | RegexOptions.Compiled);

	private Regex _sentenceSplitter = new Regex("(?<=[.?,;!]\\s+|<p>|<s>)", RegexOptions.Compiled);

	private Regex _wordSplitter = new Regex("(?=\\s+)", RegexOptions.Compiled);

	public void OnPreprocessTTS(TTSSpeaker speaker, List<string> phrases)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		while (num < phrases.Count)
		{
			string text = _cleaner.Replace(phrases[num], " ");
			if (text.Length <= MaxTextLength)
			{
				phrases[num] = text;
				num++;
				continue;
			}
			phrases.RemoveAt(num);
			string[] array = _sentenceSplitter.Split(text);
			foreach (string text2 in array)
			{
				if (text2.Length == 0)
				{
					continue;
				}
				if (stringBuilder.Length > 0 && stringBuilder.Length + text2.Length > MaxTextLength)
				{
					phrases.Insert(num, stringBuilder.ToString().Trim());
					stringBuilder.Clear();
					num++;
				}
				if (text2.Length <= MaxTextLength)
				{
					stringBuilder.Append(text2);
					continue;
				}
				string[] array2 = _wordSplitter.Split(text2);
				for (int j = 0; j < array2.Length; j++)
				{
					string text3 = array2[j];
					if (text3.Length != 0)
					{
						if (stringBuilder.Length > 0 && stringBuilder.Length + text3.Length > MaxTextLength)
						{
							phrases.Insert(num, stringBuilder.ToString().Trim());
							stringBuilder.Clear();
							num++;
						}
						if (stringBuilder.Length == 0)
						{
							text3 = text3.TrimStart();
						}
						if (text3.Length <= MaxTextLength)
						{
							stringBuilder.Append(text3);
							continue;
						}
						stringBuilder.Append(text3.Substring(0, MaxTextLength));
						VLog.W($"Word is longer than MaxTextLength & will be truncated\nWord: {text3}\nTruncated: {stringBuilder}\nFrom Length: {text3.Length}\nTo Length: {MaxTextLength}");
						phrases.Insert(num, stringBuilder.ToString());
						stringBuilder.Clear();
						num++;
					}
				}
			}
			if (stringBuilder.Length > 0)
			{
				phrases.Insert(num, stringBuilder.ToString().Trim());
				stringBuilder.Clear();
				num++;
			}
		}
	}
}
