using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using BOOL        = System.Boolean;
using CHAR        = System.Char;
using BYTE        = System.Byte;
using SHORT       = System.Int16;
using WORD        = System.UInt16;
using INT         = System.Int32;
using UINT        = System.UInt32;
using DWORD       = System.UInt32;
using LONG        = System.Int64;
using QWORD       = System.UInt64;
using LPSTR       = System.String;
using FILE        = System.IO.File;
using LIST_LPSTR  = System.Collections.Generic.List<string>;

using PAL_POS     = System.UInt32;

using static Pal.PalCommon;
using static Pal.PalGlobal;
using static Pal.PalPalette;
using static Pal.PalVideo;
using static Pal.PalSystem;

namespace Pal
{
   public unsafe class PalUnpak
   {
      public static BYTE
      CalcShadowColor(
         BYTE     bSourceColor
      )
      {
         return (BYTE)((bSourceColor & 0xF0) | ((bSourceColor & 0x0F) >> 1));
      }

      public static INT
      RLEBlitToSurface(
            BYTE*       lpBitmapRLE,
      ref   Surface     surfaceDest,
            PAL_POS     pos
      )
      {
         return RLEBlitToSurfaceWithShadow(lpBitmapRLE, ref surfaceDest, pos, FALSE);
      }

      public static INT
      RLEBlitToSurfaceWithShadow(
            BYTE*       lpBitmapRLE,
      ref   Surface     surfaceDest,
            PAL_POS     pos,
            BOOL        bShadow
      )
      {
         UINT        i, j, k, sx;
         INT         x, y;
         UINT        uiLen = 0;
         UINT        uiWidth = 0;
         UINT        uiHeight = 0;
         UINT        uiSrcX = 0;
         BYTE        T;
         INT         dx = (SHORT)PAL_X(pos);
         INT         dy = (SHORT)PAL_Y(pos);
         BYTE*       p;

         fixed (Surface* lpDstSurface = &surfaceDest)
         {
            //
            // Check for NULL pointer.
            //
            if (lpBitmapRLE == null || lpDstSurface == null)
               return -1;

            //
            // Skip the 0x00000002 in the file header.
            //
            if (lpBitmapRLE[0] == 0x02 && lpBitmapRLE[1] == 0x00 &&
               lpBitmapRLE[2] == 0x00 && lpBitmapRLE[3] == 0x00)
            {
               lpBitmapRLE += 4;
            }

            //
            // Get the width and height of the bitmap.
            //
            uiWidth = (UINT)(lpBitmapRLE[0] | (lpBitmapRLE[1] << 8));
            uiHeight = (UINT)(lpBitmapRLE[2] | (lpBitmapRLE[3] << 8));

            //
            // Check whether bitmap intersects the surface.
            //
            if (uiWidth + dx <= 0 || dx >= lpDstSurface->w ||
                uiHeight + dy <= 0 || dy >= lpDstSurface->h)
            {
               goto end;
            }

            //
            // Calculate the total length of the bitmap.
            // The bitmap is 8-bpp, each pixel will use 1 BYTE.
            //
            uiLen = uiWidth * uiHeight;

            //
            // Start decoding and blitting the bitmap.
            //
            lpBitmapRLE += 4;
            for (i = 0; i < uiLen;)
            {
               T = *lpBitmapRLE++;
               if (((T & 0x80) != 0) && (T <= (0x80 + uiWidth)))
               {
                  i += (UINT)(T - 0x80);
                  uiSrcX += (UINT)(T - 0x80);

                  if (uiSrcX >= uiWidth)
                  {
                     uiSrcX -= uiWidth;
                     dy++;
                  }
               }
               else
               {
                  //
                  // Prepare coordinates.
                  //
                  j = 0;
                  sx = uiSrcX;
                  x = (INT)(dx + uiSrcX);
                  y = dy;

                  //
                  // Skip the points which are out of the surface.
                  //
                  if (y < 0)
                  {
                     j += (UINT)(-y * uiWidth);
                     y = 0;
                  }
                  else if (y >= lpDstSurface->h)
                  {
                     goto end; // No more pixels needed, break out
                  }

                  while (j < T)
                  {
                     //
                     // Skip the points which are out of the surface.
                     //
                     if (x < 0)
                     {
                        j -= (UINT)x;

                        if (j >= T) break;

                        sx -= (UINT)x;
                        x = 0;
                     }
                     else if (x >= lpDstSurface->w)
                     {
                        j += uiWidth - sx;
                        x = (INT)(x - sx);
                        sx = 0;
                        y++;

                        if (y >= lpDstSurface->h)
                        {
                           goto end; // No more pixels needed, break out
                        }

                        continue;
                     }

                     //
                     // Put the pixels in row onto the surface
                     //
                     k = T - j;

                     if (lpDstSurface->w - x < k) k = (UINT)(lpDstSurface->w - x);
                     if (uiWidth - sx < k) k = uiWidth - sx;

                     sx += k;
                     p = ((BYTE*)lpDstSurface->pixels) + y * lpDstSurface->w;

                     if (bShadow)
                     {
                        j += k;

                        for (; k != 0; k--)
                        {
                           p[x] = CalcShadowColor(p[x]);
                           x++;
                        }
                     }
                     else
                     {
                        for (; k != 0; k--)
                        {
                           p[x] = lpBitmapRLE[j];
                           j++;
                           x++;
                        }
                     }

                     if (sx >= uiWidth)
                     {
                        sx -= uiWidth;
                        x = (INT)(x - uiWidth);
                        y++;

                        if (y >= lpDstSurface->h)
                        {
                           goto end; // No more pixels needed, break out
                        }
                     }
                  }

                  lpBitmapRLE += T;
                  i += T;
                  uiSrcX += T;

                  while (uiSrcX >= uiWidth)
                  {
                     uiSrcX -= uiWidth;
                     dy++;
                  }
               }
            }
         }

      end:
         //
         // Success
         //
         return 0;
      }

