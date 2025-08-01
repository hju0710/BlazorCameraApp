// Models/CameraInfo.cs

namespace BlazorCameraApp.Models;

// 使用 C# record 提供一個輕量級、不可變的資料結構
public record CameraInfo(
    string Id,          // 唯一的識別碼，例如 "cam1"
    string Name,        // 顯示在畫面上的名稱，例如 "大門口"
    string StreamName   // 在 go2rtc.yaml 中設定的串流名稱
);
