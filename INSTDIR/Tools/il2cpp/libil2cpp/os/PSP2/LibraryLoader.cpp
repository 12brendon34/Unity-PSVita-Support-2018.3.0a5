#include "il2cpp-config.h"

#if IL2CPP_TARGET_PSP2

//#define VERBOSE_OUTPUT

#ifdef VERBOSE_OUTPUT
#include <stdio.h>
#endif

#include <string.h>
#include <unordered_map>

#include "il2cpp-metadata.h"
#include "os/LibraryLoader.h"
#include "os/Mutex.h"
#include "vm/PlatformInvoke.h"
#include "dlopen.h"

namespace il2cpp
{
namespace os
{
    static std::vector<std::pair<std::string, void*> > s_NativeDllCache;
    typedef std::vector<std::pair<std::string, void*> >::const_iterator DllCacheIterator;
    os::FastMutex s_NativeDllCacheMutex;

    Il2CppMethodPointer LibraryLoader::GetHardcodedPInvokeDependencyFunctionPointer(const il2cpp::utils::StringView<Il2CppNativeChar>& nativeDynamicLibrary, const il2cpp::utils::StringView<char>& entryPoint)
    {
        return NULL;
    }

    void* LibraryLoader::LoadDynamicLibrary(const utils::StringView<Il2CppNativeChar>& nativeDynamicLibrary)
    {
        return LoadDynamicLibrary(nativeDynamicLibrary, 0);
    }

    void* LibraryLoader::LoadDynamicLibrary(const utils::StringView<Il2CppNativeChar>& nativeDynamicLibrary, int flags)
    {
        std::string module_suprx(nativeDynamicLibrary.Str(), nativeDynamicLibrary.Length());
        module_suprx += ".suprx";

        {
            os::FastAutoLock lock(&s_NativeDllCacheMutex);

            for (DllCacheIterator it = s_NativeDllCache.begin(); it != s_NativeDllCache.end(); it++)
            {
                if (it->first.compare(0, std::string::npos, module_suprx.c_str(), module_suprx.length()) == 0)
                {
                    return it->second;
                }
            }
        }

#ifdef VERBOSE_OUTPUT
        printf("Attempting to load dynamic library: %s\n", module_suprx.c_str());
#endif

        void* module = psp2::dlopen(module_suprx.c_str(), 0, 0, NULL);

        if (module != NULL)
        {
            os::FastAutoLock lock(&s_NativeDllCacheMutex);
            s_NativeDllCache.push_back(std::make_pair(module_suprx, module));
        }

        return module;
    }

    Il2CppMethodPointer LibraryLoader::GetFunctionPointer(void* dynamicLibrary, const PInvokeArguments& pinvokeArgs)
    {
        if (dynamicLibrary == NULL)
            return NULL;

        StringViewAsNullTerminatedStringOf(char, pinvokeArgs.entryPoint, entryPoint);

#ifdef VERBOSE_OUTPUT
        printf("Attempting to load method at entry point: %s\n", entryPoint);
#endif

        return reinterpret_cast<Il2CppMethodPointer>(psp2::dlsym(dynamicLibrary, entryPoint));
    }

    Il2CppMethodPointer LibraryLoader::GetFunctionPointer(void* dynamicLibrary, const char* functionName)
    {
        return reinterpret_cast<Il2CppMethodPointer>(psp2::dlsym(dynamicLibrary, functionName));
    }

    void LibraryLoader::CleanupLoadedLibraries()
    {
        for (DllCacheIterator it = s_NativeDllCache.begin(); it != s_NativeDllCache.end(); it++)
        {
            psp2::dlclose(it->second);
        }
    }

    bool LibraryLoader::CloseLoadedLibrary(void*& dynamicLibrary)
    {
        if (dynamicLibrary == NULL)
            return false;

        os::FastAutoLock lock(&s_NativeDllCacheMutex);
        for (DllCacheIterator it = s_NativeDllCache.begin(); it != s_NativeDllCache.end(); it++)
        {
            if (it->second == dynamicLibrary)
            {
                psp2::dlclose(it->second);
                s_NativeDllCache.erase(it);
                return true;
            }
        }
        return false;
    }
}
}

#endif
