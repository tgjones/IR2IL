#include <stdio.h>
#include <math.h>

#define NI (1024)

#define _CLONE_   II==2

#define _TRUE_ 1
#define _FALSE_ 0


double r[NI];

void sub_abc(int II) {
  int i;
  for (i=0; i<II; ++i) {
    r[i] = r[i] * i + 1.0;
  }
}

int main(void) {
  puts("OK");
}
