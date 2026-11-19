namespace HrLeave.Api.Domain;

/// <summary>One working day's check-in and check-out (TASK-006). Hours feed overtime (TASK-004).</summary>
public sealed record AttendanceDay(Guid EmployeeId, DateOnly Day, TimeOnly CheckIn, TimeOnly? CheckOut)
{
    public const int StandardMinutes = 8 * 60;

    public int WorkedMinutes => CheckOut is { } o && o > CheckIn ? (int)(o - CheckIn).TotalMinutes : 0;

    public bool IsLate(TimeOnly shiftStart, int graceMinutes = 15) => CheckIn > shiftStart.AddMinutes(graceMinutes);
}
