#include<stdio.h>
#include<complex.h>

#define CF _Complex float
#define CD _Complex double
#define CL _Complex long double

#define FLOAT_FLAG2


void real_print1(char arg)
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

void real_print2(float arg)
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

void foo()
{
   struct { char a;float b;} x[] = {[1] = {.b = 8}};
   real_print1(x[0].a);
   real_print2(x[0].b);
   real_print1(x[1].a);
   real_print2(x[1].b);
}

int main()
{
   foo();
   return 0;	
}
