#ifndef SN_TARGET_PSP2
#error  ##################### This header is for PSP2 only ########################
#endif

#ifndef _FCNTL_H
#define _FCNTL_H

#include <yvals.h>
#include <scetypes.h>
#include <stdio.h>
#include <kernel/iofilemgr.h>

#define O_CREAT         SCE_O_CREAT
#define O_EXCL          SCE_O_EXCL
#define O_TRUNC         SCE_O_TRUNC
#define O_APPEND        SCE_O_APPEND
#define   O_RDONLY      SCE_O_RDONLY
#define   O_RDWR        SCE_O_RDWR
#define   O_WRONLY      SCE_O_WRONLY

#endif /* _FCNTL_H */
