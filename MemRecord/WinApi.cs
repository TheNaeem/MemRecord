using System;
using System.Runtime.InteropServices;

namespace MemRecord;

public class Memory
{
    [DllImport("kernel32.dll")]
    public static extern bool ReadProcessMemory(
        IntPtr hProcess, 
        IntPtr lpBaseAddress, 
        byte[] lpBuffer, 
        int dwSize, 
        out IntPtr lpNumberOfBytesRead);
}