      public static INT
      RLEBlitWithColorShift(
            BYTE*          lpBitmapRLE,
      ref   Surface        surfacelpDest,
            PAL_POS        pos,
            INT            iColorShift
      )
      /*++
        Purpose:

          Blit an RLE-compressed bitmap to an SDL surface.
          NOTE: Assume the surface is already locked, and the surface is a 8-bit one.

        Parameters:

          [IN]  lpBitmapRLE - pointer to the RLE-compressed bitmap to be decoded.

          [OUT] surfacelpDest - pointer to the destination SDL surface.

          [IN]  pos - position of the destination area.

          [IN]  iColorShift - shift the color by this value.

        Return value:

          0 = success, -1 = error.

      --*/
      {
         UINT     i, j, k, sx;
         INT      x, y;
         UINT     uiLen = 0;
         UINT     uiWidth = 0;
         UINT     uiHeight = 0;
         UINT     uiSrcX = 0;
         BYTE     T, b;
         INT      dx = PAL_X(pos);
         INT      dy = PAL_Y(pos);
         BYTE*    p;

         fixed (Surface* lpDstSurface = &surfacelpDest)
         {
            //
            // Check for NULL pointer.
            //
            if (lpBitmapRLE == null || lpDstSurface == null)
            {
               return -1;
            }

            //
            // Skip the 0x00000002 in the file header.
            //
            if (lpBitmapRLE[0] == 0x02 && lpBitmapRLE[1] == 0x00 &&
                lpBitmapRLE[2] == 0x00 && lpBitmapRLE[3] == 0x00)
            {
               lpBitmapRLE += 4;
            }

            //
            // Get the width and height of the bitmap.
            //
            uiWidth = (UINT)(lpBitmapRLE[0] | (lpBitmapRLE[1] << 8));
            uiHeight = (UINT)(lpBitmapRLE[2] | (lpBitmapRLE[3] << 8));

            //
            // Check whether bitmap intersects the surface.
            //
            if (uiWidth + dx <= 0 || dx >= lpDstSurface->w ||
                uiHeight + dy <= 0 || dy >= lpDstSurface->h)
            {
               goto end;
            }

            //
            // Calculate the total length of the bitmap.
            // The bitmap is 8-bpp, each pixel will use 1 BYTE.
            //
            uiLen = uiWidth * uiHeight;

            //
            // Start decoding and blitting the bitmap.
            //
            lpBitmapRLE += 4;
            for (i = 0; i < uiLen;)
            {
               T = *lpBitmapRLE++;
               if (((T & 0x80) != 0) && (T <= (0x80 + uiWidth)))
               {
                  i += (UINT)(T - 0x80);
                  uiSrcX += (UINT)(T - 0x80);

                  if (uiSrcX >= uiWidth)
                  {
                     uiSrcX -= uiWidth;
                     dy++;
                  }
               }
               else
               {
                  //
                  // Prepare coordinates.
                  //
                  j = 0;
                  sx = uiSrcX;
                  x = (INT)(dx + uiSrcX);
                  y = dy;

                  //
                  // Skip the points which are out of the surface.
                  //
                  if (y < 0)
                  {
                     j += (UINT)(-y * uiWidth);
                     y = 0;
                  }
                  else if (y >= lpDstSurface->h)
                  {
                     goto end; // No more pixels needed, break out
                  }

                  while (j < T)
                  {
                     //
                     // Skip the points which are out of the surface.
                     //
                     if (x < 0)
                     {
                        j -= (UINT)x;

                        if (j >= T) break;

                        sx -= (UINT)x;
                        x = 0;
                     }
                     else if (x >= lpDstSurface->w)
                     {
                        j += uiWidth - sx;
                        x = (INT)(x - sx);
                        sx = 0;
                        y++;

                        if (y >= lpDstSurface->h)
                        {
                           goto end; // No more pixels needed, break out
                        }

                        continue;
                     }

                     //
                     // Put the pixels in row onto the surface
                     //
                     k = T - j;

                     if (lpDstSurface->w - x < k) k = (UINT)(lpDstSurface->w - x);
                     if (uiWidth - sx < k) k = uiWidth - sx;

                     sx += k;
                     sx += k;
                     p = ((BYTE*)lpDstSurface->pixels) + y * lpDstSurface->w;

                     for (; k != 0; k--)
                     {
                        b = (BYTE)(lpBitmapRLE[j] & 0x0F);

                        if ((INT)b + iColorShift > 0x0F)
                        {
                           b = 0x0F;
                        }
                        else if ((INT)b + iColorShift < 0)
                        {
                           b = 0;
                        }
                        else
                        {
                           b = (BYTE)(b + iColorShift);
                        }

                        p[x] = (BYTE)(b | (lpBitmapRLE[j] & 0xF0));
                        j++;
                        x++;
                     }

                     if (sx >= uiWidth)
                     {
                        sx -= uiWidth;
                        x = (INT)(x - uiWidth);
                        y++;

                        if (y >= lpDstSurface->h)
                        {
                           goto end; // No more pixels needed, break out
                        }
                     }
                  }

                  lpBitmapRLE += T;
                  i += T;
                  uiSrcX += T;

                  while (uiSrcX >= uiWidth)
                  {
                     uiSrcX -= uiWidth;
                     dy++;
                  }
               }
            }
         }

      end:
         //
         // Success
         //
         return 0;
      }

