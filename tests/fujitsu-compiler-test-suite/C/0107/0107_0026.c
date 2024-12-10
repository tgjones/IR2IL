#include <stdio.h>
#include <memory.h>

int *array1[1000] ;
int *array2[1000] ;
int  res[1000];

static void init(void)
{
  memset(array1, 0,  sizeof(array1));
  memset(array2, -1, sizeof(array2));
  array2[0] = NULL;
}

static void (*fun)() = init;
int main()
{
  int i;

  fun();

  for(i=0;i<1000;i++) {
    res[i] = array2[i] == array1[i] ? 10 : 20;
  }

  if (res[0] == 10 && res[999] == 20) {
    puts("OK");
  }
  else {
    printf("NG %d %d\n", res[0], res[999]);
  }
}
