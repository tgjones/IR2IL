#include<stdio.h>
int a;
extern int a;
int main()
{
  if( a==0 )
    printf("\n");
}
