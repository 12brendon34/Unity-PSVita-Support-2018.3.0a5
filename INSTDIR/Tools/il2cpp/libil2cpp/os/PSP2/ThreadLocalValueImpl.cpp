#include "il2cpp-config.h"

#if IL2CPP_THREADS_PTHREAD

#include "ThreadLocalValueImpl.h"
#include <cassert>

#define _unused(x) ((void)x)

namespace il2cpp
{
namespace os
{
    bool ThreadLocalValueImpl::s_TlsKeysInit = true;
    ThreadLocalValueImpl::TlsKey ThreadLocalValueImpl::s_KeyTable[kTLSMaxKeys];
    __thread void* s_TlsSpecificData[ThreadLocalValueImpl::kTLSMaxKeys];

    int ThreadLocalValueImpl::KeyCreate(TlsKey *key)
    {
        if (s_TlsKeysInit)
        {
            memset(s_KeyTable, 0, sizeof(s_KeyTable));
            s_TlsKeysInit = false;
        }

        int kix;
        for (kix = 0; kix < kTLSMaxKeys; kix++)
        {
            if (s_KeyTable[kix].count == 0)
            {
                s_KeyTable[kix].count++;
                key->key = kix;
                return 0;
            }
        }

        return EAGAIN;
    }

    int ThreadLocalValueImpl::KeyDelete(TlsKey key)
    {
        int k = key.key;
        int c;

        if (k < 0 || k >= kTLSMaxKeys)
            return EINVAL;

        if ((c = s_KeyTable[k].count) == 0)
        {
            return 0;
        }

        if (c == 1)
        {
            s_KeyTable[k].count = 0;
        }

        return 0;
    }

    int ThreadLocalValueImpl::GetSpecific(TlsKey key, void* value)
    {
        int k = key.key;

        if (k < 0 || k >= kTLSMaxKeys)
        {
            return EINVAL;
        }

        if (s_KeyTable[k].count == 0)
        {
            return 0;
        }

        if (s_TlsSpecificData[k] == NULL)
        {
            if (value != NULL)
            {
                s_KeyTable[k].count++;
            }
        }
        else
        {
            if (value == NULL)
            {
                s_KeyTable[k].count--;
            }
        }
        s_TlsSpecificData[k] = value;

        return 0;
    }

    void * ThreadLocalValueImpl::SetSpecific(TlsKey key)
    {
        return s_TlsSpecificData[key.key];
    }

    ThreadLocalValueImpl::ThreadLocalValueImpl()
    {
        TlsKey key;
        int result = KeyCreate(&key);
        assert(!result);
        _unused(result);
        m_Key = key;
    }

    ThreadLocalValueImpl::~ThreadLocalValueImpl()
    {
        int result = KeyDelete(m_Key);
        assert(result == 0);
    }

    ErrorCode ThreadLocalValueImpl::SetValue(void* value)
    {
        if (GetSpecific(m_Key, value))
            return kErrorCodeGenFailure;

        return kErrorCodeSuccess;
    }

    ErrorCode ThreadLocalValueImpl::GetValue(void** value)
    {
        *value = SetSpecific(m_Key);
        return kErrorCodeSuccess;
    }
}
}

#endif
