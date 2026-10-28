namespace HrLeave.Api.Domain;

public enum LeaveType { Annual, Sick, Emergency, Hajj, Maternity }

public enum LeaveStatus { Pending, Approved, Rejected, Cancelled }

public sealed record Employee(Guid Id, string Name, DateOnly JoinedOn, Guid? ManagerId);

public sealed class LeaveRequest
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid EmployeeId { get; init; }
    public required LeaveType Type { get; init; }
    public required DateOnly From { get; init; }
    public required DateOnly To { get; init; }
    public string? Reason { get; init; }
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
    public Guid? DecidedBy { get; set; }
    public string? DecisionNote { get; set; }

    /// <summary>Calendar days, inclusive — the unit the labour law uses for annual leave.</summary>
    public int Days => To.DayNumber - From.DayNumber + 1;
}
