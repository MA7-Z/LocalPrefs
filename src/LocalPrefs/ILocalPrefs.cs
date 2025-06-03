namespace AndanteTribe.IO;

/// <summary>
/// Interface for managing local preferences/settings storage.
/// Provides methods to load, save, and delete local data.
/// </summary>
public interface ILocalPrefs
{
    /// <summary>
    /// Loads a value of the specified type from local storage.
    /// </summary>
    /// <typeparam name="T">The type of data to load.</typeparam>
    /// <param name="key">The unique identifier for the stored data.</param>
    /// <returns>The loaded value, or default value if not found.</returns>
    T? Load<T>(string key);

    /// <summary>
    /// Asynchronously saves a value to local storage.
    /// </summary>
    /// <typeparam name="T">The type of data to save.</typeparam>
    /// <param name="key">The unique identifier for storing the data.</param>
    /// <param name="value">The value to save.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    ValueTask SaveAsync<T>(string key, T value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously deletes data with the specified key from local storage.
    /// </summary>
    /// <param name="key">The unique identifier of the data to delete.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    ValueTask DeleteAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously deletes all stored local data.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous delete all operation.</returns>
    ValueTask DeleteAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether data exists for the specified key.
    /// </summary>
    /// <param name="key">The key to check for existence.</param>
    /// <returns><c>true</c> if data exists for the specified key; otherwise, <c>false</c>.</returns>
    bool HasKey(string key);
}