using System.Collections.ObjectModel;

namespace g3;

public interface IMultiCurve2d
{
	ReadOnlyCollection<IParametricCurve2d> Curves { get; }
}
