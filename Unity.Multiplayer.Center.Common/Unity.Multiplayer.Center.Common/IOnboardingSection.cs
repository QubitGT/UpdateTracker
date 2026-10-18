using UnityEngine.UIElements;

namespace Unity.Multiplayer.Center.Common;

public interface IOnboardingSection
{
	VisualElement Root { get; }

	void Load();

	void Unload();
}
