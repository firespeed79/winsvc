﻿using System;
using System.Runtime.InteropServices;
using winsvc.AccessMasks;

namespace winsvc
{
    // Conventions

    // All declarations are Unicode
    // BOOL are marshalled as bool
    // HANDLEs are marshalled as UIntPtr
    // DWORDs are marshalled as Int32

    internal static class NativeMethods
    {
        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr OpenSCManager(string machineName, string databaseName, UInt32 desiredAccess);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool CloseServiceHandle(IntPtr serviceControlObject);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr OpenService(IntPtr serviceControlObject, string serviceName, UInt32 desiredAccess);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr CreateService(
            IntPtr serviceConrolObject,
            string serviceName,
            string displayName,
            UInt32 desiredAccess,
            UInt32 serviceType,
            UInt32 startType,
            UInt32 errorControl,
            string binaryPathName,
            string loadOrderGroup,
            IntPtr tagId,
            string dependencies,
            string serviceStartName,
            string password);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool DeleteService(IntPtr serviceHandle);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool StartService(IntPtr serviceHandle, int argCount, string[] argVectors);

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool ControlService(IntPtr serviceHandle, SERVICE_CONTROL control, ref ServiceStatus serviceStatus);
    }
}