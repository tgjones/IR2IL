struct S {
  char dummy;
  long array[10];
};

typedef struct S Stype;

Stype obj;

void test()
{
  obj.array[3] = 5;
}

#include <stdio.h>

int main()
{
  test();

  if (obj.array[3] == 5) {
    printf("test ok\n");
  } else {
    printf("test ng: %d\n", obj.array[3]);
  }

  return 0;
}