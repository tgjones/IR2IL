#include<stdio.h>
void f3(register volatile unsigned long int,int);
void f2(register volatile unsigned int,int);
void f1(register volatile int,int);
void f0(register volatile int ,int);
int main()
{
  f3(1ul,2);
  f2(1u,2);
  f1(1,2);
  f0(1,2);
}

void f3(register volatile unsigned long int a,int b)
{
  if (a == 1ul)
      printf("***** \n");
    else
      printf("***** \n");

  if (b == 2)
      printf("***** \n");
    else
      printf("***** \n");
}

void f2(register volatile unsigned int a,int b)
{
  if (a == 1u)
      printf("***** \n");
    else
      printf("***** \n");

  if (b == 2)
      printf("***** \n");
    else
      printf("***** \n");
}

void f1(register volatile int a,int b)
{
  if (a == 1)
      printf("***** \n");
    else
      printf("***** \n");

  if (b == 2)
      printf("***** \n");
    else
      printf("***** \n");
}

void f0(register volatile  int a,int b)
{
  if (a == 1)
      printf("***** \n");
    else
      printf("***** \n");

  if (b == 2)
      printf("***** \n");
    else
      printf("***** \n");
}
