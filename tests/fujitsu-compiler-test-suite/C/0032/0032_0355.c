#include<stdio.h>
#include<complex.h>

#define CF _Complex float
#define CD _Complex double
#define CL _Complex long double




void real_print1(unsigned long long int arg)
{
  int len=sizeof(arg);	
  if(len < 4)
    printf("%x\n",arg);
  else if(len == 4)
#ifdef 	FLOAT_FLAG1
    printf("%f\n",arg);
#else
    printf("%d\n",arg);
#endif
  else if(len == 8)
#ifdef 	DOUBLE_FLAG1
    printf("%lf\n",arg);
#else
    printf("%lld\n",arg);
#endif
  else if(len == 16 || len == 12)
    printf("%Lf\n",arg);
}

void real_print2(long double arg)
{
  int len=sizeof(arg);	
  if(len < 4)
    printf("%x\n",arg);
  else if(len == 4)
#ifdef 	FLOAT_FLAG2
    printf("%f\n",arg);
#else
    printf("%d\n",arg);
#endif
  else if(len == 8)
#ifdef 	DOUBLE_FLAG2
    printf("%lf\n",arg);
#else
    printf("%lld\n",arg);
#endif
  else if(len == 16 || len == 12)
    printf("%Lf\n",arg);
}

struct S { unsigned long long int a; long double b;};

void foo()
{
   struct { unsigned long long int a,b; struct S s;} x[] = {[1] = {.b = 1, .s = {.b = -8}}};
   real_print1(x[0].a);
   real_print1(x[0].b);
   real_print1(x[0].s.a);
   real_print2(x[0].s.b);
   real_print1(x[1].a);
   real_print1(x[1].b);
   real_print1(x[1].s.a);
   real_print2(x[1].s.b);
}

int main()
{
   foo();
   return 0;	
}
