#pragma once

#if IL2CPP_TARGET_PSP2

#include "PSP2/AtomicImpl-c-api.h"
#include <sce_atomic.h>

namespace il2cpp
{
namespace os
{
    inline void Atomic::FullMemoryBarrier()
    {
        sceAtomicMemoryBarrier();
    }
}
}

#endif
