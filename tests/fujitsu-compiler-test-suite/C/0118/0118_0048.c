
#include <stdio.h>

#define loop_num 1000
#define Correct_answer 100

void sub(char *a, char *b, int n, int k)
{
  int i;
  for (i=0; i<n; i=i+k) {
    a[i] = a[i] + b[i];
  }
  return;
}

int main()
{
  char a[loop_num], b[loop_num];
  int i,result;

  for (i=0; i<loop_num; i++) {
    a[i] = 0;
    b[i] = 1;
  }

  sub(a,b,loop_num,10);

  result = 0;
  for (i=0; i<loop_num; i++) {
    result += a[i];
  }
  
  if (result == Correct_answer) {
    printf("ok\n");
  } else {
    printf("ng ... %d\n",result);
  }
  return 0;
}