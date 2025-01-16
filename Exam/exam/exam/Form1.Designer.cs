namespace exam
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblLevel;
        private System.Windows.Forms.Label lblAttempts;
        private System.Windows.Forms.Label lblTimer;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.lblLevel = new System.Windows.Forms.Label();
            this.lblAttempts = new System.Windows.Forms.Label();
            this.lblTimer = new System.Windows.Forms.Label();

            this.SuspendLayout();

            // Уровень
            this.lblLevel.AutoSize = true;
            this.lblLevel.Location = new System.Drawing.Point(10, 10);
            this.lblLevel.Text = "Уровень: 1";

            // Попытки
            this.lblAttempts.AutoSize = true;
            this.lblAttempts.Location = new System.Drawing.Point(150, 10);
            this.lblAttempts.Text = "Попытки: 3";

            // Таймер
            this.lblTimer.AutoSize = true;
            this.lblTimer.Location = new System.Drawing.Point(300, 10);
            this.lblTimer.Text = "Время: 30";

            // Настройки формы
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 400);
            this.Controls.Add(this.lblLevel);
            this.Controls.Add(this.lblAttempts);
            this.Controls.Add(this.lblTimer);
            this.Text = "Тренажёр памяти";

            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
