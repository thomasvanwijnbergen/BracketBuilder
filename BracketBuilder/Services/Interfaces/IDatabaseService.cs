using BracketBuilder.Models;

namespace BracketBuilder.Services.Interfaces;

public interface IDatabaseService
{
    #region accounts
    public Task<string> Register(UserAccount account);
    public Task<List<UserAccount>> GetUsersByFilter(int? id = null, string? username = null, string? displayname = null, string? role = null, string? Email = null, bool? verified = null);
    public Task<List<UserAccount>> GetUsersBySearchFilter(string? username = null, string? displayname = null, string? role = null, string? Email = null, string? bio = null, bool? verified = null);
    public Task UpdateUser(int id, string? password = null, string? username = null, string? displayname = null, string? role = null, string? Email = null, string? Identification = null, DateTime? VerifyRequest = null, bool? Verified = null, bool? Banned = null, Guid? VerifiedGuid = null);
    public Task UpdateUserByProperty(int id, string property, string value);
    public Task DeleteUser(int id);
    #endregion
}
