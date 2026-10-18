namespace Fusion;

internal interface INetworkString
{
	bool Equals<TOtherSize>(ref NetworkString<TOtherSize> other) where TOtherSize : unmanaged, IFixedStorage;
}
