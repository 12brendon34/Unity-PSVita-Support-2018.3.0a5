#include "il2cpp-config.h"

#if IL2CPP_TARGET_PSP2

#include "vm/Exception.h"
#include "os/Console.h"

namespace il2cpp
{
namespace os
{
namespace Console
{
    int32_t InternalKeyAvailable(int32_t ms_timeout)
    {
        NOT_SUPPORTED_IL2CPP(Console::InternalKeyAvailable, "This call is not supported for PS Vita.");
        return -1;
    }

    bool SetBreak(bool wantBreak)
    {
        NOT_SUPPORTED_IL2CPP(Console::SetBreak, "This call is not supported for PS Vita.");
        return true;
    }

    bool SetEcho(bool wantEcho)
    {
        NOT_SUPPORTED_IL2CPP(Console::SetEcho, "This call is not supported for PS Vita.");
        return true;
    }

    bool TtySetup(const std::string& keypadXmit, const std::string& teardown, uint8_t* control_characters, int32_t** size)
    {
        NOT_SUPPORTED_IL2CPP(Console::TtySetup, "This call is not supported for PS Vita.");
        return true;
    }
}
}
}

#endif
