using System;
using UnityEngine.Events;

namespace Meta.Voice.Net.PubSub;

public interface IPubSubAdapter
{
	PubSubSettings Settings { get; set; }

	PubSubSubscriptionState SubscriptionState { get; }

	UnityEvent OnSubscribed { get; }

	UnityEvent OnUnsubscribed { get; }

	event Action<PubSubSubscriptionState> OnTopicSubscriptionStateChange;
}
