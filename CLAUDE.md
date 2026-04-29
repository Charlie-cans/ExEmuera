# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 语言偏好

默认使用中文进行思考和输出。所有对话、代码注释、解释说明均使用简体中文，除非代码本身（变量名、函数名、错误信息等）需要保持英文。

## Project Overview

uEmuera — Unity 2020.3.34f1 移植版 Emuera（era 游戏引擎）。原始 EM+EE 扩展版源码在 `D:/code/era/emuera.em.gitlab/`。

**目标：** 让 Unity 版支持 EM+EE 扩展指令，以运行 eratw-sub-modding 等魔改游戏。

## Build / Compile Check

### Unity Editor 未打开时（推荐）
```bash
"/c/Program Files/Unity/Hub/Editor/2020.3.49f1/Editor/Unity.exe" -batchmode -quit -nographics -projectPath "D:/code/era/uEmuera" -logFile "D:/code/era/unity_build.log"
```
编译成功输出 `BUILD OK`，失败则 `grep "error CS" D:/code/era/unity_build.log`。

### Unity Editor 已打开时
Unity Editor 锁定项目文件，batchmode 会报错退出。此时：
```bash
touch "D:/code/era/uEmuera/Assets/Scripts/<修改的文件>.cs"  # 触发自动重编译
# 等待10-15秒后检查 Editor.log：
grep "error CS" "/c/Users/charlie/AppData/Local/Unity/Editor/Editor.log" | tail -10
```

## Architecture

### 三层架构
1. **`Assets/Scripts/Emuera/`** — 核心引擎层（命名空间 `MinorShift.Emuera`）。源于 Emuera1824，对 Unity 无感知。
   - `GameProc/Function/` — 指令注册三件套：枚举(`BuiltInFunctionCode.cs`) → 实现(`Instraction.Child.cs`) → 注册(`FunctionIdentifier.cs`)
   - `GameData/Function/` — 式中函数方法：注册(`Creator.cs`) → 实现(`Creator.Method.cs`)
   - `GameData/Variable/` — 变量系统，`VariableData.cs` 持有所有运行时数据
   - `PluginSystem/` — 插件系统（`IPluginMethod.cs`、`PluginManager.cs`），2026年新增
2. **`Assets/Scripts/uEmuera/`** — Unity 渲染/平台层。模拟 WinForms/GDI+ API，桥接引擎到 Unity。
   - `Window.cs` — `MainWindow` 类，持有 Sound 系统（线程安全队列 + Unity AudioSource）
3. **`Assets/Scripts/*.cs`**（根级别）— Unity MonoBehaviour 入口和管理器

### 引导流程
`MainEntry.Awake()` → `FirstWindow.Show()`（游戏选择器）→ 选游戏 → `FirstWindow.Run()` → `SpriteManager.Init()` → `EmueraMain.Run()` → `EmueraThread.Work()` → `Program.Main()` → 创建 `MainWindow` → `EmueraConsole` → `Process` → 加载 ERB/ERH/CSV → 执行脚本

### 指令注册流程
1. `BuiltInFunctionCode.cs` — 定义 `FunctionCode` 枚举值
2. `Instraction.Child.cs` — 实现 `AbstractInstruction` 子类
3. `FunctionIdentifier.cs` — `addFunction()` 注册枚举→实现映射
4. `Creator.Method.cs` — 实现 `FunctionMethod` 子类（式中函数，如 `GETCHARA`、`XML_GET`）
5. `Creator.cs` — 在静态字典中注册函数名→`FunctionMethod` 映射
6. `FunctionArgType.cs` + `ArgumentBuilder.cs` — 参数类型和构建器

### 变量数据存储（VariableData.cs 关键字段）
- `DataTables` — `Dictionary<string, DataTable>`（DT_* 系列）
- `DataStringMaps` — `Dictionary<string, Dictionary<string, string>>`（MAP_* 系列）
- `DataXmlDocument` — `Dictionary<string, System.Xml.XmlDocument>`（XML_* 系列）
- `dataInteger/dataString` — 标量变量
- `dataIntegerArray/dataStringArray` — 1D 数组（`VariableToken.SetValue(val, new Int64[]{index})`）
- `characterList` — 角色数据

### 游戏数据加载
- 游戏目录：`D:/butter/era/eratw-sub-modding-main/`
- 图像资源目录：`<游戏目录>/resources/`（`Program.ContentDir`）
- 要求编码：UTF-8
- 入口：`FirstWindow.cs` → 扫描 `MainEntry.era_path` 下含 `emuera.config` 或 `ERB/` 的子目录

## EM+EE 移植进度

详见 `TODO.md` 和 memory 文件。当前状态（2026-04-29）：

