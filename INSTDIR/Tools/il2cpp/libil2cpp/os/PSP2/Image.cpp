#include "il2cpp-config.h"

// Section Start and End Pseudo-Symbols, see https://psvita.scedev.net/docs/developer_tools-en,Linker,Section_Symbols/1/
extern unsigned char const __start__Ztext[];

namespace il2cpp
{
namespace os
{
namespace Image
{
    void* GetImageBase()
    {
        return (void*)__start__Ztext;
    }
}
}
}