      public static INT
      RLEBlitMonoColor(
            BYTE*       lpBitmapRLE,
      ref   Surface     surfacelpDest,
            PAL_POS     pos,
            BYTE        bColor,
            INT         iColorShift
      )
      /*++
        Purpose:

          Blit an RLE-compressed bitmap to an SDL surface in mono-color form.
          NOTE: Assume the surface is already locked, and the surface is a 8-bit one.

        Parameters:

          [IN]  lpBitmapRLE - pointer to the RLE-compressed bitmap to be decoded.

          [OUT] lpDstSurface - pointer to the destination SDL surface.

          [IN]  pos - position of the destination area.

          [IN]  bColor - the color to be used while drawing.

          [IN]  iColorShift - shift the color by this value.

        Return value:

          0 = success, -1 = error.

      --*/
      {
         UINT     i, j, k, sx;
         INT      x, y;
         UINT     uiLen = 0;
         UINT     uiWidth = 0;
         UINT     uiHeight = 0;
         UINT     uiSrcX = 0;
         BYTE     T, b;
         INT      dx = PAL_X(pos);
         INT      dy = PAL_Y(pos);
         BYTE*    p;

         fixed (Surface* lpDstSurface = &surfacelpDest)
         {
            //
            // Check for NULL pointer.
            //
            if (lpBitmapRLE == null || lpDstSurface == null)
            {
               return -1;
            }

            //
            // Skip the 0x00000002 in the file header.
            //
            if (lpBitmapRLE[0] == 0x02 && lpBitmapRLE[1] == 0x00 &&
                lpBitmapRLE[2] == 0x00 && lpBitmapRLE[3] == 0x00)
            {
               lpBitmapRLE += 4;
            }

            //
            // Get the width and height of the bitmap.
            //
            uiWidth = (UINT)(lpBitmapRLE[0] | (lpBitmapRLE[1] << 8));
            uiHeight = (UINT)(lpBitmapRLE[2] | (lpBitmapRLE[3] << 8));

            //
            // Check whether bitmap intersects the surface.
            //
            if (uiWidth + dx <= 0 || dx >= lpDstSurface->w ||
                uiHeight + dy <= 0 || dy >= lpDstSurface->h)
            {
               goto end;
            }

            //
            // Calculate the total length of the bitmap.
            // The bitmap is 8-bpp, each pixel will use 1 BYTE.
            //
            uiLen = uiWidth * uiHeight;

            //
            // Start decoding and blitting the bitmap.
            //
            lpBitmapRLE += 4;
            bColor &= 0xF0;
            for (i = 0; i < uiLen;)
            {
               T = *lpBitmapRLE++;
               if (((T & 0x80) != 0) && (T <= (0x80 + uiWidth)))
               {
                  i += (UINT)(T - 0x80);
                  uiSrcX += (UINT)(T - 0x80);

                  if (uiSrcX >= uiWidth)
                  {
                     uiSrcX -= uiWidth;
                     dy++;
                  }
               }
               else
               {
                  //
                  // Prepare coordinates.
                  //
                  j = 0;
                  sx = uiSrcX;
                  x = (INT)(dx + uiSrcX);
                  y = dy;

                  //
                  // Skip the points which are out of the surface.
                  //
                  if (y < 0)
                  {
                     j += (UINT)(-y * uiWidth);
                     y = 0;
                  }
                  else if (y >= lpDstSurface->h)
                  {
                     goto end; // No more pixels needed, break out
                  }

                  while (j < T)
                  {
                     //
                     // Skip the points which are out of the surface.
                     //
                     if (x < 0)
                     {
                        j -= (UINT)x;

                        if (j >= T) break;

                        sx -= (UINT)x;
                        x = 0;
                     }
                     else if (x >= lpDstSurface->w)
                     {
                        j += uiWidth - sx;
                        x = (INT)(x - sx);
                        sx = 0;
                        y++;

                        if (y >= lpDstSurface->h)
                        {
                           goto end; // No more pixels needed, break out
                        }

                        continue;
                     }

                     //
                     // Put the pixels in row onto the surface
                     //
                     k = T - j;

                     if (lpDstSurface->w - x < k) k = (UINT)(lpDstSurface->w - x);
                     if (uiWidth - sx < k) k = uiWidth - sx;

                     sx += k;
                     p = ((BYTE*)lpDstSurface->pixels) + y * lpDstSurface->w;

                     for (; k != 0; k--)
                     {
                        b = (BYTE)(lpBitmapRLE[j] & 0x0F);

                        if ((INT)b + iColorShift > 0x0F)
                        {
                           b = 0x0F;
                        }
                        else if ((INT)b + iColorShift < 0)
                        {
                           b = 0;
                        }
                        else
                        {
                           b = (BYTE)(b + iColorShift);
                        }

                        p[x] = (BYTE)(b | bColor);
                        j++;
                        x++;
                     }

                     if (sx >= uiWidth)
                     {
                        sx -= uiWidth;
                        x = (INT)(x - uiWidth);
                        y++;

                        if (y >= lpDstSurface->h)
                        {
                           goto end; // No more pixels needed, break out
                        }
                     }
                  }

                  lpBitmapRLE += T;
                  i += T;
                  uiSrcX += T;

                  while (uiSrcX >= uiWidth)
                  {
                     uiSrcX -= uiWidth;
                     dy++;
                  }
               }
            }
         }

      end:
         //
         // Success
         //
         return 0;
      }

