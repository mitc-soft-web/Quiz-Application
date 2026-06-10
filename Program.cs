using Microsoft.EntityFrameworkCore;
using Quiz_Application.Contract;
using Quiz_Application.DBCONTEXT;
using Quiz_Application.Implementation.Repository;
using Quiz_Application.Implementation.Service;
using Quiz_Application.Interface.Repository;
using Quiz_Application.Interface.Service;
using Quiz_Application.Interfaces.Repositories;
using Quiz_Application.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;


var builder = WebApplication.CreateBuilder(args);

var renderPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(renderPort))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{renderPort}");
}

builder.Services.AddControllersWithViews();

var quizConnectionString = NormalizePostgresConnectionString(
    builder.Configuration.GetConnectionString("QuizContext")
    ?? builder.Configuration["DATABASE_URL"]
    ?? string.Empty);

builder.Services.AddDbContext<QuizContext>(options =>
{
    options.UseNpgsql(
            quizConnectionString,
            npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(10, TimeSpan.FromSeconds(30), null);
            })
        .ConfigureWarnings(warnings =>
            warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
});


builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/User/Login";
        options.LogoutPath = "/User/Logout";
        options.Cookie.Name = "MSTechQuizCookie";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<ILanguageRepository, LanguageRepository>();
builder.Services.AddScoped<ILanguageService, LanguageService>();
builder.Services.AddScoped<IQuizRepository, QuizRepository>();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IAnswerRepository, AnswerRepository>();
builder.Services.AddScoped<ISuggestionRepository, SuggestionRepository>();
builder.Services.AddScoped<ISuggestionService, SuggestionService>();
builder.Services.AddScoped<IAnswerService, AnswerService>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IResultRepository, ResultRepository>();
builder.Services.AddScoped<IResultService, ResultService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<QuizContext>();
        dbContext.Database.Migrate();
    }
    catch (NpgsqlException ex) when (app.Environment.IsDevelopment())
    {
        logger.LogError(ex, "Could not connect to PostgreSQL. Start PostgreSQL locally on port 5432 or run: docker compose up -d");
        throw;
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Login}/{id?}"); 



app.Run();

static string NormalizePostgresConnectionString(string connectionString)
{
    if (!Uri.TryCreate(connectionString, UriKind.Absolute, out var uri) ||
        (uri.Scheme != "postgres" && uri.Scheme != "postgresql"))
    {
        return connectionString;
    }

    var userInfo = uri.UserInfo.Split(':', 2);
    var username = Uri.UnescapeDataString(userInfo.ElementAtOrDefault(0) ?? string.Empty);
    var password = Uri.UnescapeDataString(userInfo.ElementAtOrDefault(1) ?? string.Empty);
    var database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));

    return $"Host={uri.Host};Port={uri.Port};Database={database};Username={username};Password={password};SSL Mode=Require;Trust Server Certificate=true";
}
