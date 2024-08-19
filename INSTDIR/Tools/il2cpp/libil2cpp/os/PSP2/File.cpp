/*
    The PS Vita il2cpp File implementation.

    Sony don't supply a posix api for Vita, what they do supply is almost, but not quite, entirely
    unlike posix; hence some similarities to the posix implementation but too many differences to allow
    us to use the same code.
*/

#include "il2cpp-config.h"

#if IL2CPP_TARGET_PSP2

#include <kernel.h>
#include <rtc.h>
#include <string>
#include "os/File.h"
#include "utils/PathUtils.h"
#include "vm/Exception.h"

#define INVALID_FILE_HANDLE     ((FileHandle*)-1)
#define INVALID_FILE_ATTRIBUTES ((UnityPalFileAttributes)((uint32_t)-1))
#pragma diag_suppress=1787      // Suppress warning 1787: "<value> is not a valid value for the enumeration type", needed for INVALID_FILE_ATTRIBUTES.

namespace il2cpp
{
namespace os
{
    // The current directory, since this will likely get passed to and from .NET we will keep this as a Unix path.
    static std::string s_CurrentDirectory = "/app0/Media";

    struct FileHandle
    {
        SceUID fd;
        std::string path;
        FileType type;
        FileAccess accessMode;
        FileOptions options;
    };

    /*
        Remap a Unix path to a vita path.

        The .NET class libs handle Unix style paths, e.g. "/mounted_device_or_root_dir/file"
        or single character windows style device specs, e.g. "C:/somefile".

        Vita device specs are a string followed by a colon, e.g. "app0:/file".

        So, to avoid lots of risky changes to the class libs we handle remapping Unix paths
        to vita paths at the point where we call into the low-level IO functions.
        e.g. "/app0/Media/Folder" becomes "app0:/Media/Folder"

        We also remap absolute paths which don't specify a Vita device to a path within app0:
        e.g. "/root/file" becomes "app0:/root/file"

        Finally, if the path is relative then it will be prefixed with the current directory.
    */
    std::string RemapPath(const std::string& path)
    {
        // Has it already got a Vita device specifier?
        size_t colon = path.find_first_of(':', 1);
        if (colon != std::string::npos)
        {
            // If the path begins with a slash and has a colon then remove up to and including the last slash before the colon.
            // This is required as the .NET class libs don't know how to recognize rooted paths on Vita, e.g. "photo0:"
            // so they decide that they are relative and prefix with the current directory (which this bit of code removes).
            if (path[0] == '/')
            {
                size_t slash = path.rfind('/', colon);
                if (slash != std::string::npos)
                {
                    return path.substr(slash + 1, path.length());
                }
            }

            return path;
        }

        // Iterate twice, first iteration assumes a fully qualified Unix path, second is for current directory + relative path.
        std::string currPath = path;
        for (int i = 0; i < 2; i++)
        {
            // If the path starts with a slash it is absolute, e.g. "/mounted_device_or_root_dir/file"...
            if (currPath[0] == '/')
            {
                // Is the root a valid Vita device?
                // "app0" is the most commonly used device so keep it first in the array.
                static const std::string vitaDevices[] = {"/app0", "/addcont0", "/savedata0", "/addcont1", "/savedata1", "/photo0", "/music0", "/video0"};
                for (int i = 0; i < sizeof(vitaDevices) / sizeof(std::string); i++)
                {
                    if (currPath.substr(0, vitaDevices[i].length()) == vitaDevices[i])
                    {
                        size_t secondSlash = currPath.find_first_of('/', 1);
                        if (secondSlash != std::string::npos)
                        {
                            return currPath.substr(1, secondSlash - 1) + ":" + currPath.substr(secondSlash);
                        }
                        else
                        {
                            return vitaDevices[i].substr(1) + ":";
                        }
                    }
                }

                // The root wasn't a Vita device so map it to a directory in "app0:"
                return std::string("app0:") + (currPath.length() > 1 ? currPath : "");
            }

            // No leading slash so assume it's a relative path and prefix with the current directory for the second iteration.
            currPath = s_CurrentDirectory + (currPath.length() ? "/" + currPath : "");
        }

        // The path is probably bad, just return it and let the file system report errors.
        return path;
    }

