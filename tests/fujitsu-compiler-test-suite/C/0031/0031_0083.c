#include<stdio.h>
#include<complex.h>

#define CF _Complex float
#define CD _Complex double
#define CL _Complex long double

void dump(CF arg, int len)
{
  if(len == 8)
    printf("%f %f \n",crealf(arg),cimagf(arg));
  else if(len == 16)
    printf("%lf %lf \n",creal(arg),cimag(arg));
  else 
    printf("%Lf %Lf \n",creall(arg),cimagl(arg));
}


static CF y = 0 + 12345.3456i;

CF foo()
{
   extern CF x; x += (x!=y)? 10.1234 : 20.1234;
   return x;
}

CF x = 7893457.1793 + 12345.3456i;

int main()
{
   CF x = foo();
   dump(x, sizeof(x));
   return 0;
}
