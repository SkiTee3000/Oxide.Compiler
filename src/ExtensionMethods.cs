﻿using Microsoft.CodeAnalysis;
using ObjectStream.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Oxide.CompilerServices
{
    public static class ExtensionMethods
    {
        public static Serilog.Events.LogEventLevel ToSerilog(this LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Trace:
                    return Serilog.Events.LogEventLevel.Verbose;
                case LogLevel.Debug:
                    return Serilog.Events.LogEventLevel.Debug;
                case LogLevel.Information:
                    return Serilog.Events.LogEventLevel.Information;
                case LogLevel.Warning:
                    return Serilog.Events.LogEventLevel.Warning;
                case LogLevel.Error:
                    return Serilog.Events.LogEventLevel.Error;
                case LogLevel.Critical:
                    return Serilog.Events.LogEventLevel.Fatal;
                default:
                    return Serilog.Events.LogEventLevel.Information;
            }
        }

        public static Platform Platform(this CompilerData data)
        {
            return data.Platform switch
            {
                CompilerPlatform.AnyCPU32Preferred => Microsoft.CodeAnalysis.Platform.AnyCpu32BitPreferred,
                CompilerPlatform.Arm => Microsoft.CodeAnalysis.Platform.Arm,
                CompilerPlatform.X64 => Microsoft.CodeAnalysis.Platform.X64,
                CompilerPlatform.X86 => Microsoft.CodeAnalysis.Platform.X86,
                CompilerPlatform.IA64 => Microsoft.CodeAnalysis.Platform.Itanium,
                _ => Microsoft.CodeAnalysis.Platform.AnyCpu,
            };
        }

        public static OutputKind OutputKind(this CompilerData data)
        {
            return data.Target switch
            {
                CompilerTarget.Module => Microsoft.CodeAnalysis.OutputKind.NetModule,
                CompilerTarget.WinExe => Microsoft.CodeAnalysis.OutputKind.WindowsApplication,
                CompilerTarget.Exe => Microsoft.CodeAnalysis.OutputKind.ConsoleApplication,
                _ => Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary,
            };
        }

        public static Microsoft.CodeAnalysis.CSharp.LanguageVersion CSharpVersion(this CompilerData data)
        {
            return data.Version switch
            {
                CompilerLanguageVersion.Preview => Microsoft.CodeAnalysis.CSharp.LanguageVersion.Preview,
                CompilerLanguageVersion.V12 => Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp12,
                CompilerLanguageVersion.V11 => Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp11,
                CompilerLanguageVersion.V10 => Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp10,
                CompilerLanguageVersion.V9 => Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp9,
                CompilerLanguageVersion.V8 => Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp8,
                CompilerLanguageVersion.V7 => Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp7,
                CompilerLanguageVersion.V6 => Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp6,
                CompilerLanguageVersion.V5 => Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp5,
                CompilerLanguageVersion.V4 => Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp4,
                CompilerLanguageVersion.V3 => Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp3,
                CompilerLanguageVersion.V2 => Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp2,
                CompilerLanguageVersion.V1 => Microsoft.CodeAnalysis.CSharp.LanguageVersion.CSharp1,
                _ => Microsoft.CodeAnalysis.CSharp.LanguageVersion.Latest,
            };
        }

        public static PortableExecutableReference? ResolveToReference(this ICompilerService? compiler, string assemblyName)
        {
            if (string.IsNullOrWhiteSpace(assemblyName))
                throw new ArgumentNullException(nameof(assemblyName));

            string path = Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), assemblyName);
            if (File.Exists(path))
                return MetadataReference.CreateFromFile(path);

            string? env = Environment.GetEnvironmentVariable("OXIDE:Path:Libraries");

            if (!string.IsNullOrWhiteSpace(env))
            {
                path = Path.Combine(env, assemblyName);
                if (File.Exists(path))
                    return MetadataReference.CreateFromFile(path);
            }

            return null;
        }

        public static Task<PortableExecutableReference?> ResolveToReference(this ICompilerService? compiler, string assemblyName, CancellationToken token = default) => Task.Run(() => ResolveToReference(compiler, assemblyName), token);


        [UnconditionalSuppressMessage("SingleFile", "IL3000:Avoid accessing Assembly file path when publishing as a single file", Justification = "<Pending>")]
        public static PortableExecutableReference? ToReference(this Assembly assembly)
        {
            if (!string.IsNullOrWhiteSpace(assembly.Location))
                return MetadataReference.CreateFromFile(assembly.Location);

            AssemblyName name = assembly.GetName();

            if (name != null && !string.IsNullOrWhiteSpace(name.Name))
            {
                return ResolveToReference(null, name.Name + ".dll") ?? ResolveToReference(null, name.Name + ".exe");
            }

            return null;
        }

        public static PortableExecutableReference? ToReference(this Type type) => ToReference(type.Assembly);

        public static Task<PortableExecutableReference?> ToReference(this Type type, CancellationToken token = default) => Task.Run(() => ToReference(type), token);

        public static Task<PortableExecutableReference?> ToReference(this Assembly assembly, CancellationToken token = default) => Task.Run(() => ToReference(assembly), token);
    }
}
