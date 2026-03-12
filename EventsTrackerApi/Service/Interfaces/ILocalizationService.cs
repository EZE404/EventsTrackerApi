namespace EventsTrackerApi.Service.Interfaces;

/// <summary>
/// Service for retrieving localized strings from resource files.
/// </summary>
public interface ILocalizationService
{
    /// <summary>
    /// Gets a localized string by key.
    /// </summary>
    /// <param name="key">The resource key.</param>
    /// <returns>The localized string.</returns>
    string GetString(string key);

    /// <summary>
    /// Gets a localized string with format arguments.
    /// </summary>
    /// <param name="key">The resource key.</param>
    /// <param name="args">Format arguments.</param>
    /// <returns>The formatted localized string.</returns>
    string GetFormattedString(string key, params object[] args);

    /// <summary>
    /// Gets the current culture.
    /// </summary>
    string CurrentCulture { get; }
}
