using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
using LIST_LPSTR  = System.Collections.Generic.List<string>;

using static Pal.PalGlobal;
using static Pal.PalCommon;
using static Pal.PalSystem;
using System.Runtime.InteropServices;

namespace Pal
{
   public unsafe class PalPalette
   {
      public struct BGRA
      {
         public BYTE    blue;
         public BYTE    green;
         public BYTE    red;
         public BYTE    alpha;
      }

      public struct Palette
      {
         public List<BGRA>    day;     // 白天
         public List<BGRA>    night;   // 夜晚
      }

      public static RawColor4
      ColorToRaw(
         BYTE        R,
         BYTE        G,
         BYTE        B,
         BYTE        A = 0xFF
      )
      {
         return new RawColor4(R / 255.0f, G / 255.0f, B / 255.0f, A / 255.0f);
      }

      public static RawColor4
      ColorToRaw(
         Color       color
      )
      {
         return ColorToRaw(color.R, color.G, color.B, color.A);
      }

      public static RawColor4
      ColorToRaw(
         BGRA        rgb,
         BOOL        fIsAlpha = FALSE
      )
      {
         return ColorToRaw(rgb.red, rgb.green, rgb.blue, rgb.alpha);
      }


      public static void
      Init()
      /*++
       * 
       * 作用：
       *    初始化调色板
       * 
       * 参数：
       *    无
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         INT            i, j, nPalette, nColor, nColor_Time;
         DWORD*         lpPalette;
         BYTE*          lpPaletteData;
         List<BGRA>     bgra_list;

         g_Global.wPaletteID = 0;

         BYTE* lpPAT = g_Global.files.PAT_MKF;
         {
            lpPalette = (DWORD*)lpPAT;

            //
            // 获取 <调色板> 计数
            //
            nPalette = (INT)(lpPalette[0] / sizeof(DWORD) - 1);

            //
            // 初始化 <调色板列表>
            //
            g_Global.palette = new Palette[nPalette];

            //
            // 提取调色板
            //
            for (i = 0; i < nPalette; i++)
            {
               //
               // 将指针定位到 <PAT_SUB_i> 的开头
               //
               lpPaletteData = &lpPAT[lpPalette[i]];

               //
               // 初始化 day 调色板
               //
               g_Global.palette[i].day = new List<BGRA>();

               //
               // 获取当前调色板 <颜色> 计数
               //
               nColor = (INT)((lpPalette[i + 1] - lpPalette[i]) / (sizeof(BGRA)));

               //
               // 获取当前调色板白天 <颜色> 计数
               //
               nColor_Time = (nColor > PALETTE_COLOR_MAX) ? PALETTE_COLOR_MAX : nColor;

               //
               // 提取颜色值
               //
               bgra_list = g_Global.palette[i].day;
               for (j = 0; bgra_list.Count < nColor_Time; j++)
               {
                  bgra_list.Add(
                     new BGRA
                     {
                        blue  = (BYTE)(lpPaletteData[j * 3 + 2] << 2),
                        green = (BYTE)(lpPaletteData[j * 3 + 1] << 2),
                        red   = (BYTE)(lpPaletteData[j * 3 + 0] << 2),
                        alpha = (BYTE)((j == 0xFF) ? 0x00 : 0xFF),
                     }
                  );
               }

               //
               // 初始化 day 调色板
               //
               g_Global.palette[i].night = new List<BGRA>();

               //
               // 获取当前调色板夜晚 <颜色> 计数
               //
               nColor_Time = nColor - PALETTE_COLOR_MAX;
               nColor_Time = (nColor_Time > 0) ? nColor_Time : 0;

               //
               // 提取颜色值
               //
               bgra_list = g_Global.palette[i].day;
               for (j = 0; bgra_list.Count < nColor_Time; j++)
               {
                  bgra_list.Add(
                     new BGRA
                     {
                        blue = (BYTE)(lpPaletteData[j * 3 + 2] << 2),
                        green = (BYTE)(lpPaletteData[j * 3 + 1] << 2),
                        red = (BYTE)(lpPaletteData[j * 3 + 0] << 2),
                        alpha = (BYTE)((j == 0xFF) ? 0x00 : 0xFF),
                     }
                  );
               }
            }
         }
      }

      public static BGRA
      GetColor(
         WORD     wPaletteID,
         BOOL     fNightPalette,
         BYTE     bColorID
      )
      {
         fixed (Palette* thisPat = &g_Global.palette[wPaletteID])
         {
            return (fNightPalette) ? thisPat->night[bColorID] : thisPat->day[bColorID];
         }
      }

      public static BGRA
      GetColor(
         BYTE     bColorID
      )
      {
         return GetColor(g_Global.wPaletteID, g_Global.fNightPalette, bColorID);
      }

      public static void
      SetColor(
         BYTE     bColorID,
         BGRA     bgra
      )
      {
         if (!g_Global.fNightPalette)
            g_Global.palette[g_Global.wPaletteID].day[bColorID] = bgra;
         else
            g_Global.palette[g_Global.wPaletteID].night[bColorID] = bgra;
      }
   }
}
