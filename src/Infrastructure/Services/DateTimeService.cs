using System;
using ProductManagement.Application.Interfaces;

namespace ProductManagement.Infrastructure.Services;

public class DateTimeService : IDateTimeService
{
    public DateTime UtcNow => DateTime.UtcNow;
}
