namespace MyORMLibrary;

public class DbSet<T> where T : class, new()
{
    private readonly ORMContext _context;
    private readonly string _tableName;

    public DbSet(ORMContext context, string table)
    {
        _context = context;
        _tableName = table;
    }

    public T Create(T entity) => _context.Create(entity, _tableName);
    public T? Find(int id) => _context.ReadById<T>(id, _tableName);
    public List<T> All() => _context.ReadAll<T>(_tableName);
    public void Update(int id, T e) => _context.Update(id, e, _tableName);
    public bool Delete(int id) => _context.Delete(id, _tableName);
}
