namespace SaveSystem.Serialization
{
    // Generic interface for data serialization and deserialization.
    // Converts an object to a text format and restores it from that text.
    public interface ISaveSerializer<TData>
    {
        // Serializes an object to a string so it can be saved or transmitted.
        string Serialize(TData data);

        // Deserializes a string back into an object.
        TData Deserialize(string json);
    }
}
