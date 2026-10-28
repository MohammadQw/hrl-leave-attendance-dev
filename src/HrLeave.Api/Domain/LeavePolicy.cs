namespace HrLeave.Api.Domain;

/// <summary>
/// Entitlements per Kuwait Private Sector Labour Law (Law 6/2010), simplified for the MVP.
/// Annual: 30 days per year, available after 9 months of service (Art. 70).
/// Sick: 15 days full pay per year in the first tier (Art. 69) — later tiers are TASK-007.
/// Hajj: 21 days once during service, after 2 years (Art. 76).
/// </summary>
public static class LeavePolicy
{
    public const int AnnualDaysPerYear = 30;
    public const int AnnualEligibleAfterMonths = 9;
    public const int SickFullPayDays = 15;
    public const int HajjDays = 21;
    public const int HajjEligibleAfterYears = 2;

    public static int MonthsOfService(Employee e, DateOnly today)
        => (today.Year - e.JoinedOn.Year) * 12 + today.Month - e.JoinedOn.Month - (today.Day < e.JoinedOn.Day ? 1 : 0);

    /// <summary>Returns null when the request is allowed, otherwise the reason it is not.</summary>
    public static string? Validate(LeaveRequest request, Employee employee, IEnumerable<LeaveRequest> history, DateOnly today)
    {
        if (request.To < request.From) return "The end date is before the start date.";
        if (request.From < today && request.Type != LeaveType.Sick) return "Only sick leave can be recorded retrospectively.";

        var taken = history
            .Where(h => h.EmployeeId == employee.Id && h.Type == request.Type && h.Status == LeaveStatus.Approved && h.From.Year == request.From.Year)
            .Sum(h => h.Days);

        return request.Type switch
        {
            LeaveType.Annual when MonthsOfService(employee, today) < AnnualEligibleAfterMonths
                => $"Annual leave is available after {AnnualEligibleAfterMonths} months of service.",
            LeaveType.Annual when taken + request.Days > AnnualDaysPerYear
                => $"Only {AnnualDaysPerYear - taken} annual leave days remain this year.",
            LeaveType.Sick when taken + request.Days > SickFullPayDays
                => $"Only {SickFullPayDays - taken} full-pay sick days remain this year; longer sick leave needs HR review.",
            LeaveType.Hajj when MonthsOfService(employee, today) < HajjEligibleAfterYears * 12
                => $"Hajj leave is available after {HajjEligibleAfterYears} years of service.",
            LeaveType.Hajj when history.Any(h => h.EmployeeId == employee.Id && h.Type == LeaveType.Hajj && h.Status == LeaveStatus.Approved)
                => "Hajj leave can be taken once during service.",
            LeaveType.Hajj when request.Days > HajjDays => $"Hajj leave is at most {HajjDays} days.",
            _ => null,
        };
    }

    public static (int Annual, int Sick) Balance(Employee employee, IEnumerable<LeaveRequest> history, int year)
    {
        int Taken(LeaveType t) => history
            .Where(h => h.EmployeeId == employee.Id && h.Type == t && h.Status == LeaveStatus.Approved && h.From.Year == year)
            .Sum(h => h.Days);
        return (AnnualDaysPerYear - Taken(LeaveType.Annual), SickFullPayDays - Taken(LeaveType.Sick));
    }
}
