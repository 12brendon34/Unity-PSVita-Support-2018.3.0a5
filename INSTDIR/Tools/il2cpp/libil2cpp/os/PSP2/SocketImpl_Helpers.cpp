#include "il2cpp-config.h"

#if IL2CPP_TARGET_PSP2

#include <libnetctl.h>

#include "arpa/inet.h"
#include "sys/socket.h"

#include "SocketImpl_Helpers.h"

#define WWW_UNITY3D_COM "83.221.146.11" // www.unity3d.com hardcoded


namespace il2cpp
{
namespace os
{
    struct in_addr *get_local_ips(int32_t family, int32_t *nips)
    {
        static in_addr localip;
        SceNetCtlInfo info;
        sceNetCtlInetGetInfo(SCE_NET_CTL_INFO_IP_ADDRESS, &info);

        inet_pton(SCE_NET_AF_INET, info.ip_address, &localip);

        if (nips)
            *nips = 1;

        return &localip;
    }

    struct hostent *gethostbyname(const char *hostname)
    {
        static struct hostent he;
        static in_addr  addr;
        static in_addr* addrlist[] = {&addr, NULL};
        static void*        alias = NULL;

#if USE_RESOLVER_PARAM
        SceNetResolverParam rparam;
#endif  /* USE_RESOLVER_PARAM */
        SceNetId rid = -1;
        int ret;

#if USE_RESOLVER_PARAM
        rparam.allocate = allocateCbFunction;
        rparam.free = freeCbFunction;
        rparam.user = NULL;
        rid = sceNetResolverCreate("resolver", &rparam, 0);
#else
        rid = sceNetResolverCreate("resolver", NULL, 0);
#endif  /* USE_RESOLVER_PARAM */
        if (rid < 0)
        {
            printf("gethostbyname: sceNetResolverCreate failed (0x%x errno=%d)\n", rid, sce_net_errno);
            return (NULL);
        }

        ret = sceNetResolverStartNtoa(rid, hostname, &addr, 0, 0, 0);
        if (ret < 0)
        {
            printf("gethostbyname: sceNetResolverStartNtoa failed (0x%x errno=%d)\n", ret, sce_net_errno);
            sceNetResolverDestroy(rid);
            return (NULL);
        }

        ret = sceNetResolverDestroy(rid);
        if (ret < 0)
        {
            printf("gethostbyname: sceNetResolverDestroy failed (0x%x errno=%d)\n", rid, sce_net_errno);
            return (NULL);
        }

        static char emptyName[] = { 0 };
        he.h_name       = emptyName;
        he.h_length     = 4;
        he.h_addrtype   = AF_INET;
        he.h_aliases    = (char**)&alias;
        he.h_addr_list  = (char**)&addrlist;

        return &he;
    }

