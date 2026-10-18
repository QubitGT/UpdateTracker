using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Meta.WitAi.Json;

public class WitResponseNode
{
	public virtual WitResponseNode this[int aIndex]
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public virtual WitResponseNode this[string aKey]
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public virtual string Value
	{
		get
		{
			return "";
		}
		set
		{
		}
	}

	public virtual string[] ChildNodeNames => Array.Empty<string>();

	public virtual int Count => 0;

	public virtual IEnumerable<WitResponseNode> Childs
	{
		get
		{
			yield break;
		}
	}

	public IEnumerable<WitResponseNode> DeepChilds
	{
		get
		{
			foreach (WitResponseNode child in Childs)
			{
				foreach (WitResponseNode deepChild in child.DeepChilds)
				{
					yield return deepChild;
				}
			}
		}
	}

	public virtual int AsInt
	{
		get
		{
			if (int.TryParse(Value, out var result))
			{
				return result;
			}
			return 0;
		}
		set
		{
			Value = value.ToString();
		}
	}

	public virtual float AsFloat
	{
		get
		{
			if (float.TryParse(Value, out var result))
			{
				return result;
			}
			return 0f;
		}
		set
		{
			Value = value.ToString();
		}
	}

	public virtual double AsDouble
	{
		get
		{
			if (double.TryParse(Value, out var result))
			{
				return result;
			}
			return 0.0;
		}
		set
		{
			Value = value.ToString();
		}
	}

	public virtual bool AsBool
	{
		get
		{
			if (bool.TryParse(Value, out var result))
			{
				return result;
			}
			return !string.IsNullOrEmpty(Value);
		}
		set
		{
			Value = (value ? "true" : "false");
		}
	}

	public virtual WitResponseArray AsArray => this as WitResponseArray;

