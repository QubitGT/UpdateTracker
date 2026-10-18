using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oculus.Interaction;

public class UniqueIdentifier
{
	private class Decorator : ValueToClassDecorator<int, object>
	{
		private Decorator()
		{
		}

		public static Decorator GetFromContext(Context context)
		{
			return context.GetOrCreateSingleton(() => new Decorator());
		}
	}

	private Context _context;

	private static Random Random = new Random();

	private static HashSet<int> _identifierSet = new HashSet<int>();

	public int ID { get; private set; }

	private UniqueIdentifier(int identifier, Context context)
	{
		ID = identifier;
		_context = context;
	}

	[Obsolete]
	public static UniqueIdentifier Generate()
	{
		int num;
		do
		{
			num = Random.Next(int.MaxValue);
		}
		while (_identifierSet.Contains(num));
		_identifierSet.Add(num);
		return new UniqueIdentifier(num, Context.Global.GetInstance());
	}

	public static UniqueIdentifier Generate(Context context, object instance)
	{
		int num;
		do
		{
			num = Random.Next(int.MaxValue);
		}
		while (_identifierSet.Contains(num));
		_identifierSet.Add(num);
		Decorator.GetFromContext(context).AddDecoration(num, instance);
		return new UniqueIdentifier(num, context);
	}

	public static void Release(UniqueIdentifier identifier)
	{
		_identifierSet.Remove(identifier.ID);
		Decorator.GetFromContext(identifier._context).RemoveDecoration(identifier.ID);
	}

	public static bool TryGetInstanceFromIdentifier(Context context, int identifier, out object instance)
	{
		return Decorator.GetFromContext(context).TryGetDecoration(identifier, out instance);
	}

	public static Task<object> GetInstanceFromIdentifierAsync(Context context, int identifier)
	{
		return Decorator.GetFromContext(context).GetDecorationAsync(identifier);
	}
}
