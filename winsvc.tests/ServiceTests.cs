using System;
using System.Linq;
using frogmore.winsvc.dummy_service;
using frogmore.winsvc.Enumerations;
using frogmore.winsvc.Flags;
using frogmore.winsvc.Structs;
using NUnit.Framework;

namespace frogmore.winsvc.tests
{
    [TestFixture]
    public class ServiceTests
    {
        [SetUp]
        public void Setup()
        {
            CleanUp.DeleteDummyServiceIfItExists();
        }

        [TearDown]
        public void TearDown()
        {
            CleanUp.DeleteDummyServiceIfItExists();
        }

        [Test]
        public void DeleteService()
        {
            using (var scm = ServiceControlManager.OpenServiceControlManager(null, SCM_ACCESS.SC_MANAGER_CREATE_SERVICE))
            using (var service = ServiceControlManagerTests.CreateDummyService(scm))
            {
                service.Delete();
            }
        }

        [Test]
        public void Start()
        {
            using (var scm = ServiceControlManager.OpenServiceControlManager(null, SCM_ACCESS.SC_MANAGER_CREATE_SERVICE))
            using (var service = ServiceControlManagerTests.CreateDummyService(scm))
            {
                service.Start();
                service.WaitForServiceToStart();

                service.StopServiceAndWait();
            }
        }

        [Test]
        public void StartWithParameters()
        {
            using (var scm = ServiceControlManager.OpenServiceControlManager(null, SCM_ACCESS.SC_MANAGER_CREATE_SERVICE))
            using (var service = ServiceControlManagerTests.CreateDummyService(scm))
            {
                service.Start(new[] {"Dummy Parameter"});
                service.WaitForServiceToStart();

                service.StopServiceAndWait();
            }
        }

        [Test]
        public void Control()
        {
            using (var scm = ServiceControlManager.OpenServiceControlManager(null, SCM_ACCESS.SC_MANAGER_CREATE_SERVICE))
            using (var service = ServiceControlManagerTests.CreateDummyService(scm))
            {
                service.Start();
                service.WaitForServiceToStart();

                service.Control(SERVICE_CONTROL.SERVICE_CONTROL_PAUSE);
                service.WaitForServiceStatus(SERVICE_STATE.SERVICE_PAUSED);

                service.Control(SERVICE_CONTROL.SERVICE_CONTROL_CONTINUE);
                service.WaitForServiceStatus(SERVICE_STATE.SERVICE_RUNNING);


                service.StopServiceAndWait();
            }
        }


        [Test]
        public void ControlEx()
        {
            using (var scm = ServiceControlManager.OpenServiceControlManager(null, SCM_ACCESS.SC_MANAGER_CREATE_SERVICE))
            using (var service = ServiceControlManagerTests.CreateDummyService(scm))
            {
                service.Start();
                service.WaitForServiceToStart();

                var parameters = new SERVICE_CONTROL_STATUS_REASON_PARAMS();
                service.ControlEx(SERVICE_CONTROL.SERVICE_CONTROL_PAUSE, ref parameters);
                Assert.That(parameters.serviceStatus.currentState, Is.Not.EqualTo(SERVICE_STATE.SERVICE_RUNNING));
                service.WaitForServiceStatus(SERVICE_STATE.SERVICE_PAUSED);

                service.ControlEx(SERVICE_CONTROL.SERVICE_CONTROL_CONTINUE, ref parameters);
                service.WaitForServiceStatus(SERVICE_STATE.SERVICE_RUNNING);


                service.StopServiceAndWait();
            }
        }


        [Test]
        public void QueryStatus()
        {
            using (var scm = ServiceControlManager.OpenServiceControlManager(null, SCM_ACCESS.SC_MANAGER_CREATE_SERVICE))
            using (var service = ServiceControlManagerTests.CreateDummyService(scm))
            {
                Assert.That(service.QueryStatus().dwCurrentState, Is.EqualTo(SERVICE_STATE.SERVICE_STOPPED));

                service.Start();

                service.WaitForServiceToStart();

                Assert.That(service.QueryStatus().dwCurrentState, Is.EqualTo(SERVICE_STATE.SERVICE_RUNNING));

                service.StopServiceAndWait();
            }
        }

