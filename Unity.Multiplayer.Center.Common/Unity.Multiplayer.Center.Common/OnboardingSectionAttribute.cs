using System;

namespace Unity.Multiplayer.Center.Common;

[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
public sealed class OnboardingSectionAttribute : Attribute
{
	public readonly string Id;

	public OnboardingSectionCategory Category { get; }

	public DisplayCondition DisplayCondition { get; set; }

	public SelectedSolutionsData.HostingModel HostingModelDependency { get; set; }

	public SelectedSolutionsData.NetcodeSolution NetcodeDependency { get; set; }

	public int Priority { get; set; }

	public int Order { get; set; }

	public string TargetPackageId { get; set; }

	public OnboardingSectionAttribute(OnboardingSectionCategory category, string id)
	{
		Category = category;
		Id = id;
	}
}
