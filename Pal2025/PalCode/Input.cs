using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using System.Xml.Linq;

using DKey        = SharpDX.DirectInput.Key;

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

using static Pal.PalGlobal;
using static Pal.PalSystem;
using static Pal.PalVideo;
using static Pal.PalInput;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security.Cryptography;

namespace Pal
{
   public unsafe class PalInput
   {
      public enum PalKey
      {
         None = 0,
         Menu,
         Search,
         Down,
         Left,
         Up,
         Right,
         PgUp,
         PgDn,
         Repeat,
         Auto,
         Defend,
         UseItem,
         ThrowItem,
         Flee,
         Status,
         Force,
         Home,
         End,
         AnyKey,
         MaxKey = AnyKey,
      };

      public struct PalKeyMap
      {
         public DKey       DX;
         public PalKey     PAL;

         public PalKeyMap(
            DKey     dx,
            PalKey   pal
         )
         {
            DX    = dx;
            PAL   = pal;
         }
      }

      public static PalKeyMap[] PAL_KEYMAP = new PalKeyMap[] {
         new PalKeyMap( DKey.Up,             PalKey.Up         ),
         new PalKeyMap( DKey.NumberPad8,     PalKey.Up         ),
         new PalKeyMap( DKey.Down,           PalKey.Down       ),
         new PalKeyMap( DKey.NumberPad2,     PalKey.Down       ),
         new PalKeyMap( DKey.Left,           PalKey.Left       ),
         new PalKeyMap( DKey.NumberPad4,     PalKey.Left       ),
         new PalKeyMap( DKey.Right,          PalKey.Right      ),
         new PalKeyMap( DKey.NumberPad8,     PalKey.Right      ),
         new PalKeyMap( DKey.Escape,         PalKey.Menu       ),
         new PalKeyMap( DKey.Insert,         PalKey.Menu       ),
         new PalKeyMap( DKey.LeftAlt,        PalKey.Menu       ),
         //new PalKeyMap( DKey.RightAlt,       PalKey.Menu       ),
         new PalKeyMap( DKey.NumberPad0,     PalKey.Menu       ),
         new PalKeyMap( DKey.Return,         PalKey.Search     ),
         new PalKeyMap( DKey.Space,          PalKey.Search     ),
         new PalKeyMap( DKey.NumberPadEnter, PalKey.Search     ),
         new PalKeyMap( DKey.LeftControl,    PalKey.Search     ),
         new PalKeyMap( DKey.PageUp,         PalKey.PgUp       ),
         new PalKeyMap( DKey.NumberPad9,     PalKey.PgUp       ),
         new PalKeyMap( DKey.PageDown,       PalKey.PgDn       ),
         new PalKeyMap( DKey.NumberPad3,     PalKey.PgDn       ),
         new PalKeyMap( DKey.Home,           PalKey.Home       ),
         new PalKeyMap( DKey.NumberPad7,     PalKey.Home       ),
         new PalKeyMap( DKey.End,            PalKey.End        ),
         new PalKeyMap( DKey.NumberPad1,     PalKey.End        ),
         new PalKeyMap( DKey.R,              PalKey.Repeat     ),
         new PalKeyMap( DKey.A,              PalKey.Auto       ),
         new PalKeyMap( DKey.D,              PalKey.Defend     ),
         new PalKeyMap( DKey.E,              PalKey.UseItem    ),
         new PalKeyMap( DKey.W,              PalKey.ThrowItem  ),
         new PalKeyMap( DKey.Q,              PalKey.Flee       ),
         new PalKeyMap( DKey.F,              PalKey.Force      ),
         new PalKeyMap( DKey.S,              PalKey.Status     ),
      };

      public enum PalDirection
      {
         South = 0,
         West,
         North,
         East,
         Unknown
      }

      private struct PalInputState
      {
         public PalDirection     dir, prevdir;
         //public PalKey           dwKeyPress;
         public fixed DWORD      dwKeyOrder[4];
         public DWORD            dwKeyMaxCount;
      }

