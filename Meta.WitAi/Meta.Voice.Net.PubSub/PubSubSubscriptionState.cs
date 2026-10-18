namespace Meta.Voice.Net.PubSub;

public enum PubSubSubscriptionState
{
	NotSubscribed,
	Subscribing,
	Subscribed,
	Unsubscribing,
	SubscribeError,
	UnsubscribeError
}
