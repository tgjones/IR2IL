#include <stdio.h>
#define loop_count 10

void sub(int *xxx, int *yyy) {
  int i;
  for (i=0; i<loop_count;i++) {
    if (xxx[i] == yyy[i]) {
      yyy[i] = xxx[i];
    }
  }
}
int main() {
  int xxx[loop_count];
  int yyy[loop_count];
  int i;

  for (i=0;i<loop_count;i++) {
    xxx[i] = i+1;
    yyy[i] = i*2-10;
  }
  sub(xxx,yyy);
  if (yyy[5] == 0) {
    printf("OK\n");
  } else {
    printf("NG %d\n",yyy[5]);
  }
}