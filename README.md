# 物流快递仓储后端（C#/.NET 8 Web API）

本项目为物流/快递/仓储的后端示例（仅后端），前端暂不创建。默认使用 EF Core InMemory 数据库，开箱即可运行。

## 环境要求
- 安装 .NET SDK 8.0（Windows 下建议安装 x64 版本）
  - 下载地址：`https://dotnet.microsoft.com/download/dotnet/8.0`
- PowerShell 或 CMD 终端

## 目录结构
- `src/Logistics.Api`：Web API 项目
- `Logistics.sln`：解决方案

## 运行
```bash
# 在解决方案根目录（含 Logistics.sln）
 dotnet restore
 dotnet build -c Debug
 dotnet run --project src/Logistics.Api
```
启动后默认打开 Swagger：
- `https://localhost:7149/swagger`（或终端输出的地址）

## 主要功能（示例）
- 仓库：创建、列表
- 产品：创建、列表
- 库存：收货、按仓库查询
- 订单：创建、查询、分配（扣减库存）
- 发运：对已分配订单发运，生成运单与跟踪号
- 轨迹：新增轨迹事件、按跟踪号查询

## 示例接口
- POST `api/warehouses` { name, address }
- GET `api/warehouses`
- POST `api/products` { sku, name }
- GET `api/products`
- POST `api/inventory/receive` { warehouseId, productId, quantity }
- GET `api/inventory/{warehouseId}`
- POST `api/orders` { customerId, items: [{ productId, quantity }] }
- GET `api/orders/{orderId}`
- POST `api/orders/{orderId}/allocate` { warehouseId }
- POST `api/shipments/ship/{orderId}` { courierCode }
- GET `api/shipments/{id}`
- POST `api/tracking/{shipmentId}/events` { status, location?, note? }
- GET `api/tracking/by-number/{trackingNumber}`

## 切换到真实数据库（可选）
- 替换 `Microsoft.EntityFrameworkCore.InMemory` 为实际提供器（如 SQL Server/SQLite）
- 在 `Program.cs` 中将 `UseInMemoryDatabase` 改为对应的 `UseSqlServer/UseSqlite`
- 添加迁移并更新数据库：
```bash
 dotnet tool install --global dotnet-ef
 dotnet ef migrations add InitialCreate --project src/Logistics.Api
 dotnet ef database update --project src/Logistics.Api
```

## 开发说明
- 服务类在 `Services/`，通过 DI 自动注册（名称以 `Service` 结尾）
- 实体在 `Models/`，上下文在 `Data/ApplicationDbContext.cs`
- 控制器在 `Controllers/`，已启用 Swagger 便于调试
