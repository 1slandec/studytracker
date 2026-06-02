using StudyTracker.Models;

namespace StudyTracker.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string id);

    Task<List<User>> GetStudentsAsync();
}
