namespace Unity.Cinemachine;

public interface ICinemachineMixer : ICinemachineCamera
{
	bool IsLiveChild(ICinemachineCamera child, bool dominantChildOnly = false);
}