      public static INT
      FBPBlitToSurface(
            BYTE*       lpBitmapFBP,
      ref   Surface     surfacelpDest
      )
      /*++
        Purpose:

          Blit an uncompressed bitmap in FBP.MKF to an SDL surface.
          NOTE: Assume the surface is already locked, and the surface is a 8-bit 320x200 one.

        Parameters:

          [IN]  lpBitmapFBP - pointer to the RLE-compressed bitmap to be decoded.

          [OUT] lpDstSurface - pointer to the destination SDL surface.

        Return value:

          0 = success, -1 = error.

      --*/
      {
         INT      x, y;
         BYTE*    p;

         fixed (Surface* lpDstSurface = &surfacelpDest)
         {
            if (lpBitmapFBP == null || lpDstSurface == null ||
               lpDstSurface->w != PalVideo.WIDTH || lpDstSurface->h != PalVideo.HEIGHT)
            {
               return PAL_ERROR;
            }

            //
            // simply copy everything to the surface
            //
            for (y = 0; y < PalVideo.HEIGHT; y++)
            {
               p = (BYTE*)(lpDstSurface->pixels) + y * lpDstSurface->w;
               for (x = 0; x < PalVideo.WIDTH; x++)
               {
                  *(p++) = *(lpBitmapFBP++);
               }
            }
         }

         return 0;
      }

