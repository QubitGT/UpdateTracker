using System.Collections.Generic;

namespace UnityEngine.Formats.Fbx.Exporter;

internal delegate void HandleUpdate(FbxPrefab updatedInstance, IEnumerable<GameObject> updatedObjects);
