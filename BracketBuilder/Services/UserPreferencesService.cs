using Microsoft.JSInterop;
using BracketBuilder.Services.Interfaces;

namespace BracketBuilder.Services;

public class UserPreferencesService : IUserPreferencesService
{
    private const string ThemePreferenceKey = "theme";

    private readonly IJSRuntime _jsRuntime;

    public UserPreferencesService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<bool> ExistsThemePreference()
    {
        var theme = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", ThemePreferenceKey);
        return !string.IsNullOrEmpty(theme);
    }
    public async Task<string> GetCurrentThemePreference()
    {
        var theme = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", ThemePreferenceKey);
        return string.IsNullOrEmpty(theme) ? "defaultTheme" : theme;
    }
    public async Task SetCurrentThemePreference(string theme)
    {
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", ThemePreferenceKey, theme);
    }
}
