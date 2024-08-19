#include <assert.h>

extern "C"
{
int gettimeofday(struct timeval *tp, void *tzp)
{
    // Note that this should be called from multiple threads,
    // as it is used by libil2cpp to implement timed waits. It shold
    // probably use pthread_getsystemtime_np or sceKernelGetProcessTime.
    assert(0 && "This needs to be implemented.");
    return 0;
}
}
