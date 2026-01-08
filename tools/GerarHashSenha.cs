// Script C# para gerar hash de senha usando BCrypt
// Compilar: dotnet build tools/GerarHashSenha.csproj
// Executar: dotnet run --project tools/GerarHashSenha.csproj [senha]

using BCrypt.Net;

var senha = args.Length > 0 ? args[0] : "123456";
var hash = BCrypt.Net.BCrypt.HashPassword(senha);

Console.WriteLine("=========================================");
Console.WriteLine("Gerador de Hash de Senha (BCrypt)");
Console.WriteLine("=========================================");
Console.WriteLine($"Senha: {senha}");
Console.WriteLine($"Hash: {hash}");
Console.WriteLine("=========================================");
Console.WriteLine();
Console.WriteLine("SQL para inserir no banco:");
Console.WriteLine($"UPDATE UsuariosToken SET SenhaHash = '{hash}' WHERE Login = 'seu_login';");
Console.WriteLine($"-- OU para inserir um novo usuário:");
Console.WriteLine($"INSERT INTO UsuariosToken (Id, Login, SenhaHash, Nome, Email, Ativo, DataCriacao)");
Console.WriteLine($"VALUES (NEWID(), 'seu_login', '{hash}', 'Nome do Usuário', 'email@exemplo.com', 1, GETUTCDATE());");
