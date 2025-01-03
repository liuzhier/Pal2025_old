using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;
using SharpDX.Direct2D1;
using SharpDX;
using SharpDX.DXGI;

using BOOL        = System.Boolean;
using CHAR        = System.Char;
using BYTE        = System.Byte;
using SHORT       = System.Int16;
using WORD        = System.UInt16;
using INT         = System.Int32;
using DWORD       = System.UInt32;
using LONG        = System.Int64;
using QWORD       = System.UInt64;
using FLOAT       = System.Single;
using LPSTR       = System.String;
using FILE        = System.IO.File;

using DBitmap        = SharpDX.Direct2D1.Bitmap;
using DPixelFormat   = SharpDX.Direct2D1.PixelFormat;
using DAlphaMode     = SharpDX.Direct2D1.AlphaMode;
using GIFormat       = SharpDX.DXGI.Format;


using static Pal.PalCommon;
using static Pal.PalGlobal;
using static Pal.PalSystem;
using static Pal.PalVideo;
using static Pal.PalText;
using static Pal.PalPalette;
using static Pal.PalInput;

namespace Pal
{
   public unsafe class PalGame
   {
      public const WORD FPS         = 10;
      public const WORD FRAME_TIME  = (1000 / FPS);

      public static void
      MainLoop()
      /*++
       * 
       * 作用：
       *    游戏主逻辑的入口点
       * 
       * 参数：
       *    无
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         DWORD       dwTime;

         //
         // 进入主菜单标题画面
         //
         PalUi.TitleMenu();

         //
         // 运行主游戏循环
         //
         dwTime = PalTimer.GetTicks();
         while (TRUE)
         {
            //
            // 清除上一帧的输入状态。
            //
            PalInput.ClearKeyState();

            //
            // 等待一帧的时间。这里接受输入。
            //
            PalUtil.DelayUntil(dwTime);

            //
            // 设置下一帧的时间。
            //
            dwTime = PalTimer.GetTicks() + FRAME_TIME;

            //
            // Run the main frame routine.
            //
            Play.StartFrame();
         }
      }
   }
}
