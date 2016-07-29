using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

using MSBuild.NugetContentRestore.Tasks.Entities;
using MSBuild.NugetContentRestore.Tasks.Extensions;
using MSBuild.NugetContentRestore.Tasks.Utilities;

using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MSBuild.NugetContentRestore.Tasks
{
    public class NugetContentRestoreTask : Task
    {

        #region Private Members

        private readonly string[] _folders = new[] { "Scripts", "Images", "fonts", "content" };
        private readonly string[] _ignoreFilePatterns = new[] { "*.transform", "*.install.xdt", "*.pp" };

        private string _configFileFullPath;
        #endregion

        #region Public Properties

        public string[] Folders { get { return _folders; } }
        public string[] IgnoreFilePatterns { get { return _ignoreFilePatterns; } }

        public string[] AdditionalFolders { get; set; }
        public string[] AdditionalIgnoreFilePatterns { get; set; }

        [Required]
        public string SolutionDir { get; set; }

        [Required]
        public string ProjectDir { get; set; }

        /// <summary>
        /// Enable SmartRestore (default: true)
        /// 
        /// SmartRestore detects if a file has changed in the source (package folders) compared to a target file.
        /// If it hasn't changed, it will skip the file copy, which for packages with large content folders, can significantly improve Visual Studio build performance.
        /// </summary>
        public bool EnableSmartRestore { get; set; } = true;


        public string ConfigFileFullPath
        {
            get
            {
                return _configFileFullPath ?? Path.Combine(ProjectDir, "packages.config");
            }
            set { _configFileFullPath = value;  }
        }

        #endregion

        #region Task Implementation

        public override bool Execute()
        {
            if (!IsValidInput()) return false;

            var startTime = System.DateTime.Now;

            Log.LogMessage(MessageImportance.Low, "NugetContentRestore :: SolutionDir='{0}'", SolutionDir);
            Log.LogMessage(MessageImportance.Low, "NugetContentRestore :: ProjectDir='{0}'", ProjectDir);
            Log.LogMessage(MessageImportance.Low, "NugetContentRestore :: ConfigFileFullPath='{0}'", ConfigFileFullPath);
            Log.LogMessage(MessageImportance.Low, "NugetContentRestore :: EnableSmartRestore='{0}'", EnableSmartRestore.ToString());

            // Get NuGet Package Configuration
            var packages = GetPackages();
            foreach (var package in packages)
            {
                var packageFullPath = Path.Combine(SolutionDir, "packages", package.FolderName);
                Log.LogMessage(MessageImportance.Low, "NugetContentRestore :: {0} :: FullPath='{1}'", package.FolderName, packageFullPath);
                if (!Directory.Exists(packageFullPath)) continue;

                var packageContentsFullPath = Path.Combine(packageFullPath, "Content");
                Log.LogMessage(MessageImportance.Low, "NugetContentRestore :: {0} :: ContentsFullPath='{1}'", package.FolderName, packageContentsFullPath);
                if (!Directory.Exists(packageContentsFullPath)) continue;

                // Create Regex List for Ignore File Patterns
                var ignoreFilePatternsArray = _ignoreFilePatterns;
                if (AdditionalIgnoreFilePatterns != null)
                    ignoreFilePatternsArray = ignoreFilePatternsArray.Union(AdditionalIgnoreFilePatterns).ToArray();

                var ignoreFilePatterns = from p in ignoreFilePatternsArray
                                         select new Wildcard(p, RegexOptions.IgnoreCase);

                // Restore Package Content for predefined folders (_folders)
                var filePatterns = ignoreFilePatterns as Wildcard[] ?? ignoreFilePatterns.ToArray();
                foreach (var folder in _folders)
                {
                    var sourceFolderInfo = new DirectoryInfo(Path.Combine(packageContentsFullPath, folder));
                    if (!sourceFolderInfo.Exists) continue;

                    Log.LogMessage(MessageImportance.High, "NugetContentRestore :: {0} :: {1} :: Restoring content files", package.FolderName, folder);
                    sourceFolderInfo.CopyTo(Path.Combine(ProjectDir, folder), true, filePatterns.ToArray(), EnableSmartRestore);
                }

                // Restore Package Content for additional folders (AdditionalFolder)
                if (AdditionalFolders == null) continue;
                foreach (var folder in AdditionalFolders)
                {
                    var sourceFolderInfo = new DirectoryInfo(Path.Combine(packageContentsFullPath, folder));
                    if (!sourceFolderInfo.Exists) continue;

					Log.LogMessage(MessageImportance.High, "NugetContentRestore :: {0} :: {1} :: Restoring content files", package.FolderName, folder);
                    sourceFolderInfo.CopyTo(Path.Combine(ProjectDir, folder), true, filePatterns.ToArray(), EnableSmartRestore);
                }
            }

            var elapsed = System.DateTime.Now - startTime;
            Log.LogMessage(MessageImportance.High, "NugetContentRestore :: Completed restore of all package contents in {0} seconds", elapsed.TotalSeconds);

            return true;
        }

        #endregion

        #region Private Methods

        private bool IsValidInput()
        {
            if (!Directory.Exists(SolutionDir))
            {
                Log.LogError("NugetContentRestore :: Could not find directory SolutionDir='{0}'", SolutionDir);
                return false;
            }

            if (!Directory.Exists(ProjectDir))
            {
                Log.LogError("NugetContentRestore :: Could not find directory ProjectDir='{0}'", ProjectDir);
                return false;
            }

            if (!File.Exists(ConfigFileFullPath))
            {
                Log.LogError("NugetContentRestore :: Could not find file ConfigFileFullPath='{0}'", ConfigFileFullPath);
                return false;
            }

            return true;
        }

        private IEnumerable<Package> GetPackages()
        {
            List<Package> packages;
            var serializer = new XmlSerializer(typeof(NuGetPackageConfiguration));
            using (var reader = new StreamReader(ConfigFileFullPath))
            {
                var nugetConfiguration = (NuGetPackageConfiguration)serializer.Deserialize(reader);
                packages = nugetConfiguration.Packages;
            }
            return packages;
        }
        #endregion

    }

}
