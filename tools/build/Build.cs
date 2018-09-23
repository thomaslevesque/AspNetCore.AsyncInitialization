using System.IO;
using System.Runtime.CompilerServices;
using McMaster.Extensions.CommandLineUtils;
using static Bullseye.Targets;
using static SimpleExec.Command;

namespace build
{
    [Command(ThrowOnUnexpectedArgument = false)]
    [SuppressDefaultHelpOption]
    class Build
    {
        static void Main(string[] args) =>
            CommandLineApplication.Execute<Build>(args);

        [Option("-h|-?|--help", "Show help message", CommandOptionType.NoValue)]
        public bool ShowHelp { get; } = false;

        [Option("-v|--version", "The version to build", CommandOptionType.SingleValue)]
        public string Version { get; } = "0.0.0";

        [Option("-c|--configuration", "The configuration to build", CommandOptionType.SingleValue)]
        public string Configuration { get; } = "Release";

        public string[] RemainingArguments { get; } = null;

        public void OnExecute(CommandLineApplication app)
        {
            if (ShowHelp)
            {
                app.ShowHelp();
                app.Out.WriteLine("Bullseye help:");
                app.Out.WriteLine();
                RunTargets(new[] { "-h" });
                return;
            }

            Directory.SetCurrentDirectory(GetSolutionDirectory());
            string solutionFile = "AspNetCore.AsyncInitialization.sln";
            string libraryProject = "src/AspNetCore.AsyncInitialization/AspNetCore.AsyncInitialization.csproj";
            string testProject = "tests/AspNetCore.AsyncInitialization.Tests/AspNetCore.AsyncInitialization.Tests.csproj";

            Target(
                "build",
                () => Run("dotnet", $"build -c \"{Configuration}\" /p:Version=\"{Version}\" \"{solutionFile}\""));

            Target(
                "test",
                DependsOn("build"),
                () => Run("dotnet", $"test -c \"{Configuration}\" --no-build \"{testProject}\""));

            Target(
                "pack",
                DependsOn("build"),
                () => Run("dotnet", $"pack -c \"{Configuration}\" --no-build /p:Version=\"{Version}\" \"{libraryProject}\""));

            Target("default", DependsOn("test", "pack"));

            RunTargets(RemainingArguments);
        }

        private static string GetSolutionDirectory() =>
            Path.GetFullPath(Path.Combine(GetScriptDirectory(), @"..\.."));

        private static string GetScriptDirectory([CallerFilePath] string filename = null) => Path.GetDirectoryName(filename);
    }
}