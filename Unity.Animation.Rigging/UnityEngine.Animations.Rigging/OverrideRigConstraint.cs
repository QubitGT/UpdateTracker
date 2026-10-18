namespace UnityEngine.Animations.Rigging;

public class OverrideRigConstraint<TConstraint, TJob, TData, TBinder> : IRigConstraint where TConstraint : MonoBehaviour, IRigConstraint where TJob : struct, IWeightedAnimationJob where TData : struct, IAnimationJobData where TBinder : AnimationJobBinder<TJob, TData>, new()
{
	[SerializeField]
	protected TConstraint m_Constraint;

	private static readonly TBinder s_Binder = new TBinder();

	IAnimationJobBinder IRigConstraint.binder => s_Binder;

	IAnimationJobData IRigConstraint.data => m_Constraint.data;

	Component IRigConstraint.component => m_Constraint.component;

	public float weight
	{
		get
		{
			return m_Constraint.weight;
		}
		set
		{
			m_Constraint.weight = value;
		}
	}

	public OverrideRigConstraint(TConstraint baseConstraint)
	{
		m_Constraint = baseConstraint;
	}

	public IAnimationJob CreateJob(Animator animator)
	{
		TJob val = (TJob)((IAnimationJobBinder)s_Binder).Create(animator, m_Constraint.data, (Component)m_Constraint);
		val.jobWeight = FloatProperty.BindCustom(animator, ConstraintsUtils.ConstructCustomPropertyName(m_Constraint, ConstraintProperties.s_Weight));
		return val;
	}

	public void DestroyJob(IAnimationJob job)
	{
		s_Binder.Destroy((TJob)job);
	}

	public void UpdateJob(IAnimationJob job)
	{
		((IAnimationJobBinder)s_Binder).Update(job, m_Constraint.data);
	}

	public bool IsValid()
	{
		return m_Constraint.IsValid();
	}
}