        [Test]
        public void QueryConfig()
        {
            using (var scm = ServiceControlManager.OpenServiceControlManager(null, SCM_ACCESS.SC_MANAGER_CREATE_SERVICE))
            using (var service = ServiceControlManagerTests.CreateDummyService(scm))
            {
                var config = service.QueryConfig();

                Assert.That(config.DisplayName, Is.EqualTo(DummyService.DisplayName));

                // Service is cleaned up in TearDown
            }
        }

        [Test]
        public void QueryStatusEx()
        {
            using (var scm = ServiceControlManager.OpenServiceControlManager(null, SCM_ACCESS.SC_MANAGER_CREATE_SERVICE))
            using (var service = ServiceControlManagerTests.CreateDummyService(scm))
            {
                var status = service.QueryStatusEx();
                Assert.That(status.currentState, Is.EqualTo(SERVICE_STATE.SERVICE_STOPPED));

                service.Start();
                service.WaitForServiceToStart();
                Assert.That(service.QueryStatusEx().currentState, Is.EqualTo(SERVICE_STATE.SERVICE_RUNNING));

                service.StopServiceAndWait();
            }
        }

        [Test]
        public void ChangeConfig()
        {
            using (var scm = ServiceControlManager.OpenServiceControlManager(null, SCM_ACCESS.SC_MANAGER_CREATE_SERVICE))
            using (var service = ServiceControlManagerTests.CreateDummyService(scm))
            {
                // No changes should not throw
                service.ChangeConfig(
                    SERVICE_TYPE.SERVICE_NO_CHANGE, 
                    SERVICE_START_TYPE.SERVICE_NO_CHANGE,
                    SERVICE_ERROR_CONTROL.SERVICE_NO_CHANGE,
                    null, 
                    null,
                    IntPtr.Zero, 
                    null, 
                    null,
                    null,
                    null);

                // Set service type to share process
                service.ChangeConfig(
                    SERVICE_TYPE.SERVICE_WIN32_SHARE_PROCESS, 
                    SERVICE_START_TYPE.SERVICE_NO_CHANGE,
                    SERVICE_ERROR_CONTROL.SERVICE_NO_CHANGE,
                    null, 
                    null,
                    IntPtr.Zero, 
                    null, 
                    null,
                    null,
                    null);
                Assert.That(service.QueryConfig().ServiceType, Is.EqualTo((uint) SERVICE_TYPE.SERVICE_WIN32_SHARE_PROCESS));
                
                // Set start type to disabled
                service.ChangeConfig(
                    SERVICE_TYPE.SERVICE_NO_CHANGE, 
                    SERVICE_START_TYPE.SERVICE_DISABLED,
                    SERVICE_ERROR_CONTROL.SERVICE_NO_CHANGE,
                    null, 
                    null,
                    IntPtr.Zero, 
                    null, 
                    null,
                    null,
                    null);
                Assert.That(service.QueryConfig().StartType, Is.EqualTo((uint) SERVICE_START_TYPE.SERVICE_DISABLED));

                // Set error control to critical
                service.ChangeConfig(
                    SERVICE_TYPE.SERVICE_NO_CHANGE,
                    SERVICE_START_TYPE.SERVICE_NO_CHANGE,
                    SERVICE_ERROR_CONTROL.SERVICE_ERROR_CRITICAL,
                    null,
                    null,
                    IntPtr.Zero,
                    null,
                    null,
                    null,
                    null);
                Assert.That(service.QueryConfig().ErrorControl,
                    Is.EqualTo((uint) SERVICE_ERROR_CONTROL.SERVICE_ERROR_CRITICAL));

                service.ChangeConfig(
                    SERVICE_TYPE.SERVICE_NO_CHANGE,
                    SERVICE_START_TYPE.SERVICE_NO_CHANGE,
                    SERVICE_ERROR_CONTROL.SERVICE_NO_CHANGE,
                    null,
                    null,
                    IntPtr.Zero,
                    null,
                    null,
                    null,
                    "New Display Name");
                Assert.That(service.QueryConfig().DisplayName, Is.EqualTo("New Display Name"));
            }
        }

