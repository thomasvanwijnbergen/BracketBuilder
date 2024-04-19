using BracketBuilder.Auth;
using BracketBuilder.Models;

namespace BracketBuilder.Database;

public static class DefaultData
{
    public static void Initialize(DatabaseContext db)
    {
        if(db.Accounts.Count() == 0 || db.Accounts == null)
        {
            var Admin = new UserAccount()
            {
                Id= 1,
                DisplayName = "admin",
                Username = "admin",
                Password = "admin",
                Email = "admin@example.com",
                Verified = true,
                Role = UserAccount.Admin,
            };
            db.Accounts.AddRange(Admin);
#if DEBUG
            var defaultAccounts = new List<UserAccount>()
            {
                new(2,"Daan", true) { Role = UserAccount.Moderator },
                new(3,"Eva", true),
                new(4,"Milan", true),
                new(5,"Sophie", true),
                new(6,"Bram", true) { Role = UserAccount.Moderator },
                new(7,"Lotte", true),
                new(8,"Luuk", true),
                new(9,"Noa", true) { Role = UserAccount.Moderator },
                new(10,"Finn", true),
                new(11,"Fleur", true),
                new(12,"Sem", true) {Email = "Sem@gmail.com" },
                new(16,"Sara", true) { Email = "Sara@gmail.com"},
                new(13,"Thomas", true){ Role = UserAccount.Moderator, Email = "Thomas@gmail.com"},
                new(14,"Julia", true),
                new(15,"Haspers", false) { Banned = true },
            };

            db.Accounts.AddRange(defaultAccounts);

#endif
            //Tournament tour = new Tournament()
            //{
            //    Creator = Admin,
            //    MaxPlayers = 8,
            //    Password = "",
            //    isPublic = true,
            //    Guid = Guid.Empty,
            //    FinalMatch = null,
            //    Date = DateTime.Now,
            //};
            //db.Tournaments.Add(tour);
            db.SaveChanges();
        }
        //for (int i = 0; i < 128; i++)
        //{
        //    Accounts.Add(new UserAccount()
        //    {
        //        DisplayName = "user " + i,
        //        Username = "user " + i,
        //        Password = "user",
        //        Email = "user@example.com",
        //        Role = "User"
        //    });
        //}
        

    }
}
