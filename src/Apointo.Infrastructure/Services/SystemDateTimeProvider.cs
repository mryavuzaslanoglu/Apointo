using System;
using Apointo.Application.Common.Interfaces;

namespace Apointo.Infrastructure.Services;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}