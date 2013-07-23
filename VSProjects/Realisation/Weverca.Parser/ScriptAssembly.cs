using System.Collections.Generic;
using System.Reflection;

using PHP.Core;
using PHP.Core.Emit;
using PHP.Core.Reflection;

namespace Weverca.Parsers
{
    /// <summary>
    /// Assembly of PHP script
    /// </summary>
    /// <remarks>
    /// Since Weverca does not perform complete compilation to assembly and only uses syntactic analysis,
    /// assembly of PHP script does not exist. Instead of, assembly of this application is used. However,
    /// Phalanger utilizes descendants of <see cref="CompilationUnit" /> abstract class during the
    /// compilation that contains useful information. Specifically, <see cref="ScriptCompilationUnit" />
    /// class which is slightly more robust than other implementations of <c>CompilationUnit</c>. This class
    /// requires existing PHP assembly, but also works with any other assembly, precisely because it is
    /// applied AST only.
    /// </remarks>
    internal sealed class BasicScriptAssembly : ScriptAssembly
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BasicScriptAssembly" /> class.
        /// </summary>
        /// <param name="assembly">Assembly of PHP script</param>
        /// <param name="assemblyName">Name of the assembly</param>
        /// <param name="compilationUnit">PHP script compilation unit</param>
        internal BasicScriptAssembly(
            Assembly/*!*/ assembly,
            AssemblyName assemblyName,
            ScriptCompilationUnit/*!*/ compilationUnit)
            : base(ApplicationContext.Default, assembly)
        {
            var module = assembly.ManifestModule;
            Debug.Assert(module != null, "There must be main module with manifest");
            var assemblyBuilder = new BasicScriptAssemblyBuilder(this, assemblyName, module);
            var subNamespace = ScriptBuilder.GetSubnamespace(
                compilationUnit.SourceUnit.SourceFile.RelativePath, true);
            Builder = new ScriptBuilder(compilationUnit, assemblyBuilder, subNamespace);
        }

        /// <summary>
        /// Gets or sets builder providing means for building PHP script
        /// </summary>
        public ScriptBuilder Builder { get; internal set; }

        /// <summary>
        /// Gets script module of PHP script
        /// </summary>
        /// <returns>Module of PHP script</returns>
        public ScriptModule GetModule()
        {
            return Builder;
        }

        #region ScriptAssembly overrides

        /// <summary>
        /// Gets a value indicating whether the script assembly comprising of multiple script modules
        /// </summary>
        /// <remarks>
        /// It is always false.
        /// </remarks>
        public override bool IsMultiScript
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets all script modules of PHP script. The assembly consists of one module only.
        /// </summary>
        /// <returns>Script modules of PHP script</returns>
        public override IEnumerable<ScriptModule> GetModules()
        {
            yield return Builder;
        }

        /// <summary>
        /// Gets module of PHP script
        /// </summary>
        /// <param name="name">PHP source file</param>
        /// <returns>Module of PHP script</returns>
        public override PhpModule GetModule(PhpSourceFile name)
        {
            return Builder;
        }

        #endregion
    }
}
