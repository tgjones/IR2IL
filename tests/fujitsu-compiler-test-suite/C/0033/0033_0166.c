#include<stdio.h>
#include<complex.h>

#define CF _Complex float
#define CD _Complex double
#define CL _Complex long double

void dump(CD arg, int len)
{
  if(len == 8)
    printf("%f %f \n",crealf(arg),cimagf(arg));
  else if(len == 16)
    printf("%lf %lf \n",creal(arg),cimag(arg));
  else 
    printf("%Lf %Lf \n",creall(arg),cimagl(arg));
}

CD g() { return 7893457.1793 + 12345.3456i; }

CD foo()
{
   CD x = 100.1234 + 100.1234i; if(g() == x)x += g(); else printf("ok\n"); x = 1.0 + 1.0i;
   return x;
}

int main()
{
   CD x = foo();
   dump(x, sizeof(x));
   return 0;	
}
