using System;
using System.IO;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace uEmuera
{
    // Serilog 初始化 + Session 管理。日志调用直接用 Serilog 的 Log.ForContext(...)
    public static class EmueraLogger
    {
        private static string _sessionDir;
        public static string SessionDir => _sessionDir;

        public static void Init()
        {
            _sessionDir = Path.Combine(
                Environment.CurrentDirectory, "Logs",
                DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
            Directory.CreateDirectory(_sessionDir);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(
                    path: Path.Combine(_sessionDir, "emuera.log"),
                    outputTemplate: "[{Timestamp:HH:mm:ss.fff}] [{Level:u3}] [{Tag}] {Message:lj}{Exception}",
                    fileSizeLimitBytes: 16 * 1024 * 1024,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: 9,
                    encoding: System.Text.Encoding.UTF8,
                    flushToDiskInterval: TimeSpan.FromSeconds(2),
                    buffered: true)
                .WriteTo.Sink(new UnityConsoleSink(), LogEventLevel.Debug)
                .Enrich.WithProperty("Tag", "App")
                .CreateLogger();

            Log.Debug("════════════════════════════════");
            Log.Information("会话开始 — {Dir}", _sessionDir);
        }

        public static void Flush() => Log.CloseAndFlush();

        public static void WriteFile(string filename, string content)
        {
            if (_sessionDir == null) return;
            try { File.WriteAllText(Path.Combine(_sessionDir, filename), content, System.Text.Encoding.UTF8); }
            catch { }
        }

        // Unity Console sink：结构化日志 → UnityEngine.Debug
        private sealed class UnityConsoleSink : ILogEventSink
        {
            private readonly MessageTemplateTextFormatter _f =
                new MessageTemplateTextFormatter("[{Timestamp:HH:mm:ss.fff}] [{Level:u3}] [{Tag}] {Message:lj}", null);

            public void Emit(LogEvent e)
            {
                var sw = new StringWriter();
                _f.Format(e, sw);
                switch (e.Level)
                {
                    case LogEventLevel.Error:
                    case LogEventLevel.Fatal:
                        UnityEngine.Debug.LogError(sw.ToString()); break;
                    case LogEventLevel.Warning:
                        UnityEngine.Debug.LogWarning(sw.ToString()); break;
                    default:
                        UnityEngine.Debug.Log(sw.ToString()); break;
                }
            }
        }
    }
}
