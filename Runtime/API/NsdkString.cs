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
