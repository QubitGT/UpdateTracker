using System.Text;
using OVRSimpleJSON;
using UnityEngine;

public class OVRSystemPerfMetrics
{
	public class PerfMetrics
	{
		public int frameCount;

		public float frameTime;

		public float deltaFrameTime;

		public bool appCpuTime_IsValid;

		public float appCpuTime;

		public bool appGpuTime_IsValid;

		public float appGpuTime;

		public bool compositorCpuTime_IsValid;

		public float compositorCpuTime;

		public bool compositorGpuTime_IsValid;

		public float compositorGpuTime;

		public bool compositorDroppedFrameCount_IsValid;

		public int compositorDroppedFrameCount;

		public bool compositorSpaceWarpMode_IsValid;

		public int compositorSpaceWarpMode;

		public bool systemGpuUtilPercentage_IsValid;

		public float systemGpuUtilPercentage;

		public bool systemCpuUtilAveragePercentage_IsValid;

		public float systemCpuUtilAveragePercentage;

		public bool systemCpuUtilWorstPercentage_IsValid;

		public float systemCpuUtilWorstPercentage;

		public bool deviceCpuClockFrequencyInMHz_IsValid;

		public float deviceCpuClockFrequencyInMHz;

		public bool deviceGpuClockFrequencyInMHz_IsValid;

		public float deviceGpuClockFrequencyInMHz;

		public bool deviceCpuClockLevel_IsValid;

		public int deviceCpuClockLevel;

		public bool deviceGpuClockLevel_IsValid;

		public int deviceGpuClockLevel;

		public bool[] deviceCpuCoreUtilPercentage_IsValid = new bool[OVRPlugin.MAX_CPU_CORES];

		public float[] deviceCpuCoreUtilPercentage = new float[OVRPlugin.MAX_CPU_CORES];

		public string ToJSON()
		{
			JSONObject jSONObject = new JSONObject();
			jSONObject.Add("frameCount", new JSONNumber(frameCount));
			jSONObject.Add("frameTime", new JSONNumber(frameTime));
			jSONObject.Add("deltaFrameTime", new JSONNumber(deltaFrameTime));
			if (appCpuTime_IsValid)
			{
				jSONObject.Add("appCpuTime", new JSONNumber(appCpuTime));
			}
			if (appGpuTime_IsValid)
			{
				jSONObject.Add("appGpuTime", new JSONNumber(appGpuTime));
			}
			if (compositorCpuTime_IsValid)
			{
				jSONObject.Add("compositorCpuTime", new JSONNumber(compositorCpuTime));
			}
			if (compositorGpuTime_IsValid)
			{
				jSONObject.Add("compositorGpuTime", new JSONNumber(compositorGpuTime));
			}
			if (compositorDroppedFrameCount_IsValid)
			{
				jSONObject.Add("compositorDroppedFrameCount", new JSONNumber(compositorDroppedFrameCount));
			}
			if (compositorSpaceWarpMode_IsValid)
			{
				jSONObject.Add("compositorSpaceWarpMode", new JSONNumber(compositorSpaceWarpMode));
			}
			if (systemGpuUtilPercentage_IsValid)
			{
				jSONObject.Add("systemGpuUtilPercentage", new JSONNumber(systemGpuUtilPercentage));
			}
			if (systemCpuUtilAveragePercentage_IsValid)
			{
				jSONObject.Add("systemCpuUtilAveragePercentage", new JSONNumber(systemCpuUtilAveragePercentage));
			}
			if (systemCpuUtilWorstPercentage_IsValid)
			{
				jSONObject.Add("systemCpuUtilWorstPercentage", new JSONNumber(systemCpuUtilWorstPercentage));
			}
			if (deviceCpuClockFrequencyInMHz_IsValid)
			{
				jSONObject.Add("deviceCpuClockFrequencyInMHz", new JSONNumber(deviceCpuClockFrequencyInMHz));
			}
			if (deviceGpuClockFrequencyInMHz_IsValid)
			{
				jSONObject.Add("deviceGpuClockFrequencyInMHz", new JSONNumber(deviceGpuClockFrequencyInMHz));
			}
			if (deviceCpuClockLevel_IsValid)
			{
				jSONObject.Add("deviceCpuClockLevel", new JSONNumber(deviceCpuClockLevel));
			}
			if (deviceGpuClockLevel_IsValid)
			{
				jSONObject.Add("deviceGpuClockLevel", new JSONNumber(deviceGpuClockLevel));
			}
			for (int i = 0; i < OVRPlugin.MAX_CPU_CORES; i++)
			{
				if (deviceCpuCoreUtilPercentage_IsValid[i])
				{
					jSONObject.Add("deviceCpuCore" + i + "UtilPercentage", new JSONNumber(deviceCpuCoreUtilPercentage[i]));
				}
			}
			return jSONObject.ToString();
		}

