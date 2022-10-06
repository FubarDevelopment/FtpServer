// This file isn't generated, but this comment is necessary to exclude it from StyleCop analysis.
// <auto-generated/>

using System.IO;
using System.Runtime.InteropServices;

namespace System.Net.Security
{
    internal unsafe static class SslDirectCall
    {
        public static void CloseNotify(SslStream sslStream)
        {
            try
            {
                if (sslStream.IsAuthenticated)
                {
                    bool isServer = sslStream.IsServer;

                    byte[] result;
                    int resultSz;

                    int SCHANNEL_SHUTDOWN = 1;
                    var workArray = BitConverter.GetBytes(SCHANNEL_SHUTDOWN);

                    var nativeApiHelpers = GetNativeApiHelpers(sslStream);
                    var (securityContextHandle, credentialsHandleHandle) = nativeApiHelpers.GetHandlesFunc();

                    int bufferSize = 1;
                    NativeApi.SecurityBufferDescriptor securityBufferDescriptor = new NativeApi.SecurityBufferDescriptor(bufferSize);
                    NativeApi.SecurityBufferStruct[] unmanagedBuffer = new NativeApi.SecurityBufferStruct[bufferSize];

                    fixed (NativeApi.SecurityBufferStruct* ptr = unmanagedBuffer)
                    fixed (void* workArrayPtr = workArray)
                    {
                        securityBufferDescriptor.UnmanagedPointer = (void*)ptr;

                        unmanagedBuffer[0].token = (IntPtr)workArrayPtr;
                        unmanagedBuffer[0].count = workArray.Length;
                        unmanagedBuffer[0].type = NativeApi.BufferType.Token;

                        NativeApi.SecurityStatus status;
                        status = (NativeApi.SecurityStatus)NativeApi.ApplyControlToken(ref securityContextHandle, securityBufferDescriptor);
                        if (status == NativeApi.SecurityStatus.OK)
                        {
                            unmanagedBuffer[0].token = IntPtr.Zero;
                            unmanagedBuffer[0].count = 0;
                            unmanagedBuffer[0].type = NativeApi.BufferType.Token;

                            NativeApi.SSPIHandle contextHandleOut = default(NativeApi.SSPIHandle);
                            NativeApi.ContextFlags outflags = NativeApi.ContextFlags.Zero;
                            long ts = 0;

                            var inflags = NativeApi.ContextFlags.SequenceDetect |
                                        NativeApi.ContextFlags.ReplayDetect |
                                        NativeApi.ContextFlags.Confidentiality |
                                        NativeApi.ContextFlags.AcceptExtendedError |
                                        NativeApi.ContextFlags.AllocateMemory |
                                        NativeApi.ContextFlags.InitStream;

                            if (isServer)
                            {
                                status = (NativeApi.SecurityStatus)NativeApi.AcceptSecurityContext(ref credentialsHandleHandle, ref securityContextHandle, null,
                                    inflags, NativeApi.Endianness.Native, ref contextHandleOut, securityBufferDescriptor, ref outflags, out ts);
                            }
                            else
                            {
                                status = (NativeApi.SecurityStatus)NativeApi.InitializeSecurityContextW(ref credentialsHandleHandle, ref securityContextHandle, null,
                                    inflags, 0, NativeApi.Endianness.Native, null, 0, ref contextHandleOut, securityBufferDescriptor, ref outflags, out ts);
                            }
                            if (status == NativeApi.SecurityStatus.OK)
                            {
                                byte[] resultArr = new byte[unmanagedBuffer[0].count];
                                Marshal.Copy(unmanagedBuffer[0].token, resultArr, 0, resultArr.Length);
                                Marshal.FreeCoTaskMem(unmanagedBuffer[0].token);
                                result = resultArr;
                                resultSz = resultArr.Length;
                            }
                            else
                            {
                                throw new InvalidOperationException(string.Format("AcceptSecurityContext/InitializeSecurityContextW returned [{0}] during CloseNotify.", status));
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException(string.Format("ApplyControlToken returned [{0}] during CloseNotify.", status));
                        }
                    }

                    var innerStream = nativeApiHelpers.GetInnerStreamFunc();
                    innerStream.Write(result, 0, resultSz);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private static NativeApiHelpers GetNativeApiHelpers(SslStream sslStream)
        {
            try
            {
                var sslState = ReflectUtil.GetField(sslStream, "_SslState");
                if (sslState != null)
                {
                    return new NativeApiHelpers()
                    {
                        GetHandlesFunc = () => GetHandlesForDotNetFramework(sslState),
                        GetInnerStreamFunc = () => (Stream)ReflectUtil.GetProperty(sslState, "InnerStream"),
                    };
                }

                sslState = ReflectUtil.GetField(sslStream, "_sslState");
                if (sslState != null)
                {
                    return new NativeApiHelpers()
                    {
                        GetHandlesFunc = () => GetHandlesForNetCore10(sslStream),
                        GetInnerStreamFunc = () => (Stream)ReflectUtil.GetField(sslStream, "_innerStream"),
                    };
                }

                return new NativeApiHelpers()
                {
                    GetHandlesFunc = () => GetHandlesForNetCore30(sslStream),
                    GetInnerStreamFunc = () => (Stream)ReflectUtil.GetField(sslStream, "_innerStream"),
                };
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private static (NativeApi.SSPIHandle securityContextHandle, NativeApi.SSPIHandle credentialsHandleHandle)
            GetHandlesForDotNetFramework(object sslstate)
        {
            NativeApi.SSPIHandle securityContextHandle = default;
            NativeApi.SSPIHandle credentialsHandleHandle = default;

            var context = ReflectUtil.GetProperty(sslstate, "Context");

            var securityContext = ReflectUtil.GetField(context, "m_SecurityContext");
            var securityContextHandleOriginal = ReflectUtil.GetField(securityContext, "_handle");
            var credentialsHandle = ReflectUtil.GetField(context, "m_CredentialsHandle");
            var credentialsHandleHandleOriginal = ReflectUtil.GetField(credentialsHandle, "_handle");

            securityContextHandle.HandleHi = (IntPtr)ReflectUtil.GetField(securityContextHandleOriginal, "HandleHi");
            securityContextHandle.HandleLo = (IntPtr)ReflectUtil.GetField(securityContextHandleOriginal, "HandleLo");
            credentialsHandleHandle.HandleHi = (IntPtr)ReflectUtil.GetField(credentialsHandleHandleOriginal, "HandleHi");
            credentialsHandleHandle.HandleLo = (IntPtr)ReflectUtil.GetField(credentialsHandleHandleOriginal, "HandleLo");

            return (securityContextHandle, credentialsHandleHandle);
        }

        private static (NativeApi.SSPIHandle securityContextHandle, NativeApi.SSPIHandle credentialsHandleHandle)
            GetHandlesForNetCore10(SslStream sslStream)
        {
            NativeApi.SSPIHandle securityContextHandle = default;
            NativeApi.SSPIHandle credentialsHandleHandle = default;

            var sslstate = ReflectUtil.GetField(sslStream, "_sslState");

            var context = ReflectUtil.GetProperty(sslstate, "Context");

            var securityContext = ReflectUtil.GetField(context, "_securityContext");
            var securityContextHandleOriginal = ReflectUtil.GetField(securityContext, "_handle");
            var credentialsHandle = ReflectUtil.GetField(context, "_credentialsHandle");
            var credentialsHandleHandleOriginal = ReflectUtil.GetField(credentialsHandle, "_handle");

            // It seems that the order was reverted?
            securityContextHandle.HandleHi = (IntPtr)ReflectUtil.GetField(securityContextHandleOriginal, "dwLower");
            securityContextHandle.HandleLo = (IntPtr)ReflectUtil.GetField(securityContextHandleOriginal, "dwUpper");
            credentialsHandleHandle.HandleHi = (IntPtr)ReflectUtil.GetField(credentialsHandleHandleOriginal, "dwLower");
            credentialsHandleHandle.HandleLo = (IntPtr)ReflectUtil.GetField(credentialsHandleHandleOriginal, "dwUpper");

            return (securityContextHandle, credentialsHandleHandle);
        }

        private static (NativeApi.SSPIHandle securityContextHandle, NativeApi.SSPIHandle credentialsHandleHandle)
            GetHandlesForNetCore30(SslStream sslStream)
        {
            NativeApi.SSPIHandle securityContextHandle = default;
            NativeApi.SSPIHandle credentialsHandleHandle = default;

            var context = ReflectUtil.GetField(sslStream, "_context");

            var securityContext = ReflectUtil.GetField(context, "_securityContext");
            var securityContextHandleOriginal = ReflectUtil.GetField(securityContext, "_handle");
            var credentialsHandle = ReflectUtil.GetField(context, "_credentialsHandle");
            var credentialsHandleHandleOriginal = ReflectUtil.GetField(credentialsHandle, "_handle");

            // It seems that the order was reverted?
            securityContextHandle.HandleHi = (IntPtr)ReflectUtil.GetField(securityContextHandleOriginal, "dwLower");
            securityContextHandle.HandleLo = (IntPtr)ReflectUtil.GetField(securityContextHandleOriginal, "dwUpper");
            credentialsHandleHandle.HandleHi = (IntPtr)ReflectUtil.GetField(credentialsHandleHandleOriginal, "dwLower");
            credentialsHandleHandle.HandleLo = (IntPtr)ReflectUtil.GetField(credentialsHandleHandleOriginal, "dwUpper");

            return (securityContextHandle, credentialsHandleHandle);
        }

        private class NativeApiHelpers
        {
            public Func<(NativeApi.SSPIHandle securityContextHandle, NativeApi.SSPIHandle credentialsHandleHandle)> GetHandlesFunc
            {
                get;
                set;
            }

            public Func<Stream> GetInnerStreamFunc { get; set; }
        }
    }
}
