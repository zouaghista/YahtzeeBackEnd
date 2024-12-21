using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using YahtzeeBackEnd.Contexts;
using YahtzeeBackEnd.Entites;
using YahtzeeBackEnd.Hubs;
using YahtzeeBackEnd.Services.Registery;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin();
            builder.AllowAnyHeader();
            builder.AllowAnyMethod();
        });
});
builder.Services.AddSingleton<IGameRegistery, GameRegistery>();
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
});
var app = builder.Build();
app.MapHub<GameHub>("/Game");
app.Run();
