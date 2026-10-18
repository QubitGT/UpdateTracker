using System;

namespace Meta.WitAi;

public static class WitConstants
{
	public const string API_VERSION = "20250213";

	public const string SDK_VERSION = "78.0.0";

	public const string CLIENT_NAME = "wit-unity";

	public const string URI_SCHEME = "https";

	public const string URI_AUTHORITY = "api.wit.ai";

	public const string URI_GRAPH_AUTHORITY = "graph.wit.ai/myprofile";

	public const int URI_DEFAULT_PORT = -1;

	public const WitRequestType DEFAULT_REQUEST_TYPE = WitRequestType.Http;

	public const int DEFAULT_REQUEST_TIMEOUT = 10000;

	public const string HEADER_REQUEST_ID = "X-Wit-Client-Request-Id";

	public const string HEADER_OP_ID = "X-Wit-Client-Operation-Id";

	public const string HEADER_CLIENT_USER_ID = "client-user-id";

	public const string HEADER_AUTH = "Authorization";

	public const string HEADER_USERAGENT = "User-Agent";

	public const string HEADER_USERAGENT_CONFID_MISSING = "not-yet-configured";

	public const string HEADER_POST_CONTENT = "Content-Type";

	public const string HEADER_GET_CONTENT = "Accept";

	public const string HEADER_TAG_ID = "tag";

	public const string HEADER_DEBUG = "is_debug";

	public const string RESPONSE_REQUEST_ID = "client_request_id";

	public const string RESPONSE_CLIENT_USER_ID = "client_user_id";

	public const string RESPONSE_OPERATION_ID = "operation_id";

	public const string RESPONSE_TYPE_KEY = "type";

	public const string RESPONSE_TYPE_PARTIAL_TRANSCRIPTION = "PARTIAL_TRANSCRIPTION";

	public const string RESPONSE_TYPE_FINAL_TRANSCRIPTION = "FINAL_TRANSCRIPTION";

	public const string RESPONSE_TYPE_PARTIAL_NLP = "PARTIAL_UNDERSTANDING";

	public const string RESPONSE_TYPE_FINAL_NLP = "FINAL_UNDERSTANDING";

	public const string RESPONSE_TYPE_READY_FOR_AUDIO = "INITIALIZED";

	public const string RESPONSE_TYPE_TTS = "SYNTHESIZE_DATA";

	public const string RESPONSE_TYPE_ERROR = "ERROR";

	public const string RESPONSE_TYPE_ABORTED = "ABORTED";

	public const string RESPONSE_TYPE_END = "END_STREAM";

	public const string ENDPOINT_SPEECH = "speech";

	public const string ENDPOINT_JSON_MIME = "application/json";

	public const int ENDPOINT_SPEECH_SAMPLE_RATE = 16000;

	public const string ENDPOINT_MESSAGE = "message";

	public const string ENDPOINT_MESSAGE_PARAM = "q";

	public const string ENDPOINT_JSON_DELIMITER = "\r\n";

	public const string ENDPOINT_ERROR_PARAM = "error";

	public const string ENDPOINT_CONTEXT_PARAM = "context";

	public const string ERROR_REACHABILITY = "Endpoint not reachable";

	public const string ERROR_NO_CONFIG = "No WitConfiguration Set";

	public const string ERROR_NO_CONFIG_TOKEN = "No WitConfiguration Client Token";

	public const string ENDPOINT_TTS = "synthesize";

	public const string ENDPOINT_TTS_PARAM = "q";

	public const string ENDPOINT_TTS_EVENTS = "viseme";

	public const string ENDPOINT_TTS_EVENT_EXTENSION = "v";

	public const string ENDPOINT_TTS_NO_CLIP = "No tts clip provided";

	public const string ENDPOINT_TTS_NO_TEXT = "No text provided";

	public const int ENDPOINT_TTS_CHANNELS = 1;

	public const int ENDPOINT_TTS_SAMPLE_RATE = 24000;

	public const float ENDPOINT_TTS_DEFAULT_READY_LENGTH = 1.5f;

	public const float ENDPOINT_TTS_DEFAULT_MAX_LENGTH = 15f;

	public const int ENDPOINT_TTS_DEFAULT_PRELOAD = 5;

	public const int ENDPOINT_TTS_BUFFER_LENGTH = 24000;

	public const int ENDPOINT_TTS_DEFAULT_SAMPLE_LENGTH = 720;

	public const int ENDPOINT_TTS_ERROR_MAX_LENGTH = 2400;

	public const int ENDPOINT_TTS_MAX_TEXT_LENGTH = 280;

	public const string ERROR_TTS_CACHE_DOWNLOAD = "Preloaded files cannot be downloaded at runtime. The file will be streamed instead. If you wish to download this file at runtime, use the temporary or permanent cache.";

	public const string ERROR_TTS_DECODE = "Data failed to encode";

	public const string ENDPOINT_DICTATION = "dictation";

	public const string ENDPOINT_COMPOSER_SPEECH = "converse";

	public const string ENDPOINT_COMPOSER_MESSAGE = "event";

	public const string ERROR_NO_TRANSCRIPTION = "Empty transcription.";

	public const string CANCEL_ERROR = "Cancelled";

	public const string CANCEL_MESSAGE_DEFAULT = "Request was cancelled.";

	public const string CANCEL_MESSAGE_PRE_SEND = "Request cancelled prior to transmission begin";

	public const string CANCEL_MESSAGE_PRE_AUDIO = "Request cancelled prior to audio transmission";

	public const int ERROR_CODE_GENERAL = -1;

	public const int ERROR_CODE_NO_CONFIGURATION = -2;

	public const int ERROR_CODE_NO_CLIENT_TOKEN = -3;

