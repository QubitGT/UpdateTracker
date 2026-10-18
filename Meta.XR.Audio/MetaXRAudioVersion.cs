using System.Runtime.InteropServices;
using UnityEngine;

public class MetaXRAudioVersion : MonoBehaviour
{
	private void Awake()
	{
		int Major = 0;
		int Minor = 0;
		int Patch = 0;
		MetaXRAudio_GetVersion(ref Major, ref Minor, ref Patch);
		Debug.Log(string.Format($"MetaXRAudio Version: {Major}.{Minor}.{Patch}"));
	}

	[DllImport("MetaXRAudioUnity")]
	private static extern void MetaXRAudio_GetVersion(ref int Major, ref int Minor, ref int Patch);
}
