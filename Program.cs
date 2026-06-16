using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OficinaMecanica.Api.Application.Gateways;
using OficinaMecanica.Api.Application.UseCases.Auth;
using OficinaMecanica.Api.Application.UseCases.Clientes;
using OficinaMecanica.Api.Application.UseCases.OrdensServico;
using OficinaMecanica.Api.Application.UseCases.PecasInsumos;
using OficinaMecanica.Api.Application.UseCases.Servicos;
using OficinaMecanica.Api.Application.UseCases.Veiculos;
using OficinaMecanica.Api.Application.Settings;
using OficinaMecanica.Api.Infrastructure.Gateways;
using Microsoft.EntityFrameworkCore;
using OficinaMecanica.Api.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings");
var jwtSettings = jwtSettingsSection.Get<JwtSettings>()!;
var jwtKey = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 4, 0))));
builder.Services.Configure<JwtSettings>(jwtSettingsSection);
builder.Services.Configure<AdminUserSettings>(builder.Configuration.GetSection("AdminUserSettings"));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(jwtKey),
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddScoped<IClienteGateway, ClienteGateway>();
builder.Services.AddScoped<IVeiculoGateway, VeiculoGateway>();
builder.Services.AddScoped<IServicoGateway, ServicoGateway>();
builder.Services.AddScoped<IPecaInsumoGateway, PecaInsumoGateway>();
builder.Services.AddScoped<IOrdemServicoGateway, OrdemServicoGateway>();
builder.Services.AddScoped<LoginUseCase>();
builder.Services.AddScoped<ObterTodosClientesUseCase>();
builder.Services.AddScoped<ObterClientePorIdUseCase>();
builder.Services.AddScoped<CriarClienteUseCase>();
builder.Services.AddScoped<AtualizarClienteUseCase>();
builder.Services.AddScoped<RemoverClienteUseCase>();
builder.Services.AddScoped<ObterTodosVeiculosUseCase>();
builder.Services.AddScoped<ObterVeiculoPorIdUseCase>();
builder.Services.AddScoped<CriarVeiculoUseCase>();
builder.Services.AddScoped<AtualizarVeiculoUseCase>();
builder.Services.AddScoped<RemoverVeiculoUseCase>();
builder.Services.AddScoped<ObterTodosServicosUseCase>();
builder.Services.AddScoped<ObterServicoPorIdUseCase>();
builder.Services.AddScoped<CriarServicoUseCase>();
builder.Services.AddScoped<AtualizarServicoUseCase>();
builder.Services.AddScoped<RemoverServicoUseCase>();
builder.Services.AddScoped<ObterTodasPecasInsumosUseCase>();
builder.Services.AddScoped<ObterPecaInsumoPorIdUseCase>();
builder.Services.AddScoped<CriarPecaInsumoUseCase>();
builder.Services.AddScoped<AtualizarPecaInsumoUseCase>();
builder.Services.AddScoped<RemoverPecaInsumoUseCase>();
builder.Services.AddScoped<ObterTodasOrdensServicoUseCase>();
builder.Services.AddScoped<ObterTempoMedioExecucaoUseCase>();
builder.Services.AddScoped<ObterOrdensPorCpfCnpjClienteUseCase>();
builder.Services.AddScoped<ObterOrdemServicoPorIdUseCase>();
builder.Services.AddScoped<CriarOrdemServicoUseCase>();
builder.Services.AddScoped<IniciarDiagnosticoUseCase>();
builder.Services.AddScoped<EnviarOrcamentoUseCase>();
builder.Services.AddScoped<AprovarOrcamentoUseCase>();
builder.Services.AddScoped<RecusarOrcamentoUseCase>();
builder.Services.AddScoped<CancelarOrdemServicoUseCase>();
builder.Services.AddScoped<FinalizarOrdemServicoUseCase>();
builder.Services.AddScoped<EntregarOrdemServicoUseCase>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    if (dbContext.Database.IsRelational())
    {
        dbContext.Database.Migrate();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}
