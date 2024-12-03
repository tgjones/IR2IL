
#include <stdio.h>
#include <limits.h>

#define N (1024)
#define end ((unsigned long int)ULONG_MAX)

void for_comp (double *restrict a,double *restrict b) {
  unsigned long int u, c=0,d;
  u = 0;
  for(u=end;u>0;u++){
    c++;
    d = u % 1024;
    a[d] = b[d] ;
  }
}

int main(void) {
  printf("OK\n");
}
