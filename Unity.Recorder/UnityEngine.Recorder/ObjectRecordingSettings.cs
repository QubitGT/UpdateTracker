namespace UnityEngine.Recorder;

[AddComponentMenu("Recording/Object Recording Settings")]
public class ObjectRecordingSettings : MonoBehaviour
{
	[Tooltip("Record localPosition curves for this object.")]
	public bool recordPosition = true;

	[Tooltip("Record localRotation curves for this object.")]
	public bool recordRotation = true;

	[Tooltip("Record localScale curves for this object.")]
	public bool recordScale = true;
}