    static bool IsValidHandle(FileHandle* handle, int* error)
    {
        if (!handle || handle == INVALID_FILE_HANDLE)
        {
            if (error)
            {
                *error = kErrorCodeInvalidHandle;
            }
            return false;
        }

        return true;
    }

    ErrorCode FileErrnoToErrorCodePSP2(int32_t code)
    {
        ErrorCode ret;

        switch (code)
        {
            case SCE_ERROR_ERRNO_EACCES:
            case SCE_ERROR_ERRNO_EPERM:
            case SCE_ERROR_ERRNO_EROFS:
                ret = kErrorCodeAccessDenied;
                break;

            case SCE_ERROR_ERRNO_EAGAIN:
                ret = kErrorCodeSharingViolation;
                break;

            case SCE_ERROR_ERRNO_EBUSY:
                ret = kErrorCodeLockViolation;
                break;

            case SCE_ERROR_ERRNO_EEXIST:
                ret = kErrorCodeFileExists;
                break;

            case SCE_ERROR_ERRNO_EINVAL:
            case SCE_ERROR_ERRNO_ESPIPE:
                ret = kErrorSeek;
                break;

            case SCE_ERROR_ERRNO_EISDIR:
                ret = kErrorCodeCannotMake;
                break;

            case SCE_ERROR_ERRNO_ENFILE:
            case SCE_ERROR_ERRNO_EMFILE:
                ret = kErrorCodeTooManyOpenFiles;
                break;

            case SCE_ERROR_ERRNO_ENOENT:
            case SCE_ERROR_ERRNO_ENOTDIR:
                ret = kErrorCodeFileNotFound;
                break;

            case SCE_ERROR_ERRNO_ENODEV:
                ret = kErrorDeviceNotConnected;
                break;

            case SCE_ERROR_ERRNO_ENOSPC:
                ret = kErrorCodeHandleDiskFull;
                break;

            case SCE_ERROR_ERRNO_ENOTEMPTY:
                ret = kErrorCodeDirNotEmpty;
                break;

            case SCE_ERROR_ERRNO_ENOEXEC:
                ret = kErrorBadFormat;
                break;

            case SCE_ERROR_ERRNO_ENAMETOOLONG:
                ret = kErrorCodeFileNameExcedRange;
                break;

            case SCE_ERROR_ERRNO_EINPROGRESS:
                ret = kErrorIoPending;
                break;

            case SCE_ERROR_ERRNO_ENOSYS:
                ret = kErrorNotSupported;
                break;

            case SCE_ERROR_ERRNO_EBADF:
                ret = kErrorCodeInvalidHandle;
                break;

            case SCE_ERROR_ERRNO_EIO:
                ret = kErrorCodeInvalidHandle;
                break;

            case SCE_ERROR_ERRNO_EINTR:
                ret = kErrorIoPending;
                break;

            case SCE_ERROR_ERRNO_EPIPE:
                ret = kErrorCodeWriteFault;
                break;

            default:
                ret = kErrorCodeGenFailure;
                break;
        }

        return ret;
    }

