
#include <stdio.h>

void *fx[10],*cx[10];
int ix=0;

extern void __cyg_profile_func_enter(void *func_address, void *call) ;
extern void __cyg_profile_func_exit(void *func_add, void *call) ;

int add(int a, int b) {
  return a + b;
}

int main() {
  int result = add(3, 7);
  printf("result = %d\n", result);
  return 0;
}

__attribute__((no_instrument_function))
void __cyg_profile_func_enter(void *func_add, void *call) {
  ix++;
  fx[ix]=func_add;
  cx[ix]=call;
}

__attribute__((no_instrument_function))
void __cyg_profile_func_exit(void *func_add, void *call) {
  if (fx[ix]==func_add) printf("cnt:%d ok\n",ix);
  if (cx[ix]==call    ) printf("cnt:%d ok\n",ix);
  ix--;
}
