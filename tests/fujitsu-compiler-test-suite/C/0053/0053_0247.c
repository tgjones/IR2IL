#include<stdio.h>
struct tag1 { int a; int b; };
int main()
{
  printf("********** \n");
  {
    int val1 = 1,val2 = 2,c,a = 1;
    int *const p11 = &val1,*const p12 = &val2;

    c = *(a ? p11 : p12);
    if (c == 1)
        printf("***** \n");
      else
        printf("***** \n");
  }
  {
    static struct tag1 st1 = { 1,2 },st2 = { 3,4 },c;
    struct tag1 *const p21 = &st1,*const p22 = &st2;
    int a = 1;

    c = *(a ? p21 : p22);
    if (c.a == 1)
        printf("***** \n");
      else
        printf("***** \n");
  }
  {
    int volatile const vc31 = 1,vc32 = 2; int c,a = 1;

    c = a ? vc31 : vc32;
    if (c == 1)
        printf("***** \n");
      else
        printf("***** \n");
  }
  {
    static volatile const struct tag1 st1={ 1,2 },st2={ 3,4 };
    static struct tag1 c;
    int a = 1;

    c = (a ? st1 : st2);
    if (c.a == 1)
        printf("***** \n");
      else
        printf("***** \n");
  }
  {
    int const v51 = 1,v52 = 2; int c,a = 1;

    c = a ? v51 : v52;
    if (c == 1)
        printf("***** \n");
      else
        printf("***** \n");
  }
  {
    static const struct tag1 st1 = { 1,2 },st2 = { 3,4 };
    static struct tag1 c;
    int a = 1;

    c = (a ? st1 : st2);
    if (c.a == 1)
        printf("***** \n");
      else
        printf("***** \n");
  }
  {
    int val1 = 1,val2 = 2,*c,a = 1;
    int *volatile const p71 = &val1,*volatile const p72 = &val2;

    c = (int *)(a ? p71 : p72);
    if (*c == 1)
        printf("***** \n");
      else
        printf("***** \n");
  }
  {
    static struct tag1 st1={ 1,2 },st2={ 3,4 };
    volatile const struct tag1 *p81 = &st1,*p82 = &st2,*c;
    int a = 1;

    c = (a ? p81 : p82);
    if (c->a == 1)
        printf("***** \n");
      else
        printf("***** \n");
  }
  {
    int  a=2;
    int  b=3;
    int c=0,i=0;

    i = c ? a : b;
    if (i == 3)
        printf("***** \n");
      else
        printf("***** \n");
  }
  {
    static  const volatile  struct tag {int a; int b;}st={2,3},
                                                        sd={4,5};
    static  struct tag c;
    int a=0;

    c=(a?st:sd);
    if (c.a == 4)
        printf("***** \n");
      else
        printf("***** \n");
  }
  printf("********** \n");
}
