using System.Globalization;
using System.Resources;
using EventsTrackerApi.Service.Interfaces;

namespace EventsTrackerApi.Service;

/// <summary>
/// Implementation of localization service using ResourceManager.
/// </summary>
public class LocalizationService : ILocalizationService
{
    private readonly ResourceManager _resourceManager;
    private readonly CultureInfo _culture;

    /// <summary>
    /// Initializes a new instance of the LocalizationService.
    /// </summary>
    public LocalizationService()
    {
        _resourceManager = new ResourceManager("EventsTrackerApi.Resources.Strings", typeof(LocalizationService).Assembly);
        _culture = CultureInfo.CurrentUICulture;
    }

    /// <inheritdoc />
    public string GetString(string key)
    {
        try
        {
            return _resourceManager.GetString(key, _culture) ?? key;
        }
        catch
        {
            return key;
        }
    }

    /// <inheritdoc />
    public string GetFormattedString(string key, params object[] args)
    {
        var format = GetString(key);
        try
        {
            return string.Format(format, args);
        }
        catch
        {
            return format;
        }
    }

    /// <inheritdoc />
    public string CurrentCulture => _culture.Name;
}
