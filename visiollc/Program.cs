using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using WinSCP;

namespace blueprism_sftp_45
{
    class Program
    {
        private const string HostName = "datadrop-usfed.apptio.com";
        private const string UserName = "deptva";
        private const string SshKeyPath =  @"C:\\Zillion\\POC\\blueprism\\blueprism-sftp\\private-key-file\\openssh-private-2.ppk";
        private const string SshKeyPassphrase = "BttApptioBot42?";
        static void Main(string[] args)
        {
            Winscp();
        }

        static void Winscp()
        {

            try
            {
                using (Session session = GetSession(HostName, UserName, SshKeyPath, SshKeyPassphrase))
                {
                    var dt = ListFiles(session, "/incoming/");
                    foreach (DataRow item in dt.Rows)
                    {
                        Console.WriteLine(item[0]);
                    }

                    var dt1 = ListDirectories(session, "/");
                    foreach (DataRow item in dt1.Rows)
                    {
                        Console.WriteLine(item[0]);
                    }

                    var dt2 = DownloadFiles(session, @"C:\\Zillion\\POC\\blueprism\\blueprism-sftp\\private-key-file\\", "/incoming/");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static Session GetSession(string host, string username, string privatekeyPath, string privatekeyPassphrase)
        {
            SessionOptions sessionOptions = new SessionOptions()
            {
                Protocol = Protocol.Sftp,
                HostName = host,
                UserName = username,
                SshPrivateKeyPath = privatekeyPath,
                PrivateKeyPassphrase = privatekeyPassphrase,
                SshHostKeyPolicy = SshHostKeyPolicy.AcceptNew
            };
            Session session = new Session();
            session.Open(sessionOptions);
            return session;
        }


        /// <summary>
        /// Uploads one or more files from local directory to remote directory.
        /// </summary>
        /// <param name="session">Sftp Session</param>
        /// <param name="localPath">Full path to local file or directory to upload. Filename in the path can be replaced with Windows wildcard1 to select multiple files. To upload all files in a directory, use mask *.</param>
        /// <param name="remotePath">Full path to upload the file to. When uploading multiple files, the filename in the path should be replaced with operation mask or omitted (path ends with slash).</param>
        public static TransferOperationResult UploadFiles(Session session, string localPath, string remotePath)
        {
            // Upload files
            TransferOptions transferOptions = new TransferOptions();
            transferOptions.TransferMode = TransferMode.Binary;


            TransferOperationResult transferResult;
            transferResult = session.PutFiles(localPath, remotePath, false, transferOptions);
            // Throw on any error
            transferResult.Check();

            return transferResult;
        }

        /// <summary>
        /// Lists only the Files of specified remote directory.
        /// </summary>
        /// <param name="session">Sftp Session</param>
        /// <param name="remoteDirPath">Full path to remote directory to be read.</param>
        public static DataTable ListFiles(Session session, string remoteDirPath)
        {
            // Upload files
            TransferOptions transferOptions = new TransferOptions();
            transferOptions.TransferMode = TransferMode.Binary;

            RemoteDirectoryInfo directory = session.ListDirectory(remoteDirPath);

            var table = new DataTable();
            int index = 0;
            var properties = new List<PropertyInfo>();
            foreach (RemoteFileInfo obj in directory.Files)
            {
                if (index == 0)
                {
                    foreach (var property in obj.GetType().GetProperties())
                    {
                        if (Nullable.GetUnderlyingType(property.PropertyType) != null)
                        {
                            continue;
                        }
                        if (property.Name != "Attributes")
                        {
                            properties.Add(property);
                            table.Columns.Add(new DataColumn(property.Name, typeof(string)));
                        }
                    }
                }
                object[] values = new object[properties.Count];
                for (int i = 0; i < properties.Count; i++)
                {
                    values[i] = properties[i].GetValue(obj);
                }
                if (!obj.IsDirectory)
                    table.Rows.Add(values);
                index++;
            }
            return table;
        }


        /// <summary>
        /// Lists only the directories of specified remote directory.
        /// </summary>
        /// <param name="session">Sftp Session</param>
        /// <param name="remoteDirPath">Full path to remote directory to be read.</param>
        public static DataTable ListDirectories(Session session, string remoteDirPath)
        {
            // Upload files
            TransferOptions transferOptions = new TransferOptions();
            transferOptions.TransferMode = TransferMode.Binary;

            RemoteDirectoryInfo directory = session.ListDirectory(remoteDirPath);
            var table = new DataTable();
            int index = 0;
            var properties = new List<PropertyInfo>();

            foreach (RemoteFileInfo obj in directory.Files)
            {
                if (index == 0)
                {
                    foreach (var property in obj.GetType().GetProperties())
                    {
                        if (Nullable.GetUnderlyingType(property.PropertyType) != null)
                        {
                            continue;
                        }
                        if (property.Name != "Attributes")
                        {
                            properties.Add(property);
                            table.Columns.Add(new DataColumn(property.Name, typeof(string)));
                        }
                    }
                }
                object[] values = new object[properties.Count];
                for (int i = 0; i < properties.Count; i++)
                {
                    values[i] = properties[i].GetValue(obj);
                }
                if (obj.IsDirectory && !obj.IsParentDirectory && !obj.IsThisDirectory)
                    table.Rows.Add(values);
                index++;
            }
            return table;
        }


        /// <summary>
        /// Downloads one or more files from remote directory to local directory.
        /// </summary>
        /// <param name="session">Sftp Session</param>
        /// <param name="localPath">Full path to download the file to. When downloading multiple files, the filename in the path should be replaced with operation mask or omitted (path ends with backslash).</param>
        /// <param name="remotePath">Full path to remote directory followed by slash and wildcard to select files or subdirectories to download. To download all files in a directory, use mask</param>
        public static TransferOperationResult DownloadFiles(Session session, string localPath, string remotePath)
        {
            // Download files
            TransferOptions transferOptions = new TransferOptions();
            transferOptions.TransferMode = TransferMode.Binary;


            TransferOperationResult transferResult;
            transferResult = session.GetFiles(remotePath, localPath, false, transferOptions);

            // Throw on any error
            transferResult.Check();

            return transferResult;
        }


        /// <summary>
        /// Removes one or more remote files.
        /// </summary>
        /// <param name="session">Sftp Session</param>
        /// <param name="remoteFilePath">Full path to remote directory followed by slash and wildcard to select files or subdirectories to remove.</param>
        public static RemovalOperationResult RemoveFiles(Session session, string remoteFilePath)
        {
            RemovalOperationResult operationResult;
            operationResult = session.RemoveFiles(remoteFilePath);
            operationResult.Check();
            return operationResult;
        }


        /// <summary>
        /// Executes command on the remote server.
        /// </summary>
        /// <param name="session">Sftp Session</param>
        /// <param name="command">Command to execute.</param>
        public static CommandExecutionResult ExecuteCommand(Session session, string command)
        {
            var operationResult = session.ExecuteCommand(command);
            operationResult.Check();
            return operationResult;
        }
    }
}
