namespace SaveSystem.Versioning
{
    // Interface for a single version migration step.
    // Implement one class per version transition (e.g. V1 -> V2).
    public interface ISaveMigration<TData> where TData : class
    {
        int FromVersion { get; }
        int ToVersion   { get; }

        // Transforms data from the old version format to the new one
        TData Migrate(TData oldData);
    }
}
