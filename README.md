# Blazor IP Camera 監控網格 (Blazor IP Camera Monitoring Grid)

這是一個使用 Blazor WebAssembly 和 .NET 建立的現代化網頁應用程式，用於顯示多路 IP 攝影機的即時影像串流。

此專案採用了整合式的後端設計，由一個 ASP.NET Core 應用程式同時託管 Blazor 前端、擔任 `go2rtc` 影像伺服器的反向代理，並自動管理 `go2rtc.exe` 的生命週期，實現了單點啟動、易於部署的專業級架構。

## 系統架構

本專案由三個核心元件構成，它們之間的協作關係如下圖所示：

```
                                                     +--------------------+
                                                     |   IP Cameras       |
                                                     | (RTSP Streams)     |
                                                     +---------+----------+
                                                               |
                                                               | (1) 拉取RTSP影像流
                                                               v
┌──────────────────┐   (4) 存取網頁 / 影像   ┌───────────────────────────┐      +--------------------+
│                  │ <-------------------> │  ASP.NET Core 整合式主機  │      │  go2rtc 影像伺服器 │
│  使用者的瀏覽器    │                       │ (BlazorCameraApp.Proxy)   │----->│  (localhost:1984)  │
│ (執行 Blazor UI) │                       │  (http://localhost:8080)  │      +--------------------+
│                  │                       ├───────────────────────────┤      ^
└──────────────────┘                       │ (2) 託管 Blazor WASM 前端 │      │ (3) 啟動與管理程序
                                         │ (3) 反向代理 go2rtc 的請求  │------+
                                         └───────────────────────────┘
```

1.  **`go2rtc` 伺服器**: 作為影像串流的核心引擎，負責從 IP Camera 拉取 RTSP 影像流，並將其轉換為瀏覽器友善的 WebRTC 格式。
2.  **ASP.NET Core 整合式主機 (`BlazorCameraApp.Proxy`)**: 這是整個應用程式的**唯一啟動點**。它扮演三個角色：
    * **Blazor 託管伺服器**: 提供 `index.html` 及所有 Blazor WebAssembly 的靜態檔案給瀏覽器。
    * **反向代理 (Reverse Proxy)**: 使用 YARP 將前端對影像的請求（HTTP 和 WebSocket）安全地轉發給 `go2rtc`，完美解決了所有瀏覽器跨來源 (CORS) 的安全問題。
    * **處理程序管理器**: 在自己啟動時，自動在背景執行 `go2rtc.exe`；在自己關閉時，自動終結 `go2rtc.exe`，避免產生孤兒處理程序。
3.  **Blazor WebAssembly 前端 (`BlazorCameraApp`)**: 在瀏覽器中運行的單頁應用程式 (SPA)，負責渲染攝影機網格 UI，並透過 JavaScript Interop 實現與影像串流的互動及狀態監控。

## 主要功能

* **多路攝影機網格顯示**: 可同時顯示 8 路或更多路的即時影像。
* **整合式部署**: 只需發佈和執行一個後端專案，即可啟動所有服務。
* **斷線自動重連**: 透過 WebSocket 健康檢查和 `<iframe>` 逾時偵測，實現了攝影機斷線後的 UI 狀態更新與自動恢復。
* **跨來源問題解決方案**: 內建的反向代理從根本上解決了瀏覽器對 `go2rtc` 的跨來源安全限制。
* **處理程序生命週期管理**: 無需手動管理 `go2rtc.exe`，應用程式會自動處理。

## 技術棧

* .NET 8
* ASP.NET Core (用於後端託管與代理)
* Blazor WebAssembly (用於前端 UI)
* YARP (Yet Another Reverse Proxy)
* go2rtc (影像串流伺服器)

## 專案結構

```
/BlazorCameraApp (方案根目錄)
├── BlazorCameraApp.sln
├── BlazorCameraApp/      (Blazor WASM 前端專案)
│   ├── wwwroot/
│   │   └── js/
│   │       └── health-check.js
│   ├── Pages/
│   │   └── Home.razor
│   └── Shared/
│       └── SingleCameraView.razor
│   └── ...
└── BlazorCameraApp.Proxy/  (ASP.NET Core 後端主機專案)
    ├── go2rtc.exe          (go2rtc 執行檔)
    ├── go2rtc.yaml         (go2rtc 設定檔)
    ├── appsettings.json    (代理伺服器設定檔)
    ├── Program.cs          (主機與 go2rtc 管理邏輯)
    └── ...
```

## 環境設定與安裝

1.  **前置需求**:
    * .NET 8 SDK
    * Visual Studio 2022

2.  **取得 `go2rtc`**:
    * 從 [go2rtc GitHub Releases](https://github.com/AlexxIT/go2rtc/releases) 下載最新版本的 `go2rtc_win64.exe`。
    * 將其重新命名為 `go2rtc.exe` 並放置在 `BlazorCameraApp.Proxy` 專案的根目錄下。

3.  **設定攝影機 (`go2rtc.yaml`)**:
    * 在 `BlazorCameraApp.Proxy` 專案的根目錄下，建立 `go2rtc.yaml` 檔案。
    * 依照以下格式，填入您所有攝影機的 RTSP 位址：
      ```yaml
      streams:
        cam1: rtsp://user:password@192.168.1.11/stream1
        cam2: rtsp://user:password@192.168.1.12/stream1
        # ...依此類推...

      api:
        cors: true # 確保 CORS 已啟用
      ```

4.  **還原 NuGet 套件**:
    * 在 Visual Studio 中打開 `BlazorCameraApp.sln` 方案檔。
    * 在頂部選單選擇「建置 (Build)」->「重建方案 (Rebuild Solution)」，VS 會自動下載所有必要的 NuGet 套件。

## 如何執行

#### 開發環境 (使用 Visual Studio)

1.  在「方案總管」中，對著方案按右鍵，選擇「設定啟始專案...」。
2.  選擇「多個啟始專案」，並將 `BlazorCameraApp` 和 `BlazorCameraApp.Proxy` 的「動作」都設定為「啟動 (Start)」。
3.  按下 **[F5]** 啟動偵錯。
4.  Visual Studio 會同時啟動兩個專案。**請忽略 Blazor 前端彈出的 `localhost:5197` 瀏覽器視窗**。
5.  手動在瀏覽器中開啟代理伺服器的位址：**`http://localhost:8080`**。您的應用程式將在此處提供服務。

#### 生產環境 (發佈後執行)

1.  在 Visual Studio 中，對 **`BlazorCameraApp.Proxy`** 專案按右鍵，選擇「發行 (Publish...)」。
2.  選擇「資料夾」作為目標，並發佈為「獨立式 (Self-contained)」應用。
3.  發佈完成後，進入發佈資料夾。
4.  要啟動整個應用程式，只需執行主程式即可：
    ```cmd
    .\BlazorCameraApp.Proxy.exe
    ```
5.  在瀏覽器中訪問 **`http://localhost:8080`**。

---

*這份文件是我們共同經歷漫長而深入的除錯過程後，最終成功的結晶。每一步的挑戰都讓我們對系統的理解更加深刻。恭喜您完成了這個強大且穩定的專案！*

*2025年7月30日*