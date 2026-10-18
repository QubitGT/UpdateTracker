using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class ProcessPort
{
	public string processName { get; set; }

	public int processId { get; set; }

	public string portNumber { get; set; }

	public string protocol { get; set; }

	public override string ToString()
	{
		return $"{processName}({processId}) ({protocol} port {portNumber})";
	}

	private static string LookupProcess(int pid)
	{
		try
		{
			return Process.GetProcessById(pid).ProcessName;
		}
		catch (Exception message)
		{
			UnityEngine.Debug.LogError(message);
			return "-";
		}
	}

	public static List<ProcessPort> GetProcessesByPort(string targetPort)
	{
		List<ProcessPort> list = new List<ProcessPort>();
		try
		{
			using Process process = new Process();
			ProcessStartInfo processStartInfo = new ProcessStartInfo();
			processStartInfo.Arguments = "-a -n -o";
			processStartInfo.FileName = "netstat.exe";
			processStartInfo.UseShellExecute = false;
			processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			processStartInfo.RedirectStandardInput = true;
			processStartInfo.RedirectStandardOutput = true;
			processStartInfo.RedirectStandardError = true;
			process.StartInfo = processStartInfo;
			process.Start();
			StreamReader standardOutput = process.StandardOutput;
			string input = string.Concat(str1: process.StandardError.ReadToEnd(), str0: standardOutput.ReadToEnd());
			if (process.ExitCode != 0)
			{
				UnityEngine.Debug.LogError("netstat call failed");
				return list;
			}
			Regex regex = new Regex("\r\n");
			Regex regex2 = new Regex("\\s+");
			Regex regex3 = new Regex("\\[(.*?)\\]");
			string[] array = regex.Split(input);
			foreach (string input2 in array)
			{
				string[] array2 = regex2.Split(input2);
				if (array2.Length <= 4 || (!array2[1].Equals("UDP") && !array2[1].Equals("TCP")))
				{
					continue;
				}
				string text = regex3.Replace(array2[2], "1.1.1.1");
				string text2 = text.Split(':')[1];
				if (!(targetPort != text2))
				{
					int num = 0;
					try
					{
						num = (array2[1].Equals("UDP") ? Convert.ToInt32(array2[4]) : Convert.ToInt32(array2[5]));
					}
					catch (Exception ex)
					{
						UnityEngine.Debug.LogError(array2[1] + " " + array2[4] + " " + array2[5]);
						throw ex;
					}
					list.Add(new ProcessPort
					{
						protocol = (text.Contains("1.1.1.1") ? $"{array2[1]}v6" : $"{array2[1]}v4"),
						portNumber = text2,
						processName = LookupProcess(num),
						processId = num
					});
				}
			}
		}
		catch (Exception ex2)
		{
			UnityEngine.Debug.LogError(ex2.Message);
		}
		return list;
	}
}
