using System;
using System.IO;
using System.Linq;

using MSBuild.NugetContentRestore.Tasks.Utilities;

namespace MSBuild.NugetContentRestore.Tasks.Extensions
{
    public static class DirectoryExtensions
    {

        public static void CopyTo(this DirectoryInfo source, string destination, bool recursive, Wildcard[] ignorePatterns, bool smartRestore)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (destination == null) throw new ArgumentNullException("destination");
            if (!source.Exists) throw new DirectoryNotFoundException("Source directory not found: " + source.FullName);

            var target = new DirectoryInfo(destination);
            string targetFile;
            FileInfo targetFileInfo;

            if (!target.Exists) target.Create();


            foreach (var file in source.GetFiles())
            {
                if (ignorePatterns.Any(p => p.IsMatch(file.Name))) continue;

                targetFile = Path.Combine(target.FullName, file.Name);

                if (smartRestore)
                {
                    // perform additional checks to see if we really need to copy the file
                    targetFileInfo = new FileInfo(targetFile);

                    // if target exists and source/target have same write time, skip copying this file (don't worry about file contents - turn off SmartRestore if wanting that)
                    if (targetFileInfo.Exists && file.LastWriteTime.Equals(targetFileInfo.LastWriteTime)) continue;
                }


                file.CopyTo(targetFile, true);
            }

            // Exit Condition
            if (!recursive) return;

            foreach (var directory in source.GetDirectories())
                CopyTo(directory, Path.Combine(target.FullName, directory.Name), recursive, ignorePatterns, smartRestore);
        }

    }
}
