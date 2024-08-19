// PS Vita platform specific implementation.

#define IFNAMSIZ (8)
struct ifreq
{
    char ifr_name[IFNAMSIZ]; /* Interface name */
    union
    {
        struct sockaddr ifr_addr;
        struct sockaddr ifr_dstaddr;
        struct sockaddr ifr_broadaddr;
        struct sockaddr ifr_netmask;
        struct sockaddr ifr_hwaddr;
        short          ifr_flags;
        int        ifr_ifindex;
        int        ifr_metric;
        int        ifr_mtu;
        //      struct ifmap    ifr_map;
        char           ifr_slave[IFNAMSIZ];
        char           ifr_newname[IFNAMSIZ];
        char *         ifr_data;
    };
};

struct ifconf
{
    int           ifc_len; /* size of buffer */
    union
    {
        char *        ifc_buf; /* buffer address */
        struct ifreq * ifc_req; /* array of structures */
    };
};

// set up dummy values so that FIONREAD will compile
#define IOCPARM_MASK        0x7ff                           /* parameters must be < 2k bytes */
#define IOCGROUP(x)         (((x) >> 8) & 0xff)
#define IOC_VOID            0x20000000                      /* no parameters */
#define IOC_OUT             0x40000000                      /* copy out parameters */
#define IOC_IN              0x80000000                      /* copy in parameters */
#define IOC_INOUT           (IOC_IN|IOC_OUT)
#define _IOWR(g, n, t)        _IOC(IOC_INOUT,   (g), (n), sizeof(t))
#define _IOC(inout, group, num, len) \
        (inout | ((len & IOCPARM_MASK) << 16) | ((group) << 8) | (num))
#define _IOR(g, n, t)         _IOC(IOC_OUT,   (g), (n), sizeof(t))
#define FIONREAD            _IOR('f', 127, int)             /* get # bytes to read */
#define SIOCGIFCONF         _IOWR('i',36, struct ifconf)    /* get ifnet list */
#define SIOCGIFFLAGS        _IOWR('i',17, struct ifreq)     /* get ifnet flags */

#define FIONBIO             4                               // value created pretty much at random for psp2

int ioctl(int fd, unsigned long request, ...);