    static int ConvertFlags(int fileaccess, int createmode)
    {
        int flags;

        switch (fileaccess)
        {
            case kFileAccessRead:
                flags = SCE_O_RDONLY;
                break;

            case kFileAccessWrite:
                flags = SCE_O_WRONLY;
                break;

            case kFileAccessReadWrite:
                flags = SCE_O_RDWR;
                break;

            default:
                flags = 0;
                break;
        }

        switch (createmode)
        {
            case kFileModeCreateNew:
                flags |= SCE_O_CREAT | SCE_O_EXCL;
                break;

            case kFileModeCreate:
                flags |= SCE_O_CREAT | SCE_O_TRUNC;
                break;

            case kFileModeOpen:
                break;

            case kFileModeOpenOrCreate:
            case kFileModeAppend:
                flags |= SCE_O_CREAT;
                break;

            case kFileModeTruncate:
                flags |= SCE_O_TRUNC;
                break;
            default:
                flags = 0;
                break;
        }

        return flags;
    }

    static bool IsDirectory(const std::string& path, int* sceError)
    {
        std::string mappedPath = RemapPath(path);
        SceIoStat statbuf;
        int ret = sceIoGetstat(mappedPath.c_str(), &statbuf);

        if (sceError)
        {
            *sceError = ret;
        }

        if (ret != SCE_OK)
        {
            return false;
        }

        return SCE_STM_ISDIR(statbuf.st_mode);
    }

    bool SetCurrentDirectoryInternal(const std::string& path, int* error)
    {
        int sceError = SCE_OK;
        if (IsDirectory(path, &sceError))
        {
            *error = FileErrnoToErrorCodePSP2(sceError);
            return false;
        }

        s_CurrentDirectory = path;

        *error = kErrorCodeSuccess;
        return true;
    }

    std::string GetCurrentDirectoryInternal(int* error)
    {
        *error = kErrorCodeSuccess;
        return s_CurrentDirectory;
    }

    static int64_t DateTimeToTicks(const SceDateTime& dateTime)
    {
        SceUInt64 rtcTime = 0;
        sceRtcGetWin32FileTime(&dateTime, &rtcTime);
        return rtcTime;
    }

    static void TicksToDateTime(int64_t ticks, SceDateTime& dateTime)
    {
        sceRtcSetWin32FileTime(&dateTime, (SceUInt64)ticks);
    }

    static bool FileExists(const std::string& path)
    {
        std::string mappedPath = RemapPath(path);
        SceIoStat statbuf;
        return sceIoGetstat(mappedPath.c_str(), &statbuf) == SCE_OK;
    }

    static bool InternalCopyFile(SceUID srcFd, SceUID destFd, const SceIoStat& srcStat, int* error)
    {
        const SceSSize bufferSize = srcStat.st_size > 65536 ? 65536 : srcStat.st_size;
        char *buffer = new char[bufferSize];

        SceSSize readBytes;

        while ((readBytes = sceIoRead(srcFd, buffer, bufferSize)) > 0)
        {
            char* writeBuffer = buffer;
            SceSSize writeBytes = readBytes;

            while (writeBytes > 0)
            {
                const SceSSize writtenBytes = sceIoWrite(destFd, writeBuffer, writeBytes);
                if (writtenBytes < 0)
                {
                    if (writtenBytes == SCE_ERROR_ERRNO_EINTR)
                        continue;

                    delete buffer;

                    *error = FileErrnoToErrorCodePSP2(writtenBytes);
                    return false;
                }

                writeBytes -= writtenBytes;
                writeBuffer += writtenBytes;
            }
        }

        delete buffer;

        if (readBytes < 0)
        {
            *error = FileErrnoToErrorCodePSP2(readBytes);
            return false;
        }

        assert(readBytes == 0);

        return true;
    }

    static int InternalRename(const std::string& sourceName, const std::string& destName)
    {
        if (!FileExists(sourceName))
        {
            return SCE_ERROR_ERRNO_ENOENT;
        }

        // sceIoRename() fails if a file with destName already exists, so to emulate posix
        // behavior (which would just replace the destination file) we need to handle this.
        if (FileExists(destName))
        {
            // Check that both are the same type, e.g. file or directory.
            if (IsDirectory(sourceName, NULL) != IsDirectory(destName, NULL))
            {
                return SCE_ERROR_ERRNO_EACCES;
            }

            // Remove the existing file (or directory).
            int ret = sceIoRemove(destName.c_str());
            if (ret != SCE_OK)
            {
                return ret;
            }
        }

        return sceIoRename(sourceName.c_str(), destName.c_str());
    }

