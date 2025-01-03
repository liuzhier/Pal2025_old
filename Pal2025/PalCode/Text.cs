using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;
using SharpDX.DirectWrite;
using System.Security.Cryptography;
using System.Windows.Forms;
using SharpDX.Direct2D1;
using SharpDX.DirectInput;

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

using PAL_POS     = System.UInt32;

using DWrite      = SharpDX.DirectWrite;

using DKey        = SharpDX.DirectInput.Key;

using static Pal.PalCommon;
using static Pal.PalVideo;
using static Pal.PalSystem;
using static Pal.PalUtil;
using static Pal.PalGlobal;
using static Pal.PalInput;
using static Pal.PalPalette;
using static Pal.PalCodePage;
using static Pal.PalUi;

namespace Pal
{
   public unsafe class PalText
   {
      public enum DialogPosition
      {
         Upper         = 0,
         Center,
         Lower,
         CenterWindow,
         Menu
      }

      public   const    WORD           FONT_SIZE      = 16;
      public   static   BYTE           FONT_ICON_ID   = 0;
      private  static   Pal_Rect       FONT_RECT      = new Pal_Rect { x = 0, y = 0, w = 0, h = 0 };
      public   static   DWORD          DELAY_DISPLAY  = 3, DELAY_CLEAR = 0;
      public   static   BOOL           IS_SKIP        = FALSE;
      public   static   DialogPosition DIALOG_POS     = DialogPosition.Upper;

