using System;
using System.Collections.Generic;
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

using PAL_POS     = System.UInt32;

using static Pal.PalSystem;
using static Pal.PalGlobal;
using static Pal.PalCommon;
using static Pal.PalText;
using static Pal.PalInput;
using static Pal.PalVideo;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using static Pal.PalPalette;

namespace Pal
{
   public unsafe class PalUi
   {
      public static  BYTE* lpSpriteUI                          = null;

      public const   WORD  CHUNKNUM_SPRITEUI                   = 9;

      public const   WORD  MENUITEM_VALUE_CANCELLED            = 0xFFFF;

      public const   WORD  MAINMENU_LABEL_NEWGAME              = 7;
      public const   WORD  MAINMENU_LABEL_LOADGAME             = 8;

      public const   WORD  LOADMENU_LABEL_SLOT_FIRST           = 43;

      public const   BYTE  MENUITEM_COLOR                      = 0x0F;
      public const   BYTE  MENUITEM_COLOR_CONFIRMED            = 0x2C;
      public const   BYTE  MENUITEM_COLOR_INACTIVE             = 0x1C;
      public const   BYTE  MENUITEM_COLOR_ALPHA                = 0xFF;
      public const   BYTE  MENUITEM_COLOR_SELECTED_FIRST       = 0xF9;
      public const   BYTE  MENUITEM_COLOR_SELECTED_TOTALNUM    = 6;

      public enum PalNumColor
      {
         Yellow   = 19,
         Blue     = 29,
         Cyan     = 56,
      }

      public enum PalAlignMode
      {
         Left = 0,
         Middle,
         Right,
      }

      public struct PalMenu
      {
         public LPSTR[]          listText;               // 选项文本
         public BYTE             bColorID;               // 文本颜色
         public PAL_POS          pos;                    // 绘制对齐顶点
         public INT              iSpacing;               // 行间距
         public PalAlignMode     alignMode;              // 对齐方式
         public Pal_Rect         rect = new Pal_Rect();  // 实际脏区

         public PalMenu() { }
      }

      public struct PalBoxDesc
      {
         public PAL_POS          pos;              // 绘制/备份的对齐顶点
         public WORD             wWidth, wHeight;  // 宽高
         public PalAlignMode     alignMode;        // 对齐方式
         public BOOL             fSaveArea;        // 备份的画面
         public Surface          savedArea;        // 备份的画面
      }

      public static void
      Init()
      /*++
       * 
       * 作用：
       *    初始化 UI 界面的 Sprite
       * 
       * 参数：
       *    无
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         PalUnpak.MKFReadChunk(out lpSpriteUI, CHUNKNUM_SPRITEUI, g_Global.files.DATA_MKF);
      }

      public static void
      Free()
      /*++
       * 
       * 作用：
       *    释放 UI 界面占用的内存空间
       * 
       * 参数：
       *    无
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         S_FREE(lpSpriteUI);
      }

      public static BYTE
      GetMenuColorWithSelected()
      /*++
       * 
       * 作用：
       *    获取菜单文本光标的颜色索引
       * 
       * 参数：
       *    无
       * 
       * 返回值：
       *    光标颜色索引
       * 
      --*/
      {
         return (BYTE)(MENUITEM_COLOR_SELECTED_FIRST
            + PalTimer.GetTicks()
            / (600 / MENUITEM_COLOR_SELECTED_TOTALNUM)
            % MENUITEM_COLOR_SELECTED_TOTALNUM
         );
      }