		public bool LoadFromJSON(string json)
		{
			JSONObject jSONObject = JSONNode.Parse(json) as JSONObject;
			if (jSONObject == null)
			{
				return false;
			}
			frameCount = ((jSONObject["frameCount"] != null) ? jSONObject["frameCount"].AsInt : 0);
			frameTime = ((jSONObject["frameTime"] != null) ? jSONObject["frameTime"].AsFloat : 0f);
			deltaFrameTime = ((jSONObject["deltaFrameTime"] != null) ? jSONObject["deltaFrameTime"].AsFloat : 0f);
			appCpuTime_IsValid = jSONObject["appCpuTime"] != null;
			appCpuTime = (appCpuTime_IsValid ? jSONObject["appCpuTime"].AsFloat : 0f);
			appGpuTime_IsValid = jSONObject["appGpuTime"] != null;
			appGpuTime = (appGpuTime_IsValid ? jSONObject["appGpuTime"].AsFloat : 0f);
			compositorCpuTime_IsValid = jSONObject["compositorCpuTime"] != null;
			compositorCpuTime = (compositorCpuTime_IsValid ? jSONObject["compositorCpuTime"].AsFloat : 0f);
			compositorGpuTime_IsValid = jSONObject["compositorGpuTime"] != null;
			compositorGpuTime = (compositorGpuTime_IsValid ? jSONObject["compositorGpuTime"].AsFloat : 0f);
			compositorDroppedFrameCount_IsValid = jSONObject["compositorDroppedFrameCount"] != null;
			compositorDroppedFrameCount = (compositorDroppedFrameCount_IsValid ? jSONObject["compositorDroppedFrameCount"].AsInt : 0);
			compositorSpaceWarpMode_IsValid = jSONObject["compositorSpaceWarpMode"] != null;
			compositorSpaceWarpMode = (compositorSpaceWarpMode_IsValid ? jSONObject["compositorSpaceWarpMode"].AsInt : 0);
			systemGpuUtilPercentage_IsValid = jSONObject["systemGpuUtilPercentage"] != null;
			systemGpuUtilPercentage = (systemGpuUtilPercentage_IsValid ? jSONObject["systemGpuUtilPercentage"].AsFloat : 0f);
			systemCpuUtilAveragePercentage_IsValid = jSONObject["systemCpuUtilAveragePercentage"] != null;
			systemCpuUtilAveragePercentage = (systemCpuUtilAveragePercentage_IsValid ? jSONObject["systemCpuUtilAveragePercentage"].AsFloat : 0f);
			systemCpuUtilWorstPercentage_IsValid = jSONObject["systemCpuUtilWorstPercentage"] != null;
			systemCpuUtilWorstPercentage = (systemCpuUtilWorstPercentage_IsValid ? jSONObject["systemCpuUtilWorstPercentage"].AsFloat : 0f);
			deviceCpuClockFrequencyInMHz_IsValid = jSONObject["deviceCpuClockFrequencyInMHz"] != null;
			deviceCpuClockFrequencyInMHz = (deviceCpuClockFrequencyInMHz_IsValid ? jSONObject["deviceCpuClockFrequencyInMHz"].AsFloat : 0f);
			deviceGpuClockFrequencyInMHz_IsValid = jSONObject["deviceGpuClockFrequencyInMHz"] != null;
			deviceGpuClockFrequencyInMHz = (deviceGpuClockFrequencyInMHz_IsValid ? jSONObject["deviceGpuClockFrequencyInMHz"].AsFloat : 0f);
			deviceCpuClockLevel_IsValid = jSONObject["deviceCpuClockLevel"] != null;
			deviceCpuClockLevel = (deviceCpuClockLevel_IsValid ? jSONObject["deviceCpuClockLevel"].AsInt : 0);
			deviceGpuClockLevel_IsValid = jSONObject["deviceGpuClockLevel"] != null;
			deviceGpuClockLevel = (deviceGpuClockLevel_IsValid ? jSONObject["deviceGpuClockLevel"].AsInt : 0);
			for (int i = 0; i < OVRPlugin.MAX_CPU_CORES; i++)
			{
				deviceCpuCoreUtilPercentage_IsValid[i] = jSONObject["deviceCpuCore" + i + "UtilPercentage"] != null;
				deviceCpuCoreUtilPercentage[i] = (deviceCpuCoreUtilPercentage_IsValid[i] ? jSONObject["deviceCpuCore" + i + "UtilPercentage"].AsFloat : 0f);
			}
			return true;
		}
	}

