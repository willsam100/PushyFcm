namespace Pushy.Droid
open System.Reflection
open System.Runtime.CompilerServices

// the name of the type here needs to match the name inside the ResourceDesigner attribute
type Resources = Pushy.Droid.Resource
[<assembly: Android.Runtime.ResourceDesigner("Pushy.Droid.Resources", IsApplication=true)>]

[<assembly: AssemblyTitle("Pushy.Droid")>]
[<assembly: AssemblyDescription("")>]
[<assembly: AssemblyConfiguration("")>]
[<assembly: AssemblyCompany("willsam100")>]
[<assembly: AssemblyProduct("")>]
[<assembly: AssemblyCopyright("")>]
[<assembly: AssemblyTrademark("")>]

// The assembly version has the format {Major}.{Minor}.{Build}.{Revision}

[<assembly: AssemblyVersion("1.0.0.0")>]

//[<assembly: AssemblyDelaySign(false)>]
//[<assembly: AssemblyKeyFile("")>]

()
