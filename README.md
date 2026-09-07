# STS2_Hoolheyak

Slay the Spire 2 版霍尔海雅（HoolheyakMod）Mod 迁移工程。

## 项目简介

将《杀戮尖塔 1》的 HoolheyakMod 角色 Mod 迁移到《杀戮尖塔 2》。当前为第一轮代码迁移，包含完整工程骨架、角色、卡牌、能力、遗物、药水等类文件；部分依赖 STS2 新 UI/API 的复杂机制保留 TODO 或降级实现。

## 目录结构

```text
STS2_Hoolheyak/
├─ HoolheyakMod.Loader.csproj      加载器工程（生成 HoolheyakMod.dll）
├─ HoolheyakMod.csproj             主体工程（生成 Beta/Stable 实现 DLL）
├─ HoolheyakMod.sln
├─ HoolheyakMod.json               模组清单
├─ Loader/                      加载器源码
├─ HoolheyakModCore/               C# 实现代码
│  ├─ Cards/                    卡牌
│  ├─ Character/                角色与卡池
│  ├─ Powers/                   能力
│  ├─ Relics/                   遗物
│  ├─ Potions/                  药水
│  ├─ Events/                   事件
│  ├─ Monsters/                 怪物
│  ├─ Encounters/               遭遇
│  └─ Scripts/                  配置/工具
├─ HoolheyakMod/                Godot 资源、美术、音频、本地化
└─ Reports/                     迁移报告（位于仓库根目录）
```

## 构建

需要：
- .NET 9 SDK
- Godot 4.5.1 Mono
- Slay the Spire 2 游戏目录（并配置 `HoolheyakMod.csproj` / `HoolheyakMod.Loader.csproj` 中的 `Sts2Dir`）

```bash
# Beta（默认）
dotnet build HoolheyakMod.sln -c Release -p:GameBranch=Beta

# Stable
dotnet build HoolheyakMod.sln -c Release -p:GameBranch=Stable

# 只编译 C#，跳过 Godot PCK 导出
dotnet build HoolheyakMod.csproj -c Release -p:GameBranch=Stable -p:SkipGodotExport=true
```

## 当前状态

- 工程骨架、Loader、Stable/Beta 双 DLL 方案已完成。
- 82 张卡牌、34 个 Power、16 个遗物、3 个药水均已建立 C# 类；多数已实现主要效果，部分复杂机制保留 TODO。
- 事件/怪物/遭遇为占位实现。
- 本地化已转换为 STS2 JSON 格式（`eng`/`zhs`）。
- 已修复 net9.0 编译错误，并用本地 Stable/Beta 引用完成 C# 编译验证（0 错误/0 警告）。
- 仍需在装有 Godot 4.5.1 的本地环境执行完整构建、PCK 导出与冒烟测试。

## 报告

- `Reports/Migration_Plan.md`
- `Reports/API_Compatibility_Report.md`
- `Reports/Asset_Migration_Report.md`
- `Reports/Compatibility_Report.md`
