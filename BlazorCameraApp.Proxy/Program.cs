using System.Diagnostics;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// ���U IHttpClientFactory�A�o�O ASP.NET Core ���ϥ� HttpClient ���̨ι��
builder.Services.AddHttpClient();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseCors();
app.UseRouting();

// --- �������� �s�W�����d�ˬd API ���I �������� ---
app.MapGet("/health", async (IHttpClientFactory httpClientFactory) =>
{
    // �إߤ@�� HttpClient ���
    var client = httpClientFactory.CreateClient();
    // �]�w�@�Ӹ��u���W�ɮɶ��A�Ҧp 3 ��
    client.Timeout = TimeSpan.FromSeconds(3);

    try
    {
        // �ѥN�z���A���b�������ճs�u go2rtc �� API
        var response = await client.GetAsync("http://localhost:1984/api/streams");

        // �p�G go2rtc �^�����\�A�h�ڭ̪����d�ˬd�]�^�����\ (HTTP 200 OK)
        response.EnsureSuccessStatusCode();
        return Results.Ok("online");
    }
    catch (Exception)
    {
        // �p�G�s�u go2rtc ���� (�����])�A�h�^���A�Ȥ��i�� (HTTP 503 Service Unavailable)
        return Results.StatusCode((int)HttpStatusCode.ServiceUnavailable);
    }
});
// --- �������� ���d�ˬd API ���� �������� ---

app.MapReverseProxy();
app.MapFallbackToFile("index.html");

// --- go2rtc �{�Ǻ޲z�{���X (�O������) ---
Process? go2rtcProcess = null;
var exePath = Path.Combine(AppContext.BaseDirectory, "go2rtc.exe");
if (File.Exists(exePath))
{
    var startInfo = new ProcessStartInfo
    {
        FileName = exePath,
        WorkingDirectory = AppContext.BaseDirectory,
        CreateNoWindow = true,
        UseShellExecute = false,
        RedirectStandardInput = true
    };
    go2rtcProcess = Process.Start(startInfo);
    if (go2rtcProcess != null)
    {
        app.Logger.LogInformation("go2rtc process started with PID: {pid}", go2rtcProcess.Id);
    }
}

app.Run();