    static UnityPalFileAttributes StatToFileAttribute(const std::string& path, SceIoStat& pathStat)
    {
        uint32_t fileAttributes = 0;

        const std::string filename(il2cpp::utils::PathUtils::Basename(path));

        if (SCE_STM_ISDIR(pathStat.st_mode))
        {
            fileAttributes |= kFileAttributeDirectory;

            if (filename[0] == '.')
            {
                fileAttributes |= kFileAttributeHidden;
            }
        }
        else
        {
            if (filename[0] == '.')
            {
                fileAttributes |= kFileAttributeHidden;
            }
            else
            {
                fileAttributes |= kFileAttributeNormal;
            }
        }

        if ((pathStat.st_mode & SCE_STM_RWU) != SCE_STM_RWU)
        {
            fileAttributes |= kFileAttributeReadOnly;
        }

        return (UnityPalFileAttributes)fileAttributes;
    }

    bool File::Isatty(FileHandle* fileHandle)
    {
        return (GetFileType(fileHandle) == kFileTypeChar);
    }

    FileHandle* File::GetStdInput()
    {
        static FileHandle* s_handle = NULL;
        if (s_handle)
        {
            return s_handle;
        }
        s_handle = new FileHandle();
        s_handle->path = "tty0:";
        s_handle->fd = sceIoOpen(s_handle->path.c_str(), SCE_O_RDONLY, SCE_STM_RWU);
        s_handle->type = kFileTypeChar;
        s_handle->accessMode = kFileAccessRead;
        s_handle->options = kFileOptionsNone;
        return s_handle;
    }

    FileHandle* File::GetStdOutput()
    {
        static FileHandle* s_handle = NULL;
        if (s_handle)
        {
            return s_handle;
        }
        s_handle = new FileHandle();
        s_handle->path = "tty0:";
        s_handle->fd = sceIoOpen(s_handle->path.c_str(), SCE_O_RDWR, SCE_STM_RWU);
        s_handle->type = kFileTypeChar;
        s_handle->accessMode = kFileAccessReadWrite;
        s_handle->options = kFileOptionsNone;
        return s_handle;
    }

    FileHandle* File::GetStdError()
    {
        static FileHandle* s_handle = NULL;
        if (s_handle)
        {
            return s_handle;
        }
        s_handle = new FileHandle();
        s_handle->path = "tty0:";
        s_handle->fd = sceIoOpen(s_handle->path.c_str(), SCE_O_RDWR, SCE_STM_RWU);
        s_handle->type = kFileTypeChar;
        s_handle->accessMode = kFileAccessReadWrite;
        s_handle->options = kFileOptionsNone;
        return s_handle;
    }

    bool File::CreatePipe(FileHandle** read_handle, FileHandle** write_handle)
    {
        // Vita does not have any equivalent for pipe().
        NOT_SUPPORTED_IL2CPP(File::CreatePipe, "This call is not supported for PS Vita.");
        return false;
    }

    bool File::CreatePipe(FileHandle** read_handle, FileHandle** write_handle, int* error)
    {
        // Vita does not have any equivalent for pipe().
        NOT_SUPPORTED_IL2CPP(File::CreatePipe, "This call is not supported for PS Vita.");
        return false;
    }

    bool File::DuplicateHandle(FileHandle* source_process_handle, FileHandle* source_handle, FileHandle* target_process_handle,
        FileHandle** target_handle, int access, int inherit, int options, int* error)
    {
        NOT_SUPPORTED_IL2CPP(File::DuplicateHandle, "This call is not supported for PS Vita.");
        return false;
    }

    FileType File::GetFileType(FileHandle* handle)
    {
        if (!IsValidHandle(handle, NULL))
        {
            return kFileTypeUnknown;
        }
        return handle->type;
    }

