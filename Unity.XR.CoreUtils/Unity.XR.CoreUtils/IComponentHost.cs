namespace Unity.XR.CoreUtils;

public interface IComponentHost<THostType> where THostType : class
{
	THostType[] HostedComponents { get; }
}
