#include "il2cpp-config.h"

#if IL2CPP_TARGET_PSP2

#include <kernel.h>
#include "os/Directory.h"
#include "os/ErrorCodes.h"
#include "os/File.h"
#include "utils/DirectoryUtils.h"
#include "utils/PathUtils.h"
#include "utils/StringUtils.h"

namespace il2cpp
{
namespace os
{
    ErrorCode FileErrnoToErrorCodePSP2(int32_t code);
    bool SetCurrentDirectoryInternal(const std::string& path, int* error);
    std::string GetCurrentDirectoryInternal(int* error);
    std::string RemapPath(const std::string& path);

    static void DirectoryGlob(SceUID dir, const std::string& pattern, std::set<std::string>& result)
    {
        if (pattern.empty())
            return;

        std::string matchPattern = il2cpp::utils::CollapseAdjacentStars(pattern);

        SceIoDirent entry;
        while (sceIoDread(dir, &entry) > 0)
        {
            const std::string filename(entry.d_name);

            if (!il2cpp::utils::Match(filename, matchPattern))
                continue;

            result.insert(filename);
        }
    }

    static bool DirectoryGlob(const std::string& directoryPath, const std::string& pattern, std::set<std::string>& result, int* error)
    {
        std::string mappedPath = RemapPath(directoryPath);
        SceUID dir = sceIoDopen(mappedPath.c_str());
        if (dir < 0)
        {
            *error = FileErrnoToErrorCodePSP2(dir);
            return false;
        }

        DirectoryGlob(dir, pattern, result);

        sceIoDclose(dir);
        *error = kErrorCodeSuccess;
        return true;
    }

    std::string Directory::GetCurrent(int *error)
    {
        return GetCurrentDirectoryInternal(error);
    }

    bool Directory::SetCurrent(const std::string& path, int *error)
    {
        return SetCurrentDirectoryInternal(path, error);
    }

    bool Directory::Create(const std::string& path, int *error)
    {
        std::string mappedPath = RemapPath(path);
        int ret = sceIoMkdir(mappedPath.c_str(), SCE_STM_RWU);
        if (ret != SCE_OK)
        {
            *error = FileErrnoToErrorCodePSP2(ret);
            return false;
        }

        *error = kErrorCodeSuccess;
        return true;
    }

    bool Directory::Remove(const std::string& path, int *error)
    {
        std::string mappedPath = RemapPath(path);
        int ret = sceIoRmdir(mappedPath.c_str());
        if (ret != SCE_OK)
        {
            *error = FileErrnoToErrorCodePSP2(ret);
            return false;
        }

        *error = kErrorCodeSuccess;
        return true;
    }

    std::set<std::string> Directory::GetFileSystemEntries(const std::string& path, const std::string& pathWithPattern, int32_t attributes, int32_t mask, int* error)
    {
        const std::string directoryPath(il2cpp::utils::PathUtils::DirectoryName(pathWithPattern));
        const std::string pattern(il2cpp::utils::PathUtils::Basename(pathWithPattern));

        std::set<std::string> globResult;

        if (DirectoryGlob(directoryPath, pattern, globResult, error) == false)
            return std::set<std::string>();

        if (il2cpp::utils::StringUtils::EndsWith(pattern, ".*"))
        {
            /* Special-case the patterns ending in '.*', as
             * windows also matches entries with no extension with
             * this pattern.
             */

            if (DirectoryGlob(directoryPath, pattern.substr(0, pattern.length() - 2), globResult, error) == false)
                return std::set<std::string>();
        }

        std::set<std::string> result;

        for (std::set<std::string>::const_iterator it = globResult.begin(), end = globResult.end(); it != end; ++it)
        {
            const std::string& filename = *it;

            if (filename == "." || filename == "..")
                continue;

            const std::string path(directoryPath + IL2CPP_DIR_SEPARATOR + filename);

            int attributeError;
            const int32_t pathAttributes = static_cast<int32_t>(File::GetFileAttributes(path, &attributeError));

            if (attributeError != kErrorCodeSuccess)
                continue;

            if ((pathAttributes & mask) == attributes)
                result.insert(path);
        }

        *error = kErrorCodeSuccess;
        return result;
    }

    struct FindHandlePSP2
    {
        SceUID fd;
    };

    Directory::FindHandle::FindHandle(const utils::StringView<Il2CppNativeChar>& searchPathWithPattern) :
        osHandle(NULL)
    {
        directoryPath = il2cpp::utils::PathUtils::DirectoryName(searchPathWithPattern);
        pattern = il2cpp::utils::PathUtils::Basename(searchPathWithPattern);

        // Special-case the patterns ending in '.*', as windows also matches entries with no extension with this pattern.
        if (il2cpp::utils::StringUtils::EndsWith(pattern, ".*"))
        {
            pattern.erase(pattern.size() - 1, 1);
            *pattern.rbegin() = '*';
        }

        pattern = il2cpp::utils::CollapseAdjacentStars(pattern);
    }

    Directory::FindHandle::~FindHandle()
    {
        IL2CPP_ASSERT(osHandle == NULL);
    }

    int32_t Directory::FindHandle::CloseOSHandle()
    {
        int32_t result = os::kErrorCodeSuccess;

        if (osHandle)
        {
            FindHandlePSP2* handle = (FindHandlePSP2*)osHandle;
            sceIoDclose(handle->fd);
            free(handle);
            osHandle = NULL;
        }

        return result;
    }

    os::ErrorCode Directory::FindFirstFile(FindHandle* findHandle, const utils::StringView<Il2CppNativeChar>& searchPathWithPattern, Il2CppNativeString* resultFileName, int32_t* resultAttributes)
    {
        std::string mappedPath = RemapPath(findHandle->directoryPath);
        SceUID res = sceIoDopen(mappedPath.c_str());
        if (res < 0)
        {
            return FileErrnoToErrorCodePSP2(res);
        }

        FindHandlePSP2* handle = (FindHandlePSP2*)malloc(sizeof(FindHandlePSP2));
        handle->fd = res;

        findHandle->SetOSHandle(handle);

        return FindNextFile(findHandle, resultFileName, resultAttributes);
    }

    os::ErrorCode Directory::FindNextFile(FindHandle* findHandle, Il2CppNativeString* resultFileName, int32_t* resultAttributes)
    {
        FindHandlePSP2* handle = (FindHandlePSP2*)findHandle->osHandle;

        SceIoDirent entry;
        while (1)
        {
            int res = sceIoDread(handle->fd, &entry);
            if (res <= 0)
            {
                return os::kErrorCodeNoMoreFiles;
            }

            const std::string filename(entry.d_name);

            if (il2cpp::utils::Match(filename, findHandle->pattern))
            {
                const Il2CppNativeString path = utils::PathUtils::Combine(findHandle->directoryPath, filename);

                int attributeError;
                const int32_t pathAttributes = static_cast<int32_t>(File::GetFileAttributes(path, &attributeError));

                if (attributeError == kErrorCodeSuccess)
                {
                    *resultFileName = filename;
                    *resultAttributes = pathAttributes;
                    return os::kErrorCodeSuccess;
                }
            }
        }
    }
}
}

#endif
