#include <stdio.h>

int main() {
  unsigned int a[1000], b[1000], c[1000];
  long int i;
  for (i=0; i<1000; i++) {
    b[i] = i;
    c[i] = i+1;
  }
  
  for (i=16; i<945; i++) {
    a[i] = c[i+55]+b[i-10]+b[i-14]+c[i+51]+c[i+49]+b[i-16];
  }

  if (a[100] == c[155]+b[90]+b[86]+c[151]+c[149]+b[84]) {
    printf("OK\n");
  } else {
    printf("NG a[100]=%d\n",a[100]);
  }
  return 1;
}
