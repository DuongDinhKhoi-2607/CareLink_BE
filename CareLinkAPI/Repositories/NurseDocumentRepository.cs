using CareLinkAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CareLinkAPI.Repositories;

public class NurseDocumentRepository(CareLinkDbContext db) : INurseDocumentRepository
{
    public Task<List<NurseDocument>> GetByNurseIdAsync(Guid nurseId, CancellationToken cancellationToken = default) =>
        db.NurseDocuments
            .AsNoTracking()
            .Where(d => d.NurseId == nurseId)
            .OrderBy(d => d.DocumentType)
            .ToListAsync(cancellationToken);

    public Task<List<NurseDocument>> GetByNurseIdForUpdateAsync(Guid nurseId, CancellationToken cancellationToken = default) =>
        db.NurseDocuments
            .Where(d => d.NurseId == nurseId)
            .ToListAsync(cancellationToken);

    public Task<NurseDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.NurseDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public Task<NurseDocument?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken = default) =>
        db.NurseDocuments
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public Task<NurseDocument?> GetByNurseAndTypeAsync(Guid nurseId, int documentType, CancellationToken cancellationToken = default) =>
        db.NurseDocuments
            .FirstOrDefaultAsync(d => d.NurseId == nurseId && d.DocumentType == documentType, cancellationToken);

    public async Task AddAsync(NurseDocument document, CancellationToken cancellationToken = default) =>
        await db.NurseDocuments.AddAsync(document, cancellationToken);

    public async Task AddRangeAsync(IEnumerable<NurseDocument> documents, CancellationToken cancellationToken = default) =>
        await db.NurseDocuments.AddRangeAsync(documents, cancellationToken);

    public void Update(NurseDocument document) =>
        db.NurseDocuments.Update(document);

    public void Delete(NurseDocument document) =>
        db.NurseDocuments.Remove(document);
}
