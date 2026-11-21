namespace HrLeave.Api.Domain;

/// <summary>
/// Overtime per Labour Law Art. 66 (TASK-004, in progress): working days at 125% of the hourly
/// wage, the weekly rest day at 150%, official holidays at 200%. Max 2 hours/day, 180 hours/year.
/// Awaiting Checkpoint A confirmation of the yearly cap with HR before wiring into payroll.
/// </summary>
public static class Overtime
{
    public const int MaxDailyMinutes = 120;
    public const int MaxYearlyHours = 180;

    public static decimal Multiplier(bool restDay, bool officialHoliday) => officialHoliday ? 2.00m : restDay ? 1.50m : 1.25m;

    public static int EligibleMinutes(AttendanceDay day)
        => Math.Clamp(day.WorkedMinutes - AttendanceDay.StandardMinutes, 0, MaxDailyMinutes);
}
