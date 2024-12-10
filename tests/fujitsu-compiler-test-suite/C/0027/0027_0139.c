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




CD foo()
{
   CD result, *P = &result;
   CD resultarray[10];
   CD x = 7893457.1793 + 12345.3456i; CD *p = &x; if(*p == 7893457.1793 + 12345.3456i)*p += 1000000;
   result = (*p);
   *P = (*p);
   resultarray[9] = (*p);
   dump(result, sizeof(result));
   dump(*P, sizeof(result));
   dump(resultarray[9], sizeof(result));
   return result;
}

int main()
{
   volatile int X = foo();
   return 0;	
}