﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using winsvc.Enumerations;
using winsvc.Flags;
using winsvc.Structs;

namespace winsvc
{
    public sealed class ServiceControlManager : SafeHandleZeroOrMinusOneIsInvalid, IServiceControlManager
    {
        private ServiceControlManager(string machineName, SCM_ACCESS desiredAccess) : base(true)
        {
            handle = NativeMethods.OpenSCManager(machineName, null, (uint) desiredAccess);
            if (handle == IntPtr.Zero)
            {
                throw new Win32Exception();
            }
        }

        public static IServiceControlManager OpenServiceControlManager(string machineName, SCM_ACCESS desiredAccess)
        {
            return new ServiceControlManager(machineName, desiredAccess);
        }

        public IService OpenService(string serviceName, SERVICE_ACCESS desiredAccess)
        {
            var serviceHandle = NativeMethods.OpenService(handle, serviceName, (uint) desiredAccess);
            if (serviceHandle == IntPtr.Zero)
            {
                throw new Win32Exception();
            }

            return new Service(serviceHandle);
        }

        public IService CreateService(
            string serviceName, 
            string displayName, 
            SERVICE_ACCESS desiredAccess, 
            SERVICE_TYPE serviceType,
            SERVICE_START_TYPE startType,
            SERVICE_ERROR_CONTROL errorControl, 
            string binaryPathName, 
            string loadOrderGroup, 
            IntPtr tagId, 
            string dependencies,
            string serviceStartName, 
            string password)
        {
            var serviceHandle = NativeMethods.CreateService(handle, serviceName, displayName, (uint) desiredAccess, (uint) serviceType,
                (uint) startType, (uint) errorControl, binaryPathName, loadOrderGroup, tagId, dependencies, serviceStartName, password);
            if (serviceHandle == IntPtr.Zero)
            {
                throw new Win32Exception();
            }

            return new Service(serviceHandle);
        }

        public IEnumerable<ENUM_SERVICE_STATUS> EnumServicesStatus()
        {
            // ReSharper disable once InconsistentNaming
            const int ERROR_MORE_DATA = 234;

            int needed = 0;
            int servicesReturned = 0;
            uint resumeHandle = 0;

            if (NativeMethods.EnumServicesStatus(handle, SERVICE_TYPE.SERVICE_WIN32, SERVICE_STATE_FLAGS.SERVICE_STATE_ALL, IntPtr.Zero, 0, ref needed, ref servicesReturned, ref resumeHandle))
            {
                throw new ApplicationException("Unexpected success enumerating services with zero buffer");
            }

            // We expect an ERROR_MORE_DATA error as the buffer size passed in was zero, otherwise something strage is going on
            if (Marshal.GetLastWin32Error() != ERROR_MORE_DATA) 
            {
                throw new Win32Exception();
            }

            IntPtr bufferPtr = Marshal.AllocHGlobal(needed);
            var ptr = bufferPtr;
            try
            {
                if (!NativeMethods.EnumServicesStatus(handle, SERVICE_TYPE.SERVICE_WIN32,
                    SERVICE_STATE_FLAGS.SERVICE_STATE_ALL, bufferPtr, needed, ref needed, ref servicesReturned,
                    ref resumeHandle))
                {
                    throw new Win32Exception();
                }

                for (int i = 0; i < servicesReturned; i++)
                {
                    yield return (ENUM_SERVICE_STATUS) Marshal.PtrToStructure(ptr, typeof(ENUM_SERVICE_STATUS));
                    ptr += Marshal.SizeOf(typeof(ENUM_SERVICE_STATUS));
                }
            }
            finally
            {
                Marshal.FreeHGlobal(bufferPtr);
            }
        }

        protected override bool ReleaseHandle()
        {
            return NativeMethods.CloseServiceHandle(handle);
        }
    }
}