using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;

using Pal;

using static Pal.PalSystem;
using static Pal.PalMain;

namespace Pal
{
   public partial class WIN_Main : Form
   {

      public WIN_Main()
      {
         InitializeComponent();
      }

      private void WIN_Main_Load(object sender, EventArgs e)
      {
         //
         // 初始化窗口控件
         //
         Window_InitVideo();

         //
         // 动态调整窗口大小
         //
         Window_AutoSetWindowsSize();

         //
         // 进入游戏主循环
         //
         Pal_Main();
      }

      private void WIN_Main_FormClosed(object sender, FormClosedEventArgs e)
      {
         Pal_ShutDown();
      }

      public const int SM_CXSCREEN = 0;
      public const int SM_CYSCREEN = 1;

      private void Window_AutoSetWindowsSize()
      {
         int w, h;
         RECT rect;
         Graphics graphics;

         //
         // 获取正确的窗口矩形范围
         //
         rect = new RECT();
         rect.Left = 0;
         rect.Top = 0;
         rect.Right = (int)this.Width;
         rect.Bottom = (int)this.Height;
         PalSystem.AdjustWindowRectEx(ref rect, 0x6CF0000, false, 0);

         //
         // 计算正确的窗口大小
         //
         w = rect.Right - rect.Left;
         h = rect.Bottom - rect.Top;

         //
         // 获取表征资源
         //
         graphics = Graphics.FromHwnd(IntPtr.Zero);

         //
         // 获取成功则设置实际尺寸
         //
         if (graphics != null)
         {
            this.Width = (int)(PalVideo.WIN_W / Math.Round(graphics.DpiX / 96.0));
            this.Height = (int)(PalVideo.WIN_H / Math.Round(graphics.DpiY / 96.0));
         }

         //
         // 重新设置坐标
         //
#if DEBUG
         this.Left = Screen.PrimaryScreen.Bounds.Width - this.Width;
#else    // DEBUG
         this.Left   = (Screen.PrimaryScreen.Bounds.Width - this.Width) / 2;
#endif   // DEBUG
         this.Top = (Screen.PrimaryScreen.Bounds.Height - this.Height) / 2;

         //
         // 显示窗口
         //
         this.Show();
      }

      private void Window_InitVideo()
      {
         PalVideo.WIN_Pal = this;

         //
         // 禁用窗体 IME，以免跳出输入法
         //
         this.ImeMode = ImeMode.Off;
      }

      private void WIN_Main_Activated(object sender, EventArgs e)
      {
         //
         // 允许 DirectInput 进行输入事件
         //
         PalInput.PAL_KEY_IN = TRUE;
      }

      private void WIN_Main_Deactivate(object sender, EventArgs e)
      {
         //
         // 禁止 DirectInput 进行输入事件
         //
         PalInput.PAL_KEY_IN = FALSE;
      }

      protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
      {
         // 检查是否是Alt键
         if ((keyData & Keys.Alt) == Keys.Alt)
         {
            return true;
         }

         return base.ProcessCmdKey(ref msg, keyData);
      }
   }
}
