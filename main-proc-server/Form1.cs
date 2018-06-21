using System;
using System.IO.Pipes;
using System.Threading;
using System.Windows.Forms;
using System.Text;
using System.IO;

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

        private Mutex TextOutputMutex;

        private Semaphore NamedPipeHandlerSemaphore;
        private const int NamedPipeHandlerSemaphoreMax = 3;

        public MainForm()
        {
            InitializeComponent();
            LoggerSingletone.Begin(rtbLogWindow);

            FormClosing += MainForm_FormClosing;

            /* CONFIG BEGIN */



            /* CONFIG END */

            TextOutputMutex = new Mutex();
            NamedPipeHandlerSemaphore = new Semaphore(
                NamedPipeHandlerSemaphoreMax, 
                NamedPipeHandlerSemaphoreMax
                );

            var ThreadMainBody = new Thread(() =>
            {
                var NpName = "foo";
                var NpDir = PipeDirection.In;
                var NpMax = NamedPipeServerStream.MaxAllowedServerInstances;
                var NpTransmission = PipeTransmissionMode.Message;
                var NpOpt = PipeOptions.Asynchronous;

                for (; ; )
                {
                    LoggerSingletone.WriteLineMutexed("Waiting for semaphore to signal;");
                    NamedPipeHandlerSemaphore.WaitOne();

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
                                var ClMessage = UnicodeBytesToString(Buf, BufCount);
                                LoggerSingletone.WriteLineMutexed($"Client in thread #{ThreadID}: {ClMessage};");
                            }
                        }

                        NamedPipe.Disconnect();
                        NamedPipe.Close();
                        NamedPipeHandlerSemaphore.Release();

                        LoggerSingletone.WriteLineMutexed($"Worker thread #{ThreadID} terminated;");
                    });
                    WorkerThread.IsBackground = true;
                    WorkerThread.Start();
                }
            });
            ThreadMainBody.IsBackground = true;
            ThreadMainBody.Start();
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
