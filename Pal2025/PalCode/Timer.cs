/*
   This section is ported from the C SDL library
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

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

namespace Pal
{
   public class PalTimer
   {
      public   static   BOOL  ticks_started     = FALSE;
      private  static   QWORD ticks_per_second  = 0;
      private  static   QWORD start_ticks       = 0;

      public static BOOL
      TicksPassed(QWORD tick_now, QWORD tick_target)
      {
         return (tick_now >= tick_target);
      }

      public static void
      TicksInit()
      {
         if (ticks_started) return;

         ticks_started = TRUE;

         QueryPerformanceFrequency(ref ticks_per_second);
         QueryPerformanceCounter(ref start_ticks);
      }

      public static QWORD
      GetTicks64()
      {
         QWORD now      = 0;

         if (!ticks_started) TicksInit();

         QueryPerformanceCounter(ref now);

         return ((now - start_ticks) * 1000) / ticks_per_second;
      }

      /* This is a legacy support function; GetTicks() returns a Uint32,
         which wraps back to zero every ~49 days. The newer GetTicks64()
         doesn't have this problem, so we just wrap that function and clamp to
         the low 32-bits for binary compatibility. */
      public static DWORD
      GetTicks()
      {
         return (DWORD)(GetTicks64() & 0xFFFFFFFF);
      }
   }
}
