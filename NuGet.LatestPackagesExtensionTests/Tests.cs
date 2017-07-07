using System.IO;
using NuGet.CommandLine;
using NUnit.Framework;

namespace NuGet.LatestPackagesExtensionTests
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void Test()
        {
            string packagesPath = Path.Combine(Directory.GetCurrentDirectory(), "packages.config");
            string latestPackagesPath = Path.Combine(Directory.GetCurrentDirectory(), "latestPackages.config");
            File.WriteAllText(latestPackagesPath, @"<?xml version=""1.0"" encoding=""utf-8""?><packages><package id=""NUnit"" version=""0.0"" targetFramework=""net35"" /></packages>");
            File.Delete(packagesPath);

            var dir = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            var filename = "NuGet.LatestPackagesExtensions.dll";
            File.Copy("NuGet.LatestPackagesExtensions.dll", Path.Combine(dir, filename)); // Resharper test runner puts each assembly in a separate folder, so this is needed.
            Program.Main(new[]
            {
                "updateLatest",
                latestPackagesPath,
                packagesPath
            });

            Assert.True(File.ReadAllText(packagesPath).Contains(@"id=""NUnit"""));
            Assert.False(File.ReadAllText(packagesPath).Contains("2.6.2"));
        }
    }
}
