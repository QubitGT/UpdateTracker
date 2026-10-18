using UnityEngine.Playables;

namespace UnityEngine.Animations.Rigging;

public abstract class AnimationJobBinder<TJob, TData> : IAnimationJobBinder where TJob : struct, IAnimationJob where TData : struct, IAnimationJobData
{
	public abstract TJob Create(Animator animator, ref TData data, Component component);

	public abstract void Destroy(TJob job);

	public virtual void Update(TJob job, ref TData data)
	{
	}

	IAnimationJob IAnimationJobBinder.Create(Animator animator, IAnimationJobData data, Component component)
	{
		TData data2 = (TData)data;
		return Create(animator, ref data2, component);
	}

	void IAnimationJobBinder.Destroy(IAnimationJob job)
	{
		Destroy((TJob)job);
	}

	void IAnimationJobBinder.Update(IAnimationJob job, IAnimationJobData data)
	{
		TData data2 = (TData)data;
		Update((TJob)job, ref data2);
	}

	AnimationScriptPlayable IAnimationJobBinder.CreatePlayable(PlayableGraph graph, IAnimationJob job)
	{
		return AnimationScriptPlayable.Create(graph, (TJob)job);
	}
}
