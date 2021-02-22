using com.RazorSoftware.Logging;
using GitProxyApp.Tunnels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Log = com.RazorSoftware.Logging.Log;

namespace GitProxyApp
{
    class Program
    {
        static int Main(string[] args)
        {
            string cofigPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), SettingsFile.ConfigurationDirectory);
            DirectoryInfo configDirectory = new DirectoryInfo(cofigPath);

            if (!configDirectory.Exists)
                configDirectory.Create();

            Log.Initialize(false);
            for (int i = 0; i < 2000; i++)
            {
                FileInfo fi = new FileInfo(Path.Combine(cofigPath, $"GitProxy-{i}.log"));

                try
                {
                    Log.OpenFile(fi.FullName, "default", LogLevel.Debug, true);
                    break;
                }
                catch (Exception)
                {
                    // Ignore
                }
            }

            Log.Console.Enabled = true;
            Log.Console.AutoColorOutput = true;
            string settingsFilename = Path.Combine(cofigPath, SettingsFile.DefaultSettingsFilename);

            SettingsFile settings;
            if (!File.Exists(settingsFilename))
            {
                settings = new SettingsFile();
                settings.SaveToFile(settingsFilename);
            }
            else
            {
                settings = SettingsFile.LoadFromDisk(settingsFilename);
            }

            Log.Console.MinimumLevel = settings.VerboseMode ? LogLevel.Debug : LogLevel.Message;
            Log.Write("GitProxy> ", LogLevel.Debug);

            Log.Write(settings.GitExecutablePath, LogLevel.Debug);
            Log.Write(" ", LogLevel.Debug);

            List<string> gitArguments = new List<string>(args);
            if (settings.GitExtraArguments != null && settings.GitExtraArguments.Length > 0)
                gitArguments.AddRange(settings.GitExtraArguments);

            foreach (string a in gitArguments)
                Log.Write(a + " ", LogLevel.Debug);
            Log.WriteLine(LogLevel.Debug);

            SshTunnel tunnel = null;
            GitSession session = null;
            int remoteExitCode = 255;
            string errorOutput = null;
            string output = null;
            try
            {
                Log.WriteLine("Opening SSH tunnel to %@:%@ using username=%@. Credentials: %@ %@", LogLevel.Debug,
                    settings.SshHostname,
                    settings.SshPort,
                    settings.SshUsername,
                    !string.IsNullOrWhiteSpace(settings.SshPassword) ? "(Password)" : "",
                    !string.IsNullOrWhiteSpace(settings.SshKeyFile) ? "(PublicKey)" : ""
                );

                tunnel = SshTunnel.Open(settings.SshHostname, new Tunnels.Credentials.SshCredentials(settings.SshUsername, settings.SshPassword, settings.SshKeyFile), settings.SshPort);

                if (!tunnel.IsOpen)
                {
                    Log.WriteLine("Failed to open tunnel to remote host %@ on port %@.", LogLevel.Error, settings.SshHostname, settings.SshPort);
                    return 1;
                }
                session = tunnel.OpenGitSession(new GitSessionParameters() { Arguments = gitArguments.ToArray(), ExecutablePath = settings.GitExecutablePath, WorkingDirectory = MapDirectory(settings.DirectoryMappings, Directory.GetCurrentDirectory()) });

                Log.WriteLine("Remote GIT Session opened", LogLevel.Debug);

                Log.WriteLine("Waiting for StdOut...", LogLevel.Debug);
                output = session.ReadToEnd();
                errorOutput = session.StdError;
                remoteExitCode = session.GetExitCode();

                Log.WriteLine("Remote process exited with code %@", LogLevel.Debug, session.GetExitCode().ToString("N0"));
            }
            catch (Exception ex)
            {
                Log.WriteException(ex, LogLevel.Error);
                Log.WriteLine();
                return 255;
            }

            if (session != null && !session.IsDisposed)
            {
                Log.WriteLine("Closing remote GIT session...", LogLevel.Debug);
                session.Dispose();
            }

            if (tunnel != null && tunnel.IsOpen)
            {
                Log.WriteLine("Closing SSH tunnel...", LogLevel.Debug);
                tunnel.Close();
            }

            Log.WriteLine();

            if (!string.IsNullOrWhiteSpace(errorOutput))
                Log.WriteLine(errorOutput, LogLevel.Error);
            Log.Shutdown();

            Console.Write(output);

            return remoteExitCode;
        }

        static string MapDirectory(PathMapping[] mappings, string sourceDirectory)
        {
            foreach (PathMapping map in mappings)
            {
                if (map.LocalPath == "**" || sourceDirectory.StartsWith(map.LocalPath, StringComparison.OrdinalIgnoreCase))
                {
                    string transformedPath = map.LocalPath.Replace('\\', '/');
                    string finalPath = map.RemotePath;
                    if (!finalPath.EndsWith('/'))
                        finalPath += '/';

                    if (map.LocalPath != "**")
                    {
                        transformedPath = sourceDirectory.Substring(map.LocalPath.Length);
                        transformedPath = transformedPath.Replace('\\', '/');
                        if (transformedPath.StartsWith('/'))
                            finalPath += transformedPath.Substring(1);
                        else
                            finalPath += transformedPath;
                    }

                    finalPath = finalPath.Replace('\\', '/');

                    Log.WriteLine("Mapped path: \"%@\" ==> \"%@\"", LogLevel.Debug, sourceDirectory, finalPath);
                    return finalPath;
                }
            }

            Log.WriteLine("Unmapped path: \"%@\"", LogLevel.Debug, sourceDirectory);
            return sourceDirectory;
        }
    }
}
