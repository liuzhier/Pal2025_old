using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vlc.DotNet.Forms;

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

using static Pal.PalVideo;
using static Pal.PalCommon;
using static Pal.PalSystem;

namespace Pal
{
   public class PalMovie
   {
      private static VlcControl VLC_CTRL;

      public static void
      Init()
      {
         VLC_CTRL = new VlcControl();
         VLC_CTRL.BeginInit();
         VLC_CTRL.Dock = DockStyle.Fill;
         VLC_CTRL.BackColor = Color.Black;
         VLC_CTRL.Name = "vlcControl1";
         VLC_CTRL.Spu = -1;
         VLC_CTRL.TabIndex = 0;
         VLC_CTRL.Visible = FALSE;
         VLC_CTRL.Enabled = FALSE;
         VLC_CTRL.VlcLibDirectory = new DirectoryInfo($"{g_lpszGamePath}/{g_lpsz3rd}/{g_lpsz3rdVLC}/"); // VLC安装路径
         VLC_CTRL.EndInit();
         WIN_Pal.Controls.Add(VLC_CTRL);
      }

      public static void
      Play(
         INT      iMovieID,
         LPSTR    lpszMovieType = "avi"
      )
      {
         VLC_CTRL.Visible = TRUE;
         VLC_CTRL.Enabled = TRUE;
         //VLC_CTRL.SetMedia(new Uri($"{g_lpszGamePath}/{g_lpszMoviePath}/{iMovieID:D3}.{lpszMovieType}"));
         VLC_CTRL.Play(new FileInfo($"{g_lpszGamePath}/{g_lpszMoviePath}/{iMovieID:D3}.{lpszMovieType}"));
         //VLC_CTRL.SetMedia(new Uri(@"D:\SWORD\NewPal\Video\end_1.bik"));
      }

      public static void
      Stop()
      {
         VLC_CTRL.Stop();
         VLC_CTRL.Enabled = TRUE;
         VLC_CTRL.Visible = FALSE;
      }

      public static BOOL
      Status()
      {
         return VLC_CTRL.IsPlaying;
      }

      public static void
      Free()
      {
         if (VLC_CTRL != null)
         {
            Stop();
            WIN_Pal.Controls.Remove(VLC_CTRL);
            VLC_CTRL.Dispose();
            VLC_CTRL = null;
         }
      }
   }
}
