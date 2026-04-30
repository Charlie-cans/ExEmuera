using Serilog;
using System.Runtime.CompilerServices;

namespace uEmuera.Media.SystemSounds
{
    public static class Hand
    {
        public static void Play(string msg = null,
            [CallerFilePath] string file = null,
            [CallerLineNumber] int line = 0)
        {
            var caller = file != null ? $"{System.IO.Path.GetFileName(file)}:{line}" : "?";
            Log.ForContext("Tag", "System")
               .Warning("系统报错 [{Caller}] {Msg}", caller, msg ?? "(无详情)");
        }
    }
    public static class Asterisk
    {
        public static void Play()
        {
            Log.ForContext("Tag", "System").Debug("Asterisk.Play — 系统提示");
        }
    }
}
