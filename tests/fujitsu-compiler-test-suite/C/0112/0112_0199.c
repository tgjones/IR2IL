

#include <stdio.h>
#include <math.h>
#ifndef __USE_ISOC99


#include "func_def_ansi.h"
#endif
#define n 31
int main() {
  double r8a[n];
  double r8b[n] = {10.2,-8.1,6.9,-4.2,2.0,-1.0,3.3,5.5,-7.7,9.9,
                   10.2,-8.1,6.9,-4.2,2.0,-1.0,3.3,5.5,-7.7,9.9,
                   10.2,-8.1,6.9,-4.2,2.0,-1.0,3.3,5.5,-7.7,9.9,
                   10.2};
  double result_trunc8[n] = {10.0,-8.0,6.0,-4.0,2.0,-1.0,3.0,5.0,-7.0,9.0,
                             10.0,-8.0,6.0,-4.0,2.0,-1.0,3.0,5.0,-7.0,9.0,
                             10.0,-8.0,6.0,-4.0,2.0,-1.0,3.0,5.0,-7.0,9.0,
                             10.0};
  double r8c[n];
  double r8d[n] = {10.2,-8.1,6.9,-4.2,2.0,-1.0,3.3,5.5,-7.7,9.9,
                   10.2,-8.1,6.9,-4.2,2.0,-1.0,3.3,5.5,-7.7,9.9,
                   10.2,-8.1,6.9,-4.2,2.0,-1.0,3.3,5.5,-7.7,9.9,
                   10.2};
  double result_nearbyint8[n] = {10.0,-8.0,7.0,-4.0,2.0,-1.0,3.0,6.0,-8.0,10.0,
                                 10.0,-8.0,7.0,-4.0,2.0,-1.0,3.0,6.0,-8.0,10.0,
                                 10.0,-8.0,7.0,-4.0,2.0,-1.0,3.0,6.0,-8.0,10.0,
                                 10.0};
  int i;
  for (i = 0;i < n;i++) {
    r8a[i] = 0.0;
    r8c[i] = 0.0;
  }
  for (i = 0;i < n;i++) {
    r8a[i] = trunc(r8b[i]);
    r8c[i] = nearbyint(r8d[i]);
 }
  for (i = 0;i < n;i++) {
    if (r8a[i] != result_trunc8[i] || r8c[i] != result_nearbyint8[i]) {
      goto NG;
    }
  }
  printf("OK\n");
  goto OK;
  NG: printf("NG\n");
  for (i = 0;i < n;i++) {
    printf("%f ",r8a[i]);
    printf("%f ",r8c[i]);
  }
  OK:;
}
