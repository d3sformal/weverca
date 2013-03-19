/*

 Copyright (c) 2005-2006 Tomas Matousek, Ladislav Prosek, Vaclav Novak, Pavel Novak, Jan Benda and Martin Maly.
 Copyright (c) 2006-2012 DEVSENSE s.r.o.

 The use and distribution terms for this software are contained in the file named License.txt, 
 which can be found in the root of the Phalanger distribution. By using this software 
 in any fashion, you are agreeing to be bound by the terms of this license.
 
 You must not remove this notice from this software.

*/

using System.Reflection;
using System.Security;
using System.Runtime.CompilerServices;
using System;

[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("The Phalanger Project Team")]
[assembly: AssemblyProduct("Phalanger")]
[assembly: AssemblyCopyright("Copyright (c) 2004-2010 Tomas Matousek, Ladislav Prosek, Vaclav Novak, Pavel Novak, Jan Benda, Martin Maly, Tomas Petricek, Daniel Balas, Miloslav Beno, Jakub Misek")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
//[assembly: CLSCompliant(true)]

#if !SILVERLIGHT
[assembly: AssemblyTitle("Phalanger Core")]
[assembly: AssemblyDescription("Phalanger Core Functionality")]
//[assembly: AllowPartiallyTrustedCallers]
#else
[assembly: AssemblyTitle("Phalanger Core (Silverlight)")]
[assembly: AssemblyDescription("Phalanger Core Functionality (Silverlight)")]
#endif

[assembly: AssemblyVersion("3.0.0.0")]
[assembly: AssemblyFileVersion("3.0.0.0")]

//[assembly: InternalsVisibleTo("PhpNetClasslibrary, PublicKey=0024000004800000940000000602000000240000525341310004000001000100611b1c313d77d51b5ac4d5b309e8712919634a716ae826dd133e722fe5e4f10012a8b96c40b7098d669ac5f78581b83cfa412d1a436a65450fac212d0d2dca824f8b1ab51b98af6d44d14ffd9a7aacd21e23557971564886df047070ca34d51869f3eddfb343739ee014e1b117772885fbc0758232461c5db7c659ca98b981a9")]
[assembly: InternalsVisibleTo("Phalanger.LanguageService, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b74f6114dcb75b60485b38820a516d1592ba89587f8449feac300570596bddac07226721e06178a9d2f8fbf0887bde659421378186cf0bfa31908b8f1965cc2cebeba22c9b232fb6cf5183eb12588bbdd61f0df0b390352f8be981f950642fedb8ad7cb241808f233cecb8ebaa2eb45b657744e95200c51ec39b686c66ad2eb6")]
[assembly: InternalsVisibleTo("ControlFlowAnalysis.Tests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100ab8cc8d483a0c3d0f88b85c856852450e58b6f1fb54b921bf2106138aae1f2065ec3751e864398506d8efce19abe7e0b718e333e5792dcd7ca7c085382a8bcf81579f875935d9e535efcd6cfc1408b77d5bf08da0b0662c6cffa97227194b34730b1c80b50d861b1b22411aecfacfc29c69540b95016bc87d2bcd9a416a299e8")]
[assembly: InternalsVisibleTo("Tests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c39956dd44004c91a570db8effcce5e08f55e9578acc6277d839d2ae30b5275223a582fe50273c98d301391df51ec628b9187a55cf7bf1919436910cc2dc3373e3a9a887a1f4f26315a6b9b7cdda1e54eb690a5818759ba28ebdfb939139cd469395b31a3d4734fbbf2d147fe11a3487797feba6f5607b2f002403c7c61d91a8")]