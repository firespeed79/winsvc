#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0
//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");


//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////


Task("Restore-NuGet-Packages")
    .Does(() =>
{
    NuGetRestore("./winsvc.sln");
});

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    MSBuild("./winsvc.sln", settings => 
    {
        settings.SetConfiguration("Debug");
        settings.Verbosity = Verbosity.Quiet; 
    });
    MSBuild("./winsvc.sln", settings => 
    {
        settings.SetConfiguration("Release");
        settings.Verbosity = Verbosity.Quiet; 
    });
});

Task("Run-Unit-Tests-x64")
    .IsDependentOn("Build")
    .Does(() =>
{
    NUnit3(@".\winsvc.tests\bin\Debug\winsvc.tests.dll", new NUnit3Settings {
        NoResults = true,
        X86 = false,
        });
    NUnit3(@".\winsvc.tests\bin\Release\winsvc.tests.dll", new NUnit3Settings {
        NoResults = true,
        X86 = false,
        });
});

Task("Run-Unit-Tests-x86")
    .IsDependentOn("Build")
    .Does(() =>
{
    NUnit3(@".\winsvc.tests\bin\Debug\winsvc.tests.dll", new NUnit3Settings {
        NoResults = true,
        X86 = true,
        });
    NUnit3(@".\winsvc.tests\bin\Release\winsvc.tests.dll", new NUnit3Settings {
        NoResults = true,
        X86 = true,
        });
});

Task("Run-Unit-Tests")
    .IsDependentOn("Run-Unit-Tests-x86")
    .IsDependentOn("Run-Unit-Tests-x64");

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Run-Unit-Tests");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
