using Microsoft.Build.Framework;
using Microsoft.SignCheck.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SignCheck
{
    public class SignCheckTask : Microsoft.Build.Utilities.Task
    {
        public bool EnableJarSignatureVerification
        {
            get;
            set;
        }
        public bool EnableXmlSignatureVerification
        {
            get;
            set;
        }
        public string FileStatus
        {
            get;
            set;
        }

        public string ExclusionsOutput
        {
            get;
            set;
        }

        public bool SkipTimestamp
        {
            get;
            set;
        }
        public bool Recursive
        {
            get;
            set;
        }
        public bool VerifyStrongName
        {
            get;
            set;
        }
        public string ExclusionsFile
        {
            get;
            set;
        }
        public ITaskItem[] InputFiles
        {
            get;
            set;
        }
        [Required]
        public string LogFile
        {
            get;
            set;
        }
        [Required]
        public string ErrorLogFile
        {
            get;
            set;
        }
        public string Verbosity
        {
            get;
            set;
        }
        public string ArtifactFolder
        {
            get;
            set;
        }

        public override bool Execute()
        {
            Options options = new Options();
            options.EnableJarSignatureVerification = EnableJarSignatureVerification;
            options.EnableXmlSignatureVerification = EnableXmlSignatureVerification;
            options.ExclusionsFile = ExclusionsFile;
            options.ExclusionsOutput = ExclusionsOutput;
            string[] filestatuses = FileStatus.Split(';', ',');
            options.FileStatus = filestatuses;
            options.Recursive = Recursive;
            options.SkipTimestamp = SkipTimestamp;
            options.VerifyStrongName = VerifyStrongName;
            options.LogFile = LogFile;
            options.ErrorLogFile = ErrorLogFile;

            List<string> inputFiles = new List<string>();
            if (InputFiles != null)
            {
                foreach (var checkFile in InputFiles.Select(s => s.ItemSpec).ToArray())
                {
                    if (Path.IsPathRooted(checkFile))
                    {
                        inputFiles.Add(checkFile);
                    }
                    else
                    {
                        ArtifactFolder = ArtifactFolder ?? Environment.CurrentDirectory;
                        SearchOption fileSearchOptions = Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                        var matchedFiles = Directory.GetFiles(ArtifactFolder, checkFile, fileSearchOptions);

                        if(matchedFiles.Length == 1)
                        {
                            inputFiles.Add(matchedFiles[0]);
                        }
                        else if(matchedFiles.Length == 0)
                        {
                            Log.LogError($"Unable to find file '{checkFile}' in folder '{ArtifactFolder}'.  Try specifying 'Recursive=true` to include subfolders");
                        }
                        else if (matchedFiles.Length > 1)
                        {
                            Log.LogError($"found multiple files matching pattern '{checkFile}'");
                            foreach(var file in matchedFiles)
                            {
                                Log.LogError($" - {file}");
                            }
                        }
                    }
                }
            }
            options.InputFiles = inputFiles.ToArray();

            LogVerbosity verbosity;
            if(Enum.TryParse<LogVerbosity>(Verbosity, out verbosity))
            {
                options.Verbosity = verbosity;
            }
            
            var sc = new SignCheck(new string[] { });
            sc.Options = options;
            int result = sc.Run();
            return result != 0 || Log.HasLoggedErrors ? false: true;
        }
    }
}