### 已完整实现（非桩）
- **XML 系统** — `System.Xml.XmlDocument`，全部指令（DOCUMENT/GET/SET/ADDNODE/REMOVENODE/REPLACE/ADDATTRIBUTE/REMOVEATTRIBUTE）
- **Sound 系统** — Unity AudioSource + `ConcurrentQueue<SoundCommand>` 线程安全队列，主线程 `ProcessSoundQueue()` 处理
- **MAP 系统** — `Dictionary<string, Dictionary<string, string>>`，全部指令
- **DataTable 系统** — `System.Data.DataTable`
- **SQLite 系统** — Mono.Data.Sqlite + P/Invoke sqlite3.dll
- **CALLSHARP / Plugin 系统** — 反射加载 `Plugins/*.dll`，`IPluginMethod` 接口
- **VARSETEX / ARRAYMSORTEX** — 批量变量设置/多数组排序
- **HTML_STRINGLEN/SUBSTRING/STRINGLINES** — HTML 标签处理后再操作
- **SETBIT/CLEARBIT/INVERTBIT** — 位操作
- **ENUMFILES / EXISTVAR / EXISTFUNCTION** — 辅助函数

### 仍为桩或未实现
- `<div>` 完整渲染（内联内容正常，无边框/阴影/背景色）
- `CALLSHARP` 的 `SP_CALLCSHARP` 参数构建器在 Unity 版未注册（当前走 ScriptProc.cs 直接处理）
- HTML_PRINT 表达式 `{EXPR}` 求值
- INPUTANY/BINPUT/TOOLTIP/SETBGIMAGE 扩展

## 已知陷阱

### Creator.cs 重复注册
`Creator.cs` 中同一函数名出现两次时，**后者覆盖前者**。曾导致 `HTML_TOPLAINTEXT`、`HTML_GETPRINTEDSTR`、`HTML_POPPRINTINGSTR` 的正确实现被末尾的 Stub 覆盖。添加新函数时务必检查是否已存在。

### FunctionMethod 参数数量
- 设置 `argumentTypeArray` 时，基类自动校验参数数量。**不要**设死参数数量如果函数支持可选参数。
- 正确做法：不设 `argumentTypeArray`（设 `CanRestructure = true`），重写 `CheckArgumentType` 自行校验。
- 示例：`HTML_STRINGLEN` 接受1～2个参数，通过自定义 `CheckArgumentType` 处理。

### VariableToken.SetValue 签名
`SetValue(Int64 value, Int64[] arguments)` — 第二个参数是索引数组，不是单个索引。
- 1D: `SetValue(val, new Int64[]{i})`
- 2D: `SetValue(val, new Int64[]{idx1, i})`
- 3D: `SetValue(val, new Int64[]{idx2, idx1, i})`
- 用 `SetValueAll(val, start, end, charaPos)` 做范围设置

### Sound 线程安全
Sound 方法由引擎后台线程调用，Unity API 只能在主线程使用。当前方案：`ConcurrentQueue` 入队 → `MainWindow.Update()` 主线程出队处理 → `GenericUtils.StartCoroutine` 加载 AudioClip。

### 配置键名
Unity 版使用 `ConfigCode` 枚举的 `ToString()` 作为键名（如 `CompatiErrorLine`），**不是**桌面版用的日文名或英文别名。

### PRINT 指令命名
`PRINT_Instruction` 构造函数解析名称必须匹配固定模式。不能随意添加 PRINT* 变体——**不要**对新的 PRINT 类指令用 `addPrintFunction()`，用 `addFunction()` + 通用 `ArgumentBuilder`。

### WebP 解码
- libwebp DLL 已替换为 v1.6.0（`Assets/unity.webp/Plugins/x64/`），新增依赖 `libsharpyuv.dll`
- 动画 WebP 降级为取第一帧（`Texture2DExt.cs` 中的 `AnimatedWebPHelper`）
- `SpriteManager.cs` 中 WebP 解码已加 try-catch 保护

### 运行时错误调试
游戏 `emuera.log` 只显示外层异常，**Unity Editor.log** (`%LOCALAPPDATA%/Unity/Editor/Editor.log`) 才含有内层异常细节。

## 添加新 EM+EE 函数的检查清单

添加一个新的 `FunctionMethod`：
1. `BuiltInFunctionCode.cs` — 添加 `FunctionCode` 枚举值（EM+EE 区域）
2. `FunctionIdentifier.cs` — `addFunction()` 注册指令
3. `Creator.Method.cs` — 实现 `FunctionMethod` 子类
4. `Creator.cs` — 在 `methodList` 字典中注册
5. 如需要新的参数类型 → `FunctionArgType.cs` + `ArgumentBuilder.cs`
6. 如需要运行时数据存储 → `VariableData.cs`
7. 编译验证 + 运行测试
