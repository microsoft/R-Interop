using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace RInterop
{
    public class TraceLogger : ILogger
    {
        private TraceSource TraceSource { get; }

        public TraceLogger(string rootFolder, string prefix)
        {
            TraceSource = new TraceSource(prefix, SourceLevels.All);

            rootFolder = Path.Combine(Environment.ExpandEnvironmentVariables(rootFolder), prefix);
            Directory.CreateDirectory(rootFolder);

            string logFilePath = Path.Combine(rootFolder,
                string.Format(CultureInfo.InvariantCulture, "{0}_{1}.log",
                prefix,
                DateTime.UtcNow.Ticks));

            Trace.AutoFlush = true;

            TraceListener consoleListener = new ConsoleTraceListener();
            consoleListener.Filter = new EventTypeFilter(SourceLevels.Information);
            TraceSource.Listeners.Add(consoleListener);

            // First instance gets log file logFilename_0.log, second instance logFilename_1.log. 
            int maxLogFileNumber = -1;
            string newLogFilename = string.Empty;
            var logFileMask = string.Format(CultureInfo.InvariantCulture,
                "{0}.*{1}",
                Path.GetFileNameWithoutExtension(logFilePath),
                Path.GetExtension(logFilePath));

            // Get all the logFilename.N.log files
            DirectoryInfo di = new DirectoryInfo(
                Path.GetDirectoryName(Path.Combine(Environment.CurrentDirectory, logFilePath)));
            FileInfo[] logFiles = di.GetFiles(logFileMask, SearchOption.TopDirectoryOnly);

            // Sort files according to name 
            Array.Sort(logFiles, new CompareFileInfoNames());
            foreach (FileInfo f in logFiles)
            {
                // Find the log file number (e.g. 1 for "sqlnexus.001.log")
                int logFileNum;

                // Keep track of the largest log file number we've encountered so far
                if (Int32.TryParse(f.FullName.Split('.')[1], out logFileNum)
                    && logFileNum > maxLogFileNumber)
                {
                    maxLogFileNumber = logFileNum;
                }

                try
                {
                    // See if we can open the file exclusively (if so, it's safe to reuse)
                    FileStream fs = f.Open(FileMode.Open, FileAccess.Read, FileShare.None);
                    fs.Close();
                    newLogFilename = f.FullName;
                    break;
                }
                catch { }   // This log file is in use -- try next 
            }

            // If we didn't find a reusable log file, we'll create a new using the next available log file number
            if (string.IsNullOrEmpty(newLogFilename))
            {
                maxLogFileNumber++;
                newLogFilename = string.Format(CultureInfo.InvariantCulture,
                    @"{0}\{1}.{2}.{3}",
                    Path.GetDirectoryName(Path.Combine(Environment.CurrentDirectory, logFilePath)),
                    Path.GetFileNameWithoutExtension(logFilePath),
                    maxLogFileNumber.ToString("000", CultureInfo.InvariantCulture),
                    Path.GetExtension(logFilePath));    // e.g. "logFilename.001.log"
            }

            if (File.Exists(newLogFilename)) { File.Delete(newLogFilename); }

            // Hook up the log file as a trace listener
            FileStream fstream = new FileStream(newLogFilename,
                FileMode.Create,
                FileSystemRights.WriteData,
                FileShare.Read,
                (int)Math.Pow(2, 10),
                FileOptions.None,
                GetFileSecurity());

            TraceListener traceListener = new TextWriterTraceListener(fstream);
            traceListener.Filter = new EventTypeFilter(SourceLevels.All);
            TraceSource.Listeners.Add(traceListener);
        }

        public static FileSecurity GetFileSecurity()
        {
            var fss = new FileSecurity();
            //  Deny anonymous 
            try
            {
                fss.AddAccessRule(new FileSystemAccessRule(@"NT AUTHORITY\ANONYMOUS LOGON",
                    FileSystemRights.FullControl,
                    AccessControlType.Deny));
            }
            catch (IdentityNotMappedException)
            {
                //eat this because use can change names
            }
            try
            {
                // Deny guests 
                fss.AddAccessRule(new FileSystemAccessRule(WindowsBuiltInRole.Guest.ToString(),
                    FileSystemRights.FullControl,
                    AccessControlType.Deny));
            }
            catch (IdentityNotMappedException)
            {
                //eat this because use can change names
            }

            try
            {
                // Admins only can see that the file exists 
                fss.AddAccessRule(new FileSystemAccessRule(@"BuiltIn\Administrators",
                    FileSystemRights.ReadExtendedAttributes | FileSystemRights.ReadAttributes |
                    FileSystemRights.Synchronize,
                    AccessControlType.Allow));
            }
            catch (IdentityNotMappedException)
            {
                //eat this because use can change names
            }

            try
            {
                var strCurrentUser = WindowsIdentity.GetCurrent()?.Name;

                // Current User full control 
                if (strCurrentUser != null)
                {
                    fss.AddAccessRule(new FileSystemAccessRule(strCurrentUser,
                        FileSystemRights.FullControl,
                        AccessControlType.Allow));
                }
            }
            catch (IdentityNotMappedException)
            {
                //eat this because use can change names
            }

            return fss;
        }

        public void LogInformation(string message, params object[] parameters)
        {
            TraceSource.TraceInformation(string.Format(CultureInfo.InvariantCulture, "{0:O} {1}", DateTime.UtcNow, message, parameters));
        }

        public void Close()
        {
            TraceSource.Flush();
            TraceSource.Close();
        }

        private string GetCallingFunction()
        {
            StackFrame frame = new StackTrace().GetFrame(3);
            var declaringType = frame.GetMethod().DeclaringType;
            if (declaringType != null)
            {
                string callingFunction = string.Format(CultureInfo.InvariantCulture, "{0}.{1}", declaringType.FullName, frame.GetMethod().Name);
                return callingFunction;
            }
            return string.Empty;
        }
    }

    /// <summary>
    /// Used to sort an array of FileInfo objects by filename.
    /// </summary>
    class CompareFileInfoNames : IComparer
    {
        public int Compare(object x, object y)
        {
            FileInfo firstFile = (FileInfo)x;
            FileInfo secondFile = (FileInfo)y;
            return string.CompareOrdinal(firstFile.FullName, secondFile.FullName);
        }
    }
}
