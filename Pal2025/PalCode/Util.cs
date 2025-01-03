using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;

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

namespace Pal
{
   public unsafe class PalUtil
   {
      private static void
      Delay(
         DWORD    milliseconds
      )
      /*++
       * 
       * 作用：
       *    延迟一段时间
       * 
       * 参数：
       *    [IN]  milliseconds      欲延迟的毫秒数
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         if (PalTimer.ticks_started) PalTimer.TicksInit();

         PalSystem.Sleep(milliseconds);
         //Thread.Sleep((int)milliseconds);
      }

      public static void
      DelayUntil(
         QWORD    tick_target
      )
      /*++
       * 
       * 作用：
       *    延迟一段时间，期间接收输入
       * 
       * 参数：
       *    [IN]  tick_target    到哪个时间段停止延迟（Get_Tick + 帧延迟）
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         PalInput.ProcessEvent();

         while (!PalTimer.TicksPassed(PalTimer.GetTicks(), tick_target))
         {
            Delay(1);
            PalInput.ProcessEvent();
         }
      }

      public static void
      DelayTime(
         DWORD    milliseconds
      )
      /*++
       * 
       * 作用：
       *    延迟一段时间，期间接收输入
       * 
       * 参数：
       *    [IN]  tick_target    到哪个时间段停止延迟（Get_Tick + 帧延迟）
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         DelayUntil(PalTimer.GetTicks() + milliseconds);
      }

      public static DWORD
      STR2DWORD(
         LPSTR    lpszVal
      )
      /*++
       * 
       * 作用：
       *    字符串转 4 字节无符号整数
       * 
       * 参数：
       *    [IN]  lpszVal     欲转换的字符串型数值
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         if (lpszVal == null)
         {
            return 0u;
         }

         return uint.Parse(lpszVal, CultureInfo.CurrentCulture);
      }

      public static WORD
      STR2UNCODE(
         LPSTR lpszWord
      )
      /*++
       * 
       * 作用：
       *    字符串转 Uncode 码
       * 
       * 参数：
       *    [IN]  lpszVal     欲转换的字符串
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         if (lpszWord == null) return 0;

         return BitConverter.ToUInt16(Encoding.Unicode.GetBytes(lpszWord), 0);
      }
   }
}
