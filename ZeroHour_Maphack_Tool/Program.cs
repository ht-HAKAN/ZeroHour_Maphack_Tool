/*
 * =========================================================================================
 * PROJECT        : Command & Conquer Generals Zero Hour - Maphack & Radar Tool
 * DEVELOPED BY   : Hakan Tuna
 * VERSION        : 1.0.5 (Compatible with Official v1.05 Patch)
 * DESCRIPTION    : A specialized memory manipulation tool designed for Zero Hour v1.05.
 * This utility uses external byte-patching via Win32 API to unlock the radar and remove the Shroud/Fog of War.

 * * [ IMPORTANT: VERSION REQUIREMENT ]
 * - This tool is EXCLUSIVELY designed for Zero Hour v1.05.
 * - Using this tool on other versions (v1.04, mods, etc.) WILL NOT WORK and may cause the game to crash due to offset mismatches.
 * - Running with Administrator privileges is required for process memory access.
 * =========================================================================================
 */

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace ZeroHour_Maphack_Tool
{
    class Program
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out int lpNumberOfBytesWritten);

        // Process Access Rights
        const int PROCESS_ALL_ACCESS = 0x1F0FFF;

        static void Main(string[] args)
        {
            Console.Title = "Command & Conquer Generals Zero Hour Maphack Tool";

            Console.WriteLine("--- Command & Conquer Generals Zero Hour & Radar Tool ---");
            Console.WriteLine("[INFO] Waiting for game process (game.dat)...");

            Process targetProcess = null;
            IntPtr baseAddress = IntPtr.Zero;

            // 1. Process Detection Loop
            while (true)
            {
                foreach (Process p in Process.GetProcesses())
                {
                    try
                    {
                        // Check for standard ZH process names
                        if (p.ProcessName.ToLower() == "game" || p.ProcessName.ToLower() == "game.dat")
                        {
                            targetProcess = p;
                            baseAddress = p.MainModule.BaseAddress;
                            break;
                        }
                    }
                    catch {}
                }

                if (targetProcess != null) break;
                Thread.Sleep(500); // Check every 0.5 seconds
            }


            Console.WriteLine($"[INFO] Process found: {targetProcess.ProcessName} (PID: {targetProcess.Id})");
            Console.WriteLine($"[INFO] Base Address: 0x{baseAddress.ToString("X")}");

            IntPtr handle = OpenProcess(PROCESS_ALL_ACCESS, false, targetProcess.Id);

            if (handle != IntPtr.Zero)
            {
                Console.WriteLine("[INFO] Applying patches...");

                try
                {
                    //  Patch 1: Maphack (Remove Fog of War) 
                    PatchMemory(handle, baseAddress, 0x6F511, new byte[] { 0x31, 0xDB, 0x90, 0x90 });
                    PatchMemory(handle, baseAddress, 0x37A57E, new byte[] { 0xEB, 0x04 });
                    Console.WriteLine("[+] Maphack applied successfully.");

                    //  Patch 2: Stealth Hack (Detect Invisible Units) 
                    PatchMemory(handle, baseAddress, 0x72E5B, new byte[] { 0x31, 0xFF, 0x90, 0x90 });
                    PatchMemory(handle, baseAddress, 0x37A52D, new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });
                    Console.WriteLine("[+] Stealth hack applied successfully.");

                    // Patch 3: Radar Hack (Enable Radar) 
                    PatchMemory(handle, baseAddress, 0x56C00, new byte[] { 0x31, 0xC0, 0x40, 0xC3, 0x90 });
                    Console.WriteLine("[+] Radar hack applied successfully.");

                    Console.WriteLine("---------------------------------------------");
                    Console.WriteLine("[SUCCESS] All cheats active. You can play now.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] An error occurred while patching: {ex.Message}");
                }

                Console.WriteLine("\nPress ENTER to close this tool...");
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("[ERROR] Failed to open process. Please run as Administrator.");
                Console.ReadLine();
            }
        }

        // Helper method to write memory
        static void PatchMemory(IntPtr hProcess, IntPtr baseAddr, int offset, byte[] bytes)
        {
            IntPtr targetAddress = IntPtr.Add(baseAddr, offset);
            int written;
            WriteProcessMemory(hProcess, targetAddress, bytes, bytes.Length, out written);
        }
    }
}