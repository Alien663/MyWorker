# InitWorker - .NET Worker Service 原型專案

這是一個 .NET 8 背景服務 (Worker Service) 的原型專案，設計用於快速開發需要長時間運行的背景程式、定時任務或後端服務。

## 專案概述

此專案提供了一個完整的 Worker Service 架構範本，整合了常見的企業級應用需求：

- ✅ **背景服務框架** - 使用 .NET Hosted Service
- ✅ **依賴注入 (DI)** - 完整的服務註冊與生命週期管理
- ✅ **配置管理** - 支援多環境配置檔案與 User Secrets
- ✅ **Entity Framework Core** - 資料庫存取範例
- ✅ **HttpClient Factory** - HTTP 客戶端管理與 Proxy 設定
- ✅ **OAuth 2.0 認證** - 使用 Microsoft.Identity.Client 進行服務間認證
- ✅ **日誌記錄** - 整合 Microsoft.Extensions.Logging
- ✅ **模組化設計** - 服務分離至獨立專案

## 專案結構

```
InitWorker.sln                  # 方案檔
│
├── InitWorker/                 # 主要 Worker Service 專案
│   ├── Program.cs              # 應用程式入口與服務設定
│   ├── Worker.cs               # 背景服務執行邏輯
│   ├── InitWorker.csproj       # 專案檔
│   ├── appsettings.json        # 配置檔
│   ├── appsettings.Development.json  # 開發環境配置檔
│   └── Option/
│       └── ProxyOptions.cs     # Proxy 設定模型
│
├── SampleService1/             # 範例服務 1 - 資料庫存取服務
│   ├── Entities/
│   │   └── SampleContext.cs    # Entity Framework DbContext
│   ├── Interfaces/
│   │   └── ISampleService.cs   # 服務介面
│   └── Services/
│       └── SampleService.cs    # 服務實作
│
└── SampleService2/             # 範例服務 2 - HTTP 客戶端服務
    ├── Interfaces/
    │   └── IMyHttpClient.cs    # HTTP 客戶端介面
    ├── Services/
    │   └── MyHttpClient.cs     # HTTP 客戶端實作 (含 OAuth 認證)
    └── Options/
        └── SampleOption.cs     # OAuth 設定模型
```

## 快速開始

### 前置需求

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) 或更高版本
- (可選) SQL Server - 如果需要使用 Entity Framework 功能

### 1. Clone 專案

```bash
git clone <repository-url>
cd InitWorker
```

### 2. 設定配置檔

編輯 `InitWorker/appsettings.json` 或 `appsettings.Development.json`：

```json
{
  "ConnectionStrings": {
    "Default": "Server=your-server;Database=your-db;User Id=your-user;Password=your-password;"
  },
  "Proxy": {
    "IP": "http://your-proxy:port",
    "Account": "proxy-account",
    "Password": "proxy-password"
  },
  "SampleOption": {
    "Authority": "https://login.microsoftonline.com/tenant-id",
    "Resource": "https://your-resource.com",
    "Scope": "https://your-resource.com/.default",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret"
  },
  "MyProject": {
    "Client": {
      "ResourceURL": "https://api.example.com"
    }
  }
}
```

### 3. 開發環境機密資料 (可選)

使用 User Secrets 儲存敏感資訊：

```bash
cd InitWorker
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Default" "your-connection-string"
dotnet user-secrets set "SampleOption:ClientSecret" "your-secret"
```

### 4. 建置與執行

```bash
# 還原套件
dotnet restore

# 建置專案
dotnet build

# 執行應用程式
dotnet run --project InitWorker
```

## 核心功能說明

### 1. Worker Service (背景服務)

[Worker.cs](InitWorker/Worker.cs) 是背景服務的主要執行邏輯：

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    // 在這裡實作你的背景任務邏輯
    // 例如：定時執行、訊息佇列處理、資料同步等
}
```

**使用情境：**
- 定時任務執行
- 訊息佇列消費者
- 資料同步服務
- 監控與健康檢查

### 2. 依賴注入設定

[Program.cs](InitWorker/Program.cs) 中註冊所有服務：

```csharp
services.AddScoped<ISampleService, SampleService>();
services.AddHttpClient<IMyHttpClient, MyHttpClient>();
services.AddDbContext<SampleContext>(options => 
    options.UseSqlServer(connectionString)
);
```

**支援的生命週期：**
- `AddSingleton` - 單例模式
- `AddScoped` - 每個作用域一個實例
- `AddTransient` - 每次請求都建立新實例

### 3. HttpClient Factory 與 Proxy

專案整合了 `IHttpClientFactory`，支援：
- **Proxy 設定** - 透過配置檔設定 HTTP Proxy
- **OAuth 2.0 認證** - 自動取得與管理 Access Token
- **Base Address 設定** - 統一管理 API 端點

```csharp
services.AddHttpClient<IMyHttpClient, MyHttpClient>((sp, client) =>
{
    client.BaseAddress = new Uri(resourceURL);
})
.ConfigurePrimaryHttpMessageHandler(BuildHttpHandler);
```

### 4. Entity Framework Core

[SampleService1](SampleService1) 示範了 EF Core 的整合：

```csharp
services.AddDbContext<SampleContext>(options =>
{
    options.UseSqlServer(connectionString);
});
```

可用於：
- 資料庫 CRUD 操作
- 複雜查詢
- 交易管理

### 5. 配置管理

支援多層級配置：

1. **基礎配置** - `appsettings.json`
2. **環境配置** - `appsettings.Development.json`
3. **User Secrets** - 開發環境敏感資料
4. **環境變數** - 生產環境配置

配置優先順序：環境變數 > User Secrets > 環境配置檔 > 基礎配置檔

### 6. 日誌記錄

使用 `Microsoft.Extensions.Logging`：

```csharp
_logger.LogInformation("Initialization started.");
_logger.LogError(ex, "An error occurred.");
```

**日誌等級：**
- Trace
- Debug
- Information
- Warning
- Error
- Critical

## 如何客製化

### 新增自己的服務

1. **建立服務介面與實作：**

```csharp
// Interfaces/IMyService.cs
public interface IMyService
{
    Task DoSomethingAsync();
}

