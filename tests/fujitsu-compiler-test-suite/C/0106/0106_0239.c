#include <stdio.h>

int main() {
  double a[1000], b[1000], c[1000];
  long int i;
  for (i=0; i<1000; i++) {
    b[i] = i;
    c[i] = i+1;
  }
  for (i=0; i<923; i++) {
    a[i]=b[i+76]+c[i]+b[i+77]+b[i+74];
  }
  if (a[100] == b[176]+c[100]+b[177]+b[174]) {
    printf("OK\n");
  } else {
    printf("NG %f\n",a[100]);
  }
  return 1;
}
