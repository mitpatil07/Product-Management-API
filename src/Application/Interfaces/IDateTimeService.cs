using System;

namespace ProductManagement.Application.Interfaces
{
    public interface IDateTimeService
    {
        DateTime UtcNow { get; }
    }
}
