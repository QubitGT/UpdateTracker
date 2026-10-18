using System;
using Meta.WitAi.Json;
using UnityEngine;

namespace Meta.WitAi.Data.Info;

[Serializable]
public struct WitAppInfo
{
	[Header("App Info")]
	[SerializeField]
	public string name;

	[SerializeField]
	public string id;

	[SerializeField]
	public string lang;

	[SerializeField]
	[JsonProperty("private")]
	public bool isPrivate;

	[SerializeField]
	[JsonProperty("created_at")]
	public string createdAt;

	[Header("Training Info")]
	[JsonProperty("training_status")]
	public WitAppTrainingStatus trainingStatus;

	[JsonProperty("last_training_duration_secs")]
	public int lastTrainDuration;

	[JsonProperty("last_trained_at")]
	public string lastTrainedAt;

	[JsonProperty("will_train_at")]
	public string nextTrainAt;

	[Header("NLU Info")]
	public WitIntentInfo[] intents;

	public WitEntityInfo[] entities;

	public WitTraitInfo[] traits;

	public WitVersionTagInfo[] versionTags;

	[Header("TTS Info")]
	public WitVoiceInfo[] voices;
}
