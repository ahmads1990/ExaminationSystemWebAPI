using ExaminationSystemWebAPI.Models;
using System.Linq.Expressions;

namespace ExaminationSystemWebAPI.Data.GenericRepo;

public interface IRepository<Entity> where Entity : BaseModel
{
    IQueryable<Entity> GetAll();
    IQueryable<Entity> GetAllWithoutDeleted();
    IQueryable<Entity> GetByCondition(Expression<Func<Entity, bool>> expression);
    Task<Entity?> GetByID(string id);
    bool CheckExistsByID(string id);
    void Add(Entity entity);
    IEnumerable<Entity> AddRange(IEnumerable<Entity> entities);
    void Update(Entity entity);
    void SaveInclude(Entity entity, params string[] properties);
    void SaveExclude(Entity entity, params string[] properties);
    void Delete(Entity entity);
    void SoftDelete(Entity entity);
    void SaveChanges();
    Task SaveChangesAsync();
}