      public static void
      DrawRowBox(
         BYTE*       lpBoxLeft,
         BYTE*       lpBoxMid,
         BYTE*       lpBoxRight,
         INT         nLen,
         PAL_POS     pos,
         INT         nShadowOffset = 0
      )
      /*++
       * 
       * 作用：
       *    绘制一行的菜单背景
       * 
       * 参数：
       *    [IN]  lpBoxLeft      起始的 SpriteUI 背景
       *    [IN]  lpBoxMid       重复的 SpriteUI 背景
       *    [IN]  lpBoxRight     结束的 SpriteUI 背景
       *    [IN]  nLen           SpriteUI 背景重复次数
       *    [IN]  pos            绘制坐标
       *    [IN]  nShadowOffset  阴影坐标偏移
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         INT         i, x, y;

         //
         // 获取绘制坐标
         //
         x = PAL_X(pos);
         y = PAL_Y(pos);

         if (nShadowOffset != 0)
         {
            //
            // 绘制阴影
            //
            PalUnpak.RLEBlitToSurfaceWithShadow(
                  lpBoxLeft,
            ref   DX_Screen,
                  PAL_XY(
                     x + nShadowOffset,
                     y + nShadowOffset
                  ),
                  TRUE
            );

            x += PalUnpak.RLEGetWidth(lpBoxLeft);

            for (i = 0; i < nLen; i++)
            {
               PalUnpak.RLEBlitToSurfaceWithShadow(
                     lpBoxMid,
               ref   DX_Screen,
                     PAL_XY(
                        x + nShadowOffset,
                        y + nShadowOffset
                     ),
                     TRUE
               );
               x += PalUnpak.RLEGetWidth(lpBoxMid);
            }

            PalUnpak.RLEBlitToSurfaceWithShadow(
                  lpBoxRight,
            ref   DX_Screen,
                  PAL_XY(x + nShadowOffset, y + nShadowOffset),
                  TRUE
            );
         }

         //
         // 回退坐标
         //
         x = PAL_X(pos);

         //
         // 开始绘制此行背景
         //
         PalUnpak.RLEBlitToSurface(
               lpBoxLeft,
         ref   DX_Screen,
               pos
         );

         x += PalUnpak.RLEGetWidth(lpBoxLeft);

         for (i = 0; i < nLen; i++)
         {
            PalUnpak.RLEBlitToSurface(
                  lpBoxMid,
            ref   DX_Screen,
                  PAL_XY(x, y)
            );
            x += PalUnpak.RLEGetWidth(lpBoxMid);
         }

         PalUnpak.RLEBlitToSurface(
               lpBoxRight,
         ref   DX_Screen,
               PAL_XY(x, y)
         );
      }

      public static void
      DrawPaperBox(
      ref   PalBoxDesc     boxDesc
      )
      /*++
       * 
       * 作用：
       *    绘制纸质菜单背景
       * 
       * 参数：
       *    [IN]  box   盒子的描述内容
       * 
       * 返回值：
       *    无
       * 
      --*/
      {

      }

      public static void
      DrawInventoryBox(
      ref   PalBoxDesc     boxDesc,
            INT            nShadowOffset = 6
      )
      /*++
       * 
       * 作用：
       *    绘制库存菜单背景
       * 
       * 参数：
       *    [IN]  box   盒子的描述内容
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         const INT   iSpriteID_Left    = 9;
         const INT   iSpriteID_Mid     = 10;
         const INT   iSpriteID_Right   = 11;

         BYTE*       lpBoxLeft, lpBoxMid, lpBoxRight;
         Pal_Rect    rectOrigin, rect;
         INT         i;

         rectOrigin = new Pal_Rect
         {
            x = PAL_X(boxDesc.pos),
            y = PAL_Y(boxDesc.pos),
         };

         if (boxDesc.fSaveArea)
         {
            /*++
            //
            // 初始化备份区域
            //
            boxDesc.savedArea = new Surface
            {
               rect = new Pal_Rect
               {
                  x = 0,
                  y = 0,
                  w = boxDesc.wWidth,
                  h = boxDesc.wHeight,
               },
            };

            //
            // 标记欲备份的脏区
            //
            DX_Screen.rect.x = PAL_X(boxDesc.pos);
            DX_Screen.rect.y = PAL_Y(boxDesc.pos);
            DX_Screen.rect.w = boxDesc.savedArea.rect.w;
            DX_Screen.rect.h = boxDesc.savedArea.rect.h;

            //
            // 备份绘制区域
            //
            PalVideo.BlitSurface(
               ref DX_Screen,
               ref boxDesc.savedArea
            );
            --*/
            PalVideo.CopyEntireSurface(ref DX_Screen, ref boxDesc.savedArea);
         }

