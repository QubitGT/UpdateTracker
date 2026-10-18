using System;

namespace Oculus.Platform.Models;

public class ContentRating
{
	public readonly string AgeRatingImageUri;

	public readonly string AgeRatingText;

	public readonly string[] Descriptors;

	public readonly string[] InteractiveElements;

	public readonly string RatingDefinitionUri;

	public ContentRating(IntPtr o)
	{
		AgeRatingImageUri = CAPI.ovr_ContentRating_GetAgeRatingImageUri(o);
		AgeRatingText = CAPI.ovr_ContentRating_GetAgeRatingText(o);
		uint num = CAPI.ovr_ContentRating_GetDescriptorsSize(o);
		Descriptors = new string[num];
		for (uint num2 = 0u; num2 < num; num2++)
		{
			Descriptors[num2] = CAPI.ovr_ContentRating_GetDescriptor(o, num2);
		}
		uint num3 = CAPI.ovr_ContentRating_GetInteractiveElementsSize(o);
		InteractiveElements = new string[num3];
		for (uint num4 = 0u; num4 < num3; num4++)
		{
			InteractiveElements[num4] = CAPI.ovr_ContentRating_GetInteractiveElement(o, num4);
		}
		RatingDefinitionUri = CAPI.ovr_ContentRating_GetRatingDefinitionUri(o);
	}
}