    UnityPalFileAttributes File::GetFileAttributes(const std::string& path, int* error)
    {
        std::string mappedPath = RemapPath(path);
        SceIoStat statbuf;
        const int ret = sceIoGetstat(mappedPath.c_str(), &statbuf);
        if (ret != SCE_OK)
        {
            *error = FileErrnoToErrorCodePSP2(ret);
            return INVALID_FILE_ATTRIBUTES;
        }

        *error = kErrorCodeSuccess;
        return StatToFileAttribute(path, statbuf);
    }

    bool File::SetFileAttributes(const std::string& path, UnityPalFileAttributes attributes, int* error)
    {
        std::string mappedPath = RemapPath(path);
        SceIoStat statbuf;
        int ret = sceIoGetstat(mappedPath.c_str(), &statbuf);
        if (ret != SCE_OK)
        {
            *error = FileErrnoToErrorCodePSP2(ret);
            return false;
        }

        if (attributes & kFileAttributeReadOnly)
        {
            statbuf.st_mode = (statbuf.st_mode & ~SCE_STM_RWU) | SCE_STM_RU;
        }
        else
        {
            statbuf.st_mode |= SCE_STM_RWU;
        }

        ret = sceIoChstat(mappedPath.c_str(), &statbuf, SCE_CST_MODE);
        if (ret != SCE_OK)
        {
            *error = FileErrnoToErrorCodePSP2(ret);
            return false;
        }

        *error = kErrorCodeSuccess;
        return true;
    }

    bool File::GetFileStat(const std::string& path, FileStat* stat, int* error)
    {
        std::string mappedPath = RemapPath(path);
        SceIoStat statbuf;
        const int ret = sceIoGetstat(mappedPath.c_str(), &statbuf);
        if (ret != SCE_OK)
        {
            *error = FileErrnoToErrorCodePSP2(ret);
            return false;
        }

        stat->length = (stat->attributes & kFileAttributeDirectory) > 0 ? 0 : statbuf.st_size;
        stat->attributes = StatToFileAttribute(path, statbuf);
        stat->creation_time = DateTimeToTicks(statbuf.st_ctime);
        stat->last_write_time = DateTimeToTicks(statbuf.st_mtime);
        stat->last_access_time = DateTimeToTicks(statbuf.st_atime);

        *error = kErrorCodeSuccess;
        return true;
    }

    bool File::CopyFile(const std::string& src, const std::string& dest, bool overwrite, int* error)
    {
        std::string mappedSrc = RemapPath(src);
        std::string mappedDst = RemapPath(dest);
        SceUID srcFd = sceIoOpen(mappedSrc.c_str(), SCE_O_RDONLY, SCE_STM_RWU);
        if (srcFd < 0)
        {
            *error = FileErrnoToErrorCodePSP2(srcFd);
            return false;
        }

        SceIoStat srcStat;
        int ret = sceIoGetstatByFd(srcFd, &srcStat);
        if (ret != SCE_OK)
        {
            *error = FileErrnoToErrorCodePSP2(ret);
            return false;
        }

        SceUID dstFd;
        SceIoMode dstMode = srcStat.st_mode & SCE_STM_RWU;
        if (!overwrite)
        {
            dstFd = sceIoOpen(mappedDst.c_str(), SCE_O_WRONLY | SCE_O_CREAT | SCE_O_EXCL, dstMode);
        }
        else
        {
            dstFd = sceIoOpen(mappedDst.c_str(), SCE_O_WRONLY | SCE_O_TRUNC, dstMode);

            if (dstFd < 0)
            {
                // File does not already exist so retry with SCE_O_CREAT.
                dstFd = sceIoOpen(mappedDst.c_str(), SCE_O_WRONLY | SCE_O_CREAT | SCE_O_TRUNC, dstMode);
            }
            else
            {
                *error = kErrorCodeAlreadyExists; // Apparently this error is set if we overwrite the dest file
            }
        }

        if (dstFd < 0)
        {
            *error = FileErrnoToErrorCodePSP2(srcFd);
            return false;
        }

        ret = InternalCopyFile(srcFd, dstFd, srcStat, error);

        sceIoClose(srcFd);
        sceIoClose(dstFd);

        return ret;
    }

