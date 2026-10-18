using System;

namespace Fusion.Photon.Realtime;

internal class RaiseEventOptions
{
	public static readonly RaiseEventOptions Default = new RaiseEventOptions();

	public EventCaching CachingOption;

	public byte InterestGroup;

	public int[] TargetActors;

	public ReceiverGroup Receivers;

	[Obsolete("Not used where SendOptions are a parameter too. Use SendOptions.Channel instead.")]
	public byte SequenceChannel;

	public WebFlags Flags = WebFlags.Default;
}
