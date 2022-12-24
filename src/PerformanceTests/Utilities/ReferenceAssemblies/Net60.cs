// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Reference.Assemblies
{
    public static class Net60
    {
        public static PortableExecutableReference MicrosoftCSharp { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.MicrosoftCSharp).GetReference(filePath: "Microsoft.CSharp.dll", display: "Microsoft.CSharp (net60)");
        public static PortableExecutableReference MicrosoftVisualBasicCore { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.MicrosoftVisualBasicCore).GetReference(filePath: "Microsoft.VisualBasic.Core.dll", display: "Microsoft.VisualBasic.Core (net60)");
        public static PortableExecutableReference MicrosoftVisualBasic { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.MicrosoftVisualBasic).GetReference(filePath: "Microsoft.VisualBasic.dll", display: "Microsoft.VisualBasic (net60)");
        public static PortableExecutableReference MicrosoftWin32Primitives { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.MicrosoftWin32Primitives).GetReference(filePath: "Microsoft.Win32.Primitives.dll", display: "Microsoft.Win32.Primitives (net60)");
        public static PortableExecutableReference MicrosoftWin32Registry { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.MicrosoftWin32Registry).GetReference(filePath: "Microsoft.Win32.Registry.dll", display: "Microsoft.Win32.Registry (net60)");
        public static PortableExecutableReference mscorlib { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.mscorlib).GetReference(filePath: "mscorlib.dll", display: "mscorlib (net60)");
        public static PortableExecutableReference netstandard { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.netstandard).GetReference(filePath: "netstandard.dll", display: "netstandard (net60)");
        public static PortableExecutableReference SystemAppContext { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemAppContext).GetReference(filePath: "System.AppContext.dll", display: "System.AppContext (net60)");
        public static PortableExecutableReference SystemBuffers { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemBuffers).GetReference(filePath: "System.Buffers.dll", display: "System.Buffers (net60)");
        public static PortableExecutableReference SystemCollectionsConcurrent { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemCollectionsConcurrent).GetReference(filePath: "System.Collections.Concurrent.dll", display: "System.Collections.Concurrent (net60)");
        public static PortableExecutableReference SystemCollections { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemCollections).GetReference(filePath: "System.Collections.dll", display: "System.Collections (net60)");
        public static PortableExecutableReference SystemCollectionsImmutable { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemCollectionsImmutable).GetReference(filePath: "System.Collections.Immutable.dll", display: "System.Collections.Immutable (net60)");
        public static PortableExecutableReference SystemCollectionsNonGeneric { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemCollectionsNonGeneric).GetReference(filePath: "System.Collections.NonGeneric.dll", display: "System.Collections.NonGeneric (net60)");
        public static PortableExecutableReference SystemCollectionsSpecialized { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemCollectionsSpecialized).GetReference(filePath: "System.Collections.Specialized.dll", display: "System.Collections.Specialized (net60)");
        public static PortableExecutableReference SystemComponentModelAnnotations { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemComponentModelAnnotations).GetReference(filePath: "System.ComponentModel.Annotations.dll", display: "System.ComponentModel.Annotations (net60)");
        public static PortableExecutableReference SystemComponentModelDataAnnotations { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemComponentModelDataAnnotations).GetReference(filePath: "System.ComponentModel.DataAnnotations.dll", display: "System.ComponentModel.DataAnnotations (net60)");
        public static PortableExecutableReference SystemComponentModel { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemComponentModel).GetReference(filePath: "System.ComponentModel.dll", display: "System.ComponentModel (net60)");
        public static PortableExecutableReference SystemComponentModelEventBasedAsync { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemComponentModelEventBasedAsync).GetReference(filePath: "System.ComponentModel.EventBasedAsync.dll", display: "System.ComponentModel.EventBasedAsync (net60)");
        public static PortableExecutableReference SystemComponentModelPrimitives { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemComponentModelPrimitives).GetReference(filePath: "System.ComponentModel.Primitives.dll", display: "System.ComponentModel.Primitives (net60)");
        public static PortableExecutableReference SystemComponentModelTypeConverter { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemComponentModelTypeConverter).GetReference(filePath: "System.ComponentModel.TypeConverter.dll", display: "System.ComponentModel.TypeConverter (net60)");
        public static PortableExecutableReference SystemConfiguration { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemConfiguration).GetReference(filePath: "System.Configuration.dll", display: "System.Configuration (net60)");
        public static PortableExecutableReference SystemConsole { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemConsole).GetReference(filePath: "System.Console.dll", display: "System.Console (net60)");
        public static PortableExecutableReference SystemCore { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemCore).GetReference(filePath: "System.Core.dll", display: "System.Core (net60)");
        public static PortableExecutableReference SystemDataCommon { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemDataCommon).GetReference(filePath: "System.Data.Common.dll", display: "System.Data.Common (net60)");
        public static PortableExecutableReference SystemDataDataSetExtensions { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemDataDataSetExtensions).GetReference(filePath: "System.Data.DataSetExtensions.dll", display: "System.Data.DataSetExtensions (net60)");
        public static PortableExecutableReference SystemData { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemData).GetReference(filePath: "System.Data.dll", display: "System.Data (net60)");
        public static PortableExecutableReference SystemDiagnosticsContracts { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemDiagnosticsContracts).GetReference(filePath: "System.Diagnostics.Contracts.dll", display: "System.Diagnostics.Contracts (net60)");
        public static PortableExecutableReference SystemDiagnosticsDebug { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemDiagnosticsDebug).GetReference(filePath: "System.Diagnostics.Debug.dll", display: "System.Diagnostics.Debug (net60)");
        public static PortableExecutableReference SystemDiagnosticsDiagnosticSource { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemDiagnosticsDiagnosticSource).GetReference(filePath: "System.Diagnostics.DiagnosticSource.dll", display: "System.Diagnostics.DiagnosticSource (net60)");
        public static PortableExecutableReference SystemDiagnosticsFileVersionInfo { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemDiagnosticsFileVersionInfo).GetReference(filePath: "System.Diagnostics.FileVersionInfo.dll", display: "System.Diagnostics.FileVersionInfo (net60)");
        public static PortableExecutableReference SystemDiagnosticsProcess { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemDiagnosticsProcess).GetReference(filePath: "System.Diagnostics.Process.dll", display: "System.Diagnostics.Process (net60)");
        public static PortableExecutableReference SystemDiagnosticsStackTrace { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemDiagnosticsStackTrace).GetReference(filePath: "System.Diagnostics.StackTrace.dll", display: "System.Diagnostics.StackTrace (net60)");
        public static PortableExecutableReference SystemDiagnosticsTextWriterTraceListener { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemDiagnosticsTextWriterTraceListener).GetReference(filePath: "System.Diagnostics.TextWriterTraceListener.dll", display: "System.Diagnostics.TextWriterTraceListener (net60)");
        public static PortableExecutableReference SystemDiagnosticsTools { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemDiagnosticsTools).GetReference(filePath: "System.Diagnostics.Tools.dll", display: "System.Diagnostics.Tools (net60)");
        public static PortableExecutableReference SystemDiagnosticsTraceSource { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemDiagnosticsTraceSource).GetReference(filePath: "System.Diagnostics.TraceSource.dll", display: "System.Diagnostics.TraceSource (net60)");
        public static PortableExecutableReference SystemDiagnosticsTracing { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemDiagnosticsTracing).GetReference(filePath: "System.Diagnostics.Tracing.dll", display: "System.Diagnostics.Tracing (net60)");
        public static PortableExecutableReference System { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.System).GetReference(filePath: "System.dll", display: "System (net60)");
        public static PortableExecutableReference SystemDrawing { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemDrawing).GetReference(filePath: "System.Drawing.dll", display: "System.Drawing (net60)");
        public static PortableExecutableReference SystemDrawingPrimitives { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemDrawingPrimitives).GetReference(filePath: "System.Drawing.Primitives.dll", display: "System.Drawing.Primitives (net60)");
        public static PortableExecutableReference SystemDynamicRuntime { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemDynamicRuntime).GetReference(filePath: "System.Dynamic.Runtime.dll", display: "System.Dynamic.Runtime (net60)");
        public static PortableExecutableReference SystemFormatsAsn1 { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemFormatsAsn1).GetReference(filePath: "System.Formats.Asn1.dll", display: "System.Formats.Asn1 (net60)");
        public static PortableExecutableReference SystemGlobalizationCalendars { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemGlobalizationCalendars).GetReference(filePath: "System.Globalization.Calendars.dll", display: "System.Globalization.Calendars (net60)");
        public static PortableExecutableReference SystemGlobalization { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemGlobalization).GetReference(filePath: "System.Globalization.dll", display: "System.Globalization (net60)");
        public static PortableExecutableReference SystemGlobalizationExtensions { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemGlobalizationExtensions).GetReference(filePath: "System.Globalization.Extensions.dll", display: "System.Globalization.Extensions (net60)");
        public static PortableExecutableReference SystemIOCompressionBrotli { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemIOCompressionBrotli).GetReference(filePath: "System.IO.Compression.Brotli.dll", display: "System.IO.Compression.Brotli (net60)");
        public static PortableExecutableReference SystemIOCompression { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemIOCompression).GetReference(filePath: "System.IO.Compression.dll", display: "System.IO.Compression (net60)");
        public static PortableExecutableReference SystemIOCompressionFileSystem { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemIOCompressionFileSystem).GetReference(filePath: "System.IO.Compression.FileSystem.dll", display: "System.IO.Compression.FileSystem (net60)");
        public static PortableExecutableReference SystemIOCompressionZipFile { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemIOCompressionZipFile).GetReference(filePath: "System.IO.Compression.ZipFile.dll", display: "System.IO.Compression.ZipFile (net60)");
        public static PortableExecutableReference SystemIO { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemIO).GetReference(filePath: "System.IO.dll", display: "System.IO (net60)");
        public static PortableExecutableReference SystemIOFileSystemAccessControl { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemIOFileSystemAccessControl).GetReference(filePath: "System.IO.FileSystem.AccessControl.dll", display: "System.IO.FileSystem.AccessControl (net60)");
        public static PortableExecutableReference SystemIOFileSystem { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemIOFileSystem).GetReference(filePath: "System.IO.FileSystem.dll", display: "System.IO.FileSystem (net60)");
        public static PortableExecutableReference SystemIOFileSystemDriveInfo { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemIOFileSystemDriveInfo).GetReference(filePath: "System.IO.FileSystem.DriveInfo.dll", display: "System.IO.FileSystem.DriveInfo (net60)");
        public static PortableExecutableReference SystemIOFileSystemPrimitives { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemIOFileSystemPrimitives).GetReference(filePath: "System.IO.FileSystem.Primitives.dll", display: "System.IO.FileSystem.Primitives (net60)");
        public static PortableExecutableReference SystemIOFileSystemWatcher { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemIOFileSystemWatcher).GetReference(filePath: "System.IO.FileSystem.Watcher.dll", display: "System.IO.FileSystem.Watcher (net60)");
        public static PortableExecutableReference SystemIOIsolatedStorage { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemIOIsolatedStorage).GetReference(filePath: "System.IO.IsolatedStorage.dll", display: "System.IO.IsolatedStorage (net60)");
        public static PortableExecutableReference SystemIOMemoryMappedFiles { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemIOMemoryMappedFiles).GetReference(filePath: "System.IO.MemoryMappedFiles.dll", display: "System.IO.MemoryMappedFiles (net60)");
        public static PortableExecutableReference SystemIOPipesAccessControl { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemIOPipesAccessControl).GetReference(filePath: "System.IO.Pipes.AccessControl.dll", display: "System.IO.Pipes.AccessControl (net60)");
        public static PortableExecutableReference SystemIOPipes { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemIOPipes).GetReference(filePath: "System.IO.Pipes.dll", display: "System.IO.Pipes (net60)");
        public static PortableExecutableReference SystemIOUnmanagedMemoryStream { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemIOUnmanagedMemoryStream).GetReference(filePath: "System.IO.UnmanagedMemoryStream.dll", display: "System.IO.UnmanagedMemoryStream (net60)");
        public static PortableExecutableReference SystemLinq { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemLinq).GetReference(filePath: "System.Linq.dll", display: "System.Linq (net60)");
        public static PortableExecutableReference SystemLinqExpressions { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemLinqExpressions).GetReference(filePath: "System.Linq.Expressions.dll", display: "System.Linq.Expressions (net60)");
        public static PortableExecutableReference SystemLinqParallel { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemLinqParallel).GetReference(filePath: "System.Linq.Parallel.dll", display: "System.Linq.Parallel (net60)");
        public static PortableExecutableReference SystemLinqQueryable { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemLinqQueryable).GetReference(filePath: "System.Linq.Queryable.dll", display: "System.Linq.Queryable (net60)");
        public static PortableExecutableReference SystemMemory { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemMemory).GetReference(filePath: "System.Memory.dll", display: "System.Memory (net60)");
        public static PortableExecutableReference SystemNet { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemNet).GetReference(filePath: "System.Net.dll", display: "System.Net (net60)");
        public static PortableExecutableReference SystemNetHttp { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemNetHttp).GetReference(filePath: "System.Net.Http.dll", display: "System.Net.Http (net60)");
        public static PortableExecutableReference SystemNetHttpJson { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemNetHttpJson).GetReference(filePath: "System.Net.Http.Json.dll", display: "System.Net.Http.Json (net60)");
        public static PortableExecutableReference SystemNetHttpListener { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemNetHttpListener).GetReference(filePath: "System.Net.HttpListener.dll", display: "System.Net.HttpListener (net60)");
        public static PortableExecutableReference SystemNetMail { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemNetMail).GetReference(filePath: "System.Net.Mail.dll", display: "System.Net.Mail (net60)");
        public static PortableExecutableReference SystemNetNameResolution { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemNetNameResolution).GetReference(filePath: "System.Net.NameResolution.dll", display: "System.Net.NameResolution (net60)");
        public static PortableExecutableReference SystemNetNetworkInformation { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemNetNetworkInformation).GetReference(filePath: "System.Net.NetworkInformation.dll", display: "System.Net.NetworkInformation (net60)");
        public static PortableExecutableReference SystemNetPing { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemNetPing).GetReference(filePath: "System.Net.Ping.dll", display: "System.Net.Ping (net60)");
        public static PortableExecutableReference SystemNetPrimitives { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemNetPrimitives).GetReference(filePath: "System.Net.Primitives.dll", display: "System.Net.Primitives (net60)");
        public static PortableExecutableReference SystemNetRequests { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemNetRequests).GetReference(filePath: "System.Net.Requests.dll", display: "System.Net.Requests (net60)");
        public static PortableExecutableReference SystemNetSecurity { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemNetSecurity).GetReference(filePath: "System.Net.Security.dll", display: "System.Net.Security (net60)");
        public static PortableExecutableReference SystemNetServicePoint { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemNetServicePoint).GetReference(filePath: "System.Net.ServicePoint.dll", display: "System.Net.ServicePoint (net60)");
        public static PortableExecutableReference SystemNetSockets { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemNetSockets).GetReference(filePath: "System.Net.Sockets.dll", display: "System.Net.Sockets (net60)");
        public static PortableExecutableReference SystemNetWebClient { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemNetWebClient).GetReference(filePath: "System.Net.WebClient.dll", display: "System.Net.WebClient (net60)");
        public static PortableExecutableReference SystemNetWebHeaderCollection { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemNetWebHeaderCollection).GetReference(filePath: "System.Net.WebHeaderCollection.dll", display: "System.Net.WebHeaderCollection (net60)");
        public static PortableExecutableReference SystemNetWebProxy { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemNetWebProxy).GetReference(filePath: "System.Net.WebProxy.dll", display: "System.Net.WebProxy (net60)");
        public static PortableExecutableReference SystemNetWebSocketsClient { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemNetWebSocketsClient).GetReference(filePath: "System.Net.WebSockets.Client.dll", display: "System.Net.WebSockets.Client (net60)");
        public static PortableExecutableReference SystemNetWebSockets { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemNetWebSockets).GetReference(filePath: "System.Net.WebSockets.dll", display: "System.Net.WebSockets (net60)");
        public static PortableExecutableReference SystemNumerics { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemNumerics).GetReference(filePath: "System.Numerics.dll", display: "System.Numerics (net60)");
        public static PortableExecutableReference SystemNumericsVectors { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemNumericsVectors).GetReference(filePath: "System.Numerics.Vectors.dll", display: "System.Numerics.Vectors (net60)");
        public static PortableExecutableReference SystemObjectModel { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemObjectModel).GetReference(filePath: "System.ObjectModel.dll", display: "System.ObjectModel (net60)");
        public static PortableExecutableReference SystemReflectionDispatchProxy { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemReflectionDispatchProxy).GetReference(filePath: "System.Reflection.DispatchProxy.dll", display: "System.Reflection.DispatchProxy (net60)");
        public static PortableExecutableReference SystemReflection { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemReflection).GetReference(filePath: "System.Reflection.dll", display: "System.Reflection (net60)");
        public static PortableExecutableReference SystemReflectionEmit { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemReflectionEmit).GetReference(filePath: "System.Reflection.Emit.dll", display: "System.Reflection.Emit (net60)");
        public static PortableExecutableReference SystemReflectionEmitILGeneration { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemReflectionEmitILGeneration).GetReference(filePath: "System.Reflection.Emit.ILGeneration.dll", display: "System.Reflection.Emit.ILGeneration (net60)");
        public static PortableExecutableReference SystemReflectionEmitLightweight { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemReflectionEmitLightweight).GetReference(filePath: "System.Reflection.Emit.Lightweight.dll", display: "System.Reflection.Emit.Lightweight (net60)");
        public static PortableExecutableReference SystemReflectionExtensions { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemReflectionExtensions).GetReference(filePath: "System.Reflection.Extensions.dll", display: "System.Reflection.Extensions (net60)");
        public static PortableExecutableReference SystemReflectionMetadata { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemReflectionMetadata).GetReference(filePath: "System.Reflection.Metadata.dll", display: "System.Reflection.Metadata (net60)");
        public static PortableExecutableReference SystemReflectionPrimitives { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemReflectionPrimitives).GetReference(filePath: "System.Reflection.Primitives.dll", display: "System.Reflection.Primitives (net60)");
        public static PortableExecutableReference SystemReflectionTypeExtensions { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemReflectionTypeExtensions).GetReference(filePath: "System.Reflection.TypeExtensions.dll", display: "System.Reflection.TypeExtensions (net60)");
        public static PortableExecutableReference SystemResourcesReader { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemResourcesReader).GetReference(filePath: "System.Resources.Reader.dll", display: "System.Resources.Reader (net60)");
        public static PortableExecutableReference SystemResourcesResourceManager { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemResourcesResourceManager).GetReference(filePath: "System.Resources.ResourceManager.dll", display: "System.Resources.ResourceManager (net60)");
        public static PortableExecutableReference SystemResourcesWriter { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemResourcesWriter).GetReference(filePath: "System.Resources.Writer.dll", display: "System.Resources.Writer (net60)");
        public static PortableExecutableReference SystemRuntimeCompilerServicesUnsafe { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemRuntimeCompilerServicesUnsafe).GetReference(filePath: "System.Runtime.CompilerServices.Unsafe.dll", display: "System.Runtime.CompilerServices.Unsafe (net60)");
        public static PortableExecutableReference SystemRuntimeCompilerServicesVisualC { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemRuntimeCompilerServicesVisualC).GetReference(filePath: "System.Runtime.CompilerServices.VisualC.dll", display: "System.Runtime.CompilerServices.VisualC (net60)");
        public static PortableExecutableReference SystemRuntime { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemRuntime).GetReference(filePath: "System.Runtime.dll", display: "System.Runtime (net60)");
        public static PortableExecutableReference SystemRuntimeExtensions { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemRuntimeExtensions).GetReference(filePath: "System.Runtime.Extensions.dll", display: "System.Runtime.Extensions (net60)");
        public static PortableExecutableReference SystemRuntimeHandles { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemRuntimeHandles).GetReference(filePath: "System.Runtime.Handles.dll", display: "System.Runtime.Handles (net60)");
        public static PortableExecutableReference SystemRuntimeInteropServices { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemRuntimeInteropServices).GetReference(filePath: "System.Runtime.InteropServices.dll", display: "System.Runtime.InteropServices (net60)");
        public static PortableExecutableReference SystemRuntimeInteropServicesRuntimeInformation { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemRuntimeInteropServicesRuntimeInformation).GetReference(filePath: "System.Runtime.InteropServices.RuntimeInformation.dll", display: "System.Runtime.InteropServices.RuntimeInformation (net60)");
        public static PortableExecutableReference SystemRuntimeIntrinsics { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemRuntimeIntrinsics).GetReference(filePath: "System.Runtime.Intrinsics.dll", display: "System.Runtime.Intrinsics (net60)");
        public static PortableExecutableReference SystemRuntimeLoader { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemRuntimeLoader).GetReference(filePath: "System.Runtime.Loader.dll", display: "System.Runtime.Loader (net60)");
        public static PortableExecutableReference SystemRuntimeNumerics { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemRuntimeNumerics).GetReference(filePath: "System.Runtime.Numerics.dll", display: "System.Runtime.Numerics (net60)");
        public static PortableExecutableReference SystemRuntimeSerialization { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemRuntimeSerialization).GetReference(filePath: "System.Runtime.Serialization.dll", display: "System.Runtime.Serialization (net60)");
        public static PortableExecutableReference SystemRuntimeSerializationFormatters { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemRuntimeSerializationFormatters).GetReference(filePath: "System.Runtime.Serialization.Formatters.dll", display: "System.Runtime.Serialization.Formatters (net60)");
        public static PortableExecutableReference SystemRuntimeSerializationJson { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemRuntimeSerializationJson).GetReference(filePath: "System.Runtime.Serialization.Json.dll", display: "System.Runtime.Serialization.Json (net60)");
        public static PortableExecutableReference SystemRuntimeSerializationPrimitives { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemRuntimeSerializationPrimitives).GetReference(filePath: "System.Runtime.Serialization.Primitives.dll", display: "System.Runtime.Serialization.Primitives (net60)");
        public static PortableExecutableReference SystemRuntimeSerializationXml { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemRuntimeSerializationXml).GetReference(filePath: "System.Runtime.Serialization.Xml.dll", display: "System.Runtime.Serialization.Xml (net60)");
        public static PortableExecutableReference SystemSecurityAccessControl { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemSecurityAccessControl).GetReference(filePath: "System.Security.AccessControl.dll", display: "System.Security.AccessControl (net60)");
        public static PortableExecutableReference SystemSecurityClaims { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemSecurityClaims).GetReference(filePath: "System.Security.Claims.dll", display: "System.Security.Claims (net60)");
        public static PortableExecutableReference SystemSecurityCryptographyAlgorithms { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemSecurityCryptographyAlgorithms).GetReference(filePath: "System.Security.Cryptography.Algorithms.dll", display: "System.Security.Cryptography.Algorithms (net60)");
        public static PortableExecutableReference SystemSecurityCryptographyCng { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemSecurityCryptographyCng).GetReference(filePath: "System.Security.Cryptography.Cng.dll", display: "System.Security.Cryptography.Cng (net60)");
        public static PortableExecutableReference SystemSecurityCryptographyCsp { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemSecurityCryptographyCsp).GetReference(filePath: "System.Security.Cryptography.Csp.dll", display: "System.Security.Cryptography.Csp (net60)");
        public static PortableExecutableReference SystemSecurityCryptographyEncoding { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemSecurityCryptographyEncoding).GetReference(filePath: "System.Security.Cryptography.Encoding.dll", display: "System.Security.Cryptography.Encoding (net60)");
        public static PortableExecutableReference SystemSecurityCryptographyOpenSsl { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemSecurityCryptographyOpenSsl).GetReference(filePath: "System.Security.Cryptography.OpenSsl.dll", display: "System.Security.Cryptography.OpenSsl (net60)");
        public static PortableExecutableReference SystemSecurityCryptographyPrimitives { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemSecurityCryptographyPrimitives).GetReference(filePath: "System.Security.Cryptography.Primitives.dll", display: "System.Security.Cryptography.Primitives (net60)");
        public static PortableExecutableReference SystemSecurityCryptographyX509Certificates { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemSecurityCryptographyX509Certificates).GetReference(filePath: "System.Security.Cryptography.X509Certificates.dll", display: "System.Security.Cryptography.X509Certificates (net60)");
        public static PortableExecutableReference SystemSecurity { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemSecurity).GetReference(filePath: "System.Security.dll", display: "System.Security (net60)");
        public static PortableExecutableReference SystemSecurityPrincipal { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemSecurityPrincipal).GetReference(filePath: "System.Security.Principal.dll", display: "System.Security.Principal (net60)");
        public static PortableExecutableReference SystemSecurityPrincipalWindows { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemSecurityPrincipalWindows).GetReference(filePath: "System.Security.Principal.Windows.dll", display: "System.Security.Principal.Windows (net60)");
        public static PortableExecutableReference SystemSecuritySecureString { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemSecuritySecureString).GetReference(filePath: "System.Security.SecureString.dll", display: "System.Security.SecureString (net60)");
        public static PortableExecutableReference SystemServiceModelWeb { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemServiceModelWeb).GetReference(filePath: "System.ServiceModel.Web.dll", display: "System.ServiceModel.Web (net60)");
        public static PortableExecutableReference SystemServiceProcess { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemServiceProcess).GetReference(filePath: "System.ServiceProcess.dll", display: "System.ServiceProcess (net60)");
        public static PortableExecutableReference SystemTextEncodingCodePages { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemTextEncodingCodePages).GetReference(filePath: "System.Text.Encoding.CodePages.dll", display: "System.Text.Encoding.CodePages (net60)");
        public static PortableExecutableReference SystemTextEncoding { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemTextEncoding).GetReference(filePath: "System.Text.Encoding.dll", display: "System.Text.Encoding (net60)");
        public static PortableExecutableReference SystemTextEncodingExtensions { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemTextEncodingExtensions).GetReference(filePath: "System.Text.Encoding.Extensions.dll", display: "System.Text.Encoding.Extensions (net60)");
        public static PortableExecutableReference SystemTextEncodingsWeb { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemTextEncodingsWeb).GetReference(filePath: "System.Text.Encodings.Web.dll", display: "System.Text.Encodings.Web (net60)");
        public static PortableExecutableReference SystemTextJson { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemTextJson).GetReference(filePath: "System.Text.Json.dll", display: "System.Text.Json (net60)");
        public static PortableExecutableReference SystemTextRegularExpressions { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemTextRegularExpressions).GetReference(filePath: "System.Text.RegularExpressions.dll", display: "System.Text.RegularExpressions (net60)");
        public static PortableExecutableReference SystemThreadingChannels { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemThreadingChannels).GetReference(filePath: "System.Threading.Channels.dll", display: "System.Threading.Channels (net60)");
        public static PortableExecutableReference SystemThreading { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemThreading).GetReference(filePath: "System.Threading.dll", display: "System.Threading (net60)");
        public static PortableExecutableReference SystemThreadingOverlapped { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemThreadingOverlapped).GetReference(filePath: "System.Threading.Overlapped.dll", display: "System.Threading.Overlapped (net60)");
        public static PortableExecutableReference SystemThreadingTasksDataflow { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemThreadingTasksDataflow).GetReference(filePath: "System.Threading.Tasks.Dataflow.dll", display: "System.Threading.Tasks.Dataflow (net60)");
        public static PortableExecutableReference SystemThreadingTasks { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemThreadingTasks).GetReference(filePath: "System.Threading.Tasks.dll", display: "System.Threading.Tasks (net60)");
        public static PortableExecutableReference SystemThreadingTasksExtensions { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemThreadingTasksExtensions).GetReference(filePath: "System.Threading.Tasks.Extensions.dll", display: "System.Threading.Tasks.Extensions (net60)");
        public static PortableExecutableReference SystemThreadingTasksParallel { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemThreadingTasksParallel).GetReference(filePath: "System.Threading.Tasks.Parallel.dll", display: "System.Threading.Tasks.Parallel (net60)");
        public static PortableExecutableReference SystemThreadingThread { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemThreadingThread).GetReference(filePath: "System.Threading.Thread.dll", display: "System.Threading.Thread (net60)");
        public static PortableExecutableReference SystemThreadingThreadPool { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemThreadingThreadPool).GetReference(filePath: "System.Threading.ThreadPool.dll", display: "System.Threading.ThreadPool (net60)");
        public static PortableExecutableReference SystemThreadingTimer { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemThreadingTimer).GetReference(filePath: "System.Threading.Timer.dll", display: "System.Threading.Timer (net60)");
        public static PortableExecutableReference SystemTransactions { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemTransactions).GetReference(filePath: "System.Transactions.dll", display: "System.Transactions (net60)");
        public static PortableExecutableReference SystemTransactionsLocal { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemTransactionsLocal).GetReference(filePath: "System.Transactions.Local.dll", display: "System.Transactions.Local (net60)");
        public static PortableExecutableReference SystemValueTuple { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemValueTuple).GetReference(filePath: "System.ValueTuple.dll", display: "System.ValueTuple (net60)");
        public static PortableExecutableReference SystemWeb { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemWeb).GetReference(filePath: "System.Web.dll", display: "System.Web (net60)");
        public static PortableExecutableReference SystemWebHttpUtility { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemWebHttpUtility).GetReference(filePath: "System.Web.HttpUtility.dll", display: "System.Web.HttpUtility (net60)");
        public static PortableExecutableReference SystemWindows { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemWindows).GetReference(filePath: "System.Windows.dll", display: "System.Windows (net60)");
        public static PortableExecutableReference SystemXml { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemXml).GetReference(filePath: "System.Xml.dll", display: "System.Xml (net60)");
        public static PortableExecutableReference SystemXmlLinq { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemXmlLinq).GetReference(filePath: "System.Xml.Linq.dll", display: "System.Xml.Linq (net60)");
        public static PortableExecutableReference SystemXmlReaderWriter { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemXmlReaderWriter).GetReference(filePath: "System.Xml.ReaderWriter.dll", display: "System.Xml.ReaderWriter (net60)");
        public static PortableExecutableReference SystemXmlSerialization { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemXmlSerialization).GetReference(filePath: "System.Xml.Serialization.dll", display: "System.Xml.Serialization (net60)");
        public static PortableExecutableReference SystemXmlXDocument { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemXmlXDocument).GetReference(filePath: "System.Xml.XDocument.dll", display: "System.Xml.XDocument (net60)");
        public static PortableExecutableReference SystemXmlXmlDocument { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemXmlXmlDocument).GetReference(filePath: "System.Xml.XmlDocument.dll", display: "System.Xml.XmlDocument (net60)");
        public static PortableExecutableReference SystemXmlXmlSerializer { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemXmlXmlSerializer).GetReference(filePath: "System.Xml.XmlSerializer.dll", display: "System.Xml.XmlSerializer (net60)");
        public static PortableExecutableReference SystemXmlXPath { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemXmlXPath).GetReference(filePath: "System.Xml.XPath.dll", display: "System.Xml.XPath (net60)");
        public static PortableExecutableReference SystemXmlXPathXDocument { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.SystemXmlXPathXDocument).GetReference(filePath: "System.Xml.XPath.XDocument.dll", display: "System.Xml.XPath.XDocument (net60)");
        public static PortableExecutableReference WindowsBase { get; } = AssemblyMetadata.CreateFromImage(Net60Resources.WindowsBase).GetReference(filePath: "WindowsBase.dll", display: "WindowsBase (net60)");
        public static IEnumerable<PortableExecutableReference> All { get; } = new PortableExecutableReference[]
        {
            MicrosoftCSharp,
            MicrosoftVisualBasicCore,
            MicrosoftVisualBasic,
            MicrosoftWin32Primitives,
            MicrosoftWin32Registry,
            mscorlib,
            netstandard,
            SystemAppContext,
            SystemBuffers,
            SystemCollectionsConcurrent,
            SystemCollections,
            SystemCollectionsImmutable,
            SystemCollectionsNonGeneric,
            SystemCollectionsSpecialized,
            SystemComponentModelAnnotations,
            SystemComponentModelDataAnnotations,
            SystemComponentModel,
            SystemComponentModelEventBasedAsync,
            SystemComponentModelPrimitives,
            SystemComponentModelTypeConverter,
            SystemConfiguration,
            SystemConsole,
            SystemCore,
            SystemDataCommon,
            SystemDataDataSetExtensions,
            SystemData,
            SystemDiagnosticsContracts,
            SystemDiagnosticsDebug,
            SystemDiagnosticsDiagnosticSource,
            SystemDiagnosticsFileVersionInfo,
            SystemDiagnosticsProcess,
            SystemDiagnosticsStackTrace,
            SystemDiagnosticsTextWriterTraceListener,
            SystemDiagnosticsTools,
            SystemDiagnosticsTraceSource,
            SystemDiagnosticsTracing,
            System,
            SystemDrawing,
            SystemDrawingPrimitives,
            SystemDynamicRuntime,
            SystemFormatsAsn1,
            SystemGlobalizationCalendars,
            SystemGlobalization,
            SystemGlobalizationExtensions,
            SystemIOCompressionBrotli,
            SystemIOCompression,
            SystemIOCompressionFileSystem,
            SystemIOCompressionZipFile,
            SystemIO,
            SystemIOFileSystemAccessControl,
            SystemIOFileSystem,
            SystemIOFileSystemDriveInfo,
            SystemIOFileSystemPrimitives,
            SystemIOFileSystemWatcher,
            SystemIOIsolatedStorage,
            SystemIOMemoryMappedFiles,
            SystemIOPipesAccessControl,
            SystemIOPipes,
            SystemIOUnmanagedMemoryStream,
            SystemLinq,
            SystemLinqExpressions,
            SystemLinqParallel,
            SystemLinqQueryable,
            SystemMemory,
            SystemNet,
            SystemNetHttp,
            SystemNetHttpJson,
            SystemNetHttpListener,
            SystemNetMail,
            SystemNetNameResolution,
            SystemNetNetworkInformation,
            SystemNetPing,
            SystemNetPrimitives,
            SystemNetRequests,
            SystemNetSecurity,
            SystemNetServicePoint,
            SystemNetSockets,
            SystemNetWebClient,
            SystemNetWebHeaderCollection,
            SystemNetWebProxy,
            SystemNetWebSocketsClient,
            SystemNetWebSockets,
            SystemNumerics,
            SystemNumericsVectors,
            SystemObjectModel,
            SystemReflectionDispatchProxy,
            SystemReflection,
            SystemReflectionEmit,
            SystemReflectionEmitILGeneration,
            SystemReflectionEmitLightweight,
            SystemReflectionExtensions,
            SystemReflectionMetadata,
            SystemReflectionPrimitives,
            SystemReflectionTypeExtensions,
            SystemResourcesReader,
            SystemResourcesResourceManager,
            SystemResourcesWriter,
            SystemRuntimeCompilerServicesUnsafe,
            SystemRuntimeCompilerServicesVisualC,
            SystemRuntime,
            SystemRuntimeExtensions,
            SystemRuntimeHandles,
            SystemRuntimeInteropServices,
            SystemRuntimeInteropServicesRuntimeInformation,
            SystemRuntimeIntrinsics,
            SystemRuntimeLoader,
            SystemRuntimeNumerics,
            SystemRuntimeSerialization,
            SystemRuntimeSerializationFormatters,
            SystemRuntimeSerializationJson,
            SystemRuntimeSerializationPrimitives,
            SystemRuntimeSerializationXml,
            SystemSecurityAccessControl,
            SystemSecurityClaims,
            SystemSecurityCryptographyAlgorithms,
            SystemSecurityCryptographyCng,
            SystemSecurityCryptographyCsp,
            SystemSecurityCryptographyEncoding,
            SystemSecurityCryptographyOpenSsl,
            SystemSecurityCryptographyPrimitives,
            SystemSecurityCryptographyX509Certificates,
            SystemSecurity,
            SystemSecurityPrincipal,
            SystemSecurityPrincipalWindows,
            SystemSecuritySecureString,
            SystemServiceModelWeb,
            SystemServiceProcess,
            SystemTextEncodingCodePages,
            SystemTextEncoding,
            SystemTextEncodingExtensions,
            SystemTextEncodingsWeb,
            SystemTextJson,
            SystemTextRegularExpressions,
            SystemThreadingChannels,
            SystemThreading,
            SystemThreadingOverlapped,
            SystemThreadingTasksDataflow,
            SystemThreadingTasks,
            SystemThreadingTasksExtensions,
            SystemThreadingTasksParallel,
            SystemThreadingThread,
            SystemThreadingThreadPool,
            SystemThreadingTimer,
            SystemTransactions,
            SystemTransactionsLocal,
            SystemValueTuple,
            SystemWeb,
            SystemWebHttpUtility,
            SystemWindows,
            SystemXml,
            SystemXmlLinq,
            SystemXmlReaderWriter,
            SystemXmlSerialization,
            SystemXmlXDocument,
            SystemXmlXmlDocument,
            SystemXmlXmlSerializer,
            SystemXmlXPath,
            SystemXmlXPathXDocument,
            WindowsBase,
        };
    }
}
