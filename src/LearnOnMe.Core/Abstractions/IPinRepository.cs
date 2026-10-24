using LearnOnMe.Core.Models;

namespace LearnOnMe.Core.Abstractions;

public interface IPinRepository
{
    IReadOnlyList<Pin> GetForSubject(long subjectId);

    long Add(Pin pin);

    void Update(Pin pin);

    void Delete(long id);
}
