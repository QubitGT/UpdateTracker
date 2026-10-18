namespace Oculus.Platform.Models;

public class PingResult
{
	private ulong? pingTimeUsec;

	public ulong ID { get; private set; }

	public ulong PingTimeUsec
	{
		get
		{
			if (!pingTimeUsec.HasValue)
			{
				return 0uL;
			}
			return pingTimeUsec.Value;
		}
	}

	public bool IsTimeout => !pingTimeUsec.HasValue;

	public PingResult(ulong id, ulong? pingTimeUsec)
	{
		ID = id;
		this.pingTimeUsec = pingTimeUsec;
	}
}
