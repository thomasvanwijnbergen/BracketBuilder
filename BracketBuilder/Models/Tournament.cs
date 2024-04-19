using Microsoft.AspNetCore.Mvc.ViewFeatures;
using MudBlazor;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.Design;
using System.Runtime.CompilerServices;

namespace BracketBuilder.Models;

public class Tournament
{ 
    public int Id { get; set; }
    public Guid Guid { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "Tournament";
    public UserAccount Creator { get; set; }
    public Match? FinalMatch { get; set; } = null;
    public DateTime CreationDate { get; set; } = DateTime.Now;
    public DateTime? StartingDate { get; set; } = null;
    public DateTime? FinishDate { get; set; } = null;
    public string? Description { get; set; }

    public int MaxPlayers { get; set; }
    public string Password { get; set; }
    public bool isPublic { get; set; }
    public List<UserAccount> Players { get; set; } = new List<UserAccount>();
    public List<UserAccount> Admins { get; set; } = new List<UserAccount>();
    public List<string> EmailDomains { get; set; } = new List<string>();
    public bool Finished { get; set; } = false;
    public UserAccount First { get; set; } = null;
    public UserAccount Second { get; set; } = null;
    public UserAccount Third { get; set; } = null;
    public int AmountOfMatchesInPool { get; set; } = 2;
    public int AmountOfPlayerInMatch { get; set; } = 4;
    public int AmountStart { get; set; } = 0;
    public bool AlwaysFit { get; set; } = true;

    public readonly static List<int> PossibleStartingToNextRound = new List<int>() { 2,3,4 };

    private static readonly int FinalReachedWheight = 5;

    public string EmailDomainsString
    {
        get 
        {
            return string.Join("\n", EmailDomains);
        }

        set
        {
            if (value != null)
            {
                EmailDomains.Clear();
                foreach (string domain in value.Split("\n"))
                {
                    if (string.IsNullOrWhiteSpace(domain))
                        continue;

                    EmailDomains.Add(domain.Trim());
                }
            }
        }
    }

    public bool HasAdmin(UserAccount? user) 
    {
        if (user == null)
            return false;

        return Admins.Any(a => a.Id == user.Id);
    }

    public bool EmailAllowed(UserAccount? user)
    {
        if (user == null)
            return false;

        if (EmailDomains.Count == 0)
            return true;

        return EmailDomains.Any(d => user.Email.EndsWith(d,StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Creates a tournament from the given players
    /// </summary>
    /// <param name="players">List of Player</param>
    /// <returns>Completly linked tournament</returns>
    public void Build(bool automaticStarting, bool automaticGoingTonextRound)
    {
        const int maxLimits = 512; // this is the limit of the tournament view show
        
        //int[] veelvoud = new int[] { /*2,*/ 4, 8, 16, 32, 64, 128, 256, 512, 1024 };
        //randomize the player list for fair and new brackets
        //players = players.OrderBy(p => Random.Shared.Next()).ToList();
        int playercount = Players.Count();
        
        if (playercount > maxLimits)
        {
            throw new Exception($"There is a limit of {maxLimits} players that is may enter a tournament");
        }

        #region calculating configuration
        int amountStart = AmountStart; //start at final match
        int amountOfMatchesInPool = AmountOfMatchesInPool; //default it at 2

        if (automaticStarting && automaticGoingTonextRound)
            CalculateMatchesFast(playercount, AmountOfPlayerInMatch, AlwaysFit, ref amountStart, ref amountOfMatchesInPool);
        else
            CalculateMatches(playercount, AmountOfPlayerInMatch, AlwaysFit, automaticStarting, automaticGoingTonextRound, ref amountStart, ref amountOfMatchesInPool);
        
        AmountOfMatchesInPool = amountOfMatchesInPool;
        AmountStart = amountStart;
        #endregion

        //all matches wich still needs to be linked
        List<Match> UnstableMatches = new List<Match>();

        //the ones that are done being linked
        List<Match> StableMatches = new List<Match>();

        //generate the starting matches
        for (int i = 0; i < AmountStart; i++)
        {
            UnstableMatches.Add(new Match() { Position = 1});
        }

        
        //seed this list with players by sorting them from best to worst and filling them in in a spiral pattern
        List<Player> seededPlayers = SeedPlayerList(this.Players);

        //fill all unstablematches with the players in a spiral pattern
        SpiralFill(ref UnstableMatches, ref seededPlayers);

        //build up the tournament tree by linking the childeren to the parents until you are left with one final match
        LinkMatches(ref UnstableMatches, ref StableMatches);


        //that last match has all of the childeren
        FinalMatch = UnstableMatches.First();
    }

    public static int CalculateSeedingScore(UserAccount account)
    {
        int seedingScore = 0;

        //apply base seeding
        seedingScore += account.BaseSeedingScore;

        //add the amount of times they won a match
        seedingScore += account.AmoutOfWonMatches;

        //the amount of times they reached the finals times how important that is (the wheight)
        seedingScore += (account.AmoutOfTimesInFinal * FinalReachedWheight);

        return seedingScore;
    }

    /// <summary>
    /// Used for seeding
    /// lets say we have 4 starting matches, and the players with the scores 55 66 77 88
    /// use this method we caluclate the seeding table
    /// 0321
    /// 021
    /// 88 44 
    /// 55 22 
    /// 66 33 
    /// 77
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    private List<int> GetSeedingTable(int amount)
    {
        List<int> SeedingTable = new List<int>();
        for (int i = 0; i < amount; i += 2)
        {
            int middle = (SeedingTable.Count() / 2);

            SeedingTable.Insert(middle, i);
            if (SeedingTable.Count < amount)
                SeedingTable.Insert(middle, i + 1);
        }
        SeedingTable.Reverse();
        return SeedingTable;
    }
    /// <summary>
    /// Seed the accounts into players sorted by the amount of starting matches
    /// </summary>
    /// <param name="accounts"></param>
    /// <returns></returns>
    private List<Player> SeedPlayerList(List<UserAccount> accounts)
    {
        //Sort the list from best to worst
        List<UserAccount> sortedAccounts = accounts.OrderByDescending(p => CalculateSeedingScore(p)).ToList();
        
        //calculate the seeding table given the amount of starting matches we have
        List<int> seedingTable = GetSeedingTable(AmountStart);
        
        //get the amount of iterations we need to do for a given set of players
        int count = (int)Math.Ceiling((double)sortedAccounts.Count() / (double)AmountStart);

        List<Player> outputPlayers = new List<Player>();

        //reorder the players so that when filled in a spiral pattern itll have the correct seeding
        for (int i = 0; i < count; i++)
        {
            if (sortedAccounts.Count() == 0) break;

            //for every starting match we get the top {startingmatch} best players
            var list = sortedAccounts.Take(AmountStart).ToList();

            //get the amount of players we just grabbed to see if we still have enough
            int listCount = list.Count();
            if (listCount != seedingTable.Count()) //if we dont have enough, recalculate the seeding table
                seedingTable = GetSeedingTable(listCount);

            //re-order the players acording to the seeding table
            foreach (var index in seedingTable)
            {
                //use the account and make a new player
                outputPlayers.Add(new Player()
                {
                    Account = list[index],
                    Position = 1,
                });
            }

            //remove the players from the old list
            foreach (var item in list)
            {
                sortedAccounts.Remove(item);
            }
        }
        return outputPlayers;
    }


    private void SpiralFill(ref List<Match> UnstableMatches, ref List<Player> seededPlayers)
    {
        //fill the starting matching in a spiral pattern
        for (int j = 0; j < AmountOfPlayerInMatch; j++) //for each space in a match of 3
        {
            for (int i = 0; i < UnstableMatches.Count(); i++) //for each match
            {
                Match match = UnstableMatches[i];
                if (seededPlayers.Count != 0) //if there are still players left to divide
                {
                    match.Competitors.AddRange(seededPlayers.Take(1).ToList());
                    seededPlayers = seededPlayers.Skip(1).ToList();
                }
            }
        }
    }

    private void LinkMatches(ref List<Match> UnstableMatches, ref List<Match> StableMatches)
    {
        //link all starting matches and redo this until we have one match left which contains all of the childerens
        while (UnstableMatches.Count > 1)
        {
            //link the matches by making a new match which has the two previous matches and uses them as left and right childeren
            BuildMatchTree(ref UnstableMatches, ref StableMatches);
            UnstableMatches = new List<Match>(StableMatches);
            StableMatches.Clear();
        }
    }


    // just gonna brute force
    /// <summary>
    /// start at final match (1)
    /// times 2
    /// enough?
    /// great!
    /// otherwise times 3
    /// enough?
    /// great!
    /// otherwise times 4
    /// enough?
    /// great!
    /// otherwise time 2 and then next cycle
    /// 
    /// 
    /// </summary>
    /// <param name="totalPlayers"></param>
    /// <param name="maxPlayersPerMatch"></param>
    /// <param name="startingMatches"></param>
    /// <param name="startingMatchesToNextRound"></param>
    public static void CalculateMatches(int totalPlayers, int maxPlayersPerMatch, bool alwaysFit, bool automaticStarting, bool automaticGoingToNext, ref int startingMatches, ref int startingMatchesToNextRound)
    {
        int origionalStartingAmount = startingMatches;
        int origionalGoingToNextAmount = startingMatchesToNextRound;

        startingMatches = 1;
        startingMatchesToNextRound = 2;

        //if amount play players we could get with this configuration is not enough
        int amountOfPlayersPossible = (startingMatches * maxPlayersPerMatch);

        while (amountOfPlayersPossible < totalPlayers)
        {
            //try each of the 3 starting matches
            foreach (int StartingToNextRound in Tournament.PossibleStartingToNextRound)
            {
                if (alwaysFit && (StartingToNextRound > maxPlayersPerMatch)) continue;
                //are they enough?
                //great lets apply them and break out
                int starting = (startingMatches * StartingToNextRound);
                int newAmountOfPlayersPossible = (starting * maxPlayersPerMatch);

                if (newAmountOfPlayersPossible >= totalPlayers)
                {
                    if (!automaticStarting && starting < origionalStartingAmount)
                        continue;
                    if (!automaticGoingToNext && StartingToNextRound != origionalGoingToNextAmount)
                        continue;
                    startingMatches = starting;
                    startingMatchesToNextRound = StartingToNextRound;
                    return;
                }
            }
            //they arent? lets just go with default of 2 and move to the next round ans try our luck there
            startingMatches *= 2;
            amountOfPlayersPossible = (startingMatches * maxPlayersPerMatch);
        }
    }

    public static void CalculateMatchesFast(int totalPlayers, int maxPlayersPerMatch, bool alwaysFit, ref int startingMatches, ref int startingMatchesToNextRound)
    {
        startingMatches = 1;
        startingMatchesToNextRound = 2;

        //if amount play players we could get with this configuration is not enough
        while ((startingMatches * maxPlayersPerMatch) < totalPlayers)
        {
            //try each of the 3 starting matches
            foreach (int StartingToNextRound in Tournament.PossibleStartingToNextRound)
            {
                if (alwaysFit && (StartingToNextRound > maxPlayersPerMatch)) continue;

                //are they enough?
                //great lets apply them and break out
                if (((startingMatches * StartingToNextRound) * maxPlayersPerMatch) >= totalPlayers)
                {
                    startingMatches *= StartingToNextRound;
                    startingMatchesToNextRound = StartingToNextRound;
                    return;
                }
            }
            //they arent? lets just go with default of 2 and move to the next round ans try our luck there
            startingMatches *= 2;
        }
    }


    /// <summary>
    /// Link the matches by making a new match which has the two previous matches and uses them as left and right childeren
    /// </summary>
    /// <param name="unstableMatches">All matches wich still needs to be linked</param>
    /// <param name="stableMatches">The ones that are done being linked</param>
    private void BuildMatchTree(ref List<Match> unstableMatches, ref List<Match> stableMatches)
    {
        int amountOfMatches = 2;
        //still need to do something here to set amountOfMatches to this.AmountOfMatchesInPool if itll be the first round

        //if this is the first time
        if (unstableMatches.Any(m => m.Children.Count() == 0)) 
        {
            amountOfMatches = AmountOfMatchesInPool;
        }

        int iterations = (unstableMatches.Count() / amountOfMatches);
        //for each 2 matches that still need to be linked
        for (int i = 0; i < iterations; i++)
        {
            //make a new match
            Match match = new Match();

            //link the childeren to the new match
            match.Children.AddRange(unstableMatches.Take(amountOfMatches));

            //fix the parents of the childeren (this works because in c# classes are stored on the heap and they are pretty much always passed by refference automatically)
            foreach (var child in match.Children)
            {
                child.Parent = match;
            }
            //match.LeftChild.Parent = match;
            //match.RightChild.Parent = match;

            //remove the matches we just used
            unstableMatches = unstableMatches.Skip(amountOfMatches).ToList();
            match.Position = match.Children.First().Position + 1;

            //and it to the done list
            stableMatches.Add(match);
        }
    }
}
