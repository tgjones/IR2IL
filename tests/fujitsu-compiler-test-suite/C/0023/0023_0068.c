#include <stdlib.h>
#include <stdio.h>

int main()
{
  unsigned long long int a;

  a = 0x7fffffffffffffffULL % 0x000000007fffffffULL;

#if defined(big_endian)
  if (   *((unsigned int *)&a)==0x0
      && *((((unsigned int *)&a))+1)==0x1 ) {
#else
  if (   *((unsigned int *)&a)==0x1
      && *((((unsigned int *)&a))+1)==0x0 ) {
#endif
    printf("OK\n");
    exit(0);
  }

  printf("%x %x\n", a, *(((unsigned int *)&a)+1));
  printf("NG\n");
  exit(1);
}
