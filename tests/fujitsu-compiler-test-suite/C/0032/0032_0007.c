#include<stdio.h>
#include<complex.h>

#define CF _Complex float
#define CD _Complex double
#define CL _Complex long double



void real_print(short arg)
{
  int len=sizeof(arg);	
  if(len < 4)
    printf("%x\n",arg);
  else if(len == 4)
#ifdef 	FLOAT_FLAG
    printf("%f\n",arg);
#else
    printf("%d\n",arg);
#endif
  else if(len == 8)
#ifdef 	DOUBLE_FLAG
    printf("%lf\n",arg);
#else
    printf("%lld\n",arg);
#endif
  else if(len == 16 || len == 12)
    printf("%Lf\n",arg);
}

void foo()
{
   struct { short a,b;} x[] = {[1] = {.b = 8}};
   real_print(x[0].a);
   real_print(x[0].b);
   real_print(x[1].a);
   real_print(x[1].b);
}

int main()
{
   foo();
   return 0;	
}
