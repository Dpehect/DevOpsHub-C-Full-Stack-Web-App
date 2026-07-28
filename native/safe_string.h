#ifndef DEVOPSHUB_SAFE_STRING_H
#define DEVOPSHUB_SAFE_STRING_H

#include <stddef.h>

int safe_copy(char *destination, size_t destination_size, const char *source);
int safe_format(char *destination, size_t destination_size, const char *format, ...);

#endif
