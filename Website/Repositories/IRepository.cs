namespace RSA_UI.Repositories;

public interface IRepository<T>
{
    T? GetById(string id);
    bool Update(T entity);
    T? Add(T entity);
    bool Delete(T entity);
    T? First(Func<T, bool> f);
    IEnumerable<T> GetAll();
    IEnumerable<T> Filter(Func<T, bool> f);
}