using System;
using uEmuera.Forms;
using uEmuera.Drawing;
using MinorShift.Emuera.GameProc;
using MinorShift.Emuera.GameView;
using MinorShift._Library;

namespace uEmuera.Window
{
    public class DebugDialog : IDisposable
    {
        public void Dispose()
        { }

        internal void SetParent(EmueraConsole emueraConsole, Process emuera)
        {
            throw new NotImplementedException();
        }

        internal void Show()
        {
            throw new NotImplementedException();
        }

        internal void Focus()
        {
            throw new NotImplementedException();
        }

        public bool Created { get { return true; } }
    }

    public class MainWindow : IDisposable
    {
        public static string uEmueraVer = "";

		// EM+EE Sound (Unity AudioSource stubs)
		private static UnityEngine.AudioSource sfxSource;
		private static UnityEngine.AudioSource bgmSource;
		private static void EnsureAudioSources()
		{
			if (sfxSource == null)
			{
				var go = new UnityEngine.GameObject("EmueraAudio");
				UnityEngine.Object.DontDestroyOnLoad(go);
				sfxSource = go.AddComponent<UnityEngine.AudioSource>();
				bgmSource = go.AddComponent<UnityEngine.AudioSource>();
				bgmSource.loop = true;
			}
		}
		public void PlaySound(string filename) { EnsureAudioSources(); var p = System.IO.Path.Combine(MinorShift._Library.Sys.ExeDir, "sound", filename); if (System.IO.File.Exists(p)) UnityEngine.Debug.Log("[Emuera] PLAYSOUND: " + p); }
		public void StopSound() { if (sfxSource != null) sfxSource.Stop(); }
		public void PlayBGM(string filename) { EnsureAudioSources(); var p = System.IO.Path.Combine(MinorShift._Library.Sys.ExeDir, "sound", filename); if (System.IO.File.Exists(p)) UnityEngine.Debug.Log("[Emuera] PLAYBGM: " + p); }
		public void StopBGM() { if (bgmSource != null) bgmSource.Stop(); }
		public void SetSoundVolume(int vol) { EnsureAudioSources(); sfxSource.volume = UnityEngine.Mathf.Clamp01(vol / 100.0f); }
		public void SetBGMVolume(int vol) { EnsureAudioSources(); bgmSource.volume = UnityEngine.Mathf.Clamp01(vol / 100.0f); }

        public MainWindow()
        {}

        public void Dispose()
        { }

        public void clear_richText()
        {
            //uEmuera.Logger.Info("MainWindow.clear_richText");
            //throw new NotImplementedException();
        }
        public void Focus()
        {
            //uEmuera.Logger.Info("MainWindow.Focus");
            //throw new NotImplementedException();
        }

        public void Refresh()
        {
            //uEmuera.Logger.Info("MainWindow.Refresh");
            dirty_ = true;
            if(console_ != null)
                console_.NeedSetTimer();
        }

        public void Close()
        {
            uEmuera.Logger.Info("MainWindow.Close");
            //throw new NotImplementedException();
        }
        public void update_lastinput()
        {
            uEmuera.Logger.Info("MainWindow.update_lastinput");
            //throw new NotImplementedException();
        }

        internal void Reboot()
        {
            uEmuera.Logger.Info("MainWindow.Reboot");
            //throw new NotImplementedException();
        }

        internal void ShowConfigDialog()
        {
            uEmuera.Logger.Info("MainWindow.ShowConfigDialog");
            //throw new NotImplementedException();
        }

        public void Init()
        {
            if(created_)
                return;
            created_ = true;
            console_ = new EmueraConsole(this);
            console_.Initialize();
        }
        public void Update()
        {
            //uEmuera.Logger.Info("MainWindow.Update");
            if(console_ == null)
                return;

            if(console_.IsInitializing)
            {
                ShowProcess();
                if(!dirty_)
                    return;
            }
            else if(console_.IsInProcess)
            {
                CheckProcess();
                if(wait_process && !EmueraThread.instance.IsSkipFlag)
                    return;
                if(!dirty_)
                    return;
            }
            else if(!dirty_)
            {
                return;
            }

            uEmuera.Logger.Info("MainWindow.Update Dirty");
            dirty_ = false;

            GenericUtils.SetBackgroundColor(console_.bgColor);

            var console_count = console_.GetDisplayLinesCount();
            if(console_count == 0)
            {
                //清空
                GenericUtils.ClearText();
                return;
            }

            bool need_update_flag = false;
            int prev = GenericUtils.GetTextMaxLineNo() - 1;
            int dis_lineno = prev;
            int index = 0;
            if(dis_lineno >= 0)
            {
                var cl = console_.GetDisplayLinesForuEmuera(console_count - 1);
                var con_lineno = cl.LineNo;
                var clindex = console_count - 1;

                //LineNo 匹配
                if(con_lineno > dis_lineno)
                    clindex -= (con_lineno - dis_lineno);
                else
                    dis_lineno = con_lineno;

                var min_lineno = GenericUtils.GetTextMinLineNo();
                while(dis_lineno >= min_lineno)
                {
                    cl = console_.GetDisplayLinesForuEmuera(clindex);
                    if(cl == null)
                        break;
                    var tl = GenericUtils.GetText(dis_lineno);
                    if(cl == tl)
                        break;
                    clindex -= 1;
                    dis_lineno -= 1;
                }
                if(prev > dis_lineno)
                {
                    var remove = prev - dis_lineno;
                    GenericUtils.RemoveTextCount(remove);
                    need_update_flag = true;
                }
                index = clindex + 1;
            }
            while(index < console_count)
            {
                var line = console_.GetDisplayLinesForuEmuera(index);
                if(line != null)
                    GenericUtils.AddText(line, line.LineNo <= prev);
                index += 1;
            }

            if(console_.IsWaitingEnterKey || console_.IsInProcess)
                GenericUtils.SetLastButtonGeneration(-1);
            else
                GenericUtils.SetLastButtonGeneration(console_.LastButtonGeneration);
            if(need_update_flag)
                GenericUtils.TextUpdate();

            GenericUtils.ShowIsInProcess(false);
            last_process_tic = 0;
        }

        private EmueraConsole console_ = null;
        private bool dirty_ = false;

        public string InternalEmueraVer { get { return uEmueraVer; } }
        public string EmueraVerText { get { return uEmueraVer; } }

        public bool Created { get { return created_; } }
        bool created_ = false;

        public ScrollBar ScrollBar = new ScrollBar();
        public PictureBox MainPicBox = new PictureBox();
        public string Text { get; set; }
        public ToolTip ToolTip = new ToolTip();
        public TextBox TextBox = new TextBox();

        void ShowProcess()
        {
            GenericUtils.ShowIsInProcess(true);
        }
        void CheckProcess()
        {
            var now = MinorShift._Library.WinmmTimer.TickCount;
            if(last_process_tic == 0)
                last_process_tic = now;
            else if(now - last_process_tic > 1500u)
            {
                GenericUtils.ShowIsInProcess(true);
            }
        }
        uint last_process_tic = 0;
        bool wait_process = true;
    }
}
