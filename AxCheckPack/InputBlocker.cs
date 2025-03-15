using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AxCheckPack
{
    class InputBlocker
    {
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        // Variables to track barcode scanner input
        private static DateTime _lastKeyTime = DateTime.MinValue;
        private static bool _isBarcodeInput = false;
        //private static int block = 0;

        public static IntPtr BlockInput(int block = 0)
        {
//             block = Convert.ToInt32(STM.QueryData_ExecuteScalarProductEngineering(string.Format(@"SELECT [Seq]
//                          ,[ComputerName]
//                          ,[Active]
//                      FROM [pd].[InputBlocker]
//                      WHERE ComputerName = '{0}' AND Active = 1", STM.GetComputerName)));

            // Set the keyboard hook
            if (block > 0)
                _hookID = SetHook(_proc);

            return _hookID;
        }

        public static IntPtr UnblockInput(int block = 0)
        {
            // Unhook when the application exits
            if (block > 0)
                UnhookWindowsHookEx(_hookID);

            return _hookID;
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(
            int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);

                // Check if the input is from a barcode scanner
                if (IsBarcodeScannerInput(vkCode))
                {
                    _isBarcodeInput = true;
                    // Allow the input
                    return CallNextHookEx(_hookID, nCode, wParam, lParam);
                }
                else
                {
                    // Block non-barcode input
                    return (IntPtr)5;
                }
            }

            // Pass the event to the next hook in the chain
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private static bool IsBarcodeScannerInput(int vkCode)
        {
            // Barcode scanners typically send input very quickly
            DateTime now = DateTime.Now;
            TimeSpan timeSinceLastKey = now - _lastKeyTime;
            _lastKeyTime = now;

            // If the time between key presses is very short, assume it's a barcode scanner
            if (timeSinceLastKey.TotalMilliseconds < 250) // Adjust this threshold as needed
            {
                //MessageBox.Show(timeSinceLastKey.ToString());
                return true;
            }

            return false;
        }

        // Constants
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, Delegate lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

    }
}
