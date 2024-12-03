#include <stdlib.h>
#include <stdio.h>

int main()
{
  long long int  a = 2*2147483648LL-2147483648LL-2147483646LL;

#if defined(big_endian)
  if (   *((int *)&a)==0x0
      && *((((int *)&a))+1)==0x2 ) {
#else
  if (   *((int *)&a)==0x2
      && *((((int *)&a))+1)==0x0 ) {
#endif
    printf("OK\n");
    exit(0);
  }

  printf("NG\n");
  exit(1);
}
