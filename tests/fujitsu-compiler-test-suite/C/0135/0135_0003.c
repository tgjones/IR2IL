#include <stdlib.h>

#include <stdio.h>
#include <math.h>
int main()
{
  long int i;
  float b[100]={5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,
                5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,
                5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,
                5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,
	        5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5};
  float a[100]={10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,
                10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,
                10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,
                10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,
                10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10};

  for (i=1; i<100; i++)
  {
    b[i]=a[i]/i*50+b[i];
    if (b[i]>0.0)
      {
	b[i]=a[i]/i*50+b[i];
      }
    else if (b[i]==0.0)
      {
	b[i]=a[i]+b[i-1];
      }
    else
      {
        a[i]=50+b[i-1];
        b[i]=a[i]/i*50+b[i];
      }
    b[i]=a[i]/i*50+b[i];
  }

  for ( i=0; i<100; i++ ) printf("b[%ld] = %f\n",i,b[i]);
  exit(0);
}
