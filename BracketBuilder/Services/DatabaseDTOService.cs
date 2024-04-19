using BracketBuilder.Database;
using BracketBuilder.Database.ModelsDTO;
using BracketBuilder.Models;
using BracketBuilder.Pages;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace BracketBuilder.Services
{
    public class DatabaseDTOService
    {
        private readonly DatabaseContext _db;

        public DatabaseDTOService(DatabaseContext db)
        {
            _db = db;
        }

        #region (C) Creating them in the database
        /// <summary>
        /// adds/uploads a tournament to the database
        /// </summary>
        /// <param name="obj">the tournament that you want to be on the database</param>
        /// <returns>nothin</returns>
        public async Task<Guid> AddTournament(Tournament obj)
        {
            //if the object is null itself we cant really do anything (should probaly change this to a exception)
            if (obj == null) return Guid.Empty;

            ////for each players make new couple with the player id and the id of this tournament
            //foreach (var player in obj.Players)
            //{
            //    await _db.TournamentPlayerCouples.AddAsync(new TournamentAccountCouple()
            //    {
            //        //Id = player.Id,
            //        TournamentId = obj.Id,
            //        UserAccountId = player.Id,
            //    });
            //}

            //add the final match (will add all other matches becase AddMatch is recursive)
            int finalId = await AddMatch(obj.Id, obj.FinalMatch);



            int firstId = obj.First == null ? -1 : obj.First.Id;
            int secondId = obj.Second == null ? -1 : obj.Second.Id;
            int thirdId = obj.Third == null ? -1 : obj.Third.Id;

            //add the tournament and save it
            var res = await _db.Tournaments.AddAsync(new TournamentDTO()
            {
                Id = obj.Id,
                Guid = obj.Guid,
                Name = obj.Name,
                AmountOfMatchesInPool = obj.AmountOfMatchesInPool,
                AmountOfPlayerInMatch = obj.AmountOfPlayerInMatch,
                AlwaysFit = obj.AlwaysFit,
                AmountOfStart = obj.AmountStart,
                CreatorId = obj.Creator.Id,
                FirstId = firstId,
                SecondId = secondId,
                ThirdId = thirdId,
                Finished = obj.Finished,
                FinalMatchId = finalId,
                Password = obj.Password,
                CreationDate = obj.CreationDate,
                StartingDate = obj.StartingDate,
                FinishDate = obj.FinishDate,
                Description = obj.Description,
                MaxPlayers = obj.MaxPlayers,
                isPublic = obj.isPublic,
            });
            await _db.SaveChangesAsync();


            await _db.TournamentAdminCouples.AddAsync(new TournamentAccountAdminCouple()
            {
                //Id = player.Id,
                TournamentId = res.Entity.Id,
                UserAccountId = res.Entity.CreatorId,
            });
            foreach (var admin in obj.Admins)
            {
                await _db.TournamentAdminCouples.AddAsync(new TournamentAccountAdminCouple()
                {
                    //Id = player.Id,
                    TournamentId = res.Entity.Id,
                    UserAccountId = admin.Id,
                });
            }

            foreach (var domain in obj.EmailDomains)
            {
                await _db.EmailTournamentCouples.AddAsync(new EmailTournamentCouple()
                {
                    //Id = player.Id,
                    TournamentId = res.Entity.Id,
                    Domain = domain,
                });
            }

            await _db.SaveChangesAsync();

            return res.Entity.Guid;
        }

        /// <summary>
        /// adds/uploads a match to the database
        /// </summary>
        /// <param name="obj">the match that you want on the database</param>
        /// <param name="tournamentId">the tournament that you want the match to be linked with in the database</param>
        /// <returns>the id of the match we just uploaded?? but if it was null then it returns -1</returns>
        public async Task<int> AddMatch(int tournamentId, Match obj)
        {
            if (obj == null)
                return -1;

            //declare nullable int for parent id's
            int? parent = null;
            List<int?> children = new List<int?>();

            //add it to get a location/id
            await _db.AddAsync(new MatchDTO()
            {
                TournamentId = tournamentId,
                Position = obj.Position,
                ParentId = null,
            });
            await _db.SaveChangesAsync();

            //get the last one
            var last = await _db.Matches.OrderBy(m => m.Id).LastOrDefaultAsync();
            if (last == null)
                throw new Exception("no matches found!");
            //update our id
            obj.Id = last.Id;

            //if it has a parent we try to resolve it (this  hasnt been tested yet)
            if (obj.Parent != null)
            {
                //we dont upload it here because that will cause an infinite loop
                var p = await _db.Matches.FirstOrDefaultAsync(m => m.Id == obj.Parent.Id);
                parent = p?.Id;
            }

            foreach (var child in obj.Children)
            {
                var id = await AddMatch(tournamentId, child);
                children.Add(id);
            }

            last.ParentId = parent;
            foreach (var childId in children)
            {
                await _db.MatchChildCouples.AddAsync(new MatchChildCouple()
                {
                    //Id = player.Id,
                    BelongToMatchId = obj.Id,
                    ChildId = childId,
                });
            }

            //for each competitor in the match we create a new couple with this match id and the current player id
            foreach (var player in obj.Competitors)
            {
                if (player.Account == null)
                    continue;
                await _db.MatchPlayerCouples.AddAsync(new MatchPlayerCouple()
                {
                    MatchId = obj.Id,
                    UserAccountId = player.Account.Id,
                    Score = player.Score,
                    Winner = player.Winner,
                });
                await _db.SaveChangesAsync();
                var lastPlayer = _db.MatchPlayerCouples.OrderBy(c => c.Id).LastOrDefault();
                player.Id = lastPlayer.Id;
            }
            await _db.SaveChangesAsync();
            return last.Id;
        }
        /// <summary>
        /// Create a link between the tournament and the participate player 
        /// </summary>
        /// <param name="tournament">the tournament in which the player is participating,</param>
        /// <param name="user">the player is participating</param>
        /// <returns></returns>
        public async Task CreateTournamentPlayerCouple(Tournament tournament, UserAccount user)
        {
            if (tournament != null && user != null)
            {
                await CreateTournamentPlayerCouple(tournament.Id, user.Id);
            }
        }
        public async Task CreateTournamentPlayerCouple(int tournamentId, int userId)
        {
            await _db.TournamentPlayerCouples.AddAsync(new TournamentAccountCouple()
            {
                TournamentId = tournamentId,
                UserAccountId = userId,
            });
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Create a link between the creator(Admin) and the tournament
        /// </summary>
        /// <param name="tournament">The tournament which is created by the admin</param>
        /// <param name="user">The admin that created the tournament</param>
        /// <returns></returns>
        public async Task CreateTournamentAdminCouple(Tournament tournament, UserAccount user)
        {
            if (tournament != null && user != null)
            {
                await _db.TournamentAdminCouples.AddAsync(new TournamentAccountAdminCouple()
                {
                    TournamentId = tournament.Id,
                    UserAccountId = user.Id,
                });
                await _db.SaveChangesAsync();
            }
        }
        /// <summary>
        /// create restriction domain for the tournament
        /// </summary>
        /// <param name="tournamentId">the tournament that needs a restriction domain</param>
        /// <param name="domain">the domain for the restriction</param>
        /// <returns></returns>
        public async Task CreateEmailTournamentCouple(int tournamentId, string domain)
        {
            await _db.EmailTournamentCouples.AddAsync(new EmailTournamentCouple()
            {
                TournamentId = tournamentId,
                Domain = domain,
            });
            await _db.SaveChangesAsync();
        }
        #endregion

        #region (R) Reading from database
        /// <summary>
        /// gets all tournaments from the database using filters, to apply filter simply use GetTournamentsByFilter(name:"mario",maxplayers:8);
        /// </summary>
        /// <param name="id">the id of the tournament (leave as null no not filter)</param>
        /// <param name="guid">the guid of the tournament (leave as null no not filter)</param>
        /// <param name="name">the name of the tournament (leave as null no not filter)</param>
        /// <param name="maxplayers">the max players of the tournament (leave as null no not filter)</param>
        /// <param name="ispublic">weather or not the tournament is public (leave as null no not filter)</param>
        /// <returns>a list of tournaments that match whatever you filtered by</returns>
        public async Task<List<Tournament>> GetTournamentsByFilter(int? id = null, int? creatorId = null, Guid? guid = null, string? name = null, int? maxplayers = null, bool? ispublic = null, int? FinalMatchId = null, bool? finished = null)
        {
            List<Tournament> tournaments = new List<Tournament>();

            //filter the tournaments based on what filters you want if you leave them as null they wont be filtered
            var querry = (await _db.Tournaments.Where(a =>
                (id == null || a.Id == id) &&
                (creatorId == null || a.CreatorId == creatorId) &&
                (guid == null || a.Guid == guid) &&
                (name == null || (a.Name.ToLower()).Contains(((string)name).ToLower())) && //a.Name.Contains(name.ToString(), StringComparison.OrdinalIgnoreCase)
                (maxplayers == null || a.MaxPlayers == maxplayers) &&
                (ispublic == null || a.isPublic == ispublic) &&
                (finished == null || a.Finished == finished) &&
                (FinalMatchId == null || a.FinalMatchId == FinalMatchId)
            ).OrderByDescending(t => t.CreationDate.Ticks).ToListAsync());


            foreach (var tournament in querry)
            {
                var t = await ResolveTournament(tournament);
                if (t != null)
                    tournaments.Add(t);
            }
            return tournaments;
        }
        /// <summary>
        /// Resolves a TournamentDTO (database version) to a Tournament 
        /// (with all the linked entities and stuff for easy access)
        /// </summary>
        /// <param name="tournament">The dto tournament</param>
        /// <returns>the resolved tournament or null if something happened</returns>
        private async Task<Tournament?> ResolveTournament(TournamentDTO tournament)
        {
            if (tournament == null) return null;

            //gets the creator based on its id
            var First = await _db.Accounts.FirstOrDefaultAsync(a => a.Id == tournament.FirstId); 
            var Second = await _db.Accounts.FirstOrDefaultAsync(a => a.Id == tournament.SecondId);
            var Third = await _db.Accounts.FirstOrDefaultAsync(a => a.Id == tournament.ThirdId);

            var creator = await _db.Accounts.FirstOrDefaultAsync(a => a.Id == tournament.CreatorId);
            if (creator == null) return null;

            //tries to get all the players that have joined this tournament
            var users = await GetPlayersByTournamentId((int)tournament.Id);
            List<Player> players = new List<Player>();

            foreach (var user in users)
            {
                //create a new player and add it
                players.Add(new Player()
                {
                    Id = -1,
                    Account = user,
                    Score = -1,
                    Winner = false,
                });
            }

            List<UserAccount> admins = await GetAdminsByTournamentId((int)tournament.Id);
            if (admins == null)
                admins = new List<UserAccount>() { creator };

            List<string> domains = new List<string>(); 
            List<EmailTournamentCouple> domainsDto = await GetEmailTournamentCouples((int)tournament.Id);
            if (domainsDto != null)
            {
                domains = domainsDto.Select(dto => dto.Domain).ToList();

                if (domains == null) domains = new List<string>();
            }

            //tries to get the final match by its id
            var finalmatch = await GetMatchById(tournament.FinalMatchId, players);

            //return the new linked tournament
            return new Tournament()
            {
                Id = tournament.Id,
                Guid = tournament.Guid,
                Name = tournament.Name,
                AmountOfMatchesInPool = tournament.AmountOfMatchesInPool,
                AmountOfPlayerInMatch = tournament.AmountOfPlayerInMatch,
                AmountStart = tournament.AmountOfStart,
                AlwaysFit = tournament.AlwaysFit,
                Creator = creator,
                FinalMatch = finalmatch,
                StartingDate = tournament.StartingDate,
                CreationDate = tournament.CreationDate,
                FinishDate = tournament.FinishDate,
                Description = tournament.Description,

                Finished = tournament.Finished,
                First = First,
                Second = Second,
                Third = Third,

                Password = tournament.Password,
                MaxPlayers = tournament.MaxPlayers,
                isPublic = tournament.isPublic,
                Players = users,
                Admins = admins,
                EmailDomains = domains,
            };
        }

        /// <summary>
        /// gets a match by its id
        /// </summary>
        /// <param name="matchId">the id of the match</param>
        /// <returns>the match if it was found or null if it wasnt found</returns>
        public async Task<Match?> GetMatchById(int? matchId, List<Player> PlayersLookup, int? parentId = null)
        {
            if (matchId == null) return null;

            //gets the matchDTO by the id
            MatchDTO? match = await _db.Matches.FirstOrDefaultAsync(t => t.Id == matchId);
            if (match == null) return null;

            var matchChildren = await _db.MatchChildCouples.Where(c => c.BelongToMatchId == matchId).ToListAsync();

            //tries to get every player in the match
            var competitors = await GetMatchCompetitorsByMatchId((int)matchId,PlayersLookup);
            if (competitors == null) return null;
            
            //create a match so it can be stored on the heap
            Match current = new Match();
            current.Children.Clear();

            Match? parent = null;
            if (parentId != null)
            {
                parent = await GetMatchById(match.ParentId, PlayersLookup);
            }

            foreach (var childCouple in matchChildren)
            {
                Match? child = await GetMatchById(childCouple.ChildId, PlayersLookup);
                if (child != null)
                {
                    child.Parent = current;
                    current.Children.Add(child);
                }
            }
            if(parent != null)
            {
                current.Parent = parent;
            }

            //everything should be linked up and ready to go
            //so set the final properties and return it

            current.Id = match.Id;
            current.Competitors = competitors;
            current.Position = match.Position;
            current.TournamentId= match.TournamentId;
            
            return current;

        }
        public async Task<List<Match>> GetMatchesByMatchPositionTournamentId(int Position, int tournamentId, List<Player> Players)
        {
            List<Match> matches = new List<Match>();
            //gets the matchDTO by the id
            List<MatchDTO>? matchesDTO = await _db.Matches.Where(t => t.Position == Position && t.TournamentId == tournamentId).ToListAsync();
            if (matchesDTO == null) return null;
            foreach (var matchDTO in matchesDTO)
            {
                var current = await GetMatchById(matchDTO.Id, Players,matchDTO.ParentId);
                matches.Add(current);
            }
            return matches;
        }

        /// <summary>
        /// gets all the players inside a match by the id of the match
        /// </summary>
        /// <param name="matchID">the id of the match</param>
        /// <returns>a list of all players that are playing in that match</returns>
        public async Task<List<Player>> GetMatchCompetitorsByMatchId(int matchID, List<Player> PlayersLookup)
        {
            List<Player> Competitors = new List<Player>();

            if (PlayersLookup.Count() == 0)
                return Competitors;

            //get all couples by the match id
            var couples = await _db.MatchPlayerCouples.Where(wp => wp.MatchId == matchID).ToListAsync();
            foreach (var couple in couples)
            {
                //get the player by the userid that we got off the couple
                var account = _db.Accounts.FirstOrDefault(a => a.Id == couple.UserAccountId);
                if (account == null)
                    continue;

                var player = PlayersLookup.FirstOrDefault(p => p.Account.Id == account.Id);
                if (player == null)
                    continue;

                //create a new player and add it
                player.Id = couple.Id;
                player.Position = couple.Position;
                player.Score = couple.Score;
                player.Winner = couple.Winner;

                Competitors.Add(player);
            }
            return Competitors;
        }

        /// <summary>
        /// gets all players that have joined a tournament by the id of the tournament
        /// </summary>
        /// <param name="tournamentId">the id of the tournament</param>
        /// <returns>a list of useraccounts that have joined the tournament</returns>
        public async Task<List<UserAccount>> GetPlayersByTournamentId(int tournamentId)
        {
            List<UserAccount> players = new List<UserAccount>();

            //get all tournament-account couple from the tournament id
            var couples = _db.TournamentPlayerCouples.Where(ta => ta.TournamentId == tournamentId);

            foreach (var couple in couples)
            {
                //get the accociated useraccount from the couple we just got
                var account = _db.Accounts.FirstOrDefault(a => a.Id == couple.UserAccountId);
                if (account != null)
                    players.Add(account);
            }

            return players;
        }

        /// <summary>
        /// Gets the link between Tournament and Account
        /// </summary>
        /// <param name="tournament">the tournament that is chosen</param>
        /// <param name="user">the user that participate in the tournament</param>
        /// <returns>link between tournament and Account</returns>
        public async Task<TournamentAccountCouple?> GetTournamentPlayerCouple(Tournament tournament, UserAccount user)
        {
            if (tournament != null && user != null)
            {
                var couple = await _db.TournamentPlayerCouples.FirstOrDefaultAsync(ta => ta.TournamentId == tournament.Id && ta.UserAccountId == user.Id);
                if (couple != null)
                {
                    return couple;
                }
                return null;
            }
            return null;
        }

        /// <summary>
        /// Get the Admin of the tournament
        /// </summary>
        /// <param name="tournamentId">The tournament that needs the admin information</param>
        /// <returns>List of user accounts</returns>
        public async Task<List<UserAccount>> GetAdminsByTournamentId(int tournamentId)
        {
            List<UserAccount> admins = new List<UserAccount>();

            //get all tournament-account couple from the tournament id
            var couples = _db.TournamentAdminCouples.Where(ta => ta.TournamentId == tournamentId);

            foreach (var couple in couples)
            {
                //get the accociated useraccount from the couple we just got
                var account = _db.Accounts.FirstOrDefault(a => a.Id == couple.UserAccountId);
                if (account != null)
                    admins.Add(account);
            }

            return admins;
        }


        public async Task<TournamentAccountAdminCouple?> GetTournamentAdminCouple(Tournament tournament, UserAccount user)
        {
            if (tournament != null && user != null)
            {
                var couple = await _db.TournamentAdminCouples.FirstOrDefaultAsync(ta => ta.TournamentId == tournament.Id && ta.UserAccountId == user.Id);
                if (couple != null)
                {
                    return couple;
                }
                return null;
            }
            return null;
        }

        public async Task<List<EmailTournamentCouple>> GetEmailTournamentCouples(int tournamentId)
        {
            var couples = await (_db.EmailTournamentCouples.Where(ta => ta.TournamentId == tournamentId).ToListAsync());
            if (couples != null)
            {
                return couples;
            }
            return null;
        }
        #endregion

        #region (U) Updating them in the database

        /// <summary>
        /// Update the tournament
        /// </summary>
        /// <param name="id">the Id of the tournament</param>
        /// <param name="tournament">the tournament that is needed to be updated</param>
        /// <param name="finalMatchId">the final match of the tournament</param>
        /// <returns></returns>
        public async Task UpdateTournament(int id, Tournament tournament, int? finalMatchId = null)
        {
            var result = (await _db.Tournaments.FirstOrDefaultAsync(t => t.Id == id));
            if (result != null)
            {
                if (finalMatchId != null)
                {
                    await DeleteMatchById(finalMatchId);
                    var matchId = await AddMatch(id, tournament.FinalMatch);

                    if (matchId != result.FinalMatchId)
                        result.FinalMatchId = matchId;
                }

                //remove all domains
                var domainCouples = _db.EmailTournamentCouples.Where(c => c.TournamentId == id);
                _db.EmailTournamentCouples.RemoveRange(domainCouples);

                //add all domains
                _db.EmailTournamentCouples.AddRange(tournament.EmailDomains.Select(e => new EmailTournamentCouple(id, e)));

                if (tournament.Finished != result.Finished)
                    result.Finished = tournament.Finished;

                if (tournament.First != null && tournament.First.Id != result.FirstId)
                    result.FirstId = tournament.First.Id;
                if (tournament.Second != null && tournament.Second.Id != result.SecondId)
                    result.SecondId = tournament.Second.Id;
                if (tournament.Third != null && tournament.Third.Id != result.ThirdId)
                    result.ThirdId = tournament.Third.Id;

                if (tournament.Name != result.Name)
                    result.Name = tournament.Name;

                if (tournament.Password != result.Password)
                    result.Password = tournament.Password;

                if (tournament.isPublic != result.isPublic)
                    result.isPublic = tournament.isPublic;

                if (tournament.StartingDate != result.StartingDate)
                    result.StartingDate = tournament.StartingDate;

                if (tournament.FinishDate != result.FinishDate)
                    result.FinishDate = tournament.FinishDate;

                if (tournament.Description != result.Description)
                    result.Description = tournament.Description;

                if (tournament.AmountOfMatchesInPool != result.AmountOfMatchesInPool)
                    result.AmountOfMatchesInPool = tournament.AmountOfMatchesInPool;

                if (tournament.AmountOfPlayerInMatch != result.AmountOfPlayerInMatch)
                    result.AmountOfPlayerInMatch = tournament.AmountOfPlayerInMatch;

                if (tournament.MaxPlayers != result.MaxPlayers)
                    result.MaxPlayers = tournament.MaxPlayers;

                if (tournament.AmountStart != result.AmountOfStart)
                    result.AmountOfStart = tournament.AmountStart;

                if (tournament.AlwaysFit != result.AlwaysFit)
                    result.AlwaysFit = tournament.AlwaysFit;

                _db.SaveChanges();
            }
        }
        /// <summary>
        /// Update the player
        /// </summary>
        /// <param name="id">Id of the player</param>
        /// <param name="player">the player that needs to be updated</param>
        /// <returns></returns>
        public async Task UpdateMatchPlayercouple(int id, Player player)
        {
            var result = (await _db.MatchPlayerCouples.FirstOrDefaultAsync(t => t.Id == id));
            if (result != null)
            {
                if (player.Winner != result.Winner)
                    result.Winner = player.Winner;

                if (player.Position != result.Position)
                    result.Position = player.Position;

                if (player.Score != result.Score)
                    result.Score = player.Score;

                _db.SaveChanges();
            }
        }

        /// <summary>
        /// Bulk update players
        /// </summary>
        /// <param name="players">List of the players that is needed to be updated</param>
        /// <returns></returns>
        public async Task BulkUpdateMatchPlayercouples(List<Player> players)
        {
            foreach (var player in players)
            {
                int id = player.Id;
                var result = (await _db.MatchPlayerCouples.FirstOrDefaultAsync(t => t.Id == id));
                var resultAccount = (await _db.Accounts.FirstOrDefaultAsync(t => t.Id == player.Account.Id));
                if (result != null && resultAccount != null)
                {
                    if (player.Account.AmoutOfWonMatches != resultAccount.AmoutOfWonMatches)
                        resultAccount.AmoutOfWonMatches = player.Account.AmoutOfWonMatches;

                    if (player.Account.AmountOfWonTournaments != resultAccount.AmountOfWonTournaments)
                        resultAccount.AmountOfWonTournaments = player.Account.AmountOfWonTournaments;

                    if (player.Account.AmoutOfTimesInFinal != resultAccount.AmoutOfTimesInFinal)
                        resultAccount.AmoutOfTimesInFinal = player.Account.AmoutOfTimesInFinal;

                    if (player.Account.AmountOfCompetedTournaments != resultAccount.AmountOfCompetedTournaments)
                        resultAccount.AmountOfCompetedTournaments = player.Account.AmountOfCompetedTournaments;


                    if (player.Winner != result.Winner)
                        result.Winner = player.Winner;

                    if (player.Position != result.Position)
                        result.Position = player.Position;

                    if (player.Score != result.Score)
                        result.Score = player.Score;
                }
            }
            _db.SaveChanges();
        }

        /// <summary>
        /// Updates a match by removing or adding the player based on if the next match has the same player
        /// </summary>
        /// <param name="PlayerId">the player to check for</param>
        /// <param name="match">the match to update</param>
        /// <returns></returns>
        public async Task UpdateMatch(int PlayerId, Match match)
        {
            if (match != null && match.Parent != null)
            {
                var play = match.Competitors.FirstOrDefault(p => p.Id == PlayerId);
                //if (play == null)
                //    throw new Exception("player doest exist");
                bool hasBeenAdded = (match.Parent.Competitors.FirstOrDefault(p => p.Account.Id == play?.Account?.Id) != null);
                if (hasBeenAdded) // player is null this parent does NOT have the player
                {
                    //var couples = await _db.MatchPlayerCouples.Where(p => p.MatchId == match.Parent.Id).ToListAsync();
                    var couple = await _db.MatchPlayerCouples.FirstOrDefaultAsync(p => p.MatchId == match.Parent.Id && p.UserAccountId == play.Account.Id);
                    if (couple != null)
                    { 
                        _db.MatchPlayerCouples.Remove(couple);
                        var oldCouple = await _db.MatchPlayerCouples.FirstOrDefaultAsync(p => p.MatchId == match.Id && p.UserAccountId == play.Account.Id);
                        if (oldCouple != null)
                        { 
                            oldCouple.Position = play.Position;
                            oldCouple.Winner = play.Winner;
                        }
                    }
                }
                else if (play != null)
                {
                    //update position
                    var oldCouple = await _db.MatchPlayerCouples.FirstOrDefaultAsync(p => p.MatchId == match.Id && p.UserAccountId == play.Account.Id);
                    if (oldCouple != null)
                        oldCouple.Position = play.Position;
                    await UpdateMatchPlayercouple(PlayerId, play);
                    // Player goes to the next Match
                    await _db.MatchPlayerCouples.AddAsync(new MatchPlayerCouple()
                    {
                        MatchId = match.Parent.Id,
                        UserAccountId = play.Account.Id,
                        Score = play.Score,
                        Position = play.Position,
                        Winner = play.Winner,
                    });
                }
                await _db.SaveChangesAsync();
            }
        }
        #endregion

        #region (D) Deleting them from the database

        /// <summary>
        /// Deletes the link between the tournament and the player
        /// </summary>
        /// <param name="tournament">tournament where the link is needed to be deleted</param>
        /// <param name="user">the player in the tournament that needs to be deleted</param>
        /// <returns></returns>
        public async Task DeleteTournamentPlayerCouple(Tournament tournament, UserAccount user)
        {
            if (tournament != null && user != null)
            {
                var couple = await GetTournamentPlayerCouple(tournament, user);
                if (couple != null)
                {
                    _db.TournamentPlayerCouples.Remove(couple);
                    await _db.SaveChangesAsync();
                }
            }
        }

        /// <summary>
        /// Delete the tournament admin
        /// </summary>
        /// <param name="tournament">the tournament where the admin needed to be deleted</param>
        /// <param name="user">the admin that needs to be deleted out of the tournament</param>
        /// <returns></returns>
        public async Task DeleteTournamentAdminCouple(Tournament tournament, UserAccount user)
        {
            if (tournament != null && user != null)
            {
                var couple = await GetTournamentAdminCouple(tournament, user);
                if (couple != null)
                {
                    _db.TournamentAdminCouples.Remove(couple);
                    await _db.SaveChangesAsync();
                }
            }
        }

        /// <summary>
        /// Delete the restricion of the tournament
        /// </summary>
        /// <param name="tournamentId">the tournament that the restriction needs to be delete</param>
        /// <param name="domain">the domain that is the restricion</param>
        /// <returns></returns>
        public async Task DeleteEmailTournamentCouple(int tournamentId, string domain)
        {
            var couple = await GetEmailTournamentCouples(tournamentId);
            if (couple != null)
            {
                _db.EmailTournamentCouples.RemoveRange(couple);
                await _db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Delete the match
        /// </summary>
        /// <param name="matchId">The match id that need to be deleted</param>
        /// <returns></returns>
        public async Task DeleteMatchById(int? matchId)
        {
            if (matchId == null) return;

            //gets the matchDTO by the id
            MatchDTO? match = await _db.Matches.FirstOrDefaultAsync(t => t.Id == matchId);
            if (match == null) return;

            var matchChildren = await _db.MatchChildCouples.Where(c => c.BelongToMatchId == matchId).ToListAsync();

            //get all the couples and delete them
            var couples = await _db.MatchPlayerCouples.Where(wp => wp.MatchId == matchId).ToListAsync();
            _db.MatchPlayerCouples.RemoveRange(couples);

            foreach (var child in matchChildren)
            {
                await DeleteMatchById(child.ChildId);
            }

            //delete the match itself
            _db.Matches.Remove(match);

            //save
            _db.SaveChanges();
        }
        
        public async Task DeleteMatchCouple(int matchId, int userAccountId)
        {
            if (matchId == null || userAccountId == null) return;

            var matchPlayerCouple = await _db.MatchPlayerCouples.Where(wp => wp.MatchId == matchId && wp.UserAccountId == userAccountId).ToListAsync();
            _db.MatchPlayerCouples.RemoveRange(matchPlayerCouple);
            _db.SaveChanges();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tournamentId">the tournament id</param>
        /// <param name="tournament">the tournament that needs to be deleted</param>
        /// <returns></returns>
        public async Task DeleteTournament(int tournamentId, Tournament tournament) 
        {
            //get all tournaments by the id
            TournamentDTO result = (await _db.Tournaments.FirstOrDefaultAsync(t => t.Id == tournamentId));
            if (result != null)
            {
                await DeleteMatchById(tournament?.FinalMatch?.Id);


                //remove players
                var playerCouples = _db.TournamentPlayerCouples.Where(ta => ta.TournamentId == tournamentId);
                _db.TournamentPlayerCouples.RemoveRange(playerCouples);
                //remove admins
                var adminCouples = _db.TournamentAdminCouples.Where(ta => ta.TournamentId == tournamentId);
                _db.TournamentAdminCouples.RemoveRange(adminCouples);
                //remove domains
                var domainCouples = _db.EmailTournamentCouples.Where(ta => ta.TournamentId == tournamentId);
                _db.EmailTournamentCouples.RemoveRange(domainCouples);

                _db.Tournaments.Remove(result);

                _db.SaveChanges();
            }
        }
        #endregion
    }
}