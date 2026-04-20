using System.Linq.Expressions;
using HabitDev.Entities;

namespace HabitDev.DTOs.Users;

internal static class UserQueries
{
    public static Expression<Func<User, UserDto>> ProjectToDto()
    {
        return u => new UserDto()
        {
            Id = u.Id,
            Email = u.Email,
            Name = u.Name,
            CreatedAtUtc = u.CreatedAtUtc,
            UpdatedAtUtc = u.UpdatedAtUtc
        };
    }
}
