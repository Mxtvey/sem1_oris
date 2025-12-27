namespace MyORMLibrary.Atributes;

[AttributeUsage(AttributeTargets.Property)]
public class ForeignKeyAttribute : Attribute
{
    public string ReferenceTable { get; }

    public ForeignKeyAttribute(string referenceTable)
    {
        ReferenceTable = referenceTable;
    }
}
