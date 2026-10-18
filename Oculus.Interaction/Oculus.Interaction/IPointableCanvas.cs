using UnityEngine;

namespace Oculus.Interaction;

public interface IPointableCanvas : IPointableElement, IPointable
{
	Canvas Canvas { get; }
}
