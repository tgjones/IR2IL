#include <stdio.h>

int main() {
  unsigned int a[1000], b[1000];
  int i;
  for (i=0; i<1000; i++) {
    b[i] = i;
  }
  for (i=991; i>1; i--) {
    a[i] = b[i+6]+b[i]+b[i-2]+b[i+8];
  }
  if (a[100] == b[106]+b[100]+b[98]+b[108]) {
    printf("OK\n");
  } else {
    printf("NG\n");
  }
  return 1;
}