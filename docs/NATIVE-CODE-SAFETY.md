# Native Code Safety

The current application is managed C# and TypeScript and does not require manual `malloc/free`.

If native C/C++ modules are introduced:

- Prefer `std::vector`, `std::array`, `std::string`, RAII and smart pointers.
- Reject unbounded `strcpy`, `strcat`, `sprintf`, `gets` and raw pointer arithmetic.
- Validate destination capacity before every copy.
- Use `snprintf` and verify truncation.
- Use `strncpy` only with explicit null termination.
- Compile with stack protector, FORTIFY_SOURCE, ASan and UBSan.
- Run CodeQL C/C++ and native fuzz tests in CI.

Example:

```c
int copy_name(char *destination, size_t destination_size, const char *source)
{
    if (destination == NULL || source == NULL || destination_size == 0)
        return -1;

    int written = snprintf(destination, destination_size, "%s", source);

    if (written < 0 || (size_t)written >= destination_size)
        return -2;

    return 0;
}
```
