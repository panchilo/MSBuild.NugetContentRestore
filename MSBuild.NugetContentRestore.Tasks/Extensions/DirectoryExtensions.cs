using System;
using System.IO;
using System.Linq;

using Microsoft.Build.Evaluation;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using MSBuild.NugetContentRestore.Tasks.Utilities;

namespace MSBuild.NugetContentRestore.Tasks.Extensions
{
    public static class DirectoryExtensions
    {
        private static string CurrentProjectPath { get; set; }
        private static Project CurrentProject { get; set; }

        public static void CopyTo(this DirectoryInfo source, string destination, bool recursive, Wildcard[] ignorePatterns, bool smartRestore, string projectPath, TaskLoggingHelper log)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (destination == null) throw new ArgumentNullException("destination");
            if (!source.Exists) throw new DirectoryNotFoundException("Source directory not found: " + source.FullName);

            var target = new DirectoryInfo(destination);

            if (!target.Exists) target.Create();

            foreach (var file in source.GetFiles())
            {
                if (ignorePatterns.Any(p => p.IsMatch(file.Name))) continue;

                var targetFile = Path.Combine(target.FullName, file.Name);

                if (smartRestore)
                {
                    // perform additional checks to see if we really need to copy the file
                    var targetFileInfo = new FileInfo(targetFile);

                    // if target exists and source/target have same write time, skip copying this file (don't worry about file contents - turn off SmartRestore if wanting that)
                    if (targetFileInfo.Exists && file.LastWriteTime.Equals(targetFileInfo.LastWriteTime)) continue;
                }

                if (string.Equals(file.Extension, ".pp", StringComparison.InvariantCultureIgnoreCase))
                {
                    targetFile = targetFile.Substring(0, targetFile.Length - ".pp".Length);
                    log.LogMessage(MessageImportance.Low, "NugetContentRestore :: Processing Source Code Transformation for file '{0}' in project '{1}'", targetFile, projectPath);
                    ProccessSourceCodeTransformation(projectPath, file.FullName, targetFile, log);
                }
                else {
                    file.CopyTo(targetFile, true);
                }
            }

            // Exit Condition
            if (!recursive) return;

            foreach (var directory in source.GetDirectories())
                CopyTo(directory, Path.Combine(target.FullName, directory.Name), true, ignorePatterns, smartRestore, projectPath, log);
        }

        private static void ProccessSourceCodeTransformation(string projectPath, string sourcePath, string targetPath, TaskLoggingHelper log)
        {
            if (!string.Equals(CurrentProjectPath, projectPath, StringComparison.InvariantCultureIgnoreCase))
            {
                CurrentProjectPath = projectPath;
                CurrentProject = new Project(CurrentProjectPath);
            }

            var source = File.ReadAllLines(sourcePath);
            ReplaceProjectPropertyTokens(source, log);
            File.WriteAllLines(targetPath, source);
        }

        private static void ReplaceProjectPropertyTokens(string[] source, TaskLoggingHelper log)
        {
            for (var i = 0; i < source.Length; i++)
            {
                var splitLine = source[i].Split('$');
                if (splitLine.Length <= 2)
                {
                    continue;
                }

                for (var j = 1; j < splitLine.Length; j += 2)
                {
                    var token = splitLine[j];
                    if (token.Contains(" "))
                    {
                        // Not a valid token...
                        j--;
                        continue;
                    }
                    var value = CurrentProject.GetPropertyValue(token);
                    log.LogMessage(MessageImportance.Low, "NugetContentRestore :: Replacing token '{0}' with '{1}'", token, value);
                    splitLine[j] = value;
                }
                source[i] = string.Join(string.Empty, splitLine);
            }
        }
    }
}
