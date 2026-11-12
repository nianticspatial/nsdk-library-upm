// Copyright 2022-2026 Niantic Spatial.

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace NianticSpatial.NSDK.AR.API
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct NsdkString
    {
        public IntPtr data;
        public UInt32 length;

        /// <summary>Copies the UTF-8 bytes into a managed string; empty when unset.</summary>
        public string ToManagedString()
        {
            if (data == IntPtr.Zero || length == 0)
            {
                return string.Empty;
            }

            var bytes = new byte[length];
            Marshal.Copy(data, bytes, 0, (int)length);
            return Encoding.UTF8.GetString(bytes);
        }
    }

    /// <summary>
    /// Mirrors the native NsdkIdString (ARDK_IdString): a NUL-terminated UTF-8 identifier in a
    /// fixed buffer, carried by value inside a native struct rather than borrowed from a resource
    /// handle. Marshal the field as
    /// <c>[MarshalAs(UnmanagedType.ByValArray, SizeConst = NsdkIdString.Size)] public byte[]</c>
    /// and read it with <see cref="Decode"/>.
    /// </summary>
    internal static class NsdkIdString
    {
        /// <summary>Must match ARDK_ID_STRING_SIZE in core/include/ardk_id_string.h.</summary>
        public const int Size = 64;

        /// <summary>
        /// Reads the string up to the first NUL. Returns the empty string for an unset id, and
        /// for a buffer with no terminator at all — the native side always writes one, so a
        /// buffer without one did not come from an NsdkIdString field.
        /// </summary>
        public static string Decode(byte[] buffer)
        {
            if (buffer == null)
            {
                return string.Empty;
            }

            var nulIndex = Array.IndexOf(buffer, (byte)0);
            if (nulIndex < 0)
            {
                return string.Empty;
            }

            return Encoding.UTF8.GetString(buffer, 0, nulIndex);
        }
    }

    internal class ManagedNsdkString : IDisposable
    {
        private readonly IntPtr _data;
        private readonly uint _length;
        private bool _disposed;

        public ManagedNsdkString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                _data = IntPtr.Zero;
                _length = 0;
            }
            else
            {
                byte[] bytes = Encoding.UTF8.GetBytes(str);
                _data = Marshal.AllocHGlobal(bytes.Length);
                Marshal.Copy(bytes, 0, _data, bytes.Length);
                _length = (uint)bytes.Length;
            }
        }

        void IDisposable.Dispose()
        {
            Dispose();
            GC.SuppressFinalize(this);
        }

        ~ManagedNsdkString()
        {
            Dispose();
        }

        public NsdkString ToNsdkString() => new() { data = _data, length = _length };

        private void Dispose()
        {
            if (!_disposed)
            {
                if (_data != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(_data);
                }

                _disposed = true;
            }
        }
    }
}
