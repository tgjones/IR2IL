#include <stdio.h>

int main(void) {
  int i,n;
  int a[100],b[100];
  n = 100;
  for(i=0;i<n;++i) {
    a[i] = 1;
    b[i] = i;
  }

  for(i=0;i<n;++i) {
    a[b[++i]] =7;
  }
  printf("%d %d\n",a[0],a[1]);
}