      private  static DirectInput      DX_INPUT;
      private  static Keyboard         DX_KEYBOARD;
      private  static KeyboardState    DX_KEY_STATE      = new KeyboardState();
      private  static DWORD[]          listKeyLastTime   = new DWORD[PAL_KEYMAP.Length];
      private  static BOOL[]           PAL_KEY_STATE     = new BOOL[(INT)PalKey.MaxKey];
      public   static BOOL             PAL_KEY_IN        = FALSE;
      private  static PalInputState    PAL_INPUT_STATE;

      public static void
      Init()
      /*++
       * 
       * 作用：
       *    初始化输入系统
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
         // 创建DirectInput对象
         //
         DX_INPUT = new DirectInput();

         //
         // 创建键盘设备对象
         //
         var devices = DX_INPUT.GetDevices(DeviceType.Keyboard, DeviceEnumerationFlags.AllDevices);

         foreach (var device in devices)
         {
            //
            // 搜寻键盘设备
            //
            if (device.Type == DeviceType.Keyboard)
            {
               DX_KEYBOARD = new SharpDX.DirectInput.Keyboard(DX_INPUT);
            }
         }
      }

      public static void
      Free()
      /*++
       * 
       * 作用：
       *    释放输入系统占用的内存
       * 
       * 参数：
       *    无
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         if (DX_INPUT != null)
         {
            DX_INPUT.Dispose();
            DX_INPUT = null;
         }

         if (DX_KEYBOARD != null)
         {
            DX_KEYBOARD.Dispose();
            DX_KEYBOARD = null;
         }
      }

      public static PalDirection
      GetCurrDirection()
      /*++
       * 
       * 作用：
       *    获取当前方向
       * 
       * 参数：
       *    无
       * 
       * 返回值：
       *    
       * 
      --*/
      {
         INT               i;
         PalDirection      currDir = PalDirection.South;

         for (i = 1; i < 4; i++)
         {
            if (PAL_INPUT_STATE.dwKeyOrder[(INT)currDir] < PAL_INPUT_STATE.dwKeyOrder[i])
            {
               currDir = (PalDirection)i;
            }
         }

         if (!_2B(PAL_INPUT_STATE.dwKeyOrder[(INT)currDir])) currDir = PalDirection.Unknown;

         return currDir;
      }

      public static void
      KeyDown(
         PalKey      key,
         BOOL        fRepeat
      )
      /*++
        Purpose:

          Called when user pressed a key.

        Parameters:

          [IN]  key - keycode of the pressed key.

        Return value:

          None.

      --*/
      {
         PalDirection currDir = PalDirection.Unknown;

         if (!fRepeat)
         {
            if (_2B(key & PalKey.Down))
            {
               currDir = PalDirection.South;
            }
            else if (_2B(key & PalKey.Left))
            {
               currDir = PalDirection.West;
            }
            else if (_2B(key & PalKey.Up))
            {
               currDir = PalDirection.North;
            }
            else if (_2B(key & PalKey.Right))
            {
               currDir = PalDirection.East;
            }

            if (currDir != PalDirection.Unknown)
            {
               PAL_INPUT_STATE.dwKeyMaxCount++;
               PAL_INPUT_STATE.dwKeyOrder[(INT)currDir] = PAL_INPUT_STATE.dwKeyMaxCount;
               PAL_INPUT_STATE.dir = PalInput.GetCurrDirection();
            }
         }

         //PAL_INPUT_STATE.dwKeyPress |= key;
      }

