namespace Liv.Lck;

internal interface ILckActiveCameraConfigurer
{
	LckResult<ILckCamera> GetActiveCamera();

	LckResult ActivateCameraById(string cameraId, string monitorId = null);

	LckResult StopActiveCamera();
}
