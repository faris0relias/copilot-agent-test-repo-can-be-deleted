using Microsoft.AspNetCore.Builder;
using Relias.ContentLibraryService.Consumer.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Configure();

var app = builder.Build();
app.Configure();


app.Run();