	public const int ERROR_CODE_NO_DATA_FROM_SERVER = -4;

	public const int ERROR_CODE_INVALID_DATA_FROM_SERVER = -5;

	public const int ERROR_CODE_ABORTED = -6;

	public const int ERROR_CODE_TIMEOUT = 14;

	public static string TTS_VOICE = "voice";

	public const string TTS_VOICE_DEFAULT = "Charlie";

	public static string TTS_STYLE = "style";

	public const string TTS_STYLE_DEFAULT = "default";

	public static string TTS_SPEED = "speed";

	public const int TTS_SPEED_DEFAULT = 100;

	public const int TTS_SPEED_MIN = 50;

	public const int TTS_SPEED_MAX = 200;

	public static string TTS_PITCH = "pitch";

	public const int TTS_PITCH_DEFAULT = 100;

	public const int TTS_PITCH_MIN = 25;

	public const int TTS_PITCH_MAX = 200;

	public const string TTS_EMPTY_ID = "EMPTY";

	public const TTSWitAudioType TTS_TYPE_DEFAULT = TTSWitAudioType.MPEG;

	public const string KEY_RESPONSE_TRANSCRIPTION = "text";

	public const string KEY_RESPONSE_TRANSCRIPTION_IS_FINAL = "is_final";

	public const string KEY_RESPONSE_NLP_INTENTS = "intents";

	public const string KEY_RESPONSE_NLP_ENTITIES = "entities";

	public const string KEY_RESPONSE_NLP_TRAITS = "traits";

	public const string KEY_RESPONSE_PARTIAL = "partial_response";

	public const string KEY_RESPONSE_FINAL = "response";

	public const string KEY_RESPONSE_ACTION = "action";

	public const string KEY_RESPONSE_IS_FINAL = "is_final";

	public const string KEY_RESPONSE_CODE = "code";

	public const string KEY_RESPONSE_ERROR = "error";

	public const string ERROR_RESPONSE_EMPTY_TRANSCRIPTION = "empty-transcription";

	public const string ERROR_RESPONSE_TIMEOUT = "timeout";

	public const int ERROR_CODE_SIMULATED = 500;

	public const string ERROR_RESPONSE_SIMULATED = "Simulated Server Error";

	public const string WIT_SOCKET_URL = "wss://api.wit.ai/composer";

	public const int WIT_SOCKET_CONNECT_TIMEOUT = 2000;

	public const int WIT_SOCKET_RECONNECT_ATTEMPTS = -1;

	public const float WIT_SOCKET_RECONNECT_INTERVAL = 1f;

	public const int WIT_SOCKET_RECONNECT_INTERVAL_MIN = 100;

	public const string WIT_SOCKET_REQUEST_ID_KEY = "client_request_id";

	public const string WIT_SOCKET_CLIENT_USER_ID_KEY = "client_user_id";

	public const string WIT_SOCKET_OPERATION_ID_KEY = "operation_id";

	public const string WIT_SOCKET_API_KEY = "api_version";

	public const string WIT_SOCKET_CONTENT_KEY = "content_type";

	public const int WIT_SOCKET_DISCONNECT_CODE = 499;

	public const string WIT_SOCKET_DISCONNECT_ERROR = "WebSocket disconnected";

	public const string WIT_SOCKET_AUTH_TOKEN = "wit_auth_token";

	public const string WIT_SOCKET_AUTH_RESPONSE_KEY = "success";

	public const string WIT_SOCKET_AUTH_RESPONSE_VAL = "true";

	public const string WIT_SOCKET_AUTH_RESPONSE_ERROR = "Authentication denied";

	public const string WIT_SOCKET_DATA_KEY = "data";

	public const string WIT_SOCKET_ACCEPT_KEY = "accept_header";

	public const string WIT_SOCKET_END_KEY = "end_stream";

	public const string WIT_SOCKET_ABORT_KEY = "abort";

	public const string WIT_SOCKET_TRANSCRIBE_KEY = "transcribe";

	public const string WIT_SOCKET_TRANSCRIBE_MULTIPLE_KEY = "multiple_segments";

	public const string WIT_SOCKET_TRANSCRIBE_IS_FINAL = "end_transcription";

	public const char WIT_SOCKET_PARAM_START = '[';

	public const char WIT_SOCKET_PARAM_END = ']';

	public const char WIT_SOCKET_PARAM_DELIM = ',';

	public const string WIT_SOCKET_EXTERNAL_ENDPOINT_KEY = "external";

	public const string WIT_SOCKET_EXTERNAL_UNKNOWN_CLIENT_USER_KEY = "unknown";

	public const string WIT_SOCKET_PUBSUB_SUBSCRIBE_KEY = "subscribe";

	public const string WIT_SOCKET_PUBSUB_UNSUBSCRIBE_KEY = "unsubscribe";

	public const string WIT_SOCKET_PUBSUB_TOPIC_KEY = "topic";

	public const string WIT_SOCKET_PUBSUB_TOPIC_TRANSCRIPTION_KEY = "_ASR";

	public const string WIT_SOCKET_PUBSUB_TOPIC_COMPOSER_KEY = "_COMP";

	public const string WIT_SOCKET_PUBSUB_PUBLISH_KEY = "publish_topics";

	public const string WIT_SOCKET_PUBSUB_PUBLISH_TRANSCRIPTION_KEY = "1";

	public const string WIT_SOCKET_PUBSUB_PUBLISH_COMPOSER_KEY = "2";

	public const string PARAM_OP_ID = "operationId";

	public const string PARAM_REQUEST_ID = "requestID";

	public const string PARAM_N_BEST_INTENTS = "nBestIntents";

	public static string GetUniqueId()
	{
		return $"{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}-{Guid.NewGuid()}";
	}
}
