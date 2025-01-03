using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using LIST_LPSTR  = System.Collections.Generic.List<string>;

using PAL_POS     = System.UInt32;

namespace Pal
{
   public class PalCommon
   {
#if DEBUG
      public const   LPSTR g_lpszGamePath    = "F:/Pal2025";
#else // DEBUG
      //public const   LPSTR g_lpszGamePath    = Application.StartupPath
      public const LPSTR g_lpszGamePath      = "./";
#endif // DEBUG
      public const   LPSTR g_lpszPalDll      = $"Pal.dll";
      public const   LPSTR g_lpszWavePath    = $"Waves";
      public const   LPSTR g_lpszMoviePath   = $"Videos";
      public const   LPSTR g_lpsz3rd         = $"3rd";
      public const   LPSTR g_lpsz3rdVLC      = $"VLC";
      public static  readonly LPSTR[]  GAME_FILE_NAME_LIST  =
      {
         "ABC.MKF",  "BALL.MKF", "DATA.MKF",    "F.MKF",    "FBP.MKF",
         "FIRE.MKF", "GOP.MKF",  "MAP.MKF",     "MGO.MKF",  "PAT.MKF",
         "RGM.MKF",  "RNG.MKF",  "SOUNDS.MKF",  "SSS.MKF",
      };
      public static  readonly LPSTR    GAME_FILE_MESSAGES_NAME    = "M.MSG";
      public static  readonly LPSTR    GAME_FILE_OBJECTNAME_NAME  = "WORD.DAT";

      public static PAL_POS
      PAL_XY(dynamic x, dynamic y) => (PAL_POS)((((WORD)y << 16) & 0xFFFF0000) | ((WORD)x & 0xFFFF));

      public static WORD
      PAL_X(PAL_POS xy) => (WORD)((xy) & 0xFFFF);

      public static WORD
      PAL_Y(PAL_POS xy) => (WORD)(((xy) >> 16) & 0xFFFF);

      public const   BYTE  OBJECTNAME_SUB_LEN         = 10;       // <对象名称> 的固定字节数

      public const   WORD  PALETTE_COLOR_MAX          = 256;      // <调色板> 的最大颜色计数

      public const   BYTE  COLOR_MESSAGES_NAME        = 0x8C;     // <对话名称> 调色板索引
      public const   BYTE  COLOR_MESSAGES_NORMAL      = 0x4F;     // <普通对话> 调色板索引
      public const   BYTE  COLOR_MESSAGES_SHADOW      = 0x00;     // <对话阴影> 调色板索引

      public const   BYTE  MENU_TITLESCREEN_FIRST     = 0x07;     // <对话阴影> 调色板索引
      public const   BYTE  MENU_TITLESCREEN_COUNT     = 2;        // <对话阴影> 调色板索引

      // maximum number of players in party
      public const   INT   MAX_PLAYERS_IN_PARTY          = 3;

      // total number of possible player roles
      public const   INT   MAX_PLAYER_ROLES              = 6;

      // totally number of playable player roles
      public const   INT   MAX_PLAYABLE_PLAYER_ROLES     = 5;

      // maximum number of equipments for a player
      public const   INT   MAX_PLAYER_EQUIPMENTS         = 6;

      // total number of magic attributes
      public const   INT   NUM_MAGIC_ELEMENTAL           = 5;

      // maximum number of magics for a player
      public const   INT   MAX_PLAYER_MAGICS             = 32;

      // maximum number of effective poisons to players
      public const   INT   MAX_POISONS                   = 16;

      // maximum entries of inventory
      public const   INT   MAX_INVENTORY                 = 256;

      // maximum number of scenes
      public const   INT   MAX_SCENES                    = 300;

      // maximum number of objects
      public const   INT   MAX_OBJECTS                   = 600;
   }
}
