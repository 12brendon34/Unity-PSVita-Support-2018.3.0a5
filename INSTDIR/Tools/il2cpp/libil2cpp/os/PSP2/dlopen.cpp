#include "il2cpp-config.h"

#if IL2CPP_TARGET_PSP2

#include <string>
#include <map>
#include <kernel.h>
#include <np.h>
#include <stdio.h>

#include "dlopen.h"

typedef int (*EXTERNFUNCTION)();

struct FunctionMap { const char* name; EXTERNFUNCTION address; };

typedef std::map<std::pair<std::string, std::string>, EXTERNFUNCTION> ModulesMap;
static ModulesMap s_ModulesMap;

typedef std::map<SceUID, std::string> HandlesMap;
static HandlesMap s_HandlesMap;

static size_t s_ModuleArgsSize = 0;
static void* s_ModuleArgsMem = NULL;

// Sets the command line arg size and memory pointer for passing into sceKernelLoadStartModule()
IL2CPP_EXPORT void il2cpp_set_dlopen_sce_commandline_arguments(size_t args, void* argp)
{
    s_ModuleArgsSize = args;
    s_ModuleArgsMem = argp;
}

// Required by the native plugin boot strap prx
extern "C" __declspec(dllexport) void RegisterModule(const char* module_name, void* module_exports, size_t module_exports_count, SceSize sz, void* arg)
{
    FunctionMap* exports = (FunctionMap*)module_exports;
    SceUID mod = *(SceUID*)arg;
    for (size_t i = 0; i != module_exports_count; i++)
    {
        EXTERNFUNCTION func = exports[i].address;
        s_ModulesMap.insert(std::make_pair(std::make_pair(std::string(module_name), std::string(exports[i].name)), func));
        s_HandlesMap[mod] = std::string(module_name);
    }
}

namespace il2cpp
{
namespace os
{
namespace psp2
{
    typedef std::map<std::string, SceUID> BootstapHandlesMap;
    static BootstapHandlesMap g_BootstrapHandlesMap;

    static const char* s_lastError = NULL;

    static void set_last_error(const char* lastError)
    {
        s_lastError = lastError;
    }

    static const char* get_last_error()
    {
        return s_lastError;
    }

    void* dlopen(const char *module_path, int flags, int sz, void* args)
    {
        int modres;
        SceUID mod;

        set_last_error(NULL);

        std::string full_path;
        std::string module = std::string(module_path);
        size_t pos = module.find_first_of(":/\\");
        if (pos != std::string::npos && module[pos] == ':' || pos == 0)
            full_path = module_path;
        else
            full_path = std::string("app0:/Media/Plugins/") + module;

        if ((mod = sceKernelLoadStartModule(full_path.c_str(), s_ModuleArgsSize, s_ModuleArgsMem, 0, SCE_NULL, &modres)) < 0)
            return NULL;

        // register the exports (basically load and start the bootstrap. After that it can be safely removed since it's done it's job.
        std::string bootstrap_path = full_path + ".b.suprx";
        SceUID bootstrap_id;
        if ((bootstrap_id = sceKernelLoadStartModule(bootstrap_path.c_str(), sizeof(mod), &mod, 0, SCE_NULL, &modres)) < 0)
            return NULL;

        g_BootstrapHandlesMap[std::string(module_path)] = bootstrap_id;

        set_last_error(NULL);
        return (void*)mod;
    }

    static EXTERNFUNCTION GetProcAddress(const char* module, const char* name)
    {
        if (NULL == name || NULL == module)
            return NULL;

        std::string s(module);
        if (s.length() >= 26)
            s = s.substr(0, 26).c_str();

        ModulesMap::const_iterator it = s_ModulesMap.find(std::make_pair(s, std::string(name)));
        if (it != s_ModulesMap.end())
            return it->second;

        printf("[SUPRX] Unknown function %s\n", name);
        return NULL;
    }

    void* dlsym(void* handle, const char *name)
    {
        const char* module_name = s_HandlesMap[(SceUID)handle].c_str();
        void* fnptr = (void*)GetProcAddress(module_name, name);
        return fnptr;
    }

    int dlclose(void *handle)
    {
        int res, modres;
        set_last_error(NULL);

        std::string module_name = s_HandlesMap[(SceUID)handle] + std::string(".suprx");
        std::string bootstrap_path = module_name + std::string(".b.suprx");
        SceUID bootstrap_id = g_BootstrapHandlesMap[bootstrap_path];
        res = sceKernelStopUnloadModule(bootstrap_id, 0, NULL, 0, NULL, &modres);
        res = sceKernelStopUnloadModule((SceUID)handle, 0, NULL, 0, NULL, &modres);
        set_last_error(NULL);
        return res;
    }
} /* namespace psp2 */
} /* namespace os */
} /* namespace il2cpp */

#endif //IL2CPP_TARGET_PSP2
