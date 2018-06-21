using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace main_proc_server
{
    public partial class MainForm : Form
    {
        private string UnicodeBytesToString(byte[] Bytes, int BytesCount)
        {
            return Encoding.Unicode.GetString(Bytes, 0, BytesCount);
        }

        private string UnicodeBytesToString(byte[] Bytes)
        {
            return UnicodeBytesToString(Bytes, Bytes.Length);
        }

        public MainForm()
        {
            InitializeComponent();

            FormClosing += MainForm_FormClosing;
            bAddClient.Click += BAddClient_Click;
            rtbLogWindow.HandleCreated += BeginMainTask;
        }

        private const string confName = ".config";
        private int confValue = 5;

        private void StartApplication(string appName, int appCount = 1)
        {
            try
            {
                while (appCount-- > 0)
                    System.Diagnostics.Process.Start(appName);
            }
            catch (Exception)
            {
                MessageBox.Show(
                            $"Failed to open application '{appName}'.",
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error,
                            MessageBoxDefaultButton.Button1
                            );
            }
        }

        private void BeginMainTask(object sender, EventArgs e)
        {
            LoggerSingletone.Begin(rtbLogWindow);

            /* CONFIG BEGIN */
            if (File.Exists(confName))
                using (var fileStream = File.OpenRead(confName))
                using (var streamReader = new StreamReader(fileStream))
                {
                    var fileContents = streamReader.ReadToEnd();
                    if (Regex.IsMatch(fileContents, "[1-9]\\d*"))
                        confValue = int.Parse(fileContents);
                    else
                        MessageBox.Show(
                            $"Wrong format of config file '{confName}'." +
                            $"\nDefault value will be used: {confValue}.", 
                            "Error", 
                            MessageBoxButtons.OK, 
                            MessageBoxIcon.Error, 
                            MessageBoxDefaultButton.Button1
                            );

                    streamReader.Close();
                    fileStream.Close();
                }
            else
                using (var fileStream = File.Create(confName))
                using (var streamWriter = new StreamWriter(fileStream))
                {
                    streamWriter.Write(confValue);

                    streamWriter.Close();
                    fileStream.Close();
                }
            /* CONFIG END */

            // Add number of clients
            StartApplication("main-proc-client.exe", confValue);

            var ThreadMainBody = new Thread(() =>
            {
                var NPipeSemaphoreMax = 3;
                var NPipeSemaphore = new Semaphore(NPipeSemaphoreMax, NPipeSemaphoreMax);

                var NpName = "foo";
                var NpDir = PipeDirection.In;
                var NpMax = NamedPipeServerStream.MaxAllowedServerInstances;
                var NpTransmission = PipeTransmissionMode.Message;
                var NpOpt = PipeOptions.Asynchronous;

                for (; ; )
                {
                    LoggerSingletone.WriteLineMutexed("Waiting for semaphore to signal;");
                    NPipeSemaphore.WaitOne();

                    LoggerSingletone.WriteLineMutexed("Creating named pipe;");
                    var NamedPipe = new NamedPipeServerStream(NpName, NpDir, NpMax, NpTransmission, NpOpt);

                    LoggerSingletone.WriteLineMutexed("Waiting for client to connect;");
                    NamedPipe.WaitForConnection();

                    var WorkerThread = new Thread(() =>
                    {
                        var BufCount = 4096;
                        var Buf = new byte[BufCount];

                        var ThreadID = Thread.CurrentThread.ManagedThreadId;
                        LoggerSingletone.WriteLineMutexed($"Created new worker thread #{ThreadID};");

                        for (int BytesCount; NamedPipe.IsConnected;)
                        {
                            BytesCount = NamedPipe.Read(Buf, 0, BufCount);
                            if (BytesCount > 0)
                            {
                                var ClMessage = UnicodeBytesToString(Buf, BytesCount);
                                LoggerSingletone.WriteLineMutexed($"Client in thread #{ThreadID}: {ClMessage};");
                            }
                        }

                        NamedPipe.Disconnect();
                        NamedPipe.Close();
                        NPipeSemaphore.Release();

                        LoggerSingletone.WriteLineMutexed($"Worker thread #{ThreadID} terminated;");
                    });
                    WorkerThread.IsBackground = true;
                    WorkerThread.Start();
                }
            });
            ThreadMainBody.IsBackground = true;
            ThreadMainBody.Start();
        }

        private async void BAddClient_Click(object sender, EventArgs e)
        {
            await Task.Run(() => StartApplication("main-proc-client.exe"));
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            bool Reason = e.CloseReason == CloseReason.UserClosing;
            bool Length = rtbLogWindow.Text.Length > 0;

            if (Reason && Length)
            {
                var result = MessageBox.Show(
                    "Do you want to save application .log file?",
                    "Question",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1
                    );

                // Save log to file if user agreed
                if (result == DialogResult.Yes)
                {
                    using (var sfd = new SaveFileDialog())
                    {
                        sfd.RestoreDirectory = true;
                        sfd.FileName = $"{DateTime.Now.Ticks}.log";
                        sfd.DefaultExt = "log";
                        sfd.Filter = "Log files (*.log)|*.log";

                        if (sfd.ShowDialog() == DialogResult.OK)
                            using (var sw = new StreamWriter(sfd.OpenFile()))
                                sw.WriteLine(rtbLogWindow.Text);
                    }
                }
            }
        }
    }
}
