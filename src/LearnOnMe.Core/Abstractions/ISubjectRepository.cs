using LearnOnMe.Core.Models;

namespace LearnOnMe.Core.Abstractions;

public interface ISubjectRepository
{
    IReadOnlyList<Subject> GetAll();

    Subject? GetById(long id);

    long Add(Subject subject);

    void Update(Subject subject);

    void Delete(long id);
}
