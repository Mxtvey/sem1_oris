using MyORMLibrary.Atributes;

namespace MyORMLibrary.Entity;

public class UserEntity
{
    [PrimaryKey]
    public int Id { get; set; }

    [Column("username")]
    public string Username { get; set; }

    [Column("password_hash")]
    public string PasswordHash { get; set; }

    [Column("email")]
    public string Email { get; set; }

    [Column("role")]
    public string Role { get; set; } 
    
    [Column("phone")]
    public string Phone { get; set; }
}