      public static void
      InitFontIcon()
      /*++
       * 
       * 作用：
       *    初始化文本末尾的光标
       * 
       * 参数：
       *    无
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         PalUnpak.MKFReadChunk(out g_Global.FONT_ICON, 12, g_Global.files.DATA_MKF);
      }

      public static void
      FreeFontIcon()
      /*++
       * 
       * 作用：
       *    释放文本末尾的光标占用的资源
       * 
       * 参数：
       *    无
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         if (g_Global.FONT_ICON != null)
         {
            Marshal.FreeHGlobal((IntPtr)g_Global.FONT_ICON);
            g_Global.FONT_ICON = null;
         }
      }

      public static TextMetrics
      GetTextMetrics(
         LPSTR    lpszWord
      )
      /*++
       * 
       * 作用：
       *    获取单字实际的 RECT
       * 
       * 参数：
       *    [IN]  lpszText    待检查的文本
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         return new DWrite.TextLayout(DX_Write_Factory, lpszWord, DX_Font, 0, 0).Metrics;
      }

      public static FLOAT
      GetTextWidth(
         LPSTR    lpszText
      )
      /*++
       * 
       * 作用：
       *    获取文本实际的宽度
       * 
       * 参数：
       *    [IN]  lpszText    待检查的文本
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         FLOAT    flWidth = 0;

         foreach (CHAR lpszWord in lpszText)
         {
            flWidth += GetTextMetrics(lpszWord.ToString()).Width;
         }

         return flWidth;
      }

      public static FLOAT
      GetWordWidth(
         WORD     wObjectID
      )
      /*++
       * 
       * 作用：
       *    获取对象名称实际的宽度
       * 
       * 参数：
       *    [IN]  lpszText    待检查的文本
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         return PalText.GetTextWidth(g_Global.obgect_name[wObjectID]);
      }

      public static FLOAT
      GetTextHeight(
         LPSTR    lpszText
      )
      /*++
       * 
       * 作用：
       *    获取文本实际的宽度
       * 
       * 参数：
       *    [IN]  lpszText    待检查的文本
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         FLOAT flHeight = 0;

         foreach (CHAR lpszWord in lpszText)
         {
            flHeight += GetTextMetrics(lpszWord.ToString()).Height;
         }

         return flHeight;
      }

      private static void
      DrawCharToSurface(
         LPSTR       lpszWord,
         BYTE        bColorID
      )
      /*++
       * 
       * 作用：
       *    绘制单个文字，允许文字阴影
       * 
       * 参数：
       *    [IN]  lpszWord    待输出文本
       *    [IN]  bColorID    文本颜色
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         INT      row, col;
         INT      pixel_offset_x = 0, pixel_offset_y = 0;
         INT      pixel_id;
         WORD     row_mask, col_mask, word_uncode = PalUtil.STR2UNCODE(lpszWord);

         for (row = 0; row < FONT_RECT.h; row++)
         {
            //
            // 截断超出屏幕部分
            //
            pixel_offset_y = FONT_RECT.y + row;

            //
            // 跳过超出屏幕上端的部分
            //
            if (pixel_offset_y < 0) continue;

            //
            // 遇到超出屏幕下端的部分，直接结束绘制
            //
            if (pixel_offset_y >= DX_Screen.h) break;

            //
            // 获取文字一行像素的掩码
            //
            row_mask = PalCodePage.SimSun[word_uncode, row];

            if (row_mask == 0)
            {
               //
               // 此行无像素，跳过
               //
               continue;
            }

            for (col = 0; col < FONT_RECT.w; col++)
            {
               //
               // 阻止超出屏幕部分从另一端冒出
               //
               pixel_offset_x = col + FONT_RECT.x;

               //
               // 跳过超出屏幕左端的部分
               //
               if (pixel_offset_x < 0) continue;

               //
               // 遇到超出屏幕右端的部分，直接跳过该行
               //
               if (pixel_offset_x >= DX_Screen.w) break;

               //
               // 获取文字单个像素的掩码
               //
               col_mask = (WORD)(row_mask & (1 << col));

               if (col_mask == 0)
               {
                  //
                  // 此为透明像素，跳过
                  //
                  continue;
               }

               pixel_id = pixel_offset_y * DX_Screen.w + pixel_offset_x - 1;

               //
               // 避免把颜色写到 buffer 以外的地方
               //
               if ((pixel_id < 0) || (pixel_id > (DX_Screen.w * DX_Screen.h)))
                  continue;

               ((BYTE*)DX_Screen.pixels)[pixel_id] = bColorID;
            }
         }
      }

      private static void
      DrawChar(
         LPSTR       lpszWord,
         BYTE        bColorID,
         BOOL        fShadow
      )
      /*++
       * 
       * 作用：
       *    绘制单个文字，允许文字阴影
       * 
       * 参数：
       *    [IN]  lpszText    待输出文本
       *    [IN]  bColorID    文本颜色
       *    [IN]  fShadow     是否输出字体阴影
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         INT      x, y;

         x = FONT_RECT.x;
         y = FONT_RECT.y;

         if (lpszWord == "") return;

         if (fShadow)
         {
            //
            // 绘制黑色的文本阴影
            //
            FONT_RECT.x += 1;
            FONT_RECT.y += 1;
            DrawCharToSurface(lpszWord, 0x00);

            //FONT_RECT.y -= 1;
            //DrawCharToSurface(lpszWord, 0x00);

            //FONT_RECT.x -= 1;
            //FONT_RECT.y += 1;
            //DrawCharToSurface(lpszWord, 0x00);
         }

         //
         // 回退坐标
         //
         FONT_RECT.x = x;
         FONT_RECT.y = y;

         if (bColorID != MENUITEM_COLOR_ALPHA)
         {
            //
            // 绘制单字
            //
            DrawCharToSurface(lpszWord, bColorID);
         }
      }

      public static void
      DrawText(
         LPSTR       lpszText,
         PAL_POS     pos,
         BYTE        bColorID    = 0x0F,
         BOOL        fShadow     = TRUE,
         BOOL        fRegex      = TRUE,
         BOOL        fIsDialog   = FALSE,
         BOOL        fUpdate     = FALSE
      )
      /*++
       * 
       * 作用：
       *    显示文本，允许特殊字符进行输出控制
       * 
       * 参数：
       *    [IN]  lpszText       待输出文本
       *    [IN]  pos            输出到的屏幕坐标
       *    [IN]  bColorID       文本颜色（缺省：白色）
       *    [IN]  fShadow        是否输出字体阴影（缺省：开启）
       *    [IN]  fRegex         是否允许特殊字符进行输出控制（缺省：开启，匹配@-"()~$）
       *    [IN]  fIsDialog      是否为对话框（缺省：关闭）
       *    [IN]  fUpdate        是否立即绘制文本（缺省：开启）
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         INT            i;
         LPSTR          lpszWord;
         TextMetrics    sngMetrics;
         LPSTR          lpszLastRegex  = "";
         BYTE           bActualColorID = bColorID;

         if (lpszText == "") return;

         //
         // 设置初始 PosX
         //
         FONT_RECT.x = (SHORT)PAL_X(pos);

         for (i = 0; i < lpszText.Length; i++)
         {
            //
            // 截取单字
            //
            lpszWord          = lpszText[i].ToString();

            if (fRegex)
            {
               //
               // 输出控制检查
               //
               switch (lpszWord)
               {
                  //
                  // 控制符：醒目颜色
                  //
                  case "\'":
                     if (lpszLastRegex != "'")
                     {
                        lpszLastRegex = lpszWord;
                        bActualColorID = 0x1A;
                     }
                     else lpszLastRegex = "";
                     continue;

                  case "-":
                     if (lpszLastRegex != "-")
                     {
                        lpszLastRegex = lpszWord;
                        bActualColorID = 0x8D;
                     }
                     else lpszLastRegex = "";
                     continue;

                  case "\"":
                     if (lpszLastRegex != "\"")
                     {
                        lpszLastRegex = lpszWord;
                        bActualColorID = 0x2D;
                     }
                     else lpszLastRegex = "";
                     continue;

                  //
                  // 控制符：表情符号 汗滴，爱心
                  //
                  case "(":
                     if (i >= lpszText.Length - 1)
                     {
                        FONT_ICON_ID = 1;
                        continue;
                     }
                     break;

                  case ")":
                     if (i >= lpszText.Length - 1)
                     {
                        FONT_ICON_ID = 2;
                        continue;
                     }
                     break;

                  //
                  // 控制符：输出速度
                  //
                  case "$":
                     i++;
                     DELAY_DISPLAY = PalUtil.STR2DWORD(lpszText.Substring(i, 2)) * 10 / 7;
                     i++;
                     continue;

                  //
                  // 控制符：倒计时清理对话
                  //
                  case "~":
                     i++;
                     DELAY_CLEAR = PalUtil.STR2DWORD(lpszText.Substring(i, 2));
                     PalUtil.DelayTime(DELAY_CLEAR * 80 / 7);
                     i++;
                     continue;
               }

               //
               // 还原颜色
               //
               if (lpszLastRegex == "") bActualColorID = bColorID;
            }

            //
            // 获取单字实际尺寸
            //
            sngMetrics  = GetTextMetrics(lpszWord);

            //
            // 设置输出坐标
            //
            FONT_RECT.y = (SHORT)PAL_Y(pos);
            FONT_RECT.w = FONT_SIZE;
            FONT_RECT.h = (INT)Math.Min(sngMetrics.Height, FONT_SIZE);

            //
            // 绘制单字
            //
            DrawChar(lpszWord, bActualColorID, fShadow);

            //
            // 非直接跳过时，进行文字延迟输出
            //
            if (!fIsDialog && !IS_SKIP && fUpdate)
            {
               PalInput.ClearKeyState();
               PalUtil.DelayTime(DELAY_DISPLAY * 8);

               if (KeyPress(PalKey.Search)
                  || KeyPress(PalKey.Menu))
               {
                  //
                  // 用户按下对话跳过键，跳过逐字延迟，直接显示完整对话
                  //
                  IS_SKIP = TRUE;
               }
            }

            if (fUpdate)
            {
               //
               // 单字直接呈现到屏幕
               //
               FONT_RECT.w += 1;
               FONT_RECT.h += 1;

               PalVideo.Update(FONT_RECT);
            }

            //
            // 下个单字 PosX 右移
            //
            FONT_RECT.x += (INT)sngMetrics.Width;
         }

         if (DIALOG_POS == DialogPosition.Upper
            || DIALOG_POS == DialogPosition.Lower)
         {
            //
            // 若为普通的上下对话框，则在末尾显示对话光标
            //
            FONT_RECT.w = FONT_RECT.h = FONT_SIZE + 1;

            PalUnpak.RLEBlitToSurface(
               PalUnpak.SpriteGetFrame(g_Global.FONT_ICON, FONT_ICON_ID),
               ref DX_Screen,
               PAL_XY(FONT_RECT.x, FONT_RECT.y)
            );

            PalVideo.Update(FONT_RECT);

            //
            // 玩家击键后清除当前对话
            //
            while (TRUE)
            {
               PalInput.ClearKeyState();
               PalUtil.DelayTime(100);

               if (KeyPress())
               {
                  //
                  // 按下任意键
                  //
                  break;
               }

               if (DIALOG_POS == DialogPosition.Upper
                  || DIALOG_POS == DialogPosition.Lower
                  || DIALOG_POS == DialogPosition.Menu)
               {
                  //
                  // 光标 palette 左移
                  //
                  BGRA bgra = PalPalette.GetColor(0xF9);
                  for (bColorID = 0xF9; bColorID < 0xFE; bColorID++)
                  {
                     PalPalette.SetColor(bColorID, PalPalette.GetColor((BYTE)(bColorID + 1)));
                  }
                  PalPalette.SetColor(0xFE, bgra);

                  PalVideo.Update();
               }
            }

            FONT_ICON_ID   = 0;
            IS_SKIP        = FALSE;
         }
      }

      public static void
      SetDialogDelayTime(
         DWORD    iDelayTime
      )
      /*++
       * 
       * 作用：
       *    设置对话框倒计时自动关闭的时长
       * 
       * 参数：
       *    [IN]  iDelayTime     欲设置的倒计时时长.
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         DELAY_CLEAR = iDelayTime;
      }
   }
}
