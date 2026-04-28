# EM+EE → Unity uEmuera 移植 TODO

## 已完成
- [x] BuiltInFunctionCode 扩展 (35+ 新指令)
- [x] DataTable 系统 (DT_CREATE/COLUMN_ADD/ROW_ADD/CELL_GET 等)
- [x] MAP 系统 (MAP_CREATE/SET/GET/HAS/REMOVE/GETKEYS 等)
- [x] GETVAR/GETVARS/SETVAR 动态变量查找
- [x] CLEARMEMORY/GETMEMORYUSAGE
- [x] EXISTSOUND/EXISTFILE/EXISTFUNCTION
- [x] HTML_STRINGLEN/HTML_SUBSTRING/HTML_STRINGLINES
- [x] HTML_TOPLAINTEXT/HTML_GETPRINTEDSTR/HTML_POPPRINTINGSTR
- [x] GETMETH/GETMETHS/EXISTMETH (plugin stubs)
- [x] REGEXPMATCH stub
- [x] Config 查找大小写+空格不敏感 (GETCONFIG)
- [x] SPRITECREATE 范围限制缓和
- [x] 动画 WebP 降级取第一帧
- [x] libwebp DLL 升级到 1.6.0
- [x] 图片加载 15s 延迟 → 立即
- [x] 颜色名补全 40+ CSS 颜色
- [x] 无限循环检测不再弹 MessageBox
- [x] HTML_PRINTFORM 新指令
- [x] <div> 标签防崩溃桩
- [x] <shape> param px 后缀支持

## 需要完整实现（非桩）

### 高优先级
- [ ] SQLite 系统 (SQL_CONNECT/SQL_EXECUTE/SQL_IMPORT_MAP_XML 等)
  - 需要 NuGet Microsoft.Data.Sqlite
  - 涉及 FunctionMethod 注册 + 数据库 CRUD

- [ ] `<div>` 标签完整渲染
  - 需要移植 ConsoleDivPart (GDI+ → Unity UI)
  - MixedNum/StyledBoxModel/BoxBorder 等类
  - 圆角边框/阴影/背景色渲染

- [ ] CALLSHARP / Plugin 系统
  - 加载 .NET DLL 插件
  - IPluginMethod 接口

- [ ] 图片背景渲染修复
  - ConsoleImagePart 查找 sprite 失败问题
  - 追踪 GCREATEFROMFILE → SPRITECREATE → AppContents.GetSprite 链

### 中优先级
- [ ] Sound 系统 (PLAYSOUND/STOPSOUND/SETSOUNDVOLUME)
  - 当前 Unity AudioSource 桩，需文件加载 + 播放

- [ ] XML 系统 (XML_DOCUMENT/XML_GET/XML_SET/XML_ADDNODE 等)
  - 用 System.Xml 实现

- [ ] VARSETEX / ARRAYMSORTEX
  - 从 EM+EE Creator.Method.cs 移植

- [ ] HTML_PRINT 完整表达式支持
  - `{EXPR}` 括号在 HTML 属性中的求值

- [ ] 字体大小动态调节 UI
  - OptionWindow menu_2 加字体 +/- 按钮

### 低优先级
- [ ] INPUTANY/BINPUT/ONEBINPUT 输入扩展
- [ ] TOOLTIP 扩展 (TOOLTIP_SETFONT 等)
- [ ] SETBGIMAGE/CLEARBGIMAGE/REMOVEBGIMAGE
- [ ] HTML_PRINT_ISLAND/HTML_PRINT_ISLAND_CLEAR

## 已知限制
- 图片渲染白屏：sprite 查找失败，GCREATEFROMFILE 异步加载链问题
- `<div>` 无视觉效果：只有功能桩，无边框/背景/布局
- SQL 全桩：数据库操作返回 1 不报错但无实际效果
- Sound 全桩：不播放音频