         //
         // 初始化上边框
         //
         lpBoxLeft   = PalUnpak.SpriteGetFrame(lpSpriteUI, iSpriteID_Left);
         lpBoxMid    = PalUnpak.SpriteGetFrame(lpSpriteUI, iSpriteID_Mid);
         lpBoxRight  = PalUnpak.SpriteGetFrame(lpSpriteUI, iSpriteID_Right);

         //
         // 累积到总尺寸
         //
         rectOrigin.w   = PalUnpak.RLEGetWidth(lpBoxLeft) + PalUnpak.RLEGetWidth(lpBoxRight);
         rectOrigin.w  += PalUnpak.RLEGetWidth(lpBoxMid) * boxDesc.wWidth;

         //
         // 绘制上边框
         //
         PalUi.DrawRowBox(
            lpBoxLeft,
            lpBoxMid,
            lpBoxRight,
            boxDesc.wWidth,
            boxDesc.pos,
            nShadowOffset
         );

         //
         // 累积到总尺寸
         //
         rectOrigin.h += PalUnpak.RLEGetHeight(lpBoxMid);

         //
         // 初始化重复框
         //
         lpBoxLeft   = PalUnpak.SpriteGetFrame(lpSpriteUI, iSpriteID_Left + 3);
         lpBoxMid    = PalUnpak.SpriteGetFrame(lpSpriteUI, iSpriteID_Mid + 3);
         lpBoxRight  = PalUnpak.SpriteGetFrame(lpSpriteUI, iSpriteID_Right + 3);

         //
         // 回退坐标
         //
         rect.x = rectOrigin.x;

         //
         // 绘制重复框
         //
         for (i = 0; i < boxDesc.wHeight; i++)
         {
            PalUi.DrawRowBox(
               lpBoxLeft,
               lpBoxMid,
               lpBoxRight,
               boxDesc.wWidth,
               PAL_XY(rect.x, rectOrigin.y + rectOrigin.h),
               nShadowOffset
            );

            //
            // 累积到总尺寸
            //
            rectOrigin.h += PalUnpak.RLEGetHeight(lpBoxMid);
         }

         //
         // 初始化重复框
         //
         lpBoxLeft = PalUnpak.SpriteGetFrame(lpSpriteUI, iSpriteID_Left + 6);
         lpBoxMid = PalUnpak.SpriteGetFrame(lpSpriteUI, iSpriteID_Mid + 6);
         lpBoxRight = PalUnpak.SpriteGetFrame(lpSpriteUI, iSpriteID_Right + 6);

         //
         // 回退坐标
         //
         rect.x = rectOrigin.x;

         //
         // 绘制下边框
         //
         PalUi.DrawRowBox(
            lpBoxLeft,
            lpBoxMid,
            lpBoxRight,
            boxDesc.wWidth,
            PAL_XY(rect.x, rectOrigin.y + rectOrigin.h),
            nShadowOffset
         );

         //
         // 累积到总尺寸
         //
         rectOrigin.h += PalUnpak.RLEGetHeight(lpBoxMid);

