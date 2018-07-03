using System;
using System.IO;
using System.Drawing;
using System.Text;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ServiceStack.Redis;

namespace AppShareClip
{
    public partial class MainForm : Form
    {
        [DllImport("User32.dll")]
        protected static extern int SetClipboardViewer(int hWndNewViewer);

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString,int nMaxCount);

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, System.Text.StringBuilder retVal, int size, string filePath);


        IntPtr nextClipboardViewer;
        NotifyIcon AppShareClipIcon;
        Icon AppShareClipIconImage;
        ContextMenu contextMenu = new ContextMenu();

        bool reallyQuit = false;
        bool isCopying = false;
        string localIP = "127.0.0.1";
        string workPath = Environment.GetEnvironmentVariable("APPSIMULATOR_WORK_PATH");
        string redisServerIP = Environment.GetEnvironmentVariable("REDIS_SERVER_IP");
        int copyCnt = 0;
        int copyErrorCnt = 0;
        DateTime startTime = DateTime.Now;


        public string GetTimerNo()
        {
            string timerNo = File.ReadAllText(this.workPath + "\\Controller\\timerNo.conf");

            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            Int64 timeStamp = Convert.ToInt64(ts.TotalMilliseconds);
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long mTime = long.Parse(timeStamp + "0000");
            TimeSpan toNow = new TimeSpan(mTime);
            Console.WriteLine("<<GetTimerNo>>" + startTime.Add(toNow).ToString("HH:mm:ss ffff"));
            return timerNo;
        }

        public MainForm()
        {
            InitializeComponent();
            nextClipboardViewer = (IntPtr)SetClipboardViewer((int)this.Handle);
            if (this.clipboardHistoryList.Items.Count > 0)
                this.clipboardHistoryList.SetSelected(0, true);
            clipboardHistoryList.Select();

            this.TopMost = false; // 总在最前

            this.localIP = this.GetLocalIP();

            AppShareClipIconImage = Properties.Resources.clipboard;
            AppShareClipIcon = new NotifyIcon
            {
                Icon = AppShareClipIconImage,
                Visible = true
            };

            MenuItem quitMenuItem = new MenuItem("Quit");
            MenuItem showFormItem = new MenuItem("AppShareClip");
            this.contextMenu.MenuItems.Add(showFormItem);
            this.contextMenu.MenuItems.Add("-");
            this.contextMenu.MenuItems.Add(quitMenuItem);

            AppShareClipIcon.ContextMenu = contextMenu;

            quitMenuItem.Click += QuitMenuItem_Click;
            showFormItem.Click += ShowForm;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
        }

        static Int64 GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalMilliseconds);
        }

        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            const int WM_DRAWCLIPBOARD = 0x308;
            const int WM_CHANGECBCHAIN = 0x030D;
            // const int WM_CLIPBOARDUPDATE = 0x031D;
            // const int WM_COPY = 0x301;
            // const int WM_PASTE = 0x302;

            switch (m.Msg)
            {
                case WM_DRAWCLIPBOARD:
                    if (!isCopying)
                    {
                        Int64 timeStamp = GetTimeStamp();
                        string timerNo = GetTimerNo();
                        AddClipBoardEntry(timeStamp,timerNo);
                    }
                    StringBuilder sb = new StringBuilder(256);
                    if (GetWindowText(m.HWnd, sb, 256) > 0)
                    {
                        Console.WriteLine(sb.ToString());
                    }
                    break;
                case WM_CHANGECBCHAIN:
                    StringBuilder sb1 = new StringBuilder(256);
                    if (GetWindowText(m.HWnd, sb1, 256) > 0)
                    {
                        Console.WriteLine(sb1.ToString());
                    }
                    if (m.WParam == nextClipboardViewer)
                        nextClipboardViewer = m.LParam;
                    else
                        SendMessage(nextClipboardViewer, m.Msg, m.WParam, m.LParam);
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }
        private string GetLocalIP()
        {
            /*
            string strHostName = "";
            string ip = "";
            strHostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);
            IPAddress[] addr = ipEntry.AddressList;
            for(int i=0;i< addr.Length; i++)
            {
                if (addr[i].ToString().StartsWith("172"))
                {
                    ip = addr[i].ToString();
                    break;
                }
            }
            */
            string ip = Environment.GetEnvironmentVariable("APPSIMULATOR_IP");
            label1.Text = "将从 " + ip + " >> 传送到Redis:" + this.redisServerIP;
            return ip;
        }

        public string TimeStampToStr(Int64 timeStamp)
        {
            DateTime start = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long mTime = long.Parse(timeStamp.ToString() + "0000");
            TimeSpan toNow = new TimeSpan(mTime);
            //return start.Add(toNow).ToString("yyyy/MM/dd HH:mm:ss ffff");
            return start.Add(toNow).ToString("HH:mm:ss ffff");
        }

        private void AddClipBoardEntry(Int64 timeStamp, string timerNo)
        {
            if (Clipboard.ContainsText())
            {
                string strTimeStamp = TimeStampToStr(timeStamp);
                string clipboardText = Clipboard.GetText();
                if (!String.IsNullOrEmpty(clipboardText))
                {
                    if (clipboardText.Contains("http"))
                    {
                        clipboardText = clipboardText.Substring(clipboardText.LastIndexOf("http"));
                        try
                        {
                            Console.WriteLine("<<timerNo>> " + timerNo + "<<timeStamp>> " + strTimeStamp + "<<url>> " + clipboardText);
                            RedisClient Client = new RedisClient(this.redisServerIP, 6379);
                            Client.ChangeDb(11);
                            Client.AddItemToSet("devices:" + this.localIP + ":" + timerNo, clipboardText);
                            Client.AddItemToSet("devices:" + this.localIP + "_org:" + timerNo, clipboardText);
                            clipboardHistoryList.Items.Insert(0, clipboardText);
                            label1.Text = "从 " + this.localIP + " 传送到Redis： " + this.redisServerIP + " (" + timerNo + ")";
                            this.copyCnt++;
                        }
                        catch (Exception e)
                        {
                            this.copyErrorCnt++;
                            Console.WriteLine(e);
                        }
                        finally
                        {
                            toolStripStatusLabel.Text = "Copy成功：" + this.copyCnt.ToString() + " 个，失败：" + this.copyErrorCnt.ToString() +
                                                        " 个, 耗时: " + (DateTime.Now - this.startTime).ToString().Substring(0, 8);
                            deleteButton.Enabled = true;
                        }
                    }
                }
            }
            else
            {
                toolStripStatusLabel.Text = "History entry was not added because it was null or empty";
            }
        }


        private void ClearClipboardHistory()
        {
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to clear the clipboard history",
                                                        "Clear clipboard history?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dialogResult == DialogResult.Yes)
            {
                clipboardHistoryList.ClearSelected();
                clipboardHistoryList.Items.Clear();
                toolStripStatusLabel.Text = "剪贴板历史信息将被清空.";
            }
        }

        public void HandleExternalException(ExternalException e)
        {
            MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void GoDownList()
        {
            if (clipboardHistoryList.Items.Count > 1)
            {
                if (clipboardHistoryList.SelectedIndex == clipboardHistoryList.Items.Count - 1)
                {
                    clipboardHistoryList.SetSelected(0, true);
                }
                else
                {
                    clipboardHistoryList.SetSelected(clipboardHistoryList.SelectedIndex + 1, true);
                }
            }
        }

        private void GoUpList()
        {
            if (clipboardHistoryList.Items.Count > 1)
            {
                if (clipboardHistoryList.SelectedIndex == 0)
                {
                    clipboardHistoryList.SetSelected(clipboardHistoryList.Items.Count - 1, true);
                }
                else
                {
                    int currentlySelected = clipboardHistoryList.SelectedIndex;
                    clipboardHistoryList.SetSelected(currentlySelected - 1, true);
                }
            }
        }

        private void SelectLastEntry()
        {
            if (clipboardHistoryList.Items.Count > 0)
            {
                clipboardHistoryList.SetSelected(clipboardHistoryList.Items.Count - 1, true);
                toolStripStatusLabel.Text = "Selected last list entry.";
            }
            else
            {
                toolStripStatusLabel.Text = "Cannot select last entry of empty list.";
            }
        }

        private void SelectFirstEntry()
        {
            if (clipboardHistoryList.Items.Count > 0)
            {
                clipboardHistoryList.SetSelected(0, true);
                toolStripStatusLabel.Text = "Selected first list entry.";
            }
            else
            {
                toolStripStatusLabel.Text = "Cannot select first entry of empty list.";
            }
        }

        private void UpdateStatusEntryLabel()
        {
            int selectedIndex = clipboardHistoryList.SelectedIndex;
            if (selectedIndex >= 0)
            {
                toolStripEntryLabel.Text = "Entry : " + (selectedIndex + 1);
            }
            else
            {
                toolStripEntryLabel.Text = "没有选择任何信息";
            }
        }

        #region EventHandlers

        private void QuitMenuItem_Click(object sender, EventArgs e)
        {
            toolStripStatusLabel.Text = "清空剪贴板历史信息";
            ClearClipboardHistory();
            toolStripStatusLabel.Text = "退出 ...";
            AppShareClipIcon.Dispose();
            Environment.Exit(0);
        }

        private void CopyButton_Click(object sender, EventArgs e)
        {
            try
            {
                isCopying = true;
                Clipboard.SetText(clipboardHistoryList.SelectedItem.ToString());
                toolStripStatusLabel.Text = "Text copied to clipboard.";
                isCopying = false;
            }
            catch (ExternalException exception)
            {
                toolStripStatusLabel.Text = "Clipboard copy failed due to an external exception.";
                HandleExternalException(exception);
            }
            catch (NullReferenceException)
            {
                toolStripStatusLabel.Text = "Could not copy null value to clipboard.";
            }
        }

        private void GoUpButton_Click(object sender, EventArgs e)
        {
            GoUpList();
        }

        private void GoDownButton_Click(object sender, EventArgs e)
        {
            GoDownList();
        }

        private void RemoveSelectedEntry()
        {
            int selectedIndex = clipboardHistoryList.SelectedIndex;
            if (selectedIndex > -1)
            {
                clipboardHistoryList.Items.RemoveAt(selectedIndex);
                if (clipboardHistoryList.Items.Count == 0)
                {
                    deleteButton.Enabled = false;
                }
                else
                {
                    if (selectedIndex == 0)
                        clipboardHistoryList.SetSelected(0, true);
                    else
                        clipboardHistoryList.SetSelected(selectedIndex - 1, true);
                }
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!reallyQuit)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void ShowForm(object sender, EventArgs e)
        {
            this.Show();
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                    GoUpList();
                    break;

                case Keys.Down:
                    GoDownList();
                    break;

                case Keys.Delete:
                    ClearClipboardHistory();
                    break;

                case Keys.Back:
                    clearButton.PerformClick();
                    break;

                case Keys.Enter:
                    copyButton.PerformClick();
                    break;

                case Keys.End:
                    SelectLastEntry();
                    break;

                case Keys.Escape:
                    this.Visible = false;
                    break;

                case Keys.Home:
                    SelectFirstEntry();
                    break;
            }

            e.Handled = true;
        }

        private void ClipboardHistoryList_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateStatusEntryLabel();
        }
        #endregion

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            RemoveSelectedEntry();
        }

        private void ClearHistoryButton_Click(object sender, EventArgs e)
        {
            ClearClipboardHistory();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}
