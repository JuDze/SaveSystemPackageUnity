using UnityEngine;

namespace SaveSystem.Serialization
{
    // JSON serialization implementation using Unity's JsonUtility.
    // Converts an object to JSON text and restores it from JSON.
    public class JsonSaveSerializer<TData> : ISaveSerializer<TData>
    {
        // Serializes an object to JSON format
        public string Serialize(TData data)
        {
            return JsonUtility.ToJson(data, true);
        }

        // Deserializes JSON text back into an object
        public TData Deserialize(string json)
        {
            return JsonUtility.FromJson<TData>(json);
        }
    }
}
