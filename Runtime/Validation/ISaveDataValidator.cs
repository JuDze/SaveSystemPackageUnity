namespace SaveSystem.Validation
{
    // Interface for validating and sanitizing save data.
    // Called after loading or migrating data to ensure all values are valid.
    public interface ISaveDataValidator<TData>
    {
        TData Validate(TData data);
    }
}