        [Test]
        public void ChangeConfig2()
        {
            using (var scm = ServiceControlManager.OpenServiceControlManager(null, SCM_ACCESS.SC_MANAGER_CREATE_SERVICE | SCM_ACCESS.SC_MANAGER_ENUMERATE_SERVICE))
            using (var service = ServiceControlManagerTests.CreateDummyService(scm))
            {
                var description = new SERVICE_DESCRIPTION
                {
                    Description = "A dummy service"
                };

                service.ChangeConfig2(ref description);
                Assert.That(service.QueryConfig2<SERVICE_DESCRIPTION>().Description, Is.EqualTo("A dummy service"));
            }
        }

        [Test]
        public void ChangeConfigDependentServices()
        {
            using (var scm = ServiceControlManager.OpenServiceControlManager(null, SCM_ACCESS.SC_MANAGER_CREATE_SERVICE | SCM_ACCESS.SC_MANAGER_ENUMERATE_SERVICE))
            using (var service = ServiceControlManagerTests.CreateDummyService(scm))
            {
                // Just pick the first service to be dependency
                var dependentServiceNames = scm.EnumServicesStatus(SERVICE_TYPE.SERVICE_WIN32, SERVICE_STATE_FLAGS.SERVICE_STATE_ALL).Select(ss => ss.ServiceName).Take(1).ToList();
                service.ChangeConfig(SERVICE_TYPE.SERVICE_NO_CHANGE, SERVICE_START_TYPE.SERVICE_NO_CHANGE, SERVICE_ERROR_CONTROL.SERVICE_NO_CHANGE, null, null, IntPtr.Zero, dependentServiceNames, null, null, null);

                using (var dependentService = scm.OpenService(dependentServiceNames.First(), SERVICE_ACCESS.SERVICE_ENUMERATE_DEPENDENTS))
                {
                    var serviceName = dependentService.EnumDependentServices(SERVICE_STATE_FLAGS.SERVICE_STATE_ALL).Select(ss => ss.ServiceName).First();

                    Assert.That(serviceName, Is.EqualTo(DummyService.SvcName));
                }
            }
        }


        [Test]
        public void EnumDependentServices()
        {
            using (var scm = ServiceControlManager.OpenServiceControlManager(null, SCM_ACCESS.SC_MANAGER_CREATE_SERVICE | SCM_ACCESS.SC_MANAGER_ENUMERATE_SERVICE))
            {
                // Just pick the first service to be dependency
                var dependentServiceNames = scm.EnumServicesStatus(SERVICE_TYPE.SERVICE_WIN32, SERVICE_STATE_FLAGS.SERVICE_STATE_ALL).Select(ss => ss.ServiceName).Take(1).ToList();

                var path = typeof(DummyService).Assembly.Location;

                using (scm.CreateService(
                    DummyService.DisplayName,
                    DummyService.DisplayName,
                    SERVICE_ACCESS.SERVICE_ALL_ACCESS,
                    SERVICE_TYPE.SERVICE_WIN32_OWN_PROCESS,
                    SERVICE_START_TYPE.SERVICE_AUTO_START,
                    SERVICE_ERROR_CONTROL.SERVICE_ERROR_NORMAL,
                    path,
                    "",
                    IntPtr.Zero,
                    dependentServiceNames,
                    null,
                    null))
                {
                }

                using (var service = scm.OpenService(dependentServiceNames.First(), SERVICE_ACCESS.SERVICE_ENUMERATE_DEPENDENTS))
                {
                    var serviceName = service.EnumDependentServices(SERVICE_STATE_FLAGS.SERVICE_STATE_ALL).Select(ss => ss.ServiceName).First();

                    Assert.That(serviceName, Is.EqualTo(DummyService.DisplayName));
                }

            }
        }


    }
}