// Services/CameraService.cs
using BlazorCameraApp.Models;
using System.Collections.Generic;

// ↓↓↓↓ 這行命名空間宣告至關重要！它必須和你的資料夾結構匹配 ↓↓↓↓
namespace BlazorCameraApp.Services;

public class CameraService
{
    // 在真實應用中，這份清單可能來自於資料庫或設定檔
    private readonly List<CameraInfo> _cameras = new()
    {
        new CameraInfo("cam1", "大門口", "cam1"),
        new CameraInfo("cam2", "後院", "cam2"),
        new CameraInfo("cam3", "停車場入口", "cam3"),
        new CameraInfo("cam4", "倉庫 A", "cam4"),
        new CameraInfo("cam5", "辦公室", "cam5"),
        new CameraInfo("cam6", "走道", "cam6"),
        new CameraInfo("cam7", "機房", "cam7"),
        new CameraInfo("cam8", "機房", "cam8")

    };

    public IReadOnlyList<CameraInfo> GetCameras() => _cameras;
}