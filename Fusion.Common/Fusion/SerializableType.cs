#define DEBUG
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Text.RegularExpressions;

namespace Fusion;

[Serializable]
public struct SerializableType : IEquatable<SerializableType>
{
	private static class Cache
	{
		public static Dictionary<string, object> Types = new Dictionary<string, object>();
	}

	public string AssemblyQualifiedName;

	private static readonly Regex s_shortNameRegex = new Regex(", (Version|Culture|PublicKeyToken)=[^, \\]]+", RegexOptions.Compiled);

	public bool IsValid => !string.IsNullOrEmpty(AssemblyQualifiedName);

	public Type Value
	{
		get
		{
			if (string.IsNullOrEmpty(AssemblyQualifiedName))
			{
				return null;
			}
			object value;
			lock (Cache.Types)
			{
				if (!Cache.Types.TryGetValue(AssemblyQualifiedName, out value))
				{
					try
					{
						value = Type.GetType(AssemblyQualifiedName, throwOnError: true);
					}
					catch (Exception source)
					{
						value = ExceptionDispatchInfo.Capture(source);
					}
					Cache.Types.Add(AssemblyQualifiedName, value);
				}
			}
			if (value is ExceptionDispatchInfo exceptionDispatchInfo)
			{
				exceptionDispatchInfo.Throw();
			}
			Type type = (Type)value;
			if (type == null)
			{
				throw new Exception("Type " + AssemblyQualifiedName + " not found");
			}
			return type;
		}
	}

	public SerializableType(Type type)
	{
		if (type == null)
		{
			AssemblyQualifiedName = string.Empty;
		}
		else
		{
			AssemblyQualifiedName = type.AssemblyQualifiedName;
		}
	}

	public SerializableType(string type)
	{
		if (string.IsNullOrEmpty(type))
		{
			AssemblyQualifiedName = string.Empty;
		}
		else
		{
			AssemblyQualifiedName = type;
		}
	}

	public SerializableType AsShort()
	{
		SerializableType result = default(SerializableType);
		result.AssemblyQualifiedName = GetShortAssemblyQualifiedName(AssemblyQualifiedName);
		return result;
	}

	public static implicit operator SerializableType(Type type)
	{
		return new SerializableType(type);
	}

	public static implicit operator Type(SerializableType serializableType)
	{
		return serializableType.Value;
	}

	public bool Equals(SerializableType other)
	{
		return AssemblyQualifiedName == other.AssemblyQualifiedName;
	}

	public override bool Equals(object obj)
	{
		return obj is SerializableType other && Equals(other);
	}

	public override int GetHashCode()
	{
		return (AssemblyQualifiedName != null) ? AssemblyQualifiedName.GetHashCode() : 0;
	}

	public static string GetShortAssemblyQualifiedName(Type type)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		string assemblyQualifiedName = type.AssemblyQualifiedName;
		if (assemblyQualifiedName == null)
		{
			throw new InvalidOperationException($"Type {type} has no AssemblyQualifiedName");
		}
		return GetShortAssemblyQualifiedName(assemblyQualifiedName);
	}

	internal static string GetShortAssemblyQualifiedName(string assemblyQualifiedName)
	{
		return s_shortNameRegex.Replace(assemblyQualifiedName, string.Empty);
	}
}
[Serializable]
public struct SerializableType<BaseType> : IEquatable<SerializableType<BaseType>>
{
	public string AssemblyQualifiedName;

	public bool IsValid => !string.IsNullOrEmpty(AssemblyQualifiedName);

	public Type Value
	{
		get
		{
			SerializableType serializableType = default(SerializableType);
			serializableType.AssemblyQualifiedName = AssemblyQualifiedName;
			Type value = serializableType.Value;
			Assert.Check(value != null);
			if (!value.IsSubclassOf(typeof(BaseType)))
			{
				throw new Exception($"Type mismatch: {value} must inherit from {typeof(BaseType)}");
			}
			return value;
		}
	}

	public SerializableType(Type type)
	{
		AssemblyQualifiedName = type.AssemblyQualifiedName;
	}

	public SerializableType<BaseType> AsShort()
	{
		SerializableType<BaseType> result = default(SerializableType<BaseType>);
		result.AssemblyQualifiedName = SerializableType.GetShortAssemblyQualifiedName(AssemblyQualifiedName);
		return result;
	}

	public static implicit operator SerializableType<BaseType>(Type type)
	{
		return new SerializableType<BaseType>(type);
	}

	public static implicit operator Type(SerializableType<BaseType> serializableType)
	{
		return serializableType.Value;
	}

	public bool Equals(SerializableType<BaseType> other)
	{
		return AssemblyQualifiedName == other.AssemblyQualifiedName;
	}

	public override bool Equals(object obj)
	{
		return obj is SerializableType<BaseType> other && Equals(other);
	}

	public override int GetHashCode()
	{
		return (AssemblyQualifiedName != null) ? AssemblyQualifiedName.GetHashCode() : 0;
	}
}
