#include "math.h"
#include "stdio.h"
#define N 64

void sub(char a[N]) {
  int i;
  for(i=0;i<N;++i) {
    a[i] = a[i] + i;
  }
}


int main()
{
  char dest[N];
  int i;
  int ok=1;

#pragma loop nosimd
  for(i=0;i<N;i++) {
    dest[i] = 2;
  }

  sub (dest);

#pragma loop nosimd
  for (i = 0;i < N;i++) {
    if (i+2 != dest[i]) {
      printf(" NG: %d: %d \n",i,dest[i]);
      ok = 0;
    }
  }
  if (ok) {
    printf("ok\n");
  }
}
