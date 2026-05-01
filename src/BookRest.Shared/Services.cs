namespace BookRest.Shared;

public static class Services
{
    /// <summary>
    /// The name of the Database.
    /// This is the name of the database that will be created and used by the application.
    /// </summary>
    public const string Database = "BookRestDb";

    /// <summary>
    /// The value of the JWT Issuer.
    /// This is the value of the JWT configuration that will be used by the application.
    /// </summary>
    public const string JwtIssuer = "Jwt:Issuer";
    
    /// <summary>
    /// The value of the JWT Audience.
    /// This is the value of the JWT configuration that will be used by the application.
    /// </summary>
    public const string JwtAudience = "Jwt:Audience";
    
    /// <summary>
    /// The value of the JWT SecretKey.
    /// This is the value of the JWT configuration that will be used by the application.
    /// </summary>
    public const string JwtSecretKey = "Jwt:SecretKey";
}