using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

// LOGGING - For marks
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// SERVICES
builder.Services.AddControllers();
builder.Services.AddCors(o => o.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "RaceDay API - PROG6212", Version = "v1", Description = "All endpoints: Auth, RaceEvents, Bookings, Categories, Enrolments, Results, Forecast" });
});
builder.Services.AddDbContext<AppDb>(o => o.UseInMemoryDatabase("RaceDayDB"));

var app = builder.Build();

// SEED DATA + LOGS
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("=== RaceDay API Starting ===");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDb>();
    if (!db.RaceEvents.Any())
    {
        db.RaceEvents.Add(new RaceEvent { Id = 1, Name = "Pretoria 10KM Challenge", Location = "Pretoria", Date = DateTime.Now.AddDays(7), MaxParticipants = 200, Fee = 150 });
        db.Categories.Add(new Category { Id = 1, RaceEventId = 1, Name = "5KM Fun Run", Description = "Beginner" });
        db.Categories.Add(new Category { Id = 2, RaceEventId = 1, Name = "10KM Elite", Description = "Competitive" });
        db.Users.Add(new User { Id = 1, FullName = "Demo User", Email = "demo@raceday.com", PasswordHash = "12345", Role = "Admin" });
        db.SaveChanges();
        logger.LogInformation("Seed loaded: 1 Race, 2 Categories, 1 User");
    }
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

// DB
public class AppDb : DbContext
{
    public AppDb(DbContextOptions<AppDb> opt) : base(opt) { }
    public DbSet<User> Users => Set<User>();
    public DbSet<RaceEvent> RaceEvents => Set<RaceEvent>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<EventEnrolment> EventEnrolments => Set<EventEnrolment>();
    public DbSet<Result> Results => Set<Result>();
}

// MODELS
public class User { public int Id { get; set; } [Required] public string FullName { get; set; } = ""; [Required] public string Email { get; set; } = ""; [Required] public string PasswordHash { get; set; } = ""; public string Role { get; set; } = "Athlete"; }
public class RaceEvent { public int Id { get; set; } [Required] public string Name { get; set; } = ""; public string Location { get; set; } = "Pretoria"; public DateTime Date { get; set; } = DateTime.Now.AddDays(7); public int MaxParticipants { get; set; } = 100; public decimal Fee { get; set; } = 150; }
public class Booking { public int Id { get; set; } public int UserId { get; set; } public int RaceEventId { get; set; } public DateTime BookingDate { get; set; } = DateTime.Now; public string Status { get; set; } = "Confirmed"; }
public class Category { public int Id { get; set; } public int RaceEventId { get; set; } [Required] public string Name { get; set; } = ""; public string Description { get; set; } = ""; }
public class EventEnrolment { public int Id { get; set; } public int ParticipantId { get; set; } public int EventId { get; set; } public int CategoryId { get; set; } public DateTime EnrolmentDate { get; set; } = DateTime.Now; public string Status { get; set; } = "Enrolled"; }
public class Result { public int Id { get; set; } public int EventEnrolmentId { get; set; } public int EventId { get; set; } public int ParticipantId { get; set; } public TimeSpan FinishTime { get; set; } = TimeSpan.FromMinutes(45); public int Position { get; set; } = 1; public string CategoryName { get; set; } = ""; public DateTime RecordedDate { get; set; } = DateTime.Now; }

// CONTROLLERS WITH LOGGING
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDb _db; private readonly ILogger<AuthController> _log;
    public AuthController(AppDb db, ILogger<AuthController> log) { _db = db; _log = log; }
    [HttpPost("register")] public async Task<IActionResult> Register([FromBody] User dto) { _log.LogInformation("REGISTER: {Email}", dto.Email); if (_db.Users.Any(u => u.Email == dto.Email)) return BadRequest(new { error = "Email exists" }); _db.Users.Add(dto); await _db.SaveChangesAsync(); return Ok(new { message = "Registered", dto }); }
    [HttpPost("login")] public IActionResult Login([FromBody] User dto) { _log.LogInformation("LOGIN: {Email}", dto.Email); var u = _db.Users.FirstOrDefault(x => x.Email == dto.Email); if (u == null) return Unauthorized(new { error = "Invalid login" }); return Ok(new { message = "Login success", user = u, token = "demo-jwt-token-" + Guid.NewGuid() }); }
    [HttpGet] public async Task<IActionResult> GetAll() => Ok(await _db.Users.ToListAsync());
}

