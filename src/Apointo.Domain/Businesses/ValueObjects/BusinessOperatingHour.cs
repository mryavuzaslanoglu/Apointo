using System;
using System.Collections.Generic;
using Apointo.Domain.Common;

namespace Apointo.Domain.Businesses.ValueObjects;

public sealed class BusinessOperatingHour : ValueObject
{
    private BusinessOperatingHour()
    {
    }

    private BusinessOperatingHour(DayOfWeek dayOfWeek, bool isClosed, TimeSpan? openTime, TimeSpan? closeTime)
    {
        DayOfWeek = dayOfWeek;
        IsClosed = isClosed;
        OpenTime = openTime;
        CloseTime = closeTime;
    }

    public DayOfWeek DayOfWeek { get; private set; }
    public bool IsClosed { get; private set; }
    public TimeSpan? OpenTime { get; private set; }
    public TimeSpan? CloseTime { get; private set; }

    public static BusinessOperatingHour Create(DayOfWeek dayOfWeek, bool isClosed, TimeSpan? openTime, TimeSpan? closeTime)
    {
        return new BusinessOperatingHour(dayOfWeek, isClosed, openTime, closeTime);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return DayOfWeek;
        yield return IsClosed;
        yield return OpenTime;
        yield return CloseTime;
    }
}
