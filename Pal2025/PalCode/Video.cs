using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using SharpDX;
using SharpDX.DirectWrite;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using System.Runtime.InteropServices;

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

using D2D1        = SharpDX.Direct2D1;
using DWrite      = SharpDX.DirectWrite;
using DBitmap     = SharpDX.Direct2D1.Bitmap;

using static Pal.PalText;
using static Pal.PalPalette;
using static Pal.PalSystem;
using static Pal.PalGlobal;

namespace Pal
{
   public unsafe class PalVideo
   {
      public   const    WORD           WIN_W                = 800;
      public   const    WORD           WIN_H                = 600;
      public   const    WORD           WIDTH                = 320;
      public   const    WORD           HEIGHT               = 200;
      public   static   RawRectangle   RECT                 = new RawRectangle(0, 0, WIDTH, HEIGHT);
      public   static   Pal_Rect       PAL_RECT             = new Pal_Rect
      {
         x = RECT.Left,
         y = RECT.Top,
         w = RECT.Right - RECT.Left,
         h = RECT.Bottom - RECT.Top,
      };

      public   static   Surface     DX_Screen         = new Surface();  // 主屏
      public   static   Surface     DX_ScreenOff      = new Surface();  // 离屏

      public   static   WIN_Main                      WIN_Pal              = null;
      public   static   D2D1.Factory                  DX_Factory;
      public   static   WindowRenderTarget            DX_Win               = null;
      public   static   RenderTargetProperties        DX_WinDesc;
      public   static   HwndRenderTargetProperties    DX_WinHwndDesc;
      //public   static   SolidColorBrush               DX_Brush             = null;
      public   static   DWrite.Factory                DX_Write_Factory     = null;
      public   static   TextFormat                    DX_Font              = null;

      public struct Pal_Rect
      {
         public INT  x = PAL_ERROR, y = PAL_ERROR;
         public INT  w = PAL_ERROR, h = PAL_ERROR;

         public Pal_Rect() { }
      }

      public struct Surface
      {
         public INT           w, h, pitch;   // 宽、高、宽幅
         public void*         pixels;        // 依赖调色板的像素 Buffer
         public BYTE*         bgra;          // BGRA 像素 Buffer
         public FLOAT         opacity;       // 整体透明度
         public RawColor4     rawColor;      // 整体混合色
         public DBitmap       bmp;           // 实际画面
         public Pal_Rect      rect;          // 矩形脏区范围
      }