      public static INT
      RLEGetWidth(
          BYTE*      lpBitmapRLE
      )
      /*++
        Purpose:

          Get the width of an RLE-compressed bitmap.

        Parameters:

          [IN]  lpBitmapRLE - pointer to an RLE-compressed bitmap.

        Return value:

          Integer value which indicates the height of the bitmap.

      --*/
      {
         if (lpBitmapRLE == null)
         {
            return 0;
         }

         //
         // Skip the 0x00000002 in the file header.
         //
         if (lpBitmapRLE[0] == 0x02 && lpBitmapRLE[1] == 0x00 &&
            lpBitmapRLE[2] == 0x00 && lpBitmapRLE[3] == 0x00)
         {
            lpBitmapRLE += 4;
         }

         //
         // Return the width of the bitmap.
         //
         return lpBitmapRLE[0] | (lpBitmapRLE[1] << 8);
      }

      public static INT
      RLEGetHeight(
          BYTE*      lpBitmapRLE
      )
      /*++
        Purpose:

          Get the width of an RLE-compressed bitmap.

        Parameters:

          [IN]  lpBitmapRLE - pointer to an RLE-compressed bitmap.

        Return value:

          Integer value which indicates the height of the bitmap.

      --*/
      {
         if (lpBitmapRLE == null)
         {
            return 0;
         }

         //
         // Skip the 0x00000002 in the file header.
         //
         if (lpBitmapRLE[0] == 0x02 && lpBitmapRLE[1] == 0x00 &&
            lpBitmapRLE[2] == 0x00 && lpBitmapRLE[3] == 0x00)
         {
            lpBitmapRLE += 4;
         }

         //
         // Return the width of the bitmap.
         //
         return lpBitmapRLE[2] | (lpBitmapRLE[3] << 8);
      }

      public static BYTE*
      MKFSpriteGetFrame(
         BYTE*    lpBuffer,
         INT      iChunkNum
      )
      /*++
        Purpose:

          Get the size of a chunk in an MKF archive.

        Parameters:

          [IN]  lpBuffer - pointer to the destination buffer.

          [IN]  uiChunkNum - the number of the chunk in the MKF archive.

        Return value:

          Integer value which indicates the size of the chunk.
          -1 if the chunk does not exist.

      --*/
      {
         INT*     lpINT;

         //
         // Check whether the uiChunkNum is out of range..
         //
         if (iChunkNum >= MKFGetChunkCount(lpBuffer)) return null;

         //
         // Get the offset of the specified chunk and the next chunk.
         //
         lpINT = (INT*)lpBuffer;

         //
         // Return the length of the chunk.
         //
         return lpBuffer + lpINT[iChunkNum];
      }

      public static INT
      SpriteGetNumFrames(
          BYTE*      lpSprite
      )
      /*++
        Purpose:

          Get the total number of frames of a sprite.

        Parameters:

          [IN]  lpSprite - pointer to the sprite.

        Return value:

          Number of frames of the sprite.

      --*/
      {
         WORD*    lpWORD = (WORD*)lpSprite;

         if (lpSprite == null) return 0;

         return lpWORD[0] - 1;
      }

      public static BYTE*
      SpriteGetFrame(
         BYTE*    lpSprite,
         INT      iFrameNum
      )
      /*++
        Purpose:

          Get the pointer to the specified frame from a sprite.

        Parameters:

          [IN]  lpSprite - pointer to the sprite.

          [IN]  iFrameNum - number of the frame.

        Return value:

          Pointer to the specified frame. NULL if the frame does not exist.

      --*/
      {
         int imagecount, offset;

         if (lpSprite == null) return null;

         //
         // Hack for broken sprites like the Bloody-Mouth Bug
         //
         //   imagecount = (lpSprite[0] | (lpSprite[1] << 8)) - 1;
         //imagecount = lpSprite[0] | (lpSprite[1] << 8);
         imagecount = SpriteGetNumFrames(lpSprite);

         //
         // The frame does not exist
         //
         if (iFrameNum < 0 || iFrameNum >= imagecount) return null;

         //
         // Get the offset of the frame
         //
         iFrameNum <<= 1;
         offset = ((lpSprite[iFrameNum] | (lpSprite[iFrameNum + 1] << 8)) << 1);
         if (offset == 0x18444) offset = (WORD)offset;
         return &lpSprite[offset];
      }