	public virtual string[] AsStringArray
	{
		get
		{
			string[] array = new string[0];
			WitResponseArray asArray = AsArray;
			if (null != asArray)
			{
				array = new string[asArray.Count];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = asArray[i].Value;
				}
			}
			return array;
		}
	}

	public virtual WitResponseClass AsObject => this as WitResponseClass;

	public virtual void Add(string aKey, WitResponseNode aItem)
	{
	}

	public virtual void Add(WitResponseNode aItem)
	{
		Add("", aItem);
	}

	public virtual WitResponseNode Remove(string aKey)
	{
		return null;
	}

	public virtual WitResponseNode Remove(int aIndex)
	{
		return null;
	}

	public virtual WitResponseNode Remove(WitResponseNode aNode)
	{
		return aNode;
	}

	public override string ToString()
	{
		return "JSONNode";
	}

	public virtual string ToString(string aPrefix)
	{
		return "JSONNode";
	}

	public virtual T Cast<T>(T defaultValue = default(T))
	{
		object obj = defaultValue;
		Type typeFromHandle = typeof(T);
		if (typeFromHandle == typeof(string))
		{
			obj = Value;
		}
		else if (typeFromHandle == typeof(int))
		{
			obj = AsInt;
		}
		else if (typeFromHandle == typeof(float))
		{
			obj = AsFloat;
		}
		else if (typeFromHandle == typeof(double))
		{
			obj = AsDouble;
		}
		else if (typeFromHandle == typeof(bool))
		{
			obj = AsBool;
		}
		else if (typeFromHandle == typeof(string[]))
		{
			obj = AsStringArray;
		}
		else if (typeFromHandle == typeof(WitResponseNode))
		{
			obj = this;
		}
		else if (typeFromHandle == typeof(WitResponseArray))
		{
			obj = AsArray;
		}
		else if (typeFromHandle == typeof(WitResponseClass))
		{
			obj = AsObject;
		}
		else if (typeFromHandle == typeof(WitResponseData))
		{
			obj = this as WitResponseData;
		}
		else
		{
			VLog.W("WitResponseNode", "Cast " + GetType().Name + " to " + typeFromHandle.Name + " not supported");
		}
		return (T)obj;
	}

	public static implicit operator WitResponseNode(string s)
	{
		return new WitResponseData(s);
	}

	public static implicit operator string(WitResponseNode d)
	{
		return d?.Value;
	}

	public static bool operator ==(WitResponseNode a, object b)
	{
		if (b == null && a is WitResponseLazyCreator)
		{
			return true;
		}
		return (object)a == b;
	}

	public static bool operator !=(WitResponseNode a, object b)
	{
		return !(a == b);
	}

	public override bool Equals(object obj)
	{
		if (obj == null || obj.GetType() != GetType())
		{
			return false;
		}
		if (obj is WitResponseNode newNode)
		{
			return Equals(this, newNode);
		}
		return (object)this == obj;
	}

	public static bool Equals(WitResponseNode oldNode, WitResponseNode newNode)
	{
		if (oldNode == null && newNode == null)
		{
			return true;
		}
		if (oldNode == null || newNode == null || oldNode.GetType() != newNode.GetType())
		{
			return false;
		}
		if (newNode is WitResponseData)
		{
			return string.Equals(oldNode.Value, newNode.Value);
		}
		if (newNode is WitResponseArray)
		{
			WitResponseArray asArray = oldNode.AsArray;
			WitResponseArray asArray2 = newNode.AsArray;
			if (asArray.Count != asArray2.Count)
			{
				return false;
			}
			for (int i = 0; i < asArray2.Count; i++)
			{
				if (!Equals(asArray[i], asArray2[i]))
				{
					return false;
				}
			}
		}
		else if (newNode is WitResponseClass)
		{
			WitResponseClass asObject = oldNode.AsObject;
			WitResponseClass asObject2 = newNode.AsObject;
			if (asObject.ChildNodeNames.Length != asObject2.ChildNodeNames.Length)
			{
				return false;
			}
			string[] childNodeNames = asObject2.ChildNodeNames;
			foreach (string aKey in childNodeNames)
			{
				if (!Equals(asObject[aKey], asObject2[aKey]))
				{
					return false;
				}
			}
		}
		return true;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	internal static string Escape(string aText)
	{
		if (string.IsNullOrEmpty(aText))
		{
			return aText;
		}
		string text = "";
		for (int i = 0; i < aText.Length; i++)
		{
			char c = aText[i];
			text = c switch
			{
				'\\' => text + "\\\\", 
				'"' => text + "\\\"", 
				'\n' => text + "\\n", 
				'\r' => text + "\\r", 
				'\t' => text + "\\t", 
				'\b' => text + "\\b", 
				'\f' => text + "\\f", 
				_ => text + c, 
			};
		}
		return text;
	}

	public static WitResponseNode Parse(string aJSON)
	{
		Stack<WitResponseNode> stack = new Stack<WitResponseNode>();
		WitResponseNode witResponseNode = null;
		int i = 0;
		string text = "";
		string text2 = "";
		bool flag = false;
		for (; i < aJSON.Length; i++)
		{
			switch (aJSON[i])
			{
			case '{':
				if (flag)
				{
					text += aJSON[i];
					break;
				}
				stack.Push(new WitResponseClass());
				if (witResponseNode != null)
				{
					text2 = text2.Trim();
					if (witResponseNode is WitResponseArray)
					{
						witResponseNode.Add(stack.Peek());
					}
					else if (text2 != "")
					{
						witResponseNode.Add(text2, stack.Peek());
					}
				}
				text2 = "";
				text = "";
				witResponseNode = stack.Peek();
				break;
			case '[':
				if (flag)
				{
					text += aJSON[i];
					break;
				}
				stack.Push(new WitResponseArray());
				if (witResponseNode != null)
				{
					text2 = text2.Trim();
					if (witResponseNode is WitResponseArray)
					{
						witResponseNode.Add(stack.Peek());
					}
					else if (text2 != "")
					{
						witResponseNode.Add(text2, stack.Peek());
					}
				}
				text2 = "";
				text = "";
				witResponseNode = stack.Peek();
				break;
			case ']':
			case '}':
				if (flag)
				{
					text += aJSON[i];
					break;
				}
				if (stack.Count == 0)
				{
					throw new JSONParseException("JSON Parse: Too many closing brackets");
				}
				stack.Pop();
				if (text != "")
				{
					text2 = text2.Trim();
					if (witResponseNode is WitResponseArray)
					{
						witResponseNode.Add(text);
					}
					else if (text2 != "")
					{
						witResponseNode.Add(text2, text);
					}
				}
				text2 = "";
				text = "";
				if (stack.Count > 0)
				{
					witResponseNode = stack.Peek();
				}
				break;
			case ':':
				if (flag)
				{
					text += aJSON[i];
					break;
				}
				text2 = text;
				text = "";
				break;
			case '"':
				flag = !flag;
				break;
			case ',':
				if (flag)
				{
					text += aJSON[i];
					break;
				}
				if (text != "")
				{
					if (witResponseNode is WitResponseArray)
					{
						witResponseNode.Add(text);
					}
					else if (text2 != "")
					{
						witResponseNode.Add(text2, text);
					}
				}
				text2 = "";
				text = "";
				break;
			case '\t':
			case ' ':
				if (flag)
				{
					text += aJSON[i];
				}
				break;
			case '\\':
				i++;
				if (flag)
				{
					char c = aJSON[i];
					switch (c)
					{
					case 't':
						text += "\t";
						break;
					case 'r':
						text += "\r";
						break;
					case 'n':
						text += "\n";
						break;
					case 'b':
						text += "\b";
						break;
					case 'f':
						text += "\f";
						break;
					case 'u':
					{
						string s = aJSON.Substring(i + 1, 4);
						text += (char)int.Parse(s, NumberStyles.AllowHexSpecifier);
						i += 4;
						break;
					}
					default:
						text += c;
						break;
					}
				}
				break;
			default:
				text += aJSON[i];
				break;
			case '\n':
			case '\r':
				break;
			}
		}
		if (flag)
		{
			throw new JSONParseException("JSON Parse: Quotation marks seems to be messed up.");
		}
		return witResponseNode;
	}

	public virtual void Serialize(BinaryWriter aWriter)
	{
	}

	public void SaveToStream(Stream aData)
	{
		BinaryWriter aWriter = new BinaryWriter(aData);
		Serialize(aWriter);
	}

	public void SaveToCompressedStream(Stream aData)
	{
		throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
	}

	public void SaveToCompressedFile(string aFileName)
	{
		throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
	}

	public string SaveToCompressedBase64()
	{
		throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
	}

	public void SaveToFile(string aFileName)
	{
		Directory.CreateDirectory(new FileInfo(aFileName).Directory.FullName);
		using FileStream aData = File.OpenWrite(aFileName);
		SaveToStream(aData);
	}

	public string SaveToBase64()
	{
		using MemoryStream memoryStream = new MemoryStream();
		SaveToStream(memoryStream);
		memoryStream.Position = 0L;
		return Convert.ToBase64String(memoryStream.ToArray());
	}

	public static WitResponseNode Deserialize(BinaryReader aReader)
	{
		JSONBinaryTag jSONBinaryTag = (JSONBinaryTag)aReader.ReadByte();
		switch (jSONBinaryTag)
		{
		case JSONBinaryTag.Array:
		{
			int num2 = aReader.ReadInt32();
			WitResponseArray witResponseArray = new WitResponseArray();
			for (int j = 0; j < num2; j++)
			{
				witResponseArray.Add(Deserialize(aReader));
			}
			return witResponseArray;
		}
		case JSONBinaryTag.Class:
		{
			int num = aReader.ReadInt32();
			WitResponseClass witResponseClass = new WitResponseClass();
			for (int i = 0; i < num; i++)
			{
				string aKey = aReader.ReadString();
				WitResponseNode aItem = Deserialize(aReader);
				witResponseClass.Add(aKey, aItem);
			}
			return witResponseClass;
		}
		case JSONBinaryTag.Value:
			return new WitResponseData(aReader.ReadString());
		case JSONBinaryTag.IntValue:
			return new WitResponseData(aReader.ReadInt32());
		case JSONBinaryTag.DoubleValue:
			return new WitResponseData(aReader.ReadDouble());
		case JSONBinaryTag.BoolValue:
			return new WitResponseData(aReader.ReadBoolean());
		case JSONBinaryTag.FloatValue:
			return new WitResponseData(aReader.ReadSingle());
		default:
			throw new JSONParseException("Error deserializing JSON. Unknown tag: " + jSONBinaryTag);
		}
	}

	public static WitResponseNode LoadFromCompressedFile(string aFileName)
	{
		throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
	}

	public static WitResponseNode LoadFromCompressedStream(Stream aData)
	{
		throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
	}

	public static WitResponseNode LoadFromCompressedBase64(string aBase64)
	{
		throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
	}

	public static WitResponseNode LoadFromStream(Stream aData)
	{
		using BinaryReader aReader = new BinaryReader(aData);
		return Deserialize(aReader);
	}

	public static WitResponseNode LoadFromFile(string aFileName)
	{
		using FileStream aData = File.OpenRead(aFileName);
		return LoadFromStream(aData);
	}

	public static WitResponseNode LoadFromBase64(string aBase64)
	{
		return LoadFromStream(new MemoryStream(Convert.FromBase64String(aBase64))
		{
			Position = 0L
		});
	}
}
