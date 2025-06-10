using LawyerProject.Domain.Common;

namespace LawyerProject.Application.Common.Interfaces;

public interface IRegionFilter
{
    Task<IQueryable<T>?> FilterByRegionAsync<T>(IQueryable<T> query, string? userId) where T : IHasRegion;
}


