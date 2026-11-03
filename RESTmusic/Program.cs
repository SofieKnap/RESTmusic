using RESTmusic.Models;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton<ICollectionRepository, CollectionRepositoryList>();

const string AllowAllCORSpolicy = "AllowAll";
const string AllowLocalCORSpolicy = "AllowLocal";



builder.Services.AddCors(options =>
{
    options.AddPolicy(AllowAllCORSpolicy,

        policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });

    options.AddPolicy(AllowLocalCORSpolicy,
        policy =>
        {
            policy.WithOrigins("http://localhost:3000", "http://localhost:5500", "http://127.0.0.1:5500",
                    "https://frontmusiccollection-fwa7eybkd0f3hzg6.swedencentral-01.azurewebsites.net")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors(AllowLocalCORSpolicy);

app.UseAuthorization();

app.MapControllers();

app.Run(); 
