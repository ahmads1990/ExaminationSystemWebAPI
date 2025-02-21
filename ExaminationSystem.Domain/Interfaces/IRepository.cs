using ExaminationSystem.Domain.Entities;
using System.Linq.Expressions;

namespace ExaminationSystem.Domain.Interfaces;

public interface IRepository<Entity> where Entity : BaseModel
{
    IQueryable<Entity> GetAll();
    IQueryable<Entity> GetByCondition(Expression<Func<Entity, bool>> expression);
    Task<Entity?> GetByID(int id);
    Task<bool> CheckExistsByID(int id);
    Task<Entity> Add(Entity entity);
    Task AddRange(IEnumerable<Entity> entities);
    void Update(Entity entity);
    void SaveInclude(Entity entity, params string[] properties);
    void SaveExclude(Entity entity, params string[] properties);
    void Delete(Entity entity);
    void DeleteRange(IEnumerable<Entity> entity);
    Task<bool> SaveChanges();
}