    int gethostname(char *name,  size_t namelen)
    {
        strncpy(name, "0.0.0.0", namelen);

        struct in_addr          remote;
        struct sockaddr_in      raddress;
        struct sockaddr*        raddr = (struct sockaddr*)&raddress;
        struct sockaddr_in      laddress;
        struct sockaddr*        laddr = (struct sockaddr*)&laddress;

        SceNetId sock;
        int err;

        sock = sceNetSocket("gethostname", SCE_NET_AF_INET, SCE_NET_SOCK_DGRAM, 0);
        if (sock < 0)
        {
            return -1;
        }

        sceNetInetPton(SCE_NET_AF_INET, WWW_UNITY3D_COM, &remote);

        raddress.sin_port   = sceNetHtons(80);
        raddress.sin_family = SCE_NET_AF_INET;
        raddress.sin_addr   = remote;

        err = sceNetConnect(sock, raddr, sizeof(raddress));
        if (err < 0)
        {
            printf("gethostname: Error during connect");
            sceNetSocketClose(sock);
            return -1;
        }

        SceNetSocklen_t len = sizeof(laddress);
        err = sceNetGetsockname(sock, &laddress, &len);
        if (err < 0)
        {
            sceNetSocketClose(sock);
            return -1;
        }

        sceNetSocketClose(sock);

        //char  *Result = inet_ntoa(laddress.sin_addr);

        {
            SceNetInAddr  addr = laddress.sin_addr;

        #if USE_RESOLVER_PARAM
            SceNetResolverParam rparam;
        #endif

            SceNetId rid = -1;
            int ret;

        #if USE_RESOLVER_PARAM
            rparam.allocate = allocateCbFunction;
            rparam.free = freeCbFunction;
            rparam.user = NULL;
            rid = sceNetResolverCreate("resolver", &rparam, 0);
        #else
            rid = sceNetResolverCreate("resolver", NULL, 0);
        #endif

            if (rid < 0)
            {
                printf("gethostname: sceNetResolverCreate failed (0x%x errno=%d)\n", rid, sce_net_errno);
                return -1;
            }

            int timeout_us  = 0;
            int retry       = 0;
            int flags       = 0;

            ret = sceNetResolverStartAton(rid, &addr, name, namelen, timeout_us, retry, flags);
            if (ret < 0)
            {
                unsigned char *byteaddress = (unsigned char*)&addr;
                snprintf(name, namelen, "%d.%d.%d.%d", byteaddress[0], byteaddress[1], byteaddress[2], byteaddress[3]);
                //printf ("gethostname: sceNetResolverStartAton failed (0x%x errno=%d)\n", ret, sce_net_errno);
                //sceNetResolverDestroy(rid);
                //return(NULL);
            }
            ret = sceNetResolverDestroy(rid);
            if (ret < 0)
            {
                printf("gethostname: sceNetResolverDestroy failed (0x%x errno=%d)\n", rid, sce_net_errno);
                return -1;
            }
        }

        return 0;
    }

    unsigned long char_to_inet_addr(const char* hostname)
    {
#if USE_RESOLVER_PARAM
        SceNetResolverParam rparam;
#endif  /* USE_RESOLVER_PARAM */
        SceNetId rid = -1;
        int ret;

#if USE_RESOLVER_PARAM
        rparam.allocate = allocateCbFunction;
        rparam.free = freeCbFunction;
        rparam.user = NULL;
        rid = sceNetResolverCreate("resolver", &rparam, 0);
#else
        rid = sceNetResolverCreate("resolver", NULL, 0);
#endif  /* USE_RESOLVER_PARAM */
        if (rid < 0)
        {
            printf("sceNetResolverCreate() failed (0x%x errno=%d)\n", rid, sce_net_errno);
            return (int)0;
        }

        SceNetInAddr addr;
        ret = sceNetResolverStartNtoa(rid, hostname, &addr, 0, 0, 0);
        if (ret < 0)
        {
            printf("sceNetResolverStartNtoa() failed (0x%x errno=%d)\n", ret, sce_net_errno);
            sceNetResolverDestroy(rid);
            return 0;
        }

        ret = sceNetResolverDestroy(rid);
        if (ret < 0)
        {
            printf("sceNetResolverDestroy() failed (0x%x errno=%d)\n", rid, sce_net_errno);
            return 0;
        }
        return addr.s_addr;
    }

    /*
    char* inet_ntoa(struct in_addr in)
    {
        static char ip[64];
        unsigned char* bytes = (unsigned char*)&in;
        snprintf(ip, sizeof(ip), "%d.%d.%d.%d", bytes[0], bytes[1], bytes[2], bytes[3]);
        return ip;
    }


    int inet_aton(const char* cp, struct in_addr* inp)
    {
        unsigned long result = inet_addr(cp);
        if (result != 0 )
        {
            if (inp != NULL)
            {
                inp->s_addr = result;
            }
        }

        return result;
    }
    */
} // os
} // il2cpp

#endif //IL2CPP_TARGET_PSP2
