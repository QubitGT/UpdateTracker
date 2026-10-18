using System;

namespace Fusion.Protocol;

internal abstract class Message : IMessage
{
	private const int CustomDataLength = 1024;

	public ProtocolMessageVersion ProtocolVersion;

	public Version FusionSerializationVersion;

	private string _customData = string.Empty;

	public virtual bool IsValid => HasValidVersion;

	internal bool HasValidVersion => ProtocolVersion != 0 && FusionSerializationVersion != Versioning.InvalidVersion;

	public string CustomData
	{
		get
		{
			return _customData;
		}
		set
		{
			Assert.Always(value.Length <= 1024, $"Protocol Message Custom Data size is greater than {1024}");
			_customData = value.Substring(0, Math.Min(value.Length, 1024));
		}
	}

	public virtual Message Clone()
	{
		return (Message)MemberwiseClone();
	}

	public Message(ProtocolMessageVersion protocolMessage = ProtocolMessageVersion.V1_6_0, Version serializationVersion = null)
	{
		ProtocolVersion = protocolMessage;
		FusionSerializationVersion = serializationVersion ?? Versioning.GetCurrentVersion;
	}

	public void Serialize(BitStream stream)
	{
		byte value = (byte)ProtocolVersion;
		int value2 = FusionSerializationVersion.Major;
		int value3 = FusionSerializationVersion.Minor;
		int value4 = FusionSerializationVersion.Build;
		stream.Serialize(ref value);
		value = Math.Min((byte)10, value);
		if (value >= 2)
		{
			stream.Serialize(ref value2);
			stream.Serialize(ref value3);
			stream.Serialize(ref value4);
		}
		else
		{
			value2 = 0;
			value3 = 0;
			value4 = 0;
		}
		ProtocolVersion = (ProtocolMessageVersion)value;
		FusionSerializationVersion = new Version(value2, value3, value4);
		if (FusionSerializationVersion.ShortVersion() == Versioning.GetCurrentVersion.ShortVersion())
		{
			SerializeProtected(stream);
			if (value >= 3)
			{
				stream.Serialize(ref _customData);
			}
			else
			{
				_customData = string.Empty;
			}
		}
	}

	protected virtual void SerializeProtected(BitStream stream)
	{
	}

	public bool CheckCompatibility(ProtocolMessageVersion pluginProtocolVersion, Version pluginVersion, Version sessionSerializationVersion)
	{
		if (ProtocolVersion == ProtocolMessageVersion.Invalid || pluginProtocolVersion == ProtocolMessageVersion.Invalid || ProtocolVersion != pluginProtocolVersion)
		{
			return false;
		}
		if ((int)pluginProtocolVersion < 2)
		{
			return true;
		}
		if (FusionSerializationVersion == Versioning.InvalidVersion || sessionSerializationVersion == Versioning.InvalidVersion || FusionSerializationVersion.ShortVersion() > pluginVersion.ShortVersion() || FusionSerializationVersion.ShortVersion() < new Version(1, 0))
		{
			return false;
		}
		return FusionSerializationVersion == sessionSerializationVersion;
	}

	public override string ToString()
	{
		return string.Format("{0}={1}, {2}={3}, {4}={5}", "ProtocolVersion", ProtocolVersion, "FusionSerializationVersion", FusionSerializationVersion, "CustomData", CustomData);
	}
}
