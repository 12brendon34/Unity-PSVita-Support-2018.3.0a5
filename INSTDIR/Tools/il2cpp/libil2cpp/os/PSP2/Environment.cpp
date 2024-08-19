#include "il2cpp-config.h"

#if IL2CPP_TARGET_PSP2

#include "il2cpp-class-internals.h"
#include "os/Environment.h"
#include "vm/Exception.h"
#include "il2cpp-api.h"
#include <cassert>
#include <stdlib.h>
#include <map>
#include "os/PSP2/SocketImpl_Helpers.h"
#include "os/PSP2/Environment.h"

typedef std::map<std::string, std::string> EnvironmentVariablesMap;
EnvironmentVariablesMap *s_env = NULL;

extern "C" __declspec(dllexport) int  setenv(const char* envname, const char *envval, int overwrite)
{
    if (s_env == NULL)
    {
        s_env = new EnvironmentVariablesMap();
    }
    if ((envval == NULL) || (envval[0] == 0))
    {
        s_env->erase(envname);
    }
    else
    {
        if (overwrite == 0)
        {
            EnvironmentVariablesMap::iterator it = s_env->find(envname);
            if (it != s_env->end())
            {
                return 0;
            }                                           // if we find it, don't overwrite
        }
        s_env->insert(std::pair<std::string, std::string>(envname, envval));
    }
    return 0;
}

extern "C" __declspec(dllexport) const char *getenv(const char* envname)
{
    if (s_env == NULL)
    {
        return NULL;
    }
    EnvironmentVariablesMap::iterator it = s_env->find(envname);
    if (it == s_env->end())
    {
        return NULL;
    }
    return it->first.c_str();
}


extern "C" __declspec(dllexport) int unsetenv(const char* envname)
{
    if (s_env != NULL)
        s_env->erase(envname);
    return 0;
}

namespace il2cpp
{
namespace os
{
    std::string Environment::GetMachineName()
    {
        char hostname[256];

        if (gethostname(hostname, sizeof(hostname)) != 0)
            return std::string();

        return std::string(hostname);
    }

    int32_t Environment::GetProcessorCount()
    {
        return 3; // Vita has 4 cores, but only 3 available for apps, the OS reserves the 4th.
    }

    std::string Environment::GetOsVersionString()
    {
        return "0.0.0.0";
    }

    std::string Environment::GetOsUserName()
    {
        const std::string username(GetEnvironmentVariable("USER"));
        return username.empty() ? "PSP2" : username;
    }

    std::string Environment::GetEnvironmentVariable(const std::string& name)
    {
        const char* variable = getenv(name.c_str());
        return variable ? std::string(variable) : std::string();
    }

    void Environment::SetEnvironmentVariable(const std::string& name, const std::string& value)
    {
        if (value.empty())
        {
            unsetenv(name.c_str());
        }
        else
        {
            setenv(name.c_str(), value.c_str(), 1); // 1 means overwrite
        }
    }

    std::vector<std::string> Environment::GetEnvironmentVariableNames()
    {
        std::vector<std::string> result;
        if (s_env != NULL)
        {
            for (EnvironmentVariablesMap::iterator it = s_env->begin(); it != s_env->end(); ++it)
            {
                std::string setting = std::string(it->first) + std::string("=") + std::string(it->second);
                result.push_back(setting);
            }
        }
        return result;
    }

    std::string Environment::GetHomeDirectory()
    {
        static std::string homeDirectory(GetEnvironmentVariable("HOME"));
        return homeDirectory.empty() ? "/app0" : homeDirectory;
    }

    std::vector<std::string> Environment::GetLogicalDrives()
    {
        std::vector<std::string> result;
        result.push_back("/");
        return result;
    }

    void Environment::Exit(int result)
    {
        il2cpp_shutdown();
        exit(result);
    }

    void Environment::Abort()
    {
        abort();
    }

    std::string Environment::GetWindowsFolderPath(int folder)
    {
        // This should only be called on Windows.
        NOT_SUPPORTED_IL2CPP(Environment::GetWindowsFolderPath, "This call is not supported for PS Vita.");
        return std::string();
    }

#if NET_4_0

    bool Environment::Is64BitOs()
    {
        return false;
    }

#endif
}
}

#endif
