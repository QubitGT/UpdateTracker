using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine.Localization.SmartFormat.PersistentVariables;
using UnityEngine.UIElements;

namespace UnityEngine.Localization;

[UxmlObject]
internal class LocalVariable
{
	[Serializable]
	[CompilerGenerated]
	internal class UxmlSerializedData : UnityEngine.UIElements.UxmlSerializedData
	{
		[UxmlObjectReference]
		[SerializeReference]
		private UnityEngine.UIElements.UxmlSerializedData Variable;

		[Delayed]
		[SerializeField]
		private string Name;

		[SerializeField]
		[UxmlIgnore]
		[HideInInspector]
		private UxmlAttributeFlags Variable_UxmlAttributeFlags;

		[SerializeField]
		[UxmlIgnore]
		[HideInInspector]
		private UxmlAttributeFlags Name_UxmlAttributeFlags;

		[RegisterUxmlCache]
		[Conditional("UNITY_EDITOR")]
		public new static void Register()
		{
			UxmlDescriptionCache.RegisterType(typeof(UxmlSerializedData), new UxmlAttributeNames[2]
			{
				new UxmlAttributeNames("Name", "name", null),
				new UxmlAttributeNames("Variable", "variable", null)
			});
		}

		public override object CreateInstance()
		{
			return new LocalVariable();
		}

		public override void Deserialize(object obj)
		{
			LocalVariable localVariable = (LocalVariable)obj;
			if (UnityEngine.UIElements.UxmlSerializedData.ShouldWriteAttributeValue(Name_UxmlAttributeFlags))
			{
				localVariable.Name = Name;
			}
			if (UnityEngine.UIElements.UxmlSerializedData.ShouldWriteAttributeValue(Variable_UxmlAttributeFlags))
			{
				if (Variable != null)
				{
					IVariable variable = (IVariable)Variable.CreateInstance();
					Variable.Deserialize(variable);
					localVariable.Variable = variable;
				}
				else
				{
					localVariable.Variable = null;
				}
			}
		}
	}

	[UxmlAttribute]
	[Delayed]
	public string Name { get; set; }

	[UxmlObjectReference]
	public IVariable Variable { get; set; }
}
