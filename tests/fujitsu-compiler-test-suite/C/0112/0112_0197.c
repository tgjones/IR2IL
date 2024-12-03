

#include <stdio.h>
#include <math.h>
#ifndef __USE_ISOC99


#include "func_def_ansi.h"
#endif
#define n 31
int main() {
  float r4a[n];
  float r4b[n] = {10.2,-8.1,6.9,-4.2,2.0,-1.0,3.3,5.5,-7.7,9.9,
                  10.2,-8.1,6.9,-4.2,2.0,-1.0,3.3,5.5,-7.7,9.9,
                  10.2,-8.1,6.9,-4.2,2.0,-1.0,3.3,5.5,-7.7,9.9,
                  10.2};
  float result_trunc4[n] = {10.0,-8.0,6.0,-4.0,2.0,-1.0,3.0,5.0,-7.0,9.0,
                            10.0,-8.0,6.0,-4.0,2.0,-1.0,3.0,5.0,-7.0,9.0,
                            10.0,-8.0,6.0,-4.0,2.0,-1.0,3.0,5.0,-7.0,9.0,
                            10.0};
  float r4c[n];
  float r4d[n] = {10.2,-8.1,6.9,-4.2,2.0,-1.0,3.3,5.5,-7.7,9.9,
                  10.2,-8.1,6.9,-4.2,2.0,-1.0,3.3,5.5,-7.7,9.9,
                  10.2,-8.1,6.9,-4.2,2.0,-1.0,3.3,5.5,-7.7,9.9,
                  10.2};
  float result_nearbyint4[n] = {10.0,-8.0,7.0,-4.0,2.0,-1.0,3.0,6.0,-8.0,10.0,
                                10.0,-8.0,7.0,-4.0,2.0,-1.0,3.0,6.0,-8.0,10.0,
                                10.0,-8.0,7.0,-4.0,2.0,-1.0,3.0,6.0,-8.0,10.0,
                                10.0};
  int i;
  for (i = 0;i < n;i++) {
    r4a[i] = 0.0;
    r4c[i] = 0.0;
  }
  for (i = 0;i < n;i++) {
    r4a[i] = truncf(r4b[i]);
    r4c[i] = nearbyintf(r4d[i]);
 }
  for (i = 0;i < n;i++) {
    if (r4a[i] != result_trunc4[i] || r4c[i] != result_nearbyint4[i]) {
      goto NG;
    }
  }
  printf("OK\n");
  goto OK;
  NG: printf("NG\n");
  for (i = 0;i < n;i++) {
    printf("%f ",r4a[i]);
    printf("%f ",r4c[i]);
  }
  OK:;
}
