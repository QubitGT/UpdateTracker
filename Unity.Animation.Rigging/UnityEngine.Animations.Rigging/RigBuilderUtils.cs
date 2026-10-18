using System.Collections.Generic;
using UnityEngine.Experimental.Animations;
using UnityEngine.Playables;

namespace UnityEngine.Animations.Rigging;

internal static class RigBuilderUtils
{
	public struct PlayableChain
	{
		public string name;

		public Playable[] playables;

		public bool IsValid()
		{
			if (playables != null)
			{
				return playables.Length != 0;
			}
			return false;
		}
	}

	private static readonly ushort k_AnimationOutputPriority = 1000;

	public static Playable[] BuildRigPlayables(PlayableGraph graph, IRigLayer layer)
	{
		if (layer == null || layer.jobs == null || layer.jobs.Length == 0)
		{
			return null;
		}
		int num = layer.jobs.Length;
		Playable[] array = new Playable[num];
		for (int i = 0; i < num; i++)
		{
			IAnimationJobBinder binder = layer.constraints[i].binder;
			array[i] = binder.CreatePlayable(graph, layer.jobs[i]);
		}
		for (int j = 1; j < num; j++)
		{
			array[j].AddInput(array[j - 1], 0, 1f);
		}
		return array;
	}

	public static IEnumerable<PlayableChain> BuildPlayables(Animator animator, PlayableGraph graph, IList<IRigLayer> layers, SyncSceneToStreamLayer syncSceneToStreamLayer)
	{
		PlayableChain[] array = new PlayableChain[layers.Count + 1];
		int num = 1;
		foreach (IRigLayer layer in layers)
		{
			PlayableChain playableChain = default(PlayableChain);
			playableChain.name = layer.name;
			if (layer.Initialize(animator))
			{
				playableChain.playables = BuildRigPlayables(graph, layer);
			}
			array[num++] = playableChain;
		}
		if (syncSceneToStreamLayer.Initialize(animator, layers) && syncSceneToStreamLayer.IsValid())
		{
			PlayableChain playableChain2 = default(PlayableChain);
			playableChain2.name = "syncSceneToStream";
			playableChain2.playables = new Playable[1] { RigUtils.syncSceneToStreamBinder.CreatePlayable(graph, syncSceneToStreamLayer.job) };
			array[0] = playableChain2;
		}
		return array;
	}

	public static PlayableGraph BuildPlayableGraph(Animator animator, IList<IRigLayer> layers, SyncSceneToStreamLayer syncSceneToStreamLayer)
	{
		PlayableGraph playableGraph = PlayableGraph.Create(animator.gameObject.transform.name + "_Rigs");
		playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
		BuildPlayableGraph(playableGraph, animator, layers, syncSceneToStreamLayer);
		return playableGraph;
	}

	public static void BuildPlayableGraph(PlayableGraph graph, Animator animator, IList<IRigLayer> layers, SyncSceneToStreamLayer syncSceneToStreamLayer)
	{
		foreach (PlayableChain item in BuildPlayables(animator, graph, layers, syncSceneToStreamLayer))
		{
			if (item.IsValid())
			{
				AnimationPlayableOutput output = AnimationPlayableOutput.Create(graph, $"{item.name}-Output", animator);
				output.SetAnimationStreamSource(AnimationStreamSource.PreviousInputs);
				output.SetSortingOrder(k_AnimationOutputPriority);
				output.SetSourcePlayable(item.playables[item.playables.Length - 1]);
			}
		}
	}
}
