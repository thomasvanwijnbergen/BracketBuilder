using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BracketBuilder.Auth;
using BracketBuilder.Database;
using BracketBuilder.Models;

namespace BracketBuilder.Database;

[Route("debug")]
[ApiController]
public class AccountController : Controller
{
    private readonly DatabaseContext _db;

    public AccountController(DatabaseContext db)
    {
        _db = db;
    }
    [HttpGet]
    public async Task<ActionResult<string>> Get()
    {
        return "debug/accounts\ndebug/posts\ndebug/react";
    }

    //[HttpGet("accounts")]
    //public async Task<ActionResult<List<UserAccount>>> GetAccounts()
    //{
    //    return (await _db.Accounts.ToListAsync()).OrderByDescending(s => s.Id).ToList();
    //}
    //[HttpGet("react")]
    //public async Task<ActionResult<List<Reaction>>> GetReactions()
    //{
    //    return (await _db.Reactions.ToListAsync()).OrderByDescending(s => s.Id).ToList();
    //}
    //[HttpGet("posts")]
    //public async Task<ActionResult<List<Post>>> GetPosts()
    //{
    //    return (await _db.Posts.ToListAsync()).OrderByDescending(s => s.Id).ToList();
    //}

}
