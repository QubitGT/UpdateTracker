namespace Unity.Multiplayer.Center.Common;

public interface ISectionDependingOnUserChoices : IOnboardingSection
{
	void HandleAnswerData(AnswerData answerData)
	{
	}

	void HandleUserSelectionData(SelectedSolutionsData selectedSolutionsData)
	{
	}

	void HandlePreset(Preset preset)
	{
	}
}
