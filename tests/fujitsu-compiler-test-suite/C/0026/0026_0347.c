#include<stdio.h>
#include<complex.h>

#if __GNUC__
#define CF __complex__ float
#define CD __complex__ double
#define CL __complex__ long double
#else
#define CF _Complex float
#define CD _Complex double
#define CL _Complex long double
#endif 

#define I (0.0F+1.0iF)

void dump(CL arg, int len)
{
  if(len == 8)
    printf("%f %f \n",crealf(arg),cimagf(arg));
  else if(len == 16)
    printf("%lf %lf \n",creal(arg),cimag(arg));
  else 
    printf("%Lf %Lf \n",creall(arg),cimagl(arg));
}



void bar(CL arg)
{
  dump(arg, sizeof(arg));
}

CL foo()
{
   CL x[1] = {10.1234 + 10.1234i}; if(x[0] != 7893457.1793 + 12345.3456i)x[0] += *x;
   bar(x[0]);
   return 1;
}

int main()
{
   int X = foo();
}
