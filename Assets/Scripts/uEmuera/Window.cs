using System;
using System.Collections.Generic;
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

		// EM+EE 音频系统（线程安全队列 + 主线程处理）
		private enum SoundCmdType { PlaySound, StopSound, PlayBGM, StopBGM, SetSoundVolume, SetBGMVolume }
		private struct SoundCommand { public SoundCmdType Type; public string Filename; public int Volume; }
		private readonly System.Collections.Concurrent.ConcurrentQueue<SoundCommand> soundQueue = new System.Collections.Concurrent.ConcurrentQueue<SoundCommand>();
		private UnityEngine.AudioSource soundSource;
		private UnityEngine.AudioSource bgmSource;
		private int soundVolume = 100;
		private int bgmVolume = 100;

		private void EnsureAudioSources()
		{
			if (soundSource == null)
			{
				var go = new UnityEngine.GameObject("EmueraSound");
				UnityEngine.Object.DontDestroyOnLoad(go);
				soundSource = go.AddComponent<UnityEngine.AudioSource>();
				soundSource.loop = false;
				soundSource.playOnAwake = false;
			}
			if (bgmSource == null)
			{
				var go = new UnityEngine.GameObject("EmueraBGM");
				UnityEngine.Object.DontDestroyOnLoad(go);
				bgmSource = go.AddComponent<UnityEngine.AudioSource>();
				bgmSource.loop = true;
				bgmSource.playOnAwake = false;
			}
		}

		private void ProcessSoundQueue()
		{
			while (soundQueue.TryDequeue(out var cmd))
			{
				try
				{
					switch (cmd.Type)
					{
						case SoundCmdType.PlaySound:
							EnsureAudioSources();
							LoadAndPlay(soundSource, cmd.Filename, false, soundVolume);
							break;
						case SoundCmdType.StopSound:
							if (soundSource != null) soundSource.Stop();
							break;
						case SoundCmdType.PlayBGM:
							EnsureAudioSources();
							LoadAndPlay(bgmSource, cmd.Filename, true, bgmVolume);
							break;
						case SoundCmdType.StopBGM:
							if (bgmSource != null) bgmSource.Stop();
							break;
						case SoundCmdType.SetSoundVolume:
							soundVolume = cmd.Volume;
							if (soundSource != null) soundSource.volume = cmd.Volume / 100f;
							break;
						case SoundCmdType.SetBGMVolume:
							bgmVolume = cmd.Volume;
							if (bgmSource != null) bgmSource.volume = cmd.Volume / 100f;
							break;
					}
				}
				catch (System.Exception e) { UnityEngine.Debug.Log("[Emuera-Sound] 错误: " + e.Message); }
			}
		}

		private void LoadAndPlay(UnityEngine.AudioSource source, string filename, bool loop, int volume)
		{
			if (string.IsNullOrEmpty(filename)) return;
			string path = System.IO.Path.Combine(Sys.ExeDir, filename);
			if (!System.IO.File.Exists(path))
				path = System.IO.Path.Combine(Sys.ExeDir, filename);
			if (!System.IO.File.Exists(path))
			{
				// 尝试在 Resources 目录查找
				var ext = System.IO.Path.GetExtension(filename).ToLower();
				string nameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(filename);
				var clip = UnityEngine.Resources.Load<UnityEngine.AudioClip>(nameWithoutExt);
				if (clip != null)
				{
					source.clip = clip;
					source.loop = loop;
					source.volume = volume / 100f;
					source.Play();
					return;
				}
				UnityEngine.Debug.Log("[Emuera-Sound] 文件未找到: " + filename);
				return;
			}
			// 使用协程加载音频（由 EmueraMain 驱动）
			GenericUtils.StartCoroutine(LoadAudioCoroutine(source, path, loop, volume));
		}

		private System.Collections.IEnumerator LoadAudioCoroutine(UnityEngine.AudioSource source, string path, bool loop, int volume)
		{
			var ext = System.IO.Path.GetExtension(path).ToLower();
			UnityEngine.AudioType audioType = ext == ".ogg" ? UnityEngine.AudioType.OGGVORBIS
				: ext == ".wav" ? UnityEngine.AudioType.WAV
				: ext == ".mp3" ? UnityEngine.AudioType.MPEG
				: UnityEngine.AudioType.UNKNOWN;
			using (var www = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip("file://" + path, audioType))
			{
				yield return www.SendWebRequest();
				if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
				{
					var clip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(www);
					if (clip != null)
					{
						source.clip = clip;
						source.loop = loop;
						source.volume = volume / 100f;
						source.Play();
					}
				}
				else UnityEngine.Debug.Log("[Emuera-Sound] 加载失败: " + path + " - " + www.error);
			}
		}

		// 后台线程调用 - 添加命令到队列
		public void PlaySound(string filename) { soundQueue.Enqueue(new SoundCommand { Type = SoundCmdType.PlaySound, Filename = filename }); }
		public void StopSound() { soundQueue.Enqueue(new SoundCommand { Type = SoundCmdType.StopSound }); }
		public void PlayBGM(string filename) { soundQueue.Enqueue(new SoundCommand { Type = SoundCmdType.PlayBGM, Filename = filename }); }
		public void StopBGM() { soundQueue.Enqueue(new SoundCommand { Type = SoundCmdType.StopBGM }); }
		public void SetSoundVolume(int vol) { soundQueue.Enqueue(new SoundCommand { Type = SoundCmdType.SetSoundVolume, Volume = vol }); }
		public void SetBGMVolume(int vol) { soundQueue.Enqueue(new SoundCommand { Type = SoundCmdType.SetBGMVolume, Volume = vol }); }

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
            // 处理音频命令队列（主线程）
            ProcessSoundQueue();
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
