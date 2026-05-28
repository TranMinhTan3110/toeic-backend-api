using System.Collections.Generic;

namespace ToeicBackend.Application.DTOs;

public class PagedUsersResultDto
{
    public IEnumerable<UserProfileDto> Items { get; set; } = new List<UserProfileDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
