using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDb>(o => o.UseInMemoryDatabase("RaceDayDB"));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

// ===== MODELS =====
public class AppDb : DbContext
{
    public AppDb(DbContextOptions<AppDb> opt) : base(opt) { }
    public DbSet<User> Users { get; set; }
    public DbSet<RaceEvent> RaceEvents { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    // ADDED FOR POE
    public DbSet<Category> Categories { get; set; }
    public DbSet<EventEnrolment> EventEnrolments { get; set; }
    public DbSet<Result> Results { get; set; }
}

public class User
{
    public int Id { get; set; }
    [Required] public string FullName { get; set; } = "";
    [Required] public string Email { get; set; } = "";
    [Required] public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "Athlete";
}

public class RaceEvent
{
    public int Id { get; set; }
    [Required] public string Name { get; set; } = "";
    public string Description { get; set; } = ""; 
    public string Location { get; set; } = "Pretoria";
    public DateTime Date { get; set; } = DateTime.Now.AddDays(7);
    public double Distance { get; set; } = 10; 
    public string EventType { get; set; } = "run"; 
    public int MaxParticipants { get; set; } = 100;
    public decimal Fee { get; set; } = 150;
    public string Status { get; set; } = "Upcoming";
}

public class Booking
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int RaceEventId { get; set; }
    public DateTime BookingDate { get; set; } = DateTime.Now;
    public string Status { get; set; } = "Confirmed";
}

// ===== ADDED MODELS =====
public class Category
{
    public int Id { get; set; }
    public int RaceEventId { get; set; }
    [Required] public string Name { get; set; } = ""; 
    public string Type { get; set; } = "Distance";
    public string Description { get; set; } = "";
}

public class EventEnrolment
{
    public int Id { get; set; }
    public int ParticipantId { get; set; }
    public int EventId { get; set; }
    public int CategoryId { get; set; }
    public DateTime EnrolmentDate { get; set; } = DateTime.Now;
    public string Status { get; set; } = "Enrolled";
}

public class Result
{
    public int Id { get; set; }
    public int EventEnrolmentId { get; set; }
    public int EventId { get; set; }
    public int ParticipantId { get; set; }
    public TimeSpan FinishTime { get; set; }
    public int Position { get; set; }
    public string CategoryName { get; set; } = "";
    public DateTime RecordedDate { get; set; } = DateTime.Now;
}

// ===== CONTROLLERS =====
[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AppDb _db;
    public AuthController(AppDb db) => _db = db;

    [HttpPost("register")]
    public async Task<IActionResult> Register(User dto)
    {
        if (_db.Users.Any(u => u.Email == dto.Email)) return BadRequest("Email exists");
        dto.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.PasswordHash);
        _db.Users.Add(dto);
        await _db.SaveChangesAsync();
        return Ok(dto);
    }

    [HttpPost("login")]
    public IActionResult Login(User dto)
    {
        var user = _db.Users.FirstOrDefault(u => u.Email == dto.Email);
        if (user == null) return Unauthorized("Invalid login");
        bool ok = BCrypt.Net.BCrypt.Verify(dto.PasswordHash, user.PasswordHash);
        if (!ok) return Unauthorized("Invalid login");
        return Ok(new { message = "Login success", user.Id, user.Role, user.FullName });
    }
}

[Route("api/[controller]")]
[ApiController]
public class RaceEventsController : ControllerBase
{
    private readonly AppDb _db;
    public RaceEventsController(AppDb db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _db.RaceEvents.ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create(RaceEvent ev)
    {
        _db.RaceEvents.Add(ev);
        await _db.SaveChangesAsync();
        return Ok(ev);
    }
}

[Route("api/[controller]")]
[ApiController]
public class BookingsController : ControllerBase
{
    private readonly AppDb _db;
    public BookingsController(AppDb db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _db.Bookings.ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create(Booking b)
    {
        var race = await _db.RaceEvents.FindAsync(b.RaceEventId);
        if (race == null) return NotFound("Race not found");
        var count = _db.Bookings.Count(x => x.RaceEventId == b.RaceEventId);
        if (count >= race.MaxParticipants) return BadRequest("Race full");
        _db.Bookings.Add(b);
        await _db.SaveChangesAsync();
        return Ok(b);
    }
}

// ===== ADDED CONTROLLERS - POE =====
[Route("api/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly AppDb _db;
    public CategoriesController(AppDb db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll(int? raceEventId)
    {
        if (raceEventId != null)
            return Ok(await _db.Categories.Where(c => c.RaceEventId == raceEventId).ToListAsync());
        return Ok(await _db.Categories.ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create(Category cat)
    {
        _db.Categories.Add(cat);
        await _db.SaveChangesAsync();
        return Ok(cat);
    }
}

[Route("api/[controller]")]
[ApiController]
public class EventEnrolmentsController : ControllerBase
{
    private readonly AppDb _db;
    public EventEnrolmentsController(AppDb db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll(int? eventId, int? participantId)
    {
        var q = _db.EventEnrolments.AsQueryable();
        if (eventId != null) q = q.Where(e => e.EventId == eventId);
        if (participantId != null) q = q.Where(e => e.ParticipantId == participantId);
        return Ok(await q.ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create(EventEnrolment en)
    {
        if (_db.EventEnrolments.Any(x => x.EventId == en.EventId && x.ParticipantId == en.ParticipantId))
            return BadRequest("Already enrolled");

        _db.EventEnrolments.Add(en);
        await _db.SaveChangesAsync();
        return Ok(en);
    }
}

[Route("api/[controller]")]
[ApiController]
public class ResultsController : ControllerBase
{
    private readonly AppDb _db;
    public ResultsController(AppDb db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll(int? eventId, int? participantId)
    {
        var q = _db.Results.AsQueryable();
        if (eventId != null) q = q.Where(r => r.EventId == eventId);
        if (participantId != null) q = q.Where(r => r.ParticipantId == participantId);
        return Ok(await q.OrderBy(r => r.Position).ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create(Result res)
    {
        var enrol = await _db.EventEnrolments.FindAsync(res.EventEnrolmentId);
        if (enrol == null) return NotFound("Enrolment not found");

        if (_db.Results.Any(r => r.EventEnrolmentId == res.EventEnrolmentId))
            return BadRequest("Result already exists");

        var cat = await _db.Categories.FindAsync(enrol.CategoryId);

        res.EventId = enrol.EventId;
        res.ParticipantId = enrol.ParticipantId;
        res.CategoryName = cat?.Name ?? "";
        res.RecordedDate = DateTime.Now;

        _db.Results.Add(res);
        await _db.SaveChangesAsync();
        return Ok(res);
    }
}