      public static INT
      MKFGetChunkCount(
         BYTE*      lpFileBuf
      )
      /*++
        Purpose:

          Get the number of chunks in an MKF archive.

        Parameters:

          [IN]  lpFileBuf - pointer to an fopen'ed MKF file.

        Return value:

          Integer value which indicates the number of chunks in the specified MKF file.

      --*/
      {
         INT*     lpChunkCount = (INT*)lpFileBuf;

         if (lpFileBuf == null) return PAL_ERROR;

         return lpChunkCount[0] / sizeof(INT) - 1;
      }

      public static INT
      MKFGetChunkSize(
         INT      iChunkNum,
         BYTE*    lpFileBuf
      )
      /*++
        Purpose:

          Get the size of a chunk in an MKF archive.

        Parameters:

          [IN]  iChunkNum - the number of the chunk in the MKF archive.

          [IN]  lpFileBuf - pointer to the fopen'ed MKF file.

        Return value:

          Integer value which indicates the size of the chunk.
          -1 if the chunk does not exist.

      --*/
      {
         INT*     lpINT;
         INT      iOffset, iNextOffset;

         //
         // Get the total number of chunks.
         //
         if (iChunkNum >= MKFGetChunkCount(lpFileBuf)) return PAL_ERROR;


         lpINT = (INT*)lpFileBuf;

         //
         // Get the offset of the specified chunk and the next chunk.
         //
         iOffset     = lpINT[iChunkNum];
         iNextOffset = lpINT[iChunkNum + 1];

         //
         // Return the length of the chunk.
         //
         return iNextOffset - iOffset;
      }

      public static INT
      MKFReadChunk(
      out   BYTE*    lpBuffer,
            INT      iChunkNum,
            BYTE*    lpFileBuf
      )
      /*++
        Purpose:

          Read a chunk from an MKF archive into lpBuffer.

        Parameters:

          [OUT] lpBuffer - pointer to the destination buffer.

          [IN]  iChunkNum - the number of the chunk in the MKF archive to read.

          [IN]  lpFileBuf - pointer to the fopen'ed MKF file.

        Return value:

          Integer value which indicates the size of the chunk.
          -1 if there are error in parameters.

      --*/
      {
         INT*        lpINT;
         INT         iOffset, iNextOffset, iChunkLen;
         BYTE[]      dataBit;

         lpBuffer = null;

         if (lpFileBuf == null) return PAL_ERROR;

         if (lpFileBuf == null) return PAL_ERROR;

         lpINT = (INT*)lpFileBuf;

         //
         // Get the total number of chunks.
         //
         if (iChunkNum >= MKFGetChunkCount(lpFileBuf)) return PAL_ERROR;

         //
         // Get the offset of the chunk.
         //
         iOffset = lpINT[iChunkNum];
         iNextOffset = lpINT[iChunkNum + 1];

         //
         // Get the length of the chunk.
         //
         iChunkLen = iNextOffset - iOffset;

         //
         // Copy memory......
         //
         dataBit = new BYTE[iChunkLen];
         Marshal.Copy((IntPtr)(lpFileBuf + iOffset), dataBit, 0, iChunkLen);
         lpBuffer = (BYTE*)Marshal.AllocHGlobal(dataBit.Length);
         Marshal.Copy(dataBit, 0, (IntPtr)lpBuffer, iChunkLen);

         //
         // Return the length of the chunk.
         //
         return iChunkLen;
      }

      public static INT
      MKFGetDecompressedSize(
         INT      iChunkNum,
         BYTE*    lpFileBuf
      )
      /*++
        Purpose:

          Get the decompressed size of a compressed chunk in an MKF archive.

        Parameters:

          [IN]  iChunkNum - the number of the chunk in the MKF archive.

          [IN]  lpFileBuf - pointer to the fopen'ed MKF file.

        Return value:

          Integer value which indicates the size of the chunk.
          -1 if the chunk does not exist.

      --*/
      {
         INT*     lpINT;

         if (lpFileBuf == null) return -1;

         //
         // Get the total number of chunks.
         //
         if (iChunkNum >= MKFGetChunkCount(lpFileBuf)) return -1;

         lpINT = (INT*)lpFileBuf;

         //
         // Move pointer to header.
         //
         lpINT = (INT*)(lpFileBuf + lpINT[iChunkNum]);

         //
         // Read the header.
         //
         return lpINT[0];
      }

