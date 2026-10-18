namespace Fusion.Sockets.Stun;

internal class StunErrorAttribute
{
	public int Code { get; set; } = 0;


	public string ReasonText { get; set; } = "";


	public StunErrorAttribute(int code, string reasonText)
	{
		Code = code;
		ReasonText = reasonText;
	}
}
