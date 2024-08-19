#include "il2cpp-config.h"

#if IL2CPP_TARGET_PSP2

#include "os/StackTrace.h"
#include <kernel.h>
#include <libdbg.h>

namespace il2cpp
{
namespace os
{
    void StackTrace::WalkStack(WalkStackCallback callback, void* context, WalkOrder walkOrder)
    {
        const uint32_t kMaxFrames = 128;
        SceKernelCallFrame stack[kMaxFrames];


        uint32_t frames = 0;
        int result = sceKernelBacktraceSelf(stack, sizeof(stack), &frames, SCE_KERNEL_BACKTRACE_MODE_DONT_EXCEED);

        if (result < 0)
        {
            printf("backtrace failed 0x%x\n", result);
            return;
        }
        if (walkOrder == WalkOrder::kFirstCalledToLastCalled)
        {
            for (size_t i = frames; i--;)
            {
                if (!callback(reinterpret_cast<Il2CppMethodPointer>(stack[i].pc), context))
                    break;
            }
        }
        else
        {
            for (size_t i = 0; i < frames; i++)
            {
                if (!callback(reinterpret_cast<Il2CppMethodPointer>(stack[i].pc), context))
                    break;
            }
        }
    }
}
}

#endif
