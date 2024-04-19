using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BracketBuilder.Auth;
using BracketBuilder.Database;
using BracketBuilder.Database;
using BracketBuilder.Models;
using BracketBuilder.Services.Interfaces;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Security.AccessControl;
using static MudBlazor.CategoryTypes;
using System.Security.Principal;

namespace BracketBuilder.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly DatabaseContext _db;

        public DatabaseService(DatabaseContext db)
        {
            _db = db;
        }

        #region account
        public async Task<string> Register(UserAccount account)
        {
            if (await _db.Accounts.FirstOrDefaultAsync(a => a.Username.ToLower() == account.Username.ToLower() || a.Email.ToLower() == account.Email.ToLower()) == null)
            {
                _db.Accounts.Add(account);
                _db.SaveChanges();
                return "success";
            }
            else if (await _db.Accounts.FirstOrDefaultAsync(a => a.Username.ToLower() == account.Username.ToLower()) != null)
                return $"Username \"{account.Username}\" is already taken.";
            else if (await _db.Accounts.FirstOrDefaultAsync(a => a.Email.ToLower() == account.Email.ToLower()) != null)
                return $"Email \"{account.Username}\" is already in use.";
            else return "unknown error";
        }
        public async Task<List<UserAccount>> GetUsersByFilter(int? id = null, string? username = null, string? displayname = null, string? role = null, string? Email = null, bool? verified = null)
        {
            return (await _db.Accounts.Where(a =>
                (id == null || a.Id == id) &&
                (username == null || a.Username.ToLower() == username.ToLower()) &&
                (displayname == null || a.DisplayName.Contains(displayname)) &&
                (role == null || a.Role == role) &&
                (Email == null || a.Email.ToLower() == Email.ToLower()) &&
                (verified == null || a.Verified == verified)
            ).ToListAsync());
        }
        public async Task<List<UserAccount>> GetUsersBySearchFilter(string? username = null, string? displayname = null, string? role = null, string? Email = null, string? bio = null, bool? verified = null)
        {
            return (await _db.Accounts.Where(a =>
                (    
                    (username == null || a.Username.ToLower().Contains(username.ToLower())) ||
                    (displayname == null || a.DisplayName.ToLower().Contains(displayname.ToLower())) ||
                    (role == null || a.Role.ToLower().Contains(role.ToLower())) ||
                    (bio == null || a.Bio.ToLower().Contains(bio.ToLower())) ||
                    (Email == null || a.Email.ToLower().Contains(Email.ToLower())) 
                )
                &&
                (verified == null || a.Verified == (bool)verified)
            ).ToListAsync());
        }
        public async Task UpdateUser(int id, string? password = null, string? username = null, string? displayname = null, string? role = null, string? Email = null, string? Identification = null, DateTime? VerifyRequest = null, bool? Verified = null, bool? Banned = null, Guid? VerifiedGuid = null)
        {

            var result = (await _db.Accounts.FirstOrDefaultAsync(a => a.Id == id));
            if (result != null) 
            {
                if (password != null)
                    result.Password = password;
                if (username != null)
                    result.Username = username;
                if (displayname != null)
                    result.DisplayName = displayname;
                if (Identification != null)
                    result.Identificaton = Identification;
                if (role != null)
                    result.Role = role;
                if (Email != null)
                    result.Email = Email;
                if (VerifyRequest != null)
                    result.VerifyRequest = (DateTime)VerifyRequest;
                if (Verified != null)
                    result.Verified = (bool)Verified;
                if (Banned != null)
                    result.Banned = (bool)Banned;
                if (VerifiedGuid != null)
                    result.VerifyGuid = (Guid)VerifiedGuid;
                _db.SaveChanges();
            }
        }
        public async Task UpdateUserByProperty(int id, string property, string value)
        {
            var result = await _db.Accounts.FirstOrDefaultAsync(a => a.Id == id);
            if (result != null)
            {
                var propertyInfo = result.GetType().GetProperty(property, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (propertyInfo != null && propertyInfo.CanWrite)
                {
                    var convertedValue = Convert.ChangeType(value, propertyInfo.PropertyType);
                    propertyInfo.SetValue(result, convertedValue);
                    _db.SaveChanges();
                }
                else
                {
                    // Handle property not found or not writeable
                    // For instance: throw new Exception("Property not found or not writeable");
                }
            }
        }
        public async Task DeleteUser(int id)
        {
            var result = (await _db.Accounts.FirstOrDefaultAsync(a => a.Id == id));
            _db.Accounts.Remove(result);
            _db.SaveChanges();
        }
        #endregion
    }
}
