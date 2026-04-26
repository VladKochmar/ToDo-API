using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using ToDoApi.Data;
using ToDoApi.Exceptions;
using ToDoApi.Models.DTOs;
using ToDoApi.Services;
using ToDoApi.Validations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();

builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options
        .UseNpgsql(builder.Configuration["DbConnectionString"])
        .UseSnakeCaseNamingConvention();
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
            ValidTypes = ["at+jwt", "JWT"]
        };
    });

builder.Services.AddControllers();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, UserContext>();

builder.Services.AddScoped<IValidator<CreateUserRequest>, CreateUserRequestValidator>();
builder.Services.AddScoped<IValidator<CreateTaskRequest>, CreateTaskRequestValidator>();
builder.Services.AddScoped<IValidator<UpdateTaskRequest>, UpdateTaskRequestValidator>();
builder.Services.AddScoped<IValidator<CategoryRequest>, CategoryRequestValidator>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ITaskService, TaskService>();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
