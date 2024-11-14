namespace RSA_UI.Services;

public interface IService<T>
{
    T? GetById(string id);
    bool Update(T entity);
    T? Add(T entity);
    bool Delete(T entity);
    List<T> GetAll();
    T? First(Func<T, bool> predicate);
}