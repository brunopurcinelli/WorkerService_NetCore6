using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinSCP;
using WorkerService_NetCore6_Example.Application.Interfaces;
using WorkerService_NetCore6_Example.Domain.Models;
using static System.Collections.Specialized.BitVector32;

namespace WorkerService_NetCore6_Example.Application.Services
{
    public class WorkerAppService : IWorkerAppService
    {

        private readonly ILogger<Worker> _logger;
        private readonly WorkerParameters _workerParameters;
        private static int _counterAllDeleteFiles = 0;

        public WorkerAppService(IConfiguration configuration, ILogger<Worker> logger)
        {
            _logger = logger;

            _workerParameters = new WorkerParameters();
            new ConfigureFromConfigurationOptions<WorkerParameters>(configuration.GetSection("WorkerParameters")).Configure(_workerParameters);
        }

        public async Task Process()
        {
            try
            {
                Run();
            }
            catch (Exception)
            {
                throw;
            }
        }
        private void Run()
        {
            _workerParameters.Hosts.ForEach(hostCrypt =>
            {
                var hostParameter = new HostParameter()
                {
                    Protocol = new Configuration.EncryptDecryptHelper().Decrypt(hostCrypt.Protocol),
                    HostName = new Configuration.EncryptDecryptHelper().Decrypt(hostCrypt.HostName),
                    Port = new Configuration.EncryptDecryptHelper().Decrypt(hostCrypt.Port),
                    LogonType = new Configuration.EncryptDecryptHelper().Decrypt(hostCrypt.LogonType),
                    User = new Configuration.EncryptDecryptHelper().Decrypt(hostCrypt.User),
                    Password = new Configuration.EncryptDecryptHelper().Decrypt(hostCrypt.Password),
                    FingerPrint = new Configuration.EncryptDecryptHelper().Decrypt(hostCrypt.FingerPrint),
                    DeleteFilesOtherThanDays = new Configuration.EncryptDecryptHelper().Decrypt(hostCrypt.DeleteFilesOtherThanDays),
                    DefaultFolder = new Configuration.EncryptDecryptHelper().Decrypt(hostCrypt.DefaultFolder),
                };

                // Setup session options
                SessionOptions sessionOptions = new()
                {
                    Protocol = hostParameter.Protocol switch
                    {
                        "SFTP" => Protocol.Sftp,
                        "FTP" => Protocol.Ftp,
                        _ => Protocol.Sftp,
                    },
                    HostName = hostParameter.HostName,
                    PortNumber = Convert.ToInt32(hostParameter.Port),
                    UserName = hostParameter.User,
                    Password = hostParameter.Password,
                    SshHostKeyFingerprint = hostParameter.FingerPrint
                };

                using var session = new Session();
                session.Open(sessionOptions);

                RemoteDirectoryInfo directory = session.ListDirectory(hostParameter.DefaultFolder);
                DeleteFiles(hostParameter, session, directory);
            });
        }

        private void DeleteFiles(HostParameter hostParameter, Session session, RemoteDirectoryInfo directory)
        {
            foreach (RemoteFileInfo fileInfo in directory.Files)
            {
                if (fileInfo.IsDirectory && fileInfo.Name != "." && fileInfo.Name != "..")
                {
                    RemoteDirectoryInfo subDirectory = session.ListDirectory(fileInfo.FullName);
                    DeleteFiles(hostParameter, session, subDirectory);
                }
                else if (!fileInfo.IsDirectory && fileInfo.Name != "." && fileInfo.Name != "..")
                {
                    var days = Convert.ToInt32(hostParameter.DeleteFilesOtherThanDays);
                    DateTime deteDelete = DateTime.Now.ToLocalTime().AddDays(-days);

                    if (days > 0 &&
                        fileInfo.LastWriteTime.Date <= deteDelete.Date)
                    {
                        session.RemoveFile(fileInfo.FullName);
                        Console.WriteLine("{0} has removed", fileInfo.Name);
                        _counterAllDeleteFiles++;
                    }
                }
            }
        }


        public void Dispose()
        {
            throw new NotImplementedException();
        }

    }
}
