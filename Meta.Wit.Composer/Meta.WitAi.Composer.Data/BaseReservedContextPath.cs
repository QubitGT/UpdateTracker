namespace Meta.WitAi.Composer.Data;

public abstract class BaseReservedContextPath : IContextMapReservedPathExtension
{
	private ComposerService _composer;

	public bool HasComposer;

	protected ComposerContextMap Map => _composer.CurrentContextMap;

	protected abstract string ReservedPath { get; }

	protected internal abstract void UpdateContextMap();

	public virtual void AssignTo(ComposerService composer)
	{
		if (!(_composer == composer))
		{
			_composer = composer;
			ComposerContextMap.ReservedPaths.Add(ReservedPath);
			HasComposer = true;
		}
	}

	public virtual void Clear()
	{
		Map?.ClearData(ReservedPath);
	}
}
