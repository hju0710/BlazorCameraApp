using System.Diagnostics;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// 註冊 IHttpClientFactory，這是 ASP.NET Core 中使用 HttpClient 的最佳實踐
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

// --- ↓↓↓↓ 新增的健康檢查 API 端點 ↓↓↓↓ ---
app.MapGet("/health", async (IHttpClientFactory httpClientFactory) =>
{
    // 建立一個 HttpClient 實例
    var client = httpClientFactory.CreateClient();
    // 設定一個較短的超時時間，例如 3 秒
    client.Timeout = TimeSpan.FromSeconds(3);

    try
    {
        // 由代理伺服器在內部嘗試連線 go2rtc 的 API
        var response = await client.GetAsync("http://localhost:1984/api/streams");

        // 如果 go2rtc 回應成功，則我們的健康檢查也回報成功 (HTTP 200 OK)
        response.EnsureSuccessStatusCode();
        return Results.Ok("online");
    }
    catch (Exception)
    {
        // 如果連線 go2rtc 失敗 (任何原因)，則回報服務不可用 (HTTP 503 Service Unavailable)
        return Results.StatusCode((int)HttpStatusCode.ServiceUnavailable);
    }
});
// --- ↑↑↑↑ 健康檢查 API 結束 ↑↑↑↑ ---

app.MapReverseProxy();
app.MapFallbackToFile("index.html");

// --- go2rtc 程序管理程式碼 (保持不變) ---
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