using ExaminationSystemWebAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ExaminationSystemWebAPI.Data.GenericRepo;

public class Repository<Entity> : IRepository<Entity> where Entity : BaseModel
{
    AppDbContext _dbContext;
    DbSet<Entity> _entities;

    public Repository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
        _entities = _dbContext.Set<Entity>();
    }

    public IQueryable<Entity> GetAll()
    {
        return _entities;
    }

    public IQueryable<Entity> GetByCondition(Expression<Func<Entity, bool>> expression)
    {
        return GetAll().Where(expression);
    }

    public async Task<Entity?> GetByID(string id)
    {
        return await GetByCondition(x => x.ID == id).FirstOrDefaultAsync();
    }

    public void Add(Entity entity)
    {
        _entities.Add(entity);
    }

    public void Update(Entity entity)
    {
        _entities.Update(entity);
    }

    public void SaveInclude(Entity entity, params string[] properties)
    {
        var entry = _entities.Local.FindEntry(entity.ID);

        if (entry is null)
            entry = _entities.Entry(entity);
        else
        {
            //entry = _dbcontext.ChangeTracker
            //        .Entries<Entity>()
            //        .First(x => x.Entity.ID == entity.ID);
        }
        foreach (var property in entry.Properties)
        {
            if (properties.Contains(property.Metadata.Name))
            {
                property.CurrentValue = entry.GetType().GetProperty(property.Metadata.Name).GetValue(entity);
                property.IsModified = true;
            }
        }
    }

    public void SaveExclude(Entity entity, params string[] properties)
    {
        var entry = _entities.Local.FindEntry(entity.ID) ?? _entities.Entry(entity);

        foreach (var property in entry.Properties)
        {
            if (!properties.Contains(property.Metadata.Name))
            {
                property.CurrentValue = entry.GetType().GetProperty(property.Metadata.Name).GetValue(entity);
                property.IsModified = true;
            }
        }
    }

    public void SaveExclude2(Entity entity, params string[] properties)
    {
        _dbContext.Update(entity);

        var entry = _entities.Local.FindEntry(entity.ID);

        foreach (var property in properties)
        {
            entry.Properties
                .First(p => p.Metadata.Name == property)
                .IsModified = false;
        }
    }

    public void Delete(Entity entity)
    {
        _entities.Remove(entity);
    }

    public void SaveChanges()
    {
        _dbContext.SaveChanges();
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }
}