      public static void
      BlitSurface(
      ref   Surface        src,
      ref   Surface        dest
      )
      /*++
       * 
       * 作用：
       *    将指定屏幕的画面备份到另一个屏幕
       * 
       * 参数：
       *    无
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         INT      row, col, iSrcPixelID, iDectPixelID;

         dest.w         = Math.Max(src.rect.w, 0);
         dest.w         = Math.Min(src.rect.w, src.w);
         dest.h         = Math.Max(src.rect.h, 0);
         dest.h         = Math.Min(src.rect.h, src.h);
         dest.pitch     = src.pitch;
         dest.opacity   = src.opacity;
         dest.rawColor  = src.rawColor;

         if (dest.pixels == null)
         {
            dest.pixels = (void*)Marshal.AllocHGlobal(dest.w * dest.h);
         }

         //
         // 备份像素矩阵
         //
         for (row = 0; row < dest.h; row++)
         {
            for (col = 0; col < dest.w; col++)
            {
               iSrcPixelID    = src.w * (src.rect.y + row) + src.rect.x + col;
               iDectPixelID   = dest.w * (src.rect.y + row) + dest.rect.x + col;

               ((BYTE*)dest.pixels)[iDectPixelID] = ((BYTE*)src.pixels)[iSrcPixelID];
            }
         }
      }

      public static void
      CopyEntireSurface(
      ref   Surface        src,
      ref   Surface        dest
      )
      /*++
       * 
       * 作用：
       *    将指定屏幕的整个画面备份到另一个屏幕
       * 
       * 参数：
       *    无
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         src.rect    = PalVideo.PAL_RECT;
         dest.rect   = PalVideo.PAL_RECT;

         BlitSurface(ref src, ref dest);
      }

      public static void
      BackupScreen(
      ref   Surface     src
      )
      /*++
       * 
       * 作用：
       *    备份主屏的画面到离屏
       * 
       * 参数：
       *    无
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         PalVideo.CopyEntireSurface(ref src, ref DX_ScreenOff);
      }

      public static void
      RestoreScreen(
      ref   Surface     dest
      )
      /*++
       * 
       * 作用：
       *    还原备份的画面到主屏
       * 
       * 参数：
       *    无
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         PalVideo.CopyEntireSurface(ref DX_ScreenOff, ref dest);
      }

      public static void
      FreeSurface(
      ref   Surface     src
      )
      /*++
       * 
       * 作用：
       *    释放 Surface 占用的内存资源
       * 
       * 参数：
       *    无
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         S_FREE(ref src.pixels);
         S_FREE(ref src.bgra);

         if (src.bmp != null)
         {
            src.bmp.Dispose();
            src.bmp = null;
         }
      }

      private static void
      InitD2D1()
      /*++
       * 
       * 作用：
       *    初始化 D2D1 引擎
       * 
       * 参数：
       *    无
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         DX_Factory        = new D2D1.Factory();

         DX_WinDesc        = new RenderTargetProperties
         {
            DpiX           = 96.0f,
            DpiY           = 96.0f,
            MinLevel       = FeatureLevel.Level_DEFAULT,
            Type           = RenderTargetType.Hardware,
            Usage          = RenderTargetUsage.None,
            PixelFormat    = new PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, AlphaMode.Ignore)
         };

         DX_WinHwndDesc    = new HwndRenderTargetProperties
         {
            Hwnd           = WIN_Pal.Handle,
            // PixelSize      = new Size2(WIN_Pal.ClientSize.Width, WIN_Pal.ClientSize.Height),
            PixelSize      = new Size2(WIDTH, HEIGHT),
            PresentOptions = PresentOptions.RetainContents
         };

         DX_Win                     = new WindowRenderTarget(DX_Factory, DX_WinDesc, DX_WinHwndDesc);
         DX_Win.TextAntialiasMode   = D2D1.TextAntialiasMode.Aliased;
         //DX_Brush                   = new SolidColorBrush(DX_Win, PalPalette.ColorToRaw(0xFF, 0xFF, 0xFF));
         DX_Write_Factory           = new DWrite.Factory();
         DX_Font                    = new TextFormat(DX_Write_Factory, "SimSun", FONT_SIZE);
      }

      public static void
      InitSurface()
      /*++
       * 
       * 作用：
       *    初始化 Surface
       * 
       * 参数：
       *    无
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         INT      i;

         //
         // 初始化主屏
         //
         DX_Screen.w          = WIDTH;
         DX_Screen.h          = HEIGHT;
         DX_Screen.pitch      = DX_Screen.w * 4;
         DX_Screen.pixels     = (BYTE*) Marshal.AllocHGlobal(DX_Screen.w * DX_Screen.h);
         for (i = 0; i < DX_Screen.w * DX_Screen.h; i++) ((BYTE*)DX_Screen.pixels)[i] = 0;
         DX_Screen.bgra       = (BYTE*) Marshal.AllocHGlobal(DX_Screen.pitch * DX_Screen.h);
         DX_Screen.opacity    = 1.0f;
         DX_Screen.rawColor   = PalPalette.ColorToRaw(Color.Black);
         DX_Screen.bmp        = new DBitmap(
            DX_Win,
            new Size2(DX_Screen.w, DX_Screen.h),
            new DataPointer(DX_Screen.bgra, DX_Screen.pitch * DX_Screen.h),
            DX_Screen.pitch,
            new BitmapProperties(DX_Win.PixelFormat)
         );
         DX_Screen.rect = new Pal_Rect
         {
            x = 0, y = 0,
            w = DX_Screen.w, h = DX_Screen.h
         };

         //
         // 初始化离屏
         //
         DX_ScreenOff.w          = WIDTH;
         DX_ScreenOff.h          = HEIGHT;
         DX_ScreenOff.pitch      = DX_ScreenOff.w * 4;
         DX_ScreenOff.pixels     = (BYTE*) Marshal.AllocHGlobal(DX_ScreenOff.w * DX_ScreenOff.h);
         for (i = 0; i < DX_ScreenOff.w * DX_ScreenOff.h; i++) ((BYTE*)DX_ScreenOff.pixels)[i] = 0;
         DX_ScreenOff.bgra       = (BYTE*) Marshal.AllocHGlobal(DX_ScreenOff.pitch * DX_ScreenOff.h);
         DX_ScreenOff.opacity    = 1.0f;
         DX_ScreenOff.rawColor   = PalPalette.ColorToRaw(Color.Black);
         DX_ScreenOff.bmp        = new DBitmap(
            DX_Win,
            new Size2(DX_ScreenOff.w, DX_ScreenOff.h),
            new DataPointer(DX_ScreenOff.bgra, DX_ScreenOff.pitch * DX_ScreenOff.h),
            DX_ScreenOff.pitch,
            new BitmapProperties(DX_Win.PixelFormat)
         );
         DX_ScreenOff.rect = new Pal_Rect
         {
            x = 0, y = 0,
            w = DX_ScreenOff.w, h = DX_ScreenOff.h
         };
      }

      public static void
      Init()
      /*++
       * 
       * 作用：
       *    初始化游戏画面
       * 
       * 参数：
       *    无
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         //
         // 初始化 D2D1
         //
         InitD2D1();

         //
         // 初始化 Surface
         //
         InitSurface();
      }

      private static void
      FreeD2D1()
      /*++
       * 
       * 作用：
       *    释放 D2D1 引擎资源
       * 
       * 参数：
       *    无
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         if (DX_Font != null)
         {
            DX_Font.Dispose();
            DX_Font = null;
         }

         if (DX_Write_Factory != null)
         {
            DX_Write_Factory.Dispose();
            DX_Write_Factory = null;
         }

         //if (DX_Brush != null)
         //{
         //   DX_Brush.Dispose();
         //   DX_Brush = null;
         //}

         if (DX_Win != null)
         {
            DX_Win.Dispose();
            DX_Win = null;
         }

         if (DX_Factory != null)
         {
            DX_Factory.Dispose();
            DX_Factory = null;
         }
      }

      public static void
      Free()
      /*++
       * 
       * 作用：
       *    释放游戏画面资源
       * 
       * 参数：
       *    无
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         //
         // 释放主屏占用的内存资源
         //
         FreeSurface(ref DX_Screen);

         //
         // 释放离屏占用的内存资源
         //
         FreeSurface(ref DX_ScreenOff);

         //
         // 释放 D2D1 引擎
         //
         FreeD2D1();
      }

      public static void
      Update(
         Pal_Rect    rect
      )
      /*++
       * 
       * 作用：
       *    更新范围内的画面
       * 
       * 参数：
       *    [IN]  rect     更新范围
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         INT      row, col, max_row, max_col;
         INT      pixel_offset_x = 0, pixel_offset_y = 0;
         INT      pixel_id, bgra_id;
         BGRA     bgra;

         RECT.Left   = rect.x;
         RECT.Top    = rect.y;
         RECT.Right  = RECT.Left + rect.w;
         RECT.Bottom = RECT.Top + rect.h;

         max_row = Math.Min(rect.h, DX_Screen.h);
         max_col = Math.Min(rect.w, DX_Screen.w);

         for (row = 0; row < max_row; row++)
         {
            //
            // 截断超出屏幕部分
            //
            pixel_offset_y = rect.y + row;

            //
            // 跳过超出屏幕上端的部分
            //
            if (pixel_offset_y < 0) continue;

            //
            // 遇到超出屏幕下端的部分，直接结束绘制
            //
            if (pixel_offset_y >= DX_Screen.h) break;

            for (col = 0; col < max_col; col++)
            {
               //
               // 阻止超出屏幕部分从另一端冒出
               //
               pixel_offset_x = col + rect.x;

               //
               // 跳过超出屏幕左端的部分
               //
               if (pixel_offset_x < 0) continue;

               //
               // 遇到超出屏幕下端的部分，直接跳过该行
               //
               if (pixel_offset_x >= DX_Screen.w) break;

               pixel_id = pixel_offset_y * DX_Screen.w + pixel_offset_x;
               bgra_id  = pixel_offset_y * DX_Screen.pitch + pixel_offset_x * 4;

               //
               // 避免把颜色写到 buffer 以外的地方
               //
               if ((bgra_id < 0) || (pixel_id < 0)
                  || (pixel_id > (DX_Screen.w * DX_Screen.h))
                  || (bgra_id > DX_Screen.pitch * DX_Screen.h))
                  continue;

               bgra = PalPalette.GetColor(((BYTE*)DX_Screen.pixels)[pixel_id]);

               DX_Screen.bgra[bgra_id + 0] = bgra.blue;
               DX_Screen.bgra[bgra_id + 1] = bgra.green;
               DX_Screen.bgra[bgra_id + 2] = bgra.red;
               DX_Screen.bgra[bgra_id + 3] = bgra.alpha;
            }
         }

         //
         // 阻止实际绘制区域超出屏幕
         //
         RECT.Top    = Math.Max(RECT.Top, 0);
         RECT.Top    = Math.Min(RECT.Top, DX_Screen.w);
         RECT.Left   = Math.Max(RECT.Left, 0);
         RECT.Left   = Math.Min(RECT.Left, DX_Screen.w);

         //
         // 将 BGRA 数据写入 Bitmap
         //
         DX_Screen.bmp.CopyFromMemory(
            (IntPtr)DX_Screen.bgra + (RECT.Top * DX_Screen.pitch) + RECT.Left * 4,
            DX_Screen.pitch,
            RECT
         );

         //
         // 显示 Bitmap
         //
         DX_Win.BeginDraw();
         DX_Win.Clear(DX_Screen.rawColor);
         DX_Win.DrawBitmap(DX_Screen.bmp, DX_Screen.opacity, BitmapInterpolationMode.NearestNeighbor);
         //DX_Win.DrawBitmap(DX_ScreenOff.bmp, .8f, BitmapInterpolationMode.NearestNeighbor);
         DX_Win.EndDraw();
      }

      public static void
      Update()
      /*++
       * 
       * 作用：
       *    更新整个游戏画面
       * 
       * 参数：
       *    无
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         Update(DX_Screen.rect);
      }

      public static void
      FadeOut(
         BGRA     bgra,
         BOOL     fNight,
         INT      iDelay
      )
      /*++
       * 
       * 作用：
       *    将屏幕淡出到指定的调色板。
       * 
       * 参数：
       *    [IN]  iPaletteNum    调色板的编号。
       *    [IN]  fNight         是否使用夜间调色板。
       *    [IN]  iDelay         每步之间的延迟。
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         DWORD    dwTime, dwDelay;
         FLOAT    flChange;

         //
         // 设置要淡入的颜色
         //
         DX_Screen.rawColor = PalPalette.ColorToRaw(bgra);

         //
         // 计算每步间的透明度差异
         //
         iDelay  *= 100;
         flChange = 1.0f / iDelay;

         dwTime = PalTimer.GetTicks();

         //
         // 开始淡入
         //
         while (DX_Screen.opacity > 0.0f)
         {
            //
            // 等待一帧的时间。这里接受输入。
            //
            PalUtil.DelayUntil(dwTime);
            dwDelay = PalTimer.GetTicks() + 10;

            //
            // 透明度增加
            //
            DX_Screen.opacity -= flChange;

            PalVideo.Update();
         }
      }

      public static void
      FadeIn(
         INT      iDelay
      )
      /*++
       * 
       * 作用：
       *    将屏幕淡出到指定的调色板。
       * 
       * 参数：
       *    [IN]  iDelay         每步之间的延迟。
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         DWORD    dwTime, dwDelay;
         FLOAT    flChange;

         //
         // 计算每步间的透明度差异
         //
         iDelay  *= 100;
         flChange = 1.0f / iDelay;

         dwTime = PalTimer.GetTicks();

         //
         // 开始淡入
         //
         while (DX_Screen.opacity < 1.0f)
         {
            //
            // 等待一帧的时间。这里接受输入。
            //
            PalUtil.DelayUntil(dwTime);
            dwDelay = PalTimer.GetTicks() + 10;

            //
            // 透明度降低
            //
            DX_Screen.opacity += flChange;

            PalVideo.Update();
         }
      }
   }
}
