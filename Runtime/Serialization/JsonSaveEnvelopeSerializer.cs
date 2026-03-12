using SaveSystem.Models;
using UnityEngine;

namespace SaveSystem.Serialization
{
    // Provides JSON serialization for SaveEnvelope objects using Unity's JsonUtility.
    // Used to persist the encrypted data container and restore it on load.
    public class JsonSaveEnvelopeSerializer : ISaveEnvelopeSerializer
    {
        // Converts a SaveEnvelope to JSON.
        // prettyPrint: true — formats the JSON for human readability.
        public string Serialize(SaveEnvelope envelope)
        {
            return JsonUtility.ToJson(envelope, true);
        }

        // Converts JSON back into a SaveEnvelope object.
        public SaveEnvelope Deserialize(string json)
        {
            return JsonUtility.FromJson<SaveEnvelope>(json);
        }
    }
}