    bool File::MoveFile(const std::string& src, const std::string& dest, int* error)
    {
        std::string mappedSrc = RemapPath(src);
        std::string mappedDst = RemapPath(dest);
        SceIoStat srcStat;
        SceIoStat destStat;

        int ret = sceIoGetstat(mappedSrc.c_str(), &srcStat);
        if (ret != SCE_OK)
        {
            *error = FileErrnoToErrorCodePSP2(ret);
            return false;
        }

        // In C# land we check for the existence of src, but not for dest.
        // We check it here and return the failure if dest exists.
        //
        // On Vita we can't quickly compare the two files to see if they are
        // identical as vita does not have st_dev or st_ino.
        ret = sceIoGetstat(mappedDst.c_str(), &destStat);
        if (ret == SCE_OK) // dest exists
        {
            *error = kErrorCodeAlreadyExists;
            return false;
        }

        ret = InternalRename(mappedSrc.c_str(), mappedDst.c_str());
        if (ret != SCE_OK)
        {
            if (ret == SCE_ERROR_ERRNO_ENODEV)
            {
                if (SCE_STM_ISDIR(srcStat.st_mode))
                {
                    *error = kErrorCodeNotSameDevice;
                    return false;
                }

                if (!CopyFile(mappedSrc, mappedDst, true, error))
                {
                    return false;
                }

                return DeleteFile(mappedSrc, error);
            }
            else
            {
                *error = FileErrnoToErrorCodePSP2(ret);
                return false;
            }
        }

        *error = kErrorCodeSuccess;
        return true;
    }

    bool File::DeleteFile(const std::string& path, int* error)
    {
        std::string mappedPath = RemapPath(path);
        const UnityPalFileAttributes attributes = GetFileAttributes(mappedPath, error);

        if (*error != kErrorCodeSuccess)
        {
            return false;
        }

        if (attributes & kFileAttributeReadOnly)
        {
            *error = kErrorCodeAccessDenied;
            return false;
        }

        const int ret = sceIoRemove(mappedPath.c_str());
        if (ret != SCE_OK)
        {
            *error = FileErrnoToErrorCodePSP2(ret);
            return false;
        }

        *error = kErrorCodeSuccess;
        return true;
    }

    bool File::ReplaceFile(const std::string& sourceFileName, const std::string& destinationFileName, const std::string& destinationBackupFileName, bool ignoreMetadataErrors, int* error)
    {
        std::string mappedSrc = RemapPath(sourceFileName);
        std::string mappedDest = RemapPath(destinationFileName);
        std::string mappedDestBackup = RemapPath(destinationBackupFileName);
        std::string backupBackup = "";
        const bool backupFile = !mappedDestBackup.empty();
        int ret;

        // Backup the destination file (if backup enabled).
        if (backupFile)
        {
            // Rename the existing backup file so that we can restore it if something fails.
            if (FileExists(mappedDestBackup))
            {
                backupBackup = mappedDestBackup + ".bakinternal";
                ret = InternalRename(mappedDestBackup, backupBackup);
                if (ret != SCE_OK)
                {
                    *error = FileErrnoToErrorCodePSP2(ret);
                    return false;
                }
            }

            // Backup the destination.
            ret = InternalRename(mappedDest.c_str(), mappedDestBackup.c_str());
            if (ret != SCE_OK)
            {
                if (!backupBackup.empty())
                {
                    // Restore the original back up file.
                    ret = InternalRename(backupBackup, mappedDestBackup);
                }
                *error = FileErrnoToErrorCodePSP2(ret);
                return false;
            }
        }

        // Rename source->dest
        ret = InternalRename(mappedSrc.c_str(), mappedDest.c_str());
        if (ret != SCE_OK)
        {
            if (!backupBackup.empty())
            {
                // Restore the original back up file.
                InternalRename(backupBackup, mappedDestBackup);
            }
            *error = FileErrnoToErrorCodePSP2(ret);
            return false;
        }

        if (!backupBackup.empty())
        {
            // Delete the internal backup file.
            sceIoRemove(backupBackup.c_str());
        }

        *error = kErrorCodeSuccess;
        return true;
    }

