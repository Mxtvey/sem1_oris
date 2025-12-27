namespace MyORMLibrary;

public static class Database
{
    public static ORMContext ORM { get; private set; }

    static Database()
    {
        ORM = new ORMContext("Host=localhost;Port=5432;Database=exmpl;Username=postgres;Password=1234");
    }
}
