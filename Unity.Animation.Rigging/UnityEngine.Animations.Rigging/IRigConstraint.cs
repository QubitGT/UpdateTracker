namespace UnityEngine.Animations.Rigging;

public interface IRigConstraint
{
	IAnimationJobData data { get; }

	IAnimationJobBinder binder { get; }

	Component component { get; }

	float weight { get; set; }

	bool IsValid();

	IAnimationJob CreateJob(Animator animator);

	void UpdateJob(IAnimationJob job);

	void DestroyJob(IAnimationJob job);
}
