namespace SaveSystem.Storage
{
    // Defines the core operations for working with a save file:
    // writing, reading, existence check and deletion.
    // This abstraction allows different storage backends
    // (e.g. local file system, cloud storage, database)
    // to be used without changing any other system logic.
    public interface IStorageService
    {
        // Writes text content to the storage location
        void SaveText(string content);

        // Reads and returns text content from the storage location
        string LoadText();

        // Returns true if save data exists at the storage location
        bool Exists();

        // Deletes the save data from the storage location
        void Delete();

        // Returns the full path to the save file
        string GetPath();

        // Loads the backup save data as text
        string LoadBackupText();

        // Returns true if a backup copy exists
        bool BackupExists();
    }
}
