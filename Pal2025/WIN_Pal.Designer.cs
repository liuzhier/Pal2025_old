namespace Pal
{
   partial class WIN_Main
   {
      /// <summary>
      /// 必需的设计器变量。
      /// </summary>
      private System.ComponentModel.IContainer components = null;

      /// <summary>
      /// 清理所有正在使用的资源。
      /// </summary>
      /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
      protected override void Dispose(bool disposing)
      {
         if (disposing && (components != null))
         {
            components.Dispose();
         }
         base.Dispose(disposing);
      }

      #region Windows 窗体设计器生成的代码

      /// <summary>
      /// 设计器支持所需的方法 - 不要修改
      /// 使用代码编辑器修改此方法的内容。
      /// </summary>
      private void InitializeComponent()
      {
         System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WIN_Main));
         SuspendLayout();
         // 
         // WIN_Main
         // 
         AutoScaleDimensions = new SizeF(9F, 20F);
         AutoScaleMode = AutoScaleMode.Font;
         BackColor = SystemColors.WindowText;
         ClientSize = new Size(1060, 739);
         Icon = (Icon)resources.GetObject("$this.Icon");
         Margin = new Padding(3, 4, 3, 4);
         Name = "WIN_Main";
         StartPosition = FormStartPosition.CenterScreen;
         Text = "仙剑奇侠传 Windows 10";
         Activated += WIN_Main_Activated;
         Deactivate += WIN_Main_Deactivate;
         FormClosed += WIN_Main_FormClosed;
         Load += WIN_Main_Load;
         ResumeLayout(false);
      }

      #endregion
   }
}

