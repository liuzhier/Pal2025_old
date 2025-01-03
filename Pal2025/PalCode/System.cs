using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

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

using static Pal.PalCommon;

namespace Pal
{
   public unsafe class PalSystem
   {
      public const BOOL TRUE           = true;     // <对象名称> 的固定字节数
      public const BOOL FALSE          = false;    // <对象名称> 的固定字节数
      public const INT  PAL_ERROR      = 0x7FFFFFFF;

      [StructLayout(LayoutKind.Sequential)]
      public struct RECT
      {
         public INT Left;
         public INT Top;
         public INT Right;
         public INT Bottom;
      }

      [DllImport("user32.dll")]
      public static extern BOOL AdjustWindowRectEx(ref RECT lpRect, uint dwStyle, bool bMenu, uint dwExStyle);

      [DllImport("user32.dll")]
      public static extern INT GetSystemMetrics(int nIndex);

      [DllImport("user32.dll")]
      public static extern SHORT GetAsyncKeyState(INT vKey);

      [DllImport("kernel32.dll")]
      public static extern INT QueryPerformanceFrequency(ref QWORD lpFrequency);

      [DllImport("kernel32.dll")]
      public static extern INT QueryPerformanceCounter(ref QWORD lpPerformanceCount);

      [DllImport("kernel32.dll")]
      public static extern void Sleep(DWORD dwMilliseconds);

      [DllImport($"{g_lpszGamePath}/{g_lpszPalDll}")]
      public static extern void unpak(INT* src, INT* dest);

      /*++
       * 
       * 作用：
       *    以下均为安全地释放指针
       * 
      --*/
      public static void
      S_FREE(
         void*    lpAddr
      )
      {
         if (lpAddr != null)
         {
            Marshal.FreeHGlobal((IntPtr)lpAddr);
            lpAddr = null;
         }
      }

      public static void
      S_FREE(
         IntPtr lpAddr
      )
      {
         S_FREE((void*)lpAddr);
      }

      public static void
      S_FREE(
         CHAR*    lpAddr
      )
      {
         S_FREE((void*)lpAddr);
      }

      public static void
      S_FREE(
         BYTE*    lpAddr
      )
      {
         S_FREE((void*)lpAddr);
      }

      public static void
      S_FREE(
         SHORT*      lpAddr
      )
      {
         S_FREE((void*)lpAddr);
      }

      public static void
      S_FREE(
         WORD*    lpAddr
      )
      {
         S_FREE((void*)lpAddr);
      }

      public static void
      S_FREE(
         INT*    lpAddr
      )
      {
         S_FREE((void*)lpAddr);
      }

      public static void
      S_FREE(
         DWORD*      lpAddr
      )
      {
         S_FREE((void*)lpAddr);
      }

      public static void
      S_FREE(
         LONG*    lpAddr
      )
      {
         S_FREE((void*)lpAddr);
      }

      public static void
      S_FREE(
         QWORD*      lpAddr
      )
      {
         S_FREE((void*)lpAddr);
      }

      public static BOOL
      _2B(
         object     value
      )
      {
         return (INT)value != 0;
      }

      public static BOOL
      _2B(
         CHAR     value
      )
      {
         return value != 0;
      }

      public static BOOL
      _2B(
         BYTE     value
      )
      {
         return value != 0;
      }

      public static BOOL
      _2B(
         SHORT    value
      )
      {
         return value != 0;
      }

      public static BOOL
      _2B(
         WORD     value
      )
      {
         return value != 0;
      }

      public static BOOL
      _2B(
         INT      value
      )
      {
         return value != 0;
      }

      public static BOOL
      _2B(
         DWORD    value
      )
      {
         return value != 0;
      }

      public static BOOL
      _2B(
         LONG     value
      )
      {
         return value != 0;
      }

      public static BOOL
      _2B(
         QWORD    value
      )
      {
         return value != 0;
      }
   }
}
