using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace sharpworm
{
    class shell_exec
    {
        //// Special thanks to Dan @ http://www.kizhakkinan.com/?p=16

        //[DllImport("user32")]
        //private static extern int CallWindowProc
        //    (IntPtr lpPrevWndFunc, int hWnd, int Msg, int wParam, int lParam);
 
        //public unsafe int Add(int a, int b)
        //{
        //    // 4 parameter can be passed to CallWindowProc and can be adressed from 0x0C, 0x10, 0x14 and 0x18
        //    byte[] shellCode = { 0x8B, 0x45, 0x0C, 0x8B, 0x5D, 0x10, 0x03, 0xC3, 0xC2, 0x10, 0x00 };
        //    fixed (byte* bytePointer = shellCode)
        //    {
        //        IntPtr pointer = (IntPtr)bytePointer;
        //        return CallWindowProc(pointer, a, b, 0, 0);
        //    }
        //}
    }
}