    FileHandle* File::Open(const std::string& path, int mode, int accessMode, int shareMode, int options, int* error)
    {
        std::string mappedPath = RemapPath(path);
        int flags = ConvertFlags(accessMode, mode);
        SceUID fd = sceIoOpen(mappedPath.c_str(), flags, SCE_STM_RWU);
        if (fd < 0)
        {
            *error = FileErrnoToErrorCodePSP2(fd);
            return INVALID_FILE_HANDLE;
        }

        FileHandle* fileHandle = new FileHandle();
        fileHandle->fd = fd;
        fileHandle->path = path;
        fileHandle->type = kFileTypeDisk;
        fileHandle->options = (FileOptions)options;
        fileHandle->accessMode = (FileAccess)accessMode;

        *error = kErrorCodeSuccess;
        return fileHandle;
    }

    bool File::Close(FileHandle* handle, int* error)
    {
        if (!IsValidHandle(handle, error))
        {
            return false;
        }

        int errCode = sceIoClose(handle->fd);
        if (errCode != SCE_OK)
        {
            *error = FileErrnoToErrorCodePSP2(errCode);
            return false;
        }

        if (handle->type == kFileTypeDisk && handle->options & kFileOptionsDeleteOnClose)
        {
            sceIoRemove(handle->path.c_str());
        }

        delete handle;
        *error = kErrorCodeSuccess;
        return true;
    }

    bool File::SetFileTime(FileHandle* handle, int64_t creation_time, int64_t last_access_time, int64_t last_write_time, int* error)
    {
        if (!IsValidHandle(handle, error))
        {
            return false;
        }

        if (handle->type != kFileTypeDisk)
        {
            *error = kErrorCodeInvalidHandle;
            return false;
        }

        SceIoStat statbuf;
        TicksToDateTime(creation_time, statbuf.st_ctime);
        TicksToDateTime(last_write_time, statbuf.st_mtime);
        TicksToDateTime(last_access_time, statbuf.st_atime);

        int ret = sceIoChstatByFd(handle->fd, &statbuf, SCE_CST_CT | SCE_CST_MT | SCE_CST_AT);
        if (ret != SCE_OK)
        {
            *error = FileErrnoToErrorCodePSP2(ret);
            return false;
        }

        *error = kErrorCodeSuccess;
        return true;
    }

    int64_t File::GetLength(FileHandle* handle, int* error)
    {
        if (!IsValidHandle(handle, error))
        {
            return -1;
        }

        if (handle->type != kFileTypeDisk)
        {
            *error = kErrorCodeInvalidHandle;
            return false;
        }

        SceIoStat statbuf;
        const int ret = sceIoGetstatByFd(handle->fd, &statbuf);
        if (ret != SCE_OK)
        {
            *error = FileErrnoToErrorCodePSP2(ret);
            return -1;
        }

        *error = kErrorCodeSuccess;
        return statbuf.st_size;
    }

