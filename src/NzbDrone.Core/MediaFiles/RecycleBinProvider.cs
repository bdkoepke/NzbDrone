﻿using System;
using System.IO;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.MediaFiles
{
    public interface IRecycleBinProvider
    {
        void DeleteFolder(string path);
        void DeleteFile(string path);
        void Empty();
        void Cleanup();
    }

    public class RecycleBinProvider : IHandleAsync<SeriesDeletedEvent>, IExecute<CleanUpRecycleBinCommand>, IRecycleBinProvider
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IConfigService _configService;

        private static readonly Logger logger =  NzbDroneLogger.GetLogger();

        public RecycleBinProvider(IDiskProvider diskProvider, IConfigService configService)
        {
            _diskProvider = diskProvider;
            _configService = configService;
        }

        public void DeleteFolder(string path)
        {
            logger.Info("Attempting to send '{0}' to recycling bin", path);
            var recyclingBin = _configService.RecycleBin;

            if (String.IsNullOrWhiteSpace(recyclingBin))
            {
                logger.Info("Recycling Bin has not been configured, deleting permanently. {0}", path);
                _diskProvider.DeleteFolder(path, true);
                logger.Debug("Folder has been permanently deleted: {0}", path);
            }

            else
            {
                var destination = Path.Combine(recyclingBin, new DirectoryInfo(path).Name);

                logger.Debug("Moving '{0}' to '{1}'", path, destination);
                _diskProvider.MoveFolder(path, destination);

                logger.Debug("Setting last accessed: {0}", path);
                _diskProvider.FolderSetLastWriteTime(destination, DateTime.UtcNow);
                foreach (var file in _diskProvider.GetFiles(destination, SearchOption.AllDirectories))
                {
                    if (OsInfo.IsWindows)
                    {
                        //TODO: Better fix than this for non-Windows?
                        _diskProvider.FileSetLastWriteTime(file, DateTime.UtcNow);
                    }
                }

                logger.Debug("Folder has been moved to the recycling bin: {0}", destination);
            }
        }

        public void DeleteFile(string path)
        {
            logger.Debug("Attempting to send '{0}' to recycling bin", path);
            var recyclingBin = _configService.RecycleBin;

            if (String.IsNullOrWhiteSpace(recyclingBin))
            {
                logger.Info("Recycling Bin has not been configured, deleting permanently. {0}", path);

                if (OsInfo.IsWindows)
                {
                    logger.Debug(_diskProvider.GetFileAttributes(path));
                }

                _diskProvider.DeleteFile(path);
                logger.Debug("File has been permanently deleted: {0}", path);
            }

            else
            {
                var fileInfo = new FileInfo(path);
                var destination = Path.Combine(recyclingBin, fileInfo.Name);

                var index = 1;
                while (_diskProvider.FileExists(destination))
                {
                    index++;
                    if (fileInfo.Extension.IsNullOrWhiteSpace())
                    {
                        destination = Path.Combine(recyclingBin, fileInfo.Name + "_" + index);
                    }
                    else
                    {
                        destination = Path.Combine(recyclingBin, Path.GetFileNameWithoutExtension(fileInfo.Name) + "_" + index + fileInfo.Extension);
                    }
                }

                logger.Debug("Moving '{0}' to '{1}'", path, destination);
                _diskProvider.MoveFile(path, destination, true);

                //TODO: Better fix than this for non-Windows?
                if (OsInfo.IsWindows)
                {
                    _diskProvider.FileSetLastWriteTime(destination, DateTime.UtcNow);
                }

                logger.Debug("File has been moved to the recycling bin: {0}", destination);
            }
        }

        public void Empty()
        {
            if (String.IsNullOrWhiteSpace(_configService.RecycleBin))
            {
                logger.Info("Recycle Bin has not been configured, cannot empty.");
                return;
            }

            logger.Info("Removing all items from the recycling bin");

            foreach (var folder in _diskProvider.GetDirectories(_configService.RecycleBin))
            {
                _diskProvider.DeleteFolder(folder, true);
            }

            foreach (var file in _diskProvider.GetFiles(_configService.RecycleBin, SearchOption.TopDirectoryOnly))
            {
                _diskProvider.DeleteFile(file);
            }

            logger.Debug("Recycling Bin has been emptied.");
        }

        public void Cleanup()
        {
            if (String.IsNullOrWhiteSpace(_configService.RecycleBin))
            {
                logger.Info("Recycle Bin has not been configured, cannot cleanup.");
                return;
            }

            logger.Info("Removing items older than 7 days from the recycling bin");

            foreach (var folder in _diskProvider.GetDirectories(_configService.RecycleBin))
            {
                if (_diskProvider.FolderGetLastWrite(folder).AddDays(7) > DateTime.UtcNow)
                {
                    logger.Debug("Folder hasn't expired yet, skipping: {0}", folder);
                    continue;
                }

                _diskProvider.DeleteFolder(folder, true);
            }

            foreach (var file in _diskProvider.GetFiles(_configService.RecycleBin, SearchOption.TopDirectoryOnly))
            {
                if (_diskProvider.FileGetLastWrite(file).AddDays(7) > DateTime.UtcNow)
                {
                    logger.Debug("File hasn't expired yet, skipping: {0}", file);
                    continue;
                }

                _diskProvider.DeleteFile(file);
            }

            logger.Debug("Recycling Bin has been cleaned up.");
        }

        public void HandleAsync(SeriesDeletedEvent message)
        {
            if (message.DeleteFiles)
            {
                if (_diskProvider.FolderExists(message.Series.Path))
                {
                    DeleteFolder(message.Series.Path);
                }
            }
        }

        public void Execute(CleanUpRecycleBinCommand message)
        {
            Cleanup();
        }
    }
}
