namespace main_proc_server
{
    partial class MainForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.rtbLogWindow = new System.Windows.Forms.RichTextBox();
            this.bAddClient = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // rtbLogWindow
            // 
            this.rtbLogWindow.Location = new System.Drawing.Point(12, 12);
            this.rtbLogWindow.Name = "rtbLogWindow";
            this.rtbLogWindow.ReadOnly = true;
            this.rtbLogWindow.Size = new System.Drawing.Size(288, 209);
            this.rtbLogWindow.TabIndex = 0;
            this.rtbLogWindow.Text = "";
            // 
            // bAddClient
            // 
            this.bAddClient.Location = new System.Drawing.Point(12, 227);
            this.bAddClient.Name = "bAddClient";
            this.bAddClient.Size = new System.Drawing.Size(288, 23);
            this.bAddClient.TabIndex = 1;
            this.bAddClient.Text = "&Add Client";
            this.bAddClient.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(312, 257);
            this.Controls.Add(this.bAddClient);
            this.Controls.Add(this.rtbLogWindow);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "GUI Pipe Server";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtbLogWindow;
        private System.Windows.Forms.Button bAddClient;
    }
}

