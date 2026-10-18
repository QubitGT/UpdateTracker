using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Properties;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.UIElements;

namespace UnityEngine.Localization;

[Serializable]
[UxmlObject]
public abstract class LocalizedReference : CustomBinding, ISerializationCallbackReceiver
{
	[Serializable]
	[CompilerGenerated]
	public new abstract class UxmlSerializedData : CustomBinding.UxmlSerializedData
	{
		[UxmlAttribute("table")]
		[SerializeField]
		private TableReference TableReferenceUXML;

		[UxmlAttribute("entry")]
		[SerializeField]
		private TableEntryReference TableEntryReferenceUXML;

		[UxmlAttribute("fallback")]
		[SerializeField]
		private FallbackBehavior FallbackStateUXML;

		[SerializeField]
		[UxmlIgnore]
		[HideInInspector]
		private UxmlAttributeFlags TableReferenceUXML_UxmlAttributeFlags;

		[SerializeField]
		[UxmlIgnore]
		[HideInInspector]
		private UxmlAttributeFlags TableEntryReferenceUXML_UxmlAttributeFlags;

		[SerializeField]
		[UxmlIgnore]
		[HideInInspector]
		private UxmlAttributeFlags FallbackStateUXML_UxmlAttributeFlags;

		[RegisterUxmlCache]
		[Conditional("UNITY_EDITOR")]
		public new static void Register()
		{
			UxmlDescriptionCache.RegisterType(typeof(UxmlSerializedData), new UxmlAttributeNames[3]
			{
				new UxmlAttributeNames("TableReferenceUXML", "table", null),
				new UxmlAttributeNames("TableEntryReferenceUXML", "entry", null),
				new UxmlAttributeNames("FallbackStateUXML", "fallback", null)
			});
		}

		public override void Deserialize(object obj)
		{
			base.Deserialize(obj);
			LocalizedReference localizedReference = (LocalizedReference)obj;
			if (UnityEngine.UIElements.UxmlSerializedData.ShouldWriteAttributeValue(TableReferenceUXML_UxmlAttributeFlags))
			{
				localizedReference.TableReferenceUXML = TableReferenceUXML;
			}
			if (UnityEngine.UIElements.UxmlSerializedData.ShouldWriteAttributeValue(TableEntryReferenceUXML_UxmlAttributeFlags))
			{
				localizedReference.TableEntryReferenceUXML = TableEntryReferenceUXML;
			}
			if (UnityEngine.UIElements.UxmlSerializedData.ShouldWriteAttributeValue(FallbackStateUXML_UxmlAttributeFlags))
			{
				localizedReference.FallbackStateUXML = FallbackStateUXML;
			}
		}
	}

	[SerializeField]
	private TableReference m_TableReference;

	[SerializeField]
	private TableEntryReference m_TableEntryReference;

	[SerializeField]
	private FallbackBehavior m_FallbackState;

	[SerializeField]
	private bool m_WaitForCompletion;

	internal Locale m_LocaleOverride;

	private int m_ActivatedCount;

	public TableReference TableReference
	{
		get
		{
			return m_TableReference;
		}
		set
		{
			if (!value.Equals(m_TableReference))
			{
				m_TableReference = value;
				ForceUpdate();
			}
		}
	}

	public TableEntryReference TableEntryReference
	{
		get
		{
			return m_TableEntryReference;
		}
		set
		{
			if (!value.Equals(m_TableEntryReference))
			{
				m_TableEntryReference = value;
				ForceUpdate();
			}
		}
	}

	public FallbackBehavior FallbackState
	{
		get
		{
			return m_FallbackState;
		}
		set
		{
			m_FallbackState = value;
		}
	}

	public Locale LocaleOverride
	{
		get
		{
			return m_LocaleOverride;
		}
		set
		{
			if (!(m_LocaleOverride == value))
			{
				m_LocaleOverride = value;
				ForceUpdate();
			}
		}
	}

	public virtual bool WaitForCompletion
	{
		get
		{
			return m_WaitForCompletion;
		}
		set
		{
			m_WaitForCompletion = value;
		}
	}

	internal abstract bool ForceSynchronous { get; }

	public bool IsEmpty
	{
		get
		{
			if (TableReference.ReferenceType != 0)
			{
				return TableEntryReference.ReferenceType == TableEntryReference.Type.Empty;
			}
			return true;
		}
	}

	[UxmlAttribute("table")]
	internal TableReference TableReferenceUXML
	{
		get
		{
			return TableReference;
		}
		set
		{
			TableReference = value;
		}
	}

	[UxmlAttribute("entry")]
	internal TableEntryReference TableEntryReferenceUXML
	{
		get
		{
			return TableEntryReference;
		}
		set
		{
			TableEntryReference = value;
		}
	}

	[UxmlAttribute("fallback")]
	internal FallbackBehavior FallbackStateUXML
	{
		get
		{
			return FallbackState;
		}
		set
		{
			FallbackState = value;
		}
	}

	public void SetReference(TableReference table, TableEntryReference entry)
	{
		bool flag = false;
		if (!m_TableReference.Equals(table))
		{
			m_TableReference = table;
			flag = true;
		}
		if (!m_TableEntryReference.Equals(entry))
		{
			m_TableEntryReference = entry;
			flag = true;
		}
		if (flag)
		{
			ForceUpdate();
		}
	}

	public override string ToString()
	{
		return $"{TableReference}/{TableEntryReference.ToString(TableReference)}";
	}

	protected internal abstract void ForceUpdate();

	protected abstract void Reset();

	public virtual void OnBeforeSerialize()
	{
	}

	public virtual void OnAfterDeserialize()
	{
	}

	public LocalizedReference()
	{
		base.updateTrigger = BindingUpdateTrigger.WhenDirty;
	}

	protected override void OnActivated(in BindingActivationContext context)
	{
		base.OnActivated(in context);
		m_ActivatedCount++;
		if (m_ActivatedCount == 1)
		{
			Initialize();
		}
	}

	protected override void OnDeactivated(in BindingActivationContext context)
	{
		base.OnDeactivated(in context);
		m_ActivatedCount--;
		if (m_ActivatedCount == 0)
		{
			Cleanup();
		}
	}

	protected abstract void Initialize();

	protected abstract void Cleanup();

	internal BindingResult CreateErrorResult(in BindingContext context, VisitReturnCode errorCode, Type sourceType)
	{
		VisualElement targetElement = context.targetElement;
		string typeDisplayName = TypeUtility.GetTypeDisplayName(GetType());
		string text = $"{TypeUtility.GetTypeDisplayName(targetElement.GetType())}.{context.bindingId}";
		return errorCode switch
		{
			VisitReturnCode.InvalidPath => new BindingResult(BindingStatus.Failure, typeDisplayName + ": Binding id `" + text + "` is either invalid or contains a `null` value."), 
			VisitReturnCode.InvalidCast => new BindingResult(BindingStatus.Failure, $"{typeDisplayName}: Invalid conversion from {sourceType} for binding id `{text}`"), 
			VisitReturnCode.AccessViolation => new BindingResult(BindingStatus.Failure, typeDisplayName + ": Trying set value for binding id `" + text + "`, but it is read-only."), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
