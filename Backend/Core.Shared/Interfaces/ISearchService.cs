using Core.Shared.Entities;

namespace Core.Shared.Interfaces;

public interface ISearchService
{
    Task<IEnumerable<Book>> BasicSearchAsync(string keyword);
    Task<IEnumerable<Book>> AdvancedSearchAsync(string query);
}
