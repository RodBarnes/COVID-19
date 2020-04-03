using System;
using System.Reflection;

namespace Viewer.ViewModels
{
    public class AboutVM
    {
        readonly Assembly assembly;
        readonly Version version;

        public AboutVM()
        {
            assembly = Assembly.GetExecutingAssembly();
            ApplicationName = assembly.GetName().Name;
            version = assembly.GetName().Version;
            Description = "This tool supports creating label definitions for use with the Sales Logistics Print service which supports TSC/AMT printers.";
        }

        #region Properties

        public string ApplicationName { get; }

        public int MajorVersion => version.Major;

        public int MinorVersion => version.Minor;

        public int BuildNumber => version.Build;

        public int RevisionNumber => version.Revision;

        public string VersionString => $"Version: {MajorVersion}.{MinorVersion}.{BuildNumber}.{RevisionNumber}";

        public string Description { get; set; }

        #endregion
    }
}
