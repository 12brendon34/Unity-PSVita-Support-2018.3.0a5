#include "il2cpp-config.h"

#if IL2CPP_TARGET_PSP2

namespace il2cpp
{
namespace os
{
namespace psp2
{
    void* dlopen(const char *module_path, int flags, int sz, void* args);
    void* dlsym(void* handle, const char *name);
    int   dlclose(void *handle);
} /* namespace psp2 */
} /* namespace os */
} /* namespace il2cpp */

#endif //IL2CPP_TARGET_PSP2
