using System.Reflection;

using Npgsql;
using FluentValidation;

using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

using Todo.Api.Exceptions;
using Todo.Api.Models.DTOs;

using Todo.Api.Authorization;

using Todo.Api.Services;
using Todo.Api.Validations;

using Todo.Api.Data;
using Todo.Api.Tenancy;
using Todo.Api.Middlewares;
using Todo.Api.Data.Factories;
using Todo.Api.Data.Configurations;

string ToDoSpecificOrigins = "ToDoSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();

string[]? origins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: ToDoSpecificOrigins,
        policy =>
        {
            policy.WithOrigins(origins!)
                .AllowAnyMethod()
                .WithHeaders("x-id-token", "content-type", "authorization");
        }
    );
});

builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

IConfigurationSection dbConnectionConfigs = builder.Configuration.GetSection("DbConnection");

string GetConnectionString(IConfigurationSection section)
{
    NpgsqlConnectionStringBuilder builder = new()
    {
        Host = section["Host"],
        Port = int.Parse(section["Port"] ?? "5432"),
        Database = section["Database"],
        Username = section["Username"],
        Password = section["Password"]
    };

    return builder.ConnectionString;
}

builder.Services.Configure<AdminDbOptions>(
    builder.Configuration.GetSection("DbConnection:Admin")
);

string todoConnectionString = GetConnectionString(dbConnectionConfigs.GetSection("Todo"));
string globalConnectionString = GetConnectionString(dbConnectionConfigs.GetSection("Global"));

builder.Services.AddDbContext<GlobalDbContext>(options =>
{
    options
        .UseNpgsql(globalConnectionString)
        .UseSnakeCaseNamingConvention();
});

builder.Services.AddScoped(sp =>
{
    TenantContext tenantContext = sp.GetRequiredService<TenantContext>();

    TenantInfo tenant = tenantContext.CurrentTenant ?? throw new NotFoundException("Tenant was not found.");

    ITenantDbContextFactory factory = sp.GetRequiredService<ITenantDbContextFactory>();

    return factory.Create(tenant.ConnectionString);
});

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(options =>
    {
        options.Audience = builder.Configuration["Auth0:Audience"];
        options.Authority = builder.Configuration["Auth0:Authority"];

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidTypes = ["at+jwt", "JWT"],
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                string expectedClientId = builder.Configuration["Auth0:ClientId"]!;
                string? obtainedClientId = context.Principal?.FindFirst("client_id")?.Value;

                if (obtainedClientId is null || obtainedClientId != expectedClientId)
                {
                    context.Fail("Invalid client_id.");
                }

                return Task.CompletedTask;
            }
        };
    });


builder.Services.AddSingleton<IAuthorizationHandler, AdminOnlyHandler>();

string? adminSub = builder.Configuration["Auth0:AdminSub"];

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.Requirements.Add(new AdminOnlyRequirement(adminSub));
    });

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, UserContext>();

builder.Services.AddScoped<IValidator<CreateUserRequest>, CreateUserRequestValidator>();
builder.Services.AddScoped<IValidator<CreateTaskRequest>, CreateTaskRequestValidator>();
builder.Services.AddScoped<IValidator<UpdateTaskRequest>, UpdateTaskRequestValidator>();
builder.Services.AddScoped<IValidator<CategoryRequest>, CategoryRequestValidator>();
builder.Services.AddScoped<IValidator<ClientRequest>, ClientRequestValidator>();


builder.Services.AddScoped<TenantContext>();
builder.Services.AddScoped<ITenantDbContextFactory, TenantDbContextFactory>();

builder.Services.AddScoped<ITenantProvisioningService, TenantProvisioningService>();
builder.Services.AddScoped<ITenantDeprovisioningService, TenantDeprovisioningService>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IDbCredentialsGenerator, DbCredentialsGenerator>();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ToDo API v1");
    });
}

app.UseHttpsRedirection();

app.UseExceptionHandler();

app.UseCors(ToDoSpecificOrigins);

app.UseAuthentication();

app.UseMiddleware<TenantResolutionMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
