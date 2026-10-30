using System.Collections.Generic;
using Photon.Pun;

namespace GorillaTagScripts.ObstacleCourse;

public class ObstacleCourseManager : NetworkComponent, ITickSystemTick
{
	public List<ObstacleCourse> allObstaclesCourses = new List<ObstacleCourse>();

	public static ObstacleCourseManager Instance { get; private set; }

	public bool TickRunning { get; set; }

	protected override void Awake()
	{
		base.Awake();
		Instance = this;
	}

	internal override void OnEnable()
	{
		base.OnEnable();
		TickSystem<object>.AddCallbackTarget(this);
	}

	internal override void OnDisable()
	{
		base.OnEnable();
		TickSystem<object>.RemoveCallbackTarget(this);
	}

	public void Tick()
	{
		foreach (ObstacleCourse allObstaclesCourse in allObstaclesCourses)
		{
			allObstaclesCourse.InvokeUpdate();
		}
	}

	private void OnDestroy()
	{
		allObstaclesCourses.Clear();
	}

	protected override void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (info.Sender == PhotonNetwork.MasterClient)
		{
			stream.SendNext(allObstaclesCourses.Count);
			for (int i = 0; i < allObstaclesCourses.Count; i++)
			{
				stream.SendNext(allObstaclesCourses[i].winnerActorNumber);
				stream.SendNext(allObstaclesCourses[i].currentState);
			}
		}
	}

	protected override void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (info.Sender != PhotonNetwork.MasterClient)
		{
			return;
		}
		int num = (int)stream.ReceiveNext();
		for (int i = 0; i < num; i++)
		{
			int winnerActorNumber = (int)stream.ReceiveNext();
			ObstacleCourse.RaceState raceState = (ObstacleCourse.RaceState)stream.ReceiveNext();
			if (allObstaclesCourses[i].currentState != raceState)
			{
				allObstaclesCourses[i].Deserialize(winnerActorNumber, raceState);
			}
		}
	}
}
