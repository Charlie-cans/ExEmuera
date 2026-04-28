# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

uEmuera — Unity 2020.3.34f1 移植版 Emuera（era 游戏引擎）。原始 Windows Forms 版源码在 `D:/code/era/emuera.em.gitlab/`（EM+EE 扩展版）。

**目标：** 让 Unity 版支持 EM+EE 扩展指令，以运行 eratw-sub-modding 等魔改游戏。

## Build / Compile Check

```
"C:/Program Files/Unity/Hub/Editor/2020.3.49f1/Editor/Unity.exe" -batchmode -quit -nographics -projectPath "D:/code/era/uEmuera" -logFile "D:/code/era/unity_build.log"
```

Unity Editor 打开时会锁定项目，batchmode 无法并行运行。编译成功后输出 `BUILD SUCCESS`。

## Architecture

### 双引擎层
- **`Assets/Scripts/Emuera/`** — 核心引擎逻辑（脚本解析、变量系统、指令执行）。来自原始 Emuera1824，已针对 Unity 修改。
  - `GameProc/Function/` — 指令注册 (`FunctionIdentifier.cs`)、实现 (`Instraction.Child.cs`)、枚举 (`BuiltInFunctionCode.cs`)
  - `GameData/Function/` — 函数方法 (`Creator.cs` 注册、`Creator.Method.cs` 实现)、表达式 (`Expression/`)
  - `GameData/Variable/` — 变量系统
  - `Config/` — 配置读取
- **`Assets/Scripts/uEmuera/`** — Unity 渲染层
  - `Window.cs` — `MainWindow` 类（Unity 音频桩、文本渲染）
  - `EmueraMain.cs` — Unity MonoBehaviour 入口
  - `EmueraContent.cs`, `EmueraThread.cs`, `FirstWindow.cs` — UI/线程管理
- **`Assets/unity.webp/`** — WebP 解码库（libwebp 1.6.0，已用 MSYS2 MinGW 编译替换）

### 指令注册流程
1. `BuiltInFunctionCode.cs` — 定义 `FunctionCode` 枚举值
2. `Instraction.Child.cs` — 实现 `AbstractInstruction` 子类
3. `FunctionIdentifier.cs` — `addFunction()` 注册枚举→实现映射
4. `Creator.Method.cs` — 实现 `FunctionMethod` 子类（式中函数）
5. `Creator.cs` — 注册函数名→`FunctionMethod` 映射
6. `FunctionArgType.cs` + `ArgumentBuilder.cs` — 参数解析

### 游戏数据加载
- 入口：`FirstWindow.cs` → 扫描 `MainEntry.era_path` + `Application.persistentDataPath` 下含 `emuera.config` 或 `ERB/` 的子目录
- 游戏目录：`D:/butter/era/eratw-sub-modding-main/`
- 要求编码：UTF-8

## EM+EE 移植进度

详见 memory 文件 `emuera_em_ee_port.md`。关键点：

- **已实现：** DataTable（System.Data.DataTable）、Sound 桩、CLEARMEMORY、EXISTSOUND/EXISTFILE
- **Stub（返回 0）：** MAP、XML、CALLSHARP
- **未实现：** SQLite（需 NuGet Microsoft.Data.Sqlite）

### 新增 FunctionCode 枚举位置
`BuiltInFunctionCode.cs` 尾部 `// EM+EE additions` 区域。添加新值时注意要同步：
1. `FunctionIdentifier.cs` 的 `#region EM+EE additions` 注册
2. 如有自定义参数类型 → `FunctionArgType.cs` + `ArgumentBuilder.cs`

## 已知陷阱

### 配置键名
Unity 版使用 `ConfigCode` 枚举的 `ToString()` 作为键名（如 `CompatiErrorLine`），**不是**桌面版用的日文名或英文别名。`emuera.config` 中写成 `CompatiErrorLine:YES`。

### PRINT 指令命名
`PRINT_Instruction` 构造函数解析名称必须匹配固定模式。不能随意添加 PRINT* 变体——**不要**对新的 PRINT 类指令用 `addPrintFunction()`，用 `addFunction()` + 通用 `ArgumentBuilder`。

### WebP 解码
- libwebp DLL 已替换为 v1.6.0（`Assets/unity.webp/Plugins/x64/`），新增依赖 `libsharpyuv.dll`
- 动画 WebP 降级为取第一帧（`Texture2DExt.cs` 中的 `AnimatedWebPHelper`）
- `SpriteManager.cs` 中 WebP 解码已加 try-catch 保护

### 运行时错误调试
游戏 `emuera.log` 只显示外层异常，**Unity Editor.log** (`%LOCALAPPDATA%/Unity/Editor/Editor.log`) 才含有内层异常细节。
