namespace Fusion.Photon.Realtime;

internal class WebFlags
{
	public static readonly WebFlags Default = new WebFlags(0);

	public byte WebhookFlags;

	public const byte HttpForwardConst = 1;

	public const byte SendAuthCookieConst = 2;

	public const byte SendSyncConst = 4;

	public const byte SendStateConst = 8;

	public bool HttpForward
	{
		get
		{
			return (WebhookFlags & 1) != 0;
		}
		set
		{
			if (value)
			{
				WebhookFlags |= 1;
			}
			else
			{
				WebhookFlags = (byte)(WebhookFlags & 0xFFFFFFFEu);
			}
		}
	}

	public bool SendAuthCookie
	{
		get
		{
			return (WebhookFlags & 2) != 0;
		}
		set
		{
			if (value)
			{
				WebhookFlags |= 2;
			}
			else
			{
				WebhookFlags = (byte)(WebhookFlags & 0xFFFFFFFDu);
			}
		}
	}

	public bool SendSync
	{
		get
		{
			return (WebhookFlags & 4) != 0;
		}
		set
		{
			if (value)
			{
				WebhookFlags |= 4;
			}
			else
			{
				WebhookFlags = (byte)(WebhookFlags & 0xFFFFFFFBu);
			}
		}
	}

	public bool SendState
	{
		get
		{
			return (WebhookFlags & 8) != 0;
		}
		set
		{
			if (value)
			{
				WebhookFlags |= 8;
			}
			else
			{
				WebhookFlags = (byte)(WebhookFlags & 0xFFFFFFF7u);
			}
		}
	}

	public WebFlags(byte webhookFlags)
	{
		WebhookFlags = webhookFlags;
	}
}