    bool File::SetLength(FileHandle* handle, int64_t length, int* error)
    {
        if (!IsValidHandle(handle, error))
        {
            return false;
        }

        if (handle->type != kFileTypeDisk)
        {
            *error = kErrorCodeInvalidHandle;
            return false;
        }

        SceIoStat statbuf;
        statbuf.st_size = length;
        int ret = sceIoChstatByFd(handle->fd, &statbuf, SCE_CST_SIZE);
        if (ret != SCE_OK)
        {
            *error = FileErrnoToErrorCodePSP2(ret);
            return false;
        }

        *error = kErrorCodeSuccess;
        return true;
    }

    int64_t File::Seek(FileHandle* handle, int64_t offset, int origin, int* error)
    {
        if (!IsValidHandle(handle, error))
        {
            return -1;
        }

        if (handle->type != kFileTypeDisk)
        {
            *error = kErrorCodeInvalidHandle;
            return false;
        }

        int whence;

        switch (origin)
        {
            case kFileSeekOriginBegin:
                whence = SCE_SEEK_SET;
                break;

            case kFileSeekOriginCurrent:
                whence = SCE_SEEK_CUR;
                break;

            case kFileSeekOriginEnd:
                whence = SCE_SEEK_END;
                break;

            default:
                *error = kErrorCodeInvalidParameter;
                return -1;
        }

        const SceOff position = sceIoLseek(handle->fd, offset, whence);
        if (position < 0)
        {
            *error = FileErrnoToErrorCodePSP2((int)position);
            return -1;
        }

        *error = kErrorCodeSuccess;
        return position;
    }

    int File::Read(FileHandle* handle, char* dest, int count, int* error)
    {
        if (!IsValidHandle(handle, error))
        {
            return 0;
        }

        if ((handle->accessMode & kFileAccessRead) == 0)
        {
            *error = kErrorCodeAccessDenied;
            return 0;
        }

        SceSSize bytesRead = sceIoRead(handle->fd, dest, count);
        if (bytesRead < 0)
        {
            *error = FileErrnoToErrorCodePSP2(bytesRead);
            return 0;
        }

        *error = kErrorCodeSuccess;
        return bytesRead;
    }

    int32_t File::Write(FileHandle* handle, const char* buffer, int count, int* error)
    {
        if (!IsValidHandle(handle, error))
        {
            return 0;
        }

        if ((handle->accessMode & kFileAccessWrite) == 0)
        {
            *error = kErrorCodeAccessDenied;
            return 0;
        }

        SceSSize bytesWritten = sceIoWrite(handle->fd, buffer, count);
        if (bytesWritten < 0)
        {
            *error = FileErrnoToErrorCodePSP2(bytesWritten);
            return 0;
        }

        *error = kErrorCodeSuccess;
        return bytesWritten;
    }

    bool File::Flush(FileHandle* handle, int* error)
    {
        if (!IsValidHandle(handle, error))
        {
            return false;
        }

        if (handle->type != kFileTypeDisk)
        {
            *error = kErrorCodeInvalidHandle;
            return false;
        }

        int ret = sceIoSyncByFd(handle->fd, 0);
        if (ret != SCE_OK)
        {
            *error = FileErrnoToErrorCodePSP2(ret);
            return false;
        }

        *error = kErrorCodeSuccess;
        return true;
    }

    void File::Lock(FileHandle* handle,  int64_t position, int64_t length, int* error)
    {
        // No API for locking a handle on Vita.
        NOT_SUPPORTED_IL2CPP(File::Lock, "This call is not supported for PS Vita.");
        *error = kErrorCallNotImplemented;
    }

    void File::Unlock(FileHandle* handle,  int64_t position, int64_t length, int* error)
    {
        // No API for unlocking a handle on Vita.
        NOT_SUPPORTED_IL2CPP(File::Unlock, "This call is not supported for PS Vita.");
        *error = kErrorCallNotImplemented;
    }

    bool File::IsExecutable(const std::string& path)
    {
        // Vita does not have any any equivalient for access() to determine if a file is executable.
        NOT_SUPPORTED_IL2CPP(File::IsExecutable, "This call is not supported for PS Vita.");
        return false;
    }
}
}
#endif
