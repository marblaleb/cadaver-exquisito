using CadaverExquisito.API.Application.Interfaces;
using CadaverExquisito.API.Application.Services;
using CadaverExquisito.API.Domain.Entities;
using CadaverExquisito.API.Infrastructure.Data;
using CadaverExquisito.API.Infrastructure.Data.Repositories;
using CadaverExquisito.API.Infrastructure.Notifications;
using CadaverExquisito.API.Middleware;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

try
{
    FirebaseApp.Create(new AppOptions
    {
        Credential = GoogleCredential.GetApplicationDefault(),
        ProjectId = builder.Configuration["Firebase:ProjectId"]
    });
}
catch (Exception ex)
{
    Console.WriteLine($"[WARNING] Firebase not initialized: {ex.Message}. Auth bypass via X-Dev-User-Id header is active in Development.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ICadaverRepository, CadaverRepository>();
builder.Services.AddScoped<IFragmentRepository, FragmentRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<INotificationService, FcmNotificationService>();
builder.Services.AddScoped<CadaverService>();
builder.Services.AddScoped<FragmentService>();

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.UseMiddleware<FirebaseAuthMiddleware>();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    if (!db.Users.Any(u => u.Id == "dev-user-001"))
    {
        db.Users.Add(new User { Id = "dev-user-001", Name = "Dev User", Email = "dev@local.test" });
        db.SaveChanges();
    }
}

app.Run();
