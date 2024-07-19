using Microsoft.EntityFrameworkCore;
using Translations.Models;
using Translations.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.Configure<TranslationsDatabaseSettings>(
    builder.Configuration.GetSection("TranslationsDatabase"));
builder.Services.AddSingleton<TranslationsService>();

builder.Services.AddControllers();
builder.Services.AddDbContext<LocalizedTextContext>(opt 
    => opt.UseInMemoryDatabase("LocalizedTextEntries"));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
