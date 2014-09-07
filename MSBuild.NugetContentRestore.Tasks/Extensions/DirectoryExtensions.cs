using System;
using System.IO;
using System.Linq;

using MSBuild.NugetContentRestore.Tasks.Utilities;

namespace MSBuild.NugetContentRestore.Tasks.Extensions
{
    public static class DirectoryExtensions
    {

        public static void CopyTo(this DirectoryInfo source, string destination, bool recursive, Wildcard[] ignorePatterns)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (destination == null) throw new ArgumentNullException("destination");
            if (!source.Exists) throw new DirectoryNotFoundException("Source directory not found: " + source.FullName);

            var target = new DirectoryInfo(destination);
            if (!target.Exists) target.Create();

            foreach (var file in source.GetFiles())
            {
                if (ignorePatterns.Any(p => p.IsMatch(file.Name))) continue;
                file.CopyTo(Path.Combine(target.FullName, file.Name), true);
            }

            // Exit Condition
            if (!recursive) return;

            foreach (var directory in source.GetDirectories())
                CopyTo(directory, Path.Combine(target.FullName, directory.Name), recursive, ignorePatterns);
        }

    }
}
