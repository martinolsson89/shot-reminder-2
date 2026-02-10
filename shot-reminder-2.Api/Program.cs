using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using shot_reminder_2.Api.Middleware;
using shot_reminder_2.Application.Interfaces;
using shot_reminder_2.Application.Options;
using shot_reminder_2.Application.Use_Cases.Auth.Login;
using shot_reminder_2.Application.Use_Cases.Auth.Register;
using shot_reminder_2.Application.Use_Cases.Inventory.AddStock;
using shot_reminder_2.Application.Use_Cases.Inventory.ConsumeOne;
using shot_reminder_2.Application.Use_Cases.Inventory.Delete;
using shot_reminder_2.Application.Use_Cases.Inventory.GetStock;
using shot_reminder_2.Application.Use_Cases.Inventory.Restock;
using shot_reminder_2.Application.Use_Cases.Inventory.Update;
using shot_reminder_2.Application.Use_Cases.Shots.Delete_Shot;
using shot_reminder_2.Application.Use_Cases.Shots.Get_Latest;
using shot_reminder_2.Application.Use_Cases.Shots.GetAll;
using shot_reminder_2.Application.Use_Cases.Shots.GetById;
using shot_reminder_2.Application.Use_Cases.Shots.Register_Shot;
using shot_reminder_2.Application.Use_Cases.Shots.Update_Shot;
using shot_reminder_2.Application.Use_Cases.Users;
using shot_reminder_2.Infrastructure.Auth;
using shot_reminder_2.Infrastructure.Presistence.Mongo.Context;
using shot_reminder_2.Infrastructure.Presistence.Mongo.Indexes;
using shot_reminder_2.Infrastructure.Presistence.Mongo.Options;
using shot_reminder_2.Infrastructure.Presistence.Mongo.Repositories;
using shot_reminder_2.Infrastructure.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var corsPolicyName = "Client";

// Add services to the container.
builder.Services.AddScoped<RegisterShotHandler>();
builder.Services.AddScoped<GetShotsHandler>();
builder.Services.AddScoped<CreateUserHandler>();
builder.Services.AddScoped<UpdateShotHandler>();
builder.Services.AddScoped<DeleteShotHandler>();
builder.Services.AddScoped<GetShotByIdHandler>();
builder.Services.AddScoped<GetLatestHandler>();
builder.Services.AddScoped<RegisterUserHandler>();
builder.Services.AddScoped<LoginHandler>();
builder.Services.AddScoped<AddStockHandler>();
builder.Services.AddScoped<RestockHandler>();
builder.Services.AddScoped<ConsumeOneHandler>();
builder.Services.AddScoped<DeleteInventoryHandler>();
builder.Services.AddScoped<UpdateStockHandler>();
builder.Services.AddScoped<GetStockHandler>();

builder.Services.AddTransient<IEmailSender, GmailSmtpEmailSender>();


builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IShotRepository, ShotRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<ShotSettings>(builder.Configuration.GetSection("ShotSettings"));


builder.Services.AddSingleton<ITokenService, JwtTokenService>();
builder.Services.AddSingleton<IPasswordHasher, PasswordHasherAdapter>();
builder.Services.AddSingleton<IMongoIndexInitializer, MongoIndexInitializer>();
builder.Services.AddHostedService<MongoIndexHostedService>();

builder.Services.Configure<GoogleOAuthOptions>(builder.Configuration.GetSection("Google"));
builder.Services.AddSingleton<GoogleCalendarClientFactory>();
builder.Services.AddSingleton<ICalendarService, GoogleCalendarService>();


builder.Services.AddTransient<ExceptionMiddleware>();

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName, policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5208",
                "https://localhost:7046")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.Configure<MongoOptions>(
    builder.Configuration.GetSection("Mongo"));

builder.Services.AddSingleton<IMongoDbContext, MongoDbContext>();

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtOptions>()!;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)

        };
    });

builder.Services.AddAuthorization();


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors(corsPolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ExceptionMiddleware>();

app.MapControllers();

app.Run();
