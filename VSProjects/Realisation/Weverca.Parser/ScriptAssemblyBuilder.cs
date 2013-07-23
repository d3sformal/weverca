using System.Collections.Generic;
using System.IO;
using System.Reflection;

using PHP.Core;
using PHP.Core.Emit;
using PHP.Core.Reflection;

namespace Weverca.Parsers
{
    /// <summary>
    /// Assembly builder of PHP script
    /// </summary>
    /// <remarks>
    /// <seealso cref="BasicScriptAssembly" />
    /// </remarks>
    internal sealed class BasicScriptAssemblyBuilder : ScriptAssemblyBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BasicScriptAssemblyBuilder" /> class.
        /// </summary>
        /// <param name="assembly">Assembly of PHP script</param>
        /// <param name="assemblyName">Unique identity name of the PHP assembly</param>
        /// <param name="module">Module of PHP script</param>
        internal BasicScriptAssemblyBuilder(
            BasicScriptAssembly/*!*/ assembly,
            AssemblyName assemblyName,
            Module/*!*/ module)
            : base(
            assembly,
            assemblyName,
            Path.GetDirectoryName(module.FullyQualifiedName),
            module.Name,
            AssemblyKinds.Library,
            new List<ResourceFileReference>(0),
            false,
            false,
            true,
            null)
        {
        }

        /// <summary>
        /// Gets assembly of PHP script
        /// </summary>
        /// <returns>Assembly of PHP script</returns>
        private BasicScriptAssembly GetAssembly()
        {
            var scriptAssembly = assembly as BasicScriptAssembly;
            Debug.Assert(
                scriptAssembly != null,
                "The builder can only be created with BasicScriptAssembly parameter");
            return scriptAssembly;
        }

        #region ScriptAssemblyBuilder overrides

        /// <summary>
        /// Gets builder within the assembly providing means for building PHP script
        /// </summary>
        /// <returns>Builder of PHP script</returns>
        protected override ScriptBuilder GetEntryScriptBuilder()
        {
            var scriptAssembly = GetAssembly();
            return scriptAssembly.Builder;
        }

        /// <summary>
        /// Defines a new script belonging to the assembly builder
        /// </summary>
        /// <param name="compilationUnit">PHP script compilation unit</param>
        /// <returns>New module of PHP script</returns>
        public override IPhpModuleBuilder DefineModule(CompilationUnitBase/*!*/ compilationUnit)
        {
            var scriptCompilationUnit = compilationUnit as ScriptCompilationUnit;
            if (scriptCompilationUnit != null)
            {
                var subnamespace = ScriptBuilder.GetSubnamespace(
                    scriptCompilationUnit.SourceUnit.SourceFile.RelativePath, true);
                var builder = new ScriptBuilder(scriptCompilationUnit, this, subnamespace);
                var scriptAssembly = GetAssembly();
                scriptAssembly.Builder = builder;
                return builder;
            }
            else
            {
                if (compilationUnit != null)
                {
                    return compilationUnit.ModuleBuilder;
                }
                else
                {
                    return null;
                }
            }
        }

        #endregion
    }
}
