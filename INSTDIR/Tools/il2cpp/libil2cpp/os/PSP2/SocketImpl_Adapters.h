#pragma once

#if IL2CPP_TARGET_PSP2

#define SKIP_OPTION             -2
#define INVALID_OPTION_NAME     -1

namespace il2cpp
{
namespace os
{
    int32_t convert_socket_flags(os::SocketFlags flags);
    int32_t convert_address_family(AddressFamily family);
    int32_t convert_socket_type(SocketType type);
    int32_t convert_socket_protocol(ProtocolType protocol);
    int32_t convert_sce_net_errno(int32_t error);

    int32_t level_and_name_to_system(SocketOptionLevel level, SocketOptionName name, int32_t *system_level, int32_t *system_name);
} // os
} // il2cpp

#endif // IL2CPP_TARGET_PSP2
