using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace LibClipboard.Core.Internal
{
    /// <summary>
    /// Represents a safe handle for the native clipboard pointer.
    /// </summary>
    internal class ClipboardHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private ClipboardHandle() : base(true) { }

        protected override bool ReleaseHandle()
        {
            try
            {
                NativeMethods.clipboard_free(handle);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Contains P/Invoke declarations for the native libclipboard library.
    /// </summary>
    internal static class NativeMethods
    {
        private const string LibName = "libclipboard";

        static NativeMethods()
        {
            NativeLibrary.SetDllImportResolver(typeof(NativeMethods).Assembly, DllImportResolver);
        }

        private static IntPtr DllImportResolver(string libraryName, System.Reflection.Assembly assembly, DllImportSearchPath? searchPath)
        {
            if (libraryName == LibName)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    if (NativeLibrary.TryLoad($"{LibName}.dll", assembly, searchPath, out IntPtr handle))
                        return handle;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    if (NativeLibrary.TryLoad($"lib{LibName}.so", assembly, searchPath, out IntPtr handle))
                        return handle;
                    if (NativeLibrary.TryLoad($"{LibName}.so", assembly, searchPath, out handle))
                        return handle;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    if (NativeLibrary.TryLoad($"lib{LibName}.dylib", assembly, searchPath, out IntPtr handle))
                        return handle;
                    if (NativeLibrary.TryLoad($"{LibName}.dylib", assembly, searchPath, out handle))
                        return handle;
                }
            }
            return IntPtr.Zero;
        }

        // Core clipboard functions
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ClipboardHandle clipboard_new(IntPtr opts);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void clipboard_free(IntPtr cb);

        // Text functions
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int clipboard_set_text(ClipboardHandle cb, byte[] text);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr clipboard_text(ClipboardHandle cb);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void clipboard_text_free(ClipboardHandle cb, IntPtr text);

        // Image functions
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int clipboard_set_image(ClipboardHandle cb, byte[] data, int length);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr clipboard_image(ClipboardHandle cb, out int length);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void clipboard_image_free(ClipboardHandle cb, IntPtr data);

        // Status functions
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int clipboard_has_text(ClipboardHandle cb);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int clipboard_has_image(ClipboardHandle cb);

        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int clipboard_has_ownership(ClipboardHandle cb);

        // Polling function
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int clipboard_poll(ClipboardHandle cb);

        // Clear function
        [DllImport(LibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int clipboard_clear(ClipboardHandle cb);
    }
}
