using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using BOOL        = System.Boolean;
using CHAR        = System.Char;
using BYTE        = System.Byte;
using SHORT       = System.Int16;
using WORD        = System.UInt16;
using INT         = System.Int32;
using DWORD       = System.UInt32;
using LONG        = System.Int64;
using QWORD       = System.UInt64;
using LPSTR       = System.String;
using FILE        = System.IO.File;

using static Pal.PalSystem;
using static Pal.PalGlobal;
using static Pal.PalGame;
using static Pal.PalInput;

namespace Pal
{
   public class PalMain
   {
      public static BOOL fIsGameEnd = FALSE;

      public static void
      Pal_ShutDown()
      /*++
       * 
       * 作用：
       *    释放所有资源并结束游戏程序
       * 
       * 参数：
       *    无
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         PalMain.fIsGameEnd = TRUE;

         //
         // 释放所有资源
         //
         Free();

         //
         // 强制结束游戏
         //
         Environment.Exit(0);
      }

      private static void
      InitSubSystem()
      /*++
       * 
       * 作用：
       *    初始化游戏子系统
       * 
       * 参数：
       *    无
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         PalVideo.Init();
         PalInput.Init();
         PalSound.Init();
         PalWave.Init();
         PalMovie.Init();
      }

      private static void
      Init()
      /*++
       * 
       * 作用：
       *    初始化所有游戏资源
       * 
       * 参数：
       *    无
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         InitSubSystem();

         //
         // 初始化全局资源
         //
         PalGlobal.Init();
      }

      private static void
      FreeSubSystem()
      /*++
       * 
       * 作用：
       *    释放游戏子系统资源
       * 
       * 参数：
       *    无
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         PalVideo.Free();
         PalInput.Free();
         PalSound.Free();
         PalWave.Free();
         PalMovie.Free();
      }

      private static void
      Free()
      /*++
       * 
       * 作用：
       *    释放所有游戏资源
       * 
       * 参数：
       *    无
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         FreeSubSystem();

         //
         // 释放全局资源
         //
         PalGlobal.Free();
      }

      public static void
      Pal_Main()
      /*++
       * 
       * 作用：
       *    游戏的入口点
       * 
       * 参数：
       *    无
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         Init();

         //
         // 所有资源已初始化完毕，
         // 播放音乐 88 和开场动画
         //
         /*
         PalWave.Play(88);
         PalMovie.Play(1, "bik");
         while (TRUE)
         {
            PalInput.ClearKeyState();
            PalUtil.DelayTime(100);

            if (KeyPress(PalKey.Search) || !PalMovie.Status())
            {
               //
               // 用户按下任意键或视频播放完毕
               // 结束视频系统的播放任务
               //
               PalMovie.Stop();
               break;
            }
         }
         PalVideo.FadeOut(PalPalette.GetColor(0), FALSE, 1);
         */

         //
         // 进入游戏主循环
         //
         PalGame.MainLoop();
      }
   }
}
