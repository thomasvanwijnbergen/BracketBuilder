
using Microsoft.EntityFrameworkCore;
using BracketBuilder.Auth;
using BracketBuilder.Models;
using System.Collections.Generic;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Microsoft.Extensions.Hosting;
using BracketBuilder.Database.ModelsDTO;

namespace BracketBuilder.Database;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions options) : base(options) { }

    public DbSet<UserAccount> Accounts { get; set; }

    public DbSet<TournamentDTO> Tournaments { get; set; }
    public DbSet<MatchDTO> Matches { get; set; }

    public DbSet<MatchPlayerCouple> MatchPlayerCouples { get; set; }
    public DbSet<TournamentAccountCouple> TournamentPlayerCouples { get; set; }
    public DbSet<TournamentAccountAdminCouple> TournamentAdminCouples { get; set; }
    public DbSet<MatchChildCouple> MatchChildCouples { get; set; }
    public DbSet<EmailTournamentCouple> EmailTournamentCouples { get; set; }
    //protected override void OnModelCreating(ModelBuilder modelBuilder)
    //{
    //    //#region Keys
    //    //modelBuilder.Entity<Tournament>()
    //    //    .HasKey(x => x.Id);
    //    //modelBuilder.Entity<Player>()
    //    //    .HasKey(x => x.Id);
    //    //modelBuilder.Entity<Match>()
    //    //    .HasKey(x => x.Id);
    //    //modelBuilder.Entity<UserAccount>()
    //    //    .HasKey(x => x.Id);
    //    //#endregion

    //    //#region Auto increment
    //    //modelBuilder.Entity<Tournament>()
    //    //    .Property(t => t.Id).ValueGeneratedOnAdd();
    //    //modelBuilder.Entity<UserAccount>()
    //    //    .Property(U => U.Id).ValueGeneratedOnAdd();
    //    //modelBuilder.Entity<Player>()
    //    //    .Property(t => t.Id).ValueGeneratedOnAdd();
    //    //modelBuilder.Entity<Match>()
    //    //    .Property(p => p.Id).ValueGeneratedOnAdd();
    //    //#endregion

    //    #region Relations
    //    modelBuilder.Entity<Tournament>()
    //        .HasOne(t => t.Creator)
    //        .WithMany(u => u.Tournaments)
    //        .HasForeignKey(t => t.CreatorId);

    //    modelBuilder.Entity<Match>()
    //        .HasOne(m => m.Parent)
    //        .WithMany()
    //        .HasForeignKey(m => m.ParentId);

    //    modelBuilder.Entity<Match>()
    //        .HasOne(m => m.LeftChild)
    //        .WithMany()
    //        .HasForeignKey(m => m.LeftChildId);

    //    modelBuilder.Entity<Match>()
    //        .HasOne(m => m.RightChild)
    //        .WithMany()
    //        .HasForeignKey(m => m.RightChildId);

    //    #endregion
    //    base.OnModelCreating(modelBuilder);
    //}
}
