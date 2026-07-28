# Native Code Safety

The current backend is managed ASP.NET Core and does not use manual `malloc/free`.

If native C/C++ code is introduced:

- Prefer RAII, `std::vector`, `std::array`, `std::string` and smart pointers.
- Reject `strcpy`, `strcat`, `sprintf`, `gets` and unchecked pointer arithmetic.
- Validate destination capacity before each copy.
- Use `snprintf` and verify truncation.
- Use `strncpy` only with explicit null termination.
- Compile with stack protector, FORTIFY_SOURCE, ASan and UBSan.
- Run CodeQL C/C++ and fuzz tests in CI.

```c
int copy_text(char *destination, size_t destination_size, const char *source)
{
    if (destination == NULL || source == NULL || destination_size == 0)
        return -1;

    int written = snprintf(destination, destination_size, "%s", source);

    if (written < 0 || (size_t)written >= destination_size)
        return -2;

    return 0;
}
```
