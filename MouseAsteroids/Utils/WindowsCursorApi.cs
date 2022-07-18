using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace MouseAsteroids
{
    public static class WindowsCursorApi
    {
        #region Class Variables

        private const Int32 CURSOR_SHOWING = 0x00000001;

        [StructLayout(LayoutKind.Sequential)]
        private struct ICONINFO
        {
            public bool fIcon;         // Specifies whether this structure defines an icon or a cursor. A value of TRUE specifies 
            public Int32 xHotspot;     // Specifies the x-coordinate of a cursor's hot spot. If this structure defines an icon, the hot 
            public Int32 yHotspot;     // Specifies the y-coordinate of the cursor's hot spot. If this structure defines an icon, the hot 
            public IntPtr hbmMask;     // (HBITMAP) Specifies the icon bitmask bitmap. If this structure defines a black and white icon, 
            public IntPtr hbmColor;    // (HBITMAP) Handle to the icon color bitmap. This member can be optional if this 
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public Int32 x;
            public Int32 y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CURSORINFO
        {
            public Int32 cbSize;        // Specifies the size, in bytes, of the structure. 
            public Int32 flags;         // Specifies the cursor state. This parameter can be one of the following values:
            public IntPtr hCursor;          // Handle to the cursor. 
            public POINT ptScreenPos;       // A POINT structure that receives the screen coordinates of the cursor. 
        }

        #endregion

        #region Class Functions

        [DllImport("user32.dll", EntryPoint = "GetCursorInfo")]
        private static extern bool GetCursorInfo(out CURSORINFO pci);

        [DllImport("user32.dll", EntryPoint = "CopyIcon")]
        private static extern IntPtr CopyIcon(IntPtr hIcon);

        [DllImport("user32.dll", EntryPoint = "GetIconInfo")]
        private static extern bool GetIconInfo(IntPtr hIcon, out ICONINFO piconinfo);

        #endregion

        /// <summary>
        /// https://www.codeproject.com/articles/12850/capturing-the-desktop-screen-with-the-mouse-cursor
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Bitmap? CaptureCursor()
        {
            CURSORINFO cursorInfo = new();
            cursorInfo.cbSize = Marshal.SizeOf(cursorInfo);
            if (GetCursorInfo(out cursorInfo))
            {
                if (cursorInfo.flags == CURSOR_SHOWING)
                {
                    IntPtr hicon = CopyIcon(cursorInfo.hCursor);
                    if (GetIconInfo(hicon, out _))
                    {
                        using Icon icon = Icon.FromHandle(hicon);
                        return icon.ToBitmap();
                    }
                }
            }
            return null;
        }
    }
}
