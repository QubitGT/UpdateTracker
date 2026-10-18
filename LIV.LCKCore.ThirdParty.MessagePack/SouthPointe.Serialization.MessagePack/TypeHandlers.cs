using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SouthPointe.Serialization.MessagePack;

public class TypeHandlers
{
	private readonly SerializationContext context;

	private readonly Dictionary<Type, ITypeHandler> handlers;

	private readonly Dictionary<sbyte, IExtTypeHandler> extHandlers;

	private readonly Dictionary<Type, MapDefinition> mapDefinitions;

	public TypeHandlers(SerializationContext context)
	{
		this.context = context;
		handlers = new Dictionary<Type, ITypeHandler>
		{
			{
				typeof(bool),
				new BoolHandler()
			},
			{
				typeof(sbyte),
				new SByteHandler()
			},
			{
				typeof(byte),
				new ByteHandler()
			},
			{
				typeof(short),
				new ShortHandler()
			},
			{
				typeof(ushort),
				new UShortHandler()
			},
			{
				typeof(int),
				new IntHandler()
			},
			{
				typeof(uint),
				new UIntHandler()
			},
			{
				typeof(long),
				new LongHandler()
			},
			{
				typeof(ulong),
				new ULongHandler()
			},
			{
				typeof(float),
				new FloatHandler()
			},
			{
				typeof(double),
				new DoubleHandler()
			},
			{
				typeof(string),
				new StringHandler()
			},
			{
				typeof(byte[]),
				new ByteArrayHandler()
			},
			{
				typeof(char),
				new CharHandler()
			},
			{
				typeof(decimal),
				new DecimalHandler(context)
			},
			{
				typeof(object),
				new ObjectHandler(context)
			},
			{
				typeof(DateTime),
				new DateTimeHandler(context)
			},
			{
				typeof(Color),
				new ColorHandler(context)
			},
			{
				typeof(Color32),
				new Color32Handler(context)
			},
			{
				typeof(Guid),
				new GuidHandler(context)
			},
			{
				typeof(Quaternion),
				new QuaternionHandler(context)
			},
			{
				typeof(TimeSpan),
				new TimeSpanHandler(context)
			},
			{
				typeof(Uri),
				new UriHandler(context)
			},
			{
				typeof(Vector2),
				new Vector2Handler(context)
			},
			{
				typeof(Vector3),
				new Vector3Handler(context)
			},
			{
				typeof(Vector4),
				new Vector4Handler(context)
			},
			{
				typeof(Vector2Int),
				new Vector2IntHandler(context)
			},
			{
				typeof(Vector3Int),
				new Vector3IntHandler(context)
			}
		};
		extHandlers = new Dictionary<sbyte, IExtTypeHandler> { 
		{
			-1,
			new DateTimeHandler(context)
		} };
		mapDefinitions = new Dictionary<Type, MapDefinition>();
	}

	public ITypeHandler Get<T>()
	{
		return Get(typeof(T));
	}

	public ITypeHandler Get(Type type)
	{
		lock (handlers)
		{
			AddIfNotExist(type);
			return handlers[type];
		}
	}

	public IExtTypeHandler GetExt(sbyte extType)
	{
		lock (handlers)
		{
			return extHandlers[extType];
		}
	}

	public void SetHandler(Type type, ITypeHandler handler)
	{
		lock (handlers)
		{
			handlers[type] = handler;
		}
		if (handler is IExtTypeHandler)
		{
			IExtTypeHandler extTypeHandler = (IExtTypeHandler)handler;
			lock (extHandlers)
			{
				extHandlers[extTypeHandler.ExtType] = extTypeHandler;
			}
		}
	}

	private void AddIfNotExist(Type type)
	{
		if (handlers.ContainsKey(type))
		{
			return;
		}
		if (type.IsEnum)
		{
			AddIfNotExist(type, new DynamicEnumHandler(context, type));
			return;
		}
		if (type.IsNullable())
		{
			AddIfNotExist(type, new DynamicNullableHandler(context, type));
			return;
		}
		if (type.IsArray)
		{
			AddIfNotExist(type, new DynamicArrayHandler(context, type));
			return;
		}
		if (typeof(IList).IsAssignableFrom(type))
		{
			AddIfNotExist(type, new DynamicListHandler(context, type));
			return;
		}
		if (typeof(IDictionary).IsAssignableFrom(type))
		{
			AddIfNotExist(type, new DynamicDictionaryHandler(context, type));
			return;
		}
		if (type.IsClass || type.IsValueType)
		{
			AddIfNotExist(type, new DynamicMapHandler(context, GetLazyMapDefinition(type)));
			return;
		}
		throw new FormatException("No TypeHandler found for type: " + type);
	}

	private void AddIfNotExist(Type type, ITypeHandler handler)
	{
		if (!handlers.ContainsKey(type))
		{
			handlers.Add(type, handler);
		}
	}

	private Lazy<MapDefinition> GetLazyMapDefinition(Type type)
	{
		return new Lazy<MapDefinition>(delegate
		{
			if (!mapDefinitions.ContainsKey(type))
			{
				mapDefinitions[type] = new MapDefinition(context, type);
			}
			return mapDefinitions[type];
		});
	}
}
