using System;
using System.IO;
using UnityEngine;

namespace Fusion;

internal class ClientTimeTrace
{
	private readonly long Timestamp;

	private readonly int Player;

	private int Frames;

	private double FrameDeltaTime;

	private bool PacketReceived;

	private int PacketNumber;

	private int Packets;

	private double PacketDeltaTime;

	private double RoundTripTime;

	private Simulation.TimeFeedback PacketFeedback;

	internal string Folder => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Photon/Fusion/Dev/Logs";

	internal string File
	{
		get
		{
			string arg = DateTimeOffset.FromUnixTimeMilliseconds(Timestamp).UtcDateTime.ToString("yyyy-MM-ddTHH-mm-ssZ");
			return $"fusion_player{Player}_timing_{arg}.csv";
		}
	}

	public ClientTimeTrace(int player, TickRate.Resolved tickRate)
	{
		Timestamp = ((DateTimeOffset)DateTime.Now).ToUnixTimeMilliseconds();
		Player = player;
		WriteHeaders(tickRate);
	}

	public void OnFeedback(Simulation.TimeFeedback packetFeedback)
	{
		PacketFeedback = packetFeedback;
	}

	public void OnPacket(int packetNumber, double packetDeltaTime, double roundTripTime)
	{
		RoundTripTime = roundTripTime;
		PacketDeltaTime = packetDeltaTime;
		Packets++;
		PacketNumber = packetNumber;
		PacketReceived = true;
	}

	public void OnFrame(double frameDeltaTime)
	{
		FrameDeltaTime = frameDeltaTime;
		Frames++;
		WriteLine();
		PacketReceived = false;
	}

	private void WriteHeaders(TickRate.Resolved tickRate)
	{
		Directory.CreateDirectory(Folder);
		StreamWriter streamWriter = System.IO.File.AppendText(Folder + "/" + File);
		string text = $"{Application.platform}";
		streamWriter.WriteLine("client_platform, client_tick_hz, client_send_hz, server_tick_hz, server_send_hz");
		streamWriter.WriteLine($"{text},{tickRate.Client},{tickRate.ClientSend},{tickRate.Server},{tickRate.ServerSend}");
		streamWriter.WriteLine("frame, frame_dt, received_packet, packet, packet_sequence, packet_dt, rtt, fb_packet_dt_avg, fb_packet_dt_dev, fb_buffer_avg, fb_buffer_dev");
		streamWriter.Close();
	}

	private void WriteLine()
	{
		Directory.CreateDirectory(Folder);
		StreamWriter streamWriter = System.IO.File.AppendText(Folder + "/" + File);
		if (PacketReceived)
		{
			streamWriter.WriteLine($"{Frames - 1},{FrameDeltaTime:f4},{Convert.ToInt32(PacketReceived)},{Packets - 1},{PacketNumber},{PacketDeltaTime:f4},{RoundTripTime:f4},{PacketFeedback.RecvDeltaAvg:f4},{PacketFeedback.RecvDeltaDev:f4},{PacketFeedback.OffsetAvg:f4},{PacketFeedback.OffsetDev:f4}");
		}
		else
		{
			streamWriter.WriteLine($"{Frames - 1},{FrameDeltaTime:f4},{Convert.ToInt32(PacketReceived)}");
		}
		streamWriter.Close();
	}
}
