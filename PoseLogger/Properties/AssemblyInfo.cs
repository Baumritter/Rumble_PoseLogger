using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MelonLoader;
using PoseLogger; // The namespace of your mod class
// ...
[assembly: MelonInfo(typeof(PoseLoggerClass), PoseLogger.BuildInfo.ModName, PoseLogger.BuildInfo.ModVersion, PoseLogger.BuildInfo.Author)]
[assembly: MelonGame("Buckethead Entertainment", "RUMBLE")]
[assembly: VerifyLoaderVersion(0, 6, 2, true)]
[assembly: MelonColor(200, 0, 200, 0)]
[assembly: MelonAuthorColor(200, 0, 200, 0)]

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle(PoseLogger.BuildInfo.ModName)]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct(PoseLogger.BuildInfo.ModName)]
[assembly: AssemblyCopyright("Copyright ©  2024")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("7563b7ca-ebe1-428e-8669-99f62cbf56ab")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion(PoseLogger.BuildInfo.ModVersion)]
[assembly: AssemblyFileVersion(PoseLogger.BuildInfo.ModVersion)]
