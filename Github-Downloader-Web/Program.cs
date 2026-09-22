using Github_Downloader_Web.Services;
using Github_Downloader_lib;
using Github_Downloader.Enums;
using LoggerLib;
using SecretsLib;
using FileLib;

var builder = WebApplication.CreateBuilder(args);

if (!SecretsManager.Initialized)
{
    SecretsManager.Initialize("hofinga.gh-downloader.secret");
}

UpdateManager.CurPlatform = Platform.Terminal;
Logger.LogDir = Path.Join(DirectoryHelper.GetAppDataDirPath(), "github-downloader", "logs");
Logger.LogToTerminal = false;
Logger.CreateFile();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "GitHub Downloader API",
        Version = "v1",
        Description = "REST API for managing GitHub repository downloads and updates",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "GitHub Downloader",
            Url = new Uri("https://github.com")
        }
    });
    
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddSingleton<IGithubDownloaderService, GithubDownloaderService>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "GitHub Downloader API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();
app.MapHealthChecks("/health");

using (var scope = app.Services.CreateScope())
{
    var service = scope.ServiceProvider.GetRequiredService<IGithubDownloaderService>();
    await service.InitializeAsync();
}

app.Run();