#pragma once

#if IL2CPP_THREADS_PTHREAD

#include "os/ErrorCodes.h"
#include "utils/NonCopyable.h"

namespace il2cpp
{
namespace os
{
    class ThreadLocalValueImpl : public il2cpp::utils::NonCopyable
    {
    public:
        static const int kTLSMaxKeys = 48;

        ThreadLocalValueImpl();
        ~ThreadLocalValueImpl();
        ErrorCode SetValue(void* value);
        ErrorCode GetValue(void** value);

    private:
        struct TlsKey
        {
            int count;
            int key;
        };

        static bool s_TlsKeysInit;
        static TlsKey s_KeyTable[kTLSMaxKeys];

        static int KeyCreate(TlsKey *key);
        static int KeyDelete(TlsKey key);
        static int GetSpecific(TlsKey key, void* value);
        static void *SetSpecific(TlsKey key);

        TlsKey m_Key;
    };
}
}

#endif
