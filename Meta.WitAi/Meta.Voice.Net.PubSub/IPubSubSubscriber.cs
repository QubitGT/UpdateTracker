namespace Meta.Voice.Net.PubSub;

public interface IPubSubSubscriber
{
	event PubSubTopicSubscriptionDelegate OnTopicSubscriptionStateChange;

	PubSubSubscriptionState GetTopicSubscriptionState(string topicId);

	void Subscribe(string topicId);

	void Unsubscribe(string topicId);
}
