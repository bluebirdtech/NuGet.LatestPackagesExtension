using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace NuGet.LatestPackagesExtensionTests
{
    [TestFixture]
    public class Tests
    {
        [Test]
        public void Test()
        {
            const string packagesContent = @"<?xml version=""1.0"" encoding=""utf-8""?><packages><package id=""NUnit"" version=""2.6.2"" targetFramework=""net35"" /></packages>";
            const string packagesPath = @"..\..\packages.config";
            const string latestPackagesPath = @"..\..\latestPackages.config";
            File.WriteAllText(packagesPath, packagesContent);

            using(Process p = new Process {StartInfo = new ProcessStartInfo(@"..\..\..\.nuget\NuGet.exe", string.Format("updateLatest {0} {1}", latestPackagesPath, packagesPath)) { UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true}})
            {
                p.Start();
                p.WaitForExit();
                Assert.True(File.ReadAllText(packagesPath).Contains(@"id=""NUnit"""));
                Assert.False(File.ReadAllText(packagesPath).Contains("2.6.2"));
                //string output = p.StandardOutput.ReadToEnd();
            }
        }
    }
}
