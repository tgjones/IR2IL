#include <stdio.h>
#include <string.h>

const int a[10] = { 1,2,3,4,5,6,7,8,9,10 };
const int b[10] = { 10,20,30,40,50,60,70,80,90,100 };
const int ans[10] = { 11,22,33,44,55,66,77,88,99,110 };

void foo(int *p)
{
  int i;
  i=0;
Label:
    p[i] = a[i] + b[i];
    i++;
  if((i<=9)) goto Label;
  ;
}
int main()
{
  int c[10];

  foo(c);

  if (memcmp(c, ans, sizeof(int)*10)==0) {
    puts("OK");
  }
  else {
    puts("NG");
  }
}