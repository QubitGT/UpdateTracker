using System;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

namespace UnityEngine.Localization.SmartFormat.GlobalVariables;

[Serializable]
[Obsolete("Please use UnityEngine.Localization.SmartFormat.PersistentVariables.Variable instead.")]
public class GlobalVariable<T> : Variable<T>
{
}
