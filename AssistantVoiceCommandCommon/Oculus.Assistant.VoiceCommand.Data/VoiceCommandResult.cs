using System.Collections.Generic;

namespace Oculus.Assistant.VoiceCommand.Data;

public class VoiceCommandResult
{
	public abstract class Builder
	{
		public abstract string ActionId { get; }

		public abstract byte[] CommandOutput { get; }

		public abstract string InteractionId { get; }

		public abstract string DebugPhraseMatched { get; }

		public abstract string Utterance { get; }

		public abstract Dictionary<string, string> SlotValues { get; }

		public virtual VoiceCommandResult Build()
		{
			return new VoiceCommandResult
			{
				actionId = ActionId,
				commandOutput = CommandOutput,
				interactionId = InteractionId,
				debugPhraseMatched = DebugPhraseMatched,
				utterance = Utterance,
				slotValues = SlotValues
			};
		}
	}

	private string actionId;

	private byte[] commandOutput;

	private string utterance;

	private string interactionId;

	private string debugPhraseMatched;

	private Dictionary<string, string> slotValues;

	public string ActionId => actionId;

	public byte[] CommandOutput => commandOutput;

	public string InteractionId => interactionId;

	public string DebugPhraseMatched => debugPhraseMatched;

	public string Utterance => utterance;

	public Dictionary<string, string> MatchedSlots => slotValues;

	public string this[string slotName]
	{
		get
		{
			if (!slotValues.TryGetValue(slotName, out var value))
			{
				return null;
			}
			return value;
		}
	}

	public override string ToString()
	{
		string text = "{\n  actionId = " + actionId + ",\n  commandOutput = " + ((commandOutput != null) ? (commandOutput.Length + " bytes") : "null") + ",\n  interactionId = " + interactionId + ",\n  utterance = " + utterance + ",\n  matchedSlots = [";
		foreach (KeyValuePair<string, string> slotValue in slotValues)
		{
			text = text + "\n    " + slotValue.Key + " = " + slotValue.Value + ",";
		}
		text += "\n  ]";
		return text + "\n}";
	}

	public bool TryGetSlot(string slotName, out string slotValue)
	{
		return slotValues.TryGetValue(slotName, out slotValue);
	}

	public bool HasSlot(string slotName)
	{
		return slotValues.ContainsKey(slotName);
	}
}
