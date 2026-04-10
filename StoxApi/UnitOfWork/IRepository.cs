using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

public interface IRepository<T> where T : class
{
    public IQueryable<T> AsNoTracking();

    public IQueryable<T> AsQueryable();

    public IQueryable<T> Where(Expression<Func<T, bool>> express);

    public void Add(T entity);

    public void Update(T entity);

    public void Remove(T entity);
}

public class Repository<T> : IRepository<T> where T : class
{
    private readonly DbContext _context;

    public Repository(DbContext context)
    {
        _context = context;
    }

    public IQueryable<T> AsNoTracking()
    {
        return _context.Set<T>().AsNoTracking();
    }

    public IQueryable<T> AsQueryable()
    {
        return _context.Set<T>().AsQueryable();
    }

    public IQueryable<T> Where(Expression<Func<T, bool>> express)
    {
        return _context.Set<T>().Where(express);
    }


    public void Add(T entity)
    {
        _context.Set<T>().Add(entity);
    }

    public void Remove(T entity)
    {
        _context.Set<T>().Remove(entity);
    }

    public void Update(T entity)
    {
        _context.Set<T>().Update(entity);
    }
}