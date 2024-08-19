#pragma once

#if IL2CPP_TARGET_PSP2

namespace il2cpp
{
namespace os
{
    struct in_addr *get_local_ips(int32_t family, int32_t *nips);

    struct hostent *gethostbyname(const char *hostname);
    int gethostname(char *name,  size_t namelen);

    unsigned long char_to_inet_addr(const char* hostname);
} // os
} // il2cpp

#endif // IL2CPP_TARGET_PSP2
