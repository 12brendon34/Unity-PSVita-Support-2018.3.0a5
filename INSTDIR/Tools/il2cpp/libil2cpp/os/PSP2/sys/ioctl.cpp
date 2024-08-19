// PS Vita platform specific implementation.

#include "il2cpp-config.h"

#if IL2CPP_TARGET_PSP2

#include <string.h>
#include <stdlib.h>
#include <stdio.h>

#include "socket.h"
#include "ioctl.h"
#include <arpa/inet.h>
#include <libnetctl.h>

int ioctl(int fd, unsigned long request, ...)
{
    va_list args;
    va_start(args, request);

    int result = -1;

    switch (request)
    {
        case FIONBIO:
        {
            int val = *(va_arg(args, bool*)) ? 1 : 0;
            result = sceNetSetsockopt(fd, SCE_NET_SOL_SOCKET, SCE_NET_SO_NBIO, &val, sizeof(val));
            if (result >= 0)
            {
                result = 0;
            }
        }
        break;

        case FIONREAD:
        {
            int32_t * amount = va_arg(args, int32_t *);
            *amount = 0;

            SceNetSockInfo sockinfo;
            result = sceNetGetSockInfo(fd, &sockinfo, 1, 0);
            if (result >= 0)
            {
                *amount = sockinfo.recv_queue_length;
                result = 0;
            }
        }
        break;

        case SIOCGIFCONF:
        {
            ifconf * ifc = va_arg(args, ifconf *);
            if (ifc->ifc_len >= sizeof(ifconf))
            {
                SceNetCtlInfo info;
                in_addr localip;
                int32_t offset = 4;

                sceNetCtlInetGetInfo(SCE_NET_CTL_INFO_ETHER_ADDR, &info);
                inet_pton(AF_INET, info.ip_address, &localip);
                ifc->ifc_len = sizeof(ifconf);
                strcpy(ifc->ifc_req[0].ifr_name, "net0");
                memcpy(ifc->ifc_req[0].ifr_addr.sa_data + offset, &localip, sizeof(localip));
                result = 0;
            }
        }
        break;

        case SIOCGIFFLAGS:
            break;

        default:
            break;
    }

    va_end(args);
    return result;
}

#endif //#if IL2CPP_TARGET_PSP2
