﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.1433
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Cluefultoys.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Xml {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Xml() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Cluefultoys.Resources.Xml", typeof(Xml).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to http://schemas.microsoft.com/developer/msbuild/2003.
        /// </summary>
        internal static string MsBuildNamespace {
            get {
                return ResourceManager.GetString("MsBuildNamespace", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MsBuild.
        /// </summary>
        internal static string MsBuildPrefix {
            get {
                return ResourceManager.GetString("MsBuildPrefix", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to http://limacat.googlepages.com/cluefultoys/scc/extension/msbuild/parameters.xsd.
        /// </summary>
        internal static string SccBuildNamespace {
            get {
                return ResourceManager.GetString("SccBuildNamespace", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SccBuild.
        /// </summary>
        internal static string SccBuildPrefix {
            get {
                return ResourceManager.GetString("SccBuildPrefix", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to //MsBuild:Compile[contains(@Include,&apos;.cs&apos;) and (not(@SccBuild:Ignore) or @SccBuild:Ignore=&apos;false&apos;)]/@Include.
        /// </summary>
        internal static string XpathReadCompileTags {
            get {
                return ResourceManager.GetString("XpathReadCompileTags", resourceCulture);
            }
        }
    }
}