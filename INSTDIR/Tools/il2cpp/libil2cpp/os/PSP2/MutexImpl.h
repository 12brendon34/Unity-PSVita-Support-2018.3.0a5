#pragma once

#if IL2CPP_THREADS_PTHREAD

#include "os/ErrorCodes.h"
#include "os/WaitStatus.h"
#include "os/Posix/PosixWaitObject.h"

#include <pthread.h>

namespace il2cpp
{
namespace os
{
    class Thread;

    class MutexImpl : public posix::PosixWaitObject
    {
    public:
        MutexImpl();

        void Lock(bool interruptible);
        bool TryLock(uint32_t milliseconds, bool interruptible);
        void Unlock();

    private:
        /// Thread that currently owns the object. Used for recursion checks.
        Thread* m_OwningThread;

        /// Number of recursive locks on the owning thread.
        uint32_t m_RecursionCount;
    };

    class FastMutexImpl
    {
    public:

        FastMutexImpl()
        {
            uint32_t workAreaStart = (uint32_t)m_MutexStorage;
            uint32_t aligneWorkAreaStart = (workAreaStart + 7) & ~7;
            m_MutexWork = (SceKernelLwMutexWork*)aligneWorkAreaStart;
            sceKernelCreateLwMutex(m_MutexWork, "lwmutex", SCE_KERNEL_LW_MUTEX_ATTR_RECURSIVE | SCE_KERNEL_LW_MUTEX_ATTR_TH_FIFO, 0, SCE_NULL);
        }

        ~FastMutexImpl()
        {
            sceKernelDeleteLwMutex(m_MutexWork);
        }

        void Lock()
        {
            sceKernelLockLwMutex(m_MutexWork, 1, SCE_NULL);
        }

        void Unlock()
        {
            sceKernelUnlockLwMutex(m_MutexWork, 1);
        }

    private:
        uint8_t m_MutexStorage[sizeof(SceKernelLwMutexWork) + 7];
        SceKernelLwMutexWork* m_MutexWork;
    };
}
}

#endif
