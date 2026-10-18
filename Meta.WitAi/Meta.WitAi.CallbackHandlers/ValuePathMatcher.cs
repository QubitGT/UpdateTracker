using System;
using Meta.WitAi.Data;
using UnityEngine;

namespace Meta.WitAi.CallbackHandlers;

[Serializable]
public class ValuePathMatcher
{
	[Tooltip("The path to a value within a WitResponseNode")]
	public string path;

	[Tooltip("A reference to a wit value object")]
	public WitValue witValueReference;

	[Tooltip("Does this path need to have text in the value to be considered a match")]
	public bool contentRequired = true;

	[Tooltip("If set the match value will be treated as a regular expression.")]
	public MatchMethod matchMethod;

	[Tooltip("The operator used to compare the value with the match value. Ex: response.value > matchValue")]
	public ComparisonMethod comparisonMethod;

	[Tooltip("Value used to compare with the result when Match Required is set")]
	public string matchValue;

	[Tooltip("The variance allowed when comparing two floating point values for equality")]
	public double floatingPointComparisonTolerance = 9.999999747378752E-05;

	[Tooltip("Confidence ranges are executed in order. If checked, all confidence values will be checked instead of stopping on the first one that matches.")]
	[SerializeField]
	public bool allowConfidenceOverlap;

	[Tooltip("The confidence levels to handle for this value.\nNOTE: The selected node must have a confidence sibling node.")]
	public ConfidenceRange[] confidenceRanges;

	private WitResponseReference pathReference;

	private WitResponseReference confidencePathReference;

	public WitResponseReference ConfidenceReference
	{
		get
		{
			if (confidencePathReference != null)
			{
				return confidencePathReference;
			}
			string text = Reference?.path;
			if (!string.IsNullOrEmpty(text))
			{
				text = text.Substring(0, text.LastIndexOf("."));
				text += ".confidence";
				confidencePathReference = WitResultUtilities.GetWitResponseReference(text);
			}
			return confidencePathReference;
		}
	}

	public WitResponseReference Reference
	{
		get
		{
			if ((bool)witValueReference)
			{
				return witValueReference.Reference;
			}
			if (pathReference == null || pathReference.path != path)
			{
				pathReference = WitResultUtilities.GetWitResponseReference(path);
			}
			return pathReference;
		}
	}
}