// Services/MyService.cs
public class MyService : IMyService
{
    private readonly ILogger<MyService> _logger;
    
    public MyService(ILogger<MyService> logger)
    {
        _logger = logger;
    }
    
    public async Task DoSomethingAsync()
    {
        // 實作邏輯
    }
}
```

2. **在 Program.cs 中註冊：**

```csharp
services.AddScoped<IMyService, MyService>();
```

3. **在 Worker.cs 中使用：**

```csharp
public class Worker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var myService = scope.ServiceProvider.GetRequiredService<IMyService>();
            await myService.DoSomethingAsync();
        }
    }
}
```

### 修改執行邏輯

在 [Worker.cs](InitWorker/Worker.cs) 的 `ExecuteAsync` 方法中修改：

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    // 一次性執行任務
    await RunOnceAsync();
    
    // 或定時執行
    while (!stoppingToken.IsCancellationRequested)
    {
        await DoWorkAsync();
        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
    }
}
```

### 新增配置選項

1. **建立配置類別：**

```csharp
public class MyOptions
{
    public string Setting1 { get; set; }
    public int Setting2 { get; set; }
}
```

2. **在 appsettings.json 新增配置：**

```json
{
  "MyOptions": {
    "Setting1": "value",
    "Setting2": 123
  }
}
```

3. **註冊配置：**

```csharp
services.Configure<MyOptions>(config.GetSection("MyOptions"));
```

4. **注入使用：**

```csharp
public MyService(IOptions<MyOptions> options)
{
    var myOptions = options.Value;
}
```

## 部署

### Windows Service

將應用程式安裝為 Windows 服務：

1. 修改 `InitWorker.csproj`：

```xml
<PropertyGroup>
  <OutputType>Exe</OutputType>
  <TargetFramework>net8.0</TargetFramework>
</PropertyGroup>
```

2. 安裝為服務：

```bash
# 發佈應用程式
dotnet publish -c Release -o ./publish

# 使用 sc.exe 安裝服務
sc create "MyWorkerService" binPath="C:\path\to\publish\InitWorker.exe"
sc start "MyWorkerService"
```

### Docker 容器化

建立 `Dockerfile`：

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["InitWorker/InitWorker.csproj", "InitWorker/"]
COPY ["SampleService1/SampleService1.csproj", "SampleService1/"]
COPY ["SampleService2/SampleService2.csproj", "SampleService2/"]
RUN dotnet restore "InitWorker/InitWorker.csproj"
COPY . .
WORKDIR "/src/InitWorker"
RUN dotnet build "InitWorker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "InitWorker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "InitWorker.dll"]
```

建置與執行：

```bash
docker build -t initworker .
docker run -d --name my-worker initworker
```

### Linux 系統服務 (systemd)

建立服務檔案 `/etc/systemd/system/initworker.service`：

```ini
[Unit]
Description=Init Worker Service
After=network.target

[Service]
Type=notify
User=your-user
WorkingDirectory=/opt/initworker
ExecStart=/usr/bin/dotnet /opt/initworker/InitWorker.dll
Restart=always
RestartSec=10

[Install]
WantedBy=multi-user.target
```

啟動服務：

```bash
sudo systemctl daemon-reload
sudo systemctl enable initworker
sudo systemctl start initworker
sudo systemctl status initworker
```

## 常見問題

### Q1: 如何設定定時執行任務？

使用 `Timer` 或 `PeriodicTimer` (推薦 .NET 6+)：

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));
    
    while (!stoppingToken.IsCancellationRequested && 
           await timer.WaitForNextTickAsync(stoppingToken))
    {
        await DoWorkAsync();
    }
}
```

### Q2: 如何處理服務中的依賴注入作用域？

在 Worker 中建立作用域：

```csharp
public class Worker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var scopedService = scope.ServiceProvider
                    .GetRequiredService<IScopedService>();
                await scopedService.DoWorkAsync();
            }
            await Task.Delay(1000, stoppingToken);
        }
    }
}
```

### Q3: 如何優雅地關閉服務？

使用 `IHostApplicationLifetime`：

```csharp
public Worker(IHostApplicationLifetime lifetime)
{
    lifetime.ApplicationStopping.Register(() =>
    {
        // 清理資源
    });
}
```

### Q4: 如何除錯 Worker Service？

在 Visual Studio 中直接執行，或使用命令列：

```bash
dotnet run --project InitWorker --environment Development
```

查看日誌輸出以進行除錯。

## 相關資源

- [.NET Worker Services 官方文件](https://docs.microsoft.com/aspnet/core/fundamentals/host/hosted-services)
- [Entity Framework Core 文件](https://docs.microsoft.com/ef/core/)
- [依賴注入最佳實踐](https://docs.microsoft.com/dotnet/core/extensions/dependency-injection)
- [配置管理指南](https://docs.microsoft.com/aspnet/core/fundamentals/configuration/)

## 授權

此專案為內部原型專案，供學習與開發使用。

## 聯絡方式

如有問題或建議，請聯絡專案維護者。

---

**注意事項：**
- 請勿將敏感資訊（密碼、金鑰等）提交至版本控制系統
- 使用 User Secrets 或環境變數管理敏感資料
- 定期更新套件以確保安全性
- 在生產環境部署前，請務必進行充分測試
