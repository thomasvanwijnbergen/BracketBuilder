namespace BracketBuilder.Services.Interfaces;

public interface IUserPreferencesService
{
    public Task<bool> ExistsThemePreference();
    public Task<string> GetCurrentThemePreference();
    public Task SetCurrentThemePreference(string theme);
}
