using System.Threading.Tasks;

namespace Oculus.Interaction;

public abstract class ClassToValueDecorator<InstanceT, DecorationT> where InstanceT : class where DecorationT : struct
{
	private class Wrapper
	{
		public DecorationT _decoration;
	}

	private class InternalDecorator : ClassToClassDecorator<InstanceT, Wrapper>
	{
	}

	private readonly InternalDecorator _decorator;

	protected ClassToValueDecorator()
	{
		_decorator = new InternalDecorator();
	}

	public void AddDecoration(InstanceT instance, DecorationT decoration)
	{
		_decorator.AddDecoration(instance, new Wrapper
		{
			_decoration = decoration
		});
	}

	public void RemoveDecoration(InstanceT instance)
	{
		_decorator.RemoveDecoration(instance);
	}

	public bool TryGetDecoration(InstanceT instance, out DecorationT decoration)
	{
		if (_decorator.TryGetDecoration(instance, out var decoration2))
		{
			decoration = decoration2._decoration;
			return true;
		}
		decoration = default(DecorationT);
		return false;
	}

	public Task<DecorationT> GetDecorationAsync(InstanceT instance)
	{
		return _decorator.GetDecorationAsync(instance).ContinueWith((Task<Wrapper> wrapper) => wrapper.Result._decoration, TaskContinuationOptions.OnlyOnRanToCompletion | TaskContinuationOptions.ExecuteSynchronously);
	}
}
