namespace Meta.Conduit;

internal class ConduitDispatcherFactory
{
	private static IConduitDispatcher Instance;

	private readonly IInstanceResolver _instanceResolver;

	private readonly IParameterProvider _parameterProvider;

	public ConduitDispatcherFactory(IInstanceResolver instanceResolver)
	{
		_instanceResolver = instanceResolver;
	}

	public IConduitDispatcher GetDispatcher()
	{
		IConduitDispatcher obj = Instance ?? new ConduitDispatcher(new ManifestLoader(), _instanceResolver);
		Instance = obj;
		return obj;
	}
}
