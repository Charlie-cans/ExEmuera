# EM+EE → Unity uEmuera 移植 TODO

## 已完成
- [x] BuiltInFunctionCode 扩展 (35+ 新指令)
- [x] DataTable 系统 (DT_CREATE/COLUMN_ADD/ROW_ADD/CELL_GET 等)
- [x] MAP 系统 (MAP_CREATE/SET/GET/HAS/REMOVE/GETKEYS 等) - 完整实现
- [x] XML 系统 (XML_DOCUMENT/GET/SET/ADDNODE/REMOVENODE/REPLACE 等) - 完整实现，使用 System.Xml
- [x] GETVAR/GETVARS/SETVAR 动态变量查找
- [x] CLEARMEMORY/GETMEMORYUSAGE
- [x] EXISTSOUND/EXISTFILE/EXISTFUNCTION - 完整实现
- [x] ENUMFILES/EXISTVAR - 完整实现
- [x] SETBIT/CLEARBIT/INVERTBIT 位操作 - 完整实现
- [x] HTML_STRINGLEN/HTML_SUBSTRING/HTML_STRINGLINES - 完整实现（HTML标签处理）
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
- [x] <div> 标签防崩溃桩
- [x] <shape> param px 后缀支持
- [x] CSV .als 别名文件加载 (loadAliases, ScriptPosition双参构造器)
- [x] HTML_PRINTFORM 新指令
- [x] HTML_PRINT 后缀变体 (HTML_PRINTC/PRINTL/PRINTLC/PRINTFORMC/PRINTFORMLC/PRINTBUTTONC/PRINTBUTTON_EXC)
- [x] HTML_PRINT 第2参数支持 (SP_HTML_PRINT builder, string + optional int)
- [x] TRYCALLFORMF / TRYCALLF 注册 (TRYCALLF_Instruction 静默失败版)
- [x] SQLite 系统 (Mono.Data.Sqlite + sqlite3.dll, SQL_CONNECT/EXECUTE_SCALAR_STRING/EXECUTE_SCALAR_LONG/EXECUTE_NON_QUERY/EXECUTE_READER/IMPORT_MAP_XML)
- [x] Sound 系统 (PLAYSOUND/STOPSOUND/PLAYBGM/STOPBGM/SETSOUNDVOLUME/SETBGMVOLUME) - Unity AudioSource完整实现
- [x] CALLSHARP / Plugin 系统 - 反射加载.NET DLL插件，IPluginMethod接口
- [x] VARSETEX / ARRAYMSORTEX - 从EM+EE移植完整实现
- [x] 全代码库注释中文化 (所有日文/英文注释 → 简体中文)

## 需要完整实现（非桩）

### 高优先级
- [ ] `<div>` 标签完整渲染
  - 需要移植 ConsoleDivPart (GDI+ → Unity UI)
  - MixedNum/StyledBoxModel/BoxBorder 等类
  - 圆角边框/阴影/背景色渲染

### 中优先级
- [ ] HTML_PRINT 完整表达式支持
  - `{EXPR}` 括号在 HTML 属性中的求值

- [ ] 字体大小动态调节 UI
  - OptionWindow menu_2 加字体 +/- 按钮

### 低优先级
- [ ] INPUTANY/BINPUT/ONEBINPUT 输入扩展
- [ ] TOOLTIP 扩展 (TOOLTIP_SETFONT 等)
- [ ] SETBGIMAGE/CLEARBGIMAGE/REMOVEBGIMAGE
- [ ] HTML_PRINT_ISLAND/HTML_PRINT_ISLAND_CLEAR

## 非 EE 标准扩展（无法从 EE 源码移植）

以下指令在 eratw-sub-modding 中使用，但 EE 桌面版源码中也不存在。可能来自社区 fork 或自定义插件：

- `GETSOUNDORBGMINFO` — 音乐补丁 `音乐播放.ERB`、TEST.ERB
- `TRYCCALLSTR` — TEST.ERB
- `GDRAWGWITHROTATE` — TEST.ERB (Skia 图形扩展)
- `SET_SKIA_QUALITY` — TEST.ERB (Skia 画质设置)

## 已知限制
- `<div>` 无视觉效果：只有功能桩，无边框/背景/布局
- 非 EE 标准扩展指令：见上方列表，均为 TEST.ERB / DLC 中使用，不影响主流程
