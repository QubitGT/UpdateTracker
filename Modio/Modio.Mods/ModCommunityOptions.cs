using System;

namespace Modio.Mods;

[Flags]
public enum ModCommunityOptions
{
	None = 0,
	EnableComments = 1,
	EnablePreviews = 0x40,
	EnablePreviewUrls = 0x80,
	AllowDependencies = 0x400
}
