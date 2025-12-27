using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using MyORMLibrary.Atributes;
using Npgsql;

public class ORMContext
{
    private readonly string _connectionString;

    public ORMContext(string connectionString)
    {
        _connectionString = connectionString;
    }


  
    public T Create<T>(T entity, string tableName) where T : class, new()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var props = typeof(T).GetProperties();
        var columns = props.Where(p => !IsPrimaryKey(p) &&
                                       !IsNotMapped(p))
            .ToArray();

        var columnNames = string.Join(", ", columns.Select(GetColumnName));
        var paramNames = string.Join(", ", columns.Select(p => "@" + GetColumnName(p)));

        var sql = $"INSERT INTO {tableName} ({columnNames}) VALUES ({paramNames}) RETURNING *;";

        using var cmd = new NpgsqlCommand(sql, connection);

        foreach (var prop in columns)
        {
            var colName = GetColumnName(prop);
            var value = prop.GetValue(entity) ?? DBNull.Value;
            cmd.Parameters.AddWithValue("@" + colName, value);
        }

        using var reader = cmd.ExecuteReader();
        if (!reader.Read()) return entity;

        return MapEntity<T>(reader);
    }


 
    public T? ReadById<T>(int id, string tableName) where T : class, new()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        string sql = $"SELECT * FROM {tableName} WHERE id = @id;";
        using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = cmd.ExecuteReader();

        if (!reader.Read())
            return null;

        return MapEntity<T>(reader);
    }



    public List<T> ReadAll<T>(string tableName) where T : class, new()
    {
        var items = new List<T>();

        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        string sql = $"SELECT * FROM {tableName}";
        using var cmd = new NpgsqlCommand(sql, connection);
        using var reader = cmd.ExecuteReader();

        while (reader.Read())
            items.Add(MapEntity<T>(reader));

        return items;
    }


  
    public void Update<T>(int id, T entity, string tableName)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var props = typeof(T).GetProperties();
        var columns = props.Where(p => !IsPrimaryKey(p) &&
                                       !IsNotMapped(p))
                                        .ToArray();

        var setClause = string.Join(", ",
            columns.Select(p => $"{GetColumnName(p)} = @{GetColumnName(p)}"));

        string sql = $"UPDATE {tableName} SET {setClause} WHERE id = @id;";

        using var cmd = new NpgsqlCommand(sql, connection);

        foreach (var prop in columns)
        {
            var colName = GetColumnName(prop);
            var value = prop.GetValue(entity) ?? DBNull.Value;
            cmd.Parameters.AddWithValue("@" + colName, value);
        }

        cmd.Parameters.AddWithValue("@id", id);
        cmd.ExecuteNonQuery();
    }


    public bool Delete(int id, string tableName)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();

        var sql = $"DELETE FROM {tableName} WHERE id = @id;";
        using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@id", id);

        return cmd.ExecuteNonQuery() > 0;
    }


  

    private static T MapEntity<T>(IDataRecord reader) where T : class, new()
    {
        var entity = new T();

        var props = typeof(T).GetProperties();

        var dbColumns = new HashSet<string>();
        for (int i = 0; i < reader.FieldCount; i++)
            dbColumns.Add(reader.GetName(i).ToLower());

        foreach (var prop in props)
        {   
            if (IsNotMapped(prop))
                continue;

            var colName = GetColumnName(prop);

            if (!dbColumns.Contains(colName))
                continue;

            var value = reader[colName];

            if (value != DBNull.Value)
                prop.SetValue(entity, value);
        }


        return entity;
    }


    public static string GetColumnName(PropertyInfo prop)
    {
        var attr = prop.GetCustomAttribute<ColumnAttribute>();
        return attr != null ? attr.Name.ToLower() : prop.Name.ToLower();
    }

    public static bool IsPrimaryKey(PropertyInfo prop)
    {
        return prop.GetCustomAttribute<PrimaryKeyAttribute>() != null;
    }
    private static bool IsNotMapped(PropertyInfo prop)
    {
        return prop.GetCustomAttribute<NotMappedAttribute>() != null;
    }

}


public static class DataReaderExtensions
{
    public static bool HasColumn(this IDataRecord reader, string columnName)
    {
        for (int i = 0; i < reader.FieldCount; i++)
            if (string.Equals(reader.GetName(i), columnName, StringComparison.OrdinalIgnoreCase))
                return true;

        return false;
    }
}
