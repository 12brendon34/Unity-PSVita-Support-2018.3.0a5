#include "il2cpp-config.h"

#if IL2CPP_TARGET_PSP2

#include "os/Cryptography.h"

#include <rtc.h>
#include <libsfmt607.h>


#define CRYPTOCTX SceSfmt607Context
#define CRYPTOINIT sceSfmt607InitGenRand
#define CRYPTOFILL sceSfmt607FillArray32

static CRYPTOCTX* s_cryptoProvider = NULL;

namespace il2cpp
{
namespace os
{
    void* Cryptography::GetCryptographyProvider()
    {
        return (void*)s_cryptoProvider;
    }

    bool Cryptography::OpenCryptographyProvider()
    {
        s_cryptoProvider = (CRYPTOCTX*)malloc(sizeof(CRYPTOCTX));

        SceRtcTick utcTick;

        int seed = 0;
        int result = sceRtcGetCurrentTick(&utcTick);

        if (result == 0)
            seed = (int)(utcTick.tick);

        CRYPTOINIT(s_cryptoProvider, seed);
        return true;
    }

    void Cryptography::ReleaseCryptographyProvider(void* provider)
    {
    }

    bool Cryptography::FillBufferWithRandomBytes(void* provider, uint32_t length, unsigned char* data)
    {
        CRYPTOCTX *cryptoProvider = (CRYPTOCTX*)provider;
        if (cryptoProvider == NULL)
        {
            return false;
        }

        int numblocks = length / (4 * 4 * SCE_SFMT607_ARRAY_SIZE);
        if (numblocks > 0)
        {
            int32_t result = CRYPTOFILL(cryptoProvider, (SceUInt32*)data, numblocks * 4 * SCE_SFMT607_ARRAY_SIZE);
            if (result != 0)
            {
                printf("crypto failed 0x%x\n", result);
                return false;
            }
            length -= (numblocks * 4 * SCE_SFMT607_ARRAY_SIZE * 4);
            data += (numblocks * 4 * SCE_SFMT607_ARRAY_SIZE * 4);
        }


        if (length > 0)
        {
            SceUInt32 minbuf[4 * SCE_SFMT607_ARRAY_SIZE];
            int32_t result = CRYPTOFILL(cryptoProvider, minbuf, 4 * SCE_SFMT607_ARRAY_SIZE);
            if (result == 0)
            {
                memcpy(data, minbuf, length);
                return true;
            }
            else
            {
                printf("crypto2 failed 0x%x\n", result);
                return false;
            }
        }
        return true;
    }
}
}

#endif //IL2CPP_TARGET_PSP2
