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
        int copyCnt = 0;
        int copyErrorCnt = 0;
        DateTime startTime = DateTime.Now;


        public string GetTimerNo()
        {
            string timerNo = File.ReadAllText(this.workPath + "\\Controller\\timerNo.conf");
            return timerNo;
        }

        public MainForm()
        {
            InitializeComponent();
            nextClipboardViewer = (IntPtr)SetClipboardViewer((int)this.Handle);
            if (this.clipboardHistoryList.Items.Count > 0)
                this.clipboardHistoryList.SetSelected(0, true);
            clipboardHistoryList.Select();

            // this.TopMost = true; // 总在最前

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
        /// <summary>
        /// Takes care of the external DLL calls to user32 to receive notification when
        /// the clipboard is modified. Passes along notifications to any other process that
        /// is subscribed to the event notification chain.
        /// </summary>
        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            const int WM_DRAWCLIPBOARD = 0x308;
            const int WM_CHANGECBCHAIN = 0x030D;
            // const int WM_CLIPBOARDUPDATE = 0x031D;
            const int WM_COPY = 0x301;
            // const int WM_PASTE = 0x302;

            switch (m.Msg)
            {
                case WM_COPY:
                    StringBuilder buf0 = new StringBuilder(256);
                    if (GetWindowText(m.HWnd, buf0, 256) > 0)
                    {
                        Console.WriteLine(buf0.ToString());
                    }
                    break;
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
            Environment.SetEnvironmentVariable("CLIPBOARD_IP", ip);
            //Console.WriteLine("myIP:" + ip);
            string appName = Environment.GetEnvironmentVariable("APPSIMULATOR_APP_NAME");
            string redisServer = Environment.GetEnvironmentVariable("REDIS_SERVER_IP");
            label1.Text = " form:" + ip + " >> to:" + redisServer;
            return ip;
        }

        /// <summary>
        /// Adds a clipboard history to the clipboard history list.
        /// </summary>
        private void AddClipBoardEntry(Int64 timeStamp, string timerNo)
        {
            Console.WriteLine("<<timerNo>> " + timerNo);
            Console.WriteLine("<<timeStamp>> " + timeStamp.ToString());
            if (Clipboard.ContainsText())
            {
                string clipboardText = Clipboard.GetText();
                if (!String.IsNullOrEmpty(clipboardText))
                {
                    if (clipboardText.Contains("http"))
                    {
                        ///* syq
                        clipboardText = clipboardText.Substring(clipboardText.LastIndexOf("http"));
                        try
                        {
                            string appName = Environment.GetEnvironmentVariable("APPSIMULATOR_APP_NAME");
                            //Console.WriteLine("<<appName>> " + appName);
                            string redisServerIP = Environment.GetEnvironmentVariable("REDIS_SERVER_IP");
                            RedisClient Client = new RedisClient(redisServerIP, 6379);
                            Client.ChangeDb(11);
                            Client.AddItemToSet("devices:" + this.localIP + ":" + timerNo, clipboardText);
                            Client.AddItemToSet("devices:" + this.localIP + "_org:" + timerNo, clipboardText);
                            // Client.AddItemToList("devices:" + appName + ":" + this.myIP + "_org", timeStamp.ToString());
                            clipboardHistoryList.Items.Insert(0, clipboardText);
                            this.copyCnt++;
                        }
                        catch (Exception e)
                        {
                            this.copyErrorCnt++;
                            Console.WriteLine(e);
                        }

                        toolStripStatusLabel.Text = "copy success: " + this.copyCnt.ToString() + " failure: " + this.copyErrorCnt.ToString();
                        toolStripStatusLabel.Text += " times: " + (DateTime.Now - startTime).ToString().Substring(0,8);
                        deleteButton.Enabled = true;
                    }
                }
            }
            else
            {
                toolStripStatusLabel.Text = "History entry was not added because it was null or empty";
            }
        }


        /// <summary>
        /// Clears the clipboard history list.
        /// </summary>
        /// <remarks>
        /// The current clipboard content is preserved.
        /// </remarks>
        private void ClearClipboardHistory()
        {
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to clear the clipboard history",
                                                        "Clear clipboard history?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dialogResult == DialogResult.Yes)
            {
                clipboardHistoryList.ClearSelected();
                clipboardHistoryList.Items.Clear();
                toolStripStatusLabel.Text = "Clipboard history was cleared.";
            }
        }

        /// <summary>
        /// Displays any ExternalException in an error box.
        /// </summary>
        /// <param name="e"></param>
        public void HandleExternalException(ExternalException e)
        {
            MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// In the clipboard history list, selects the entry that is below the one that is currently selected.
        /// </summary>
        /// <remarks>
        /// The list 'loops' meaning thet if the entry that is currently selected is the last one, the first one
        /// will be selected.
        /// </remarks>
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

        /// <summary>
        /// In the clipboard history list, selects the entry that is above the one that is currently selected.
        /// </summary>
        /// <remarks>
        /// The list 'loops' meaning thet if the entry that is currently selected is the first one, the last one
        /// will be selected.
        /// </remarks>
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

        /// <summary>
        /// Selects the last entry of the clipboard history list. Generally useful when the 'End' key is pressed
        /// </summary>
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

        /// <summary>
        /// Selects the first entry of the clipboard history list. Generally useful when the 'Home' key is pressed
        /// </summary>
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

        /// <summary>
        /// Update the status bar entry label to show which entry is selected in the clipboard history list
        /// </summary>
        private void UpdateStatusEntryLabel()
        {
            int selectedIndex = clipboardHistoryList.SelectedIndex;
            if (selectedIndex >= 0)
            {
                toolStripEntryLabel.Text = "Entry : " + (selectedIndex + 1);
            }
            else
            {
                toolStripEntryLabel.Text = "No entry selected";
            }
        }

        #region EventHandlers

        private void QuitMenuItem_Click(object sender, EventArgs e)
        {
            toolStripStatusLabel.Text = "Clearing clipboard history...";
            ClearClipboardHistory();
            toolStripStatusLabel.Text = "Exiting...";
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

        /// <summary>
        /// If the user clicks the "X" in the window frame, 'reallyQuit' is going to be false;
        /// hence, AppShareClip will be sent to tray.
        /// </summary>
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
