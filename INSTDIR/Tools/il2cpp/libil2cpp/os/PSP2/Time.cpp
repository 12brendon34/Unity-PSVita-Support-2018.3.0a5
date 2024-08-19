#include "il2cpp-config.h"

#if IL2CPP_TARGET_PSP2

#include "os/Time.h"
#include <kernel.h>
#include <rtc.h>

namespace il2cpp
{
namespace os
{
    uint32_t Time::GetTicksMillisecondsMonotonic()
    {
        SceKernelSysClock c;
        sceKernelGetProcessTime(&c); // value in microseconds
        return c.quad / 1000;
    }

    int64_t Time::GetTicks100NanosecondsMonotonic()
    {
        SceKernelSysClock c;
        sceKernelGetProcessTime(&c); // value in microseconds
        return c.quad * 10;
    }

    int64_t Time::GetTicks100NanosecondsDateTime()
    {
        SceRtcTick utcTick;
        sceRtcGetCurrentTick(&utcTick); // value in microseconds since 1st January 0001
        return utcTick.tick * 10;   // should be the number of 100ns ticks since 1/1/1, UTC timezone
    }

    int64_t Time::GetSystemTimeAsFileTime()
    {
        IL2CPP_NOT_IMPLEMENTED(Time::GetSystemTimeAsFileTime);
    }
}
}

#endif
