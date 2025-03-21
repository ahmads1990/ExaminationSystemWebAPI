using ExaminationSystem.Domain.Entities;
using System.Linq.Expressions;

namespace ExaminationSystem.Domain.Interfaces;

public interface IRepository<Entity> where Entity : BaseModel
{
    IQueryable<Entity> GetAll();
    IQueryable<Entity> GetByCondition(Expression<Func<Entity, bool>> expression);
    IQueryable<Entity> GetByID(int id);
    Task<bool> CheckExistsByID(int id, CancellationToken cancellationToken = default);
    Task<Entity> Add(Entity entity, CancellationToken cancellationToken = default);
    Task AddRange(IEnumerable<Entity> entities, CancellationToken cancellationToken = default);
    void Update(Entity entity);
    void SaveInclude(Entity entity, params string[] properties);
    void SaveExclude(Entity entity, params string[] properties);
    void Delete(Entity entity);
    void DeleteRange(IEnumerable<Entity> entity);
    Task<bool> SaveChanges(CancellationToken cancellationToken = default);
}
