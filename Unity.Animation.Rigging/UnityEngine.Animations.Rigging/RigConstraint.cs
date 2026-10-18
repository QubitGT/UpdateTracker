namespace UnityEngine.Animations.Rigging;

public class RigConstraint<TJob, TData, TBinder> : MonoBehaviour, IRigConstraint where TJob : struct, IWeightedAnimationJob where TData : struct, IAnimationJobData where TBinder : AnimationJobBinder<TJob, TData>, new()
{
	[SerializeField]
	[Range(0f, 1f)]
	protected float m_Weight = 1f;

	[SerializeField]
	[ExpandChildren]
	protected TData m_Data;

	private static readonly TBinder s_Binder = new TBinder();

	public ref TData data => ref m_Data;

	public float weight
	{
		get
		{
			return m_Weight;
		}
		set
		{
			m_Weight = Mathf.Clamp01(value);
		}
	}

	IAnimationJobBinder IRigConstraint.binder => s_Binder;

	IAnimationJobData IRigConstraint.data => m_Data;

	Component IRigConstraint.component => this;

	public void Reset()
	{
		m_Weight = 1f;
		m_Data.SetDefaultValues();
	}

	public bool IsValid()
	{
		return m_Data.IsValid();
	}

	protected virtual void OnValidate()
	{
		m_Weight = Mathf.Clamp01(m_Weight);
	}

	public IAnimationJob CreateJob(Animator animator)
	{
		TJob val = s_Binder.Create(animator, ref m_Data, this);
		val.jobWeight = FloatProperty.BindCustom(animator, ConstraintsUtils.ConstructCustomPropertyName(this, ConstraintProperties.s_Weight));
		return val;
	}

	public void DestroyJob(IAnimationJob job)
	{
		s_Binder.Destroy((TJob)job);
	}

	public void UpdateJob(IAnimationJob job)
	{
		s_Binder.Update((TJob)job, ref m_Data);
	}
}
