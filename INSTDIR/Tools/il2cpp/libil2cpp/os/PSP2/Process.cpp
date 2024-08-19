#include "il2cpp-config.h"

#if IL2CPP_TARGET_PSP2

#include "os/Process.h"
#include <kernel.h>

struct ProcessHandle
{
    SceUID pid;
};

namespace il2cpp
{
namespace os
{
    int Process::GetCurrentProcessId()
    {
        return (int)sceKernelGetProcessId();
    }

    ProcessHandle* Process::GetProcess(int processId)
    {
        return (ProcessHandle*)(intptr_t)processId;
    }

    void Process::FreeProcess(ProcessHandle* handle)
    {
        // We have nothing to do here.
    }

    std::string Process::GetProcessName(ProcessHandle* handle)
    {
        return std::string();
    }
}
}

#endif
