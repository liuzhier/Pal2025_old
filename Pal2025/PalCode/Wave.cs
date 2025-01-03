using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.DirectSound;
using SharpDX.Multimedia;

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
using static Pal.PalGlobal;
using static Pal.PalSystem;
using static Pal.PalCommon;

namespace Pal
{
   public unsafe class PalWave
   {
      public static DirectSound                 DX_SOUND;
      public static SoundBuffer                 DX_SOUND_BUF;
      public static SoundBufferDescription      DX_SOUND_BUF_DESC;

      public static MemoryStream                stmByteTmp;
      public static SoundStream                 stmSoundTmp;
      public static DataStream                  stmdataTmp;
      public static DataStream                  secondPart;

      public static void
      Init()
      {
         // 创建DirectSound对象
         DX_SOUND = new DirectSound();

         // 设置协作级别
         DX_SOUND.SetCooperativeLevel(WIN_Pal.Handle, CooperativeLevel.Normal);
      }

      public static void
      Play(
         INT      iSoundID
      )
      {
         //
         // 释放所有临时流
         //
         FreeTmp();

         //
         // 从指针流中读取 Wav
         //
         stmByteTmp = new MemoryStream(FILE.ReadAllBytes($"{g_lpszGamePath}/{g_lpszWavePath}/{iSoundID:D3}.wav"));

         //
         // 创建声音流对象
         //
         stmSoundTmp = new SoundStream(stmByteTmp);

         //
         // 设置缓冲区描述
         //
         DX_SOUND_BUF_DESC = new SoundBufferDescription
         {
            Format      = stmSoundTmp.Format,
            BufferBytes = (INT)stmSoundTmp.Length,
            Flags       = BufferFlags.GlobalFocus
                        | BufferFlags.ControlPositionNotify
                        | BufferFlags.GetCurrentPosition2
                        | BufferFlags.ControlPan
                        | BufferFlags.ControlVolume
                        | BufferFlags.ControlFrequency
                        | BufferFlags.ControlFrequency,
         };

         //
         // 创建声音缓冲区
         //
         DX_SOUND_BUF = new PrimarySoundBuffer(DX_SOUND, DX_SOUND_BUF_DESC);

         //
         // 将声音数据写入缓冲区
         //
         stmdataTmp = DX_SOUND_BUF.Lock(0, DX_SOUND_BUF_DESC.BufferBytes, LockFlags.None, out secondPart);
         stmSoundTmp.ToDataStream().CopyTo(stmdataTmp);
         DX_SOUND_BUF.Unlock(stmdataTmp, secondPart);

         //
         // 开始播放声音
         //
         DX_SOUND_BUF.Play(0, PlayFlags.Looping);
      }

      public static void
      Stop()
      {
         // 停止播放
         if (DX_SOUND_BUF != null)
         {
            DX_SOUND_BUF.Stop();
         }
      }

      public static void
      FreeTmp()
      {
         Stop();

         if (DX_SOUND_BUF != null)
         {
            DX_SOUND_BUF.Dispose();
            DX_SOUND_BUF = null;
         }

         if (stmByteTmp != null)
         {
            stmByteTmp.Dispose();
            stmByteTmp = null;
         }

         if (stmSoundTmp != null)
         {
            stmSoundTmp.Dispose();
            stmSoundTmp = null;
         }

         if (stmdataTmp != null)
         {
            stmdataTmp.Dispose();
            stmdataTmp = null;
         }

         if (secondPart != null)
         {
            secondPart.Dispose();
            secondPart = null;
         }
      }

      public static void
      Free()
      {
         FreeTmp();

         if (DX_SOUND != null)
         {
            DX_SOUND.Dispose();
            DX_SOUND = null;
         }
      }
   }
}