      public static INT
      MKFDecompressChunk(
      out   BYTE*    lpBuffer,
            INT      iChunkNum,
            BYTE*    lpFileBuf
      )
      /*++
        Purpose:

          Decompress a compressed chunk from an MKF archive into lpBuffer.

        Parameters:

          [OUT] lpBuffer - pointer to the destination buffer.

          [IN]  iChunkNum - the number of the chunk in the MKF archive to read.

          [IN]  lpFileBuf - pointer to the fopen'ed MKF file.

        Return value:

          Integer value which indicates the size of the chunk.
          -1 if there are error in parameters, or buffer size is not enough.

      --*/
      {
         BYTE*    buf;
         INT      len;

         len = MKFGetChunkSize(iChunkNum, lpFileBuf);

         lpBuffer = null;
         if (len <= 0) return len;

         MKFReadChunk(out buf, iChunkNum, lpFileBuf);
         if (buf == null)
         {
            return PAL_ERROR;
         }

         len      = MKFGetDecompressedSize(iChunkNum, lpFileBuf);
         lpBuffer = (BYTE*)Marshal.AllocHGlobal(len);

         PalSystem.unpak((INT*)buf, (INT*)lpBuffer);
         Marshal.FreeHGlobal((IntPtr)buf);

         return len;
      }