	public class OVRSystemPerfMetricsTcpServer : MonoBehaviour
	{
		public static OVRSystemPerfMetricsTcpServer singleton;

		private OVRNetwork.OVRNetworkTcpServer tcpServer = new OVRNetwork.OVRNetworkTcpServer();

		public int listeningPort = 32419;

		private void OnEnable()
		{
			if (singleton != null)
			{
				Debug.LogError("Mutiple OVRSystemPerfMetricsTcpServer exists");
				return;
			}
			singleton = this;
			if (Application.isEditor)
			{
				Application.runInBackground = true;
			}
			tcpServer.StartListening(listeningPort);
		}

		private void OnDisable()
		{
			tcpServer.StopListening();
			singleton = null;
			Debug.Log("[OVRSystemPerfMetricsTcpServer] server destroyed");
		}

		private void Update()
		{
			if (tcpServer.HasConnectedClient())
			{
				string s = GatherPerfMetrics().ToJSON();
				byte[] bytes = Encoding.UTF8.GetBytes(s);
				tcpServer.Broadcast(100, bytes);
			}
		}

		public PerfMetrics GatherPerfMetrics()
		{
			PerfMetrics perfMetrics = new PerfMetrics();
			perfMetrics.frameCount = Time.frameCount;
			perfMetrics.frameTime = Time.unscaledTime;
			perfMetrics.deltaFrameTime = Time.unscaledDeltaTime;
			float? perfMetricsFloat = OVRPlugin.GetPerfMetricsFloat(OVRPlugin.PerfMetrics.App_CpuTime_Float);
			perfMetrics.appCpuTime_IsValid = perfMetricsFloat.HasValue;
			perfMetrics.appCpuTime = perfMetricsFloat.GetValueOrDefault();
			perfMetricsFloat = OVRPlugin.GetPerfMetricsFloat(OVRPlugin.PerfMetrics.App_GpuTime_Float);
			perfMetrics.appGpuTime_IsValid = perfMetricsFloat.HasValue;
			perfMetrics.appGpuTime = perfMetricsFloat.GetValueOrDefault();
			perfMetricsFloat = OVRPlugin.GetPerfMetricsFloat(OVRPlugin.PerfMetrics.Compositor_CpuTime_Float);
			perfMetrics.compositorCpuTime_IsValid = perfMetricsFloat.HasValue;
			perfMetrics.compositorCpuTime = perfMetricsFloat.GetValueOrDefault();
			perfMetricsFloat = OVRPlugin.GetPerfMetricsFloat(OVRPlugin.PerfMetrics.Compositor_GpuTime_Float);
			perfMetrics.compositorGpuTime_IsValid = perfMetricsFloat.HasValue;
			perfMetrics.compositorGpuTime = perfMetricsFloat.GetValueOrDefault();
			int? perfMetricsInt = OVRPlugin.GetPerfMetricsInt(OVRPlugin.PerfMetrics.Compositor_DroppedFrameCount_Int);
			perfMetrics.compositorDroppedFrameCount_IsValid = perfMetricsInt.HasValue;
			perfMetrics.compositorDroppedFrameCount = perfMetricsInt.GetValueOrDefault();
			perfMetricsInt = OVRPlugin.GetPerfMetricsInt(OVRPlugin.PerfMetrics.Compositor_SpaceWarp_Mode_Int);
			perfMetrics.compositorSpaceWarpMode_IsValid = perfMetricsInt.HasValue;
			perfMetrics.compositorSpaceWarpMode = perfMetricsInt.GetValueOrDefault();
			perfMetricsFloat = OVRPlugin.GetPerfMetricsFloat(OVRPlugin.PerfMetrics.System_GpuUtilPercentage_Float);
			perfMetrics.systemGpuUtilPercentage_IsValid = perfMetricsFloat.HasValue;
			perfMetrics.systemGpuUtilPercentage = perfMetricsFloat.GetValueOrDefault();
			perfMetricsFloat = OVRPlugin.GetPerfMetricsFloat(OVRPlugin.PerfMetrics.System_CpuUtilAveragePercentage_Float);
			perfMetrics.systemCpuUtilAveragePercentage_IsValid = perfMetricsFloat.HasValue;
			perfMetrics.systemCpuUtilAveragePercentage = perfMetricsFloat.GetValueOrDefault();
			perfMetricsFloat = OVRPlugin.GetPerfMetricsFloat(OVRPlugin.PerfMetrics.System_CpuUtilWorstPercentage_Float);
			perfMetrics.systemCpuUtilWorstPercentage_IsValid = perfMetricsFloat.HasValue;
			perfMetrics.systemCpuUtilWorstPercentage = perfMetricsFloat.GetValueOrDefault();
			perfMetricsFloat = OVRPlugin.GetPerfMetricsFloat(OVRPlugin.PerfMetrics.Device_CpuClockFrequencyInMHz_Float);
			perfMetrics.deviceCpuClockFrequencyInMHz_IsValid = perfMetricsFloat.HasValue;
			perfMetrics.deviceCpuClockFrequencyInMHz = perfMetricsFloat.GetValueOrDefault();
			perfMetricsFloat = OVRPlugin.GetPerfMetricsFloat(OVRPlugin.PerfMetrics.Device_GpuClockFrequencyInMHz_Float);
			perfMetrics.deviceGpuClockFrequencyInMHz_IsValid = perfMetricsFloat.HasValue;
			perfMetrics.deviceGpuClockFrequencyInMHz = perfMetricsFloat.GetValueOrDefault();
			perfMetricsInt = OVRPlugin.GetPerfMetricsInt(OVRPlugin.PerfMetrics.Device_CpuClockLevel_Int);
			perfMetrics.deviceCpuClockLevel_IsValid = perfMetricsInt.HasValue;
			perfMetrics.deviceCpuClockLevel = perfMetricsInt.GetValueOrDefault();
			perfMetricsInt = OVRPlugin.GetPerfMetricsInt(OVRPlugin.PerfMetrics.Device_GpuClockLevel_Int);
			perfMetrics.deviceGpuClockLevel_IsValid = perfMetricsInt.HasValue;
			perfMetrics.deviceGpuClockLevel = perfMetricsInt.GetValueOrDefault();
			for (int i = 0; i < OVRPlugin.MAX_CPU_CORES; i++)
			{
				perfMetricsFloat = OVRPlugin.GetPerfMetricsFloat(OVRPlugin.PerfMetrics.Device_CpuCore0UtilPercentage_Float);
				perfMetrics.deviceCpuCoreUtilPercentage_IsValid[i] = perfMetricsFloat.HasValue;
				perfMetrics.deviceCpuCoreUtilPercentage[i] = perfMetricsFloat.GetValueOrDefault();
			}
			return perfMetrics;
		}
	}

	public const int TcpListeningPort = 32419;

	public const int PayloadTypeMetrics = 100;

	public const int MaxBufferLength = 65536;

	public const int MaxMessageLength = 65532;
}
