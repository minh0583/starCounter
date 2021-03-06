﻿using System.Reflection;
using System.Runtime.InteropServices;
using Starcounter.Internal;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("UserAdmin")]
[assembly: AssemblyDescription("Sample user administration. Create **system users** and **system user groups**. Change passwords.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("StarcounterPrefabs")]
[assembly: AssemblyProduct("UserAdmin")]
[assembly: AssemblyCopyright("Copyright 2016 Starcounter")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("526edd06-b4f0-469f-a4d4-e95b3c94f5ac")]

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
[assembly: AssemblyVersion("5.0.1")]
[assembly: AssemblyFileVersion("5.0.1")]

// Starcounter Package information
//
// Unique warehouse id e.g. <myorganization.myapp>
[assembly: AssemblyMetadata("ID", "StarcounterSamples.UserAdmin")]

// Assures the current assembly has a reference to the Starcounter
// assembly. A reference to Starcounter is currently required for
// Starcounter to detect that an assembly should be hosted.
[assembly: StarcounterAssembly()]
