using System.IO;

namespace Meta.WitAi.Json;

public class WitResponseData : WitResponseNode
{
	private string m_Data;

	public override string Value
	{
		get
		{
			return m_Data;
		}
		set
		{
			m_Data = value;
		}
	}

	public WitResponseData()
	{
		m_Data = "";
	}

	public WitResponseData(string aData)
	{
		m_Data = aData;
	}

	public WitResponseData(float aData)
	{
		AsFloat = aData;
	}

	public WitResponseData(double aData)
	{
		AsDouble = aData;
	}

	public WitResponseData(bool aData)
	{
		AsBool = aData;
	}

	public WitResponseData(int aData)
	{
		AsInt = aData;
	}

	public override string ToString()
	{
		return "\"" + WitResponseNode.Escape(m_Data) + "\"";
	}

	public override string ToString(string aPrefix)
	{
		return "\"" + WitResponseNode.Escape(m_Data) + "\"";
	}

	public override void Serialize(BinaryWriter aWriter)
	{
		WitResponseData witResponseData = new WitResponseData("")
		{
			AsInt = AsInt
		};
		if (witResponseData.m_Data == m_Data)
		{
			aWriter.Write((byte)4);
			aWriter.Write(AsInt);
			return;
		}
		witResponseData.AsFloat = AsFloat;
		if (witResponseData.m_Data == m_Data)
		{
			aWriter.Write((byte)7);
			aWriter.Write(AsFloat);
			return;
		}
		witResponseData.AsDouble = AsDouble;
		if (witResponseData.m_Data == m_Data)
		{
			aWriter.Write((byte)5);
			aWriter.Write(AsDouble);
			return;
		}
		witResponseData.AsBool = AsBool;
		if (witResponseData.m_Data == m_Data)
		{
			aWriter.Write((byte)6);
			aWriter.Write(AsBool);
		}
		else
		{
			aWriter.Write((byte)3);
			aWriter.Write(m_Data);
		}
	}
}
