// Min/Max being defined in Windows minmax.h file and not listening to NOMINMAX. Work around this.
#define NOMINMAX
#define WIN32_LEAN_AND_MEAN
#include <Windows.h>

#ifdef max
#undef max
#endif

#ifdef min
#undef min
#endif

#ifdef MAX
#undef MAX
#endif

#ifdef MIN
#undef MIN
#endif
