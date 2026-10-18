using System;
using Unity.XR.CoreUtils.Datums;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme.Primitives;

[Serializable]
[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public class Vector4AffordanceThemeDatumProperty : DatumProperty<Vector4AffordanceTheme, Vector4AffordanceThemeDatum>
{
	public Vector4AffordanceThemeDatumProperty(Vector4AffordanceTheme value)
		: base(value)
	{
	}

	public Vector4AffordanceThemeDatumProperty(Vector4AffordanceThemeDatum datum)
		: base(datum)
	{
	}
}
