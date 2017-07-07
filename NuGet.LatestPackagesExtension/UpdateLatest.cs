using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using NuGet.CommandLine;

namespace NuGet.LatestPackagesExtensions.Commands
{
    [Command(typeof(UpdateLatestResources), "updateLatest", "UpdateLatestCommandDescription", MinArgs = 2, MaxArgs = 2, UsageSummary = "<latestPackages.config> <packages.config>")]
    public class UpdateLatest : Command
    {
        static class LocalConstants
        {
            public static string LatestPackageReferenceFile = "latestPackages.config";
        }

        [ImportingConstructor]
        public UpdateLatest()
        {
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        public override void ExecuteCommand()
        {
            string inputPackagesConfigPath = GetLatestPackagesConfigPath();
            string outputPackagesConfigPath = GetPackagesConfigPath();

            if (String.IsNullOrEmpty(inputPackagesConfigPath) || String.IsNullOrEmpty(outputPackagesConfigPath))
            {
                throw new CommandLineException();
            }

            if (!File.Exists(inputPackagesConfigPath))
            {
                throw new CommandLineException();
            }

            TryCreateAllDirectories(Path.GetDirectoryName(outputPackagesConfigPath));
            if(!File.Exists(outputPackagesConfigPath))
            {
                File.WriteAllText(outputPackagesConfigPath, @"<?xml version=""1.0"" encoding=""utf-8""?>
<packages>
</packages>");
            }

            PhysicalFileSystem outputFileSystem = new PhysicalFileSystem(Path.GetDirectoryName(outputPackagesConfigPath));
            PackageReferenceFile outputFile = new PackageReferenceFile(outputFileSystem, Path.GetFileName(outputPackagesConfigPath));

            // Remove all existing references from output file
            Dictionary<string, SemanticVersion> existingReferences = new Dictionary<string,SemanticVersion>();
            foreach (PackageReference packageReference in outputFile.GetPackageReferences())
            {
                existingReferences.Add(packageReference.Id, packageReference.Version);
            }
            foreach (KeyValuePair<string, SemanticVersion> pair in existingReferences)
            {
                outputFile.DeleteEntry(pair.Key, pair.Value);
            }

            PhysicalFileSystem inputFileSystem = new PhysicalFileSystem(Path.GetDirectoryName(inputPackagesConfigPath));
            PackageReferenceFile inputFile = new PackageReferenceFile(inputFileSystem, Path.GetFileName(inputPackagesConfigPath));
            foreach (PackageReference packageReference in inputFile.GetPackageReferences())
            {
                IPackage package = GetLatestPackage(packageReference.Id);
                outputFile.AddEntry(packageReference.Id, package.Version, false, packageReference.TargetFramework);
            }
        }

        static bool TryCreateAllDirectories(string path)
        {
            // Normalize path
            path = Path.GetFullPath(path);

            var createdADirectory = false;
            var pathParts = path.Split('/', '\\');
            var incrementalPath = "";
            foreach(var pathPart in pathParts)
            {
                incrementalPath += pathPart;
                if(pathPart != "" && !pathPart.Contains(":") && !Directory.Exists(incrementalPath))
                {
                    Directory.CreateDirectory(incrementalPath);
                    createdADirectory = true;
                }
                incrementalPath += '/';
            }
            return createdADirectory;
        }

        IPackage GetLatestPackage(string packageId)
        {
            foreach (var packageSource in SourceProvider.LoadPackageSources())
            {
                if(!packageSource.IsEnabled)
                    continue;

                IPackageRepository repository = RepositoryFactory.CreateRepository(packageSource.Source);
                
                // ReSharper disable once ReplaceWithSingleCallToFirstOrDefault because FirstOrDefault is not supported on the GetPackages() implementation of IQueryable
                IPackage package = repository.GetPackages().Where(p => p.IsLatestVersion && p.Id.Equals(packageId, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (package != null)
                    return package;
            }
            throw new InvalidOperationException("Package not found.");
        }

        string GetLatestPackagesConfigPath()
        {
            if (Arguments.Any())
            {
                string path = Arguments[0];
                if (path.EndsWith(LocalConstants.LatestPackageReferenceFile, StringComparison.OrdinalIgnoreCase))
                {
                    return Path.GetFullPath(path);
                }
            }

            return null;
        }

        string GetPackagesConfigPath()
        {
            if (Arguments.Any())
            {
                string path = Arguments[1];
                if (path.EndsWith(Constants.PackageReferenceFile, StringComparison.OrdinalIgnoreCase))
                {
                    return Path.GetFullPath(path);
                }
            }

            return null;
        }
    }
}