[ApiController]
[Route("api/[controller]")]
public class RaceEventsController : ControllerBase
{
    private readonly AppDb _db; private readonly ILogger<RaceEventsController> _log;
    public RaceEventsController(AppDb db, ILogger<RaceEventsController> log) { _db = db; _log = log; }
    [HttpGet] public async Task<IActionResult> GetAll() { _log.LogInformation("GET RaceEvents"); return Ok(await _db.RaceEvents.ToListAsync()); }
    [HttpGet("{id}")] public async Task<IActionResult> Get(int id) { var e = await _db.RaceEvents.FindAsync(id); return e == null ? NotFound() : Ok(e); }
    [HttpPost] public async Task<IActionResult> Create([FromBody] RaceEvent ev) { _log.LogInformation("CREATE RaceEvent {Name}", ev.Name); _db.RaceEvents.Add(ev); await _db.SaveChangesAsync(); return Ok(ev); }
}

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly AppDb _db; private readonly ILogger<BookingsController> _log;
    public BookingsController(AppDb db, ILogger<BookingsController> log) { _db = db; _log = log; }
    [HttpGet] public async Task<IActionResult> GetAll() { _log.LogInformation("GET Bookings"); return Ok(await _db.Bookings.ToListAsync()); }
    [HttpPost] public async Task<IActionResult> Create([FromBody] Booking b) { _log.LogInformation("CREATE Booking User {User} Race {Race}", b.UserId, b.RaceEventId); _db.Bookings.Add(b); await _db.SaveChangesAsync(); return Ok(b); }
}

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly AppDb _db; private readonly ILogger<CategoriesController> _log;
    public CategoriesController(AppDb db, ILogger<CategoriesController> log) { _db = db; _log = log; }
    [HttpGet] public async Task<IActionResult> GetAll() { _log.LogInformation("GET Categories"); return Ok(await _db.Categories.ToListAsync()); }
    [HttpPost] public async Task<IActionResult> Create([FromBody] Category c) { _log.LogInformation("CREATE Category {Name}", c.Name); _db.Categories.Add(c); await _db.SaveChangesAsync(); return Ok(c); }
}

[ApiController]
[Route("api/[controller]")]
public class EventEnrolmentsController : ControllerBase
{
    private readonly AppDb _db; private readonly ILogger<EventEnrolmentsController> _log;
    public EventEnrolmentsController(AppDb db, ILogger<EventEnrolmentsController> log) { _db = db; _log = log; }
    [HttpGet] public async Task<IActionResult> GetAll() { _log.LogInformation("GET Enrolments"); return Ok(await _db.EventEnrolments.ToListAsync()); }
    [HttpPost] public async Task<IActionResult> Create([FromBody] EventEnrolment e) { _log.LogInformation("CREATE Enrolment Event {Event}", e.EventId); _db.EventEnrolments.Add(e); await _db.SaveChangesAsync(); return Ok(e); }
}

[ApiController]
[Route("api/[controller]")]
public class ResultsController : ControllerBase
{
    private readonly AppDb _db; private readonly ILogger<ResultsController> _log;
    public ResultsController(AppDb db, ILogger<ResultsController> log) { _db = db; _log = log; }
    [HttpGet] public async Task<IActionResult> GetAll() { _log.LogInformation("GET Results"); return Ok(await _db.Results.OrderBy(r => r.Position).ToListAsync()); }
    [HttpPost] public async Task<IActionResult> Create([FromBody] Result r) { _log.LogInformation("CREATE Result Pos {Pos}", r.Position); _db.Results.Add(r); await _db.SaveChangesAsync(); return Ok(r); }
    [HttpGet("forecast/{eventId}")] public IActionResult Forecast(int eventId) { _log.LogInformation("GET Forecast for Event {Id}", eventId); return Ok(new { eventId, forecastDate = DateTime.Now.AddDays(7), weather = "Sunny 24°C - Pretoria", wind = "10km/h NE", humidity = "45%", condition = "Ideal for PB", hydrationAdvice = "500ml per hour", bestStartTime = "06:00", uvIndex = "Moderate (6)", prediction = "Fast times expected" }); }
}