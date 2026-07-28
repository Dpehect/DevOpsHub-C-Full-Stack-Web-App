#include "safe_string.h"

#include <stdarg.h>
#include <stdio.h>

int safe_copy(char *destination, size_t destination_size, const char *source)
{
    if (destination == NULL || source == NULL || destination_size == 0)
        return -1;

    int written = snprintf(destination, destination_size, "%s", source);

    if (written < 0)
        return -2;

    if ((size_t)written >= destination_size)
    {
        destination[destination_size - 1] = '\0';
        return -3;
    }

    return 0;
}

int safe_format(char *destination, size_t destination_size, const char *format, ...)
{
    if (destination == NULL || format == NULL || destination_size == 0)
        return -1;

    va_list arguments;
    va_start(arguments, format);
    int written = vsnprintf(destination, destination_size, format, arguments);
    va_end(arguments);

    if (written < 0)
        return -2;

    if ((size_t)written >= destination_size)
    {
        destination[destination_size - 1] = '\0';
        return -3;
    }

    return 0;
}
