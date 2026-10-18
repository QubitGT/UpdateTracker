using UnityEngine.Playables;

namespace UnityEngine.Animations.Rigging;

public interface IAnimationJobBinder
{
	IAnimationJob Create(Animator animator, IAnimationJobData data, Component component = null);

	void Destroy(IAnimationJob job);

	void Update(IAnimationJob job, IAnimationJobData data);

	AnimationScriptPlayable CreatePlayable(PlayableGraph graph, IAnimationJob job);
}