         //
         // 加上阴影的部分
         //
         rectOrigin.w += nShadowOffset;
         rectOrigin.h += nShadowOffset;
      }

      public static void
      DrawBannerBoxV(
      ref   PalBoxDesc     boxDesc
      )
      /*++
       * 
       * 作用：
       *    绘制画卷背景
       * 
       * 参数：
       *    [IN]  box   盒子的描述内容
       * 
       * 返回值：
       *    无
       * 
      --*/
      {

      }

      public static void
      DrawBannerBoxH(
      ref   PalBoxDesc     boxDesc
      )
      /*++
       * 
       * 作用：
       *    绘制横幅背景
       * 
       * 参数：
       *    [IN]  box   盒子的描述内容
       * 
       * 返回值：
       *    无
       * 
      --*/
      {

      }

      public static void
      S_FreeBox(
      ref   PalBoxDesc     boxDesc
      )
      /*++
       * 
       * 作用：
       *    绘制横幅背景
       * 
       * 参数：
       *    [IN]  box   盒子的描述内容
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         if (boxDesc.fSaveArea)
         {
            PalVideo.CopyEntireSurface(ref boxDesc.savedArea, ref DX_Screen);
            PalVideo.FreeSurface(ref boxDesc.savedArea);
         }
      }

      public static void
      DrawNumber(
         DWORD             dwNum,
         PAL_POS           pos,
         PalNumColor       color,
         PalAlignMode      alignMode
      )
      /*++
       * 
       * 作用：
       *    绘制菜单
       * 
       * 参数：
       *    [IN]  dwNum       欲绘制的数值
       *    [IN]  pos         绘制时对齐到的顶点坐标
       *    [IN]  color       数值颜色
       *    [IN]  alignMode   对齐方式
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         const WORD        wUiWidth    = 6;
         const WORD        wUiHeight   = 8;

         INT      i, iNumLen, x, y;
         BYTE*    lpNumSprite;

         iNumLen = $"{dwNum}".Length;

         x = PAL_X(pos);
         y = PAL_Y(pos);

         //
         // 根据对齐方式调整顶点坐标
         //
         switch (alignMode)
         {
            case PalAlignMode.Left:
            default:
               break;

            case PalAlignMode.Middle:
               x -= (wUiWidth * iNumLen) / 2;
               break;

            case PalAlignMode.Right:
               x -= wUiWidth * iNumLen;
               break;
         }

         //
         // 开始绘制数值
         //
         for (i = 0; i < iNumLen; i++, x += wUiWidth)
         {
            lpNumSprite = PalUnpak.SpriteGetFrame(
               lpSpriteUI,
               (INT)color + (INT)(dwNum / (10 * (iNumLen - i)) % 10)
            );

            PalUnpak.RLEBlitToSurface(lpNumSprite, ref DX_Screen, PAL_XY(x, y));
         }
      }

      public static INT
      DrawMenu(
      ref   PalMenu     menu
      )
      /*++
       * 
       * 作用：
       *    绘制菜单
       * 
       * 参数：
       *    [IN]  menu  欲绘制的菜单对象
       * 
       * 返回值：
       *    用户选择的选项【MENUITEM_VALUE_CANCELLED 退出了菜单】【number 选项ID】
       * 
      --*/
      {
         INT         i, iCursorID = 0, iMaxCursorID, iUiWidth = 0, iUiHeight;
         DWORD       pos;
         BYTE        bColorID;

         iMaxCursorID = menu.listText.Length - 1;

         //
         // 设置文字显示模式为菜单
         //
         PalText.DIALOG_POS = DialogPosition.Menu;

         while (TRUE)
         {
            PalInput.ClearKeyState();
            PalUtil.DelayTime(100);

            //
            // 按键检查
            //
            if (KeyPress(PalKey.Up))
            {
               iCursorID--;

               if (iCursorID < 0)
               {
                  iCursorID = iMaxCursorID;
               }
            }
            if (KeyPress(PalKey.Down))
            {
               iCursorID++;

               if (iCursorID > iMaxCursorID)
               {
                  iCursorID = 0;
               }
            }
            if (KeyPress(PalKey.Menu))
            {
               //
               // 用户退出了菜单
               //
               return MENUITEM_VALUE_CANCELLED;
            }
            if (KeyPress(PalKey.Search))
            {
               //
               // 用户选择了选项
               //
               return iCursorID;
            }

            //
            // 计算 Rect
            //
            foreach (LPSTR lpszText in menu.listText)
            {
               iUiWidth = Math.Max(iUiWidth, (INT)PalText.GetTextWidth(lpszText));
            }
            iUiHeight = (menu.listText.Length - 1) * menu.iSpacing + FONT_SIZE + 1;

            //
            // 开始绘制菜单文本
            //
            for (i = 0; i <= iMaxCursorID; i++)
            {
               LPSTR lpszText = menu.listText[i];

               //
               // 设置字体颜色
               //
               if (i == iCursorID)
               {
                  bColorID = PalUi.GetMenuColorWithSelected();
               }
               else
               {
                  bColorID = menu.bColorID;
               }

               //
               // 根据对齐方式设置绘制坐标
               //
               switch (menu.alignMode)
               {
                  case PalAlignMode.Left:
                     pos = PAL_XY(
                        PAL_X(menu.pos),
                        PAL_Y(menu.pos) + menu.iSpacing * i
                     );
                     break;

                  case PalAlignMode.Middle:
                     pos = PAL_XY(
                        PAL_X(menu.pos) - PalText.GetTextWidth(lpszText) / 2,
                        PAL_Y(menu.pos) + menu.iSpacing * i
                     );
                     break;

                  case PalAlignMode.Right:
                     pos = PAL_XY(
                        PAL_X(menu.pos) - PalText.GetTextWidth(lpszText),
                        PAL_Y(menu.pos) + menu.iSpacing * i
                     );
                     break;

                  default:
                     pos = menu.pos;
                     break;
               }

               //
               // 绘制菜单文本
               //
               PalText.DrawText(lpszText, pos, bColorID);
            }

            //
            // 根据对齐方式设置 rect 坐标
            //
            switch (menu.alignMode)
            {
               case PalAlignMode.Left:
               default:
                  pos = menu.pos;
                  break;

               case PalAlignMode.Middle:
                  pos = PAL_XY(
                     PAL_X(menu.pos) - iUiWidth / 2,
                     PAL_Y(menu.pos)
                  );
                  break;

               case PalAlignMode.Right:
                  pos = PAL_XY(
                     PAL_X(menu.pos) - iUiWidth,
                     PAL_Y(menu.pos)
                  );
                  break;
            }

            //
            // 将画面绘制到屏幕
            //
            PalVideo.Update(
               new PalVideo.Pal_Rect
               {
                  x = (menu.rect.x != PAL_ERROR) ? menu.rect.x : (SHORT)PAL_X(pos),
                  y = (menu.rect.y != PAL_ERROR) ? menu.rect.y : (SHORT)PAL_Y(pos),
                  w = (menu.rect.w != PAL_ERROR) ? menu.rect.w : iUiWidth + 1,
                  h = (menu.rect.h != PAL_ERROR) ? menu.rect.h : iUiHeight + 1,
               }
            );
         }
      }

      public static void
      DrawTitleMenuBackground()
      /*++
       * 
       * 作用：
       *    显示主菜单标题画面的背景图
       * 
       * 参数：
       *    无
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         BYTE*    lpBuffer;

         PalUnpak.MKFDecompressChunk(out lpBuffer, 12, g_Global.files.FBP_MKF);
         PalUnpak.FBPBlitToSurface(lpBuffer, ref PalVideo.DX_Screen);
         PalVideo.Update();

         S_FREE(lpBuffer);
      }

      public static INT
      SaveSlotMenu()
      /*++
       * 
       * 作用：
       *    显示存档菜单
       * 
       * 参数：
       *    无
       * 
       * 返回值：
       *    用户选择的选项【MENUITEM_VALUE_CANCELLED 退出了菜单】【number 选项ID】
       * 
      --*/
      {
         INT            i, iSelectSlot;
         PalBoxDesc     boxDesc;
         PalMenu        menu;

         //
         // 初始化背景盒子描述
         //
         boxDesc = new PalBoxDesc
         {
            pos         = PAL_XY(
               PalVideo.WIDTH / 2 + 30,
               6
            ),
            wWidth      = 5,
            wHeight     = 8,
            alignMode   = PalAlignMode.Left,
            fSaveArea   = FALSE,
         };

         //
         // 初始化菜单
         //
         menu = new PalMenu
         {
            listText    = new LPSTR[] {
               g_Global.obgect_name[LOADMENU_LABEL_SLOT_FIRST],
               g_Global.obgect_name[LOADMENU_LABEL_SLOT_FIRST + 1],
               g_Global.obgect_name[LOADMENU_LABEL_SLOT_FIRST + 2],
               g_Global.obgect_name[LOADMENU_LABEL_SLOT_FIRST + 3],
               g_Global.obgect_name[LOADMENU_LABEL_SLOT_FIRST + 4],
               "进度六",
               "进度七",
               "进度八",
               "进度九",
               "便捷进度",
            },
            bColorID    = MENUITEM_COLOR,
            pos         = PAL_XY(
               PAL_X(boxDesc.pos) + 12,
               PAL_Y(boxDesc.pos) + 2
            ),
            iSpacing    = FONT_SIZE + 2,
            alignMode   = boxDesc.alignMode,
            rect        = new Pal_Rect
            {
               x = PAL_X(boxDesc.pos),
               y = PAL_Y(boxDesc.pos),
               w = PalVideo.WIDTH - PAL_X(boxDesc.pos),
               h = PalVideo.HEIGHT - PAL_Y(boxDesc.pos),
            },
         };

         //
         // 绘制背景框
         //
         PalUi.DrawInventoryBox(ref boxDesc);

         //
         // 绘制进度计数
         //
         for (i = 0; i < menu.listText.Length; i++)
         {
            PalUi.DrawNumber(
               PalGlobal.GetSavedTimes(i + 1),
               PAL_XY(
                  PAL_X(menu.pos) + 100,
                  PAL_Y(menu.pos) + 5 + menu.iSpacing * i
               ),
               (i == menu.listText.Length - 1) ? PalNumColor.Cyan : PalNumColor.Yellow,
               PalAlignMode.Right
            );
         }

         //
         // 绘制菜单
         //
         iSelectSlot = PalUi.DrawMenu(ref menu);

         //
         // 删除背景框
         //
         S_FreeBox(ref boxDesc);

         //
         // 
         //
         PalVideo.Update(menu.rect);

         return iSelectSlot;
      }

      public static void
      TitleMenu()
      /*++
       * 
       * 作用：
       *    显示主菜单标题画面
       * 
       * 参数：
       *    无
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         INT         iItemSelected;
         PalMenu     menu;

         //
         // 播放背景音乐 4
         //
         PalWave.Play(4);

         //
         // 绘制背景图【剑葫芦酒】
         //
         PalUi.DrawTitleMenuBackground();
         PalVideo.FadeIn(1);

         while (TRUE)
         {
            //
            // 初始化菜单
            //
            menu = new PalMenu
            {
               listText    = new LPSTR[] {
                  g_Global.obgect_name[MAINMENU_LABEL_NEWGAME],
                  g_Global.obgect_name[MAINMENU_LABEL_LOADGAME],
               },
               bColorID    = MENUITEM_COLOR,
               pos         = PAL_XY(
                  PalVideo.WIDTH / 2,
                  PalVideo.HEIGHT / 2 - 4
               ),
               iSpacing    = FONT_SIZE + 2,
               alignMode   = PalAlignMode.Middle,
            };

            //
            // 检查用户选择的选项
            //
            if (PalUi.DrawMenu(ref menu) == 1)
            {
               //
               // 读取进度
               //
               // 备份屏幕
               //
               PalVideo.BackupScreen(ref DX_Screen);

               //
               // 进入进度菜单
               //
               iItemSelected = PalUi.SaveSlotMenu();

               //
               // 还原屏幕
               //
               PalVideo.RestoreScreen(ref DX_Screen);
               PalVideo.Update();

               if (iItemSelected != MENUITEM_VALUE_CANCELLED)
               {
                  //
                  // 用户取消了读档
                  //
                  break;
               }

               //
               // 开始读档
               //
               PalGlobal.ReadGameSave(iItemSelected);
            }
            else
            {
               //
               // 新建进度
               //

            }
         }
      }
   }
}
