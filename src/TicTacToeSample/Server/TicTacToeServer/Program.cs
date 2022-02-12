using TicTacToeServer.MatchFramework;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

MatchManager matchManager = new MatchManager();
builder.Services.AddSingleton(matchManager);

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.UseWebSockets();
app.MapControllers();

Console.WriteLine("ULS TicTacToe Server ready");

app.Run();
