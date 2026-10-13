using CareLinkAPI.Models;

namespace CareLinkAPI.Repositories;

public interface INurseDocumentRepository
{
    Task<List<NurseDocument>> GetByNurseIdAsync(Guid nurseId, CancellationToken cancellationToken = default);

    Task<List<NurseDocument>> GetByNurseIdForUpdateAsync(Guid nurseId, CancellationToken cancellationToken = default);

    Task<NurseDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<NurseDocument?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<NurseDocument?> GetByNurseAndTypeAsync(Guid nurseId, int documentType, CancellationToken cancellationToken = default);

    Task AddAsync(NurseDocument document, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<NurseDocument> documents, CancellationToken cancellationToken = default);

    void Update(NurseDocument document);

    void Delete(NurseDocument document);
}
