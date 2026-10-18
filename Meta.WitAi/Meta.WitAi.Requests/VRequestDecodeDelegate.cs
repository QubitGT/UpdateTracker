using System.Threading.Tasks;
using UnityEngine.Networking;

namespace Meta.WitAi.Requests;

internal delegate Task<TValue> VRequestDecodeDelegate<TValue>(UnityWebRequest request);