      public static void
      KeyUp(
         PalKey      key
      )
      /*++
       * 
       * 作用：
       *    设置按键弹起状态
       * 
       * 参数：
       *    用户弹起的按键
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         PalDirection currDir = PalDirection.Unknown;

         if (_2B(key & PalKey.Down))
         {
            currDir = PalDirection.South;
         }
         else if (_2B(key & PalKey.Left))
         {
            currDir = PalDirection.West;
         }
         else if (_2B(key & PalKey.Up))
         {
            currDir = PalDirection.North;
         }
         else if (_2B(key & PalKey.Right))
         {
            currDir = PalDirection.East;
         }

         if (currDir != PalDirection.Unknown)
         {
            PAL_INPUT_STATE.dwKeyOrder[(INT)currDir] = 0;
            currDir = PalInput.GetCurrDirection();
            PAL_INPUT_STATE.dwKeyMaxCount = (currDir == PalDirection.Unknown) ? 0 : PAL_INPUT_STATE.dwKeyOrder[(INT)currDir];
            PAL_INPUT_STATE.dir = currDir;
         }
      }

      public static void
      UpdateKeyboardState()
      /*++
       * 
       * 作用：
       *    检查并更新输入缓存
       * 
       * 参数：
       *    无
       * 
       * 返回值：
       *    无
       * 
      --*/
      {
         int            i;
         DWORD          dwCurrentTime;

         if (!PAL_KEY_IN)
         {
            //
            // 仅允许前台模式
            //
            return;
         }

         //
         // 开始接收按键
         //
         DX_KEYBOARD.Acquire();

         //
         // 获取按键状态
         //
         DX_KEY_STATE   = DX_KEYBOARD.GetCurrentState();

         //
         // 未按下任何键，直接退出
         //
         if (DX_KEY_STATE.PressedKeys.Count == 0) return;

         //
         // 获取当前的具体时刻
         //
         dwCurrentTime = PalTimer.GetTicks();

         //
         // 对游戏中用到的按键进行按下状态检查
         //
         for (i = 0; i < PAL_KEYMAP.Length; i++)
         {
            //
            // 跳过已被触发的功能键
            //
            if (PAL_KEY_STATE[(INT)PAL_KEYMAP[i].PAL]) continue;

            //
            // 检查该按键是否被按下
            //
            PAL_KEY_STATE[(INT)PAL_KEYMAP[i].PAL] = DX_KEY_STATE.IsPressed(PAL_KEYMAP[i].DX);

            if (PAL_KEY_STATE[(INT)PAL_KEYMAP[i].PAL])
            {
               //
               // 检查该按键是否在范围内
               //
               if (dwCurrentTime > listKeyLastTime[i])
               {
                  //
                  // 缓存此按键的按下状态
                  //
                  PalInput.KeyDown(PAL_KEYMAP[i].PAL, (listKeyLastTime[i] != 0));
#if FALSE
                  if (!fEnableKeyRepeat)
                  {
                     rgdwKeyLastTime[i] = 0xFFFFFFFF;
                  }
                  else
#endif   // FALSE
                  {
                     //
                     // 记录此按键按下的具体时刻
                     //
                     listKeyLastTime[i] = (DWORD)(dwCurrentTime + (listKeyLastTime[i] == 0 ? 600 : 75));
                  }
               }
            }
            else
            {
               if (listKeyLastTime[i] != 0)
               {
                  //
                  // 清除此按键的按下状态
                  //
                  PalInput.KeyUp(PAL_KEYMAP[i].PAL);

                  //
                  // 清除此按键按下的具体时刻
                  //
                  listKeyLastTime[i] = 0;
               }
            }
         }

         //
         // 结束接收按键
         //
         DX_KEYBOARD.Unacquire();
      }

      public static void
      ProcessEvent()
      /*++
       * 
       * 作用：
       *    处理窗口输入
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
         // 执行窗体事件
         //
         Application.DoEvents();

         //
         // 检查哪些键被按下了
         //
         PalInput.UpdateKeyboardState();
      }

      public static BOOL
      KeyPress(
         PalKey   key = PalKey.AnyKey
      )
      {
         return (key == PalKey.AnyKey) ? (DX_KEY_STATE.PressedKeys.Count > 0) : PAL_KEY_STATE[(INT)key];
      }

      public static void
      ClearKeyState()
      /*++
       * 
       * 作用：
       *    清理输入缓存
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

         //PAL_INPUT_STATE.dwKeyPress = 0;

         for (i = 0; i < PAL_KEY_STATE.Length; i++)
         {
            PAL_KEY_STATE[i] = FALSE;
         }
      }
   }
}
