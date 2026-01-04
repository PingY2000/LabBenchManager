# LabBenchManager - 实验室测试台管理系统

`LabBenchManager` 是一个基于 Web 的内部应用程序，旨在简化和自动化实验室测试测试台的分配、调度、和管理流程。它为测试工程师、请求者和管理员提供了一个集中的平台来查看测试台状态、提交测试请求、管理测试计划和审批测试报告。


## ✨ 主要功能

*   **仪表板 (Dashboard)**: 快速概览所有设备的当前状态和利用率。
*   **设备管理 (Bench Management)**: 查看所有设备的详细信息、当前分配和未来计划。支持自定义排序。
*   **使用规划 (Assignment & Scheduling)**: 可视化的日历/日程表视图，用于分配和查看特定设备在特定时间段的测试任务。
*   **审批管理 (My Assignments)**: 管理员和测试工程师可以方便地查看分配给自己的所有测试任务。
*   **用户管理 (User Management)**: 管理员可以管理系统用户及其角色（如：管理员, 测试工程师, 请求者）。
*   **报告审批 (Report Approval)**: 支持测试报告的提交和审批工作流。
*   **测试计划历史 (Test Plan History)**: 跟踪和记录测试计划的变更历史。
*   **测试台文档管理 (Bench Documents)**: 为每个测试台设备上传和管理相关文档。

## 📖 核心工作流

本系统的核心业务逻辑遵循以下流程，涵盖从申请到报告审批的完整生命周期。

1.  **提交申请**:
    *   用户在 **申请测试页面** 填写并提交测试请求。
    *   此时，申请单的状态变为 **“待审批”**。

2.  **申请管理**:
    *   管理员在 **申请管理页面** 查看所有“待审批”的请求。
    *   管理员进行 **审批** 决策：
        *   **拒绝**: 申请单状态变为 **“已拒绝”**，流程结束。
        *   **批准**: 申请单状态变为 **“待分配”**。
    *   对于“待分配”的申请，管理员需要为其 **选择一个具体的测试台**。

3.  **使用规划与执行**:
    *   分配了测试台后，任务会出现在 **使用规划页面**（即日历或日程表视图）。
    *   任务状态会按以下顺序流转：
        *   **“未开始”**: 已分配但尚未规划具体时间。
        *   **“已规划”**: 已安排在日程表中。
        *   **“已确定”**: 最终确认，准备执行。
        *   **“已完成”**: 测试任务执行完毕。
    *   系统会 **保留对“已确定”和“已完成”任务的变更记录**。

4.  **报告审批**:
    *   当一个任务状态变为 **“已完成”** 后，可以为其 **创建报告审批** 流程。
    *   在 **报告审批页面**，报告的状态会按以下顺序流转：
        *   **“待审核”**
        *   **“待批准”**
        *   **“批准通过”**
    *   此审批流程与已完成的测试任务 **可关联**，形成完整的追溯链。

## 🛠️ 技术栈

*   **后端**: .NET 8 
*   **前端**: ASP.NET Core Blazor Server
*   **数据库**: SQL Server
*   **ORM**: Entity Framework Core
*   **身份认证**: Windows Authentication (Negotiate - Kerberos/NTLM)

## 🚀 快速开始

请按照以下步骤在本地设置和运行项目。

### 1. 先决条件

*   [.NET SDK] (推荐 .NET 8.0 或更高版本)
*   [Visual Studio 2022] 或其他代码编辑器 (如 VS Code)
*   SQL Server (可以是 Express, Developer Edition, 或 VS 自带的 LocalDB)

### 2. 克隆项目

```bash
git clone https://rb-msvs-alm.de.bosch.com/tfs/Col-DC-IH-ENG-CN/ENG1-CN/_git/LabMgmt
cd LabBenchManager
```

### 3. 配置数据库连接

1.  打开 `appsettings.Development.json` 文件。
2.  找到 `ConnectionStrings` 部分。
3.  修改 `Default` 连接字符串，指向你的 SQL Server 实例。

    *   **如果使用 SQL Server LocalDB (Visual Studio 自带):**
        ```json
        "ConnectionStrings": {
          "Default": "Server=(localdb)\\mssqllocaldb;Database=LabBenchManagerDb;Trusted_Connection=True;MultipleActiveResultSets=true"
        }
        ```
    *   **如果使用 SQL Server Express 或其他实例:**
        ```json
        "ConnectionStrings": {
          "Default": "Server=YOUR_SERVER_NAME;Database=LabBenchManagerDb;Trusted_Connection=True;TrustServerCertificate=True"
        }
        ```

### 4. 数据库迁移

项目已配置为在启动时自动应用数据库迁移。你只需要运行项目，Entity Framework Core 就会自动创建数据库和所有表。

### 5. 运行项目 (开发环境)

*   **使用 Visual Studio**:
    1.  用 Visual Studio 打开 `.sln` 文件。
    2.  确保启动配置文件设置为 `httpsd` 或 `LabBenchManager`。
    3.  按 `F5` 或点击 "启动" 按钮。

*   **使用 .NET CLI**:
    ```bash
    dotnet run
    ```

应用启动后，会自动打开浏览器。你可以通过以下 URL 方便地进行开发和测试：

*   **开发登录**: 访问 `https://localhost:[端口号]/dev/login` 来以 `apac\devuser` (管理员) 身份登录。
*   **切换角色**: 访问 `https://localhost:[端口号]/dev/switch-role?role=[角色名]` 来动态切换当前开发用户的角色。
    *   可选角色: `Admin`, `TestEngineer`, `Requester`
    *   示例: `/dev/switch-role?role=TestEngineer`

## 📦 生产环境部署

1.  将 `ASPNETCORE_ENVIRONMENT` 环境变量设置为 `Production`。
2.  在 `appsettings.Production.json` 中配置生产环境的数据库连接字符串。
3.  部署到支持 Windows Authentication 的服务器环境（如 IIS）。

## 📂 项目结构

```
/LabBenchManager
├── Auth/              # 身份认证相关，如 ClaimsTransformation
├── Components/        # 可复用的 Blazor 组件 (日历, 弹窗等)
├── Data/              # EF Core DbContext
├── Migrations/        # 数据库迁移文件
├── Models/            # 数据模型 (实体类)
├── Pages/             # Blazor 页面
├── Services/          # 业务逻辑服务
├── Shared/            # 共享布局和组件 (导航菜单等)
├── Utils/             # 工具类
├── wwwroot/           # 静态文件 (CSS, JS, 图像)
├── appsettings.json   # 配置文件
├── Program.cs         # 应用入口和配置
└── README.md          # 本文件
```