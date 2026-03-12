using SaveSystem.Models;

namespace SaveSystem.Serialization
{
    // Interface that defines the serialization mechanism for SaveEnvelope objects.
    // SaveEnvelope holds the encrypted data, initialization vector and MAC value.
    public interface ISaveEnvelopeSerializer
    {
        // Converts a SaveEnvelope object to a string format
        // so it can be written to a file.
        string Serialize(SaveEnvelope envelope);

        // Converts a string back into a SaveEnvelope object.
        SaveEnvelope Deserialize(string json);
    }
}
