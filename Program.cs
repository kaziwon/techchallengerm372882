using OficinaMecanica.Api.Application.Services;
using OficinaMecanica.Api.Domain.Repositories;
using OficinaMecanica.Api.Application.Interfaces;
using OficinaMecanica.Api.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Api.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 4, 0))));

builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IVeiculoRepository, VeiculoRepository>();
builder.Services.AddScoped<IVeiculoService, VeiculoService>();
builder.Services.AddScoped<IServicoRepository, ServicoRepository>();
builder.Services.AddScoped<IServicoService, ServicoService>();
builder.Services.AddScoped<IPecaInsumoRepository, PecaInsumoRepository>();
builder.Services.AddScoped<IPecaInsumoService, PecaInsumoService>();
builder.Services.AddScoped<IOrdemServicoRepository, OrdemServicoRepository>();
builder.Services.AddScoped<IOrdemServicoService, OrdemServicoService>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

public partial class Program
{
}
