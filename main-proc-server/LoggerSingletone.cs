using System;
using System.Threading;
using System.Windows.Forms;

namespace main_proc_server
{
    class LoggerSingletone
    {
        private LoggerSingletone() { }

        static Mutex Mutex = null;
        static RichTextBox RichTextBox = null;

        static public void Begin(RichTextBox richTextBox)
        {
            if (RichTextBox != null || Mutex != null)
                throw new InvalidOperationException();

            RichTextBox = richTextBox;
            Mutex = new Mutex();
        }

        static public void End()
        {
            if (Mutex != null)
                Mutex.Close();

            Mutex = null;
            RichTextBox = null;
        }

        static public void Write(string input)
        {
            RichTextBox.Invoke((MethodInvoker)(() =>
            {
                RichTextBox.AppendText($"{DateTime.Now.ToLocalTime()} <=> {input}");
                RichTextBox.ScrollToCaret();
            }));
        }

        static public void WriteLine(string input)
        {
            RichTextBox.Invoke((MethodInvoker)(() =>
            {
                RichTextBox.AppendText($"\r\n{DateTime.Now.ToLocalTime()} <=> {input}");
                RichTextBox.ScrollToCaret();
            }));
        }

        static public void WriteMutexed(string input)
        {
            Mutex.WaitOne();
            Write(input);
            Mutex.ReleaseMutex();
        }

        static public void WriteLineMutexed(string input)
        {
            Mutex.WaitOne();
            WriteLine(input);
            Mutex.ReleaseMutex();
        }

        static public string ReadFully()
        {
            return RichTextBox.Text;
        }
    }
}
