using System;

namespace Unity.Cinemachine;

public interface IInputAxisResetSource
{
	bool HasResetHandler { get; }

	void RegisterResetHandler(Action handler);

	void UnregisterResetHandler(Action handler);
}