      public static INT
      RNGBlitToSurface(
            BYTE*       rng,
            INT         length,
      ref   Surface     surfaceDest
      )
      /*++
        Purpose:

          Blit one frame in an RNG animation to an SDL surface.
          The surface should contain the last frame of the RNG, or blank if it's the first
          frame.

          NOTE: Assume the surface is already locked, and the surface is a 320x200 8-bit one.

        Parameters:

          [IN]  rng - Pointer to the RNG data.

          [IN]  length - Length of the RNG data.

          [OUT] lpDstSurface - pointer to the destination SDL surface.

        Return value:

          0 = success, -1 = error.

      --*/
      {
         INT      ptr         = 0;
         INT      dst_ptr     = 0;
         BYTE     wdata       = 0;
         INT      x, y, i, n;

         fixed (Surface* lpDstSurface = &surfaceDest)
         {
            //
            // Check for invalid parameters.
            //
            if (length < 0)
            {
               return -1;
            }

            //
            // Draw the frame to the surface.
            // FIXME: Dirty and ineffective code, needs to be cleaned up
            //
            while (ptr < length)
            {
               BYTE data = rng[ptr++];
               switch (data)
               {
                  case 0x00:
                  case 0x13:
                     //
                     // End
                     //
                     goto end;

                  case 0x02:
                     dst_ptr += 2;
                     break;

                  case 0x03:
                     data = rng[ptr++];
                     dst_ptr += (data + 1) * 2;
                     break;

                  case 0x04:
                     wdata = (BYTE)(rng[ptr] | (rng[ptr + 1] << 8));
                     ptr += 2;
                     dst_ptr += (INT)(((UINT)wdata + 1) * 2);
                     break;

                  case 0x0a:
                     x = dst_ptr % 320;
                     y = dst_ptr / 320;
                     ((BYTE*)(lpDstSurface->pixels))[y * lpDstSurface->pitch + x] = rng[ptr++];
                     if (++x >= 320)
                     {
                        x = 0;
                        ++y;
                     }
                     ((BYTE*)(lpDstSurface->pixels))[y * lpDstSurface->pitch + x] = rng[ptr++];
                     dst_ptr += 2;
                     goto case_0x09;

                  case 0x09:
case_0x09:
                     x = dst_ptr % 320;
                     y = dst_ptr / 320;
                     ((BYTE*)(lpDstSurface->pixels))[y * lpDstSurface->pitch + x] = rng[ptr++];
                     if (++x >= 320)
                     {
                        x = 0;
                        ++y;
                     }
                     ((BYTE*)(lpDstSurface->pixels))[y * lpDstSurface->pitch + x] = rng[ptr++];
                     dst_ptr += 2;
                     goto case_0x08;

                  case 0x08:
case_0x08:
                     x = dst_ptr % 320;
                     y = dst_ptr / 320;
                     ((BYTE*)(lpDstSurface->pixels))[y * lpDstSurface->pitch + x] = rng[ptr++];
                     if (++x >= 320)
                     {
                        x = 0;
                        ++y;
                     }
                     ((BYTE*)(lpDstSurface->pixels))[y * lpDstSurface->pitch + x] = rng[ptr++];
                     dst_ptr += 2;
                     goto case_0x07;

                  case 0x07:
case_0x07:
                     x = dst_ptr % 320;
                     y = dst_ptr / 320;
                     ((BYTE*)(lpDstSurface->pixels))[y * lpDstSurface->pitch + x] = rng[ptr++];
                     if (++x >= 320)
                     {
                        x = 0;
                        ++y;
                     }
                     ((BYTE*)(lpDstSurface->pixels))[y * lpDstSurface->pitch + x] = rng[ptr++];
                     dst_ptr += 2;
                     goto case_0x06;

                  case 0x06:
case_0x06:
                     x = dst_ptr % 320;
                     y = dst_ptr / 320;
                     ((BYTE*)(lpDstSurface->pixels))[y * lpDstSurface->pitch + x] = rng[ptr++];
                     if (++x >= 320)
                     {
                        x = 0;
                        ++y;
                     }
                     ((BYTE*)(lpDstSurface->pixels))[y * lpDstSurface->pitch + x] = rng[ptr++];
                     dst_ptr += 2;
                     break;

                  case 0x0b:
                     data = *(rng + ptr++);
                     for (i = 0; i <= data; i++)
                     {
                        x = dst_ptr % 320;
                        y = dst_ptr / 320;
                        ((BYTE*)(lpDstSurface->pixels))[y * lpDstSurface->pitch + x] = rng[ptr++];
                        if (++x >= 320)
                        {
                           x = 0;
                           ++y;
                        }
                        ((BYTE*)(lpDstSurface->pixels))[y * lpDstSurface->pitch + x] = rng[ptr++];
                        dst_ptr += 2;
                     }
                     break;

                  case 0x0c:
                     wdata = (BYTE)(rng[ptr] | (rng[ptr + 1] << 8));
                     ptr += 2;
                     for (i = 0; i <= wdata; i++)
                     {
                        x = dst_ptr % 320;
                        y = dst_ptr / 320;
                        ((BYTE*)(lpDstSurface->pixels))[y * lpDstSurface->pitch + x] = rng[ptr++];
                        if (++x >= 320)
                        {
                           x = 0;
                           ++y;
                        }
                        ((BYTE*)(lpDstSurface->pixels))[y * lpDstSurface->pitch + x] = rng[ptr++];
                        dst_ptr += 2;
                     }
                     break;

                  case 0x0d:
                  case 0x0e:
                  case 0x0f:
                  case 0x10:
                     for (i = 0; i < data - (0x0d - 2); i++)
                     {
                        x = dst_ptr % 320;
                        y = dst_ptr / 320;
                        ((BYTE*)(lpDstSurface->pixels))[y * lpDstSurface->pitch + x] = rng[ptr];
                        if (++x >= 320)
                        {
                           x = 0;
                           ++y;
                        }
                        ((BYTE*)(lpDstSurface->pixels))[y * lpDstSurface->pitch + x] = rng[ptr + 1];
                        dst_ptr += 2;
                     }
                     ptr += 2;
                     break;

                  case 0x11:
                     data = *(rng + ptr++);
                     for (i = 0; i <= data; i++)
                     {
                        x = dst_ptr % 320;
                        y = dst_ptr / 320;
                        ((BYTE*)(lpDstSurface->pixels))[y * lpDstSurface->pitch + x] = rng[ptr];
                        if (++x >= 320)
                        {
                           x = 0;
                           ++y;
                        }
                        ((BYTE*)(lpDstSurface->pixels))[y * lpDstSurface->pitch + x] = rng[ptr + 1];
                        dst_ptr += 2;
                     }
                     ptr += 2;
                     break;

                  case 0x12:
                     n = (rng[ptr] | (rng[ptr + 1] << 8)) + 1;
                     ptr += 2;
                     for (i = 0; i < n; i++)
                     {
                        x = dst_ptr % 320;
                        y = dst_ptr / 320;
                        ((BYTE*)(lpDstSurface->pixels))[y * lpDstSurface->pitch + x] = rng[ptr];
                        if (++x >= 320)
                        {
                           x = 0;
                           ++y;
                        }
                        ((BYTE*)(lpDstSurface->pixels))[y * lpDstSurface->pitch + x] = rng[ptr + 1];
                        dst_ptr += 2;
                     }
                     ptr += 2;
                     break;
               }
            }
         }

      end:
         return 0;
      }
      
   }
}
