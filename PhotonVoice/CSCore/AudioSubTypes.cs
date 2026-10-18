using System;

namespace CSCore;

public static class AudioSubTypes
{
	public static readonly Guid MediaTypeAudio = new Guid("73647561-0000-0010-8000-00AA00389B71");

	public static readonly Guid Unknown = new Guid(0, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid Pcm = new Guid(1, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid Adpcm = new Guid(2, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid IeeeFloat = new Guid(3, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid Vselp = new Guid(4, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid IbmCvsd = new Guid(5, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid ALaw = new Guid(6, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid MuLaw = new Guid(7, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid Dts = new Guid(8, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid Drm = new Guid(9, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WmaVoice9 = new Guid(10, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid OkiAdpcm = new Guid(16, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid DviAdpcm = new Guid(17, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid ImaAdpcm = new Guid(17, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid MediaspaceAdpcm = new Guid(18, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid SierraAdpcm = new Guid(19, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid G723Adpcm = new Guid(20, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid DigiStd = new Guid(21, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid DigiFix = new Guid(22, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid DialogicOkiAdpcm = new Guid(23, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid MediaVisionAdpcm = new Guid(24, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid CUCodec = new Guid(25, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid YamahaAdpcm = new Guid(32, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid SonarC = new Guid(33, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid DspGroupTrueSpeech = new Guid(34, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid EchoSpeechCorporation1 = new Guid(35, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid AudioFileAf36 = new Guid(36, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid Aptx = new Guid(37, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid AudioFileAf10 = new Guid(38, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid Prosody1612 = new Guid(39, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid Lrc = new Guid(40, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid DolbyAc2 = new Guid(48, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid Gsm610 = new Guid(49, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid MsnAudio = new Guid(50, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid AntexAdpcme = new Guid(51, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid ControlResVqlpc = new Guid(52, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid DigiReal = new Guid(53, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid DigiAdpcm = new Guid(54, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid ControlResCr10 = new Guid(55, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_NMS_VBXADPCM = new Guid(56, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_CS_IMAADPCM = new Guid(57, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_ECHOSC3 = new Guid(58, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_ROCKWELL_ADPCM = new Guid(59, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_ROCKWELL_DIGITALK = new Guid(60, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_XEBEC = new Guid(61, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_G721_ADPCM = new Guid(64, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_G728_CELP = new Guid(65, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_MSG723 = new Guid(66, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid Mpeg = new Guid(80, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_RT24 = new Guid(82, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_PAC = new Guid(83, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid MpegLayer3 = new Guid(85, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_LUCENT_G723 = new Guid(89, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_CIRRUS = new Guid(96, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_ESPCM = new Guid(97, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_VOXWARE = new Guid(98, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_CANOPUS_ATRAC = new Guid(99, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_G726_ADPCM = new Guid(100, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_G722_ADPCM = new Guid(101, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_DSAT_DISPLAY = new Guid(103, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_VOXWARE_BYTE_ALIGNED = new Guid(105, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_VOXWARE_AC8 = new Guid(112, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_VOXWARE_AC10 = new Guid(113, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_VOXWARE_AC16 = new Guid(114, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_VOXWARE_AC20 = new Guid(115, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_VOXWARE_RT24 = new Guid(116, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_VOXWARE_RT29 = new Guid(117, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_VOXWARE_RT29HW = new Guid(118, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_VOXWARE_VR12 = new Guid(119, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_VOXWARE_VR18 = new Guid(120, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_VOXWARE_TQ40 = new Guid(121, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_SOFTSOUND = new Guid(128, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_VOXWARE_TQ60 = new Guid(129, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_MSRT24 = new Guid(130, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_G729A = new Guid(131, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_MVI_MVI2 = new Guid(132, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_DF_G726 = new Guid(133, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_DF_GSM610 = new Guid(134, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_ISIAUDIO = new Guid(136, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_ONLIVE = new Guid(137, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_SBC24 = new Guid(145, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_DOLBY_AC3_SPDIF = new Guid(146, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_MEDIASONIC_G723 = new Guid(147, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_PROSODY_8KBPS = new Guid(148, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_ZYXEL_ADPCM = new Guid(151, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_PHILIPS_LPCBB = new Guid(152, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_PACKED = new Guid(153, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_MALDEN_PHONYTALK = new Guid(160, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid Gsm = new Guid(161, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid G729 = new Guid(162, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid G723 = new Guid(163, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid Acelp = new Guid(164, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid RawAac = new Guid(255, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_RHETOREX_ADPCM = new Guid(256, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_IRAT = new Guid(257, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_VIVO_G723 = new Guid(273, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_VIVO_SIREN = new Guid(274, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_DIGITAL_G723 = new Guid(291, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_SANYO_LD_ADPCM = new Guid(293, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_SIPROLAB_ACEPLNET = new Guid(304, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_SIPROLAB_ACELP4800 = new Guid(305, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_SIPROLAB_ACELP8V3 = new Guid(306, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_SIPROLAB_G729 = new Guid(307, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_SIPROLAB_G729A = new Guid(308, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_SIPROLAB_KELVIN = new Guid(309, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_G726ADPCM = new Guid(320, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_QUALCOMM_PUREVOICE = new Guid(336, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_QUALCOMM_HALFRATE = new Guid(337, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_TUBGSM = new Guid(341, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_MSAUDIO1 = new Guid(352, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WindowsMediaAudio = new Guid(353, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WindowsMediaAudioProfessional = new Guid(354, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WindowsMediaAudioLosseless = new Guid(355, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WindowsMediaAudioSpdif = new Guid(356, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_UNISYS_NAP_ADPCM = new Guid(368, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_UNISYS_NAP_ULAW = new Guid(369, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_UNISYS_NAP_ALAW = new Guid(370, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_UNISYS_NAP_16K = new Guid(371, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_CREATIVE_ADPCM = new Guid(512, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_CREATIVE_FASTSPEECH8 = new Guid(514, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_CREATIVE_FASTSPEECH10 = new Guid(515, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_UHER_ADPCM = new Guid(528, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_QUARTERDECK = new Guid(544, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_ILINK_VC = new Guid(560, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_RAW_SPORT = new Guid(576, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_ESST_AC3 = new Guid(577, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_IPI_HSX = new Guid(592, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_IPI_RPELP = new Guid(593, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_CS2 = new Guid(608, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_SONY_SCX = new Guid(624, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_FM_TOWNS_SND = new Guid(768, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_BTV_DIGITAL = new Guid(1024, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_QDESIGN_MUSIC = new Guid(1104, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_VME_VMPCM = new Guid(1664, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_TPC = new Guid(1665, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_OLIGSM = new Guid(4096, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_OLIADPCM = new Guid(4097, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_OLICELP = new Guid(4098, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_OLISBC = new Guid(4099, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_OLIOPR = new Guid(4100, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_LH_CODEC = new Guid(4352, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_NORRIS = new Guid(5120, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_SOUNDSPACE_MUSICOMPRESS = new Guid(5376, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid MPEG_ADTS_AAC = new Guid(5632, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid MPEG_RAW_AAC = new Guid(5633, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid MPEG_LOAS = new Guid(5634, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid NOKIA_MPEG_ADTS_AAC = new Guid(5640, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid NOKIA_MPEG_RAW_AAC = new Guid(5641, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid VODAFONE_MPEG_ADTS_AAC = new Guid(5642, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid VODAFONE_MPEG_RAW_AAC = new Guid(5643, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid MPEG_HEAAC = new Guid(5648, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_DVM = new Guid(8192, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid Vorbis1 = new Guid(26447, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid Vorbis2 = new Guid(26448, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid Vorbis3 = new Guid(26449, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid Vorbis1P = new Guid(26479, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid Vorbis2P = new Guid(26480, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid Vorbis3P = new Guid(26481, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_RAW_AAC1 = new Guid(255, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_WMAVOICE9 = new Guid(10, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid Extensible = new Guid(65534, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_DEVELOPMENT = new Guid(65535, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static readonly Guid WAVE_FORMAT_FLAC = new Guid(61868, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);

	public static AudioEncoding EncodingFromSubType(Guid audioSubType)
	{
		int num = BitConverter.ToInt32(audioSubType.ToByteArray(), 0);
		if (Enum.IsDefined(typeof(AudioEncoding), (short)num))
		{
			return (AudioEncoding)num;
		}
		throw new ArgumentException("Invalid audioSubType.", "audioSubType");
	}

	public static Guid SubTypeFromEncoding(AudioEncoding audioEncoding)
	{
		if (Enum.IsDefined(typeof(AudioEncoding), (short)audioEncoding))
		{
			return new Guid((int)audioEncoding, 0, 16, 128, 0, 0, 170, 0, 56, 155, 113);
		}
		throw new ArgumentException("Invalid encoding.", "audioEncoding");
	}
}
