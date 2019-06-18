namespace TestDyn
{
    partial class Form1
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
            this.txtJSON = new System.Windows.Forms.TextBox();
            this.cmdTest = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtJSON
            // 
            this.txtJSON.Location = new System.Drawing.Point(12, 12);
            this.txtJSON.Multiline = true;
            this.txtJSON.Name = "txtJSON";
            this.txtJSON.Size = new System.Drawing.Size(776, 195);
            this.txtJSON.TabIndex = 0;
            // 
            // cmdTest
            // 
            this.cmdTest.Location = new System.Drawing.Point(429, 229);
            this.cmdTest.Name = "cmdTest";
            this.cmdTest.Size = new System.Drawing.Size(345, 58);
            this.cmdTest.TabIndex = 1;
            this.cmdTest.Text = "Test";
            this.cmdTest.UseVisualStyleBackColor = true;
            this.cmdTest.Click += new System.EventHandler(this.CmdTest_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.cmdTest);
            this.Controls.Add(this.txtJSON);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtJSON;
        private System.Windows.Forms.Button cmdTest;
    }
}

