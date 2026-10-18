using System.Text;

namespace OVR.OpenVR;

public struct VREvent_Keyboard_t
{
	public byte cNewInput0;

	public byte cNewInput1;

	public byte cNewInput2;

	public byte cNewInput3;

	public byte cNewInput4;

	public byte cNewInput5;

	public byte cNewInput6;

	public byte cNewInput7;

	public ulong uUserValue;

	public string cNewInput
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder(8);
			stringBuilder.Append(cNewInput0);
			stringBuilder.Append(cNewInput1);
			stringBuilder.Append(cNewInput2);
			stringBuilder.Append(cNewInput3);
			stringBuilder.Append(cNewInput4);
			stringBuilder.Append(cNewInput5);
			stringBuilder.Append(cNewInput6);
			stringBuilder.Append(cNewInput7);
			return stringBuilder.ToString();
		}
	}
}
