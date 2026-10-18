namespace Meta.Voice.Hub.Interfaces;

public interface IPageInfo
{
	string Name { get; }

	string Context { get; }

	int Priority { get; }

	string Prefix { get; }
}
