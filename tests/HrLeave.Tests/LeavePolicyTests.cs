using HrLeave.Api.Domain;
using Xunit;

namespace HrLeave.Tests;

public class LeavePolicyTests
{
    private static readonly DateOnly Today = new(2026, 11, 1);
    private static readonly Employee Veteran = new(Guid.NewGuid(), "Veteran", new DateOnly(2020, 3, 1), null);
    private static readonly Employee NewJoiner = new(Guid.NewGuid(), "New joiner", new DateOnly(2026, 6, 1), null);

    private static LeaveRequest Req(Employee e, LeaveType t, int fromDay, int days, LeaveStatus s = LeaveStatus.Pending) => new()
    {
        EmployeeId = e.Id, Type = t, From = Today.AddDays(fromDay), To = Today.AddDays(fromDay + days - 1), Status = s,
    };

    [Fact]
    public void Annual_leave_allowed_within_entitlement()
        => Assert.Null(LeavePolicy.Validate(Req(Veteran, LeaveType.Annual, 5, 10), Veteran, [], Today));

    [Fact]
    public void Annual_leave_not_available_before_nine_months()
        => Assert.Contains("9 months", LeavePolicy.Validate(Req(NewJoiner, LeaveType.Annual, 5, 3), NewJoiner, [], Today));

    [Fact]
    public void Annual_leave_cannot_exceed_thirty_days_a_year()
    {
        var history = new[] { Req(Veteran, LeaveType.Annual, -200, 25, LeaveStatus.Approved) };
        Assert.Contains("5 annual", LeavePolicy.Validate(Req(Veteran, LeaveType.Annual, 5, 6), Veteran, history, Today));
    }

    [Fact]
    public void End_before_start_is_rejected()
    {
        var r = new LeaveRequest { EmployeeId = Veteran.Id, Type = LeaveType.Annual, From = Today.AddDays(5), To = Today.AddDays(2) };
        Assert.Equal("The end date is before the start date.", LeavePolicy.Validate(r, Veteran, [], Today));
    }

    [Fact]
    public void Only_sick_leave_is_retrospective()
    {
        Assert.NotNull(LeavePolicy.Validate(Req(Veteran, LeaveType.Annual, -3, 1), Veteran, [], Today));
        Assert.Null(LeavePolicy.Validate(Req(Veteran, LeaveType.Sick, -3, 1), Veteran, [], Today));
    }

    [Fact]
    public void Hajj_once_and_after_two_years()
    {
        Assert.Contains("2 years", LeavePolicy.Validate(Req(NewJoiner, LeaveType.Hajj, 10, 10), NewJoiner, [], Today));
        var taken = new[] { Req(Veteran, LeaveType.Hajj, -400, 15, LeaveStatus.Approved) };
        Assert.Contains("once", LeavePolicy.Validate(Req(Veteran, LeaveType.Hajj, 10, 10), Veteran, taken, Today));
    }

    [Fact]
    public void Balance_counts_only_approved_leave_in_the_year()
    {
        var history = new[]
        {
            Req(Veteran, LeaveType.Annual, -100, 7, LeaveStatus.Approved),
            Req(Veteran, LeaveType.Annual, -50, 5, LeaveStatus.Rejected),
            Req(Veteran, LeaveType.Sick, -20, 2, LeaveStatus.Approved),
        };
        Assert.Equal((23, 13), LeavePolicy.Balance(Veteran, history, Today.Year));
    }
}
