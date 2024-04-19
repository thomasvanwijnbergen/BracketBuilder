using BracketBuilder.Shared.Layouts;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Web;

namespace BracketBuilder.Models
{
    [DebuggerDisplay("{Id}:{Username}")]
    public class UserAccount
    {
        public UserAccount() { }
        public UserAccount(int id,string name, bool verified, string? password = null, bool randomSeeding = true) 
        {
            Id = id;
            DisplayName = name;
            Username = name;
            Email = $"{name}@example.com";
            Verified = verified;
            if (password == null)
                Password = name;
            else
                Password = password;

            if (randomSeeding)
            {
                AmoutOfTimesInFinal = Random.Shared.Next(0,100);
                AmoutOfWonMatches = Random.Shared.Next(0,100);
                AmountOfWonTournaments = Random.Shared.Next(0,100);
                AmountOfCompetedTournaments = Random.Shared.Next(0,100);
                BaseSeedingScore = Random.Shared.Next(0,100);
            }
        }
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public string Username { get; set; }
        public string HashedPassword { get; set; }
        public string Role { get; set; } = UserAccount.User;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? Identificaton { get; set; }
        public string Bio { get; set; } = "Hello";
        public string ProfilePicture { get; set; } = "";
        public string Email { get; set; }
        public bool Verified { get; set; }
        public bool Banned { get; set; }
        public Guid VerifyGuid { get; set; }
        public DateTime VerifyRequest { get; set; } = DateTime.MinValue;
        public int AmoutOfTimesInFinal { get; set; } //= Random.Shared.Next(1,100);
        public int AmoutOfWonMatches { get; set; } //= Random.Shared.Next(1,100);
        public int AmountOfWonTournaments { get; set; }
        public int AmountOfCompetedTournaments { get; set; }
        public int BaseSeedingScore { get; set; } //only visible to admins, shouln't ruin a profile

        [NotMapped]
        public const string ServerEmail = "Bracket@Builder.info";


        [NotMapped]
        private const string EmailUrl = "https://polydev.nl/Dev/Mail/request.php";

        [NotMapped]
        public const string Admin = "Administrator";
        [NotMapped]
        public const string Moderator = "Moderator";
        [NotMapped]
        public const string Access = Admin + "," + Moderator;
        [NotMapped]
        public const string User = "User";

        [NotMapped]
        public string Password 
        {
            get => HashedPassword;
            set => HashedPassword = Hash(value,Salt);
        }
        // Gets loaded by program.cs
        public static string Salt;

        public bool HasAccess => (Role == Admin || Role == Moderator);

        public async Task SendMail(string subject, string message) 
        {
            //string encode(string input) => NavigationMenu.Base64Encode(input);
            string encode(string input) => HttpUtility.UrlEncode(input);

            string url = $"{EmailUrl}?to={encode(Email)}&subject={encode(subject)}&message={encode(message)}&from={encode(ServerEmail)}";
            await Request(url);
        }

        private async Task<string> Request(string url)
        {
            HttpClient webClient = new HttpClient();
            try
            {
                string output = await webClient.GetStringAsync(url);
                return output;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "404 error";
            }
        }


        public static string Hash(string text, string salt = "")
        {
            if (String.IsNullOrEmpty(text))
            {
                return String.Empty;
            }

            // Uses SHA256 to create the hash
            using (var sha = new System.Security.Cryptography.SHA256Managed())
            {
                // Convert the string to a byte array first, to be processed
                byte[] textBytes = System.Text.Encoding.UTF8.GetBytes(text + salt);
                byte[] hashBytes = sha.ComputeHash(textBytes);

                // Convert back to a string, removing the '-' that BitConverter adds
                string hash = BitConverter
                    .ToString(hashBytes)
                    .Replace("-", String.Empty);

                return hash;
            }
        }
    }
}
