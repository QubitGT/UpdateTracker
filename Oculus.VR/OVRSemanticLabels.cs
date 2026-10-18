using System;
using System.Collections.Generic;
using UnityEngine;

public readonly struct OVRSemanticLabels : IOVRAnchorComponent<OVRSemanticLabels>, IEquatable<OVRSemanticLabels>
{
	public enum Classification
	{
		Floor,
		Ceiling,
		WallFace,
		Table,
		Couch,
		DoorFrame,
		WindowFrame,
		Other,
		Storage,
		Bed,
		Screen,
		Lamp,
		Plant,
		WallArt,
		SceneMesh,
		InvisibleWallFace
	}

	public static readonly OVRSemanticLabels Null;

	private static char[] _semanticLabelsBuffer;

	internal const string DeprecationMessage = "String-based labels are deprecated (v65). Please use the equivalent enum-based methods.";

	OVRPlugin.SpaceComponentType IOVRAnchorComponent<OVRSemanticLabels>.Type => Type;

	ulong IOVRAnchorComponent<OVRSemanticLabels>.Handle => Handle;

	public bool IsNull => Handle == 0;

	public bool IsEnabled
	{
		get
		{
			bool enabled = default(bool);
			bool changePending = default(bool);
			if (!IsNull && OVRPlugin.GetSpaceComponentStatus(Handle, Type, out enabled, out changePending) && enabled)
			{
				return !changePending;
			}
			return false;
		}
	}

	internal OVRPlugin.SpaceComponentType Type => OVRPlugin.SpaceComponentType.SemanticLabels;

	internal ulong Handle { get; }

	[Obsolete("String-based labels are deprecated (v65). Please use the equivalent enum-based methods.")]
	public string Labels
	{
		get
		{
			if (!OVRPlugin.GetSpaceSemanticLabels(Handle, out var labels))
			{
				throw new Exception("Could not Get Semantic Labels");
			}
			return OVRSemanticClassification.ValidateAndUpgradeLabels(labels);
		}
	}

	OVRSemanticLabels IOVRAnchorComponent<OVRSemanticLabels>.FromAnchor(OVRAnchor anchor)
	{
		return new OVRSemanticLabels(anchor);
	}

	OVRTask<bool> IOVRAnchorComponent<OVRSemanticLabels>.SetEnabledAsync(bool enabled, double timeout)
	{
		throw new NotSupportedException("The SemanticLabels component cannot be enabled or disabled.");
	}

	public bool Equals(OVRSemanticLabels other)
	{
		return Handle == other.Handle;
	}

	public static bool operator ==(OVRSemanticLabels lhs, OVRSemanticLabels rhs)
	{
		return lhs.Equals(rhs);
	}

	public static bool operator !=(OVRSemanticLabels lhs, OVRSemanticLabels rhs)
	{
		return !lhs.Equals(rhs);
	}

	public override bool Equals(object obj)
	{
		if (obj is OVRSemanticLabels other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return Handle.GetHashCode() * 486187739 + ((int)Type).GetHashCode();
	}

	public override string ToString()
	{
		return $"{Handle}.SemanticLabels";
	}

	private OVRSemanticLabels(OVRAnchor anchor)
	{
		Handle = anchor.Handle;
	}

	public void GetClassifications(ICollection<Classification> classifications)
	{
		if (!OVRPlugin.GetSpaceSemanticLabelsNonAlloc(Handle, ref _semanticLabelsBuffer, out var length))
		{
			throw new Exception("Could not Get Semantic Labels");
		}
		classifications.Clear();
		FromApiString(new ReadOnlySpan<char>(_semanticLabelsBuffer, 0, length), classifications);
		bool num = classifications.Contains(Classification.InvisibleWallFace);
		bool flag = classifications.Contains(Classification.WallFace);
		if (num && !flag)
		{
			classifications.Add(Classification.WallFace);
		}
	}

	internal static Classification FromApiLabel(ReadOnlySpan<char> singleLabel)
	{
		if (singleLabel.SequenceEqual("FLOOR"))
		{
			return Classification.Floor;
		}
		if (singleLabel.SequenceEqual("CEILING"))
		{
			return Classification.Ceiling;
		}
		if (singleLabel.SequenceEqual("WALL_FACE"))
		{
			return Classification.WallFace;
		}
		if (singleLabel.SequenceEqual("COUCH"))
		{
			return Classification.Couch;
		}
		if (singleLabel.SequenceEqual("DOOR_FRAME"))
		{
			return Classification.DoorFrame;
		}
		if (singleLabel.SequenceEqual("WINDOW_FRAME"))
		{
			return Classification.WindowFrame;
		}
		if (singleLabel.SequenceEqual("OTHER"))
		{
			return Classification.Other;
		}
		if (singleLabel.SequenceEqual("STORAGE"))
		{
			return Classification.Storage;
		}
		if (singleLabel.SequenceEqual("BED"))
		{
			return Classification.Bed;
		}
		if (singleLabel.SequenceEqual("SCREEN"))
		{
			return Classification.Screen;
		}
		if (singleLabel.SequenceEqual("LAMP"))
		{
			return Classification.Lamp;
		}
		if (singleLabel.SequenceEqual("PLANT"))
		{
			return Classification.Plant;
		}
		if (singleLabel.SequenceEqual("TABLE"))
		{
			return Classification.Table;
		}
		if (singleLabel.SequenceEqual("WALL_ART"))
		{
			return Classification.WallArt;
		}
		if (singleLabel.SequenceEqual("INVISIBLE_WALL_FACE"))
		{
			return Classification.InvisibleWallFace;
		}
		if (singleLabel.SequenceEqual("GLOBAL_MESH"))
		{
			return Classification.SceneMesh;
		}
		Debug.LogWarning("Unknown classification: " + singleLabel);
		return Classification.Other;
	}

	internal static void FromApiString(ReadOnlySpan<char> apiLabels, ICollection<Classification> classifications)
	{
		int num = 0;
		int num2;
		while ((num2 = IndexOf(apiLabels, ',', num)) != -1)
		{
			AddLabel(apiLabels.Slice(num, num2 - num), classifications);
			num = num2 + 1;
		}
		if (num < apiLabels.Length)
		{
			AddLabel(apiLabels.Slice(num), classifications);
		}
		static void AddLabel(ReadOnlySpan<char> label, ICollection<Classification> labels)
		{
			if (!label.SequenceEqual("DESK"))
			{
				labels.Add(FromApiLabel(label));
			}
		}
		static int IndexOf(ReadOnlySpan<char> s, char c, int start)
		{
			for (int i = start; i < s.Length; i++)
			{
				if (s[i] == c)
				{
					return i;
				}
			}
			return -1;
		}
	}

	internal static string ToApiLabel(Classification classification)
	{
		return classification switch
		{
			Classification.Floor => "FLOOR", 
			Classification.Ceiling => "CEILING", 
			Classification.WallFace => "WALL_FACE", 
			Classification.Couch => "COUCH", 
			Classification.DoorFrame => "DOOR_FRAME", 
			Classification.WindowFrame => "WINDOW_FRAME", 
			Classification.Other => "OTHER", 
			Classification.Storage => "STORAGE", 
			Classification.Bed => "BED", 
			Classification.Screen => "SCREEN", 
			Classification.Lamp => "LAMP", 
			Classification.Plant => "PLANT", 
			Classification.Table => "TABLE", 
			Classification.WallArt => "WALL_ART", 
			Classification.InvisibleWallFace => "INVISIBLE_WALL_FACE", 
			Classification.SceneMesh => "GLOBAL_MESH", 
			_ => "OTHER", 
		};
	}

	internal static string ToApiString(IReadOnlyList<Classification> classifications)
	{
		if (classifications == null)
		{
			return string.Empty;
		}
		List<string> list;
		using (new OVRObjectPool.ListScope<string>(out list))
		{
			foreach (Classification classification in classifications)
			{
				list.Add(ToApiLabel(classification));
			}
			return string.Join(',', list);
		}
	}
}
