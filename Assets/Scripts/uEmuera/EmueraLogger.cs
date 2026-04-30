using System;
using System.IO;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;
using UnityEngine;

namespace uEmuera
{
    // Serilog 初始化 + Session 管理。日志调用直接用 Serilog 的 Log.ForContext(...)
    public static class EmueraLogger
    {
        private static string _sessionDir;
        public static string SessionDir => _sessionDir;

        public static void Init()
        {
            var baseDir = UnityEngine.Application.isEditor ? Environment.CurrentDirectory : UnityEngine.Application.persistentDataPath;
            _sessionDir = Path.Combine(
                baseDir, "Logs",
                DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
            Directory.CreateDirectory(_sessionDir);

            var logPath = Path.Combine(_sessionDir, "session.log");
            var errPath = Path.Combine(_sessionDir, "errors.log");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File(
                    path: logPath,
                    outputTemplate: "[{Timestamp:HH:mm:ss.fff}] [{Level:u3}] [{Tag}] {Message}{NewLine}{Exception}",
                    fileSizeLimitBytes: 256 * 1024 * 1024,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: 31,
                    encoding: System.Text.Encoding.UTF8,
                    flushToDiskInterval: TimeSpan.FromSeconds(2),
                    buffered: true)
                .WriteTo.File(
                    path: errPath,
                    outputTemplate: "[{Timestamp:HH:mm:ss.fff}] [{Level:u3}] [{Tag}] {Message}{NewLine}{Exception}",
                    restrictedToMinimumLevel: LogEventLevel.Warning,
                    fileSizeLimitBytes: 128 * 1024 * 1024,
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: 31,
                    encoding: System.Text.Encoding.UTF8,
                    flushToDiskInterval: TimeSpan.FromSeconds(1))
                .WriteTo.Sink(new UnityConsoleSink(), LogEventLevel.Information)
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
