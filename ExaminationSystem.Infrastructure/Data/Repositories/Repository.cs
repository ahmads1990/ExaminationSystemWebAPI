using ExaminationSystem.Domain.Entities;
using ExaminationSystem.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ExaminationSystem.Infrastructure.Data.Repositories;

public class Repository<Entity> : IRepository<Entity> where Entity : BaseModel
{
    AppDbContext _context;
    DbSet<Entity> _dbset;
    private static readonly string[] ImmutableFieldNames = { nameof(BaseModel.ID), nameof(BaseModel.CreatedDate), nameof(BaseModel.CreatedBy), nameof(BaseModel.UpdatedBy), nameof(BaseModel.UpdatedDate) };

    public Repository(AppDbContext dbContext)
    {
        _context = dbContext;
        _dbset = _context.Set<Entity>();
    }

    public IQueryable<Entity> GetAll()
    {
        return _dbset;
    }

    public IQueryable<Entity> GetByCondition(Expression<Func<Entity, bool>> expression)
    {
        return GetAll().Where(expression);
    }

    public IQueryable<Entity> GetByID(int id)
    {
        return GetByCondition(x => x.ID == id);
    }

    public async Task<Entity?> GetByID(int id, CancellationToken cancellationToken = default)
    {
        return await GetByCondition(x => x.ID == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> CheckExistsByID(int id, CancellationToken cancellationToken = default)
    {
        return await GetAll().AnyAsync(x => x.ID == id, cancellationToken);
    }

    public async Task<Entity> Add(Entity entity, CancellationToken cancellationToken = default)
    {
        entity.CreatedDate = DateTime.Now;

        var result = await _dbset.AddAsync(entity, cancellationToken);
        return result.Entity;
    }

    public async Task AddRange(IEnumerable<Entity> entities, CancellationToken cancellationToken = default)
    {
        foreach (var entity in entities)
        {
            entity.CreatedDate = DateTime.Now;
        }

        await _dbset.AddRangeAsync(entities, cancellationToken);
    }

    public void Update(Entity entity)
    {
        _dbset.Update(entity);
    }

    public void SaveInclude(Entity entity, params string[] properties)
    {
        properties.Except(ImmutableFieldNames);

        var changeTrackerEntry = _dbset.Local.FindEntry(entity.ID) ?? _dbset.Entry(entity);

        foreach (var entryProperty in changeTrackerEntry.Properties)
        {
            if (properties.Contains(entryProperty.Metadata.Name))
            {
                entryProperty.CurrentValue = entity.GetType().GetProperty(entryProperty.Metadata.Name).GetValue(entity);
                entryProperty.IsModified = true;
            }
        }
    }

    public void SaveExclude(Entity entity, params string[] properties)
    {
        properties = properties
            .Concat(ImmutableFieldNames)
            .ToArray();

        var changeTrackerEntry = _dbset.Local.FindEntry(entity.ID) ?? _dbset.Entry(entity);

        foreach (var property in changeTrackerEntry.Properties)
        {
            if (!properties.Contains(property.Metadata.Name))
            {
                property.CurrentValue = entity.GetType().GetProperty(property.Metadata.Name).GetValue(entity);
                property.IsModified = true;
            }
        }
    }

    public void Delete(Entity entity)
    {
        _dbset.Remove(entity);
    }

    public void DeleteRange(IEnumerable<Entity> entity)
    {
        _dbset.RemoveRange(entity);
    }

    public async Task<bool> SaveChanges(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken) > 0;
    }
}
