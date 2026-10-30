using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace GorillaTag.Sports;

[RequireComponent(typeof(AudioSource))]
public class SportScoreboard : NetworkComponent
{
	[Serializable]
	private class TeamParameters
	{
		[SerializeField]
		public AudioClip matchWonAudio;

		[SerializeField]
		public AudioClip goalScoredAudio;
	}

	[OnEnterPlay_SetNull]
	public static SportScoreboard Instance;

	[SerializeField]
	private List<TeamParameters> teamParameters = new List<TeamParameters>();

	[SerializeField]
	private int matchEndScore = 3;

	[SerializeField]
	private float matchEndScoreResetDelayTime = 3f;

	private List<int> teamScores = new List<int>();

	private List<int> teamScoresPrev = new List<int>();

	private bool runningMatchEndCoroutine;

	private AudioSource audioSource;

	private SportScoreboardVisuals[] scoreVisuals;

	protected override void Awake()
	{
		base.Awake();
		Instance = this;
		audioSource = GetComponent<AudioSource>();
		scoreVisuals = new SportScoreboardVisuals[teamParameters.Count];
		for (int i = 0; i < teamParameters.Count; i++)
		{
			teamScores.Add(0);
			teamScoresPrev.Add(0);
		}
	}

	public void RegisterTeamVisual(int TeamIndex, SportScoreboardVisuals visuals)
	{
		scoreVisuals[TeamIndex] = visuals;
		UpdateScoreboard();
	}

	private void UpdateScoreboard()
	{
		for (int i = 0; i < teamParameters.Count; i++)
		{
			if (!(scoreVisuals[i] == null))
			{
				int num = teamScores[i];
				if (scoreVisuals[i].score1s != null)
				{
					scoreVisuals[i].score1s.SetUVOffset(num % 10);
				}
				if (scoreVisuals[i].score10s != null)
				{
					scoreVisuals[i].score10s.SetUVOffset(num / 10 % 10);
				}
			}
		}
	}

	private void OnScoreUpdated()
	{
		for (int i = 0; i < teamScores.Count; i++)
		{
			if (teamScores[i] > teamScoresPrev[i] && teamParameters[i].goalScoredAudio != null && teamScores[i] < matchEndScore)
			{
				audioSource.GTPlayOneShot(teamParameters[i].goalScoredAudio);
			}
			teamScoresPrev[i] = teamScores[i];
		}
		if (!runningMatchEndCoroutine)
		{
			for (int j = 0; j < teamScores.Count; j++)
			{
				if (teamScores[j] >= matchEndScore)
				{
					StartCoroutine(MatchEndCoroutine(j));
					break;
				}
			}
		}
		UpdateScoreboard();
	}

	public void TeamScored(int team)
	{
		if (base.IsMine && !runningMatchEndCoroutine)
		{
			if (team >= 0 && team < teamScores.Count)
			{
				teamScores[team] += 1;
			}
			OnScoreUpdated();
		}
	}

	public void ResetScores()
	{
		if (base.IsMine && !runningMatchEndCoroutine)
		{
			for (int i = 0; i < teamScores.Count; i++)
			{
				teamScores[i] = 0;
			}
			OnScoreUpdated();
		}
	}

	private IEnumerator MatchEndCoroutine(int winningTeam)
	{
		runningMatchEndCoroutine = true;
		if (winningTeam >= 0 && winningTeam < teamParameters.Count && teamParameters[winningTeam].matchWonAudio != null)
		{
			audioSource.GTPlayOneShot(teamParameters[winningTeam].matchWonAudio);
		}
		yield return new WaitForSeconds(matchEndScoreResetDelayTime);
		runningMatchEndCoroutine = false;
		ResetScores();
	}

	protected override void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		for (int i = 0; i < teamScores.Count; i++)
		{
			stream.SendNext(teamScores[i]);
		}
	}

	protected override void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		for (int i = 0; i < teamScores.Count; i++)
		{
			teamScores[i] = (int)stream.ReceiveNext();
		}
		OnScoreUpdated();
	}
}
