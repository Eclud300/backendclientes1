using Dapper;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Habilitar CORS para evitar bloqueos
builder.Services.AddCors(options => {
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();
app.UseCors();

// Leer la conexión desde el appsettings.json
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// ==========================================
// ENDPOINTS DE CLIENTES (CRUD)
// ==========================================

// GET: Obtener todos los clientes para llenar tu tabla
app.MapGet("/api/clientes", async () => {
    using var connection = new NpgsqlConnection(connectionString);
    var clientes = await connection.QueryAsync<Cliente>("SELECT * FROM Clientes ORDER BY Clave");
    return Results.Ok(clientes);
});

// GET: Buscar un cliente por su Clave
app.MapGet("/api/clientes/{clave}", async (string clave) => {
    using var connection = new NpgsqlConnection(connectionString);
    var cliente = await connection.QueryFirstOrDefaultAsync<Cliente>("SELECT * FROM Clientes WHERE Clave = @Clave", new { Clave = clave });
    return cliente != null ? Results.Ok(cliente) : Results.NotFound();
});

// POST: Insertar un cliente nuevo
app.MapPost("/api/clientes", async (Cliente c) => {
    using var connection = new NpgsqlConnection(connectionString);
    var sql = "INSERT INTO Clientes (Clave, Nombre, Edad, FechaNacimiento) VALUES (@Clave, @Nombre, @Edad, @FechaNacimiento)";
    await connection.ExecuteAsync(sql, c);
    return Results.Ok(new { mensaje = "Guardado en la nube" });
});

// PUT: Actualizar un cliente existente
app.MapPut("/api/clientes/{clave}", async (string clave, Cliente c) => {
    using var connection = new NpgsqlConnection(connectionString);
    var sql = "UPDATE Clientes SET Nombre = @Nombre, Edad = @Edad, FechaNacimiento = @FechaNacimiento WHERE Clave = @Clave";
    await connection.ExecuteAsync(sql, new { c.Nombre, c.Edad, c.FechaNacimiento, Clave = clave });
    return Results.Ok(new { mensaje = "Actualizado en la nube" });
});

// DELETE: Eliminar un cliente de la BD
app.MapDelete("/api/clientes/{clave}", async (string clave) => {
    using var connection = new NpgsqlConnection(connectionString);
    await connection.ExecuteAsync("DELETE FROM Clientes WHERE Clave = @Clave", new { Clave = clave });
    return Results.Ok(new { mensaje = "Eliminado de la nube" });
});

// ==========================================
// ENDPOINT DE LOGIN
// ==========================================
app.MapPost("/api/login", async (UsuarioLogin u) => {
    using var connection = new NpgsqlConnection(connectionString);
    var user = await connection.QueryFirstOrDefaultAsync<UsuarioLogin>(
        "SELECT * FROM Usuarios WHERE Usuario = @Usuario AND Password = @Password", u);

    // Si lo encuentra, devuelve un "acceso: true" para que Android lo lea
    return user != null ? Results.Ok(new { acceso = true }) : Results.Unauthorized();
});

app.Run();

// ==========================================
// MODELOS DE DATOS
// ==========================================
public class Cliente
{
    public string Clave { get; set; }
    public string Nombre { get; set; }
    public string Edad { get; set; }
    public string FechaNacimiento { get; set; }
}

public class UsuarioLogin
{
    public string Usuario { get; set; }
    public string Password { get; set; }
}