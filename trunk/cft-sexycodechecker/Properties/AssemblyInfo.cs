﻿using System;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("cft-sexycodechecker")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("cft-sexycodechecker")]
[assembly: AssemblyCopyright("Copyright © Davide Inglima 2008")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("7533bc5b-9767-457b-9d95-ee51cdbcabbf")]

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
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

/*
    Category     : Microsoft.Design  (String)
    CheckId      : CA1014  (String)
    Resolution   : "Mark 'SexyCodeChecker.exe' with CLSCompliant(true) 
                   because it exposes externally visible types."
 */
[assembly: CLSCompliant(true)]

/*
    Category     : Microsoft.Performance  (String)
    CheckId      : CA1824  (String)
    Resolution   : "Because assembly 'SexyCodeChecker.exe' contains 
                   a ResX-based resource file, mark it with the NeutralResourcesLanguage 
                   attribute, specifying the language of the resources 
                   within the assembly. This could improve lookup performance 
                   the first time a resource is retrieved."
 */
[assembly: NeutralResourcesLanguageAttribute("en-US",UltimateResourceFallbackLocation.MainAssembly)]
