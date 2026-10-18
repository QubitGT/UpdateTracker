using System;
using Unity.XR.CoreUtils.Datums;

namespace UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme.Primitives;

[Serializable]
[Obsolete("The Affordance System namespace and all associated classes have been deprecated. The existing affordance system will be moved, replaced and updated with a new interaction feedback system in a future version of XRI.")]
public class Vector2AffordanceThemeDatumProperty : DatumProperty<Vector2AffordanceTheme, Vector2AffordanceThemeDatum>
{
	public Vector2AffordanceThemeDatumProperty(Vector2AffordanceTheme value)
		: base(value)
	{
	}

	public Vector2AffordanceThemeDatumProperty(Vector2AffordanceThemeDatum datum)
		: base(datum)
	{
	}
}
