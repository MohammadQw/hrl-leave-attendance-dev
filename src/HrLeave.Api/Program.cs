using System.Collections.Concurrent;
using HrLeave.Api.Domain;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// In-memory store for the MVP; persistence is TASK-005 (SQL Server, see ARC-001 in AKB).
var employees = new ConcurrentDictionary<Guid, Employee>();
var requests = new ConcurrentDictionary<Guid, LeaveRequest>();
var demo = new Employee(Guid.Parse("11111111-1111-1111-1111-111111111111"), "Demo Employee", new DateOnly(2023, 1, 15), null);
employees[demo.Id] = demo;

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "HrLeave.Api" }));

app.MapGet("/employees/{id:guid}/balance", (Guid id) =>
{
    if (!employees.TryGetValue(id, out var e)) return Results.NotFound();
    var (annual, sick) = LeavePolicy.Balance(e, requests.Values, DateTime.Today.Year);
    return Results.Ok(new { employee = e.Name, year = DateTime.Today.Year, annualRemaining = annual, sickFullPayRemaining = sick });
});

app.MapPost("/leave-requests", (LeaveRequest request) =>
{
    if (!employees.TryGetValue(request.EmployeeId, out var e)) return Results.NotFound(new { error = "Unknown employee." });
    var error = LeavePolicy.Validate(request, e, requests.Values, DateOnly.FromDateTime(DateTime.Today));
    if (error is not null) return Results.BadRequest(new { error });
    requests[request.Id] = request;
    return Results.Created($"/leave-requests/{request.Id}", request);
});

app.MapPost("/leave-requests/{id:guid}/decision", (Guid id, Decision d) =>
{
    if (!requests.TryGetValue(id, out var r)) return Results.NotFound();
    if (r.Status != LeaveStatus.Pending) return Results.Conflict(new { error = $"Already {r.Status}." });
    if (d.Approve is false && string.IsNullOrWhiteSpace(d.Note)) return Results.BadRequest(new { error = "A rejection needs a note." });
    r.Status = d.Approve ? LeaveStatus.Approved : LeaveStatus.Rejected;
    r.DecidedBy = d.ManagerId;
    r.DecisionNote = d.Note;
    return Results.Ok(r);
});

app.Run();

public sealed record Decision(Guid ManagerId, bool Approve, string? Note);

public partial class Program;
